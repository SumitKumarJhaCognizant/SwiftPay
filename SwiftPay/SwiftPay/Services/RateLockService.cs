using System;
using System.Threading.Tasks;
using AutoMapper;
using SwiftPay.Constants.Enums;
using SwiftPay.DTOs.FXQuoteDTO;
using SwiftPay.FXModule.Api.Models;
using SwiftPay.Repositories.Interfaces;
using SwiftPay.Services.Interfaces;

namespace SwiftPay.Services
{
    public class RateLockService : IRateLockService
    {
        private readonly IRateLockRepository _repo;
        private readonly IMapper _mapper;

        public RateLockService(IRateLockRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<RateLockResponseDto> LockRateAsync(CreateRateLockRequestDto request)
        {
            // 1. Map from Request DTO to Database Model
            var newLock = _mapper.Map<RateLock>(request);
            
            // 2. Set the business logic defaults
            newLock.Status = RateLockStatus.Locked;
            newLock.LockStart = DateTime.UtcNow;

            // 3. Save to database
            var savedLock = await _repo.CreateRateLockAsync(newLock);

            // 4. Map saved Model back to Response DTO
            return _mapper.Map<RateLockResponseDto>(savedLock);
        }
        public async Task<RateLockResponseDto> GetRateLockAsync(string lockId)
        {
            var rateLock = await _repo.GetRateLockByIdAsync(lockId);
            if (rateLock == null) return null;

            return _mapper.Map<RateLockResponseDto>(rateLock);
        }
    }
}