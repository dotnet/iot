// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Device.Gpio.Drivers.Libgpiod.V2;
using Xunit;
using Xunit.Abstractions;

namespace System.Device.Gpio.Tests;

[Trait("feature", "gpio")]
[Trait("feature", "gpio-libgpiod")]
[Trait("SkipOnTestRun", "Windows_NT")]
public class LibGpiodV2DriverTests : GpioControllerTestBase
{
    /// <summary>
    /// Set to a low value to not delay tests too much, see constructor of <see cref="LibGpiodV2Driver"/>
    /// </summary>
    private static readonly TimeSpan s_eventReadTimeout = TimeSpan.FromMilliseconds(1);

    public LibGpiodV2DriverTests(ITestOutputHelper testOutputHelper)
        : base(testOutputHelper)
    {
    }

    protected override GpioDriver GetTestDriver() => new LibGpiodV2Driver(0, waitForEventsTimeout: s_eventReadTimeout);

    protected override PinNumberingScheme GetTestNumberingScheme() => PinNumberingScheme.Logical;
}
