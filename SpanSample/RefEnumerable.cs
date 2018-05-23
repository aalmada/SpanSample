using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SpanSample
{
    struct RefEnumerable
    {
        readonly Stream stream;
        readonly int itemsBufferCount;

        public RefEnumerable(Stream stream, int itemsBufferCount)
        {
            this.stream = stream;
            this.itemsBufferCount = itemsBufferCount;
        }

        public Enumerator GetEnumerator() => new Enumerator(this);

        public unsafe ref struct Enumerator
        {
            static readonly int ItemSize = Unsafe.SizeOf<Foo>();

            readonly Stream stream;
            readonly Span<Foo> buffer;
            readonly Span<byte> rawBuffer;
            bool lastBuffer;
            long loadedItems;
            int currentItem;

            public Enumerator(RefEnumerable enumerable)
            {
                stream = enumerable.stream;
                buffer = new Foo[enumerable.itemsBufferCount]; // alloc items buffer
                rawBuffer = MemoryMarshal.Cast<Foo, byte>(buffer); // cast items buffer to bytes buffer (no copies)
                lastBuffer = false;
                loadedItems = 0;
                currentItem = -1;
            }

            public ref readonly Foo Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => ref buffer[currentItem];
            }

            public bool MoveNext()
            {
                if (++currentItem != loadedItems) // increment current position and check if reached end of buffer
                    return true;
                if (lastBuffer) // check if it was the last buffer
                    return false;

                // get next buffer
                var bytesRead = stream.Read(rawBuffer);
                lastBuffer = bytesRead < rawBuffer.Length;
                currentItem = 0;
                loadedItems = bytesRead / ItemSize;
                return loadedItems != 0;
            }
        }
    }
}
