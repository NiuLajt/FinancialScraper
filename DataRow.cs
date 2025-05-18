namespace FinancialScrapper
{
    internal record DataRow
    {
        public string? Name { get; }
        public double? Last { get; }
        public double? Change { get; }
        public double? ChangePercentile { get; }
        public DateTime? MeasurementDate { get; }

        public DataRow(string? name, double? last, double? change, double? changePercentile)
        {
            Name = name;
            Last = last;
            Change = change;
            ChangePercentile = changePercentile;
            MeasurementDate = DateTime.Now;
        }
    }
}