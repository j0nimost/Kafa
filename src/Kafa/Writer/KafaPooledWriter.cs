using System.Buffers;

namespace nyingi.Kafa.Writer
{
    internal sealed class KafaPooledWriter : IBufferWriter<byte>, IDisposable
    {
        private const int MAXBUFFERLENGTH = 65556;
        private byte[] _buffer;

        private int _index  = 0;

        public int WrittenCount => _index;
        public int Capacity => _buffer.Length;
        public int FreeCapacity => _buffer.Length - _index;

        public KafaPooledWriter(int length)
        {
            _buffer = ArrayPool<byte>.Shared.Rent(Math.Max(length, MAXBUFFERLENGTH));
        }
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
            return _buffer.AsMemory(sizeHint);
        }

        public Span<byte> GetSpan(int sizeHint = 0)
        {
            Resize(sizeHint);
            return _buffer.AsSpan(sizeHint);
        }


        public ReadOnlySpan<byte> WrittenAsSpan() => _buffer.AsSpan(0, _index);

        private void Resize(int sizeHint)
        {
            if (sizeHint < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(sizeHint));
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
