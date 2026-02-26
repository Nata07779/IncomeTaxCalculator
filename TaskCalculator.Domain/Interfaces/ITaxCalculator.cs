using TaxCalculator.Models;

namespace TaxCalculator.Services
{
    public interface ITaxCalculator
    {
        Task<TaxCalculationResult> CalculateAsync(decimal grossAnnualSalary);
    }
}
