using System.Buffers;
using nyingi.Kafa.Reader;
using nyingi.Kafa.Reflection;
using nyingi.Kafa.Writer;
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
            var reflection = SetupOptions<T>(options);
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

            return await ReadProcessorAsync(readerState, tr, cancellationToken).ConfigureAwait(false);
        }

        public static async ValueTask<IEnumerable<T>> ReadAsync<T>(Stream ioStream, KafaOptions? options = null, CancellationToken cancellationToken = default)
        {
            var rows = await ReadAsync(ioStream, options, cancellationToken).ConfigureAwait(false);
            var reflection = SetupOptions<T>(options);
            return reflection.SetProperties<T>(rows);
        }

        private static async ValueTask<RowEnumerable> ReadProcessorAsync(KafaReadState kafaReadState, TextReader tr, CancellationToken cancellationToken=default)
        {
            await kafaReadState.ReadStateAsync(tr, cancellationToken).ConfigureAwait(false); // read first
            kafaReadState.ProcessBuffer(); // process buffer aka Parse

            var reader = new KafaReader(kafaReadState);

            return reader.GetRows();
        }

        public static void Write<T>(IBufferWriter<byte> bufferWriter, List<T> entities, KafaOptions? options = null)
        {
            ArgumentNullException.ThrowIfNull(entities, nameof(entities));
            var reflection = SetupOptions<T>(options);
            using var writer = new KafaWriter(bufferWriter, reflection.TypeInfo.KafaOptions);
            reflection.GetProperties(writer, entities);
        }
        
        
        public static ReadOnlySpan<byte> Write<T>(List<T> entities, KafaOptions? options = null)
        {
            ArgumentNullException.ThrowIfNull(entities, nameof(entities));
            var reflection = SetupOptions<T>(options);
            using var pooledBufferWriter = new KafaPooledWriter();
            using var bufferWriter = new KafaWriter(pooledBufferWriter, reflection.TypeInfo.KafaOptions);
            reflection.GetProperties(bufferWriter, entities);
            return pooledBufferWriter.WrittenAsSpan;
        }

        public static async ValueTask<Stream> WriteToStreamAsync<T>(List<T> entities, KafaOptions? options = null, CancellationToken token = default)
        {
            ArgumentNullException.ThrowIfNull(entities, nameof(entities));
            var reflection = SetupOptions<T>(options);
            var memoryStream = new MemoryStream();
            using var bufferWriter = new KafaWriter(memoryStream, reflection.TypeInfo.KafaOptions);
            reflection.GetProperties(bufferWriter, entities);
            await bufferWriter.FlushAsync(token).ConfigureAwait(false);
            return memoryStream;
        }

        public static async ValueTask WriteToFileAsync<T>(List<T> entities, string path, KafaOptions? options = null, CancellationToken token=default)
        {
            ArgumentNullException.ThrowIfNull(entities, nameof(entities));
            ArgumentNullException.ThrowIfNull(path, nameof(path));
            var reflection = SetupOptions<T>(options);
            using var fs = new FileStream(path, FileMode.Create);
            using var bufferWriter = new KafaWriter(fs, reflection.TypeInfo.KafaOptions);
            reflection.GetProperties(bufferWriter, entities);
            await bufferWriter.FlushAsync(token).ConfigureAwait(false);
        }

        private static KafaReflection SetupOptions<T>(KafaOptions? options)
        {
            options = KafaOptions.ResolveKafaOptions(options);
            var typeInfo = new KafaTypeInfo(typeof(T), options);
            return new KafaReflection(typeInfo);
        }
    }
}
