using Kafa;

// See https://aka.ms/new-console-template for more information


FileStream fileStream = new FileStream(@"C:\Users\ADMIN\Documents\Stonks\all_stocks_5yr.csver\all_stocks_5yr.csv", new FileStreamOptions { Access = FileAccess.Read });
var options = new KafaOptions { CultureInfo = System.Globalization.CultureInfo.CurrentCulture, FileType = FileType.CSV };

var rowEnumerable = Kafa.Kafa.Read(fileStream, options);

foreach (var row in rowEnumerable)
{
    Console.Write(row.ToString());
}

Console.WriteLine("Hello, World!");
