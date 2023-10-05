namespace KafaTests
{
    class CsvData
    {
        public DateTime Date { get; set; }
        public double Open { get; set; }
        public double High { get; set; }
        public double Low { get; set; }
        public double Close { get; set; }
        public int Volume { get; set; }
        public string Name { get; set; }
    }

    class CSVDataWithAttributes
    {
        [Kafa("date")]
        public DateTime Date { get; set; }
        [Kafa(1)]
        public double Open { get; set; }
        [Kafa(2)]
        public double High { get; set; }
        [Kafa(3)]
        public double Low { get; set; }
        [Kafa(4)]
        public double Close { get; set; }
        [Kafa(5)]
        public int Volume { get; set; }
        [Kafa("name")]
        public string Name { get; set; }
    }
    public class KafaReadToTypeTests
    {
            
        private string dataMatchingCase = "Date,Open,High,Low,Close,Volume,Name\r\n2013-02-08,15.07,15.12,14.63,14.75,8407500,\"AAL\"";

        [Fact]
        public void ReadFromStreamToTypeOf()
        {
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(dataMatchingCase));

            var rows = Kafa.Read<CsvData>(stream);
            Assert.NotNull(rows);
            Assert.NotEmpty(rows);
        }

        [Fact]
        public async Task ReadFromStreamToTypeOfAsync()
        {
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(dataMatchingCase));

            var rows = await Kafa.ReadAsync<CsvData>(stream);
            Assert.NotNull(rows);
            Assert.NotEmpty(rows);
        }
        [Fact]
        public void ReadMultipleRowsFromStringToType()
        {
            string csv = "Date,Open,High,Low,Close,Volume,Name\r\n2013-02-08,15.07,15.12,14.63,14.75,8407500,AAL\r\n2013-02-11,14.89,15.01,14.26,14.46,8882000,AAL\r\n2013-02-12,14.45,14.51,14.1,14.27,8126000,AAL\r\n";

            var rows = Kafa.Read<CsvData>(csv);
            Assert.NotNull(rows);
            Assert.NotEmpty(rows);
            Assert.Equal(3, rows.Count());

        }
        [Fact]
        public async Task ReadMultipleRowsToTypeOfAsync()
        {
            string csv = "Date,Open,High,Low,Close,Volume,Name\r\n2013-02-08,15.07,15.12,14.63,14.75,8407500,AAL\r\n2013-02-11,14.89,15.01,14.26,14.46,8882000,AAL\r\n2013-02-12,14.45,14.51,14.1,14.27,8126000,AAL\r\n";
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv));

            var rows = await Kafa.ReadAsync<CsvData>(stream);
            Assert.NotNull(rows);
            Assert.NotEmpty(rows);
            Assert.Equal(3, rows.Count());

        }

        [Fact]
        public void  ReadToTypeWithAttributes()
        {
            string csv = "date,open,High,Low,Close,Total,name\r\n2013-02-08,15.07,15.12,14.63,14.75,8407500,AAL\r\n2013-02-11,14.89,15.01,14.26,14.46,8882000,AAL\r\n2013-02-12,14.45,14.51,14.1,14.27,8126000,AAL\r\n";

            var rows = Kafa.Read<CSVDataWithAttributes>(csv);
            Assert.NotNull(rows);
            Assert.NotEmpty(rows);
            foreach (var row in rows)
            {
                Assert.NotEqual(DateTime.MinValue, row.Date);
                Assert.NotEqual(default, row.Open);
                Assert.NotEqual(default, row.High);
                Assert.NotEqual(default, row.Low);
                Assert.NotEqual(default, row.Close);
                Assert.NotEqual(default, row.Volume);
                Assert.False(string.IsNullOrEmpty(row.Name));
            }
        }

    }
}
