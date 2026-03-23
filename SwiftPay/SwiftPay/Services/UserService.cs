using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using SwiftPay.Services.Interfaces;
using SwiftPay.Repositories.Interfaces;
using SwiftPay.DTOs.UserCustomerDTO;
using SwiftPay.Constants.Enums;
using SwiftPay.Models;

namespace SwiftPay.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _repo;
        private readonly IMapper _mapper;
        private readonly IAuditLogService _auditLogService;

        public UserService(IUserRepository repo, IMapper mapper, IAuditLogService auditLogService)
        {
            _repo = repo;
            _mapper = mapper;
            _auditLogService = auditLogService;
        }

        public async Task<UserResponseDto> CreateAsync(CreateUserDto dto)
        {
            // Check if email already exists - BUSINESS LOGIC
            var existingEmail = await _repo.GetByEmailAsync(dto.Email);
            if (existingEmail != null)
                throw new InvalidOperationException($"Email '{dto.Email}' is already registered. Please use a different email address.");

            // Use AutoMapper to map DTO to entity
            var entity = _mapper.Map<User>(dto);

            // Hash password before persisting
            if (!string.IsNullOrEmpty(dto.Password))
            {
                // Use BCrypt Enhanced hashing on a background thread to avoid blocking
                entity.PasswordHash = await Task.Run(() => BCrypt.Net.BCrypt.EnhancedHashPassword(dto.Password));
                entity.Password = null; // clear plain password
            }

            var created = await _repo.CreateAsync(entity);
            // Emit audit log for create
            try
            {
                await _auditLogService.CreateAsync(new DTOs.UserCustomerDTO.CreateAuditLogDto
                {
                    UserID = created.UserId,
                    Action = "User.Create",
                    Resource = "User",
                    Details = $"User {created.Email} created."
                });
            }
            catch { }
            return _mapper.Map<UserResponseDto>(created);
        }

        public async Task<UserResponseDto> GetByIdAsync(int userId)
        {
            var user = await _repo.GetByIdAsync(userId);
            return _mapper.Map<UserResponseDto>(user);
        }

        public async Task<UserResponseDto> GetByEmailAsync(string email)
        {
            var user = await _repo.GetByEmailAsync(email);
            return _mapper.Map<UserResponseDto>(user);
        }

        public async Task<IEnumerable<UserResponseDto>> GetAllAsync()
        {
            var users = await _repo.GetAllAsync();
            return _mapper.Map<List<UserResponseDto>>(users);
        }

        public async Task<UserResponseDto> UpdateAsync(int userId, UpdateUserDto dto)
        {
            var user = await _repo.GetByIdAsync(userId);
            if (user == null)
                throw new KeyNotFoundException($"User with ID {userId} not found.");

            // Check email uniqueness when updating email - BUSINESS LOGIC
            if (!string.IsNullOrEmpty(dto.Email) && dto.Email != user.Email)
            {
                var existingEmail = await _repo.GetByEmailAsync(dto.Email);
                if (existingEmail != null)
                    throw new InvalidOperationException($"Email '{dto.Email}' is already in use by another user.");
            }

            // Check phone uniqueness when updating phone - BUSINESS LOGIC
            if (!string.IsNullOrEmpty(dto.Phone) && dto.Phone != user.Phone)
            {
                var existingPhone = await _repo.GetByPhoneAsync(dto.Phone);
                if (existingPhone != null)
                    throw new InvalidOperationException($"Phone number '{dto.Phone}' is already in use by another user.");
            }

            // Use AutoMapper to map only non-null fields
            _mapper.Map(dto, user);

            var updated = await _repo.UpdateAsync(user);
            // Emit audit log for update
            try
            {
                await _auditLogService.CreateAsync(new DTOs.UserCustomerDTO.CreateAuditLogDto
                {
                    UserID = updated.UserId,
                    Action = "User.Update",
                    Resource = "User",
                    Details = $"User {updated.Email} updated."
                });
            }
            catch { }
            return _mapper.Map<UserResponseDto>(updated);
        }

        public async Task<bool> DeleteAsync(int userId)
        {
            var result = await _repo.DeleteAsync(userId);
            if (result)
            {
                try
                {
                    await _auditLogService.CreateAsync(new DTOs.UserCustomerDTO.CreateAuditLogDto
                    {
                        UserID = userId,
                        Action = "User.Delete",
                        Resource = "User",
                        Details = $"User {userId} deleted."
                    });
                }
                catch { }
            }
            return result;
        }
    }
}
