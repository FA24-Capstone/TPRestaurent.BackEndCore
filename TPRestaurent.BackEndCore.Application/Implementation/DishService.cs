using AutoMapper;
using TPRestaurent.BackEndCore.Application.Contract.IServices;
using TPRestaurent.BackEndCore.Application.IRepositories;
using TPRestaurent.BackEndCore.Common.DTO.Request;
using TPRestaurent.BackEndCore.Common.DTO.Response;
using TPRestaurent.BackEndCore.Common.DTO.Response.BaseDTO;
using TPRestaurent.BackEndCore.Common.Utils;
using TPRestaurent.BackEndCore.Domain.Enums;
using TPRestaurent.BackEndCore.Domain.Models;

namespace TPRestaurent.BackEndCore.Application.Implementation
{
    public class DishService : GenericBackendService, IDishService
    {
        private readonly IGenericRepository<Dish> _dishRepository;
        private IUnitOfWork _unitOfWork;
        private IMapper _mapper;

        public DishService
        (IServiceProvider serviceProvider,
            IGenericRepository<Dish> dishRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper
        ) : base(serviceProvider)
        {
            _dishRepository = dishRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<AppActionResult> CreateDish(DishDto dto)
        {
            var result = new AppActionResult();
            await _unitOfWork.ExecuteInTransaction(async () =>
            {
                try
                {
                    var firebaseService = Resolve<IFirebaseService>();
                    var dishSizeDetailRepository = Resolve<IGenericRepository<DishSizeDetail>>();
                    var staticFileRepository = Resolve<IGenericRepository<Image>>();
                    var dishTagRepository = Resolve<IGenericRepository<DishTag>>();
                    var tagRepository = Resolve<IGenericRepository<Tag>>();
                    var dishExsted = await _dishRepository.GetByExpression(p => p.Name == dto.Name);
                    if (dishExsted != null)
                    {
                        result = BuildAppActionResultError(result,
                            $"This dish with the name {dto.Name} is already exsited");
                    }

                    var dish = new Dish
                    {
                        DishId = Guid.NewGuid(),
                        Name = dto.Name,
                        Description = dto.Description,
                        IsMainItem = SD.EnumType.MainItemType.Contains(dto.DishItemType),
                        DishItemTypeId = dto.DishItemType,
                        isAvailable = true,
                        PreparationTime = dto.PreparationTime,
                    };

                    List<DishTag> dishTags = new List<DishTag>();
                    foreach (var tagId in dto.TagIds.Distinct())
                    {
                        var tagDb = await tagRepository.GetById(tagId);
                        if (tagDb != null)
                        {
                            dishTags.Add(new DishTag
                            {
                                DishTagId = Guid.NewGuid(),
                                DishId = dish.DishId,
                                TagId = tagId
                            });
                        }
                    }

                    List<DishSizeDetail> dishSizeDetails = new List<DishSizeDetail>();
                    if (dto.DishSizeDetailDtos.Count > 0)
                    {
                        if (dto.DishSizeDetailDtos.Count(d => d.DishSize == DishSize.SMALL) > 1
                            || dto.DishSizeDetailDtos.Count(d => d.DishSize == DishSize.MEDIUM) > 1
                            || dto.DishSizeDetailDtos.Count(d => d.DishSize == DishSize.LARGE) > 1)
                        {
                            result = BuildAppActionResultError(result,
                                $"Món ăn tồn tại kích thước trùng. Vui lòng kiểm tra lại");
                        }

                        //Check price by size
                        double checkPrice = 0;
                        foreach(var dishSizeDetail in dto.DishSizeDetailDtos.OrderBy(o => o.DishSize))
                        {
                            if(checkPrice == 0)
                            {
                                checkPrice = dishSizeDetail.Price;
                                continue;
                            }

                            if(checkPrice > dishSizeDetail.Price)
                            {
                                result = BuildAppActionResultError(result,
                                    $"Giá tiền của size {dishSizeDetail.DishSize} đang nhỏ hơn món có size nhỏ hơn");
                            }
                            checkPrice = dishSizeDetail.Price;
                        }

                        dto.DishSizeDetailDtos.ForEach(d =>
                            dishSizeDetails.Add(new DishSizeDetail
                            {
                                DishSizeDetailId = Guid.NewGuid(),
                                DishId = dish.DishId,
                                Discount = d.Discount,
                                DishSizeId = d.DishSize,
                                DailyCountdown = d.DailyCountdown,
                                QuantityLeft = d.QuantityLeft.HasValue ? d.QuantityLeft.Value : d.DailyCountdown,
                                IsAvailable = true,
                                Price = d.Price,
                                IsDeleted = false
                            }));
                    }

                    List<Image> staticList = new List<Image>();

                    var mainFile = dto.MainImageFile;
                    if (mainFile == null)
                    {
                        result = BuildAppActionResultError(result, $"The main picture of the dish is empty");
                    }

                    var mainPathName = SD.FirebasePathName.DISH_PREFIX + $"{dish.DishId}_main.jpg";
                    var uploadMainPicture = await firebaseService!.UploadFileToFirebase(mainFile, mainPathName);
                    var staticMainFile = new Image
                    {
                        StaticFileId = Guid.NewGuid(),
                        DishId = dish.DishId,
                        Path = uploadMainPicture!.Result!.ToString()!
                    };
                    staticList.Add(staticMainFile);
                    dish.Image = staticMainFile.Path;

                    foreach (var file in dto!.ImageFiles!)
                    {
                        var pathName = SD.FirebasePathName.DISH_PREFIX + $"{dish.DishId}{Guid.NewGuid()}.jpg";
                        var upload = await firebaseService!.UploadFileToFirebase(file, pathName);
                        var staticImg = new Image
                        {
                            StaticFileId = Guid.NewGuid(),
                            DishId = dish.DishId,
                            Path = upload!.Result!.ToString()!,
                        };
                        staticList.Add(staticImg);
                        if (!upload.IsSuccess)
                        {
                            result = BuildAppActionResultError(result, "Upload hình ảnh không thành công");
                        }
                    }

                    if (!BuildAppActionResultIsError(result))
                    {
                        await _dishRepository.Insert(dish);
                        await dishTagRepository.InsertRange(dishTags);
                        if (dto.DishSizeDetailDtos.Count > 0)
                        {
                            await dishSizeDetailRepository!.InsertRange(dishSizeDetails);
                        }

                        await staticFileRepository!.InsertRange(staticList);
                        await _unitOfWork.SaveChangesAsync();
                    }
                }
                catch (Exception ex)
                {
                    result = BuildAppActionResultError(result, ex.Message);
                }
            });
            return result;
        }

        public async Task<AppActionResult> DeleteDish(Guid dishId)
        {
            var result = new AppActionResult();
            try
            {
                var dishDb = await _dishRepository.GetById(dishId);
                if (dishDb == null)
                {
                    result = BuildAppActionResultError(result, $"This dish with id {dishId} doesn't existed");
                }

                dishDb!.IsDeleted = true;
                result.IsSuccess = true;
                result.Messages.Add("Đã xoá món thành công");
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }

            return result;
        }

        public async Task<AppActionResult> GetAllDish(string? keyword, DishItemType? type, int pageNumber, int pageSize,
            int? startPrice, int? endPrice)
        {
            var result = new AppActionResult();
            try
            {
                List<DishSizeResponse> dishSizeList = new List<DishSizeResponse>();
                var dishDetailsRepository = Resolve<IGenericRepository<DishSizeDetail>>();
                var dishList = await _dishRepository
                    .GetAllDataByExpression(p =>
                            (string.IsNullOrEmpty(keyword) || p.Name.ToLower().Contains(keyword.ToLower()) &&
                                !string.IsNullOrEmpty(keyword))
                            && (type > 0 && p.DishItemTypeId == type || type == 0 || type == null)
                            && p.IsMainItem && !p.IsDeleted && p.isAvailable, pageNumber, pageSize, null, false,
                        p => p.DishItemType!);
                foreach (var item in dishList.Items!)
                {
                    var dishDetailsListDb = await dishDetailsRepository!.GetAllDataByExpression(p =>
                            p.DishId == item.DishId
                            && !p.IsDeleted
                            && (!startPrice.HasValue || p.Price * (1 - p.Discount / 100) >= startPrice)
                            && (!endPrice.HasValue || p.Price * (1 - p.Discount / 100) <= endPrice),
                        0, 0, p => p.Price * (1 - p.Discount / 100), false, p => p.DishSize!);
                    var dishSizeResponse = new DishSizeResponse();
                    dishSizeResponse.Dish = _mapper.Map<DishReponse>(item);
                    dishSizeResponse.DishSizeDetails = dishDetailsListDb.Items!.OrderBy(d => d.DishSizeId).ToList();
                    dishSizeList.Add(dishSizeResponse);
                }

                result.Result = new PagedResult<DishSizeResponse>
                {
                    Items = await GetListDishRatingInformation(dishSizeList),
                    TotalPages = dishList.TotalPages,
                };
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }

            return result;
        }

        private async Task<List<DishSizeResponse>> GetListDishRatingInformation(
            List<DishSizeResponse> dishSizeResponses)
        {
            List<DishSizeResponse> responses = dishSizeResponses.ToList();
            try
            {
                var dishIds = dishSizeResponses.Select(d => d.Dish.DishId).ToList();
                var ratingRepository = Resolve<IGenericRepository<Rating>>();
                var ratingDb = await ratingRepository.GetAllDataByExpression(
                    o => o.OrderDetailId.HasValue && o.OrderDetail.DishSizeDetailId != null &&
                         dishIds.Contains(o.OrderDetail.DishSizeDetail.DishId.Value), 0, 0, null, false,
                    r => r.OrderDetail.DishSizeDetail);
                if (ratingDb.Items.Count > 0)
                {
                    var dishRating = ratingDb.Items.Where(r => r.OrderDetail.DishSizeDetailId.HasValue)
                        .GroupBy(r => r.OrderDetail.DishSizeDetail.DishId).ToDictionary(r => r.Key, r => r.ToList());
                    foreach (var response in responses)
                    {
                        response.Dish.NumberOfRating = dishRating[response.Dish.DishId].Count();
                        response.Dish.AverageRating = dishRating[response.Dish.DishId].Average(r =>
                        {
                            if (r.PointId == RatingPoint.One) return 1;
                            if (r.PointId == RatingPoint.Two) return 2;
                            if (r.PointId == RatingPoint.Three) return 3;
                            if (r.PointId == RatingPoint.Four) return 4;
                            return 5;
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                responses = dishSizeResponses.ToList();
            }

            return responses;
        }

        public async Task<AppActionResult> GetDishById(Guid dishId)
        {
            var result = new AppActionResult();
            var dishResponse = new DishDetailResponse();
            var staticFileRepository = Resolve<IGenericRepository<Image>>();
            var ratingRepository = Resolve<IGenericRepository<Rating>>();
            var dishSizeRepository = Resolve<IGenericRepository<DishSizeDetail>>();
            var orderDetailRepository = Resolve<IGenericRepository<OrderDetail>>();
            var dishTagRepository = Resolve<IGenericRepository<DishTag>>();
            var ratingListDb = new List<Rating>();
            try
            {
                var dishDb = await _dishRepository.GetByExpression(p => p.DishId == dishId, p => p.DishItemType);
                if (dishDb == null)
                {
                    result = BuildAppActionResultError(result, $"Món ăn với id {dishId} không tồn tại");
                }

                var dishTagDb =
                    await dishTagRepository!.GetAllDataByExpression(d => d.DishId == dishId, 0, 0, null, false,
                        d => d.Tag);
                dishResponse.DishTags = dishTagDb.Items.DistinctBy(t => t.TagId).ToList();
                var dishSizeDetailsDb =
                    await dishSizeRepository.GetAllDataByExpression(p => p.DishId == dishId && !p.IsDeleted, 0, 0, null,
                        false, p => p.DishSize!);
                if (dishSizeDetailsDb!.Items!.Count < 0 && dishSizeDetailsDb.Items == null)
                {
                    result = BuildAppActionResultError(result, $"size món ăn với id {dishId} không tồn tại");
                }

                dishResponse.Dish = new DishSizeResponse();
                dishResponse.Dish.Dish = _mapper.Map<DishReponse>(dishDb);
                dishResponse.Dish.DishSizeDetails = dishSizeDetailsDb!.Items!.OrderBy(d => d.DishSizeId).ToList();
                var staticFileDb =
                    await staticFileRepository!.GetAllDataByExpression(p => p.DishId == dishId, 0, 0, null, false,
                        null);

                var orderDetailDb = await orderDetailRepository.GetAllDataByExpression(
                    p => p.DishSizeDetail!.DishId == dishId && p.Order!.StatusId == OrderStatus.Completed, 0, 0, null,
                    false, null);
                if (orderDetailDb!.Items!.Count > 0 && orderDetailDb.Items != null)
                {
                    foreach (var orderDetail in orderDetailDb.Items)
                    {
                        var ratingDb = await ratingRepository!.GetAllDataByExpression(
                            p => p.OrderDetailId == orderDetail.OrderDetailId, 0, 0, null, false,
                            p => p.UpdateByAccount, p => p.CreateByAccount);
                        if (ratingDb != null)
                        {
                            ratingListDb.AddRange(ratingDb.Items);
                        }
                    }
                }

                if (ratingListDb.Count > 0)
                {
                    foreach (var rating in ratingListDb)
                    {
                        var ratingStaticFileDb = await staticFileRepository.GetAllDataByExpression(
                            p => p.RatingId == rating.RatingId, 0, 0, null, false, r => r.Rating.UpdateByAccount,
                            r => r.Rating.CreateByAccount);
                        var ratingDishResponse = new RatingResponse
                        {
                            Rating = rating,
                            RatingImgs = ratingStaticFileDb.Items!
                        };
                        dishResponse.RatingDish.Add(ratingDishResponse);
                    }

                    dishResponse.NumberOfRating = ratingListDb.Count();
                    dishResponse.AverageRating = ratingListDb.Average(r =>
                    {
                        if (r.PointId == RatingPoint.One) return 1;
                        if (r.PointId == RatingPoint.Two) return 2;
                        if (r.PointId == RatingPoint.Three) return 3;
                        if (r.PointId == RatingPoint.Four) return 4;
                        return 5;
                    });
                }

                dishResponse.DishImgs = staticFileDb.Items!;

                result.Result = dishResponse;
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }

            return result;
        }

        public async Task<AppActionResult> GetAllDishType(int pageNumber, int pagesize)
        {
            var result = new AppActionResult();
            var dishTypeRepository =
                Resolve<IGenericRepository<TPRestaurent.BackEndCore.Domain.Models.EnumModels.DishItemType>>();
            try
            {
                result.Result = await dishTypeRepository!.GetAllDataByExpression(null, 0, 0, null, false, null);
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }

            return result;
        }

        public async Task<AppActionResult> UpdateDish(UpdateDishRequestDto dto)
        {
            var result = new AppActionResult();
            await _unitOfWork.ExecuteInTransaction(async () =>
            {
                try
                {
                    var firebaseService = Resolve<IFirebaseService>();
                    var staticFileRepository = Resolve<IGenericRepository<Image>>();
                    var dishSizeDetailRepository = Resolve<IGenericRepository<DishSizeDetail>>();
                    var dishDb = await _dishRepository.GetById(dto.DishId);
                    if (dishDb == null)
                    {
                        result = BuildAppActionResultError(result, $"Món ăn với id {dto.DishId} không tồn tại");
                    }

                    dishDb.Name = dto.Name;
                    dishDb.Description = dto.Description;
                    dishDb.DishItemTypeId = dto.DishItemType;
                    dishDb.PreparationTime = dto.PreparationTime;
                    if (SD.EnumType.MainItemType.Contains(dto.DishItemType) != dishDb.IsMainItem)
                    {
                        dishDb.IsMainItem = !dishDb.IsMainItem;
                    }

                    dishDb.isAvailable = dto.IsAvailable;

                    List<DishSizeDetail> updateDishSizeDetails = new List<DishSizeDetail>();
                    List<DishSizeDetail> addDishSizeDetails = new List<DishSizeDetail>();
                    PagedResult<DishSizeDetail> toDeleteDishSize = new PagedResult<DishSizeDetail>();
                    if (dto.UpdateDishSizeDetailDtos.Count > 0)
                    {
                        foreach (var dishSizeDetail in dto.UpdateDishSizeDetailDtos)
                        {
                            if (dto.UpdateDishSizeDetailDtos.Count(d => d.DishSize == DishSize.SMALL) > 1
                                || dto.UpdateDishSizeDetailDtos.Count(d => d.DishSize == DishSize.MEDIUM) > 1
                                || dto.UpdateDishSizeDetailDtos.Count(d => d.DishSize == DishSize.LARGE) > 1)
                            {
                                result = BuildAppActionResultError(result,
                                    $"Món ăn tồn tại kích thước trùng. Vui lòng kiểm tra lại");
                            }

                            if (dishSizeDetail.DishSizeDetailId.HasValue)
                            {
                                var dishSizeDetailDb =
                                    await dishSizeDetailRepository.GetById(dishSizeDetail.DishSizeDetailId.Value);
                                if (dishSizeDetailDb != null)
                                {
                                    dishSizeDetailDb.DishSizeId = dishSizeDetail.DishSize;
                                    dishSizeDetailDb.Discount = dishSizeDetail.Discount;
                                    dishSizeDetailDb.IsAvailable = dishSizeDetail.IsAvailable;
                                    dishSizeDetailDb.DailyCountdown = (int)(dishSizeDetail.DailyCountdown.HasValue
                                        ? dishSizeDetail.DailyCountdown
                                        : dishSizeDetailDb.DailyCountdown);
                                    dishSizeDetailDb.QuantityLeft = !dishSizeDetail.QuantityLeft.HasValue
                                        ? dishSizeDetailDb.QuantityLeft.HasValue
                                            ? dishSizeDetailDb.QuantityLeft.Value
                                            : dishSizeDetail.DailyCountdown
                                        : dishSizeDetail.DailyCountdown;
                                    dishSizeDetailDb.Price = dishSizeDetail.Price;

                                    updateDishSizeDetails.Add(dishSizeDetailDb);
                                }
                                else
                                {
                                    result = BuildAppActionResultError(result,
                                        $"Không tìm thấy size món ăn chi tiết với id {dishSizeDetail.DishSizeDetailId.Value}");
                                }
                            }
                            else
                            {
                                addDishSizeDetails.Add(new DishSizeDetail
                                {
                                    DishSizeDetailId = Guid.NewGuid(),
                                    DishId = dishDb.DishId,
                                    DishSizeId = dishSizeDetail.DishSize,
                                    Discount = dishSizeDetail.Discount,
                                    IsAvailable = true,
                                    Price = dishSizeDetail.Price
                                });
                            }
                        }

                        var updateDishSizeDetailIds = updateDishSizeDetails.Select(u => u.DishSizeDetailId).ToList();
                        toDeleteDishSize = await dishSizeDetailRepository.GetAllDataByExpression(
                            d => !updateDishSizeDetailIds.Contains(d.DishSizeDetailId) && d.DishId == dto.DishId, 0, 0,
                            null, false, null);
                        toDeleteDishSize.Items.ForEach(d => d.IsDeleted = true);
                    }

                    if (!BuildAppActionResultIsError(result))
                    {
                        await _dishRepository.Update(dishDb!);
                        if (dto.UpdateDishSizeDetailDtos.Count > 0)
                        {
                            await dishSizeDetailRepository!.UpdateRange(updateDishSizeDetails);
                        }

                        if (addDishSizeDetails.Count > 0)
                        {
                            await dishSizeDetailRepository!.InsertRange(addDishSizeDetails);
                        }

                        if (toDeleteDishSize.Items.Count > 0)
                        {
                            await dishSizeDetailRepository!.UpdateRange(toDeleteDishSize.Items);
                        }

                        await _unitOfWork.SaveChangesAsync();
                        result.Messages.Add("Update dish successfully");
                    }
                }
                catch (Exception ex)
                {
                    result = BuildAppActionResultError(result, ex.Message);
                }
            });
            return result;
        }

        public async Task<AppActionResult> UpdateInactiveDish(Guid dishId)
        {
            var result = new AppActionResult();
            try
            {
                var dishDb = await _dishRepository.GetById(dishId);
                if (dishDb == null)
                {
                    result = BuildAppActionResultError(result, $"Món ăn với id {dishId} không tồn tại");
                }

                dishDb!.IsDeleted = false;
                await _dishRepository.Update(dishDb);
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }

            return result;
        }

        public async Task<AppActionResult> GetAllDishTag(int pageNumber, int pagesize)
        {
            var result = new AppActionResult();
            var tagRepository = Resolve<IGenericRepository<TPRestaurent.BackEndCore.Domain.Models.Tag>>();
            try
            {
                result.Result = await tagRepository!.GetAllDataByExpression(null, 0, 0, null, false, null);
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }

            return result;
        }

        public async Task<AppActionResult> GetAllDishSize(int pageNumber, int pagesize)
        {
            var result = new AppActionResult();
            var dishSizeRepository =
                Resolve<IGenericRepository<TPRestaurent.BackEndCore.Domain.Models.EnumModels.DishSize>>();
            try
            {
                result.Result = await dishSizeRepository!.GetAllDataByExpression(null, 0, 0, null, false, null);
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }

            return result;
        }

        public async Task<AppActionResult> InsertDishTag()
        {
            AppActionResult result = new AppActionResult();
            try
            {
                var tagRepository = Resolve<IGenericRepository<Tag>>();
                var comboRepository = Resolve<IGenericRepository<Combo>>();
                var dishTagRepository = Resolve<IGenericRepository<DishTag>>();
                var tagDb = await tagRepository.GetAllDataByExpression(null, 0, 0, null, false, null);
                var dishDb = await _dishRepository.GetAllDataByExpression(null, 0, 0, null, false, null);
                var comboDb = await comboRepository.GetAllDataByExpression(null, 0, 0, null, false, null);
                List<DishTag> dishTags = new List<DishTag>();
                Random random = new Random();
                int tagCount = tagDb.Items.Count();
                foreach (var dish in dishDb.Items)
                {
                    dishTags.Add(new DishTag
                    {
                        DishTagId = Guid.NewGuid(),
                        DishId = dish.DishId,
                        TagId = tagDb.Items[random.Next(tagCount)].TagId
                    });

                    dishTags.Add(new DishTag
                    {
                        DishTagId = Guid.NewGuid(),
                        DishId = dish.DishId,
                        TagId = tagDb.Items[random.Next(tagCount)].TagId
                    });

                    dishTags.Add(new DishTag
                    {
                        DishTagId = Guid.NewGuid(),
                        DishId = dish.DishId,
                        TagId = tagDb.Items[random.Next(tagCount)].TagId
                    });

                    dishTags.Add(new DishTag
                    {
                        DishTagId = Guid.NewGuid(),
                        DishId = dish.DishId,
                        TagId = tagDb.Items[random.Next(tagCount)].TagId
                    });

                    dishTags.Add(new DishTag
                    {
                        DishTagId = Guid.NewGuid(),
                        DishId = dish.DishId,
                        TagId = tagDb.Items[random.Next(tagCount)].TagId
                    });

                    dishTags.Add(new DishTag
                    {
                        DishTagId = Guid.NewGuid(),
                        DishId = dish.DishId,
                        TagId = tagDb.Items[random.Next(tagCount)].TagId
                    });

                    dishTags.Add(new DishTag
                    {
                        DishTagId = Guid.NewGuid(),
                        DishId = dish.DishId,
                        TagId = tagDb.Items[random.Next(tagCount)].TagId
                    });

                    dishTags.Add(new DishTag
                    {
                        DishTagId = Guid.NewGuid(),
                        DishId = dish.DishId,
                        TagId = tagDb.Items[random.Next(tagCount)].TagId
                    });
                }

                foreach (var combo in comboDb.Items)
                {
                    dishTags.Add(new DishTag
                    {
                        DishTagId = Guid.NewGuid(),
                        ComboId = combo.ComboId,
                        TagId = tagDb.Items[random.Next(tagCount)].TagId
                    });

                    dishTags.Add(new DishTag
                    {
                        DishTagId = Guid.NewGuid(),
                        ComboId = combo.ComboId,
                        TagId = tagDb.Items[random.Next(tagCount)].TagId
                    });

                    dishTags.Add(new DishTag
                    {
                        DishTagId = Guid.NewGuid(),
                        ComboId = combo.ComboId,
                        TagId = tagDb.Items[random.Next(tagCount)].TagId
                    });

                    dishTags.Add(new DishTag
                    {
                        DishTagId = Guid.NewGuid(),
                        ComboId = combo.ComboId,
                        TagId = tagDb.Items[random.Next(tagCount)].TagId
                    });
                }

                await dishTagRepository.InsertRange(dishTags);
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
            }

            return result;
        }

        public async Task<AppActionResult> UpdateDishImage(UpdateDishImageRequest dto)
        {
            var result = new AppActionResult();
            await _unitOfWork.ExecuteInTransaction(async () =>
            {
                try
                {
                    var firebaseService = Resolve<IFirebaseService>();
                    var staticFileRepository = Resolve<IGenericRepository<Image>>();

                    var staticFileDb =
                        await staticFileRepository!.GetByExpression(p =>
                            p.DishId == dto.DishId && p.Path == dto.OldImageLink);
                    if (staticFileDb == null)
                    {
                        throw new Exception(
                            $"Các file hình ảnh của món ăn với id {dto.DishId} không tồn tại");
                    }

                    var resultOfDeleteImage = await firebaseService!.DeleteFileFromFirebase(dto.OldImageLink);

                    var pathName = SD.FirebasePathName.DISH_PREFIX + $"{dto.DishId}{Guid.NewGuid()}.jpg";
                    var upload = await firebaseService!.UploadFileToFirebase(dto.Image!, pathName);

                    if (!upload.IsSuccess)
                    {
                        throw new Exception(
                            "Upload hình ảnh không thành công");
                    }

                    staticFileDb.Path = upload.Result!.ToString()!;

                    if (dto.OldImageLink.Contains("_main"))
                    {
                        var dishDb = await _dishRepository.GetByExpression(p => p.DishId == dto.DishId);
                        if (dishDb == null)
                        {
                            throw new Exception(
                                $"Không tìm thấy món ăn với id {dto.DishId}");
                        }

                        dishDb.Image = upload.Result!.ToString()!;
                        await _dishRepository.Update(dishDb);
                    }

                    if (!BuildAppActionResultIsError(result))
                    {
                        await staticFileRepository.Update(staticFileDb);
                        await _unitOfWork.SaveChangesAsync();

                        result.Messages.Add("Cập nhập hình ảnh thành công");
                    }
                }
                catch (Exception ex)
                {
                    result = BuildAppActionResultError(result, ex.Message);
                }
            });
            return result;
        }

        [Hangfire.Queue("auto-refill-dish")]
        public async Task AutoRefillDish()
        {
            var dishSizeDetailRepository = Resolve<IGenericRepository<DishSizeDetail>>();
            try
            {
                var dishSizeDetailsDb =
                    await dishSizeDetailRepository!.GetAllDataByExpression(
                        p => p.DailyCountdown != 0 && !p.Dish.IsDeleted, 0, 0, null, false, null);
                var dishSizeDetailsList = dishSizeDetailsDb.Items;
                if (dishSizeDetailsList!.Count > 0 && dishSizeDetailsList != null)
                {
                    foreach (var dishSizeDetail in dishSizeDetailsDb!.Items!)
                    {
                        dishSizeDetail.QuantityLeft = dishSizeDetail.DailyCountdown;
                        if (dishSizeDetail.DailyCountdown == 0)
                        {
                            dishSizeDetail.IsAvailable = false;
                        }
                        else
                        {
                            dishSizeDetail.IsAvailable = true;
                        }
                    }

                    var listDish = new List<Dish>();
                    var dishGroup = dishSizeDetailsDb.Items.GroupBy(d => d.DishId)
                        .ToDictionary(d => d.Key, d => d.ToList());
                    foreach (var dish in dishGroup)
                    {
                        var dishDb = await _dishRepository.GetById(dish.Key);
                        dishDb.isAvailable = !dish.Value.All(d => !d.IsAvailable);
                        listDish.Add(dishDb);
                    }

                    await _dishRepository.UpdateRange(listDish);
                    await dishSizeDetailRepository.UpdateRange(dishSizeDetailsList);
                }

                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
            }

            Task.CompletedTask.Wait();
        }
    }
}