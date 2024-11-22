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
using TPRestaurent.BackEndCore.Common.DTO.Response.BaseDTO;
using TPRestaurent.BackEndCore.Common.Utils;
using TPRestaurent.BackEndCore.Domain.Enums;
using TPRestaurent.BackEndCore.Domain.Models;

namespace TPRestaurent.BackEndCore.Application.Implementation
{
    public class RatingService : GenericBackendService, IRatingService
    {
        private IGenericRepository<Rating> _ratingRepository;
        private IUnitOfWork _unitOfWork;

        public RatingService(IServiceProvider serviceProvider, IGenericRepository<Rating> ratingRepository, IUnitOfWork unitOfWork) : base(serviceProvider)
        {
            _ratingRepository = ratingRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<AppActionResult> CreateRating(CreateRatingRequestDto createRatingRequestDto)
        {
            var utility = Resolve<Utility>();
            var result = new AppActionResult();
            var firebaseService = Resolve<IFirebaseService>();
            var staticFileRepository = Resolve<IGenericRepository<Image>>();
            var listStaticFile = new List<Image>();

            await _unitOfWork.ExecuteInTransaction(async () =>
            {
                try
                {
                    var orderDetailRepository = Resolve<IGenericRepository<OrderDetail>>();
                    var orderDetailDb = await orderDetailRepository.GetById(createRatingRequestDto.OrderDetailId);
                    if (orderDetailDb == null)
                    {
                        throw new Exception(
                            $"Không tìm thấy chi tiết đơn hàng với id {createRatingRequestDto.OrderDetailId}");
                    }

                    orderDetailDb.IsRated = true;

                    var newRating = new Rating
                    {
                        RatingId = Guid.NewGuid(),
                        Content = createRatingRequestDto.Content,
                        CreateBy = createRatingRequestDto.AccountId,
                        CreateDate = utility!.GetCurrentDateTimeInTimeZone(),
                        PointId = createRatingRequestDto.PointId,
                        Title = createRatingRequestDto!.Title!,
                        OrderDetailId = createRatingRequestDto.OrderDetailId,
                    };

                    if (createRatingRequestDto.ImageFiles != null && createRatingRequestDto.ImageFiles.Count > 0)
                    {
                        foreach (var image in createRatingRequestDto.ImageFiles)
                        {
                            var pathName = SD.FirebasePathName.RATING_PREFIX +
                                           $"{newRating.RatingId}{Guid.NewGuid()}.jpg";
                            var upload = await firebaseService!.UploadFileToFirebase(image, pathName);

                            if (!upload.IsSuccess)
                            {
                                throw new Exception("Upload hình ảnh không thành công");
                            }

                            var newStaticFileDb = new Image
                            {
                                StaticFileId = Guid.NewGuid(),
                                RatingId = newRating.RatingId,
                                Path = upload.Result.ToString()
                            };
                            ;

                            listStaticFile.Add(newStaticFileDb);
                            await staticFileRepository!.InsertRange(listStaticFile);
                        }
                    }

                    if (!BuildAppActionResultIsError(result))
                    {
                        await orderDetailRepository.Update(orderDetailDb);
                        await _ratingRepository.Insert(newRating);
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

        public async Task<AppActionResult> DeleteRating(Guid ratingId)
        {
            var staticFileRepository = Resolve<IGenericRepository<Image>>();
            var result = new AppActionResult();
            try
            {
                var ratingDb = await _ratingRepository.GetById(ratingId);
                if (ratingDb == null)
                {
                             throw new Exception ($"Không tìm thấy đánh giá với id {ratingId}");
                }
                var ratingImage = await staticFileRepository.GetAllDataByExpression(p => p.RatingId == ratingId, 0, 0, null, false, null);
                if (ratingImage.Items.Count > 0 && ratingImage.Items != null)
                {
                    await staticFileRepository.DeleteRange(ratingImage.Items);
                }
                await _ratingRepository.DeleteById(ratingId);
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }

        public async Task<AppActionResult> GetAllRatingOfDish(Guid dishId, RatingPoint? ratingPoint, int pageNumber, int pageSize)
        {
            var result = new AppActionResult();
            try
            {
                if (ratingPoint != null)
                {
                    result.Result = await _ratingRepository.GetAllDataByExpression(p => p.OrderDetail!.DishSizeDetail!.DishId == dishId && p.PointId == ratingPoint.Value, pageNumber, pageSize, null, false, p => p.OrderDetail!.DishSizeDetail!.Dish!);
                }
                else
                {
                    result.Result = await _ratingRepository.GetAllDataByExpression(p => p.OrderDetail!.DishSizeDetail!.DishId == dishId, pageNumber, pageSize, null, false, p => p.OrderDetail!.DishSizeDetail!.Dish!);
                }
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }

        public async Task<AppActionResult> GetRatingById(Guid ratingId)
        {
            var result = new AppActionResult();
            try
            {
                result.Result = await _ratingRepository.GetAllDataByExpression(p => p.RatingId == ratingId, 0, 0, null, false, p => p.OrderDetail!.DishSizeDetail!.Dish!);
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }

        public async Task<AppActionResult> UpdateRating(UpdateRatingRequestDto updateRatingRequestDto)
        {
            var utility = Resolve<Utility>();
            var result = new AppActionResult();
            var firebaseService = Resolve<IFirebaseService>();
            var staticFileRepository = Resolve<IGenericRepository<Image>>();
            var listStaticFile = new List<Image>();

            await _unitOfWork.ExecuteInTransaction(async () =>
            {
                try
                {
                    var ratingDb = await _ratingRepository.GetById(updateRatingRequestDto.RatingId);
                    if (ratingDb == null)
                    {
                        throw new Exception($"Không tìm thấy đánh giá với id {updateRatingRequestDto.RatingId}");
                    }

                    ratingDb.Title = updateRatingRequestDto.Title;
                    ratingDb.PointId = updateRatingRequestDto.PointId;
                    ratingDb.Content = updateRatingRequestDto.Content;
                    ratingDb.UpdateDate = utility.GetCurrentDateInTimeZone();

                    if (updateRatingRequestDto.ImageFiles != null && updateRatingRequestDto.ImageFiles.Count > 0)
                    {
                        var oldImageListDb =
                            await staticFileRepository!.GetAllDataByExpression(
                                p => p.RatingId == updateRatingRequestDto.RatingId, 0, 0, null, false, null);
                        var oldImageList = oldImageListDb.Items;
                        if (oldImageList!.Count > 0 && oldImageList != null)
                        {
                            foreach (var oldImageDelete in oldImageList)
                            {
                                await firebaseService!.DeleteFileFromFirebase(oldImageDelete.Path);
                            }

                            await staticFileRepository.DeleteRange(oldImageList);
                        }

                        foreach (var newImage in updateRatingRequestDto.ImageFiles)
                        {
                            var pathName = SD.FirebasePathName.RATING_PREFIX +
                                           $"{ratingDb.RatingId}{Guid.NewGuid()}.jpg";
                            var upload = await firebaseService!.UploadFileToFirebase(newImage, pathName);

                            if (!upload.IsSuccess)
                            {
                                throw new Exception("Upload hình ảnh không thành công");
                            }

                            var newStaticFileDb = new Image
                            {
                                StaticFileId = Guid.NewGuid(),
                                RatingId = ratingDb.RatingId,
                                Path = upload.Result.ToString()
                            };
                            ;

                            listStaticFile.Add(newStaticFileDb);
                            await staticFileRepository!.InsertRange(listStaticFile);
                        }
                    }

                    if (!BuildAppActionResultIsError(result))
                    {
                        await _ratingRepository.Update(ratingDb);
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
    }
}
