﻿using AutoMapper;
using Castle.Core.Resource;
using TPRestaurent.BackEndCore.Common.DTO.Request;
using TPRestaurent.BackEndCore.Common.DTO.Response;
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
                ;

            config.CreateMap<Dish, DishDto>()
             .ForMember(desc => desc.Name, act => act.MapFrom(src => src.Name))
             .ForMember(desc => desc.Description, act => act.MapFrom(src => src.Description))
             //.ForMember(desc => desc.Price, act => act.MapFrom(src => src.Price))
             //.ForMember(desc => desc.Discount, act => act.MapFrom(src => src.Discount))
             .ForMember(desc => desc.isAvailable, act => act.MapFrom(src => src.isAvailable))
             .ReverseMap();
            ;

            config.CreateMap<Table, TableDto>()
             .ForMember(desc => desc.TableName, act => act.MapFrom(src => src.TableName))
             .ForMember(desc => desc.TableSizeId, act => act.MapFrom(src => src.TableSizeId))
             .ForMember(desc => desc.TableRatingId, act => act.MapFrom(src => src.TableRatingId))
             .ReverseMap();
            ;

            config.CreateMap<ReservationRequestDto, ReservationRequest>()
             .ForMember(desc => desc.ReservationDate, act => act.MapFrom(src => src.ReservationDate))
             .ForMember(desc => desc.NumberOfPeople, act => act.MapFrom(src => src.NumberOfPeople))
             .ForMember(desc => desc.EndTime, act => act.MapFrom(src => src.EndTime))
             .ForMember(desc => desc.CreateBy, act => act.MapFrom(src => src.CustomerAccountId))
             .ForMember(desc => desc.Note, act => act.MapFrom(src => src.Note))
             .ReverseMap();
            ;

            config.CreateMap<Reservation, ReservationDto>()
             .ForMember(desc => desc.ReservationDate, act => act.MapFrom(src => src.ReservationDate))
             .ForMember(desc => desc.NumberOfPeople, act => act.MapFrom(src => src.NumberOfPeople))
             .ForMember(desc => desc.EndTime, act => act.MapFrom(src => src.EndTime))
             .ForMember(desc => desc.CustomerAccountId, act => act.MapFrom(src => src.CustomerAccountId))
             .ForMember(desc => desc.Deposit, act => act.MapFrom(src => src.Deposit))
             .ReverseMap();
            ;

            config.CreateMap<ReservationDish, ReservationDishDto>()
            .ForMember(desc => desc.ComboId, act => act.MapFrom(src => src.ComboId))
            .ForMember(desc => desc.DishSizeDetailId, act => act.MapFrom(src => src.DishSizeDetailId))
            .ForMember(desc => desc.Quantity, act => act.MapFrom(src => src.Quantity))
            .ForMember(desc => desc.Note, act => act.MapFrom(src => src.Note))
            .ReverseMap();
            ;

            config.CreateMap<OrderDetail, OrderDetailsDto>()
           .ForMember(desc => desc.ComboId, act => act.MapFrom(src => src.ComboId))
           .ForMember(desc => desc.DishSizeDetailId, act => act.MapFrom(src => src.DishSizeDetailId))
           .ForMember(desc => desc.Quantity, act => act.MapFrom(src => src.Quantity))
           .ForMember(desc => desc.Note, act => act.MapFrom(src => src.Note))
           .ReverseMap();
            ;
        });
        // Trong class MappingConfig

        return mappingConfig;
    }
}