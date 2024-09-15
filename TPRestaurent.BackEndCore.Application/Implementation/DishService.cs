﻿using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Web.Razor.Tokenizer.Symbols;
using TPRestaurent.BackEndCore.Application.Contract.IServices;
using TPRestaurent.BackEndCore.Application.IRepositories;
using TPRestaurent.BackEndCore.Common.DTO.Request;
using TPRestaurent.BackEndCore.Common.DTO.Response;
using TPRestaurent.BackEndCore.Common.DTO.Response.BaseDTO;
using TPRestaurent.BackEndCore.Common.Utils;
using TPRestaurent.BackEndCore.Domain.Enums;
using TPRestaurent.BackEndCore.Domain.Models;
using Twilio.Http;

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
            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    var firebaseService = Resolve<IFirebaseService>();
                    var dishSizeDetailRepository = Resolve<IGenericRepository<DishSizeDetail>>();
                    var staticFileRepository = Resolve<IGenericRepository<Image>>();
                    var dishExsted = await _dishRepository.GetByExpression(p => p.Name == dto.Name);
                    if (dishExsted != null)
                    {
                        result = BuildAppActionResultError(result, $"This dish with the name {dto.Name} is already exsited");
                    }
                    var dish = new Dish
                    {
                        DishId = Guid.NewGuid(),
                        Name = dto.Name,
                        Description = dto.Description,
                        //Discount = dto.Discount,
                        DishItemTypeId = dto.DishItemType,
                        isAvailable = true,
                    };
                    List<DishSizeDetail> dishSizeDetails = new List<DishSizeDetail>();
                    if (dto.DishSizeDetailDtos.Count > 0)
                    {
                        if(dto.DishSizeDetailDtos.Count(d => d.DishSize == DishSize.SMALL) > 1 
                        || dto.DishSizeDetailDtos.Count(d => d.DishSize == DishSize.MEDIUM) > 1
                        || dto.DishSizeDetailDtos.Count(d => d.DishSize == DishSize.LARGE) > 1)
                        {
                            result = BuildAppActionResultError(result, $"Món ăn tồn tại kích thước trùng. Vui lòng kiểm tra lại");
                        }
                        dto.DishSizeDetailDtos.ForEach(d =>
                            dishSizeDetails.Add(new DishSizeDetail
                            {
                                DishSizeDetailId = Guid.NewGuid(),
                                DishId = dish.DishId,
                                Discount = d.Discount,
                                DishSizeId = d.DishSize,
                                IsAvailable = true,
                                Price = d.Price                                
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
                        if(dto.DishSizeDetailDtos.Count > 0)
                        {
                            await dishSizeDetailRepository!.InsertRange(dishSizeDetails);
                        }
                        await staticFileRepository!.InsertRange(staticList);
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
                dishDb!.isAvailable = false;
                result.IsSuccess = true;
                result.Messages.Add("This dish has been delete successfully");
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }

        public async Task<AppActionResult> GetAllDish(string? keyword, DishItemType? type, int pageNumber, int pageSize)
        {
            var result = new AppActionResult();
            try
            {
                List<DishSizeResponse> dishSizeList = new List<DishSizeResponse>();
                var dishDetailsRepository = Resolve<IGenericRepository<DishSizeDetail>>();
                var dishList = await _dishRepository
                   .GetAllDataByExpression(p => (p.Name.Contains(keyword) && !string.IsNullOrEmpty(keyword) || string.IsNullOrEmpty(keyword))
                                             && (type.HasValue && p.DishItemTypeId == type || !type.HasValue), pageNumber, pageSize, null, false, p => p.DishItemType!);
                foreach (var item in dishList.Items!)
                {
                    var dishDetailsListDb = await dishDetailsRepository!.GetAllDataByExpression(p => p.DishId == item.DishId, 0, 0, null, false, p => p.DishSize!);
                    var dishSizeResponse = new DishSizeResponse();
                    dishSizeResponse.Dish = item;
                    dishSizeResponse.dishSizeDetails = dishDetailsListDb.Items!.OrderBy(d => d.DishSizeId).ToList();
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

        private async Task<List<DishSizeResponse>> GetListDishRatingInformation(List<DishSizeResponse> dishSizeResponses)
        {
            List<DishSizeResponse> responses = dishSizeResponses.ToList();
            try
            {
                var dishIds = dishSizeResponses.Select(d => d.Dish.DishId).ToList();
                var ratingRepository = Resolve<IGenericRepository<Rating>>();
                var ratingDb = await ratingRepository.GetAllDataByExpression(o => o.OrderDetail.DishSizeDetailId != null && dishIds.Contains(o.OrderDetail.DishSizeDetail.DishId.Value), 0, 0, null, false, null);
                var dishRating = ratingDb.Items.GroupBy(r => r.OrderDetail.DishSizeDetail.DishId).ToDictionary(r => r.Key, r => r.ToList());
                foreach (var response in responses)
                {
                    response.NumberOfRating = dishRating[response.Dish.DishId].Count();
                    response.AverageRating = dishRating[response.Dish.DishId].Average(r =>
                    {
                        if (r.PointId == RatingPoint.One) return 1;
                        if (r.PointId == RatingPoint.Two) return 2;
                        if (r.PointId == RatingPoint.Three) return 3;
                        if (r.PointId == RatingPoint.Four) return 4;
                        return 5;
                    });
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
            var dishResponse = new DishResponse();
            var staticFileRepository = Resolve<IGenericRepository<Image>>();
            var ratingRepository = Resolve<IGenericRepository<Rating>>();
            var dishSizeRepository = Resolve<IGenericRepository<DishSizeDetail>>();
            var orderDetailRepository = Resolve<IGenericRepository<OrderDetail>>();     
            var ratingListDb = new List<Rating>();  
            try
            {
                var dishDb = await _dishRepository.GetByExpression(p => p.DishId == dishId, p => p.DishItemType);
                if (dishDb == null)
                {
                    result = BuildAppActionResultError(result, $"Món ăn với id {dishId} không tồn tại");
                }
                var dishSizeDetailsDb = await dishSizeRepository.GetAllDataByExpression(p => p.DishId == dishId, 0, 0, null, false, p => p.Dish!, p => p.DishSize!);
                if (dishSizeDetailsDb!.Items!.Count < 0 && dishSizeDetailsDb.Items == null)
                {
                    result = BuildAppActionResultError(result, $"size món ăn với id {dishId} không tồn tại");
                }
                dishResponse.dishSizeDetails = dishSizeDetailsDb!.Items!.OrderBy(d => d.DishSizeId).ToList();
                var staticFileDb = await staticFileRepository!.GetAllDataByExpression(p => p.DishId == dishId, 0, 0, null, false, null);

                var orderDetailDb = await orderDetailRepository.GetAllDataByExpression(p => p.DishSizeDetail!.DishId == dishId && p.Order!.StatusId == OrderStatus.Completed, 0, 0, null, false, null);
                if (orderDetailDb!.Items!.Count > 0 && orderDetailDb.Items != null)
                {
                    foreach (var orderDetail in orderDetailDb.Items)
                    {
                        var ratingDb = await ratingRepository!.GetByExpression(p => p.OrderDetailId == orderDetail.OrderDetailId);
                        ratingListDb.Add(ratingDb);
                    }
                }

                if (ratingListDb.Count > 0)
                {
                    foreach (var rating in ratingListDb)
                    {
                        var ratingStaticFileDb = await staticFileRepository.GetAllDataByExpression(p => p.RatingId == rating.RatingId, 0, 0, null, false, null);
                        var ratingDishResponse = new RatingDishResponse
                        {
                            Rating = rating,
                            RatingImgs = ratingStaticFileDb.Items!
                        };
                        dishResponse.RatingDish.Add(ratingDishResponse);
                    }
                }

                dishResponse.Dish = dishDb!;
                dishResponse.DishImgs = staticFileDb.Items!;
                    dishResponse.NumberOfRating = ratingListDb.Count();
                    dishResponse.AverageRating = ratingListDb.Average(r =>
                    {
                        if (r.PointId == RatingPoint.One) return 1;
                        if (r.PointId == RatingPoint.Two) return 2;
                        if (r.PointId == RatingPoint.Three) return 3;
                        if (r.PointId == RatingPoint.Four) return 4;
                        return 5;
                    });
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
            var dishTypeRepository = Resolve<IGenericRepository<TPRestaurent.BackEndCore.Domain.Models.EnumModels.DishItemType>>();
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
            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
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
                    var oldFiles = await staticFileRepository!.GetAllDataByExpression(p => p.DishId == dto.DishId, 0, 0, null, false, null);
                    if (oldFiles.Items == null || !oldFiles.Items.Any())
                    {
                        return BuildAppActionResultError(result, $"Các file hình ảnh của món ăn với id {dishDb.DishId} không tồn tại");
                    }
                    var oldFileList = new List<Image>();
                    foreach (var oldImg in oldFiles.Items!)
                    {
                        oldFileList.Add(oldImg);
                        var pathName = SD.FirebasePathName.DISH_PREFIX + $"{dishDb!.DishId}.jpg";
                        var imageResult = firebaseService!.DeleteFileFromFirebase(pathName);
                        if (imageResult != null)
                        {
                            result.Messages.Add("Xóa các file hình ảnh thành công");
                        }
                    }

                    if (!oldFileList.Any())
                    {
                        return BuildAppActionResultError(result, "No old files found to delete.");
                    }

                    // Attempt to delete old files
                    await staticFileRepository.DeleteRange(oldFileList);
                    await _unitOfWork.SaveChangesAsync();

                    List<Image> staticList = new List<Image>();
                    var mainFile = dto.MainImageFile;
                    if (mainFile == null)
                    {
                        result = BuildAppActionResultError(result, $"The main picture of the dish is empty");
                    }
                    var mainPathName = SD.FirebasePathName.DISH_PREFIX + $"{dishDb.DishId}_main.jpg";
                    var uploadMainPicture = await firebaseService!.UploadFileToFirebase(mainFile, mainPathName);
                    var staticMainFile = new Image
                    {
                        StaticFileId = Guid.NewGuid(),
                        DishId = dishDb.DishId,
                        Path = uploadMainPicture!.Result!.ToString()!
                    };
                    staticList.Add(staticMainFile);
                    dishDb.Image = staticMainFile.Path;


                    foreach (var file in dto!.ImageFiles!)
                    {
                        var pathName = SD.FirebasePathName.DISH_PREFIX + $"{dishDb.DishId}{Guid.NewGuid()}.jpg";
                        var upload = await firebaseService!.UploadFileToFirebase(file, pathName);
                        var staticImg = new Image
                        {
                            StaticFileId = Guid.NewGuid(),
                            DishId = dishDb.DishId,
                            Path = upload!.Result!.ToString()!,
                        };
                        staticList.Add(staticImg);

                        if (!upload.IsSuccess)
                        {
                            return BuildAppActionResultError(result, "Upload hình ảnh không thành công");
                        }
                    }

                    dishDb.Name = dto.Name;
                    dishDb.Description = dto.Description;
                    //dishDb.Price = dto.Price;
                    dishDb.DishItemTypeId = dto.DishItemType;
                    //dishDb.Discount = dto.Discount;
                    dishDb.isAvailable = dto.IsAvailable;

                    List<DishSizeDetail> updateDishSizeDetails = new List<DishSizeDetail>();
                    List<DishSizeDetail> addDishSizeDetails = new List<DishSizeDetail>();
                    if (dto.UpdateDishSizeDetailDtos.Count > 0)
                    {
                        dto.UpdateDishSizeDetailDtos.ForEach(d =>
                        {
                            if (dto.UpdateDishSizeDetailDtos.Count(d => d.DishSize == DishSize.SMALL) > 1
                       || dto.UpdateDishSizeDetailDtos.Count(d => d.DishSize == DishSize.MEDIUM) > 1
                       || dto.UpdateDishSizeDetailDtos.Count(d => d.DishSize == DishSize.LARGE) > 1)
                            {
                                result = BuildAppActionResultError(result, $"Món ăn tồn tại kích thước trùng. Vui lòng kiểm tra lại");
                            }
                            if (d.DishSizeDetailId.HasValue)
                            {
                                updateDishSizeDetails.Add(new DishSizeDetail
                                {
                                    DishSizeDetailId = (Guid)d.DishSizeDetailId,
                                    DishId = dishDb.DishId,
                                    DishSizeId = d.DishSize,
                                    Discount = d.Discount,
                                    IsAvailable = d.IsAvailable,
                                    Price = d.Price
                                });
                            }
                            else
                            {
                                addDishSizeDetails.Add(new DishSizeDetail
                                {
                                    DishSizeDetailId = Guid.NewGuid(),
                                    DishId = dishDb.DishId,
                                    DishSizeId = d.DishSize,
                                    Discount = d.Discount,
                                    IsAvailable = d.IsAvailable,
                                    Price = d.Price
                                });
                            }
                        });
                    }
                    

                    if (!BuildAppActionResultIsError(result))
                    {
                        await _dishRepository.Update(dishDb!);
                        if (dto.UpdateDishSizeDetailDtos.Count > 0)
                        {
                            await dishSizeDetailRepository!.UpdateRange(updateDishSizeDetails);
                        }
                        if(addDishSizeDetails.Count > 0)
                        {
                            await dishSizeDetailRepository!.InsertRange(addDishSizeDetails);
                        }
                        await staticFileRepository.InsertRange(staticList);
                        await _unitOfWork.SaveChangesAsync();
                        result.Messages.Add("Update dish successfully");
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
    }
}
