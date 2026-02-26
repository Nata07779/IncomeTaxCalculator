using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TaxCalculator.Data;
using TaxCalculator.Models;

namespace TaxCalculator.Services
{
    public class TaxBandRepository : ITaxBandRepository
    {
        private readonly AppDbContext _db;

        private readonly ILogger<TaxBandRepository> _logger;

        public TaxBandRepository(AppDbContext db, ILogger<TaxBandRepository> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<IEnumerable<TaxBandDto>> GetTaxBandsAsync()
        {
            var bands = await _db.TaxBands
               .OrderBy(b => b.LowerLimit)
               .Select(b => new TaxBand { LowerLimit = b.LowerLimit, UpperLimit = b.UpperLimit, Rate = b.Rate })
               .ToListAsync();

            var orderedDescBands = bands.OrderByDescending(b => b.LowerLimit).ToList();

            var restoredBands = new List<TaxBandDto>();
            TaxBand? previousBand = null;

            for (int i = 0; i < orderedDescBands.Count; i++)
            {
                var band = orderedDescBands[i];

                var restoredBand = new TaxBandDto
                {
                    LowerLimit = band.LowerLimit,
                    UpperLimit = band.UpperLimit ?? (previousBand?.LowerLimit ?? int.MaxValue),
                    Rate = band.Rate
                };

                previousBand = band;
                restoredBands.Add(restoredBand);
            }

            _logger.LogDebug("Returning {Count} restored tax bands", restoredBands.Count);
            return restoredBands;
        }
    }
}
