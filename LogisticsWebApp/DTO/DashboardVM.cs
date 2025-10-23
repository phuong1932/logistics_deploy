namespace LogisticsWebApp.DTO
{
    public class DashboardVM
    {  public int CargoCount { get; set; }
        public decimal TotalRevenue { get; set; }
        public string TotalRevenueFormatted { get; set; } = "0 VND";
        public decimal AverageRevenuePerCargo { get; set; }
        public string AverageRevenueFormatted { get; set; } = "0 VND";
        public int Month { get; set; }
        public int Year { get; set; }
        public string MonthName { get; set; } = string.Empty;
        public string StartDate { get; set; } = string.Empty;
        public string EndDate { get; set; } = string.Empty;

    }
 
}