// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Device.Gpio.Tests;

namespace System.Device.Gpio.Drivers.Tests
{
    // TODO: Uncomment making public once the LibGpiodTests stop crashing with SegFault.
    // public
    class LibGpiodDriverTests : GpioControllerTestBase
    {
        protected override GpioDriver GetTestDriver() => new LibGpiodDriver();

        protected override PinNumberingScheme GetTestNumberingScheme() => PinNumberingScheme.Logical;
    }
}
