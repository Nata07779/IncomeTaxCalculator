using TaxCalculator.Models;

namespace TaxCalculator.Services
{
    public interface ITaxBandRepository
    {
        Task<IEnumerable<TaxBandDto>> GetTaxBandsAsync();
    }
}