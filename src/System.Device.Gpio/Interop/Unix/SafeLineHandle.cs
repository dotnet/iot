// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.InteropServices;

namespace System.Device.Gpio
{
    /// <summary>
    /// Pointer to a pin.
    /// </summary>
    internal class SafeLineHandle : SafeHandle
    {
        public PinMode PinMode { get; set; }

        public SafeLineHandle()
            : base(IntPtr.Zero, true)
        {
        }

        protected override bool ReleaseHandle()
        {
            // Contrary to intuition, this does not invalidate the handle (see comment on declaration)
            Interop.libgpiod.gpiod_line_release(handle);
            return true;
        }

        /// <summary>
        /// Release the lock on the line handle. <see cref="Interop.libgpiod.gpiod_line_release"/>
        /// </summary>
        public void ReleaseLock()
        {
            ReleaseHandle();
        }

        public override bool IsInvalid => handle == IntPtr.Zero || handle == Interop.libgpiod.InvalidHandleValue;
    }
}
