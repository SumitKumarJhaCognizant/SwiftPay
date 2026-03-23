using AutoMapper;
using SwiftPay.Constants.Enums;
using SwiftPay.Domain.Notification.Entities;
using SwiftPay.DTOs.UserCustomerDTO;
using SwiftPay.Repositories.Interfaces;
using SwiftPay.Services.Interfaces;

namespace SwiftPay.Services
{
	public class NotificationAlertService : INotificationAlertService
	{
		private readonly INotificationAlertRepository _repo;
		private readonly IUserRepository _userRepo;
		private readonly IMapper _mapper;
		private readonly IAuditLogService _auditLogService;

		public NotificationAlertService(INotificationAlertRepository repo, IUserRepository userRepo, IMapper mapper, IAuditLogService auditLogService)
		{
			_repo = repo;
			_userRepo = userRepo;
			_mapper = mapper;
			_auditLogService = auditLogService;
		}

	public async Task<NotificationResponseDto> CreateAsync(CreateNotificationDto dto)
	{
		// Validate that User exists - BUSINESS LOGIC
		var user = await _userRepo.GetByIdAsync(dto.UserID);
		if (user == null)
			throw new KeyNotFoundException($"User with ID {dto.UserID} does not exist.");

		// Use AutoMapper to map DTO to entity
		var entity = _mapper.Map<NotificationAlert>(dto);

		var created = await _repo.CreateAsync(entity);
		try
		{
			await _auditLogService.CreateAsync(new DTOs.UserCustomerDTO.CreateAuditLogDto
			{
				UserID = created.UserID,
				Action = "Notification.Create",
				Resource = "Notification",
				Details = $"Notification {created.NotificationID} created for user {created.UserID}."
			});
		}
		catch { }
		return _mapper.Map<NotificationResponseDto>(created);
	}

	public async Task<NotificationResponseDto> GetByIdAsync(int notificationId)
	{
		var notification = await _repo.GetByIdAsync(notificationId);
		return _mapper.Map<NotificationResponseDto>(notification);
	}

	public async Task<IEnumerable<NotificationResponseDto>> GetByUserIdAsync(int userId)
	{
		var notifications = await _repo.GetByUserIdAsync(userId);
		return _mapper.Map<List<NotificationResponseDto>>(notifications);
	}

	public async Task<IEnumerable<NotificationResponseDto>> GetUnreadByUserIdAsync(int userId)
	{
		var notifications = await _repo.GetUnreadByUserIdAsync(userId);
		return _mapper.Map<List<NotificationResponseDto>>(notifications);
	}

	public async Task<IEnumerable<NotificationResponseDto>> GetAllAsync()
	{
		var notifications = await _repo.GetAllAsync();
		return _mapper.Map<List<NotificationResponseDto>>(notifications);
	}

	public async Task<NotificationResponseDto> MarkAsReadAsync(int notificationId)
	{
		// Retrieve notification - business logic validation
		var notification = await _repo.GetByIdAsync(notificationId);
		if (notification == null)
			throw new KeyNotFoundException($"Notification with ID {notificationId} not found");

		// Set status - ReadAt and UpdatedAt are handled by AuditLogInterceptor automatically
		notification.Status = NotificationStatus.Read;

		var updated = await _repo.UpdateAsync(notification);
		try
		{
			await _auditLogService.CreateAsync(new DTOs.UserCustomerDTO.CreateAuditLogDto
			{
				UserID = updated.UserID,
				Action = "Notification.MarkAsRead",
				Resource = "Notification",
				Details = $"Notification {updated.NotificationID} marked as read for user {updated.UserID}."
			});
		}
		catch { }
		return _mapper.Map<NotificationResponseDto>(updated);
	}

	public async Task<IEnumerable<NotificationResponseDto>> MarkAllAsReadAsync(int userId)
	{
		// Retrieve all unread notifications for user - BUSINESS LOGIC
		var notifications = (await _repo.GetUnreadByUserIdAsync(userId)).ToList();

		// Update each notification status
		foreach (var notification in notifications)
		{
			notification.Status = NotificationStatus.Read;
			await _repo.UpdateAsync(notification);
			try
			{
				await _auditLogService.CreateAsync(new DTOs.UserCustomerDTO.CreateAuditLogDto
				{
					UserID = notification.UserID,
					Action = "Notification.MarkAsRead",
					Resource = "Notification",
					Details = $"Notification {notification.NotificationID} marked as read for user {notification.UserID}."
				});
			}
			catch { }
		}

		return _mapper.Map<List<NotificationResponseDto>>(notifications);
	}

		public async Task<bool> DeleteAsync(int notificationId)
		{
			var notification = await _repo.GetByIdAsync(notificationId);
			var result = await _repo.DeleteAsync(notificationId);
			if (result)
			{
				try
				{
					await _auditLogService.CreateAsync(new DTOs.UserCustomerDTO.CreateAuditLogDto
					{
						UserID = notification?.UserID ?? 0,
						Action = "Notification.Delete",
						Resource = "Notification",
						Details = $"Notification {notificationId} deleted."
					});
				}
				catch { }
			}
			return result;
		}
	}
}
