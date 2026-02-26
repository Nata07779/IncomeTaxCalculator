using Microsoft.Extensions.Logging;
using TaxCalculator.Models;

namespace TaxCalculator.Services
{
    // Strategy for progressive tax calculation
    public class ProgressiveTaxCalculator : ITaxCalculator
    {
        private readonly ITaxBandRepository _taxBandRepository;
        private readonly ILogger<ProgressiveTaxCalculator> _logger;

        public ProgressiveTaxCalculator(ITaxBandRepository taxBandRepository, ILogger<ProgressiveTaxCalculator> logger)
        {
            _taxBandRepository = taxBandRepository;
            _logger = logger;
        }

        // Calculate tax using progressive bands. Financial specifics: round to 2 decimal places using MidpointRounding.ToEven
        public async Task<TaxCalculationResult> CalculateAsync(decimal grossAnnualSalary)
        {
            var restoredBands = await _taxBandRepository.GetTaxBandsAsync();
            decimal remaining = grossAnnualSalary;
            decimal taxTotal = 0m;

            _logger.LogDebug("Loaded {BandCount} tax bands", restoredBands.Count());
            var orderedBands = restoredBands.OrderBy(b => b.LowerLimit).ToList();

            for (int i = 0; i < orderedBands.Count; i++)
            {
                var band = orderedBands[i];
                if(band.UpperLimit == null)
                {
                    throw new InvalidOperationException($"Band with LowerLimit {band.LowerLimit} has null UpperLimit after restoration.");
                }

                decimal bandLower = band.LowerLimit;
                decimal bandUpper = band.UpperLimit.Value;

                decimal taxableInBand = Math.Max(0, 
                    Math.Min((decimal)bandUpper - bandLower, remaining - Math.Max(0, bandLower))
                    );

                if (taxableInBand <= 0)
                    continue;

                decimal taxForBand = taxableInBand * band.Rate / 100m;
                _logger.LogDebug("Band {Lower}-{Upper} rate {Rate}% taxable {Taxable} tax {TaxForBand}", band.LowerLimit, band.UpperLimit, band.Rate, taxableInBand, taxForBand);
                taxTotal += taxForBand;
            }

            taxTotal = decimal.Round(taxTotal, 2, MidpointRounding.ToEven);

            var result = new TaxCalculationResult
            {
                GrossAnnualSalary = decimal.Round(grossAnnualSalary, 2, MidpointRounding.ToEven),
                GrossMonthlySalary = decimal.Round(grossAnnualSalary / 12m, 2, MidpointRounding.ToEven),
                AnnualTaxPaid = taxTotal,
                MonthlyTaxPaid = decimal.Round(taxTotal / 12m, 2, MidpointRounding.ToEven),
                NetAnnualSalary = decimal.Round(grossAnnualSalary - taxTotal, 2, MidpointRounding.ToEven)
            };
            result.NetMonthlySalary = decimal.Round(result.NetAnnualSalary / 12m, 2, MidpointRounding.ToEven);

            _logger.LogInformation("Computed tax for GrossAnnual {Gross}: AnnualTax {Tax}", grossAnnualSalary, result.AnnualTaxPaid);

            return result;
        }
    }
}
