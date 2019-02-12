using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace System.Device.Gpio
{
    internal class SafeChipIteratorHandle : SafeHandle
    {
        public SafeChipIteratorHandle() : base(IntPtr.Zero, true) { }

        protected override bool ReleaseHandle()
        {
            Interop.FreeChipIterator(handle);
            return true;
        }

        public override bool IsInvalid => handle == IntPtr.Zero || handle == new IntPtr(-1);
    }
}
