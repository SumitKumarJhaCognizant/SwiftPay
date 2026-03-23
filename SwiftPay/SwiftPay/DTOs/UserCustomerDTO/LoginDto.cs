using System.ComponentModel.DataAnnotations;

namespace SwiftPay.DTOs.UserCustomerDTO
{
    public class LoginDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
