using nyingi.Kafa.Reader;
using nyingi.Kafa.Reflection;
using System;
using System.Xml;
using static nyingi.Kafa.Reader.KafaReader;

namespace nyingi.Kafa
{
    public static partial class Kafa
    {
        public static RowEnumerable Read(ReadOnlySpan<char> span, KafaOptions? options = null)
        {
            options = KafaOptions.ResolveKafaOptions(options);
            var readerState = new KafaReadState(span.Length, options);
            return ProcessReaderState(readerState, span);
        }

        public static IEnumerable<T> Read<T>(string content, KafaOptions? options = null)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(content, nameof(content));
            var rows = Read(content.AsSpan(), options);
            var reflection = SetupOptions<T>(options);
            return reflection.SetProperties<T>(rows);
        }

        private static RowEnumerable ProcessReaderState(KafaReadState readerState, ReadOnlySpan<char> span)
        {
            readerState.ReadState(span);
            readerState.ProcessBuffer();

            var reader = new KafaReader(readerState);
            return reader.GetRows();
        }
    }
}
