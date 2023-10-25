using System.Buffers;

namespace nyingi.Kafa.Writer
{
    internal class KafaWriter : IDisposable
    {
        private IBufferWriter<byte>? _bufferWriter;
        private KafaPooledWriter? _kafaPooledWriter;
        private Stream? _stream = default;
        public KafaWriter(IBufferWriter<byte> bufferWriter)
        {
            _bufferWriter = bufferWriter ?? throw new NullReferenceException(nameof(bufferWriter));
        }
        public KafaWriter(Stream stream)
        {
            _stream = stream;
            _kafaPooledWriter = new KafaPooledWriter(0); // use the default 65k
        }

        public void Write(ReadOnlySpan<byte> values)
        {
            if (_bufferWriter != null)
            {
                _bufferWriter.Write(values);
            }
            else
            {
                _kafaPooledWriter?.Write(values);
            }
        }

        public void Flush()
        {
            if (_stream != null)
            {
                _stream.Write(_kafaPooledWriter!.WrittenAsSpan);
                _stream.Flush();
            }
        }

        public async ValueTask FlushAsync(CancellationToken cancellationToken=default)
        {
            if (_stream != null)
            {
                await _stream.WriteAsync(_kafaPooledWriter!.WrittenAsMemory, cancellationToken);
                await _stream.FlushAsync();
            }
        }
        public void Dispose()
        {
            _stream = null;
            _bufferWriter = null;
            if (_kafaPooledWriter != null)
            {
                _kafaPooledWriter.Dispose();
            }
        }
    }
}
