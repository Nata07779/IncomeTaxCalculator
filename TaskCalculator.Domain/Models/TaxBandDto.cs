namespace TaxCalculator.Models
{
    public class TaxBandDto
    {
        public int LowerLimit { get; set; }
        public int? UpperLimit { get; set; }
        public int Rate { get; set; } // percentage
    }
}
