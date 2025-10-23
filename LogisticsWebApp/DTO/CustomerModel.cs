namespace LogisticsWebApp.DTO
{
    public class CustomerModel
    {
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public string? PersonInCharge { get; set; }
        public bool IsSelected { get; set; } = false;
    }
}

