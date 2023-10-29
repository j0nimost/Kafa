using System.Runtime.CompilerServices;

namespace nyingi.Kafa.Reader
{
    public partial class KafaReader
    {
        public readonly ref struct Col
        {
            public readonly ReadOnlySpan<char> Value;
            private readonly ColEnumerable _colEnumerable;

            public int Length => Value.Length;

            public Col(ColEnumerable colEnumerable, int index)
            {
                _colEnumerable = colEnumerable;
                Value = _colEnumerable.ReadColAsSpan(index);

            }

            public Col(ColEnumerable colEnumerable, string columnName)
            {
                _colEnumerable = colEnumerable;
                int index = _colEnumerable.ReadColByHeader(columnName);
                Value = _colEnumerable.ReadColAsSpan(index);
            }
            
            public override string ToString()
            {
                return $"{Value}";
            }

        }

        public struct ColEnumerable
        {
            private readonly KafaReader _reader;
            private ReadOnlyMemory<int> _colMarkerIndexes;

            public ColEnumerable(KafaReader reader, int startColIndex, int length)
            {
                _reader = reader;
                _colMarkerIndexes = reader.ReadColMarkerAsMemory(startColIndex, length);
            }

            public int Length => _colMarkerIndexes.Length;

            public Col this[int index] => new(this, index); 

            public Col this[Index index] => new(this, index.GetOffset(this.Length));

            public Col this[string columnName] => new(this, columnName);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int ReadColByHeader(string columnName)
            {
                if(_reader.Headers != null && _reader.Headers.Contains(columnName))
                {
                    return (int)_reader.Headers[columnName]!;
                }

                throw new KafaException($"{columnName} Not Found");
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ReadOnlySpan<char> ReadColAsSpan(int index)
            {
                int lastColMarker = index + 1;

                if (index < 0 || lastColMarker >= _colMarkerIndexes.Length)
                {
                    throw new KafaException($"{index}: {nameof(index)} out of range", new ArgumentOutOfRangeException(nameof(index)));
                }
                int startIndex = _colMarkerIndexes.Span[index]; 
                startIndex = startIndex == 0 ? 0 : startIndex + 1; // SKIP Separator
                int lastIndex = _colMarkerIndexes.Span[lastColMarker];
                // TODO: Simplify this checks
                lastIndex = _reader.HasCRLF && lastColMarker == _colMarkerIndexes.Length - 1 && lastIndex != _reader.LastBufferIndex 
                                ? lastIndex - 1 : lastIndex;

                var mem = _reader.ReadColAsMemory(startIndex, lastIndex);
                return mem.Span;
            } 

            public Enumerator GetEnumerator() => new Enumerator(this);


            public struct Enumerator
            {
                private int _index = -1;

                private readonly ColEnumerable _colEnumerable;

                public Enumerator(ColEnumerable colEnumerable)
                {
                    _colEnumerable = colEnumerable;
                }

                public Col Current => new(_colEnumerable, _index);

                public bool MoveNext()
                {
                    _index++;
                    return _index + 1 < _colEnumerable.Length;
                }

                public void Reset()
                {
                    _index = -1;
                }
            }
        }

    }
}
