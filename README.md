# Kafa
A fast easy to use csv,tsv file parser. It has a low memory footprint with alot of optimizations to be done.

Kafa is also RFC-4180 Compliant [docs](https://www.rfc-editor.org/rfc/rfc4180)

![Build Status](https://github.com/j0nimost/Kafa/actions/workflows/dotnet.yml/badge.svg)

🚧 UNDER ACTIVE DEVELOPMENT 🚧
### How To Read
There are two options;
- `RowEnumerable`
- `IEnumerable<T>`

#### `RowEnumerable`
For fast performance with minimal over head the RowEnumerable allows you to iterate over the CSV rows yourself.
Example:
```c#
       var stream = new MemoryStream(Encoding.UTF8.GetBytes(sampleCSV));

       using var rows = Kafa.Read(stream);
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
```

This offers you the granular control over each element. The API offers a list of extension Methods for the `Col` struct which allows
you to Parse to any given type; `ParseInt()`, `ParseLong()`, `ParseFloat()`, `ParseDateTime()` etc.

Example:
```c#
       var stream = new MemoryStream(Encoding.UTF8.GetBytes(sampleCSV));

       using var rows = Kafa.Read(stream);
       int sum =0;
       foreach (var row in rows)
       {
            foreach (var col in row.Cols)
            {
                sum += col.ParseInt();
            }
       }
```

To also make it easier you can jump to a specific Row and Col

Example:
```c#
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(sampleCSV));

        using var rows = Kafa.Read(stream);

        var result = rows[0].Cols[0].ToString();
            // or
        var result2 = rows[new Index(0)].Cols[new Index(0)].ToString();
            // or
        var result3 = row[0].Cols["date"].ToString(); // read Column by Name
```
#### `IEnumerable<T>`
Understanding that many users want a simple easy to use experience the library allows
you to parse directly into a type.

Example:
```c#
       var stream = new MemoryStream(Encoding.UTF8.GetBytes(data));
       var result = await Kafa.ReadAsync<CsvData>(stream);
```
To make it easier to parse a file Kafa offers an Attribute Type that can match a column name or the column index.

Example: 
```c#
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
```
### How To Write
You can Write to;
- `IBufferWriter<byte>`
- `ReadOnlySpan<byte>`
- `Stream`
- `File`


Behind the curtains Kafa is using `KafaPooledWriter` a pooled IBufferWriter to write to, unless explicitly provided a custom one using 
`Write<T>(IBufferWriter<byte> bufferWriter, List<T> entities, KafaOptions options = null)` Method.

```c#
            var csvs = new List<CsvData>()
            {
                new CsvData{ Date = DateTime.Parse("10/10/2023 4:08:38 PM"), Open=12.45, Close=12.99, High=13.00, Low=12.1, Name="AMZN", Volume=1233435512},
                new CsvData{ Date = DateTime.Parse("10/10/2023 4:08:38 PM"), Open=12.45, Close=12.99, High=13.00, Low=12.1, Name="AMZN", Volume=1233435512}
            };    
            // get a ReadOnlySpan<bytes>
            var spanOfbytes = await Kafa.Write<CsvData>(csvs);
            string result = Encoding.UTF8.GetString(spanOfbytes);    

            // or 
            // write to an Stream    
            using var stream = await Kafa.WriteToStreamAsync<CsvData>(csvs);  

            // or 
            // Write to a IBufferWriter<byte> for zero allocation writes    
            var arrayWriter = new ArrayBufferWriter<byte>();
            Kafa.Write<CsvData>(arrayWriter, csvs);
            var str = Encoding.UTF8.GetString(arrayWriter.WrittenSpan);    
            
            // or
            // write directly to a file    
            await Kafa.WriteToFileAsync<CsvData>(csvs,@"C:\Users\ADMIN\Documents");

```
Both writing and reading support Attributes
### Support
Only .Net Core 7 and >
### Author
John Nyingi
