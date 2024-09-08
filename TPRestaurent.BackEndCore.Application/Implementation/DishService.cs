using AutoMapper;
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
                    var staticFileRepository = Resolve<IGenericRepository<StaticFile>>();
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

                    List<StaticFile> staticList = new List<StaticFile>();

                    var mainFile = dto.MainImageFile;
                    if (mainFile == null)
                    {
                        result = BuildAppActionResultError(result, $"The main picture of the dish is empty");
                    }
                    var mainPathName = SD.FirebasePathName.DISH_PREFIX + $"{dish.DishId}_main.jpg";
                    var uploadMainPicture = await firebaseService!.UploadFileToFirebase(mainFile, mainPathName);
                    var staticMainFile = new StaticFile
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
                        var staticImg = new StaticFile
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
                    Items = dishSizeList,
                    TotalPages = dishList.TotalPages,
                };
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }



        public async Task<AppActionResult> GetDishById(Guid dishId)
        {
            var result = new AppActionResult();
            var dishResponse = new DishResponse();
            var staticFileRepository = Resolve<IGenericRepository<StaticFile>>();
            var ratingRepository = Resolve<IGenericRepository<Rating>>();
            var dishSizeRepository = Resolve<IGenericRepository<DishSizeDetail>>();
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
                    result = BuildAppActionResultError(result, $"Size món ăn với id {dishId} không tồn tại");
                }
                dishResponse.dishSizeDetails = dishSizeDetailsDb!.Items!.OrderBy(d => d.DishSizeId).ToList();
                var staticFileDb = await staticFileRepository!.GetAllDataByExpression(p => p.DishId == dishId, 0, 0, null, false, null);

                var ratingDb = await ratingRepository!.GetAllDataByExpression(p => p.DishId == dishId, 0, 0, null, false, p => p.CreateByAccount, p => p.UpdateByAccount);

                if (ratingDb.Items.Count > 0)
                {
                    foreach (var rating in ratingDb.Items!)
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
                    var staticFileRepository = Resolve<IGenericRepository<StaticFile>>();
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
                    var oldFileList = new List<StaticFile>();
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

                    List<StaticFile> staticList = new List<StaticFile>();
                    var mainFile = dto.MainImageFile;
                    if (mainFile == null)
                    {
                        result = BuildAppActionResultError(result, $"The main picture of the dish is empty");
                    }
                    var mainPathName = SD.FirebasePathName.DISH_PREFIX + $"{dishDb.DishId}_main.jpg";
                    var uploadMainPicture = await firebaseService!.UploadFileToFirebase(mainFile, mainPathName);
                    var staticMainFile = new StaticFile
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
                        var staticImg = new StaticFile
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
