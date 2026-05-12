using AutoMapper;
using SwiftPay.Constants.Enums;
using SwiftPay.Domain.Remittance.Entities;
using SwiftPay.DTOs.RefundRefDTO;
using SwiftPay.DTOs.UserCustomerDTO;
using SwiftPay.Repositories.Interfaces;
using SwiftPay.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SwiftPay.Services
{
    public class RefundRefService : IRefundRefService
    {
        private readonly IRefundRefRepository _repo;
        private readonly IRemittanceRepository _remittanceRepo;
        private readonly ICustomerRepository _customerRepo;
        private readonly INotificationAlertService _notificationService;
        private readonly IMapper _mapper;

        public RefundRefService(
            IRefundRefRepository repo,
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

        public async Task<RefundRef> CreateAsync(CreateRefundRefDto dto)
        {
            var entity = _mapper.Map<RefundRef>(dto);
            var created = await _repo.CreateAsync(entity);
            return created;
        }

        public async Task<RefundRef?> GetByIdAsync(int id)
        {
            return await _repo.GetByIdAsync(id);
        }

        public async Task<IEnumerable<RefundRef>> GetAllAsync()
        {
            return await _repo.GetAllAsync();
        }

        public async Task<RefundRef> UpdateAsync(int id, CreateRefundRefDto dto)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) throw new Exception($"RefundRef with ID {id} not found");

            _mapper.Map(dto, existing);
            existing.UpdatedDate = DateTime.UtcNow;

            return await _repo.UpdateAsync(existing);
        }

        public async Task<RefundRef> UpdateStatusAsync(int id, RefundStatus status)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) throw new KeyNotFoundException($"RefundRef with ID {id} not found");

            existing.Status      = status;
            existing.UpdatedDate = DateTime.UtcNow;

            var updated = await _repo.UpdateAsync(existing);

            // Notify the customer when the refund status reaches a terminal or important state.
            await TryNotifyCustomerAsync(existing, status);

            return updated;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await _repo.DeleteAsync(id);
        }

        private async Task TryNotifyCustomerAsync(RefundRef refund, RefundStatus status)
        {
            try
            {
                var remittance = await _remittanceRepo.GetByIdAsync(refund.RemitID);
                if (remittance == null) return;

                var customer = await _customerRepo.GetByIdAsync(remittance.CustomerId);
                if (customer == null) return;

                string message = status switch
                {
                    RefundStatus.Completed => $"Your refund of {refund.Amount:0.00} for Remittance #{refund.RemitID} has been completed successfully.",
                    RefundStatus.Failed    => $"Your refund for Remittance #{refund.RemitID} could not be processed. Please contact support.",
                    RefundStatus.Initiated => $"A refund of {refund.Amount:0.00} for Remittance #{refund.RemitID} has been initiated.",
                    _                      => null
                };

                if (message == null) return;

                await _notificationService.CreateAsync(new CreateNotificationDto
                {
                    UserID   = customer.UserID,
                    RemitID  = refund.RemitID,
                    Message  = message,
                    Category = NotificationCategory.Refund,
                });
            }
            catch { /* notification failure must never block the main flow */ }
        }
    }
}
