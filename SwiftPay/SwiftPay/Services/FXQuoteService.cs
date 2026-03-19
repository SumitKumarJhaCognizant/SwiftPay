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
    public class FXQuoteService : IFXQuoteService
    {
        private readonly IFXQuoteRepository _repo;
        private readonly IMapper _mapper;

        public FXQuoteService(IFXQuoteRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<FXQuoteResponseDto> GenerateQuoteAsync(CreateQuoteRequestDto request)
        {
            var newQuote = _mapper.Map<FXQuote>(request);
            
            // 1. Identify the Corridor (e.g., "USD-INR")
            string corridor = $"{request.FromCurrency.ToUpper()}-{request.ToCurrency.ToUpper()}";

            // 2. Fetch the current market rate (Mid-Rate)
            decimal midRate = GetMockMarketRate(corridor);

            // 3. Business Math: Calculate our profit margin
            // 1 Basis Point (BPS) = 0.01% (or 0.0001 in decimal)
            int marginBps = 50; // SwiftPay takes a 50 bps margin on all trades
            decimal marginMultiplier = marginBps / 10000m; // Equals 0.0050

            // 4. Calculate the Final Offered Rate
            // We subtract the margin from the market rate to ensure SwiftPay makes a profit on the spread
            decimal offeredRate = midRate * (1 - marginMultiplier);

            // 5. Apply the calculated values to our model
            newQuote.MidRate = midRate;
            newQuote.MarginBps = marginBps;
            newQuote.OfferedRate = Math.Round(offeredRate, 6); // FX rates standard is 6 decimal places
            
            newQuote.QuoteTime = DateTime.UtcNow;
            newQuote.ValidUntil = DateTime.UtcNow.AddMinutes(15);
            newQuote.Status = FXQuoteStatus.Active;

            // 6. Save to Database
            var savedQuote = await _repo.AddQuoteAsync(newQuote);

            // 7. Return to Controller
            return _mapper.Map<FXQuoteResponseDto>(savedQuote);
        }

        public async Task<FXQuoteResponseDto> GetQuoteAsync(string quoteId)
        {
            var quote = await _repo.GetQuoteByIdAsync(quoteId);
            if (quote == null) return null; 

            return _mapper.Map<FXQuoteResponseDto>(quote);
        }

        // --- Private Helper Method ---
        // In a production environment, this would call an external API like Bloomberg or Reuters.
        // For our module, we will mock the live market data.
        private decimal GetMockMarketRate(string corridor)
        {
            return corridor switch
            {
                "USD-INR" => 83.5025m,
                "GBP-INR" => 105.7510m,
                "EUR-INR" => 90.2050m,
                "CAD-INR" => 61.3040m,
                "AED-INR" => 22.7350m,
                _ => throw new Exception($"Unsupported currency corridor: {corridor}") 
            };
        }
    }
}