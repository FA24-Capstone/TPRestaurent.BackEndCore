namespace TPRestaurent.BackEndCore.Common.DTO.Request
{
    public class ConfigurationVersionDto
    {
        public string ActiveValue { get; set; } = null!;
        public DateTime ActiveDate { get; set; }
        public Guid ConfigurationId { get; set; }
    }
}