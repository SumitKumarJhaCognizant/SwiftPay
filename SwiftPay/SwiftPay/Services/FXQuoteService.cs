using System;
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
    public class FXQuoteService : IFXQuoteService
    {
        private readonly IFXQuoteRepository _repo;
        private readonly IMapper _mapper;
        private readonly IFeeRuleRepository _feeRepo;

        public FXQuoteService(IFXQuoteRepository repo, IMapper mapper, IFeeRuleRepository feeRepo)
        {
            _repo = repo;
            _mapper = mapper;
            _feeRepo = feeRepo;
        }

        public async Task<FXQuoteResponseDto> GenerateQuoteAsync(CreateQuoteRequestDto request)
        {
            string fromCcy = request.FromCurrency.ToUpper();
            string toCcy = request.ToCurrency.ToUpper();
            string corridor = $"{fromCcy}-{toCcy}";

            decimal midRate = GetMockMarketRate(corridor);

            // 50 bps margin — subtract from mid-rate so SwiftPay earns on the spread
            int marginBps = 50;
            decimal marginMultiplier = marginBps / 10000m;
            decimal offeredRate = Math.Round(midRate * (1 - marginMultiplier), 6);

            // Resolve applicable fee for this corridor
            var allRules = await _feeRepo.GetAllActiveFeeRulesAsync();
            var applicableRule = allRules
                .Where(r => r.Corridor == corridor)
                .OrderByDescending(r => r.CreatedDate)
                .FirstOrDefault();

            decimal fee = applicableRule?.FeeValue ?? 0m;

            // Receiver amount = (sendAmount - fee) * offeredRate
            decimal netSendAmount = Math.Max(request.SendAmount - fee, 0m);
            decimal receiverAmount = Math.Round(netSendAmount * offeredRate, 4);

            var newQuote = new FXQuote
            {
                FromCurrency = fromCcy,
                ToCurrency = toCcy,
                SendAmount = request.SendAmount,
                ReceiverAmount = receiverAmount,
                MidRate = midRate,
                MarginBps = marginBps,
                OfferedRate = offeredRate,
                Fee = fee,
                QuoteTime = DateTime.UtcNow,
                ValidUntil = DateTime.UtcNow.AddMinutes(15),
                Status = FXQuoteStatus.Active
            };

            var savedQuote = await _repo.AddQuoteAsync(newQuote);

            return new FXQuoteResponseDto
            {
                QuoteId = savedQuote.QuoteID,
                FromCurrency = savedQuote.FromCurrency,
                ToCurrency = savedQuote.ToCurrency,
                SendAmount = savedQuote.SendAmount,
                ReceiverAmount = savedQuote.ReceiverAmount,
                MidRate = savedQuote.MidRate,
                MarginBps = savedQuote.MarginBps,
                OfferedRate = savedQuote.OfferedRate,
                Fee = savedQuote.Fee,
                ValidUntil = savedQuote.ValidUntil
            };
        }

        public async Task<FXQuoteResponseDto> GetQuoteAsync(string quoteId)
        {
            var quote = await _repo.GetQuoteByIdAsync(quoteId);
            if (quote == null) return null;

            // Re-resolve the current fee in case rules changed since quote was created
            string corridor = $"{quote.FromCurrency}-{quote.ToCurrency}";
            var allRules = await _feeRepo.GetAllActiveFeeRulesAsync();
            var applicableRule = allRules
                .Where(r => r.Corridor == corridor)
                .OrderByDescending(r => r.CreatedDate)
                .FirstOrDefault();

            return new FXQuoteResponseDto
            {
                QuoteId = quote.QuoteID,
                FromCurrency = quote.FromCurrency,
                ToCurrency = quote.ToCurrency,
                SendAmount = quote.SendAmount,
                ReceiverAmount = quote.ReceiverAmount,
                MidRate = quote.MidRate,
                MarginBps = quote.MarginBps,
                OfferedRate = quote.OfferedRate,
                Fee = applicableRule?.FeeValue ?? quote.Fee,
                ValidUntil = quote.ValidUntil
            };
        }

        private decimal GetMockMarketRate(string corridor)
        {
            return corridor switch
            {
                // Inward corridors (foreign → INR)
                "USD-INR" => 83.5025m,
                "GBP-INR" => 105.7510m,
                "EUR-INR" => 90.2050m,
                "CAD-INR" => 61.3040m,
                "AED-INR" => 22.7350m,
                "AUD-INR" => 54.8020m,
                "SGD-INR" => 62.1550m,
                "SAR-INR" => 22.2510m,
                "JPY-INR" => 0.5520m,
                "CHF-INR" => 93.4010m,
                "NZD-INR" => 50.1200m,
                "MYR-INR" => 18.9500m,
                "HKD-INR" => 10.6800m,

                // Outward corridors (INR → foreign) — inverse of inward rates
                "INR-USD" => Math.Round(1m / 83.5025m, 6),
                "INR-GBP" => Math.Round(1m / 105.7510m, 6),
                "INR-EUR" => Math.Round(1m / 90.2050m, 6),
                "INR-CAD" => Math.Round(1m / 61.3040m, 6),
                "INR-AED" => Math.Round(1m / 22.7350m, 6),
                "INR-AUD" => Math.Round(1m / 54.8020m, 6),
                "INR-SGD" => Math.Round(1m / 62.1550m, 6),
                "INR-SAR" => Math.Round(1m / 22.2510m, 6),
                "INR-JPY" => Math.Round(1m / 0.5520m, 6),
                "INR-CHF" => Math.Round(1m / 93.4010m, 6),
                "INR-NZD" => Math.Round(1m / 50.1200m, 6),
                "INR-MYR" => Math.Round(1m / 18.9500m, 6),
                "INR-HKD" => Math.Round(1m / 10.6800m, 6),

                // Major cross-currency corridors
                "EUR-USD" => 1.0820m,
                "GBP-USD" => 1.2650m,
                "USD-EUR" => Math.Round(1m / 1.0820m, 6),
                "USD-GBP" => Math.Round(1m / 1.2650m, 6),
                "USD-JPY" => 151.2000m,
                "USD-CAD" => 1.3580m,
                "AUD-USD" => 0.6550m,
                "USD-AUD" => Math.Round(1m / 0.6550m, 6),
                "USD-SGD" => 1.3450m,
                "USD-AED" => 3.6725m,
                "USD-SAR" => 3.7500m,
                "USD-HKD" => 7.8200m,
                "USD-CHF" => Math.Round(1m / 93.4010m * 83.5025m, 6),
                "GBP-EUR" => Math.Round(1.2650m / 1.0820m, 6),
                "EUR-GBP" => Math.Round(1.0820m / 1.2650m, 6),

                _ => throw new NotSupportedException($"Currency corridor '{corridor}' is not supported.")
            };
        }
    }
}
