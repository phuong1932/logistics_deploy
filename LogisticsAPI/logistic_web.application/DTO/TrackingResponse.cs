namespace logistic_web.application.DTO
{
    public class TrackingResponse
    {
        public long Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public DateTime DateCreated { get; set; }
        public string Ip { get; set; } = string.Empty;
    }
}

