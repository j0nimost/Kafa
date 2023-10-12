namespace KafaTests
{
    /// <summary>
    /// CSV Standard https://www.rfc-editor.org/rfc/rfc4180
    /// </summary>
    public class RFC4180Tests
    {
        private KafaOptions ReadEverythingOption => new KafaOptions() { HasHeader = false, FileType = FileType.CSV };

        [Fact]
        public void ReadRowsWithCRLF()
        {
            string csvString = "date,open,high,low,close,volume,Name\r\n2013-02-11,14.89,15.01,14.26,14.46,8882000,AAL\r\n2013-02-12,14.45,14.51,14.1,14.27,8126000,AAL\r\n";
            string expectedRow1 = "2013-02-11,14.89,15.01,14.26,14.46,8882000,AAL";
            string expectedRow2 = "2013-02-12,14.45,14.51,14.1,14.27,8126000,AAL";
            using var rows = Kafa.Read(csvString, ReadEverythingOption);
            Assert.Equal(expectedRow1, rows[1].ToString());
            Assert.Equal(expectedRow2, rows[2].ToString());
        }

        [Fact]
        public void ReadRowsWithNoCRLF()
        {
            string csvString = "date,open,high,low,close,volume,Name\r\n2013-02-11,14.89,15.01,14.26,14.46,8882000,AAL\r\n2013-02-12,14.45,14.51,14.1,14.27,8126000,AAL";
            string expectedRow1 = "2013-02-11,14.89,15.01,14.26,14.46,8882000,AAL";
            string expectedRow2 = "2013-02-12,14.45,14.51,14.1,14.27,8126000,AAL";
            using var rows = Kafa.Read(csvString, ReadEverythingOption);
            Assert.Equal(expectedRow1, rows[1].ToString());
            Assert.Equal(expectedRow2, rows[2].ToString());
        }

        [Fact]
        public void ReadRowsWithHeader()
        {
            string csvString = "date,open,high,low,close,volume,Name\r\n2013-02-11,14.89,15.01,14.26,14.46,8882000,AAL\r\n2013-02-12,14.45,14.51,14.1,14.27,8126000,AAL";
            string expectedRow1 = "date,open,high,low,close,volume,Name";
            string expectedRow2 = "2013-02-11,14.89,15.01,14.26,14.46,8882000,AAL";
            using var rows = Kafa.Read(csvString);
            Assert.Equal(expectedRow1, rows[0].ToString());
            Assert.Equal(expectedRow2, rows[1].ToString());
        }


        [Fact]
        public void ReadRowsWithQuotes()
        {
            string csvString = "date,open,high,low,close,volume,Name\r\n2013-02-11,14.89,15.01,14.26,14.46,8882000,\"AAL\"\r\n2013-02-12,14.45,14.51,14.1,14.27,8126000,\"AAL\"";
            string expectedRow1 = "2013-02-11,14.89,15.01,14.26,14.46,8882000,\"AAL\"";
            string expectedRow2 = "2013-02-12,14.45,14.51,14.1,14.27,8126000,\"AAL\"";
            using var rows = Kafa.Read(csvString, ReadEverythingOption);
            Assert.Equal(expectedRow1, rows[1].ToString());
            Assert.Equal(expectedRow2, rows[2].ToString());
        }

        [Fact]
        public void ReadRowsWithQuotesandCRLF()
        {
            string csvString = "date,open,high,low,close,volume,Name\r\n2013-02-11,14.89,15.01,14.26,14.46,8882000,\"AAL\r\n\"\r\n2013-02-12,14.45,14.51,14.1,14.27,8126000,\"AAL\r\n\"";
            string expectedRow1 = "2013-02-11,14.89,15.01,14.26,14.46,8882000,\"AAL\r\n\"";
            string expectedRow2 = "2013-02-12,14.45,14.51,14.1,14.27,8126000,\"AAL\r\n\"";
            using var rows = Kafa.Read(csvString, ReadEverythingOption);
            Assert.Equal(expectedRow1, rows[1].ToString());
            Assert.Equal(expectedRow2, rows[2].ToString());
        }

        [Fact]
        public void ReadRowsWithDoubleQuotes()
        {
            string csvString = "date,open,high,low,\"close\"\"volume\",Name";

            string expected = "\"close\"\"volume\"";
            using var rows = Kafa.Read(csvString, ReadEverythingOption);
            Assert.Equal(expected, rows[0].Cols[4].ToString());
        }
    }
}
