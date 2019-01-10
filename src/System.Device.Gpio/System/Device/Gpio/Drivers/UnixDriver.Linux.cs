// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;


namespace System.Device.Gpio.Drivers
{
    public abstract class UnixDriver : GpioDriver
    {
        // TODO: remove try catch after https://github.com/dotnet/corefx/issues/32015 deployed
        public static UnixDriver InitUnixDriver() {
            UnixDriver driver = null;
            try
            {
                driver = new LibGpiodDriver();
            }
            catch (DllNotFoundException e)
            {
                driver = new SysFSDriver();
            }
            return driver;
        }
    }
}
