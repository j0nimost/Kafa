using System.Buffers;

namespace nyingi.Kafa.Writer
{
    internal partial class KafaWriter : IDisposable
    {
        private readonly IBufferWriter<byte> _bufferWriter;
        public int BytesWritten { get; private set; }
        private Stream? _stream = default;
        private Memory<byte> _memory = default;


        public KafaWriter(IBufferWriter<byte> bufferWriter)
        {
            _bufferWriter = bufferWriter;
        }

        public KafaWriter(Stream stream)
        {
            _stream = stream;
        }


        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
