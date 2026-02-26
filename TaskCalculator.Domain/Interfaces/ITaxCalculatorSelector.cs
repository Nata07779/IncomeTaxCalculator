namespace TaxCalculator.Services
{
    public interface ITaxCalculatorSelector
    {
        ITaxCalculator Select(decimal grossAnnualSalary);
    }
}
