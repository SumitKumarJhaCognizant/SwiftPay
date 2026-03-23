using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using SwiftPay.Services.Interfaces;
using SwiftPay.Repositories.Interfaces;
using SwiftPay.DTOs.UserCustomerDTO;
using SwiftPay.Constants.Enums;
using SwiftPay.Domain.Remittance.Entities;
using SwiftPay.Models;

namespace SwiftPay.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository _repo;
        private readonly IUserRepository _userRepo;
        private readonly IMapper _mapper;
        private readonly IAuditLogService _auditLogService;

        public CustomerService(ICustomerRepository repo, IUserRepository userRepo, IMapper mapper, IAuditLogService auditLogService)
        {
            _repo = repo;
            _userRepo = userRepo;
            _mapper = mapper;
            _auditLogService = auditLogService;
        }

        public async Task<CustomerResponseDto> CreateAsync(CreateCustomerDto dto)
        {
            // Validate that User exists - BUSINESS LOGIC
            var user = await _userRepo.GetByIdAsync(dto.UserID);
            if (user == null)
                throw new KeyNotFoundException($"User with ID {dto.UserID} does not exist.");

            // Check if customer already exists for this user - BUSINESS LOGIC
            var existingCustomer = await _repo.GetByUserIdAsync(dto.UserID);
            if (existingCustomer != null)
                throw new InvalidOperationException($"A customer profile already exists for User ID {dto.UserID}.");

            // Use AutoMapper to map DTO to entity
            var entity = _mapper.Map<CustomerProfile>(dto);

            var created = await _repo.CreateAsync(entity);
            try
            {
                await _auditLogService.CreateAsync(new DTOs.UserCustomerDTO.CreateAuditLogDto
                {
                    UserID = created.UserID,
                    Action = "Customer.Create",
                    Resource = "Customer",
                    Details = $"Customer {created.CustomerID} created for user {created.UserID}."
                });
            }
            catch { }
            return _mapper.Map<CustomerResponseDto>(created);
        }

        public async Task<CustomerResponseDto> GetByIdAsync(int customerId)
        {
            var customer = await _repo.GetByIdAsync(customerId);
            return _mapper.Map<CustomerResponseDto>(customer);
        }

        public async Task<CustomerResponseDto> GetByUserIdAsync(int userId)
        {
            var customer = await _repo.GetByUserIdAsync(userId);
            return _mapper.Map<CustomerResponseDto>(customer);
        }

        public async Task<IEnumerable<CustomerResponseDto>> GetAllAsync()
        {
            var customers = await _repo.GetAllAsync();
            return _mapper.Map<List<CustomerResponseDto>>(customers);
        }

        public async Task<CustomerResponseDto> UpdateAsync(int customerId, UpdateCustomerDto dto)
        {
            var customer = await _repo.GetByIdAsync(customerId);
            if (customer == null)
                throw new KeyNotFoundException($"Customer with ID {customerId} not found.");

            // Use AutoMapper to map only non-null fields
            _mapper.Map(dto, customer);

            var updated = await _repo.UpdateAsync(customer);
            try
            {
                await _auditLogService.CreateAsync(new DTOs.UserCustomerDTO.CreateAuditLogDto
                {
                    UserID = updated.UserID,
                    Action = "Customer.Update",
                    Resource = "Customer",
                    Details = $"Customer {updated.CustomerID} updated for user {updated.UserID}."
                });
            }
            catch { }
            return _mapper.Map<CustomerResponseDto>(updated);
        }

        public async Task<CustomerResponseDto> UpdateRiskRatingAsync(int customerId, UpdateCustomerRiskRatingDto dto)
        {
            var customer = await _repo.GetByIdAsync(customerId);
            if (customer == null)
                throw new KeyNotFoundException($"Customer with ID {customerId} not found.");

            // Business logic: Update RiskRating
            customer.RiskRating = dto.RiskRating;

            var updated = await _repo.UpdateAsync(customer);
            try
            {
                await _auditLogService.CreateAsync(new DTOs.UserCustomerDTO.CreateAuditLogDto
                {
                    UserID = updated.UserID,
                    Action = "Customer.UpdateRiskRating",
                    Resource = "Customer",
                    Details = $"Customer {updated.CustomerID} risk rating set to {updated.RiskRating}."
                });
            }
            catch { }
            return _mapper.Map<CustomerResponseDto>(updated);
        }

        public async Task<bool> DeleteAsync(int customerId)
        {
            var result = await _repo.DeleteAsync(customerId);
            if (result)
            {
                try
                {
                    await _auditLogService.CreateAsync(new DTOs.UserCustomerDTO.CreateAuditLogDto
                    {
                        UserID = customerId,
                        Action = "Customer.Delete",
                        Resource = "Customer",
                        Details = $"Customer {customerId} deleted."
                    });
                }
                catch { }
            }
            return result;
        }
    }
}
