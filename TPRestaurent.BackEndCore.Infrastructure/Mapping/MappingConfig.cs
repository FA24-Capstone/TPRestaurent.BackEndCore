using AutoMapper;

namespace TPRestaurent.BackEndCore.Infrastructure.Mapping;

public class MappingConfig
{
    public static MapperConfiguration RegisterMap()
    {
        var mappingConfig = new MapperConfiguration(config =>
        {
        });
        // Trong class MappingConfig

        return mappingConfig;
    }
}