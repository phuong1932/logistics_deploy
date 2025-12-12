namespace LogisticsWebApp.DTO
{
    public class TrackingModel
    {
        public long Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public DateTime DateCreated { get; set; }
        public string Ip { get; set; } = string.Empty;
    }
}

