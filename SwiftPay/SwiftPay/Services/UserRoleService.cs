using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using SwiftPay.Services.Interfaces;
using SwiftPay.Repositories.Interfaces;
using SwiftPay.Models;
using SwiftPay.DTOs.UserRoleDTO;

namespace SwiftPay.Services
{
    public class UserRoleService : IUserRoleService
    {
        private readonly IUserRoleRepository _userRoleRepo;
        private readonly IUserRepository _userRepo;
        private readonly IRoleRepository _roleRepo;
        private readonly IMapper _mapper;
        private readonly IAuditLogService _auditLogService;

        public UserRoleService(IUserRoleRepository userRoleRepo, IUserRepository userRepo, IRoleRepository roleRepo, IMapper mapper, IAuditLogService auditLogService)
        {
            _userRoleRepo = userRoleRepo;
            _userRepo = userRepo;
            _roleRepo = roleRepo;
            _mapper = mapper;
            _auditLogService = auditLogService;
        }

        public async Task<UserRoleResponseDto> AssignRoleToUserAsync(int userId, CreateUserRoleRequestDto dto)
        {
            var user = await _userRepo.GetByIdAsync(userId);
            if (user == null)
                throw new KeyNotFoundException($"User with ID {userId} does not exist.");

            var role = await _roleRepo.GetByIdAsync(dto.RoleId);
            if (role == null)
                throw new KeyNotFoundException($"Role with ID {dto.RoleId} does not exist.");

            var existingUserRole = await _userRoleRepo.GetUserRoleAsync(userId, dto.RoleId);
            if (existingUserRole != null)
                throw new InvalidOperationException($"User with ID {userId} already has the role with ID {dto.RoleId}.");

            var userRole = _mapper.Map<Models.UserRole>((userId, dto));
            var assigned = await _userRoleRepo.CreateAsync(userRole);

            // Emit audit log
            try
            {
                await _auditLogService.CreateAsync(new DTOs.UserCustomerDTO.CreateAuditLogDto
                {
                    UserID = userId,
                    Action = "UserRole.Assign",
                    Resource = "UserRole",
                    Details = $"Assigned role {role.RoleType} (id={role.RoleId}) to user {userId}."
                });
            }
            catch { }

            return _mapper.Map<UserRoleResponseDto>(assigned);
        }

        public async Task<UserRoleResponseDto> GetByIdAsync(int userRoleId)
        {
            var ur = await _userRoleRepo.GetByIdAsync(userRoleId);
            return _mapper.Map<UserRoleResponseDto>(ur);
        }

        public async Task<IEnumerable<UserRoleResponseDto>> GetByUserIdAsync(int userId)
        {
            var list = await _userRoleRepo.GetByUserIdAsync(userId);
            return _mapper.Map<List<UserRoleResponseDto>>(list);
        }

        public async Task<IEnumerable<UserRoleResponseDto>> GetByRoleIdAsync(int roleId)
        {
            var list = await _userRoleRepo.GetByRoleIdAsync(roleId);
            return _mapper.Map<List<UserRoleResponseDto>>(list);
        }

        public async Task<bool> RemoveRoleFromUserAsync(int userRoleId)
        {
            var ur = await _userRoleRepo.GetByIdAsync(userRoleId);
            if (ur == null) return false;

            var result = await _userRoleRepo.DeleteAsync(userRoleId);
            if (result)
            {
                try
                {
                    await _auditLogService.CreateAsync(new DTOs.UserCustomerDTO.CreateAuditLogDto
                    {
                        UserID = ur.UserId,
                        Action = "UserRole.Remove",
                        Resource = "UserRole",
                        Details = $"Removed role {ur.RoleId} from user {ur.UserId}."
                    });
                }
                catch { }
            }

            return result;
        }
    }
}
