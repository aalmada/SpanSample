using Microsoft.Win32.SafeHandles;
using System;

namespace SpanSample
{
    sealed class NativeStreamHandle :
        SafeHandleZeroOrMinusOneIsInvalid
    {
        public NativeStreamHandle(UIntPtr size) : 
            base(true)
        {
            handle = Native.StreamNew(size);
        }

        protected override bool ReleaseHandle()
        {
            Native.StreamDelete(handle);
            return true;
        }
    }
}
