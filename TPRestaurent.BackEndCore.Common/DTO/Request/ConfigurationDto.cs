namespace TPRestaurent.BackEndCore.Common.DTO.Request
{
    public class ConfigurationDto
    {
        public string Name { get; set; } = null!;
        public string VietnameseName { get; set; } = null!;
        public string CurrentValue { get; set; } = null!;
        public string Unit { get; set; } = null!;
    }
}