namespace logistic_web.api.DTO
{
    public class UpdateUserRoleRequest
    {
        public int RoleId { get; set; }
        public int? ShipperId { get; set; }
    }
}