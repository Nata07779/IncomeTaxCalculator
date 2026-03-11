using TaskCalculator.Domain.Interfaces;
using TaxCalculator.Models;

namespace TaxCalculator.Services
{
    // Strategy wrapper that selects the appropriate calculator per request
    public class StrategyTaxCalculator : IStrategyTaxCalculator
    {
        private readonly ITaxCalculatorSelector _selector;

        public StrategyTaxCalculator(ITaxCalculatorSelector selector)
        {
            _selector = selector;
        }

        public Task<TaxCalculationResult> CalculateAsync(decimal grossAnnualSalary)
        {
            var calc = _selector.Select(grossAnnualSalary);
            return calc.CalculateAsync(grossAnnualSalary);
        }
    }
}
