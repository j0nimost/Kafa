using System.Buffers;
using System.Text;

namespace nyingi.Kafa.Writer
{
    public class KafaWriter : IDisposable
    {
        private IBufferWriter<byte>? _bufferWriter;
        private KafaPooledWriter? _kafaPooledWriter;
        private Stream? _stream = default;
        private readonly byte[] _unixNewLine =new byte[1] {(byte)'\n'};
        private readonly byte[] _winNewLine = new byte[2] {(byte)'\r',(byte)'\n'};

        private readonly KafaOptions _options;
        private readonly byte[] _cachedNewLine;
        
        private byte[] _separator = new byte[1];
        public KafaWriter(in IBufferWriter<byte> bufferWriter, KafaOptions options)
        {
            _bufferWriter = bufferWriter ?? throw new NullReferenceException(nameof(bufferWriter));
            _options = options;
            _separator[0] = (byte)_options.Separator;
            _cachedNewLine = Environment.OSVersion.Platform == PlatformID.Unix ? _unixNewLine : _winNewLine;
        }
        public KafaWriter(in Stream stream, KafaOptions options)
        {
            _stream = stream;
            _options = options;
            _kafaPooledWriter = new KafaPooledWriter(); // use the default 65k
            _cachedNewLine = Environment.OSVersion.Platform == PlatformID.Unix ? _unixNewLine : _winNewLine;
        }

        public void WriteSeparator()
        {
            Write(_separator.AsSpan());
        }

        public void WriteLine()
        {
            Write(_cachedNewLine);
        }

        public void Write(string str)
        {
            var strBytes = Encoding.UTF8.GetBytes(str);
            Write(strBytes.AsSpan());
        }

        private void Write(ReadOnlySpan<byte> values)
        {
            if (_bufferWriter != null)
            {
                _bufferWriter.Write(values);
            }
            else
            {
                _kafaPooledWriter!.Write(values);
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
                await _stream.FlushAsync(cancellationToken);
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
