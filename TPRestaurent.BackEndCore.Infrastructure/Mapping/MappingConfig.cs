using AutoMapper;
using Castle.Core.Resource;
using TPRestaurent.BackEndCore.Common.DTO.Request;
using TPRestaurent.BackEndCore.Domain.Models;

namespace TPRestaurent.BackEndCore.Infrastructure.Mapping;

public class MappingConfig
{
    public static MapperConfiguration RegisterMap()
    {
        var mappingConfig = new MapperConfiguration(config =>
        {
            config.CreateMap<Dish, DishDto>()
             .ForMember(desc => desc.Name, act => act.MapFrom(src => src.Name))
             .ForMember(desc => desc.Description, act => act.MapFrom(src => src.Description))
             .ForMember(desc => desc.Price, act => act.MapFrom(src => src.Price))
             .ForMember(desc => desc.Discount, act => act.MapFrom(src => src.Discount))
             .ForMember(desc => desc.isAvailable, act => act.MapFrom(src => src.isAvailable))
             .ReverseMap();
            ;
        });
        // Trong class MappingConfig

        return mappingConfig;
    }
}