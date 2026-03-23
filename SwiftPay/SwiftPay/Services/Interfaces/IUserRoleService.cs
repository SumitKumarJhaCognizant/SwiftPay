using System.Collections.Generic;
using System.Threading.Tasks;
using SwiftPay.DTOs.UserRoleDTO;
using SwiftPay.Models;

namespace SwiftPay.Services.Interfaces
{
    public interface IUserRoleService
    {
        Task<UserRoleResponseDto> AssignRoleToUserAsync(int userId, CreateUserRoleRequestDto dto);
        Task<UserRoleResponseDto> GetByIdAsync(int userRoleId);
        Task<IEnumerable<UserRoleResponseDto>> GetByUserIdAsync(int userId);
        Task<IEnumerable<UserRoleResponseDto>> GetByRoleIdAsync(int roleId);
        Task<bool> RemoveRoleFromUserAsync(int userRoleId);
    }
}
