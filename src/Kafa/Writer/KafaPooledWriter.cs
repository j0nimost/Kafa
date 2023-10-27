using System.Buffers;

namespace nyingi.Kafa.Writer
{
    public class KafaPooledWriter : IBufferWriter<byte>, IDisposable
    {
        private const int DefaultBufferLength = 65556;
        private byte[] _buffer;

        private int _index;

        private int Capacity => _buffer.Length;
        private int FreeCapacity => _buffer.Length - _index;
        public KafaPooledWriter(int length = 0)
        {
            _buffer = ArrayPool<byte>.Shared.Rent(Math.Max(length, DefaultBufferLength));
        }
        public ReadOnlySpan<byte> WrittenAsSpan => _buffer.AsSpan(0, _index);
        public ReadOnlyMemory<byte> WrittenAsMemory => _buffer.AsMemory(0, _index);
        public void Advance(int count)
        {
            if(count  < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }
            _index += count;
        }
        public Memory<byte> GetMemory(int sizeHint = 0)
        {
            Resize(sizeHint);
            return _buffer.AsMemory(_index);
        }
        public Span<byte> GetSpan(int sizeHint = 0)
        {
            Resize(sizeHint);
            return _buffer.AsSpan(_index);
        }
        private void Resize(int sizeHint)
        {
            if (sizeHint < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(sizeHint));
            }

            if (sizeHint == 0)
            {
                sizeHint = 1;
            }
            if(sizeHint > FreeCapacity)
            {
                int growBy = Math.Max(Capacity, sizeHint);
                int newCapacity = Capacity;
                checked
                {
                    newCapacity += growBy;
                }
                var newBuffer = ArrayPool<byte>.Shared.Rent(newCapacity);
                Array.Copy(_buffer, newBuffer, Capacity);
                _buffer = null!;
                _buffer = newBuffer;
                ArrayPool<byte>.Shared.Return(newBuffer);
            }
        }
        public void Dispose()
        {
            if(_buffer == null) 
            {
                return;
            }
            ArrayPool<byte>.Shared.Return(_buffer);
        }
        
    }
}
