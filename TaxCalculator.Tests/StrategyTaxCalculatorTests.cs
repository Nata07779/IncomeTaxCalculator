using System.Threading.Tasks;
using TaxCalculator.Models;
using TaxCalculator.Services;
using Xunit;

namespace TaxCalculator.Tests
{
    public class StrategyTaxCalculatorTests
    {
        [Fact]
        public async Task UsesPlainCalculator_WhenSalaryIsZero()
        {
            // Arrange
            var plain = new ZeroTaxCalculator();
            var selector = new FakeSelector(plain);
            var strategy = new StrategyTaxCalculator(selector);

            // Act
            var result = await strategy.CalculateAsync(0m);

            // Assert
            Assert.Equal(0m, result.AnnualTaxPaid);
            Assert.Equal(0m, result.MonthlyTaxPaid);
            Assert.Equal(0m, result.GrossMonthlySalary);
        }

        [Fact]
        public async Task DelegatesToSelectedCalculator_ForNonZeroSalary()
        {
            // Arrange
            var expected = new TaxCalculationResult { GrossAnnualSalary = 100m, AnnualTaxPaid = 12.34m };
            var fake = new FakeCalculator(expected);
            var selector = new FakeSelector(fake);
            var strategy = new StrategyTaxCalculator(selector);

            // Act
            var result = await strategy.CalculateAsync(100m);

            // Assert
            Assert.True(fake.Called);
            Assert.Equal(expected.AnnualTaxPaid, result.AnnualTaxPaid);
            Assert.Equal(expected.GrossAnnualSalary, result.GrossAnnualSalary);
        }

        private class FakeSelector : ITaxCalculatorSelector
        {
            private readonly ITaxCalculator _calculator;

            public FakeSelector(ITaxCalculator calculator) => _calculator = calculator;

            public ITaxCalculator Select(decimal grossAnnualSalary) => _calculator;
        }

        private class FakeCalculator : ITaxCalculator
        {
            public bool Called { get; private set; }
            private readonly TaxCalculationResult _result;

            public FakeCalculator(TaxCalculationResult result) => _result = result;

            public Task<TaxCalculationResult> CalculateAsync(decimal grossAnnualSalary)
            {
                Called = true;
                return Task.FromResult(_result);
            }
        }
    }
}
