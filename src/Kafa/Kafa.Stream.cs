using nyingi.Kafa.Reader;
using nyingi.Kafa.Reflection;
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
            var reflection = new KafaReflection(typeInfo, rowEnumerable.Headers);
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
            var reflection = new KafaReflection(typeInfo, rows.Headers);
            return reflection.SetProperties<T>(rows);
        }

        private static async ValueTask<RowEnumerable> ReadProcessorAsync(KafaReadState kafaReadState, TextReader tr, CancellationToken cancellationToken=default)
        {
            await kafaReadState.ReadStateAsync(tr, cancellationToken); // read first
            kafaReadState.ProcessBuffer(); // process buffer aka Parse

            var reader = new KafaReader(kafaReadState);

            return reader.GetRows();
        }
    }
}
