using TaxCalculator.Models;

namespace TaskCalculator.Domain.Interfaces
{
    public interface IStrategyTaxCalculator
    {
        Task<TaxCalculationResult> CalculateAsync(decimal grossAnnualSalary);
    }
}