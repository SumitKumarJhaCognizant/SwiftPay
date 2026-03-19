using System.Threading.Tasks;
using SwiftPay.DTOs.FXQuoteDTO;

namespace SwiftPay.Services.Interfaces
{
    public interface IFXQuoteService
    {
        Task<FXQuoteResponseDto> GenerateQuoteAsync(CreateQuoteRequestDto request);
        Task<FXQuoteResponseDto> GetQuoteAsync(string quoteId);
    }
}