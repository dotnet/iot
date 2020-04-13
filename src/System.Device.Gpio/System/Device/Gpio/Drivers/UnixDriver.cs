// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System.Device.Gpio.Drivers
{
    public abstract class UnixDriver : GpioDriver
    {
        [Obsolete]
        protected UnixDriver()
        {
            if (Environment.OSVersion.Platform != PlatformID.Unix)
            {
                throw new PlatformNotSupportedException(GetType().Name + " is only supported on Linux/Unix");
            }
        }

        protected UnixDriver(Board board)
        : base(board)
        {
            if (Environment.OSVersion.Platform != PlatformID.Unix)
            {
                throw new PlatformNotSupportedException(GetType().Name + " is only supported on Linux/Unix");
            }
        }

        // TODO: remove try catch after https://github.com/dotnet/corefx/issues/32015 deployed
        [Obsolete("Use Board.CreateI2cDevice instead")]
        public static UnixDriver Create()
        {
            UnixDriver driver = null;
            try
            {
                driver = new LibGpiodDriver();
            }
            catch (PlatformNotSupportedException)
            {
                driver = new SysFsDriver();
            }

            return driver;
        }

        internal static UnixDriver Create(Board board)
        {
            UnixDriver driver = null;
            try
            {
                driver = new LibGpiodDriver(board);
            }
            catch (PlatformNotSupportedException)
            {
                driver = new SysFsDriver(board);
            }

            return driver;
        }
    }
}
