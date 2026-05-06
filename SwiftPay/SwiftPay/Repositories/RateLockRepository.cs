using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SwiftPay.Configuration;
using SwiftPay.FXModule.Api.Models;
using SwiftPay.Repositories.Interfaces;

namespace SwiftPay.Repositories
{
    public class RateLockRepository : IRateLockRepository
    {
        private readonly AppDbContext _context;

        public RateLockRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<RateLock> CreateRateLockAsync(RateLock rateLock)
        {
            await _context.RateLocks.AddAsync(rateLock);
            await _context.SaveChangesAsync();
            return rateLock;
        }
        public async Task<RateLock> GetRateLockByIdAsync(string lockId)
        {
            return await _context.RateLocks
                .FirstOrDefaultAsync(r => r.LockID == lockId && !r.IsDeleted);
        }

        public async Task<IEnumerable<RateLock>> GetAllRateLocksAsync()
        {
            return await _context.RateLocks
                .Where(r => !r.IsDeleted)
                .OrderByDescending(r => r.LockStart)
                .ToListAsync();
        }
    }
}