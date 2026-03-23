using System.Threading.Tasks;
using SwiftPay.DTOs.UserCustomerDTO;

namespace SwiftPay.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterUserDto dto);

        Task<LoginResponseDto> LoginAsync(LoginDto dto);
    }
}
