// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Device.Gpio.Drivers;
using System.Diagnostics;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace System.Device.Gpio.Tests;

[Trait("feature", "gpio")]
[Trait("feature", "gpio-libgpiod")]
[Trait("SkipOnTestRun", "Windows_NT")]
public class LibGpiodV2DriverTests : GpioControllerTestBase
{
    private const int ChipNumber = 0;

    public LibGpiodV2DriverTests(ITestOutputHelper testOutputHelper)
        : base(testOutputHelper)
    {
    }

    protected override GpioDriver GetTestDriver() => new LibGpiodDriver(ChipNumber, LibGpiodDriverVersion.V2);

    protected override PinNumberingScheme GetTestNumberingScheme() => PinNumberingScheme.Logical;

    [Fact]
    public async Task WaitEdgeEvents_ShouldNotBlockOtherRequestOperations()
    {
        var largeWaitForEventsTimeout = TimeSpan.FromSeconds(5);
        using var gpioController = new GpioController(GetTestNumberingScheme(), new LibGpiodDriver(ChipNumber, LibGpiodDriverVersion.V2));

        gpioController.OpenPin(InputPin);

        // make event observer start waiting for events
        gpioController.RegisterCallbackForPinValueChangedEvent(InputPin, PinEventTypes.Falling | PinEventTypes.Rising, (_, args) =>
        {
        });

        // waiting is done in a background thread, so delay until the background thread is waiting
        await Task.Delay(100);

        // perform any other operation (read) and measure time
        var sw = new Stopwatch();
        sw.Start();
        gpioController.Read(InputPin);
        sw.Stop();

        // the operation should finish fairly quick
        Assert.True(sw.Elapsed < TimeSpan.FromSeconds(1));
    }
}
