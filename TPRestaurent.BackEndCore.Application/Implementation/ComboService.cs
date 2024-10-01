using AutoMapper;
using Hangfire.Logging.LogProviders;
using Humanizer;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
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
        private  IGenericRepository<Combo> _comboRepository;
        private IMapper _mapper;
        private  IUnitOfWork _unitOfWork;

        public ComboService(IServiceProvider serviceProvider, IGenericRepository<Combo> comboRepository, IMapper mapper, IUnitOfWork unitOfWork) : base(serviceProvider)
        {
            _comboRepository = comboRepository;
            _mapper = mapper;
            _unitOfWork = unitOfWork;   
        }

        public async Task<AppActionResult> CreateCombo(ComboDto comboDto)
        {
            var result = new AppActionResult();
            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
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
                        result = BuildAppActionResultError(result, $"Combo này với {comboDto.Name} đã tồn tại");
                    }

                    var comboDb = new Combo
                    {
                        ComboId = Guid.NewGuid(),
                        Description = comboDto.Description,
                        Name = comboDto.Name,
                        EndDate = comboDto.EndDate,
                        Price = comboDto.Price,
                        StartDate = comboDto.StartDate,
                    };

                    List<DishTag> dishTags = new List<DishTag>();
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
                    List<ComboOptionSet> comboOptionSetList =new List<ComboOptionSet>();
                    List<DishCombo> dishComboList =new List<DishCombo>();
                    foreach (var dishComboDto in comboDto.DishComboDtos)
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
                                result = BuildAppActionResultError(result, $"size món ăn với id {dishId.DishSizeDetailId} không tồn tại");
                            }
                            var dishCombo = new DishCombo
                            {
                                DishComboId = Guid.NewGuid(),
                                ComboOptionSetId = comboOptionSet.ComboOptionSetId,
                                DishSizeDetailId = dishId.DishSizeDetailId,
                                Quantity = dishId.Quantity
                            };
                            dishComboList.Add(dishCombo);
                        }
                    }

                    if (!BuildAppActionResultIsError(result))
                    {
                        await _comboRepository.Insert(comboDb);
                        await dishTagRepository.InsertRange(dishTags);
                        await comboOptionSetRepository!.InsertRange(comboOptionSetList);
                        await dishComboRepository!.InsertRange(dishComboList);
                        await _unitOfWork.SaveChangesAsync();
                        scope.Complete();
                    }
                }
                catch (Exception ex)
                {
                    result = BuildAppActionResultError(result, ex.Message);
                }
            }
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
                    result = BuildAppActionResultError(result, $"Combo với id {comboId} không tồn tại");
                }
                await _comboRepository.DeleteById(comboId); 
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

        public async Task<AppActionResult> GetAllCombo(string? keyword, int pageNumber, int pageSize)
        {
            var result = new AppActionResult();
            try
            {
                var currentDateTime = Resolve<Utility>().GetCurrentDateTimeInTimeZone();
                var comboDb = await _comboRepository.GetAllDataByExpression(
                    p => (string.IsNullOrEmpty(keyword) || p.Name.Contains(keyword)) && p.EndDate > currentDateTime,
                    pageNumber, pageSize, null, false, c => c.Category
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
                    0, 0, null, false, null
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
                    result = BuildAppActionResultError(result, $"Combo với id {comboId} không tồn tại");
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
                        if(ratingDb != null)
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
                    result = BuildAppActionResultError(result, $"Combo với id {comboId} không tồn tại");
                }

                var dishTagDb = await dishTagRepository!.GetAllDataByExpression(d => d.ComboId == comboId, 0, 0, null, false, d => d.Tag);
                comboResponse.DishTags = dishTagDb.Items;

                var dishComboDb = await dishComboRepository!.GetAllDataByExpression(p => p.ComboOptionSet.ComboId == comboId, 0, 0, null, false, p => p.DishSizeDetail.Dish!, p => p.DishSizeDetail.DishSize, p => p.ComboOptionSet.DishItemType);
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
                        var ratingDb = await ratingRepository!.GetByExpression(p => p.OrderDetailId == orderDetail.OrderDetailId);
                        if(ratingDb != null)
                        {
                            ratingListDb.Add(ratingDb);
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
            try
            {
                var comboDb = await _comboRepository.GetById(comboDto.ComboId);
                if (comboDb == null)
                {
                    return BuildAppActionResultError(result, $"Không tìm thấy combo với id {comboDto.ComboId}");
                }
                var firebaseService = Resolve<IFirebaseService>();
                var staticFileRepository = Resolve<IGenericRepository<Image>>();

                if (comboDto.MainImg != null)
                {
                    var mainFile = comboDto.MainImg;
                    if (mainFile == null)
                    {
                        result = BuildAppActionResultError(result, $"The main picture of the dish is empty");
                    }

                    if(comboDb.Image != null)
                    {
                        var deleteImage = await firebaseService.DeleteFileFromFirebase(comboDb.Image);
                        if (!deleteImage.IsSuccess)
                        {
                            return BuildAppActionResultError(result, $"Xảy ra lỗi khi xoá ảnh");
                        }
                    }

                    var mainPathName = SD.FirebasePathName.COMBO_PREFIX + $"{comboDb.ComboId}_main.jpg";
                    var uploadMainPicture = await firebaseService!.UploadFileToFirebase(mainFile, mainPathName);
                   
                    comboDb.Image = uploadMainPicture!.Result!.ToString()!;
                }

                List<Image> staticList = new List<Image>();

                var comboImageDb = await staticFileRepository.GetAllDataByExpression(c => c.ComboId.HasValue && c.ComboId.Value == comboDto.ComboId, 0, 0, null, false, null);
                if (comboImageDb.Items.Count() > 0)
                {
                    foreach (var image in comboImageDb.Items)
                    {
                        var deleteImage = await firebaseService.DeleteFileFromFirebase(image.Path);
                        if (!deleteImage.IsSuccess)
                        {
                            return BuildAppActionResultError(result, $"Xảy ra lỗi khi xoá ảnh");
                        }
                    }

                    await staticFileRepository.DeleteRange(comboImageDb.Items);
                }

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
                        return BuildAppActionResultError(result, "Upload ảnh không thành công");
                    }
                }

                await _comboRepository.Update(comboDb);
                await staticFileRepository.InsertRange(staticList);
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }


        public async Task<AppActionResult> UpdateCombo(UpdateComboDto comboDto)
        {
            var result = new AppActionResult();
            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
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

                    var orderDetailDb = await orderDetailRepository.GetAllDataByExpression(o => o.ComboId != null && o.ComboId == comboDto.ComboId, 0, 0, null, false, null);

                    var comboDb = await _comboRepository.GetByExpression(p => p.ComboId == comboDto.ComboId);
                    if (comboDb == null)
                    {
                        result = BuildAppActionResultError(result, $"Combo với id {comboDto.ComboId} không tồn tại");
                    }
                    comboDb.Name = comboDto.Name;
                    comboDb.Description = comboDto.Description;
                    comboDb.StartDate = comboDto.StartDate;
                    comboDb.EndDate = comboDto.EndDate;
                    comboDb.Price = comboDto.Price;

                    List<DishTag> dishTags = new List<DishTag>();

                    var existedTagDb = await dishTagRepository.GetAllDataByExpression(d => d.ComboId == comboDto.ComboId, 0, 0, null, false, null);
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
                    List<DishCombo> dishComboList = new List<DishCombo>();
                    if(comboDto.DishComboDtos.Count > 0)
                    {
                        if(orderDetailDb.Items.Count > 0)
                        {
                            result = BuildAppActionResultError(result, $"Combo với id {comboDto.ComboId} đã có đặt món, không thể thực hiện cập nhật chi tiết món");
                        }
                        var existedComboOptionSetList = await comboOptionSetRepository.GetAllDataByExpression(c => c.ComboId == comboDto.ComboId, 0, 0, null, false, null);
                        if(existedComboOptionSetList.Items.Count()  > 0)
                        {
                            var existedComboOptionSetListIds = existedComboOptionSetList.Items.Select(c => c.ComboOptionSetId).ToList();
                            var existedDishComboList = await dishComboRepository.GetAllDataByExpression(d => existedComboOptionSetListIds.Contains((Guid)d.ComboOptionSetId), 0, 0, null, false, null);
                            if (existedDishComboList.Items.Count() > 0)
                            {
                                await dishComboRepository.DeleteRange(existedDishComboList.Items);
                            }
                            await comboOptionSetRepository.DeleteRange(existedComboOptionSetList.Items);
                        }  
                    
                        foreach (var dishComboDto in comboDto.DishComboDtos)
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
                                    result = BuildAppActionResultError(result, $"size món ăn với id {dishId.DishSizeDetailId} không tồn tại");
                                }
                                var dishCombo = new DishCombo
                                {
                                    DishComboId = Guid.NewGuid(),
                                    ComboOptionSetId = comboOptionSet.ComboOptionSetId,
                                    DishSizeDetailId = dishId.DishSizeDetailId,
                                    Quantity = dishId.Quantity
                                };
                                dishComboList.Add(dishCombo);
                            }
                        }
                    }

                    if (!BuildAppActionResultIsError(result))
                    {
                        await _comboRepository.Update(comboDb);
                        await dishTagRepository.InsertRange(dishTags);
                        await comboOptionSetRepository!.InsertRange(comboOptionSetList);
                        await dishComboRepository!.InsertRange(dishComboList);
                        await _unitOfWork.SaveChangesAsync();
                        scope.Complete();
                    }
                }
                catch (Exception ex)
                {
                    result = BuildAppActionResultError(result, ex.Message);
                }
                return result;
            }
        }
    }
}
