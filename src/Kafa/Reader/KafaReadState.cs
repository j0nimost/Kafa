using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace nyingi.Kafa.Reader
{
    public struct KafaReadState : IDisposable
    {
        public char[] Buffer { get; private set; }
        public int[] ColMarker { get; private set; }


        public int ColMarkerLength { get; private set; }
        public int ColCount { get; private set; }
        public int RowCount { get; private set; }

        public int OffSet { get; private set; }

        public bool HasCLRF { get; private set; } = false;

        public int LastBufferIndex { get; private set; }

        private int bufferReadCount;
        

        private const int MaxBufferSize = 512;

        public readonly KafaOptions Options;

        public readonly int BufferLength;


        public Dictionary<string,int>? Headers { get; private set; } = default;
        public KafaReadState(int bufferLength, KafaOptions kafaOptions)
        {
            BufferLength = bufferLength;
            Buffer = ArrayPool<char>.Shared.Rent(Math.Max(BufferLength, MaxBufferSize));
            ColMarker = ArrayPool<int>.Shared.Rent(Math.Max(BufferLength / 2, MaxBufferSize));
            bufferReadCount = 0;
            Options = kafaOptions;
        }

        public async ValueTask ReadStateAsync(TextReader reader, CancellationToken cancellationToken)
        {
            do
            {
                int bufferRead = await reader.ReadAsync(Buffer, cancellationToken);

                bufferReadCount += bufferRead;

            } while (bufferReadCount < BufferLength);
        }

        public void ReadState(TextReader reader)
        {
            bufferReadCount = reader.Read(Buffer, 0, Buffer.Length);
        }

        public void ProcessBuffer()
        {
            ReadColCount();
            ReadColMarkers();

            if (Options.HasHeader)
            {
                ReadHeaderRow(ColCount);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ReadColMarkers()
        {

            int i = 0;
            int colIndexer = 0;
            int colCount = 0;
            while (Buffer[i] != '\0')
            {
                // weird but necessary, ensure we set the first index for the buffer just incase arraypool comes prepopulated
                if(colIndexer == 0) 
                {
                    ColMarker[colIndexer] = 0;
                    i++;
                    colIndexer++;
                    continue;
                }

                if (Buffer[i] == '"')
                {
                    int j = i + 1;
                    while (j < Buffer.Length)
                    {
                        if (Buffer[j] == '"')
                        {
                            break;
                        }
                        j++;
                    }

                    if (Buffer[j + 1] == '\0') // the next character is missing
                    {
                        ColMarker[colIndexer] = j;
                        colIndexer++;
                        colCount++;

                        if (colCount > ColCount) // ensure standard row length to prevent wrong parsing
                        {
                            throw new KafaException($"The file has unequal rows sizes: {colCount} > {ColCount}");
                        }
                        RowCount++;
                        break;

                    }

                    i = j + 1;
                    continue;
                }
                else if (Buffer[i] == (int)Options.FileType || Buffer[i] == '\n' ||  Buffer[i + 1] == '\0')
                {
                    ColMarker[colIndexer] = i;
                    colIndexer++;
                    colCount++;
                }

                if (Buffer[i] == '\n' || Buffer[i + 1] == '\0')
                {
                    if (colCount > ColCount) // ensure standard row length to prevent wrong parsing
                    {
                        throw new KafaException($"The file has unequal rows sizes: {colCount} > {ColCount}");
                    }
                    RowCount++;
                    colCount = 0;
                }
                i++;
            }
            // check if the file ends with an optional LF of CRLF
            int bufferIndex = 0;
            if (Buffer[i - 1] == '\n')
            {
                bufferIndex = Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(ColMarker), colIndexer - 1);
                bufferIndex = Buffer[i - 2] == '\r' ? bufferIndex - 1 : bufferIndex; // include one byte to calculate length against
            }
            else
            {
                bufferIndex = Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(ColMarker), colIndexer - 1) + 1; // add one byte to offset length
            }
            ColMarker[colIndexer - 1] = bufferIndex;
            LastBufferIndex = bufferIndex;
            ColMarkerLength = colIndexer;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ReadColCount()
        {
            int i = 0;
            while (i < Buffer.Length)
            {
                if (Buffer[i] == '"')
                {
                    // keep reading till the next "
                    int j = i + 1;
                    while (j < Buffer.Length)
                    {
                        if (Buffer[j] == '"')
                        {
                            break;
                        }
                        j++;
                    }

                    i = j + 1;
                    continue;
                }
                else if (Buffer[i] == (int)Options.FileType)
                {
                    ColCount++;
                }
                else if (Buffer[i] == '\n')
                {
                    ColCount++;

                    if (Buffer[i - 1] == '\r')
                    {
                        HasCLRF = true;
                    }
                    break;
                }
                else if(i == Buffer.Length - 1)
                {
                    ColCount++;
                }
                i++;
            }
        }
        private void ReadHeaderRow(int length)
        {
            Headers = new Dictionary<string, int>(length);
            int index = 0;
            int colStartIndex = 0, colEndIndex = 0;
            while (index + 1 <= length)
            {
                colStartIndex = this.ColMarker[index] == 0 ? 0 : this.ColMarker[index] + 1;
                colEndIndex = HasCLRF && index + 1 == length ? this.ColMarker[index + 1] - 1  : this.ColMarker[index + 1];

                Headers.Add(Buffer.AsSpan(colStartIndex, colEndIndex - colStartIndex).ToString(), index);

                index++;
            }
            OffSet = index; // for subsequent reads
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ReadRowRange(Range range)
        {
            var (offset, length) = range.GetOffsetAndLength(ColMarkerLength - 1);

            var startIndex = offset * ColCount;

            var newLength = length * ColCount;

            var newContentArr = new int[newLength];
            int k = 0;
            for (int i = startIndex; i < newLength; i++)
            {
                newContentArr[k] = ColMarker[i];
                k++;
            }

            Array.Clear(ColMarker, 0, ColMarker.Length);
            newContentArr.CopyTo(ColMarker, 0);
            ColMarkerLength = newLength;
        }

        public void Dispose()
        {
            ArrayPool<char>.Shared.Return(Buffer, true);  
            ArrayPool<int>.Shared.Return(ColMarker);
        }
    }
}
