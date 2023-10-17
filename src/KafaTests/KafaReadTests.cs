using nyingi.Kafa.Reader;
using System.IO;

namespace KafaTests
{
    public class KafaReadTests
    {
        public static IEnumerable<object[]> GetObjects() => new List<object[]>
        {
            new object[] { "date,open,high,low,close,volume,Name\r\n2013-02-08,15.07,15.12,14.63,14.75,8407500,AAL\r\n2013-02-11,14.89,15.01,14.26,14.46,8882000,AAL\r\n2013-02-12,14.45,14.51,14.1,14.27,8126000,AAL" },
            new object[] { "date,open,high,low,close,volume,Name\n2013-02-08,15.07,15.12,14.63,14.75,8407500,AAL\n2013-02-11,14.89,15.01,14.26,14.46,8882000,AAL\n2013-02-12,14.45,14.51,14.1,14.27,8126000,AAL\n" },
            new object[] { "date,open,high,low,close,volume,Name\n2013-02-08,15.07,15.12,14.63,14.75,8407500,AAL\n2013-02-11,14.89,15.01,14.26,14.46,8882000,AAL\n2013-02-12,14.45,14.51,14.1,14.27,8126000,AAL" },
            new object[] { "date,open,high,low,close,volume,Name\r\n2013-02-08,15.07,15.12,14.63,14.75,8407500,AAL\r\n2013-02-11,14.89,15.01,14.26,14.46,8882000,AAL\r\n2013-02-12,14.45,14.51,14.1,14.27,8126000,AAL\r\n" }
        };

        public static IEnumerable<object[]> GetSpecialStyles() => new List<object[]>
        {
            new object[] { "date,open,high,low,close,volume,Name\r\n2013-02-08,15.07,15.12,14.63,14.75,8407500,\"AAL\"" }
        };

        public static IEnumerable<object[]> GetDifferentRows() => new List<object[]>
        {
            new object[]{ "date,open,high,low,close,volume,Name\r\n2013-02-08,15.07,15.12,14.63,14.75,8407500,\"AAL\"" },
            new object[]{ "date,open,high,low,close,volume,Name\n2013-02-08,15.07,15.12,14.63,14.75,8407500,\"AAL\"\n2013-02-12,14.45,14.51,14.1,14.27,8126000,AAL" },
        };

        private KafaOptions ReadEverythingOption => new KafaOptions() { HasHeader = false, Separator = SeparatorFileType.CSV};

        [Theory]
        [MemberData(nameof(GetDifferentRows))]
        public void ReadRow(string rowString)
        {
            string expectedHeader = "date,open,high,low,close,volume,Name";
            string expectedRow = "2013-02-08,15.07,15.12,14.63,14.75,8407500,\"AAL\"";
            using var rows = Kafa.Read(rowString, ReadEverythingOption);
            Assert.Equal(expectedHeader, rows[0].ToString());
            Assert.Equal(expectedRow, rows[1].ToString());
        }

        [Theory]
        [MemberData(nameof(GetObjects))]
        public void ReadRowStream(string sampleCSV)
        {
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(sampleCSV));

            using var rows = Kafa.Read(stream, ReadEverythingOption);
            Assert.NotEmpty(rows);
            int count = 0;
            foreach (var row in rows)
            {

                Debug.Write(row);
                count++;
            }
            Assert.Equal(4, count);
        }

        [Theory]
        [MemberData(nameof(GetObjects))]
        public void ReadColStream(string sampleCSV)
        {
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(sampleCSV));

