namespace logistic_web.application.DTO
{
    public class UserRoleDTO
    {
        public int UserId { get; set; }
        public int RoleId { get; set; }
        public int? ShipperId { get; set; }
        public string? Description { get; set; }
    }
}