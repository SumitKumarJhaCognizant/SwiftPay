using System.Collections.Generic;
using System.Threading.Tasks;
using SwiftPay.FXModule.Api.Models;

namespace SwiftPay.Repositories.Interfaces
{
    public interface IFXQuoteRepository
    {
        Task<FXQuote> AddQuoteAsync(FXQuote quote);
        Task<FXQuote> GetQuoteByIdAsync(string quoteId);
        Task<IEnumerable<FXQuote>> GetAllQuotesAsync();
    }
}