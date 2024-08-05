using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using TPRestaurent.BackEndCore.Application.Contract.IServices;
using TPRestaurent.BackEndCore.Application.IRepositories;
using TPRestaurent.BackEndCore.Common.DTO.Request;
using TPRestaurent.BackEndCore.Common.DTO.Response.BaseDTO;
using TPRestaurent.BackEndCore.Common.Utils;
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
                        Discount = dto.Discount,
                        DishItemType = dto.DishItemType,
                        isAvailable = true,
                    };
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
                            result = BuildAppActionResultError(result, "Upload failed");
                        }
                    }
                    if (!BuildAppActionResultIsError(result))
                    {
                        await _dishRepository.Insert(dish);
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

        public async Task<AppActionResult> GetAllDish(string? keyword, int pageNumber, int pageSize)
        {
            var result = new AppActionResult();
            try
            {
                 var dishList = await _dishRepository
                    .GetAllDataByExpression(p => p.Name.Contains(keyword) || string.IsNullOrEmpty(keyword), pageNumber, pageSize, null, false, p => p.DishItemType!);
                 result.Result = dishList;  
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
            try
            {
                result.Result = await _dishRepository.GetByExpression(p => p.DishId == dishId);
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
                    dishDb.Price = dto.Price;
                    dishDb.DishItemType = dto.DishItemType;
                    dishDb.Discount = dto.Discount;
                    dishDb.isAvailable = dto.isAvailable;
                    if (!BuildAppActionResultIsError(result))
                    {
                        await _dishRepository.Update(dishDb!);
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
