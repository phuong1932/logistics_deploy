namespace LogisticsWebApp.DTO
{
    public class UserModel
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
        public bool IsSelected { get; set; }
    }

    public class UserApiResponse
    {
        public bool Success { get; set; }
        public List<UserModel> Data { get; set; } = new List<UserModel>();
        public string? Message { get; set; }
    }
}

