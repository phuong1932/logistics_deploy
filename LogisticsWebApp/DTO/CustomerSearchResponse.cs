using System.Collections.Generic;

namespace LogisticsWebApp.DTO
{
    public class CustomerSearchResponse
    {
        public bool Success { get; set; }
        public List<CustomerModel>? Data { get; set; }
        public string? Message { get; set; }
    }
}

