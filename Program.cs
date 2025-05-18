using CsvHelper;
using CsvHelper.Configuration;
using FinancialScrapper;
using System.Globalization;

int port = 8080;
int interval = 30; // interval in minutes
string fileName = "financial_markets_dataset.csv";
Console.WriteLine($"Interval: {interval} minutes, Port: {port}\n");
Console.WriteLine("Application running...");

// server for providing generated CSV as file for remote client
FileServer server = new($"http://localhost:{port}/", fileName);
server.Start();

// ensuring that the application is closed correctly with the CTRL+C shortcut
CancellationTokenSource cts = new();
Console.CancelKeyPress += (sender, e) =>
{
    e.Cancel = true;
    Console.WriteLine("\nStopping application...");
    cts.Cancel();
    server.Stop();
};


WebsiteScrapper scrapper = new("https://www.investing.com/markets/");

// repeat scrapping data regularly at specified intervals
while (!cts.Token.IsCancellationRequested)
{
    var data = await scrapper.ExtractValuesAsData();

    var config = new CsvConfiguration(CultureInfo.InvariantCulture)
    {
        HasHeaderRecord = false
    };

    using (FileStream fs = new(fileName, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite))
    using (StreamWriter writer = new(fs))
    using (CsvWriter csv = new(writer, config))
    {
        csv.WriteRecords(data.Rows);
    }

    Console.WriteLine($"{DateTime.Now} Added data to CSV file.");
    await Task.Delay(TimeSpan.FromMinutes(interval));
}