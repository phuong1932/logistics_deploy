using System;

namespace LogisticsWebApp.DTO
{
    public class UserRoleModel
    {
        public int UserId { get; set; }
        public int RoleId { get; set; }
        public int? ShipperId { get; set; }
        public string? Description { get; set; }
        
    }
}