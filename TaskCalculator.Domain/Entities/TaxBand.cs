namespace TaxCalculator.Data
{
    public class TaxBand
    {
        public int Id { get; set; }
        public string Band { get; set; } = string.Empty;
        public int LowerLimit { get; set; }
        public int? UpperLimit { get; set; }
        public int Rate { get; set; } // percentage
    }
}
