using System.Buffers;

namespace nyingi.Kafa.Writer
{
    internal partial class KafaWriter : IDisposable
    {
        private IBufferWriter<byte> _bufferWriter;
        private KafaPooledWriter _kafaPooledWriter;
        public int BytesWritten { get; private set; }
        private Stream? _stream = default;

        public KafaWriter(IBufferWriter<byte> bufferWriter)
        {
            _bufferWriter = bufferWriter;
        }

        public KafaWriter(KafaPooledWriter pooledWriter, Stream stream)
        {
            _stream = stream;
            _kafaPooledWriter = pooledWriter; // use the backing bufferWriter to flusH
        }


        public void Dispose()
        {
            _stream = null;
            _kafaPooledWriter = null;
            _bufferWriter = null;
        }
    }
}
