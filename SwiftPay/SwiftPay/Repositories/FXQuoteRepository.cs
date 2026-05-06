using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SwiftPay.Configuration; 
using SwiftPay.FXModule.Api.Models;
using SwiftPay.Repositories.Interfaces;

namespace SwiftPay.Repositories
{
    public class FXQuoteRepository : IFXQuoteRepository
    {
        private readonly AppDbContext _context;

        public FXQuoteRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<FXQuote> AddQuoteAsync(FXQuote quote)
        {
            await _context.FXQuotes.AddAsync(quote);
            await _context.SaveChangesAsync();
            return quote; 
        }

        public async Task<FXQuote> GetQuoteByIdAsync(string quoteId)
        {
            return await _context.FXQuotes.FirstOrDefaultAsync(q => q.QuoteID == quoteId && !q.IsDeleted);
        }

        // --- ADDED: The implementation to save the updated status to SQL Server ---
        public async Task<FXQuote> UpdateQuoteAsync(FXQuote quote)
        {
            _context.FXQuotes.Update(quote);
            await _context.SaveChangesAsync();
            return quote;
        }
        // --------------------------------------------------------------------------
    }
}