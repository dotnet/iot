// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Runtime.InteropServices;

namespace Iot.Device.FtCommon
{
    internal class SafeFtHandle : SafeHandle
    {
        public SafeFtHandle()
            : base(IntPtr.Zero, true)
        {
        }

        public override bool IsInvalid => handle == IntPtr.Zero;

        protected override bool ReleaseHandle()
        {
            var ftStatus = FtFunction.FT_Close(handle);
            return ftStatus == FtStatus.Ok;
        }
    }
}
