namespace LogisticsWebApp.DTO
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public List<T>? Data { get; set; }
        public string? Message { get; set; }
    }
}

