using AutoMapper;
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
    public class ComboService : GenericBackendService , IComboService
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

                    var mainFile = comboDto.MainImg;
                    if (mainFile == null)
                    {
                        result = BuildAppActionResultError(result, $"The main picture of the dish is empty");
                    }
                    var mainPathName = SD.FirebasePathName.COMBO_PREFIX + $"{comboDb.ComboId}_main.jpg";
                    var uploadMainPicture = await firebaseService!.UploadFileToFirebase(mainFile, mainPathName);
                   
                    comboDb.Image = uploadMainPicture!.Result!.ToString()!;

                    List<Image> staticList = new List<Image>();


                    foreach (var file in comboDto!.ImageFiles!)
                    {
                        var pathName = SD.FirebasePathName.COMBO_PREFIX + $"{comboDb.ComboId}{Guid.NewGuid()}.jpg";
                        var upload = await firebaseService!.UploadFileToFirebase(file, pathName );
                        var staticImg = new Image
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
                        await staticFileRepository!.InsertRange(staticList);
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

        //public async Task<AppActionResult> UpdateCombo(UpdateComboDto comboDto)
        //{
        //    var result = new AppActionResult();
        //    using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //    {
        //        try
        //        {
        //            var comboResponse = new ComboResponseDto();
        //            var dishComboRepository = Resolve<IGenericRepository<DishCombo>>();
        //            var firebaseService = Resolve<IFirebaseService>();
        //            var staticFileRepository = Resolve<IGenericRepository<StaticFile>>();




        //            var mainPathName = SD.FirebasePathName.COMBO_PREFIX + $"{comboDto!.ComboId}.jpg";
        //            var mainImageResult = firebaseService!.DeleteFileFromFirebase(mainPathName);
        //            if (mainImageResult != null)
        //            {
        //                result.Messages.Add("Xóa các file hình ảnh trên cloud thành công");
        //            }

        //            var mainFile = comboDto.MainImg;
        //            if (mainFile == null)
        //            {
        //                result = BuildAppActionResultError(result, $"The main picture of the dish is empty");
        //            }

        //            var uploadMainPicture = await firebaseService!.UploadFileToFirebase(mainFile, mainPathName);

        //            var oldFiles = await staticFileRepository!.GetAllDataByExpression(p => p.ComboId == comboDto.ComboId, 0, 0, null, false,null);
        //            if (oldFiles.Items == null || !oldFiles.Items.Any())
        //            {
        //                return BuildAppActionResultError(result, $"Các file hình ảnh của combo với id {comboDto.ComboId} không tồn tại");
        //            }
        //            var oldFileList = new List<StaticFile>();
        //            foreach (var oldImg in oldFiles.Items!)
        //            {
        //                oldFileList.Add(oldImg);
        //                var pathName = SD.FirebasePathName.COMBO_PREFIX + $"{comboDto!.ComboId}.jpg";
        //                var imageResult = firebaseService!.DeleteFileFromFirebase(pathName);
        //                if (imageResult != null)
        //                {
        //                    result.Messages.Add("Xóa các file hình ảnh trên cloud thành công");
        //                }
        //            }
        //            if (!oldFileList.Any())
        //            {
        //                return BuildAppActionResultError(result, "Không còn hình ảnh nào để xóa");
        //            }

        //            await staticFileRepository.DeleteRange(oldFileList);


        //            var comboDb = await _comboRepository.GetByExpression(p => p.ComboId == comboDto.ComboId);
        //            if (comboDb == null)
        //            {
        //                result = BuildAppActionResultError(result, $"Combo với id {comboDto.ComboId} không tồn tại");
        //            }
        //            comboDb.Name = comboDto.Name;
        //            comboDb.Description = comboDto.Description;
        //            comboDb.StartDate = comboDto.StartDate;
        //            comboDb.EndDate = comboDto.EndDate;
        //            comboDb.Price = comboDto.Price;
        //            comboDb.Image = uploadMainPicture!.Result!.ToString()!;

        //            List<StaticFile> staticList = new List<StaticFile>();
        //            foreach (var file in comboDto!.ImageFiles!)
        //            {
        //                var pathName = SD.FirebasePathName.COMBO_PREFIX + $"{comboDb.ComboId}{Guid.NewGuid()}.jpg";
        //                var upload = await firebaseService!.UploadFileToFirebase(file, pathName);
        //                var staticImg = new StaticFile
        //                {
        //                    StaticFileId = Guid.NewGuid(),
        //                    ComboId = comboDb.ComboId,
        //                    Path = upload!.Result!.ToString()!,
        //                };
        //                staticList.Add(staticImg);

        //                if (!upload.IsSuccess)
        //                {
        //                    return BuildAppActionResultError(result, "Upload hình ảnh không thành công");
        //                }

        //            }

        //            var dishComboDb = await dishComboRepository!.GetAllDataByExpression(p => p.ComboId == comboDb.ComboId, 0, 0, null, false, p => p.DishSizeDetail.Dish!);
        //            foreach (var dishComboDto in comboDto.DishComboDtos)
        //            {
        //                foreach (var dishId in dishComboDto.ListDishId)
        //                {
        //                    var existingDishCombo = await dishComboRepository.GetById(dishComboDto.DishComboId);
        //                    if (existingDishCombo == null)
        //                    {
        //                        var newDishCombo = new DishCombo
        //                        {
        //                            DishComboId = dishComboDto.DishComboId, 
        //                            ComboId = comboDb.ComboId,
        //                            HasOptions = dishComboDto.HasOptions,
        //                            OptionSetNumber = dishComboDto.OptionSetNumber, 
        //                            //DishSizeDetailId = dishId,    
        //                        };
        //                        await dishComboRepository.Insert(newDishCombo);
        //                    }
        //                    else
        //                    {
        //                        existingDishCombo.HasOptions = dishComboDto.HasOptions; 
        //                        existingDishCombo.OptionSetNumber = dishComboDto.OptionSetNumber;
        //                        //existingDishCombo.DishSizeDetailId = dishId;
        //                        await dishComboRepository.Update(existingDishCombo);    
        //                    }
        //                }
        //            }

        //            var updatedDishComboIds = comboDto.DishComboDtos.Select(dc => dc.DishComboId).ToHashSet();
        //            var dishCombosToRemove = dishComboDb.Items.Where(dc => !updatedDishComboIds.Contains(dc.DishComboId));
        //            foreach (var dishComboToRemove in dishCombosToRemove)
        //            {
        //                await dishComboRepository.DeleteById(dishComboToRemove.DishComboId);
        //            }
        //            if (!BuildAppActionResultIsError(result))
        //            {
        //                await _comboRepository.Update(comboDb);
        //                await _unitOfWork.SaveChangesAsync();
        //                scope.Complete();
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            result = BuildAppActionResultError(result, ex.Message);
        //        }
        //        return result;
        //    }
        //}
    }
}
