using Microsoft.AspNetCore.Mvc;
using TaxCalculator.Models;
using TaxCalculator.Services;

namespace TaxCalculator.Controllers
{
    [ApiController]
    [Route("api/tax")]
    public class TaxController : ControllerBase
    {
        private readonly ITaxCalculator _calculator;
        private readonly ILogger<TaxController> _logger;

        public TaxController(ITaxCalculator calculator, ILogger<TaxController> logger)
        {
            _calculator = calculator;
            _logger = logger;
        }

        [HttpPost("calculations")] 
        public async Task<ActionResult<TaxCalculationResult>> Calculate([FromBody] TaxCalculationRequest request)
        {
            if (request == null)
                return BadRequest();

            if (request.GrossAnnualSalary < 0)
                return BadRequest("GrossAnnualSalary must be non-negative");

            _logger.LogInformation("Calculating tax for gross annual salary {GrossAnnualSalary}", request.GrossAnnualSalary);
            var result = await _calculator.CalculateAsync(request.GrossAnnualSalary);
            _logger.LogInformation("Calculation completed for {GrossAnnualSalary}: AnnualTax={AnnualTax}", request.GrossAnnualSalary, result.AnnualTaxPaid);

            return Ok(result);
        }
    }
}
