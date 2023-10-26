namespace KafaTests
{
    public class KafaWriteTests
    {

        [Fact]
        public void WriteCSVNoHeader()
        {
            var csvs = new List<CsvData>()
            {
                new CsvData{ Date = DateTime.Parse("10/10/2023 4:09:45 PM"), Open=12.45, Close=12.99, High=13.00, Low=12.1, Name="AMZN", Volume=1233435512},
                new CsvData{ Date = DateTime.Parse("10/10/2023 4:09:45 PM"), Open=12.45, Close=12.99, High=13.00, Low=12.1, Name="AMZN", Volume=1233435512}
            };

            var rowmem = Kafa.Write(csvs, new KafaOptions() { HasHeader = false, Separator=SeparatorFileType.CSV});
            string expected = "";

            if (Environment.OSVersion.Platform == PlatformID.Unix)
            {
                expected = "10/10/2023 16:09:45,12.45,13,12.1,12.99,1233435512,AMZN\n10/10/2023 16:09:45,12.45,13,12.1,12.99,1233435512,AMZN\n";
            }
            else
            {
                expected = "10/10/2023 4:09:45 PM,12.45,13,12.1,12.99,1233435512,AMZN\r\n10/10/2023 4:09:45 PM,12.45,13,12.1,12.99,1233435512,AMZN\r\n";
            }

            var str = Encoding.UTF8.GetString(rowmem);
            Assert.NotNull(str);
            Assert.NotEmpty(str);
            Assert.Equal(expected, str);
        }

        [Fact]
        public void WriteCSVWithDefaultHeader()
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
            var rowmem = Kafa.Write<CsvData>(csvs);
            var str = Encoding.UTF8.GetString(rowmem);
            Assert.NotNull(str);
            Assert.NotEmpty(str);
            Assert.Equal(expected, str);
        }

        [Fact]
        public void WriteCSVWithAttributeHeader()
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
            var rowmem = Kafa.Write<CSVDataWithAttributes>(csvs);
            var str = Encoding.UTF8.GetString(rowmem);
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
            using var stream = await Kafa.WriteToStreamAsync<CsvData>(csvs);
            Assert.NotNull(stream);
        }
    }
}
