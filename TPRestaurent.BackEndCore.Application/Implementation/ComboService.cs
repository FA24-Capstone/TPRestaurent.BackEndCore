﻿using AutoMapper;
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
    public class ComboService : GenericBackendService, IComboService
    {
        private IGenericRepository<Combo> _comboRepository;
        private IMapper _mapper;
        private IUnitOfWork _unitOfWork;

        public ComboService(IServiceProvider serviceProvider, IGenericRepository<Combo> comboRepository, IMapper mapper, IUnitOfWork unitOfWork) : base(serviceProvider)
        {
            _comboRepository = comboRepository;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }

        public async Task<AppActionResult> CreateCombo(ComboDto comboDto)
        {
            var result = new AppActionResult();
            await _unitOfWork.ExecuteInTransaction(async () =>
            {
                try
                {
                    var dishComboRepository = Resolve<IGenericRepository<DishCombo>>();
                    var comboOptionSetRepository = Resolve<IGenericRepository<ComboOptionSet>>();
                    var dishSizeDetailRepository = Resolve<IGenericRepository<DishSizeDetail>>();
                    var staticFileRepository = Resolve<IGenericRepository<Image>>();
                    var tagRepository = Resolve<IGenericRepository<Tag>>();
                    var dishTagRepository = Resolve<IGenericRepository<DishTag>>();
                    var firebaseService = Resolve<IFirebaseService>();

                    var comboIsExisted = await _comboRepository.GetByExpression(p => p.Name == comboDto.Name);
                    if (comboIsExisted != null)
                    {
                        throw new Exception($"Combo này với {comboDto.Name} đã tồn tại");
                    }
                    if(comboDto.StartDate >= comboDto.EndDate)
                    {
                        throw new Exception($"Thời gian diễn ra combo không phù hợp");
                    }



                    var comboDb = new Combo
                    {
                        ComboId = Guid.NewGuid(),
                        Description = comboDto.Description,
                        Name = comboDto.Name,
                        EndDate = comboDto.EndDate,
                        Price = comboDto.Price,
                        StartDate = comboDto.StartDate,
                        IsAvailable = true,
                        IsDeleted = false
                    };

                    List<DishTag> dishTags = new List<DishTag>();
                    foreach (var tagId in comboDto.TagIds.Distinct())
                    {
                        var tagDb = await tagRepository.GetById(tagId);
                        if (tagDb != null)
                        {
                            dishTags.Add(new DishTag
                            {
                                DishTagId = Guid.NewGuid(),
                                ComboId = comboDb.ComboId,
                                TagId = tagId
                            });
                        }
                    }

                    List<ComboOptionSet> comboOptionSetList = new List<ComboOptionSet>();
                    List<DishCombo> dishComboList = new List<DishCombo>();
                    foreach (var dishComboDto in comboDto.DishComboDtos)
                    {
                        if(dishComboDto.NumOfChoice >= dishComboDto.ListDishId.Count)
                        {
                            throw new Exception($"Số lượng lựa chọn của set {dishComboDto.OptionSetNumber} đang nhiều hơn số món.");
                        }
                        var comboOptionSet = new ComboOptionSet
                        {
                            ComboOptionSetId = Guid.NewGuid(),
                            DishItemTypeId = dishComboDto.DishItemType,
                            NumOfChoice = dishComboDto.NumOfChoice,
                            OptionSetNumber = dishComboDto.OptionSetNumber,
                            ComboId = comboDb.ComboId
                        };
                        comboOptionSetList.Add(comboOptionSet);

                        foreach (var dishId in dishComboDto.ListDishId)
                        {
                            var dishExisted = await dishSizeDetailRepository!.GetByExpression(d => d.DishSizeDetailId == dishId.DishSizeDetailId && !d.IsDeleted && !d.Dish.IsDeleted);
                            if (dishExisted == null)
                            {
                                throw new Exception($"size món ăn với id {dishId.DishSizeDetailId} không tồn tại");
                            }

                            var dishCombo = new DishCombo
                            {
                                DishComboId = Guid.NewGuid(),
                                ComboOptionSetId = comboOptionSet.ComboOptionSetId,
                                DishSizeDetailId = dishId.DishSizeDetailId,
                                Quantity = dishId.Quantity,
                                QuantityLeft = dishExisted.QuantityLeft / dishId.Quantity,
                                IsAvailable = dishExisted.QuantityLeft / dishId.Quantity > 0,
                                IsDeleted = false
                            };
                            dishComboList.Add(dishCombo);
                        }
                    }

                   

                    List<Image> staticList = new List<Image>();

                    foreach (var file in comboDto!.Imgs!)
                    {
                        var pathName = SD.FirebasePathName.COMBO_PREFIX + $"{comboDb.ComboId}{Guid.NewGuid()}.jpg";
                        var upload = await firebaseService!.UploadFileToFirebase(file, pathName);
                        var staticImg = new Image
                        {
                            StaticFileId = Guid.NewGuid(),
                            ComboId = comboDb.ComboId,
                            Path = upload!.Result!.ToString()!
                        };
                        staticList.Add(staticImg);
                        if (!upload.IsSuccess)
                        {
                            throw new Exception("Upload ảnh không thành công");
                        }
                    }

                    comboDb.Image = staticList.FirstOrDefault().Path;

                    if (!BuildAppActionResultIsError(result))
                    {
                        await _comboRepository.Insert(comboDb);
                        await dishTagRepository.InsertRange(dishTags);
                        await staticFileRepository.InsertRange(staticList);
                        await comboOptionSetRepository!.InsertRange(comboOptionSetList);
                        await dishComboRepository!.InsertRange(dishComboList);
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

        public async Task<AppActionResult> DeleteComboById(Guid comboId)
        {
            var result = new AppActionResult();
            try
            {
                var comboDb = await _comboRepository.GetById(comboId);
                if (comboDb == null)
                {
                    return BuildAppActionResultError(result, $"Combo với id {comboId} không tồn tại");
                }
                comboDb.IsDeleted = true;
                await _comboRepository.Update(comboDb);
                await _unitOfWork.SaveChangesAsync();
                result.Messages.Add("Xóa combo thành công");
                result.IsSuccess = true;
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }

        public async Task<AppActionResult> GetAllCombo(string? keyword, ComboCategory? category, int? startPrice, int? endPrice, int pageNumber, int pageSize)
        {
            var result = new AppActionResult();
            try
            {
                var currentDateTime = Resolve<Utility>().GetCurrentDateTimeInTimeZone();
                var comboDb = await _comboRepository.GetAllDataByExpression(
                    p => (string.IsNullOrEmpty(keyword) || p.Name.Contains(keyword))
                    && (!category.HasValue || category.HasValue && p.CategoryId == category.Value)
                    && (!startPrice.HasValue || p.Price * (1 - p.Discount / 100) >= startPrice.Value)
                    && (!endPrice.HasValue || p.Price * (1 - p.Discount / 100) <= endPrice.Value)
                    && p.EndDate > currentDateTime && !p.IsDeleted,
                    pageNumber, pageSize, p => p.Price * (1 - p.Discount / 100), false, c => c.Category
                );

                if (comboDb.Items.Count > 0)
                {
                    var comboResponses = comboDb.Items
                        .Select(c => _mapper.Map<ComboResponseDto>(c))
                        .ToList();

                    result.Result = await GetListDishRatingInformation(comboResponses);
                }
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }

            return result;
        }

        private async Task<List<ComboResponseDto>> GetListDishRatingInformation(List<ComboResponseDto> comboResponses)
        {
            var responses = comboResponses.ToList();
            try
            {
                var comboIds = comboResponses.Select(c => c.ComboId).ToList();

                var ratingRepository = Resolve<IGenericRepository<Rating>>();
                var ratingDb = await ratingRepository.GetAllDataByExpression(
                    o => o.OrderDetailId.HasValue && o.OrderDetail.ComboId.HasValue && comboIds.Contains(o.OrderDetail.ComboId.Value),
                    0, 0, null, false, r => r.OrderDetail
                );

                if (ratingDb.Items.Count > 0)
                {
                    var comboRatings = ratingDb.Items
                        .GroupBy(r => r.OrderDetail.ComboId)
                        .ToDictionary(g => g.Key, g => g.ToList());

                    foreach (var response in responses)
                    {
                        if (comboRatings.TryGetValue(response.ComboId, out var ratings))
                        {
                            response.NumberOfRating = ratings.Count;
                            response.AverageRating = ratings.Average(r => (int)r.PointId); // Assuming PointId is an enum
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the exception or handle it accordingly
                // Keeping the original list intact if the rating fetch fails
            }

            return responses;
        }

        public async Task<AppActionResult> GetComboById(Guid comboId)
        {
            var result = new AppActionResult();
            try
            {
                var dishComboRepository = Resolve<IGenericRepository<DishCombo>>();
                var staticFileRepository = Resolve<IGenericRepository<Image>>();
                var orderDetailRepository = Resolve<IGenericRepository<OrderDetail>>();
                var ratingRepository = Resolve<IGenericRepository<Rating>>();
                var dishTagRepository = Resolve<IGenericRepository<DishTag>>();
                var comboResponse = new ComboDetailResponseDto();
                var ratingListDb = new List<Rating>();
                var comboDb = await _comboRepository.GetByExpression(p => p!.ComboId == comboId, p => p.Category);
                if (comboDb == null)
                {
                    return BuildAppActionResultError(result, $"Combo với id {comboId} không tồn tại");
                }
                var dishComboDb = await dishComboRepository!.GetAllDataByExpression(p => p.ComboOptionSet.ComboId == comboId, 0, 0, null, false, p => p.DishSizeDetail.Dish!, p => p.DishSizeDetail.DishSize, p => p.ComboOptionSet.DishItemType);
                var staticFileDb = await staticFileRepository!.GetAllDataByExpression(p => p.ComboId == comboId, 0, 0, null, false, null);

                var optionSetDictionary = dishComboDb.Items.GroupBy(d => d.ComboOptionSet).ToDictionary(g => g.Key, g => g.ToList());
                foreach (var item in optionSetDictionary)
                {
                    comboResponse.DishCombo.Add(new Common.DTO.Response.DishComboDto
                    {
                        ComboOptionSetId = item.Key.ComboOptionSetId,
                        OptionSetNumber = item.Key.OptionSetNumber,
                        NumOfChoice = item.Key.NumOfChoice,
                        DishItemTypeId = item.Key.DishItemTypeId,
                        DishItemType = item.Key.DishItemType,
                        DishCombo = item.Value
                    });
                }
                comboResponse.DishCombo = comboResponse.DishCombo.OrderBy(c => c.OptionSetNumber).ToList();
                comboResponse.DishCombo.ForEach(d => d.DishCombo.ForEach(dc => dc.ComboOptionSet = null));
                comboResponse.Imgs = staticFileDb.Items!.ToList();
                comboResponse.Combo = _mapper.Map<ComboResponseDto>(comboDb);

                var dishTagDb = await dishTagRepository!.GetAllDataByExpression(d => d.ComboId == comboId, 0, 0, null, false, d => d.Tag);
                comboResponse.DishTags = dishTagDb.Items;

                var orderDetailDb = await orderDetailRepository.GetAllDataByExpression(p => p.Combo!.ComboId == comboId && p.Order!.StatusId == OrderStatus.Completed, 0, 0, null, false, null);
                if (orderDetailDb!.Items!.Count > 0 && orderDetailDb.Items != null)
                {
                    foreach (var orderDetail in orderDetailDb.Items)
                    {
                        var ratingDb = await ratingRepository!.GetByExpression(p => p.OrderDetailId == orderDetail.OrderDetailId);
                        if (ratingDb != null)
                        {
                            ratingListDb.Add(ratingDb);
                        }
                        ratingListDb.Add(ratingDb);
                    }
                }

                if (ratingListDb.Count > 0)
                {
                    foreach (var rating in ratingListDb)
                    {
                        var ratingStaticFileDb = await staticFileRepository.GetAllDataByExpression(p => p.RatingId == rating.RatingId, 0, 0, null, false, null);
                        var ratingDishResponse = new RatingResponse
                        {
                            Rating = rating,
                            RatingImgs = ratingStaticFileDb.Items!
                        };
                        comboResponse.ComboRatings.Add(ratingDishResponse);
                    }
                    comboResponse.Combo.NumberOfRating = ratingListDb.Count();
                    comboResponse.Combo.AverageRating = ratingListDb.Average(r =>
                    {
                        if (r.PointId == RatingPoint.One) return 1;
                        if (r.PointId == RatingPoint.Two) return 2;
                        if (r.PointId == RatingPoint.Three) return 3;
                        if (r.PointId == RatingPoint.Four) return 4;
                        return 5;
                    });
                }
                comboResponse.Imgs = staticFileDb.Items!;
                result.Result = comboDb;

                result.Result = comboResponse;
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }

        public async Task<AppActionResult> GetComboById2(Guid comboId)
        {
            var result = new AppActionResult();
            try
            {
                var dishComboRepository = Resolve<IGenericRepository<DishCombo>>();
                var staticFileRepository = Resolve<IGenericRepository<Image>>();
                var orderDetailRepository = Resolve<IGenericRepository<OrderDetail>>();
                var ratingRepository = Resolve<IGenericRepository<Rating>>();
                var dishTagRepository = Resolve<IGenericRepository<DishTag>>();
                var comboResponse = new ComboDetailResponseDto();
                var ratingListDb = new List<Rating>();
                var comboDb = await _comboRepository.GetByExpression(p => p!.ComboId == comboId, p => p.Category);
                if (comboDb == null)
                {
                    return BuildAppActionResultError(result, $"Combo với id {comboId} không tồn tại");
                }

                var dishTagDb = await dishTagRepository!.GetAllDataByExpression(d => d.ComboId == comboId, 0, 0, null, false, d => d.Tag);
                comboResponse.DishTags = dishTagDb.Items.DistinctBy(t => t.TagId).ToList();

                var dishComboDb = await dishComboRepository!.GetAllDataByExpression(p => !p.DishSizeDetail.Dish.IsDeleted && p.ComboOptionSet.ComboId == comboId && !p.IsDeleted && !p.ComboOptionSet.IsDeleted, 0, 0, null, false, p => p.DishSizeDetail.Dish!, p => p.DishSizeDetail.DishSize, p => p.ComboOptionSet.DishItemType);
                var staticFileDb = await staticFileRepository!.GetAllDataByExpression(p => p.ComboId == comboId, 0, 0, null, false, null);

                var optionSetDictionary = dishComboDb.Items.GroupBy(d => d.ComboOptionSetId).ToDictionary(g => g.Key, g => g.ToList());
                var optionSetList = dishComboDb.Items.Where(d => d.ComboOptionSetId.HasValue).Select(d => d.ComboOptionSet!).DistinctBy(d => d.ComboOptionSetId);
                foreach (var item in optionSetDictionary)
                {
                    comboResponse.DishCombo.Add(new Common.DTO.Response.DishComboDto
                    {
                        ComboOptionSetId = optionSetList.FirstOrDefault(o => o.ComboOptionSetId == item.Key).ComboOptionSetId,
                        OptionSetNumber = optionSetList.FirstOrDefault(o => o.ComboOptionSetId == item.Key).OptionSetNumber,
                        NumOfChoice = optionSetList.FirstOrDefault(o => o.ComboOptionSetId == item.Key).NumOfChoice,
                        DishItemTypeId = optionSetList.FirstOrDefault(o => o.ComboOptionSetId == item.Key).DishItemTypeId,
                        DishItemType = optionSetList.FirstOrDefault(o => o.ComboOptionSetId == item.Key).DishItemType,
                        DishCombo = item.Value
                    });
                }
                comboResponse.DishCombo = comboResponse.DishCombo.OrderBy(c => c.OptionSetNumber).ToList();
                comboResponse.DishCombo.ForEach(d => d.DishCombo.ForEach(dc => dc.ComboOptionSet = null));
                comboResponse.Imgs = staticFileDb.Items!.ToList();
                comboResponse.Combo = _mapper.Map<ComboResponseDto>(comboDb)!;

                var orderDetailDb = await orderDetailRepository.GetAllDataByExpression(p => p.Combo!.ComboId == comboId && p.Order!.StatusId == OrderStatus.Completed, 0, 0, null, false, null);
                if (orderDetailDb!.Items!.Count > 0 && orderDetailDb.Items != null)
                {
                    foreach (var orderDetail in orderDetailDb.Items)
                    {
                        var ratingDb = await ratingRepository!.GetAllDataByExpression(p => p.OrderDetailId == orderDetail.OrderDetailId, 0, 0, null, false, p => p.CreateByAccount);
                        if (ratingDb.Items != null)
                        {
                            ratingListDb.AddRange(ratingDb.Items);
                        }
                    }
                }

                if (ratingListDb.Count > 0)
                {
                    foreach (var rating in ratingListDb)
                    {
                        var ratingStaticFileDb = await staticFileRepository.GetAllDataByExpression(p => p.RatingId == rating.RatingId, 0, 0, null, false, null);
                        var ratingDishResponse = new RatingResponse
                        {
                            Rating = rating,
                            RatingImgs = ratingStaticFileDb.Items!
                        };
                        comboResponse.ComboRatings.Add(ratingDishResponse);
                    }
                    comboResponse.Combo.NumberOfRating = ratingListDb.Count();
                    comboResponse.Combo.AverageRating = ratingListDb.Average(r =>
                    {
                        if (r.PointId == RatingPoint.One) return 1;
                        if (r.PointId == RatingPoint.Two) return 2;
                        if (r.PointId == RatingPoint.Three) return 3;
                        if (r.PointId == RatingPoint.Four) return 4;
                        return 5;
                    });
                }

                result.Result = comboResponse;
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }

        public async Task<AppActionResult> UploadComboImages(ComboImageDto comboDto)
        {
            AppActionResult result = new AppActionResult();
            await _unitOfWork.ExecuteInTransaction(async () =>
            {
                try
                {
                    var firebaseService = Resolve<IFirebaseService>();
                    var staticFileRepository = Resolve<IGenericRepository<Image>>();

                    var staticFileDb = await staticFileRepository!.GetByExpression(p =>
                        p.ComboId == comboDto.ComboId && p.Path == comboDto.OldImageLink);
                    if (staticFileDb == null)
                    {
                        throw new Exception($"Không có file hình ảnh của combo với id {comboDto.ComboId}");
                    }

                    var resultOfDeleteImage = await firebaseService!.DeleteFileFromFirebase(comboDto.OldImageLink);

                    var pathName = SD.FirebasePathName.COMBO_PREFIX + $"{comboDto.ComboId}{Guid.NewGuid()}.jpg";
                    var upload = await firebaseService!.UploadFileToFirebase(comboDto.Img!, pathName);

                    if (!upload.IsSuccess)
                    {
                        throw new Exception("Upload hình ảnh không thành công");
                    }

                    staticFileDb.Path = upload.Result!.ToString()!;

                    if (comboDto.OldImageLink.Contains("_main"))
                    {
                        var comboDb = await _comboRepository.GetById(comboDto.ComboId);
                        if (comboDb == null)
                        {
                            throw new Exception($"Không tìm thấy combo với id {comboDto.ComboId}");
                        }

                        comboDb.Image = upload.Result!.ToString()!;
                        await _comboRepository.Update(comboDb);
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

        public async Task<AppActionResult> UpdateCombo(UpdateComboDto comboDto)
        {
            var result = new AppActionResult();
            await _unitOfWork.ExecuteInTransaction(async () =>
            {
                try
                {
                    var comboResponse = new ComboResponseDto();
                    var dishComboRepository = Resolve<IGenericRepository<DishCombo>>();
                    var dishTagRepository = Resolve<IGenericRepository<DishTag>>();
                    var tagRepository = Resolve<IGenericRepository<Tag>>();
                    var comboOptionSetRepository = Resolve<IGenericRepository<ComboOptionSet>>();
                    var dishSizeDetailRepository = Resolve<IGenericRepository<DishSizeDetail>>();
                    var comboOrderDetailRepository = Resolve<IGenericRepository<ComboOrderDetail>>();
                    var orderDetailRepository = Resolve<IGenericRepository<OrderDetail>>();

                    var orderDetailDb = await orderDetailRepository.GetAllDataByExpression(
                        o => o.ComboId != null && o.ComboId == comboDto.ComboId, 0, 0, null, false, null);

                    var comboDb = await _comboRepository.GetByExpression(p => p.ComboId == comboDto.ComboId);
                    if (comboDb == null)
                    {
                        throw new Exception($"Combo với id {comboDto.ComboId} không tồn tại");
                    }

                    comboDb.Name = comboDto.Name;
                    comboDb.Description = comboDto.Description;
                    comboDb.StartDate = comboDto.StartDate;
                    comboDb.EndDate = comboDto.EndDate;
                    comboDb.Price = comboDto.Price;
                    comboDb.PreparationTime = comboDto.PreparationTime;

                    List<DishTag> dishTags = new List<DishTag>();

                    var existedTagDb =
                        await dishTagRepository.GetAllDataByExpression(d => d.ComboId == comboDto.ComboId, 0, 0, null,
                            false, null);
                    if (existedTagDb.Items.Count > 0)
                    {
                        await dishTagRepository.DeleteRange(existedTagDb.Items);
                    }

                    foreach (var tagId in comboDto.TagIds)
                    {
                        var tagDb = await tagRepository.GetById(tagId);
                        if (tagDb != null)
                        {
                            dishTags.Add(new DishTag
                            {
                                DishTagId = Guid.NewGuid(),
                                ComboId = comboDb.ComboId,
                                TagId = tagId
                            });
                        }
                    }

                    List<ComboOptionSet> comboOptionSetList = new List<ComboOptionSet>();
                    List<ComboOptionSet> updateComboOptionSetList = new List<ComboOptionSet>();
                    List<ComboOptionSet> removeComboOptionSetList = new List<ComboOptionSet>();
                    List<DishCombo> dishComboList = new List<DishCombo>();
                    List<DishCombo> upateDishComboList = new List<DishCombo>();
                    List<DishCombo> removeDishComboList = new List<DishCombo>();
                    if (comboDto.DishComboDtos.Count > 0)
                    {
                        foreach (var dishComboDto in comboDto.DishComboDtos)
                        {
                            if (dishComboDto.OptionSetId.HasValue)
                            {
                                var optionSetDb =
                                    await comboOptionSetRepository!.GetById(dishComboDto.OptionSetId.Value);
                                if (optionSetDb == null)
                                {
                                    throw new Exception($"Không tìm thấy Set với id {dishComboDto.OptionSetId.Value}");
                                }

                                optionSetDb.OptionSetNumber = dishComboDto.OptionSetNumber;
                                optionSetDb.NumOfChoice = dishComboDto.NumOfChoice;
                                optionSetDb.DishItemTypeId = dishComboDto.DishItemType;
                                updateComboOptionSetList.Add(optionSetDb);

                                foreach (var dishId in dishComboDto.ListDishId)
                                {
                                    var dishExisted = await dishSizeDetailRepository!.GetById(dishId.DishSizeDetailId);
                                    if (dishExisted == null)
                                    {
                                        throw new Exception($"size món ăn với id {dishId.DishSizeDetailId} không tồn tại");
                                    }

                                    var existedComboDishDb = await dishComboRepository.GetByExpression(
                                        o => o.DishSizeDetailId == dishId.DishSizeDetailId &&
                                             o.ComboOptionSetId == optionSetDb.ComboOptionSetId, null);
                                    if (existedComboDishDb != null)
                                    {
                                        existedComboDishDb.Quantity = dishId.Quantity;
                                        upateDishComboList.Add(existedComboDishDb);
                                    }
                                    else
                                    {
                                        var dishCombo = new DishCombo
                                        {
                                            DishComboId = Guid.NewGuid(),
                                            ComboOptionSetId = optionSetDb.ComboOptionSetId,
                                            DishSizeDetailId = dishId.DishSizeDetailId,
                                            Quantity = dishId.Quantity,
                                            QuantityLeft = dishExisted.QuantityLeft / dishId.Quantity,
                                            IsAvailable = dishExisted.QuantityLeft / dishId.Quantity > 0,
                                            IsDeleted = false
                                        };
                                        dishComboList.Add(dishCombo);
                                    }
                                }
                            }
                            else
                            {
                                var comboOptionSet = new ComboOptionSet
                                {
                                    ComboOptionSetId = Guid.NewGuid(),
                                    DishItemTypeId = dishComboDto.DishItemType,
                                    NumOfChoice = dishComboDto.NumOfChoice,
                                    OptionSetNumber = dishComboDto.OptionSetNumber,
                                    ComboId = comboDb.ComboId
                                };
                                comboOptionSetList.Add(comboOptionSet);

                                foreach (var dishId in dishComboDto.ListDishId)
                                {
                                    var dishExisted = await dishSizeDetailRepository!.GetById(dishId.DishSizeDetailId);
                                    if (dishExisted == null)
                                    {
                                        throw new Exception($"size món ăn với id {dishId.DishSizeDetailId} không tồn tại");
                                    }

                                    var dishCombo = new DishCombo
                                    {
                                        DishComboId = Guid.NewGuid(),
                                        ComboOptionSetId = comboOptionSet.ComboOptionSetId,
                                        DishSizeDetailId = dishId.DishSizeDetailId,
                                        Quantity = dishId.Quantity,
                                        QuantityLeft = dishExisted.QuantityLeft / dishId.Quantity,
                                        IsAvailable = dishExisted.QuantityLeft / dishId.Quantity > 0,
                                        IsDeleted = false
                                    };
                                    dishComboList.Add(dishCombo);
                                }
                            }
                        }
                    }

                    var removeComboOptionSetDb = await comboOptionSetRepository.GetAllDataByExpression(c =>
                        !updateComboOptionSetList
                            .Select(u => u.ComboOptionSetId).Contains(c.ComboOptionSetId)
                        && c.ComboId == comboDto.ComboId, 0, 0, null, false, null);
                    if (removeComboOptionSetDb.Items.Count > 0)
                    {
                        removeComboOptionSetList.AddRange(removeComboOptionSetDb.Items);
                        removeComboOptionSetList.ForEach(r => r.IsDeleted = true);
                    }

                    var insertedDishIds = comboDto.DishComboDtos.SelectMany(c =>
                        c.ListDishId.Where(l => l.DishSizeDetailId != null).Select(l => l.DishSizeDetailId)).ToList();
                    var removeDishComboDb = await dishComboRepository.GetAllDataByExpression(
                        c => !insertedDishIds.Contains(c.DishSizeDetailId.Value) &&
                             c.ComboOptionSet.ComboId == comboDto.ComboId
                        , 0, 0, null, false, null);
                    removeDishComboList.AddRange(removeDishComboDb.Items);
                    removeDishComboList.ForEach(r => r.IsDeleted = true);

                    if (!BuildAppActionResultIsError(result))
                    {
                        await _comboRepository.Update(comboDb);
                        await dishTagRepository.InsertRange(dishTags);
                        await comboOptionSetRepository!.InsertRange(comboOptionSetList);
                        await comboOptionSetRepository!.UpdateRange(updateComboOptionSetList);
                        await comboOptionSetRepository!.UpdateRange(removeComboOptionSetList);
                        await dishComboRepository!.InsertRange(dishComboList);
                        await dishComboRepository!.UpdateRange(upateDishComboList);
                        await dishComboRepository!.UpdateRange(removeDishComboList);
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

        public async Task<AppActionResult> ActivateCombo(Guid comboId)
        {
            AppActionResult result = new AppActionResult();
            try
            {
                var comboDb = await _comboRepository.GetById(comboId);
                if (comboDb == null)
                {
                    return BuildAppActionResultError(result, $"Combo với id {comboId} không tồn tại");
                }
                comboDb!.IsDeleted = false;
                var utility = Resolve<Utility>();
                if (comboDb.EndDate < utility.GetCurrentDateTimeInTimeZone())
                {
                    comboDb.StartDate = utility.GetCurrentDateTimeInTimeZone();
                    comboDb.EndDate = comboDb.StartDate.AddDays(100);
                }
                await _comboRepository.Update(comboDb);
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }
    }
}