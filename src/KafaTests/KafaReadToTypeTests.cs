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
        [KafaColumn("date")]
        public DateTime Date { get; set; }
        [KafaColumn(1)]
        public double Open { get; set; }
        [KafaColumn(2)]
        public double High { get; set; }
        [KafaColumn(3)]
        public double Low { get; set; }
        [KafaColumn(4)]
        public double Close { get; set; }
        [KafaColumn(5)]
        public int Volume { get; set; }
        [KafaColumn("name")]
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

        [Fact]
        public async Task WriteCSVNoHeaderAsync()
        {
            var csvs = new List<CsvData>()
            {
                new CsvData{ Date = DateTime.Parse("10/10/2023 4:09:45 PM"), Open=12.45, Close=12.99, High=13.00, Low=12.1, Name="AMZN", Volume=1233435512},
                new CsvData{ Date = DateTime.Parse("10/10/2023 4:09:45 PM"), Open=12.45, Close=12.99, High=13.00, Low=12.1, Name="AMZN", Volume=1233435512}
            };

            var rowmem = await Kafa.WriteAsync<CsvData>(csvs, new KafaOptions() { HasHeader=false, FileType= FileType.CSV});
            string expected = "";

            if(Environment.OSVersion.Platform == PlatformID.Unix)
            {
                expected = "10/10/2023 16:09:45,12.45,13,12.1,12.99,1233435512,AMZN\n10/10/2023 16:09:45,12.45,13,12.1,12.99,1233435512,AMZN\n";
            }
            else
            {
                expected = "10/10/2023 4:09:45 PM,12.45,13,12.1,12.99,1233435512,AMZN\r\n10/10/2023 4:09:45 PM,12.45,13,12.1,12.99,1233435512,AMZN\r\n";
            }
            var str = rowmem.ToString();
            Assert.NotNull(str);
            Assert.NotEmpty(str);
            Assert.Equal(expected, str);
        }

        [Fact]
        public async Task WriteCSVWithHeaderAsync()
        {
            var csvs = new List<CsvData>()
            {
                new CsvData{ Date = DateTime.Parse("10/10/2023 4:08:38 PM"), Open=12.45, Close=12.99, High=13.00, Low=12.1, Name="AMZN", Volume=1233435512},
                new CsvData{ Date = DateTime.Parse("10/10/2023 4:08:38 PM"), Open=12.45, Close=12.99, High=13.00, Low=12.1, Name="AMZN", Volume=1233435512}
            };
            string expected = "";

            if (Environment.OSVersion.Platform == PlatformID.Unix)
            {
                expected = "Date,Open,High,Low,Close,Volume,Name\n10/10/2023 16:09:45,12.45,13,12.1,12.99,1233435512,AMZN\n10/10/2023 16:09:45,12.45,13,12.1,12.99,1233435512,AMZN\n";
            }
            else
            {
                expected = "Date,Open,High,Low,Close,Volume,Name\r\n10/10/2023 4:08:38 PM,12.45,13,12.1,12.99,1233435512,AMZN\r\n10/10/2023 4:08:38 PM,12.45,13,12.1,12.99,1233435512,AMZN\r\n";
            }
            var rowmem = await Kafa.WriteAsync<CsvData>(csvs);
            var str = rowmem.ToString();
            Assert.NotNull(str);
            Assert.NotEmpty(str);
            Assert.Equal(expected, str);
        }

        [Fact]
        public async Task WriteCSVToStreamAsync()
        {
            var csvs = new List<CsvData>()
            {
                new CsvData{ Date = DateTime.Parse("10/10/2023 4:08:38 PM"), Open=12.45, Close=12.99, High=13.00, Low=12.1, Name="AMZN", Volume=1233435512},
                new CsvData{ Date = DateTime.Parse("10/10/2023 4:08:38 PM"), Open=12.45, Close=12.99, High=13.00, Low=12.1, Name="AMZN", Volume=1233435512}
            };
            string expected = "";

            if(Environment.OSVersion.Platform == PlatformID.Unix)
            {
                expected = "Date,Open,High,Low,Close,Volume,Name\n10/10/2023 16:08:38,12.45,13,12.1,12.99,1233435512,AMZN\n10/10/2023 16:08:38,12.45,13,12.1,12.99,1233435512,AMZN\n";
            }
            else
            {
                expected = "Date,Open,High,Low,Close,Volume,Name\r\n10/10/2023 4:08:38 PM,12.45,13,12.1,12.99,1233435512,AMZN\r\n10/10/2023 4:08:38 PM,12.45,13,12.1,12.99,1233435512,AMZN\r\n";
            }
            using var stream = await Kafa.WriteToStreamAsync<CsvData>(csvs);
            Assert.NotNull(stream);
             
            var result = Encoding.UTF8.GetString(stream.GetBuffer(), 0, (int)stream.Length);
            Assert.Equal(expected, result);
        }
    }
}
