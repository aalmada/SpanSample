using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SpanSample
{
    public struct Enumerable : IEnumerable<Foo>
    {
        readonly Stream stream;
        readonly int itemsBufferCount;

        public Enumerable(Stream stream, int itemsBufferCount)
        {
            this.stream = stream;
            this.itemsBufferCount = itemsBufferCount;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerator<Foo> GetEnumerator() => new Enumerator(this);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        struct Enumerator : IEnumerator<Foo>
        {
            static readonly int ItemSize = Unsafe.SizeOf<Foo>();

            readonly Stream stream;
            readonly Memory<Foo> buffer;
            bool lastBuffer;
            long loadedItems;
            int currentItem;

            public Enumerator(Enumerable enumerable)
            {
                stream = enumerable.stream;
                buffer = new Foo[enumerable.itemsBufferCount]; // alloc items buffer
                lastBuffer = false;
                loadedItems = 0;
                currentItem = -1;
            }

            public Foo Current
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
                var rawBuffer = MemoryMarshal.Cast<Foo,byte>(buffer.Span);
                var bytesRead = stream.Read(rawBuffer);
                lastBuffer = bytesRead < rawBuffer.Length;
                currentItem = 0;
                loadedItems = bytesRead / ItemSize;
                return loadedItems != 0;
            }

            public void Reset() => throw new NotImplementedException();

            public void Dispose()
            {
                // nothing to do
            }
        }
    }
}
