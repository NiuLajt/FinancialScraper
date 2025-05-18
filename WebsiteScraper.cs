using HtmlAgilityPack;
using System.Globalization;

namespace FinancialScrapper
{
    internal class WebsiteScraper(string url)
    {
        private readonly string _url = url;

        private async Task<string> GetRawHTML()
        {
            using HttpClient client = new();
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64)"); // fake user-agent for bypassing anti-scrap (403 FORBIDDEN)
            var html = await client.GetStringAsync(_url);
            return html;
        }

        // HTML source file is HUGE. This method is used for extracting code containing important data.
        private string Trim(string html)
        {
            HtmlDocument document = new();
            document.LoadHtml(html);
            var node = document.DocumentNode.SelectSingleNode("//table[@id='market_overview_default']");

            if (node is null) throw new NullReferenceException();

            return node.OuterHtml;
        }

        public async Task<Data> ExtractValuesAsData()
        {
            var rawHtml = await GetRawHTML();
            var table = Trim(rawHtml);

            HtmlDocument document = new();
            document.LoadHtml(table);
            var rows = document.DocumentNode.SelectNodes("//tbody/tr");

            if (rows is null) throw new NullReferenceException();

            List<DataRow> dataRows = [];

            foreach(var row in rows)
            {
                string name = row.SelectSingleNode(".//td[contains(@id, 'pair_name')]")?.InnerText.Trim() ?? "NULL";
                double last = TryParseDouble(row.SelectSingleNode(".//td[contains(@id, 'last')]")?.InnerText.Trim() ?? "2137");
                double change = TryParseDouble(row.SelectSingleNode(".//td[contains(@id, 'chg')]")?.InnerText.Trim() ?? "2137");
                double changePercentile = TryParseDouble(row.SelectSingleNode(".//td[contains(@id, 'chg_percent')]")?.InnerText.Trim() ?? "2137");

                DataRow dataRow = new(name, last, change, changePercentile);
                dataRows.Add(dataRow);
            }

            return new Data(dataRows);
        }

        // helper method
        private static double TryParseDouble(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return 0.0;
            value = value.Replace("%", "").Replace(",", ""); // remove unnecessary characters
            return double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out double result) ? result : 0.0;
        }
    }
}