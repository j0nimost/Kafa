
using System.Collections;
using System.Collections.Specialized;

namespace nyingi.Kafa.Reader
{
    public partial struct KafaReader
    {

        public readonly struct Row
        {
            private readonly ReadOnlyMemory<char> _value; 
            private readonly KafaReader _reader;
            private readonly int _startColMarkerIndex;
            private readonly int _colMarkerLength;

            public Row(in KafaReader reader, int startIndex)
            {
                _reader = reader;
                _startColMarkerIndex = startIndex;
                _value = reader.ReadRowSpan(_startColMarkerIndex, out int lastColMarkerIndex);
                _colMarkerLength = lastColMarkerIndex - _startColMarkerIndex + 1;
            }

            public readonly ColEnumerable Cols => new(_reader, _startColMarkerIndex, _colMarkerLength);
            public readonly ColEnumerable GetColsRange(Range range)
            {
                var(offset, length) = range.GetOffsetAndLength(_colMarkerLength);
                return new(_reader, offset, length);
            }

            public override string ToString()
            {
                return $"{_value}";
            }

        }

        public readonly struct RowEnumerable :  IDisposable
        {
            private readonly KafaReader _reader;

            public RowEnumerable(in KafaReader reader)
            {
                _reader = reader;
            }

            public Row this[int index] => new(_reader, GetIndexOffset(index));

            public Row this[Index index] => new(_reader, GetIndexOffset(index));

            public int Length => _reader.RowCount;

            public OrderedDictionary? Headers => _reader.Headers;
            private int GetIndexOffset(Index index)
            {
                return GetIndexOffset(index.Value);
            }

            private int GetIndexOffset(int index)
            {
                return index * _reader.ColumnCount;
            }
            public Enumerator GetEnumerator() => new Enumerator(_reader);

            public void Dispose()
            {
                _reader.Dispose();
            }
            public struct Enumerator 
            {
                private int _index = -1;
                private readonly int _columnCount;
                private readonly int _colMarkerLastIndex;
                private readonly KafaReader _reader;

                public Row Current => new(_reader, _index);
                public Enumerator(in KafaReader reader)
                {
                    _reader = reader;
                    _columnCount = _reader.ColumnCount;
                    _colMarkerLastIndex = _reader.ColMarkerLength - 1;
                    // if the Offset exists since we might have read the headers, move the index for easier arithmetics
                    _index = _reader.OffSet == 0 ? _index : 0; 

                }
                public bool MoveNext()
                {
                    if(_index > -1)
                    {
                        _index += _columnCount;
                    }
                    else
                    {
                        _index++;
                    }

                    return _index < _colMarkerLastIndex;
                }
                public void Reset()
                {
                    _index = -1;
                }
            }
        }

    }
}
