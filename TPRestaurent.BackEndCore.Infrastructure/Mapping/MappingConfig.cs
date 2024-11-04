using AutoMapper;
using Castle.Core.Resource;
using TPRestaurent.BackEndCore.Common.DTO.Request;
using TPRestaurent.BackEndCore.Common.DTO.Response;
using TPRestaurent.BackEndCore.Domain.Enums;
using TPRestaurent.BackEndCore.Domain.Models;

namespace TPRestaurent.BackEndCore.Infrastructure.Mapping;

public class MappingConfig
{
    public static MapperConfiguration RegisterMap()
    {
        var mappingConfig = new MapperConfiguration(config =>
        {

            config.CreateMap<Account, AccountResponse>()
                .ForMember(desc => desc.Id, act => act.MapFrom(src => src.Id))
                .ForMember(desc => desc.Email, act => act.MapFrom(src => src.Email))
                .ForMember(desc => desc.Gender, act => act.MapFrom(src => src.Gender))
                .ForMember(desc => desc.DOB, act => act.MapFrom(src => src.DOB))
                .ForMember(desc => desc.IsVerified, act => act.MapFrom(src => src.IsVerified))
                .ForMember(desc => desc.FirstName, act => act.MapFrom(src => src.FirstName))
                .ForMember(desc => desc.LastName, act => act.MapFrom(src => src.LastName))
                .ForMember(desc => desc.PhoneNumber, act => act.MapFrom(src => src.PhoneNumber))
                .ForMember(desc => desc.UserName, act => act.MapFrom(src => src.UserName))
                .ForMember(desc => desc.Avatar, act => act.MapFrom(src => src.Avatar))
                .ForMember(desc => desc.LoyalPoint, act => act.MapFrom(src => src.LoyaltyPoint))
                .ForMember(desc => desc.IsManuallyCreated, act => act.MapFrom(src => src.IsManuallyCreated))
                .ForMember(desc => desc.IsDeleted, act => act.MapFrom(src => src.IsDeleted))
                .ForMember(desc => desc.IsDelivering, act => act.MapFrom(src => src.IsDelivering))
                .ForMember(desc => desc.StoreCreditExpireDay, act => act.MapFrom(src => src.ExpiredDate))
                .ForMember(desc => desc.Amount, act => act.MapFrom(src => src.StoreCreditAmount))
                ;

            config.CreateMap<Table, DeviceResponse>()
            .ForMember(desc => desc.DeviceCode, act => act.MapFrom(src => src.DeviceCode))
            .ForMember(desc => desc.DevicePassword, act => act.MapFrom(src => src.DevicePassword))
            .ForMember(desc => desc.TableId, act => act.MapFrom(src => src.TableId))
            .ForMember(desc => desc.TableName, act => act.MapFrom(src => src.TableName))
            .ReverseMap()   
            ;

            config.CreateMap<Dish, DishDto>()
             .ForMember(desc => desc.Name, act => act.MapFrom(src => src.Name))
             .ForMember(desc => desc.Description, act => act.MapFrom(src => src.Description))
             //.ForMember(desc => desc.Price, act => act.MapFrom(src => src.Price))
             //.ForMember(desc => desc.Discount, act => act.MapFrom(src => src.Discount))
             .ReverseMap();
            ;

            config.CreateMap<Table, TableDto>()
             .ForMember(desc => desc.TableName, act => act.MapFrom(src => src.TableName))
             .ForMember(desc => desc.TableSizeId, act => act.MapFrom(src => src.TableSizeId))
             .ForMember(desc => desc.TableRatingId, act => act.MapFrom(src => src.RoomId))
             .ForMember(desc => desc.DeviceCode, act => act.MapFrom(src => src.DeviceCode))
            .ForMember(desc => desc.DevicePassword, act => act.MapFrom(src => src.DevicePassword))
             .ReverseMap();
            ;


            config.CreateMap<OrderDetailsDto, OrderDetail>()
            .ForMember(dest => dest.ComboId, opt => opt.MapFrom(src => src.Combo != null ? src.Combo.ComboId : (Guid?)null))
            .ForMember(dest => dest.OrderTime, opt => opt.MapFrom(src => DateTime.Now))
            .ForMember(dest => dest.OrderDetailStatusId, opt => opt.MapFrom(src => OrderDetailStatus.Unchecked)) // Assuming a default status
           .ReverseMap();
            ;


            config.CreateMap<ConfigurationDto, Configuration>()
            .ForMember(desc => desc.Name, act => act.MapFrom(src => src.Name))
            .ForMember(desc => desc.CurrentValue, act => act.MapFrom(src => src.CurrentValue))
            .ReverseMap();
            ;

            config.CreateMap<Combo, ComboResponseDto>()
            .ForMember(dest => dest.ComboId, act => act.MapFrom(src => src.ComboId))
            .ForMember(dest => dest.Name, act => act.MapFrom(src => src.Name))
            .ForMember(dest => dest.Description, act => act.MapFrom(src => src.Description))
            .ForMember(dest => dest.Image, act => act.MapFrom(src => src.Image))
            .ForMember(dest => dest.Price, act => act.MapFrom(src => src.Price))
            .ForMember(dest => dest.Discount, act => act.MapFrom(src => src.Discount))
            .ForMember(dest => dest.CategoryId, act => act.MapFrom(src => src.CategoryId))
            .ForMember(dest => dest.Category, act => act.MapFrom(src => src.Category))
            .ForMember(dest => dest.StartDate, act => act.MapFrom(src => src.StartDate))
            .ForMember(dest => dest.EndDate, act => act.MapFrom(src => src.EndDate))
            .ForMember(dest => dest.IsAvailable, act => act.MapFrom(src => src.IsAvailable))
            .ForMember(dest => dest.IsDeleted, act => act.MapFrom(src => src.IsDeleted))
            .ForMember(dest => dest.IsDeleted, act => act.MapFrom(src => src.IsDeleted))
            .ForMember(dest => dest.IsDeleted, act => act.MapFrom(src => src.IsDeleted))
            .ForMember(dest => dest.PreparationTime, act => act.MapFrom(src => src.PreparationTime));

            config.CreateMap<Dish, DishReponse>()
            .ForMember(dest => dest.DishId, act => act.MapFrom(src => src.DishId))
            .ForMember(dest => dest.Name, act => act.MapFrom(src => src.Name))
            .ForMember(dest => dest.Description, act => act.MapFrom(src => src.Description))
            .ForMember(dest => dest.Image, act => act.MapFrom(src => src.Image))
            .ForMember(dest => dest.DishItemTypeId, act => act.MapFrom(src => src.DishItemTypeId))
            .ForMember(dest => dest.DishItemType, act => act.MapFrom(src => src.DishItemType))
            .ForMember(dest => dest.IsAvailable, act => act.MapFrom(src => src.isAvailable))
            .ForMember(dest => dest.IsDeleted, act => act.MapFrom(src => src.IsDeleted))
            .ForMember(dest => dest.IsMainItem, act => act.MapFrom(src => src.IsMainItem))
            .ForMember(dest => dest.PreparationTime, act => act.MapFrom(src => src.PreparationTime))
            ;

            config.CreateMap<Dish, DishQuantityResponse>()
            .ForMember(dest => dest.DishId, act => act.MapFrom(src => src.DishId))
            .ForMember(dest => dest.Name, act => act.MapFrom(src => src.Name))
            .ForMember(dest => dest.Description, act => act.MapFrom(src => src.Description))
            .ForMember(dest => dest.Image, act => act.MapFrom(src => src.Image))
            .ForMember(dest => dest.DishItemTypeId, act => act.MapFrom(src => src.DishItemTypeId))
            .ForMember(dest => dest.DishItemType, act => act.MapFrom(src => src.DishItemType))
            .ForMember(dest => dest.IsAvailable, act => act.MapFrom(src => src.isAvailable))
            .ForMember(dest => dest.IsDeleted, act => act.MapFrom(src => src.IsDeleted))
            .ForMember(dest => dest.IsMainItem, act => act.MapFrom(src => src.IsMainItem))
            .ForMember(dest => dest.PreparationTime, act => act.MapFrom(src => src.PreparationTime))
;


            config.CreateMap<Order, OrderResponse>()
            .ForMember(dest => dest.OrderId, act => act.MapFrom(src => src.OrderId))
            .ForMember(dest => dest.OrderDate, act => act.MapFrom(src => src.OrderDate))
            .ForMember(dest => dest.AssignedTime, act => act.MapFrom(src => src.AssignedTime))
            .ForMember(dest => dest.StartDeliveringTime, act => act.MapFrom(src => src.StartDeliveringTime))
            .ForMember(dest => dest.DeliveredTime, act => act.MapFrom(src => src.DeliveredTime))
            .ForMember(dest => dest.ReservationDate, act => act.MapFrom(src => src.ReservationDate))
            .ForMember(dest => dest.MealTime, act => act.MapFrom(src => src.MealTime))
            .ForMember(dest => dest.EndTime, act => act.MapFrom(src => src.EndTime))
            .ForMember(dest => dest.CancelledTime, act => act.MapFrom(src => src.CancelledTime))
            .ForMember(dest => dest.TotalAmount, act => act.MapFrom(src => src.TotalAmount))
            .ForMember(dest => dest.StatusId, act => act.MapFrom(src => src.StatusId))
            .ForMember(dest => dest.Status, act => act.MapFrom(src => src.Status))
            .ForMember(dest => dest.AccountId, act => act.MapFrom(src => src.AccountId))
            .ForMember(dest => dest.Account, act => act.MapFrom(src => src.Account))
            .ForMember(dest => dest.AddressId, act => act.MapFrom(src => src.AddressId))
            .ForMember(dest => dest.CustomerInfoAddress, act => act.MapFrom(src => src.CustomerInfoAddress))
            .ForMember(dest => dest.LoyalPointsHistoryId, act => act.MapFrom(src => src.LoyalPointsHistoryId))
            .ForMember(dest => dest.LoyalPointsHistory, act => act.MapFrom(src => src.LoyalPointsHistory))
            .ForMember(dest => dest.Note, act => act.MapFrom(src => src.Note))
            .ForMember(dest => dest.OrderTypeId, act => act.MapFrom(src => src.OrderTypeId))
            .ForMember(dest => dest.OrderType, act => act.MapFrom(src => src.OrderType))
            .ForMember(dest => dest.NumOfPeople, act => act.MapFrom(src => src.NumOfPeople))
            .ForMember(dest => dest.Deposit, act => act.MapFrom(src => src.Deposit))
            .ForMember(dest => dest.ValidatingImg, act => act.MapFrom(src => src.ValidatingImg))
            .ForMember(dest => dest.Shipper, act => act.MapFrom(src => src.Shipper))
            .ForMember(dest => dest.CancelDeliveryReason, act => act.MapFrom(src => src.CancelDeliveryReason))
            .ForMember(dest => dest.IsPrivate, act => act.MapFrom(src => src.IsPrivate));

            config.CreateMap<Order, OrderWithFirstDetailResponse>()
            .ForMember(dest => dest.OrderId, act => act.MapFrom(src => src.OrderId))
            .ForMember(dest => dest.OrderDate, act => act.MapFrom(src => src.OrderDate))
            .ForMember(dest => dest.AssignedTime, act => act.MapFrom(src => src.AssignedTime))
            .ForMember(dest => dest.StartDeliveringTime, act => act.MapFrom(src => src.StartDeliveringTime))
            .ForMember(dest => dest.ReservationDate, act => act.MapFrom(src => src.ReservationDate))
            .ForMember(dest => dest.DeliveredTime, act => act.MapFrom(src => src.DeliveredTime))
            .ForMember(dest => dest.CancelledTime, act => act.MapFrom(src => src.CancelledTime))
            .ForMember(dest => dest.MealTime, act => act.MapFrom(src => src.MealTime))
            .ForMember(dest => dest.EndTime, act => act.MapFrom(src => src.EndTime))
            .ForMember(dest => dest.Shipper, act => act.MapFrom(src => src.Shipper))
            .ForMember(dest => dest.TotalAmount, act => act.MapFrom(src => src.TotalAmount))
            .ForMember(dest => dest.CashReceived, act => act.MapFrom(src => src.CashReceived))
            .ForMember(dest => dest.ChangeReturned, act => act.MapFrom(src => src.ChangeReturned))
            .ForMember(dest => dest.StatusId, act => act.MapFrom(src => src.StatusId))
            .ForMember(dest => dest.Status, act => act.MapFrom(src => src.Status))
            .ForMember(dest => dest.AccountId, act => act.MapFrom(src => src.AccountId))
            .ForMember(dest => dest.Account, act => act.MapFrom(src => src.Account))
            .ForMember(dest => dest.AddressId, act => act.MapFrom(src => src.AddressId))
            .ForMember(dest => dest.CustomerInfoAddress, act => act.MapFrom(src => src.CustomerInfoAddress))
            .ForMember(dest => dest.LoyalPointsHistoryId, act => act.MapFrom(src => src.LoyalPointsHistoryId))
            .ForMember(dest => dest.LoyalPointsHistory, act => act.MapFrom(src => src.LoyalPointsHistory))
            .ForMember(dest => dest.Note, act => act.MapFrom(src => src.Note))
            .ForMember(dest => dest.OrderTypeId, act => act.MapFrom(src => src.OrderTypeId))
            .ForMember(dest => dest.OrderType, act => act.MapFrom(src => src.OrderType))
            .ForMember(dest => dest.NumOfPeople, act => act.MapFrom(src => src.NumOfPeople))
            .ForMember(dest => dest.Deposit, act => act.MapFrom(src => src.Deposit))
            .ForMember(dest => dest.IsPrivate, act => act.MapFrom(src => src.IsPrivate))
            .ForMember(dest => dest.ValidatingImg, act => act.MapFrom(src => src.ValidatingImg))
            .ForMember(dest => dest.CancelDeliveryReason, act => act.MapFrom(src => src.CancelDeliveryReason))
            .ReverseMap();

            config.CreateMap<Order, ReservationTableItemResponse>()
            .ForMember(dest => dest.OrderId, opt => opt.MapFrom(src => src.OrderId))
            .ForMember(dest => dest.ReservationDate, opt => opt.MapFrom(src => src.ReservationDate))
            .ForMember(dest => dest.OrderDate, opt => opt.MapFrom(src => src.OrderDate))
            .ForMember(dest => dest.MealTime, opt => opt.MapFrom(src => src.MealTime))
            .ForMember(dest => dest.AssignedTime, opt => opt.MapFrom(src => src.AssignedTime))
            .ForMember(dest => dest.StartDeliveringTime, opt => opt.MapFrom(src => src.StartDeliveringTime))
            .ForMember(dest => dest.DeliveredTime, opt => opt.MapFrom(src => src.DeliveredTime))
            .ForMember(dest => dest.CancelledTime, opt => opt.MapFrom(src => src.CancelledTime))
            .ForMember(dest => dest.EndTime, opt => opt.MapFrom(src => src.EndTime))
            .ForMember(dest => dest.TotalAmount, opt => opt.MapFrom(src => src.TotalAmount))
            .ForMember(dest => dest.StatusId, opt => opt.MapFrom(src => src.StatusId))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
            .ForMember(dest => dest.AccountId, opt => opt.MapFrom(src => src.AccountId))
            .ForMember(dest => dest.Account, opt => opt.MapFrom(src => src.Account))
            .ForMember(dest => dest.LoyalPointsHistoryId, opt => opt.MapFrom(src => src.LoyalPointsHistoryId))
            .ForMember(dest => dest.LoyalPointsHistory, opt => opt.MapFrom(src => src.LoyalPointsHistory))
            .ForMember(dest => dest.Note, opt => opt.MapFrom(src => src.Note))
            .ForMember(dest => dest.OrderTypeId, opt => opt.MapFrom(src => src.OrderTypeId))
            .ForMember(dest => dest.OrderType, opt => opt.MapFrom(src => src.OrderType))
            .ForMember(dest => dest.NumOfPeople, opt => opt.MapFrom(src => src.NumOfPeople))
            .ForMember(dest => dest.Deposit, opt => opt.MapFrom(src => src.Deposit))
            .ForMember(dest => dest.IsPrivate, opt => opt.MapFrom(src => src.IsPrivate))
            .ForMember(dest => dest.CancelDeliveryReason, act => act.MapFrom(src => src.CancelDeliveryReason));

            config.CreateMap<Table, TableArrangementResponseItem>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.TableId))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.TableName))
            .ForMember(dest => dest.TableSizeId, opt => opt.MapFrom(src => src.TableSizeId))
            .ForMember(dest => dest.TableStatusId, opt => opt.MapFrom(src => src.TableStatusId))
            .ForMember(dest => dest.RoomId, opt => opt.MapFrom(src => src.RoomId))
            .ForMember(dest => dest.Room, opt => opt.MapFrom(src => src.Room))
            .ReverseMap();
        });



        return mappingConfig;
    }
}