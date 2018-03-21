// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System.Buffers
{
    internal sealed class NativeOwnedMemory<T> : OwnedMemory<T>
    {
        private readonly int _length;
        private readonly int _byteLength;
        private IntPtr _ptr;
        private int _retainedCount;
        private bool _disposed;

        public NativeOwnedMemory(int length)
        {
            _length = length;
            _byteLength = length * Unsafe.SizeOf<T>();
            _ptr = Marshal.AllocHGlobal(_byteLength);
            GC.AddMemoryPressure(_byteLength);
        }

        ~NativeOwnedMemory()
        {
            Debug.WriteLine($"{nameof(NativeOwnedMemory<T>)} being finalized");
            Dispose(false);
        }

        public override bool IsDisposed
        {
            get {
                lock (this)
                {
                    return _disposed && _retainedCount == 0;
                }
            }
        }

        public override int Length => _length;

        protected override bool IsRetained
        {
            get {
                lock (this)
                {
                    return _retainedCount > 0;
                }
            }
        }

        public override unsafe Span<T> Span => new Span<T>(_ptr.ToPointer(), _length);

        public override unsafe MemoryHandle Pin(int offset = 0)
        {
            if (offset < 0 || offset > _length) throw new ArgumentOutOfRangeException(nameof(offset));
            return new MemoryHandle(this, Unsafe.Add<T>(_ptr.ToPointer(), offset));
        }

        public override bool Release()
        {
            lock (this)
            {
                if (_retainedCount > 0)
                {
                    _retainedCount--;
                    if (_retainedCount == 0)
                    {
                        if (_disposed && _ptr != IntPtr.Zero)
                        {
                            Marshal.FreeHGlobal(_ptr);
                            _ptr = IntPtr.Zero;
                            GC.RemoveMemoryPressure(_byteLength);
                        }
                        return true;
                    }
                }
            }
            return false;
        }

        public override void Retain()
        {
            lock (this)
            {
                if (_retainedCount == 0 && _disposed)
                {
                    throw new Exception();
                }
                _retainedCount++;
            }
        }

        protected override void Dispose(bool disposing)
        {
            lock (this)
            {
                _disposed = true;
                if (_retainedCount == 0 && _ptr != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(_ptr);
                    _ptr = IntPtr.Zero;
                    GC.RemoveMemoryPressure(_byteLength);
                }
            }
        }

        protected override bool TryGetArray(out ArraySegment<T> arraySegment)
        {
            arraySegment = default;
            return false;
        }
    }
}