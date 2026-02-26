# TaxCalculator

.NET 8 Web API for calculating income tax using configurable tax bands. The project includes unit and integration tests, Serilog file logging, a global error handler and a simple layered layout (API / Application / Domain / Infrastructure).

Quick start
1. Ensure .NET 8 SDK is installed.
2. Restore and build: `dotnet restore` then `dotnet build`.
3. Update `appsettings.json` connection string if using SQL Server or run using the in-memory DB for tests.
4. Run the API: `dotnet run --project TaxCalculator.Api`.

Endpoints
- POST `/api/tax/calculations` — calculate tax. Body: `{ "grossAnnualSalary": 40000 }`.
- GET `/health` — simple JSON health check.

Logging
- Uses Serilog configured in `TaxCalculator/appsettings.json`. By default logs are written to `Logs/log-*.txt` (daily rolling).

Testing
- Unit tests: `dotnet test TaxCalculator.Tests`
- Integration tests: `dotnet test TaxCalculator.IntegrationTests` (uses an in-memory database and seeds tax bands)

Notes
- Strategy pattern is used to select the appropriate tax calculator implementation (zero vs progressive).
- Tax bands are seeded and restored so upper limits are derived from the next band's lower limit; the highest band is open-ended.
