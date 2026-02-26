using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using TaxCalculator.Data;
using TaxCalculator.Services;
using Xunit;

namespace TaxCalculator.Tests
{
    public class ProgressiveTaxCalculatorTests
    {
        private AppDbContext CreateInMemoryDb()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "TaxCalcTestDb")
                .Options;

            var db = new AppDbContext(options);
            if (!db.TaxBands.Any())
            {
                db.TaxBands.AddRange(new[] {
                    new Data.TaxBand { Id = 1, Band = "A", LowerLimit = 0, Rate = 0 },
                    new Data.TaxBand { Id = 2, Band = "B", LowerLimit = 5000, Rate = 20 },
                    new Data.TaxBand { Id = 3, Band = "C", LowerLimit = 20000, Rate = 40 }
                });
                db.SaveChanges();
            }

            return db;
        }

        [Theory]
        [InlineData(10000, 1000, 9000, 833.33, 750.00, 83.33)]
        [InlineData(40000, 11000, 29000, 3333.33, 2416.67, 916.67)]
        public async Task CalculateAsync_ReturnsExpectedTax(
            double grossAnnualSalary, 
            double expectedAnnualTax, 
            double expectedNetAnnual, 
            double expectedGrossMonthly, 
            double expectedNetMonthly,
            double expectedMonthlyTax)
        {
            // Arrange
            var db = CreateInMemoryDb();
            var taxBandService = new TaxBandRepository(db, NullLogger<TaxBandRepository>.Instance);
            var calc = new ProgressiveTaxCalculator(taxBandService, NullLogger<ProgressiveTaxCalculator>.Instance);

            // Act
            var result = await calc.CalculateAsync((decimal)grossAnnualSalary);

            // Assert
            Assert.Equal((decimal)grossAnnualSalary, result.GrossAnnualSalary);
            Assert.Equal((decimal)expectedAnnualTax, result.AnnualTaxPaid);
            Assert.Equal((decimal)expectedNetAnnual, result.NetAnnualSalary);
            Assert.Equal((decimal)expectedGrossMonthly, result.GrossMonthlySalary);
            Assert.Equal((decimal)expectedNetMonthly, result.NetMonthlySalary);
            Assert.Equal((decimal)expectedMonthlyTax, result.MonthlyTaxPaid);
        }
    }
}
