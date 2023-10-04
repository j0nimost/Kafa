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
    public class KafaReadToTypeTests
    {
            
        private string data = "date,open,high,low,close,volume,name\r\n2013-02-08,15.07,15.12,14.63,14.75,8407500,\"AAL\"";

        [Fact]
        public void ReadToTypeOf()
        {
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(data));

            var rows = Kafa.Read<CsvData>(stream);
            Assert.NotNull(rows);
            Assert.NotEmpty(rows);
            Assert.Equal(1, rows.Count());
        }

        [Fact]
        public async Task ReadToTypeOfAsync()
        {
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(data));

            var rows = await Kafa.ReadAsync<CsvData>(stream);
            Assert.NotNull(rows);
            Assert.NotEmpty(rows);
            Assert.Equal(1, rows.Count());
        }

        [Fact]
        public async Task ReadMultipleRowsToTypeAsync()
        {
            string csv = "date,open,high,low,close,volume,name\r\n2013-02-08,15.07,15.12,14.63,14.75,8407500,AAL\r\n2013-02-11,14.89,15.01,14.26,14.46,8882000,AAL\r\n2013-02-12,14.45,14.51,14.1,14.27,8126000,AAL\r\n";
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv));

            var rows = await Kafa.ReadAsync<CsvData>(stream);
            Assert.NotNull(rows);
            Assert.NotEmpty(rows);
            Assert.Equal(3, rows.Count());

        }

        [Fact]
        public void ReadMultipleRowsFromStringToType()
        {
            string csv = "date,open,high,low,close,volume,name\r\n2013-02-08,15.07,15.12,14.63,14.75,8407500,AAL\r\n2013-02-11,14.89,15.01,14.26,14.46,8882000,AAL\r\n2013-02-12,14.45,14.51,14.1,14.27,8126000,AAL\r\n";

            var rows = Kafa.Read<CsvData>(csv);
            Assert.NotNull(rows);
            Assert.NotEmpty(rows);
            Assert.Equal(3, rows.Count());

        }
    }
}
