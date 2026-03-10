using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using TaxCalculator.Data;
using TaxCalculator.Models;
using Xunit;

namespace TaxCalculator.IntegrationTests
{
    public class TaxApiTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public TaxApiTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Replace AppDbContext with InMemory for tests
                    var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                    if (descriptor != null)
                    {
                        services.Remove(descriptor);
                    }

                    services.AddDbContext<AppDbContext>(options =>
                    {
                        options.UseInMemoryDatabase("IntegrationTestDb");
                    });

                    // Seed tax bands similar to unit tests
                    var sp = services.BuildServiceProvider();
                    using (var scope = sp.CreateScope())
                    {
                        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                        if (!db.TaxBands.Any())
                        {
                            db.TaxBands.AddRange(new[] {
                                new TaxBand { Id = 1, Band = "A", LowerLimit = 0, Rate = 0 },
                                new TaxBand { Id = 2, Band = "B", LowerLimit = 5000, Rate = 20 },
                                new TaxBand { Id = 3, Band = "C", LowerLimit = 20000, Rate = 40 }
                            });
                            db.SaveChanges();
                        }
                    }
                });

                builder.ConfigureAppConfiguration((context, cfg) =>
                {
                    cfg.Sources.Clear();
                    var testConfig = new Dictionary<string, string>
                        {
                            { "Jwt:Key", "MOCKED-VALUE" },
                            { "Jwt:Issuer", "MOCKED-VALUE" },
                            { "Jwt:Audience", "MOCKED-VALUE" }
                        };

                    cfg.AddInMemoryCollection(testConfig);
                });
            });
        }

        [Fact]
        public async Task PostCalculations_ReturnsOk()
        {
            var client = _factory.CreateClient();

            var request = new TaxCalculationRequest { GrossAnnualSalary = 40000m };
            var resp = await client.PostAsJsonAsync("/api/tax/calculations", request);

            Assert.Equal(HttpStatusCode.OK, resp.StatusCode);

            var result = await resp.Content.ReadFromJsonAsync<TaxCalculationResult>();
            Assert.NotNull(result);
            Assert.Equal(11000m, result.AnnualTaxPaid);
        }
    }
}
