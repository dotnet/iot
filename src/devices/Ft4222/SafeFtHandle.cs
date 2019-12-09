using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace System.Device.Ft4222
{
    internal class SafeFtHandle : SafeHandle
    {
        public SafeFtHandle() : base(IntPtr.Zero, true)
        { }

        public override bool IsInvalid => handle == IntPtr.Zero;

        protected override bool ReleaseHandle()
        {
            var ftStatus = FtFunction.FT4222_UnInitialize(this);
            ftStatus = FtFunction.FT_Close(this);
            return ftStatus == FtStatus.Ok;
        }
    }
}
