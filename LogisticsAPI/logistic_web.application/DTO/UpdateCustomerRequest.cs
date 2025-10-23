namespace logistic_web.application.DTO
{
    public class UpdateCustomerRequest
    {
        public string CustomerName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public string? PersonInCharge { get; set; }
    }
}
