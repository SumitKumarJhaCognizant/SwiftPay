using System.Threading.Tasks;
using SwiftPay.FXModule.Api.Models;

namespace SwiftPay.Repositories.Interfaces
{
    public interface IRateLockRepository
    {
        Task<RateLock> CreateRateLockAsync(RateLock rateLock);
        Task<RateLock> GetRateLockByIdAsync(string lockId);
    }
}