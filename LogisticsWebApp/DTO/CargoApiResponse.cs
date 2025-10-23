namespace LogisticsWebApp.DTO
{
    public class CargoApiResponse
    {
        public bool Success { get; set; }
        public CargoModel? Data { get; set; }
        public string? Message { get; set; }
    }
}
