// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Runtime.InteropServices;

namespace Iot.Device.Ft4222
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
