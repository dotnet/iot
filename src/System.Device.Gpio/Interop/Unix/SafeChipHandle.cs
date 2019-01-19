// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Runtime.InteropServices;

namespace System.Device.Gpio
{
    internal class SafeChipHandle : SafeHandle
    {
        internal SafeChipHandle() : base(IntPtr.Zero, true) { }

        protected override bool ReleaseHandle()
        {
            Interop.CloseChip(handle);
            return true;
        }

        public override bool IsInvalid => handle == IntPtr.Zero || handle == new IntPtr(-1);
    }
}
