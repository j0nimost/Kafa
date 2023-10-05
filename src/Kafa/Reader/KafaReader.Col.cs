﻿using System.Runtime.CompilerServices;

namespace nyingi.Kafa.Reader
{
    public partial struct KafaReader
    {
        public readonly ref struct Col
        {
            public readonly ReadOnlySpan<char> Value;
            private readonly ColEnumerable _colEnumerable;
            
            public int Length
            {
                get
                {
                    return Value.Length;
                }
            }

            public Col(ColEnumerable colEnumerable, ReadOnlySpan<char> value)
            {
                Value = value;
                _colEnumerable = colEnumerable;
            }


            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public T Parse<T>() where T : ISpanParsable<T> => _colEnumerable.Parse<T>(Value);


            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool TryParse<T>(out T result) where T : ISpanParsable<T> => _colEnumerable.TryParse(Value, out result);

            public override string ToString()
            {
                return $"{Value}";
            }

        }

        public ref struct ColEnumerable
        {
            private readonly KafaReader _reader;
            private ReadOnlySpan<int> _colMarkerIndexes;

            public ColEnumerable(KafaReader reader, ReadOnlySpan<int> colMarkerIndexes)
            {
                _reader = reader;
                _colMarkerIndexes = colMarkerIndexes;
            }

            public int Length => _colMarkerIndexes.Length;

            public Col this[int index] => new(this, this.ReadColSpan(index)); 

            public Col this[Index index] => new(this, this.ReadColSpan(index.GetOffset(this.Length)));

            private ReadOnlySpan<char> ReadColSpan(int index)
            {
                int lastColMarker = index + 1;

                if (index < 0 || lastColMarker >= _colMarkerIndexes.Length)
                {
                    throw new KafaException($"{index}: {nameof(index)} out of range", new ArgumentOutOfRangeException(nameof(index)));
                }
                int startIndex = _colMarkerIndexes[index]; 
                startIndex = startIndex == 0 ? 0 : startIndex + 1; // SKIP Separator
                int lastIndex = _colMarkerIndexes[lastColMarker];
                lastIndex = _reader.HasCRLF && lastColMarker == _colMarkerIndexes.Length - 1 && lastIndex != _reader.LastBufferIndex 
                                ? lastIndex - 1 : lastIndex;
                return _reader.ReadColSpan(startIndex, lastIndex);
            } 


            public T Parse<T>(ReadOnlySpan<char> scanSpan) where T : ISpanParsable<T>
            {
                return T.Parse(scanSpan, _reader.cultureInfo);
            }

            public bool TryParse<T>(ReadOnlySpan<char> colValue, out T result) where T : ISpanParsable<T>
            {
                return T.TryParse(colValue, _reader.cultureInfo, out result!);
            }

            public Enumerator GetEnumerator() => new Enumerator(this);


            public ref struct Enumerator
            {
                private int _index = -1;

                private readonly ColEnumerable _colEnumerable;

                public Enumerator(ColEnumerable colEnumerable)
                {
                    _colEnumerable = colEnumerable;
                }

                public Col Current => new(_colEnumerable, _colEnumerable.ReadColSpan(_index));

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