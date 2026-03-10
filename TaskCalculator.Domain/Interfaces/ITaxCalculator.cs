using TaskCalculator.Domain.Enums;
using TaxCalculator.Models;

namespace TaxCalculator.Services
{
    public interface ITaxCalculator
    {
        TaxCalculatorType Type { get; }
        Task<TaxCalculationResult> CalculateAsync(decimal grossAnnualSalary);
    }
}
