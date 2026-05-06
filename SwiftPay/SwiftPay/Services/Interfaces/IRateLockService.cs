using System.Collections.Generic;
using System.Threading.Tasks;
using SwiftPay.DTOs.FXQuoteDTO;

namespace SwiftPay.Services.Interfaces
{
    public interface IRateLockService
    {
        Task<RateLockResponseDto> LockRateAsync(CreateRateLockRequestDto request);
        Task<RateLockResponseDto> GetRateLockAsync(string lockId);
        Task<IEnumerable<RateLockResponseDto>> GetAllRateLocksAsync();
    }
}