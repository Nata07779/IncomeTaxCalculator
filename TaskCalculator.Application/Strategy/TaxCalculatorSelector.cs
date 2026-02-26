using Microsoft.Extensions.DependencyInjection;

namespace TaxCalculator.Services
{
    // Factory to select appropriate tax calculator based on gross annual salary.
    public class TaxCalculatorSelector : ITaxCalculatorSelector
    {
        private readonly IServiceProvider _sp;

        public TaxCalculatorSelector(IServiceProvider sp)
        {
            _sp = sp;
        }

        public ITaxCalculator Select(decimal grossAnnualSalary)
        {
            if (grossAnnualSalary == 0m)
            {
                return _sp.GetRequiredService<ZeroTaxCalculator>();
            }

            return _sp.GetRequiredService<ProgressiveTaxCalculator>();
        }
    }
}
