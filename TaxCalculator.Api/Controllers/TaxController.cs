using Microsoft.AspNetCore.Mvc;
using TaskCalculator.Domain.Interfaces;
using TaxCalculator.Models;

namespace TaxCalculator.Controllers
{
    [ApiController]
    [Route("api/tax")]
    public class TaxController(IStrategyTaxCalculator calculator, ILogger<TaxController> logger) : ControllerBase
    {
        [HttpPost("calculations")] 
        public async Task<ActionResult<TaxCalculationResult>> Calculate([FromBody] TaxCalculationRequest request)
        {
            if (request == null)
                return BadRequest();

            if (request.GrossAnnualSalary < 0)
                return BadRequest("GrossAnnualSalary must be non-negative");

            logger.LogInformation("Calculating tax for gross annual salary {GrossAnnualSalary}", request.GrossAnnualSalary);
            var result = await calculator.CalculateAsync(request.GrossAnnualSalary);
            logger.LogInformation("Calculation completed for {GrossAnnualSalary}: AnnualTax={AnnualTax}", request.GrossAnnualSalary, result.AnnualTaxPaid);

            return Ok(result);
        }
    }
}
