namespace SwiftPay.DTOs.UserCustomerDTO
{
    public class LoginResponseDto
    {
        public string Token { get; set; }

        public AuthResponseDto User { get; set; }
    }
}
