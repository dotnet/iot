// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Gui
{
    /// <summary>
    /// Supports creation of virtual pointing devices for various operating systems
    /// </summary>
    public static class VirtualPointingDevice
    {
        /// <summary>
        /// Create a pointing device that uses relative coordinates (e.g. to simulate a traditional mouse or a joystick)
        /// </summary>
        /// <returns>A pointing device</returns>
        /// <exception cref="NotImplementedException">This operation is not currently implemented</exception>
        /// <exception cref="PlatformNotSupportedException">The operation is not supported on this platform</exception>
        public static IPointingDevice CreateRelative()
        {
            var os = Environment.OSVersion;
            if (os.Platform == PlatformID.Unix)
            {
                return new MouseClickSimulatorUInputRelative();
            }

            if (os.Platform == PlatformID.Win32NT)
            {
                return new WindowsMouseSimulator();
            }

            throw new PlatformNotSupportedException();
        }

        /// <summary>
        /// Create a pointing device that uses absolute coordinates (e.g. to simulate a touchscreen)
        /// </summary>
        /// <param name="width">Width of the input area (screen size)</param>
        /// <param name="height">Height of the input area (screen size)</param>
        /// <returns>A pointing device</returns>
        /// <remarks>
        /// To obtain the size of the virtual screen <see cref="ScreenCapture.ScreenSize"/> can be used.
        /// </remarks>
        /// <exception cref="NotImplementedException">This operation is not currently implemented</exception>
        /// <exception cref="PlatformNotSupportedException">The operation is not supported on this platform</exception>
        public static IPointingDevice CreateAbsolute(int width, int height)
        {
            var os = Environment.OSVersion;
            if (os.Platform == PlatformID.Unix)
            {
                return new MouseClickSimulatorUInputAbsolute(width, height);
            }

            if (os.Platform == PlatformID.Win32NT)
            {
                return new WindowsMouseSimulator();
            }

            throw new PlatformNotSupportedException();
        }
    }
}
