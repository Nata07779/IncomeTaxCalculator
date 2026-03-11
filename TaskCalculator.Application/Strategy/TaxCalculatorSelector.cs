using TaskCalculator.Domain.Enums;

namespace TaxCalculator.Services
{
    // Factory to select appropriate tax calculator based on gross annual salary.
    public class TaxCalculatorSelector : ITaxCalculatorSelector
    {
        private readonly IEnumerable<ITaxCalculator> _taxCalculators;

        public TaxCalculatorSelector(IEnumerable<ITaxCalculator> taxCalculators)
        {
            _taxCalculators = taxCalculators;
        }

        public ITaxCalculator Select(decimal grossAnnualSalary)
        {
            var taxCalculatorType = TaxCalculatorType.Progressive;
            if (grossAnnualSalary == 0m)
            {
                taxCalculatorType = TaxCalculatorType.Zero;
            }

            var calculator = GetCalculatorByType(taxCalculatorType);

            return calculator;
        }

        private ITaxCalculator GetCalculatorByType(TaxCalculatorType taxCalculatorType)
        {
            var calculator = _taxCalculators.FirstOrDefault(x => x.Type == taxCalculatorType);

            if (calculator is null)
            {
                throw new InvalidOperationException($"No tax calculator found for type {taxCalculatorType}.");
            }

            return calculator;
        }
    }
}
