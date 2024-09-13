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
                .ForMember(desc => desc.IsVerified, act => act.MapFrom(src => src.IsVerified))
                .ForMember(desc => desc.FirstName, act => act.MapFrom(src => src.FirstName))
                .ForMember(desc => desc.LastName, act => act.MapFrom(src => src.LastName))
                .ForMember(desc => desc.PhoneNumber, act => act.MapFrom(src => src.PhoneNumber))
                .ForMember(desc => desc.UserName, act => act.MapFrom(src => src.UserName))
                .ForMember(desc => desc.Avatar, act => act.MapFrom(src => src.Avatar))
                .ForMember(desc => desc.CustomerInfo, act => act.MapFrom(src => src.Customer))
                .ForMember(desc => desc.IsManuallyUpdate, act => act.MapFrom(src => src.IsManuallyCreated))
                ;

            config.CreateMap<Device, DeviceResponse>()
            .ForMember(desc => desc.DeviceId, act => act.MapFrom(src => src.DeviceId))
            .ForMember(desc => desc.DeviceCode, act => act.MapFrom(src => src.DeviceCode))
            .ForMember(desc => desc.DevicePassword, act => act.MapFrom(src => src.DevicePassword))
            .ForMember(desc => desc.TableId, act => act.MapFrom(src => src.TableId))
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
             .ReverseMap();
            ;


            config.CreateMap<OrderDetailsDto, OrderDetail>()
            .ForMember(dest => dest.ComboId, opt => opt.MapFrom(src => src.Combo != null ? src.Combo.ComboId : (Guid?)null))
            .ForMember(dest => dest.OrderTime, opt => opt.MapFrom(src => DateTime.Now))
            .ForMember(dest => dest.OrderDetailStatusId, opt => opt.MapFrom(src => OrderDetailStatus.Pending)) // Assuming a default status
           .ReverseMap();
            ;


            config.CreateMap<ConfigurationDto, Configuration>()
            .ForMember(desc => desc.Name, act => act.MapFrom(src => src.Name))
            .ForMember(desc => desc.CurrentValue, act => act.MapFrom(src => src.CurrentValue))
            .ReverseMap();
            ;

        });
        // Trong class MappingConfig

        return mappingConfig;
    }
}