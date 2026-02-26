using TaxCalculator.Models;

namespace TaxCalculator.Services
{
    // Simple calculator used for (salary == 0)
    public class ZeroTaxCalculator : ITaxCalculator
    {
        public Task<TaxCalculationResult> CalculateAsync(decimal grossAnnualSalary)
        {
            // For a zero salary, all results are zeroed with proper rounding
            var result = new TaxCalculationResult
            {
                GrossAnnualSalary = decimal.Round(grossAnnualSalary, 2, MidpointRounding.ToEven),
                GrossMonthlySalary = decimal.Round(grossAnnualSalary / 12m, 2, MidpointRounding.ToEven),
                AnnualTaxPaid = 0m,
                MonthlyTaxPaid = 0m,
                NetAnnualSalary = decimal.Round(grossAnnualSalary, 2, MidpointRounding.ToEven)
            };
            result.NetMonthlySalary = decimal.Round(result.NetAnnualSalary / 12m, 2, MidpointRounding.ToEven);

            return Task.FromResult(result);
        }
    }
}
