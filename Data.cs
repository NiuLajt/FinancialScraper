namespace FinancialScrapper
{
    internal readonly struct Data(IEnumerable<DataRow> rows)
    {
        public IEnumerable<DataRow> Rows { get; } = rows;
    }
}
