using Humanizer;
using System;
using System.Collections.Generic;
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
using TPRestaurent.BackEndCore.Domain.Models;

namespace TPRestaurent.BackEndCore.Application.Implementation
{
    public class ComboService : GenericBackendService , IComboService
    {
        private readonly IGenericRepository<Combo> _comboRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ComboService(IServiceProvider serviceProvider, IGenericRepository<Combo> comboRepository, IUnitOfWork unitOfWork) : base(serviceProvider)
        {
            _comboRepository = comboRepository;   
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
                    var dishSizeDetailRepository = Resolve<IGenericRepository<DishSizeDetail>>();
                    var staticFileRepository = Resolve<IGenericRepository<StaticFile>>();
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

                    var mainFile = comboDto.MainImg;
                    if (mainFile == null)
                    {
                        result = BuildAppActionResultError(result, $"The main picture of the dish is empty");
                    }
                    var mainPathName = SD.FirebasePathName.COMBO_PREFIX + $"{comboDb.ComboId}_main.jpg";
                    var uploadMainPicture = await firebaseService!.UploadFileToFirebase(mainFile, mainPathName);
                   
                    comboDb.Image = uploadMainPicture!.Result!.ToString()!;

                    List<StaticFile> staticList = new List<StaticFile>();


                    foreach (var file in comboDto!.ImageFiles!)
                    {
                        var pathName = SD.FirebasePathName.COMBO_PREFIX + $"{comboDb.ComboId}{Guid.NewGuid()}.jpg";
                        var upload = await firebaseService!.UploadFileToFirebase(file, pathName );
                        var staticImg = new StaticFile
                        {
                            StaticFileId = Guid.NewGuid(),
                            ComboId = comboDb.ComboId,
                            Path = upload!.Result!.ToString()!
                        };
                        staticList.Add(staticImg);
                        if (!upload.IsSuccess)
                        {
                            result = BuildAppActionResultError(result, "Upload ảnh không thành công");
                        }
                    }

                    foreach (var dishComboDto in comboDto.DishComboDtos)
                    {
                        foreach (var dishId in dishComboDto.ListDishId)
                        {
                            var dishExisted = await dishSizeDetailRepository!.GetById(dishId);
                            if (dishExisted == null)
                            {
                                result = BuildAppActionResultError(result, $"Size món ăn với id {dishId} không tồn tại");
                            }
                            var dishCombo = new DishCombo
                            {
                                DishComboId = Guid.NewGuid(),
                                ComboId = comboDb.ComboId,
                                OptionSetNumber = dishComboDto.OptionSetNumber,
                                HasOptions = dishComboDto.HasOptions,
                                DishSizeDetailId = dishId
                            };
                            await dishComboRepository!.Insert(dishCombo);
                        }
                    }

                    if (!BuildAppActionResultIsError(result))
                    {
                        await _comboRepository.Insert(comboDb);
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
                var comboDb = 
                    await _comboRepository.GetAllDataByExpression((p => p.Name.Contains(keyword) || string.IsNullOrEmpty(keyword)), pageNumber, pageSize, null, false, c => c.Category);
                result.Result = comboDb;        
            }
            catch (Exception ex) 
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }

