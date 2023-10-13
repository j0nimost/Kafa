using System.Collections.Specialized;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace nyingi.Kafa.Reader
{
    public partial struct KafaReader : IDisposable
    {
        private readonly KafaReadState _kafaReadState;
        private readonly CultureInfo? cultureInfo;
        public KafaReader(in KafaReadState kafaReadState)
        {
            _kafaReadState = kafaReadState;
            cultureInfo = kafaReadState.Options.CultureInfo;
        }

        public int ColumnCount => _kafaReadState.ColCount;
        public int RowCount => _kafaReadState.RowCount;

        public  int BufferLength => _kafaReadState.BufferLength;

        public  int ColMarkerLength => _kafaReadState.ColMarkerLength;

        public int LastBufferIndex => _kafaReadState.LastBufferIndex;

        public bool HasCRLF => _kafaReadState.HasCRLF;

        public int OffSet => _kafaReadState.OffSet;

        public OrderedDictionary? Headers => _kafaReadState.Headers;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlyMemory<char> ReadRowSpan(int index, out int lastColMarkerIndex)
        {
            lastColMarkerIndex = index + ColumnCount;

            if(index < 0 || lastColMarkerIndex > ColMarkerLength - 1)
            {
                throw new KafaException($"{index}: {nameof(index)} out of range", new ArgumentOutOfRangeException(nameof(index)));
            }

            int startColIndex = Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(_kafaReadState.ColMarker), index);
            int lastColIndex = Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(_kafaReadState.ColMarker), lastColMarkerIndex);
            startColIndex = startColIndex == 0 ? startColIndex : startColIndex + 1; // escape \n token
            // if in last row pass the lastColIndex
            // else check if HASCRLF then skip \r else return current
            lastColIndex = lastColMarkerIndex == ColMarkerLength - 1 ? lastColIndex :
                                                            (_kafaReadState.HasCRLF ? lastColIndex - 1 : lastColIndex);
            return _kafaReadState.Buffer.AsMemory(startColIndex, lastColIndex - startColIndex); 
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<char> ReadColSpan(int startIndex, int lastIndex)
        {
            return _kafaReadState.Buffer.AsSpan(startIndex, lastIndex - startIndex);
        }

        public RowEnumerable GetRows(Range range)
        {
            _kafaReadState.ReadRowRange(range);
            return new(this);
        }

        public RowEnumerable GetRows()
        {
            return new(this);
        }

        private ReadOnlySpan<int> ReadColMarkerSpan(int index, int length)
        {
            if(index < 0 || index + length > _kafaReadState.ColMarkerLength)
            {
                throw new KafaException($"{index}: {nameof(index)} out of range", new ArgumentOutOfRangeException(nameof(index)));
            }

            return _kafaReadState.ColMarker.AsSpan(index, length);
        }

        public void Dispose()
        {
            _kafaReadState.Dispose();
        }
    }
}
