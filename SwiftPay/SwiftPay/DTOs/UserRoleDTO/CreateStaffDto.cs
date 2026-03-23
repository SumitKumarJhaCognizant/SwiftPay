using System.ComponentModel.DataAnnotations;

namespace SwiftPay.DTOs.UserRoleDTO
{
    public class CreateStaffDto
    {
        [Required]
        [StringLength(255, MinimumLength = 2)]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(255)]
        public string Email { get; set; }

        [Required]
        [Phone]
        [StringLength(20)]
        public string Phone { get; set; }

        [Required]
        [StringLength(512, MinimumLength = 6)]
        public string Password { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int RoleId { get; set; }
    }
}
