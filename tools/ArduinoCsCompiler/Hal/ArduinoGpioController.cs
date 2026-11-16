// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Device.Gpio;
using ArduinoCsCompiler.Runtime;

namespace ArduinoCsCompiler.Hal
{
    [ArduinoReplacement(typeof(GpioController), IncludingPrivates = true)]
    internal class ArduinoGpioController
    {
        [ArduinoImplementation]
        public static GpioDriver GetBestDriverForBoard()
        {
            return new ArduinoNativeGpioDriver();
        }
    }
}
