using nyingi.Kafa.Reader;
using nyingi.Kafa.Reflection;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Xml;
using static nyingi.Kafa.Reader.KafaReader;

namespace nyingi.Kafa
{
    public static partial class Kafa
    {
        public static RowEnumerable Read(Stream ioStream, KafaOptions? options = null)
        {
            ArgumentNullException.ThrowIfNull(ioStream, nameof(ioStream));
            options = KafaOptions.ResolveKafaOptions(options);
            using TextReader tr = new StreamReader(ioStream, options.Encoding!);

            var readerState = new KafaReadState((int)ioStream.Length, options);

            readerState.ReadState(tr);
            readerState.ProcessBuffer();

            var reader = new KafaReader(readerState);
            return reader.GetRows();
        }

        public static IEnumerable<T> Read<T>(Stream ioStream, KafaOptions? options = null)
        {
            var rowEnumerable = Read(ioStream, options);
            var typeInfo = new KafaTypeInfo(typeof(T), options);
            var reflection = new KafaReflection(typeInfo);
            return reflection.SetProperties<T>(rowEnumerable);
        }

        public static RowEnumerable Read(Stream ioStream, Range rowRange, KafaOptions? options = null)
        {
            ArgumentNullException.ThrowIfNull(ioStream, nameof(ioStream));
            options= KafaOptions.ResolveKafaOptions(options);
            using TextReader tr = new StreamReader(ioStream, options.Encoding!);

            var readerState = new KafaReadState((int)ioStream.Length, options);

            readerState.ReadState(tr);

            var reader = new KafaReader(readerState);
            return reader.GetRows(rowRange);
        }

        public static async ValueTask<RowEnumerable> ReadAsync(Stream ioStream, KafaOptions? options = null, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(ioStream, nameof(ioStream));
            options = KafaOptions.ResolveKafaOptions(options);
            using TextReader tr = new StreamReader(ioStream, options.Encoding!);

            var readerState = new KafaReadState((int)ioStream.Length, options);

            return await ReadProcessorAsync(readerState, tr, cancellationToken);
        }

        public static async ValueTask<IEnumerable<T>> ReadAsync<T>(Stream ioStream, KafaOptions? options = null, CancellationToken cancellationToken = default)
        {
            var rows = await ReadAsync(ioStream, options, cancellationToken);
            var typeInfo = new KafaTypeInfo(typeof(T), options);
            var reflection = new KafaReflection(typeInfo);
            return reflection.SetProperties<T>(rows);
        }

        private static async ValueTask<RowEnumerable> ReadProcessorAsync(KafaReadState kafaReadState, TextReader tr, CancellationToken cancellationToken=default)
        {
            await kafaReadState.ReadStateAsync(tr, cancellationToken); // read first
            kafaReadState.ProcessBuffer(); // process buffer aka Parse

            var reader = new KafaReader(kafaReadState);

            return reader.GetRows();
        }
        
        public static async ValueTask<TextWriter> WriteAsync<T>(List<T> entities, KafaOptions options =null)
        {
            ArgumentNullException.ThrowIfNull(entities, nameof(entities));
            var reflection = SetupOptions<T>(options);
            using var strWriter = new StringWriter(new StringBuilder(4096)); // 4Kb
            return await reflection.GetProperties<T>(entities, strWriter);
        }

        public static async ValueTask<MemoryStream> WriteToStreamAsync<T>(List<T> entities, KafaOptions options = null)
        {
            ArgumentNullException.ThrowIfNull(entities, nameof(entities));
            var reflection = SetupOptions<T>(options);
            var memoryStream = new MemoryStream();
            using var strWriter = new StreamWriter(memoryStream, leaveOpen: true);
            var textStream = await reflection.GetProperties<T>(entities, strWriter);
            textStream.Flush();
            memoryStream.Seek(0, SeekOrigin.Begin);

            return memoryStream;
        }

        public static async ValueTask WriteToFileAsync<T>(List<T> entities, string path, KafaOptions options = null)
        {
            ArgumentNullException.ThrowIfNull(entities, nameof(entities));
            ArgumentNullException.ThrowIfNull(path, nameof(path));
            var reflection = SetupOptions<T>(options);
            using var fs = new FileStream(path, FileMode.Create);
            using var strWriter = new StreamWriter(fs, options.Encoding!, 512);
            await reflection.GetProperties<T>(entities, strWriter);
        }

        private static KafaReflection SetupOptions<T>(KafaOptions options)
        {
            options = KafaOptions.ResolveKafaOptions(options);
            var typeInfo = new KafaTypeInfo(typeof(T), options);
            return new KafaReflection(typeInfo);
        }
    }
}
