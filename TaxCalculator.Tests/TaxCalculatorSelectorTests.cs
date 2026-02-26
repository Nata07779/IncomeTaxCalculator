using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using TaxCalculator.Models;
using TaxCalculator.Services;
using Xunit;

namespace TaxCalculator.Tests
{
    public class TaxCalculatorSelectorTests
    {
        private class SimpleServiceProvider : IServiceProvider
        {
            private readonly Dictionary<Type, object> _map;
            public SimpleServiceProvider(Dictionary<Type, object> map) => _map = map;
            public object? GetService(Type serviceType) => _map.TryGetValue(serviceType, out var v) ? v : null;
        }

        private class FakeRepo : ITaxBandRepository
        {
            public Task<IEnumerable<TaxBandDto>> GetTaxBandsAsync()
            {
                return Task.FromResult((IEnumerable<TaxBandDto>)Array.Empty<TaxBandDto>());
            }
        }

        [Fact]
        public void Select_ReturnsPlainCalculator_ForZeroSalary()
        {
            // Arrange
            var plain = new ZeroTaxCalculator();
            var progressive = new ProgressiveTaxCalculator(new FakeRepo(), NullLogger<ProgressiveTaxCalculator>.Instance);
            var map = new Dictionary<Type, object>
            {
                { typeof(ZeroTaxCalculator), plain },
                { typeof(ProgressiveTaxCalculator), progressive }
            };
            var sp = new SimpleServiceProvider(map);
            var selector = new TaxCalculatorSelector(sp);

            // Act
            var calc = selector.Select(0m);

            // Assert
            Assert.IsType<ZeroTaxCalculator>(calc);
        }

        [Fact]
        public void Select_ReturnsProgressiveCalculator_ForNonZeroSalary()
        {
            // Arrange
            var plain = new ZeroTaxCalculator();
            var progressive = new ProgressiveTaxCalculator(new FakeRepo(), NullLogger<ProgressiveTaxCalculator>.Instance);
            var map = new Dictionary<Type, object>
            {
                { typeof(ZeroTaxCalculator), plain },
                { typeof(ProgressiveTaxCalculator), progressive }
            };
            var sp = new SimpleServiceProvider(map);
            var selector = new TaxCalculatorSelector(sp);

            // Act
            var calc = selector.Select(100m);

            // Assert
            Assert.IsType<ProgressiveTaxCalculator>(calc);
        }
    }
}
