using AutoMapper;
using SwiftPay.Constants.Enums;
using SwiftPay.Domain.Remittance.Entities;
using SwiftPay.DTOs.AmendmentDTO;
using SwiftPay.DTOs.UserCustomerDTO;
using SwiftPay.Repositories.Interfaces;
using SwiftPay.Services.Interfaces;
using System;
using System.Threading.Tasks;

namespace SwiftPay.Services
{
    public class AmendmentService : IAmendmentService
    {
        private readonly IAmendmentRepository _repo;
        private readonly IRemittanceRepository _remittanceRepo;
        private readonly ICustomerRepository _customerRepo;
        private readonly INotificationAlertService _notificationService;
        private readonly IMapper _mapper;

        public AmendmentService(
            IAmendmentRepository repo,
            IRemittanceRepository remittanceRepo,
            ICustomerRepository customerRepo,
            INotificationAlertService notificationService,
            IMapper mapper)
        {
            _repo = repo;
            _remittanceRepo = remittanceRepo;
            _customerRepo = customerRepo;
            _notificationService = notificationService;
            _mapper = mapper;
        }

        public async Task<Amendment> CreateAsync(CreateAmendmentDto dto)
        {
            var entity = _mapper.Map<Amendment>(dto);
            entity.RequestedDate = DateTime.UtcNow;
            entity.CreatedDate   = DateTime.UtcNow;
            entity.Status        = AmendmentStatus.Pending;
            var created = await _repo.CreateAsync(entity);
            return created;
        }

        public async Task<Amendment?> GetByIdAsync(int id)
        {
            return await _repo.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Amendment>> GetAllAsync()
        {
            return await _repo.GetAllAsync();
        }

        public async Task<Amendment> UpdateAsync(int id, CreateAmendmentDto dto)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) throw new Exception($"Amendment with ID {id} not found");

            // Map fields from DTO onto existing entity
            _mapper.Map(dto, existing);
            existing.UpdatedDate = DateTime.UtcNow;

            return await _repo.UpdateAsync(existing);
        }

        public async Task<Amendment> UpdateStatusAsync(int id, AmendmentStatus status)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) throw new KeyNotFoundException($"Amendment with ID {id} not found");

            existing.Status      = status;
            existing.UpdatedDate = DateTime.UtcNow;

            var updated = await _repo.UpdateAsync(existing);

            // When an amendment is approved, apply NewValue to the linked remittance field.
            if (status == AmendmentStatus.Approved && !string.IsNullOrWhiteSpace(existing.FieldChanged))
            {
                var remittance = await _remittanceRepo.GetByIdAsync(existing.RemitID);
                if (remittance != null && !remittance.IsDeleted)
                {
                    switch (existing.FieldChanged.Trim().ToLowerInvariant())
                    {
                        case "purposecode":
                            remittance.PurposeCode = existing.NewValue;
                            break;
                        case "sourceoffunds":
                            remittance.SourceOfFunds = existing.NewValue;
                            break;
                        case "beneficiaryid":
                            if (int.TryParse(existing.NewValue, out var benId))
                                remittance.BeneficiaryId = benId;
                            break;
                        case "sendamount":
                            if (decimal.TryParse(existing.NewValue,
                                System.Globalization.NumberStyles.Any,
                                System.Globalization.CultureInfo.InvariantCulture,
                                out var amount))
                                remittance.SendAmount = amount;
                            break;
                    }
                    remittance.UpdateDate = DateTime.UtcNow;
                    await _remittanceRepo.UpdateAsync(remittance);
                }
            }

            // Notify the customer about the amendment decision.
            await TryNotifyCustomerAsync(existing, status);

            return updated;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await _repo.DeleteAsync(id);
        }

        private async Task TryNotifyCustomerAsync(Amendment amendment, AmendmentStatus status)
        {
            try
            {
                var remittance = await _remittanceRepo.GetByIdAsync(amendment.RemitID);
                if (remittance == null) return;

                var customer = await _customerRepo.GetByIdAsync(remittance.CustomerId);
                if (customer == null) return;

                string message = status switch
                {
                    AmendmentStatus.Approved => $"Your amendment request for Remittance #{amendment.RemitID} has been approved. " +
                                                $"'{amendment.FieldChanged}' has been updated to '{amendment.NewValue}'.",
                    AmendmentStatus.Rejected => $"Your amendment request for Remittance #{amendment.RemitID} has been rejected.",
                    _                        => null
                };

                if (message == null) return;

                await _notificationService.CreateAsync(new CreateNotificationDto
                {
                    UserID   = customer.UserID,
                    RemitID  = amendment.RemitID,
                    Message  = message,
                    Category = NotificationCategory.Compliance,
                });
            }
            catch { /* notification failure must never block the main flow */ }
        }
    }
}
