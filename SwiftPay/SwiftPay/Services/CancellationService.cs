using AutoMapper;
using SwiftPay.Constants.Enums;
using SwiftPay.Domain.Remittance.Entities;
using SwiftPay.DTOs.CancellationDTO;
using SwiftPay.DTOs.UserCustomerDTO;
using SwiftPay.Repositories.Interfaces;
using SwiftPay.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SwiftPay.Services
{
    public class CancellationService : ICancellationService
    {
        private readonly ICancellationRepository _repo;
        private readonly IRemittanceRepository _remittanceRepo;
        private readonly ICustomerRepository _customerRepo;
        private readonly INotificationAlertService _notificationService;
        private readonly IMapper _mapper;

        public CancellationService(
            ICancellationRepository repo,
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

        public async Task<Cancellation> CreateAsync(CreateCancellationDto dto)
        {
            var entity = _mapper.Map<Cancellation>(dto);
            entity.RequestedDate = DateTime.UtcNow;
            entity.CreatedDate   = DateTime.UtcNow;
            entity.Status        = CancellationStatus.Requested;
            var created = await _repo.CreateAsync(entity);
            return created;
        }

        public async Task<Cancellation?> GetByIdAsync(int id)
        {
            return await _repo.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Cancellation>> GetAllAsync()
        {
            return await _repo.GetAllAsync();
        }

        public async Task<Cancellation> UpdateAsync(int id, CreateCancellationDto dto)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) throw new Exception($"Cancellation with ID {id} not found");

            _mapper.Map(dto, existing);
            existing.UpdatedDate = DateTime.UtcNow;

            return await _repo.UpdateAsync(existing);
        }

        public async Task<Cancellation> UpdateStatusAsync(int id, CancellationStatus status)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) throw new KeyNotFoundException($"Cancellation with ID {id} not found");

            existing.Status      = status;
            existing.UpdatedDate = DateTime.UtcNow;

            var updated = await _repo.UpdateAsync(existing);

            // When a cancellation is approved, mark the linked remittance as Cancelled.
            if (status == CancellationStatus.Approved)
            {
                var remittance = await _remittanceRepo.GetByIdAsync(existing.RemitID);
                if (remittance != null && !remittance.IsDeleted)
                {
                    remittance.Status     = RemittanceRequestStatus.Cancelled;
                    remittance.UpdateDate = DateTime.UtcNow;
                    await _remittanceRepo.UpdateAsync(remittance);
                }
            }

            // Notify the customer about the cancellation status change.
            await TryNotifyCustomerAsync(existing.RemitID, status);

            return updated;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await _repo.DeleteAsync(id);
        }

        private async Task TryNotifyCustomerAsync(int remitId, CancellationStatus status)
        {
            try
            {
                var remittance = await _remittanceRepo.GetByIdAsync(remitId);
                if (remittance == null) return;

                var customer = await _customerRepo.GetByIdAsync(remittance.CustomerId);
                if (customer == null) return;

                string message = status switch
                {
                    CancellationStatus.Approved => $"Your cancellation request for Remittance #{remitId} has been approved. The remittance has been cancelled.",
                    CancellationStatus.Rejected => $"Your cancellation request for Remittance #{remitId} has been rejected. The remittance will continue processing.",
                    CancellationStatus.Posted   => $"Your cancellation for Remittance #{remitId} has been finalised and posted.",
                    _                           => null
                };

                if (message == null) return;

                await _notificationService.CreateAsync(new CreateNotificationDto
                {
                    UserID   = customer.UserID,
                    RemitID  = remitId,
                    Message  = message,
                    Category = NotificationCategory.Refund,
                });
            }
            catch { /* notification failure must never block the main flow */ }
        }
    }
}
