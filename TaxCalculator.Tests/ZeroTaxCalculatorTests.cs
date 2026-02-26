using System.Threading.Tasks;
using TaxCalculator.Services;
using Xunit;

namespace TaxCalculator.Tests
{
    public class ZeroTaxCalculatorTests
    {
        [Fact]
        public async Task CalculateAsync_ZeroSalary_ReturnsZeros()
        {
            // Arrange
            var calc = new ZeroTaxCalculator();

            // Act
            var result = await calc.CalculateAsync(0m);

            // Assert
            Assert.Equal(0m, result.GrossAnnualSalary);
            Assert.Equal(0m, result.GrossMonthlySalary);
            Assert.Equal(0m, result.AnnualTaxPaid);
            Assert.Equal(0m, result.MonthlyTaxPaid);
            Assert.Equal(0m, result.NetAnnualSalary);
            Assert.Equal(0m, result.NetMonthlySalary);
        }

        [Fact]
        public async Task CalculateAsync_NonZeroSalary_ReturnsNoTaxAndNetEqualsGross()
        {
            // Arrange
            var calc = new ZeroTaxCalculator();
            decimal gross = 12345.67m;

            // Act
            var result = await calc.CalculateAsync(gross);

            // Assert
            Assert.Equal(decimal.Round(gross, 2, System.MidpointRounding.ToEven), result.GrossAnnualSalary);
            Assert.Equal(decimal.Round(gross / 12m, 2, System.MidpointRounding.ToEven), result.GrossMonthlySalary);
            Assert.Equal(0m, result.AnnualTaxPaid);
            Assert.Equal(0m, result.MonthlyTaxPaid);
            Assert.Equal(decimal.Round(gross, 2, System.MidpointRounding.ToEven), result.NetAnnualSalary);
            Assert.Equal(decimal.Round(gross / 12m, 2, System.MidpointRounding.ToEven), result.NetMonthlySalary);
        }
    }
}