        public async Task<AppActionResult> GetComboById(Guid comboId)
        {
            var result = new AppActionResult();
            try
            {
                var dishComboRepository = Resolve<IGenericRepository<DishCombo>>();
                var staticFileRepository = Resolve<IGenericRepository<StaticFile>>();
                var comboResponse = new ComboResponseDto();
                var comboDb = await _comboRepository.GetByExpression(p => p!.ComboId == comboId, p => p.Category);
                if (comboDb == null)
                {
                    result = BuildAppActionResultError(result, $"Combo với id {comboId} không tồn tại");
                }
                var dishComboDb = await dishComboRepository!.GetAllDataByExpression(p => p.ComboId == comboId, 0, 0, null, false, p => p.DishSizeDetail.Dish!);
                var staticFileDb = await staticFileRepository!.GetAllDataByExpression(p => p.ComboId == comboId, 0, 0, null, false, null);

                comboResponse.DishCombo = dishComboDb.Items!;
                comboResponse.Imgs = staticFileDb.Items!.Select(s => s.Path).ToList();
                comboResponse.Combo = comboDb!;
                result.Result = comboResponse;
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
                    var firebaseService = Resolve<IFirebaseService>();
                    var staticFileRepository = Resolve<IGenericRepository<StaticFile>>();




                    var mainPathName = SD.FirebasePathName.COMBO_PREFIX + $"{comboDto!.ComboId}.jpg";
                    var mainImageResult = firebaseService!.DeleteFileFromFirebase(mainPathName);
                    if (mainImageResult != null)
                    {
                        result.Messages.Add("Xóa các file hình ảnh trên cloud thành công");
                    }

                    var mainFile = comboDto.MainImg;
                    if (mainFile == null)
                    {
                        result = BuildAppActionResultError(result, $"The main picture of the dish is empty");
                    }

                    var uploadMainPicture = await firebaseService!.UploadFileToFirebase(mainFile, mainPathName);

                    var oldFiles = await staticFileRepository!.GetAllDataByExpression(p => p.ComboId == comboDto.ComboId, 0, 0, null, false,null);
                    if (oldFiles.Items == null || !oldFiles.Items.Any())
                    {
                        return BuildAppActionResultError(result, $"Các file hình ảnh của combo với id {comboDto.ComboId} không tồn tại");
                    }
                    var oldFileList = new List<StaticFile>();
                    foreach (var oldImg in oldFiles.Items!)
                    {
                        oldFileList.Add(oldImg);
                        var pathName = SD.FirebasePathName.COMBO_PREFIX + $"{comboDto!.ComboId}.jpg";
                        var imageResult = firebaseService!.DeleteFileFromFirebase(pathName);
                        if (imageResult != null)
                        {
                            result.Messages.Add("Xóa các file hình ảnh trên cloud thành công");
                        }
                    }
                    if (!oldFileList.Any())
                    {
                        return BuildAppActionResultError(result, "Không còn hình ảnh nào để xóa");
                    }

                    await staticFileRepository.DeleteRange(oldFileList);


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
                    comboDb.Image = uploadMainPicture!.Result!.ToString()!;

                    List<StaticFile> staticList = new List<StaticFile>();
                    foreach (var file in comboDto!.ImageFiles!)
                    {
                        var pathName = SD.FirebasePathName.COMBO_PREFIX + $"{comboDb.ComboId}{Guid.NewGuid()}.jpg";
                        var upload = await firebaseService!.UploadFileToFirebase(file, pathName);
                        var staticImg = new StaticFile
                        {
                            StaticFileId = Guid.NewGuid(),
                            ComboId = comboDb.ComboId,
                            Path = upload!.Result!.ToString()!,
                        };
                        staticList.Add(staticImg);

                        if (!upload.IsSuccess)
                        {
                            return BuildAppActionResultError(result, "Upload hình ảnh không thành công");
                        }

                    }

                    var dishComboDb = await dishComboRepository!.GetAllDataByExpression(p => p.ComboId == comboDb.ComboId, 0, 0, null, false, p => p.DishSizeDetail.Dish!);
                    foreach (var dishComboDto in comboDto.DishComboDtos)
                    {
                        foreach (var dishId in dishComboDto.ListDishId)
                        {
                            var existingDishCombo = await dishComboRepository.GetById(dishComboDto.DishComboId);
                            if (existingDishCombo == null)
                            {
                                var newDishCombo = new DishCombo
                                {
                                    DishComboId = dishComboDto.DishComboId, 
                                    ComboId = comboDb.ComboId,
                                    HasOptions = dishComboDto.HasOptions,
                                    OptionSetNumber = dishComboDto.OptionSetNumber, 
                                    //DishSizeDetailId = dishId,    
                                };
                                await dishComboRepository.Insert(newDishCombo);
                            }
                            else
                            {
                                existingDishCombo.HasOptions = dishComboDto.HasOptions; 
                                existingDishCombo.OptionSetNumber = dishComboDto.OptionSetNumber;
                                //existingDishCombo.DishSizeDetailId = dishId;
                                await dishComboRepository.Update(existingDishCombo);    
                            }
                        }
                    }

                    var updatedDishComboIds = comboDto.DishComboDtos.Select(dc => dc.DishComboId).ToHashSet();
                    var dishCombosToRemove = dishComboDb.Items.Where(dc => !updatedDishComboIds.Contains(dc.DishComboId));
                    foreach (var dishComboToRemove in dishCombosToRemove)
                    {
                        await dishComboRepository.DeleteById(dishComboToRemove.DishComboId);
                    }
                    if (!BuildAppActionResultIsError(result))
                    {
                        await _comboRepository.Update(comboDb);
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
