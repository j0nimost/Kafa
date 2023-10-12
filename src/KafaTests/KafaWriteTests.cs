namespace KafaTests
{
    public class KafaWriteTests
    {

        [Fact]
        public async Task WriteCSVNoHeaderAsync()
        {
            var csvs = new List<CsvData>()
            {
                new CsvData{ Date = DateTime.Parse("10/10/2023 4:09:45 PM"), Open=12.45, Close=12.99, High=13.00, Low=12.1, Name="AMZN", Volume=1233435512},
                new CsvData{ Date = DateTime.Parse("10/10/2023 4:09:45 PM"), Open=12.45, Close=12.99, High=13.00, Low=12.1, Name="AMZN", Volume=1233435512}
            };

            var rowmem = await Kafa.WriteAsync<CsvData>(csvs, new KafaOptions() { HasHeader = false, FileType = FileType.CSV });
            string expected = "";

            if (Environment.OSVersion.Platform == PlatformID.Unix)
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
        public async Task WriteCSVWithDefaultHeaderAsync()
        {
            var csvs = new List<CsvData>()
            {
                new CsvData{ Date = DateTime.Parse("10/10/2023 4:08:38 PM"), Open=12.45, Close=12.99, High=13.00, Low=12.1, Name="AMZN", Volume=1233435512},
                new CsvData{ Date = DateTime.Parse("10/10/2023 4:08:38 PM"), Open=12.45, Close=12.99, High=13.00, Low=12.1, Name="AMZN", Volume=1233435512}
            };
            string expected = "";

            if (Environment.OSVersion.Platform == PlatformID.Unix)
            {
                expected = "Date,Open,High,Low,Close,Volume,Name\n10/10/2023 16:08:38,12.45,13,12.1,12.99,1233435512,AMZN\n10/10/2023 16:08:38,12.45,13,12.1,12.99,1233435512,AMZN\n";
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
        public async Task WriteCSVWithAttributeHeaderAsync()
        {
            var csvs = new List<CSVDataWithAttributes>()
            {
                new CSVDataWithAttributes{ Date = DateTime.Parse("10/10/2023 4:08:38 PM"), Open=12.45, Close=12.99, High=13.00, Low=12.1, Name="AMZN", Volume=1233435512},
                new CSVDataWithAttributes{ Date = DateTime.Parse("10/10/2023 4:08:38 PM"), Open=12.45, Close=12.99, High=13.00, Low=12.1, Name="AMZN", Volume=1233435512}
            };
            string expected = "";

            if (Environment.OSVersion.Platform == PlatformID.Unix)
            {
                expected = "date,Open,High,Low,Close,Volume,name\n10/10/2023 16:08:38,12.45,13,12.1,12.99,1233435512,AMZN\n10/10/2023 16:08:38,12.45,13,12.1,12.99,1233435512,AMZN\n";
            }
            else
            {
                expected = "date,Open,High,Low,Close,Volume,name\r\n10/10/2023 4:08:38 PM,12.45,13,12.1,12.99,1233435512,AMZN\r\n10/10/2023 4:08:38 PM,12.45,13,12.1,12.99,1233435512,AMZN\r\n";
            }
            var rowmem = await Kafa.WriteAsync<CSVDataWithAttributes>(csvs);
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

            if (Environment.OSVersion.Platform == PlatformID.Unix)
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
