# Kafa
A fast easy to use csv,tsv file parser. It has a low memory footprint with alot of optimizations to be done.

![Build Status](https://github.com/j0nimost/Kafa/actions/workflows/dotnet.yml/badge.svg)

🚧 UNDER ACTIVE DEVELOPMENT 🚧
### How To Use
There are two options;
- `RowEnumerable`
- `IEnumerable<T>`

#### `RowEnumerable`
For fast performance with minimal reflection over head the RowEnumerable allows you to iterate over the CSV rows yourself.
Example:
```c#
       var stream = new MemoryStream(Encoding.UTF8.GetBytes(sampleCSV));

       using var rows = Kafa.Read(stream);
       int rowCount =0, colCount = 0;
       foreach (var row in rows)
       {
           foreach (var col in row.GetCols())
           {
               Debug.WriteLine(col.ToString());
               colCount++;
           }
           rowCount++;
       }
```

This offers you the granular control over each element. In addition you can also use `Parse<T>()` and `TryParse<T>(out var val)` on Col

Example:
```c#
       var stream = new MemoryStream(Encoding.UTF8.GetBytes(sampleCSV));

       using var rows = Kafa.Read(stream);
       int sum =0;
       foreach (var row in rows)
       {
            foreach (var col in row.Cols)
            {
                sum += col.Parse<int>();
            }
       }
```

To also make it easier you can jump to a specific Row and Col

Example:
```c#
       var stream = new MemoryStream(Encoding.UTF8.GetBytes(sampleCSV));

       using var rows = Kafa.Read(stream);

       var result = rows[0].GetCols()[0].ToString();
            // or
       var result2 = rows[new Index(0)].Cols[new Index(0)].ToString();
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
```
### Support
Only .Net Core 7 and >
### Author
John Nyingi
