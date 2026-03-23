using System.Collections.Generic;
using System.Threading.Tasks;
using SwiftPay.Models;

namespace SwiftPay.Repositories.Interfaces
{
    public interface IRoleRepository
    {
        Task<Role> CreateAsync(Role entity);
        Task<Role> GetByIdAsync(int roleId);
        Task<Role> GetByRoleTypeAsync(SwiftPay.Constants.Enums.RoleType roleType);
        Task<IEnumerable<Role>> GetAllAsync();
        Task<Role> UpdateAsync(Role entity);
        Task<bool> DeleteAsync(int roleId);
    }
}
