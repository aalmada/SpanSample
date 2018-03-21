using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SpanSample
{
    public sealed class NativeEnumerable : 
        IEnumerable<int>
    {
        readonly int itemsCount;
        readonly int itemsBufferCount;

        public NativeEnumerable(int itemsCount, int itemsBufferCount)
        {
            this.itemsCount = itemsCount;
            this.itemsBufferCount = itemsBufferCount;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerator<int> GetEnumerator() => new Enumerator(this);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        struct Enumerator : IEnumerator<int>
        {
            readonly NativeStreamHandle stream;
            readonly NativeOwnedMemory<int> buffer;
            bool lastBuffer;
            long loadedItems;
            int currentItem;

            public Enumerator(NativeEnumerable enumerable)
            {
                stream = new NativeStreamHandle((UIntPtr)enumerable.itemsCount);
                buffer = new NativeOwnedMemory<int>(enumerable.itemsBufferCount); // alloc items buffer
                lastBuffer = false;
                loadedItems = 0;
                currentItem = -1;
            }

            public int Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => buffer.Span[currentItem];
            }

            object IEnumerator.Current => Current;

            public bool MoveNext()
            {
                if (++currentItem != loadedItems) // increment current position and check if reached end of buffer
                    return true;
                if (lastBuffer) // check if it was the last buffer
                    return false;

                // get next buffer
                this.loadedItems = (int)Native.StreamRead(this.stream, ref MemoryMarshal.GetReference(this.buffer.Span), (UIntPtr)this.buffer.Length);
                lastBuffer = loadedItems < buffer.Length;
                currentItem = 0;
                return loadedItems != 0;
            }

            public void Reset() => Native.StreamReset(this.stream);

            public void Dispose()
            {
                this.stream.Dispose();
                this.buffer.Dispose();
            }
        }
    }
}
