using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly IFXQuoteRepository _quoteRepo; // --- ADDED: Inject the Quote Repository ---
        private readonly IMapper _mapper;

        public RateLockService(IRateLockRepository repo, IFXQuoteRepository quoteRepo, IMapper mapper)
        {
            _repo = repo;
            _quoteRepo = quoteRepo; // Assign it here
            _mapper = mapper;
        }

        public async Task<RateLockResponseDto> LockRateAsync(CreateRateLockRequestDto request)
        {
            // ==========================================
            //       THE SECURITY CHECKPOINT (BOUNCER)
            // ==========================================
            
            // 1. Find the exact Quote in the database
            var quote = await _quoteRepo.GetQuoteByIdAsync(request.QuoteID);
            
            // 2. Does the quote even exist?
            if (quote == null) 
                throw new Exception("Quote not found.");

            // 3. WHO OWNS IT? (This stops Customer from stealing other Customer's quote!)
            if (quote.CustomerID != request.CustomerID) 
                throw new UnauthorizedAccessException("You are not authorized to lock this quote.");

            // 4. Is the quote already locked or used?
            if (quote.Status != FXQuoteStatus.Active) 
                throw new InvalidOperationException("This quote is no longer active and cannot be locked.");

            // 5. Is it expired? (Older than 15 minutes)
            if (quote.ValidUntil < DateTime.UtcNow)
            {
                quote.Status = FXQuoteStatus.Expired;
                await _quoteRepo.UpdateQuoteAsync(quote); // Mark it as expired in DB
                throw new InvalidOperationException("This quote has expired. Please generate a new one.");
            }
            
            // ==========================================

            // If it survives all checks, map it to the database model
            var newLock = _mapper.Map<RateLock>(request);
            
            // Set the business logic defaults
            newLock.Status = RateLockStatus.Locked;
            newLock.LockStart = DateTime.UtcNow;
            newLock.LockExpiry = DateTime.UtcNow.AddMinutes(15);
            
            // Save the new Rate Lock to the database
            var savedLock = await _repo.CreateRateLockAsync(newLock);

            // SYSTEM UPDATE: Update the original Quote's status to Locked so it can't be used twice!
            quote.Status = FXQuoteStatus.Locked;
            await _quoteRepo.UpdateQuoteAsync(quote);

            // Map saved Model back to Response DTO
            return _mapper.Map<RateLockResponseDto>(savedLock);
        }
        
        public async Task<RateLockResponseDto> GetRateLockAsync(string lockId)
        {
            var rateLock = await _repo.GetRateLockByIdAsync(lockId);
            if (rateLock == null) return null;
            return _mapper.Map<RateLockResponseDto>(rateLock);
        }

        public async Task<IEnumerable<RateLockResponseDto>> GetAllRateLocksAsync()
        {
            var all = await _repo.GetAllRateLocksAsync();
            return all.Select(l => _mapper.Map<RateLockResponseDto>(l));
        }
    }
}