            using var rows = Kafa.Read(stream, ReadEverythingOption);
            Assert.NotEmpty(rows);
            int rowCount =0, colCount = 0;
            foreach (var row in rows)
            {
                foreach (var col in row.Cols)
                {
                    Debug.WriteLine(col.ToString());
                    colCount++;
                }
                rowCount++;
            }
            Assert.Equal(4, rowCount);
            Assert.Equal(28, colCount);
        }

        [Theory]
        [MemberData(nameof(GetObjects))]
        public void ReadColString(string sampleCSV)
        {
            using var rows = Kafa.Read(sampleCSV, ReadEverythingOption);
            Assert.NotEmpty(rows);
            int rowCount = 0, colCount = 0;
            foreach (var row in rows)
            {
                foreach (var col in row.Cols)
                {
                    Debug.WriteLine(col.ToString());
                    colCount++;
                }
                rowCount++;
            }
            Assert.Equal(4, rowCount);
            Assert.Equal(28, colCount);
        }



        [Theory]
        [MemberData(nameof(GetObjects))]
        public void ReadStream(string sampleCSV)
        {
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(sampleCSV));
            using var rows = Kafa.Read(stream);
            Assert.NotEmpty(rows);
        }

        [Theory]
        [MemberData(nameof(GetObjects))]
        public async Task ReadStreamAsync(string sampleCSV)
        {
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(sampleCSV));
            using var rows = await Kafa.ReadAsync(stream);
            Assert.NotEmpty(rows);
        }

        [Theory]
        [MemberData(nameof(GetObjects))]
        public void ReadRowByIntIndex(string sampleCSV)
        {
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(sampleCSV));

            var rows = Kafa.Read(stream);
            Assert.NotEmpty(rows);
            Assert.NotEmpty(rows[0].ToString());
        }

        [Theory]
        [MemberData(nameof(GetObjects))]
        public void ReadRowByIndexStruct(string sampleCSV)
        {
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(sampleCSV));

            using var rows = Kafa.Read(stream);
            Assert.NotEmpty(rows);
            Assert.NotEmpty(rows[new Index(0)].ToString());
        }

        [Theory]
        [MemberData(nameof(GetObjects))]
        public void ReadColByIntIndex(string sampleCSV)
        {
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(sampleCSV));

            using var rows = Kafa.Read(stream);
            Assert.NotEmpty(rows);
            Assert.NotEmpty(rows[0].Cols[0].ToString());
            Assert.Equal("date", rows[0].Cols[0].ToString());
            Assert.Equal("2013-02-08", rows[1].Cols[0].ToString());
            Assert.Equal("15.12", rows[1].Cols[2].ToString());
            Assert.Equal("AAL", rows[3].Cols[6].ToString());
            Assert.Equal("AAL", rows[2].Cols[6].ToString());

        }

        [Theory]
        [MemberData(nameof(GetObjects))]
        public void ReadColByIndexStruct(string sampleCSV)
        {
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(sampleCSV));

            using var rows = Kafa.Read(stream);
            Assert.NotEmpty(rows);
            Assert.Equal("date", rows[new Index(0)].Cols[new Index(0)].ToString());
            Assert.Equal("2013-02-08", rows[new Index(1)].Cols[new Index(0)].ToString());
            Assert.Equal("15.12", rows[new Index(1)].Cols[new Index(2)].ToString());
            Assert.Equal("AAL", rows[new Index(3)].Cols[new Index(6)].ToString());
            Assert.Equal("AAL", rows[new Index(2)].Cols[new Index(6)].ToString());
        }

        [Theory]
        [MemberData(nameof(GetObjects))]
        public void ReadRowOutOfRange(string sampleCSV)
        {
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(sampleCSV));
            using var rows = Kafa.Read(stream);
            Assert.NotEmpty(rows);
            Assert.Throws<KafaException>(() => rows[-1].ToString());
        }

        [Theory]
        [MemberData(nameof(GetObjects))]
        public void ReadColOutOfRange(string sampleCSV)
        {
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(sampleCSV));
            using var rows = Kafa.Read(stream);
            Assert.NotEmpty(rows);
            Assert.Throws<KafaException>(() => rows[0].Cols[-1].ToString());
            Assert.Throws<KafaException>(() => rows[0].Cols[-1].ToString());
        }

        [Fact]
        public void ReadUnequalRowSize()
        {
            string inputCsv = "date,open,high,low,close,volume,Name\r\n2013-02-08,15.07,UwU,15.12,14.63,14.75,8407500,AAL";
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(inputCsv));
            Assert.Throws<KafaException>(() =>
            {
                using var rows = Kafa.Read(stream);
            });
        }

        [Fact]
        public void ReadRowWithHeader()
        {
            string inputCsv = "date,open,high,low,close,volume,Name\r\n2013-02-08,15.07,15.12,14.63,14.75,8407500,AAL";
            using var ioStream = new MemoryStream(Encoding.UTF8.GetBytes(inputCsv));
            using TextReader tr = new StreamReader(ioStream);
            KafaOptions kafaOptions = new KafaOptions()
            {
                CultureInfo = System.Globalization.CultureInfo.CurrentCulture,
                HasHeader = true,
                Separator = SeparatorFileType.CSV
            };

            using var kafaReaderState = new KafaReadState((int)ioStream.Length, kafaOptions);
            kafaReaderState.ReadState(tr);
            Assert.NotEmpty(kafaReaderState.Buffer);
            Assert.NotEmpty(kafaReaderState.ColMarker);
            kafaReaderState.ProcessBuffer();
            Assert.NotEmpty(kafaReaderState.Headers);
            Assert.NotEqual(0, kafaReaderState.RowCount);
            Assert.NotEqual(0, kafaReaderState.ColCount);
            Assert.NotEqual(0, kafaReaderState.ColMarkerLength);
            Assert.NotEqual(0, kafaReaderState.OffSet);

        }

        [Fact]
        public void ReadRowWithoutHeader()
        {
            string inputCsv = "date,open,high,low,close,volume,Name\r\n2013-02-08,15.07,15.12,14.63,14.75,8407500,AAL";
            using var ioStream = new MemoryStream(Encoding.UTF8.GetBytes(inputCsv));
            using TextReader tr = new StreamReader(ioStream);
            KafaOptions kafaOptions = new KafaOptions()
            {
                CultureInfo = System.Globalization.CultureInfo.CurrentCulture,
                HasHeader = false,
                Separator = SeparatorFileType.CSV

            };

            using var kafaReaderState = new KafaReadState((int)ioStream.Length, kafaOptions);
            kafaReaderState.ReadState(tr);
            Assert.NotEmpty(kafaReaderState.Buffer);
            Assert.NotEmpty(kafaReaderState.ColMarker);
            kafaReaderState.ProcessBuffer();
            Assert.Null(kafaReaderState.Headers);
            Assert.NotEqual(0, kafaReaderState.RowCount);
            Assert.NotEqual(0, kafaReaderState.ColCount);
            Assert.NotEqual(0, kafaReaderState.ColMarkerLength);
            Assert.Equal(0, kafaReaderState.OffSet);

        }

        [Theory]
        [MemberData(nameof(GetSpecialStyles))]
        public void ReadSpecialSeparator(string specialStyle)
        {
            using var ioStream = new MemoryStream(Encoding.UTF8.GetBytes(specialStyle));
            using var rows = Kafa.Read(ioStream);
            Assert.NotEmpty(rows);
            Assert.Equal("\"AAL\"", rows[1].Cols[6].ToString());

        }


        [Fact]
        public void ReadUnicode()
        {
            string unicodeCsv = "2013-02-08,15.07 €,15.12 ¥,14.63 ¥,14.75 ¥,8407500,\"AAL✅\"";
            using var rows = Kafa.Read(unicodeCsv, ReadEverythingOption);
            Assert.NotEmpty(rows);
            Assert.Equal("15.07 €", rows[0].Cols[1].ToString());
            Assert.Equal("14.75 ¥", rows[0].Cols[4].ToString());
            Assert.Equal("\"AAL✅\"", rows[0].Cols[6].ToString());


        }
    }
}