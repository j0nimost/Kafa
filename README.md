# Kafa
A fast easy to use csv,tsv file parser. It has a low memory footprint with alot of optimizations to be done.

🚧 UNDER ACTIVE DEVELOPMENT 🚧
### How To Use
There are two options;
- RowEnumerable
- IEnumerable<T>

#### RowEnumerable 
For fast performance with minimal reflection over head the RowEnumerable allows you to iterate over the CSV rows yourself.
Example:
```c#
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(sampleCSV));

            using var rows = Kafa.Read(stream, ReadEverythingOption);
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

            using var rows = Kafa.Read(stream, ReadEverythingOption);
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
#### IEnumerable<T>
Understanding that many users want a simple easy to use experience like `System.Text.Json`, the library allows
you to parse directly into a type.

Example:
```c#
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(data));

            var result = await Kafa.ReadAsync<CsvData>(stream);
```
### Support
Only .Net Core 7 and >
### Author
John Nyingi