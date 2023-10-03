
using System.Collections;

namespace nyingi.Kafa.Reader
{
    public partial struct KafaReader
    {
        public readonly struct Row
        {
            public readonly ReadOnlyMemory<char> Value; 
            public readonly KafaReader _reader;
            private readonly int _startColMarkerIndex;
            private readonly int _lastColMarkerIndex;
            public readonly int ColMarkerLength;

            public Row(in KafaReader reader, int startIndex)
            {
                _reader = reader;
                _startColMarkerIndex = startIndex;
                Value = reader.ReadRowSpan(_startColMarkerIndex, out _lastColMarkerIndex);
                ColMarkerLength = _lastColMarkerIndex - _startColMarkerIndex + 1;
            }

            public readonly ColEnumerable Cols => GetCols();

            public readonly ColEnumerable GetCols()
            {
                return new(_reader, _reader.ReadColMarkerSpan(_startColMarkerIndex, ColMarkerLength));
             }

            public readonly ColEnumerable GetCols(Range range)
            {
                var(offset, length) = range.GetOffsetAndLength(ColMarkerLength);
                return new(_reader, _reader.ReadColMarkerSpan(offset, length));
            }

            public override string ToString()
            {
                return $"{Value}";
            }

        }

        public struct RowEnumerable : IEnumerable<Row>, IDisposable
        {
            private readonly KafaReader _reader;

            public RowEnumerable(in KafaReader reader)
            {
                _reader = reader;
            }

            public Row this[int index] => new(_reader, GetIndexOffset(index));

            public Row this[Index index] => new(_reader, GetIndexOffset(index));

            public int Length => _reader.RowCount;

            public Dictionary<string, int>? Headers => _reader.Headers;
            private int GetIndexOffset(Index index)
            {
                return GetIndexOffset(index.Value);
            }

            private int GetIndexOffset(int index)
            {
                return index * _reader.ColumnCount;
            }

            IEnumerator<Row> IEnumerable<Row>.GetEnumerator() => new Enumerator(_reader);

            IEnumerator IEnumerable.GetEnumerator() => new Enumerator(_reader);

            public void Dispose()
            {
                _reader.Dispose();
            }
            public struct Enumerator : IEnumerator<Row>
            {
                private int _index = -1;
                private readonly int _columnCount;
                private readonly int _colMarkerLastIndex;
                private readonly KafaReader _reader;

                public Row Current => new(_reader, _index);

                object IEnumerator.Current => Current;

                public Enumerator(in KafaReader reader)
                {
                    _reader = reader;
                    _columnCount = _reader.ColumnCount;
                    _colMarkerLastIndex = _reader.ColMarkerLength - 1;
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

                public void Dispose()
                {
                }
            }
        }

    }
}
