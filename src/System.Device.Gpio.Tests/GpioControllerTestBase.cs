// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Device.Gpio.Drivers;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace System.Device.Gpio.Tests;

public abstract class GpioControllerTestBase
{
    private readonly ITestOutputHelper _testOutputHelper;
    protected const int LedPin = 5;
    protected const int OutputPin = 5;
    protected const int InputPin = 6;
    private static readonly int WaitMilliseconds = 1000;

    protected GpioControllerTestBase(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    protected ITestOutputHelper Logger => _testOutputHelper;

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void PinValueStaysSameAfterDispose(bool closeAsHigh)
    {
        using var driver = GetTestDriver();
        if (driver is SysFsDriver)
        {
            // This check fails on the SysFsDriver, because it always sets the value to 0 when the pin is opened (but on close, the value does stay high)
            return;
        }

        using (GpioController controller = new GpioController(driver))
        {
            controller.OpenPin(OutputPin, PinMode.Output);
            controller.OpenPin(InputPin, PinMode.Input);
            controller.Write(OutputPin, closeAsHigh);
            Thread.SpinWait(100);
            Assert.Equal(closeAsHigh, controller.Read(InputPin));
        }

        using (GpioController controller = new GpioController(GetTestDriver()))
        {
            controller.OpenPin(InputPin, PinMode.Input);
            Assert.Equal(closeAsHigh, controller.Read(InputPin));
        }
    }

    [Fact]
    public void PinValueReadAndWrite()
    {
        using (GpioController controller = new GpioController(GetTestDriver()))
        {
            controller.OpenPin(OutputPin, PinMode.Output);
            controller.OpenPin(InputPin, PinMode.Input);
            controller.Write(OutputPin, PinValue.High);
            Thread.SpinWait(100);
            Assert.Equal(PinValue.High, controller.Read(InputPin));
            controller.Write(OutputPin, PinValue.Low);
            Thread.SpinWait(100);
            Assert.Equal(PinValue.Low, controller.Read(InputPin));
            controller.Write(OutputPin, PinValue.High);
            Thread.SpinWait(100);
            Assert.Equal(PinValue.High, controller.Read(InputPin));
        }
    }

    [Fact]
    public void PinCanChangeStateWhileItIsOpen()
    {
        using (GpioController controller = new GpioController(GetTestDriver()))
        {
            controller.OpenPin(OutputPin, PinMode.Input);
            Thread.SpinWait(100);
            controller.SetPinMode(OutputPin, PinMode.Output);
            controller.Write(OutputPin, PinValue.High);
            controller.SetPinMode(OutputPin, PinMode.Input);
            controller.ClosePin(OutputPin);
        }
    }

    [Fact]
    public void ThrowsInvalidOperationExceptionWhenPinIsNotOpened()
    {
        using (GpioController controller = new GpioController(GetTestDriver()))
        {
            Assert.Throws<InvalidOperationException>(() => controller.Write(OutputPin, PinValue.High));
            Assert.Throws<InvalidOperationException>(() => controller.Read(InputPin));
            Assert.Throws<InvalidOperationException>(() => controller.ClosePin(OutputPin));
            Assert.Throws<InvalidOperationException>(() => controller.SetPinMode(OutputPin, PinMode.Output));
            Assert.Throws<InvalidOperationException>(() => controller.GetPinMode(OutputPin));
        }
    }

    [Fact]
    public void IsPinOpenOnInputTest()
    {
        using (GpioController controller = new GpioController(GetTestDriver()))
        {
            // Open pin in input mode (default)
            Assert.False(controller.IsPinOpen(LedPin));
            controller.OpenPin(LedPin, PinMode.Input);
            Assert.True(controller.IsPinOpen(LedPin));
            controller.ClosePin(LedPin);
            Assert.False(controller.IsPinOpen(LedPin));
        }
    }

    [Fact]
    public void IsPinOpenOnOutputTest()
    {
        // Separate test to check the IsPinOpen works also when the PinMode is Output, See Bug #776
        using (GpioController controller = new GpioController(GetTestDriver()))
        {
            Assert.False(controller.IsPinOpen(LedPin));

            controller.OpenPin(LedPin, PinMode.Output);
            Assert.True(controller.IsPinOpen(LedPin));

            controller.Write(LedPin, PinValue.High);
            Assert.True(controller.IsPinOpen(LedPin));

            controller.Write(LedPin, PinValue.Low);
            Assert.True(controller.IsPinOpen(LedPin));
            controller.ClosePin(LedPin);
            Assert.False(controller.IsPinOpen(LedPin));
        }
    }

    [Fact]
    public void AllowWritingOnInputPin()
    {
        using (GpioController controller = new GpioController(GetTestDriver()))
        {
            controller.OpenPin(InputPin, PinMode.Input);
            controller.Write(InputPin, PinValue.High);
        }
    }

    [Fact]
    public void CanReadFromOutputPin()
    {
        using (GpioController controller = new GpioController(GetTestDriver()))
        {
            controller.OpenPin(OutputPin, PinMode.Output);

            controller.Write(OutputPin, PinValue.Low);
            Assert.Equal(PinValue.Low, controller.Read(OutputPin));

            controller.Write(OutputPin, PinValue.High);
            Assert.Equal(PinValue.High, controller.Read(OutputPin));
        }
    }

    [Fact]
    public void ThrowsIfReadingClosedPin()
    {
        using (GpioController controller = new GpioController(GetTestDriver()))
        {
            Assert.Throws<InvalidOperationException>(() => controller.Read(OutputPin));
        }
    }

    [Fact]
    public void ThrowsIfWritingClosedPin()
    {
        using (GpioController controller = new GpioController(GetTestDriver()))
        {
            Assert.Throws<InvalidOperationException>(() => controller.Write(OutputPin, PinValue.High));
        }
    }

    [Theory]
    [InlineData(PinMode.Output)]
    [InlineData(PinMode.Input)]
    [Trait("SkipOnTestRun", "Windows_NT")] // Currently, the Windows Driver is defaulting to InputPullDown, and it seems this cannot be changed
    public void OpenPinDefaultsModeToLastMode(PinMode modeToTest)
    {
        using var driver = GetTestDriver();
        if (driver is SysFsDriver)
        {
            // See issue #1581. There seems to be a library-version issue or some other random cause for this test to act differently on different hardware.
            return;
        }

        // This works for input/output on most systems, but not on pullup/down, since sometimes the hardware doesn't have read possibilities for those (ie. the Raspi 3)
        using (GpioController controller = new GpioController(driver))
        {
            controller.OpenPin(OutputPin);
            controller.SetPinMode(OutputPin, modeToTest);
            Assert.Equal(modeToTest, controller.GetPinMode(OutputPin));
            controller.ClosePin(OutputPin);
        }

        // Close controller, to make sure we're not caching
        using (GpioController controller = new GpioController(GetTestDriver()))
        {
            controller.OpenPin(OutputPin);
            Thread.Sleep(100);
            Assert.Equal(modeToTest, controller.GetPinMode(OutputPin));
        }
    }

    [Fact]
    public void AddCallbackTest()
    {
        bool wasCalled = false;
        using (GpioController controller = new GpioController(GetTestDriver()))
        {
            controller.OpenPin(InputPin, PinMode.Input);
            controller.OpenPin(OutputPin, PinMode.Output);
            controller.Write(OutputPin, PinValue.Low);
            controller.RegisterCallbackForPinValueChangedEvent(InputPin, PinEventTypes.Rising, Callback);
            controller.Write(OutputPin, PinValue.High);
            Thread.Sleep(WaitMilliseconds);
            Assert.True(wasCalled);
        }

        void Callback(object sender, PinValueChangedEventArgs pinValueChangedEventArgs)
        {
            wasCalled = true;
        }
    }

    [Fact]
    public void AddCallbackFallingEdgeNotDetectedTest()
    {
        using var driver = GetTestDriver();
        if (driver is SysFsDriver)
        {
            // This test is unreliable (flaky) with SysFs.
            // See https://github.com/dotnet/iot/issues/629 and possibly https://github.com/dotnet/iot/issues/1581
            return;
        }

        bool wasCalled = false;
        AutoResetEvent ev = new AutoResetEvent(false);
        using (GpioController controller = new GpioController(driver))
        {
            controller.OpenPin(InputPin, PinMode.Input);
            controller.OpenPin(OutputPin, PinMode.Output);
            controller.Write(OutputPin, PinValue.Low);
            controller.RegisterCallbackForPinValueChangedEvent(InputPin, PinEventTypes.Falling, Callback);
            // Sometimes, we get an extra event just at the beginning - wait for it and then drop it
            ev.WaitOne(1000);
            wasCalled = false;
            controller.Write(OutputPin, PinValue.High);
            controller.UnregisterCallbackForPinValueChangedEvent(InputPin, Callback);
            Assert.False(wasCalled);
        }

        ev.Dispose();

        void Callback(object sender, PinValueChangedEventArgs pinValueChangedEventArgs)
        {
            if (pinValueChangedEventArgs.PinNumber != InputPin)
            {
                return;
            }

            ev.Set();
            wasCalled = true;
        }
    }

    [Fact]
    public void AddCallbackRemoveCallbackTest()
    {
        int risingEventOccurredCount = 0, fallingEventOccurredCount = 0;
        using (GpioController controller = new GpioController(GetTestDriver()))
        {
            controller.OpenPin(InputPin, PinMode.Input);
            controller.OpenPin(OutputPin, PinMode.Output);
            controller.Write(OutputPin, PinValue.Low);

            controller.RegisterCallbackForPinValueChangedEvent(InputPin, PinEventTypes.Rising, (o, e) =>
            {
                risingEventOccurredCount++;
            });
            controller.RegisterCallbackForPinValueChangedEvent(InputPin, PinEventTypes.Rising, Callback);
            controller.RegisterCallbackForPinValueChangedEvent(InputPin, PinEventTypes.Rising, (o, e) =>
            {
                risingEventOccurredCount++;
                if (fallingEventOccurredCount == 4)
                {
                    controller.UnregisterCallbackForPinValueChangedEvent(InputPin, Callback);
                }
            });
            controller.RegisterCallbackForPinValueChangedEvent(InputPin, PinEventTypes.Falling, (o, e) =>
            {
                fallingEventOccurredCount++;
            });

            for (int i = 0; i < 10; i++)
            {
                controller.Write(OutputPin, PinValue.High);
                Thread.Sleep(WaitMilliseconds);
                controller.Write(OutputPin, PinValue.Low);
                Thread.Sleep(WaitMilliseconds);
            }

            Assert.Equal(25, risingEventOccurredCount);
            Assert.Equal(10, fallingEventOccurredCount);

            void Callback(object sender, PinValueChangedEventArgs e)
            {
                risingEventOccurredCount++;
            }
        }
    }

    [Fact]
    public void AddCallbackRemoveAllCallbackTest()
    {
        using GpioDriver testDriver = GetTestDriver();
        // Skipping the test for now when using the SysFsDriver or the RaspberryPi3Driver given that this test is flaky for those drivers.
        // Issue tracking this problem is https://github.com/dotnet/iot/issues/629
        if (testDriver is SysFsDriver || testDriver is RaspberryPi3Driver)
        {
            return;
        }

        RetryHelper.Execute(() =>
        {
            int risingEventOccurredCount = 0, fallingEventOccurredCount = 0;
            using (GpioController controller = new GpioController(testDriver))
            {
                controller.OpenPin(InputPin, PinMode.Input);
                controller.OpenPin(OutputPin, PinMode.Output);
                controller.Write(OutputPin, PinValue.Low);

                controller.RegisterCallbackForPinValueChangedEvent(InputPin, PinEventTypes.Falling, Callback1);
                controller.RegisterCallbackForPinValueChangedEvent(InputPin, PinEventTypes.Falling, Callback2);
                controller.RegisterCallbackForPinValueChangedEvent(InputPin, PinEventTypes.Falling, Callback3);
                controller.RegisterCallbackForPinValueChangedEvent(InputPin, PinEventTypes.Rising, Callback4);

                controller.Write(OutputPin, PinValue.High);
                Thread.Sleep(WaitMilliseconds);

                controller.UnregisterCallbackForPinValueChangedEvent(InputPin, Callback1);
                controller.UnregisterCallbackForPinValueChangedEvent(InputPin, Callback2);
                controller.UnregisterCallbackForPinValueChangedEvent(InputPin, Callback3);
                controller.UnregisterCallbackForPinValueChangedEvent(InputPin, Callback4);

                Thread.Sleep(WaitMilliseconds);
                controller.Write(OutputPin, PinValue.Low);
                Thread.Sleep(WaitMilliseconds);
                controller.Write(OutputPin, PinValue.High);

                Assert.Equal(1, risingEventOccurredCount);
                Assert.Equal(0, fallingEventOccurredCount);

                void Callback1(object sender, PinValueChangedEventArgs e)
                {
                    fallingEventOccurredCount++;
                }

                void Callback2(object sender, PinValueChangedEventArgs e)
                {
                    fallingEventOccurredCount++;
                }

                void Callback3(object sender, PinValueChangedEventArgs e)
                {
                    fallingEventOccurredCount++;
                }

                void Callback4(object sender, PinValueChangedEventArgs e)
                {
                    risingEventOccurredCount++;
                }
            }
        });
    }

    [Fact]
    public void AddCallbackRemoveAllCallbackAndAddCallbackAgainTest()
    {
        int callback1FiredTimes = 0, callback2FiredTimes = 0;
        using (GpioController controller = new GpioController(GetTestDriver()))
        {
            controller.OpenPin(InputPin, PinMode.Input);
            controller.OpenPin(OutputPin, PinMode.Output);
            controller.Write(OutputPin, PinValue.Low);
            Thread.Sleep(WaitMilliseconds);

            for (int i = 1; i < 3; i++)
            {
                controller.RegisterCallbackForPinValueChangedEvent(InputPin, PinEventTypes.Rising, Callback1Rising);
                controller.RegisterCallbackForPinValueChangedEvent(InputPin, PinEventTypes.Falling, Callback2Falling);

                controller.Write(OutputPin, PinValue.High);
                Thread.Sleep(WaitMilliseconds);
                controller.Write(OutputPin, PinValue.Low);
                Thread.Sleep(WaitMilliseconds);

                controller.UnregisterCallbackForPinValueChangedEvent(InputPin, Callback1Rising);
                controller.UnregisterCallbackForPinValueChangedEvent(InputPin, Callback2Falling);

                Assert.Equal(i, callback1FiredTimes);
                Assert.Equal(i, callback2FiredTimes);
            }

            void Callback1Rising(object sender, PinValueChangedEventArgs e)
            {
                callback1FiredTimes++;
            }

            void Callback2Falling(object sender, PinValueChangedEventArgs e)
            {
                callback2FiredTimes++;
            }
        }
    }

    [Fact]
    public void WaitForEventCancelAfter10MillisecondsTest()
    {
        using (GpioController controller = new GpioController(GetTestDriver()))
        {
            CancellationTokenSource tokenSource = new CancellationTokenSource(WaitMilliseconds);
            controller.OpenPin(InputPin, PinMode.Input);
            controller.OpenPin(OutputPin, PinMode.Output);
            controller.Write(OutputPin, PinValue.Low);

            WaitForEventResult result = controller.WaitForEvent(InputPin, PinEventTypes.Falling, tokenSource.Token);

            Assert.True(result.TimedOut);
            // Result is expected to be None if the timeout elapsed
            Assert.Equal(PinEventTypes.None, result.EventTypes);
        }
    }

    [Fact]
    public void ThrowsIfWaitingOnClosedPin()
    {
        using (GpioController controller = new GpioController(GetTestDriver()))
        {
            Assert.Throws<InvalidOperationException>(() => controller.WaitForEvent(InputPin, PinEventTypes.Falling, CancellationToken.None));
        }
    }

    [Fact]
    public void WaitForEventRisingEdgeTest()
    {
        using (GpioController controller = new GpioController(GetTestDriver()))
        {
            CancellationTokenSource tokenSource = new CancellationTokenSource();
            controller.OpenPin(InputPin, PinMode.Input);
            controller.OpenPin(OutputPin, PinMode.Output);
            controller.Write(OutputPin, PinValue.Low);

            Task.Run(() =>
            {
                Thread.Sleep(WaitMilliseconds);
                controller.Write(OutputPin, PinValue.High);
            });

            WaitForEventResult result = controller.WaitForEvent(InputPin, PinEventTypes.Rising, tokenSource.Token);

            Assert.False(result.TimedOut);
            Assert.Equal(PinEventTypes.Rising, result.EventTypes);
        }
    }

    [Fact]
    public void WaitForEventFallingEdgeTest()
    {
        TimeoutHelper.CompletesInTime(() =>
        {
            using (GpioController controller = new GpioController(GetTestDriver()))
            {
                CancellationTokenSource tokenSource = new CancellationTokenSource();
                controller.OpenPin(InputPin, PinMode.Input);
                controller.OpenPin(OutputPin, PinMode.Output);
                controller.Write(OutputPin, PinValue.Low);

                Task.Run(() =>
                {
                    controller.Write(OutputPin, PinValue.High);
                    Thread.Sleep(WaitMilliseconds);
                    controller.Write(OutputPin, PinValue.Low);
                });

                WaitForEventResult result = controller.WaitForEvent(InputPin, PinEventTypes.Falling, tokenSource.Token);

                Assert.False(result.TimedOut);
                Assert.Equal(PinEventTypes.Falling, result.EventTypes);
            }
        }, TimeSpan.FromSeconds(30));
    }

    [Fact]
    public async Task WaitForEventBothEdgesTest()
    {
        using (GpioController controller = new GpioController(GetTestDriver()))
        {
            CancellationTokenSource tokenSource = new CancellationTokenSource();
            tokenSource.CancelAfter(2000);
            controller.OpenPin(InputPin, PinMode.Input);
            controller.OpenPin(OutputPin, PinMode.Output);
            controller.Write(OutputPin, PinValue.High);

            // Wait for any events that happen because of the initialization
            controller.WaitForEvent(InputPin, PinEventTypes.Falling | PinEventTypes.Rising, tokenSource.Token);
            tokenSource.Dispose();

            var task = Task.Run(() =>
            {
                controller.Write(OutputPin, PinValue.High);
                Thread.Sleep(WaitMilliseconds);
                controller.Write(OutputPin, PinValue.Low);
                Thread.Sleep(WaitMilliseconds);
                controller.Write(OutputPin, PinValue.High);
            });

            tokenSource = new CancellationTokenSource();
            tokenSource.CancelAfter(WaitMilliseconds * 4);

            // First event is falling, second is rising
            WaitForEventResult result = controller.WaitForEvent(InputPin, PinEventTypes.Falling | PinEventTypes.Rising, tokenSource.Token);
            Assert.False(result.TimedOut);
            Assert.Equal(PinEventTypes.Falling, result.EventTypes);
            result = controller.WaitForEvent(InputPin, PinEventTypes.Falling | PinEventTypes.Rising, tokenSource.Token);
            Assert.False(result.TimedOut);
            Assert.Equal(PinEventTypes.Rising, result.EventTypes);
            await task.WaitAsync(TimeSpan.FromSeconds(30)); // Should end long before that
            tokenSource.Dispose();
        }
    }

    [Fact]
    [Trait("SkipOnTestRun", "Windows_NT")] // WindowsDriver  is not very fast so this test is flaky on it.
    public void FastInterruptHandling()
    {
        const int numPulses = 100;
        // These are in ms
        const int pulseLength = 1;
        const int waitTime = 20;
        const double acceptableTimeFactor = 2.0;

        int numInterrupts = 0;
        int numRisingEdges = 0;
        using (GpioController controller = new GpioController(GetTestDriver()))
        {
            controller.OpenPin(InputPin, PinMode.Input);
            controller.OpenPin(OutputPin, PinMode.Output);
            controller.Write(OutputPin, PinValue.Low);
            while (controller.Read(InputPin) == PinValue.High)
            {
            }

            controller.RegisterCallbackForPinValueChangedEvent(InputPin, PinEventTypes.Rising, Callback);
            // Ensure the poll thread is ready before we continue the actual loop
            // If the triggers are generated by external hardware (which is what the interrupt triggering
            // is for), we don't know when the pulses start, so no synchronisation possible there anyway
            Thread.Sleep(10);
            Stopwatch w = Stopwatch.StartNew();
            for (int i = 0; i < numPulses; i++)
            {
                controller.Write(OutputPin, PinValue.High);
                Thread.Sleep(TimeSpan.FromMilliseconds(pulseLength));
                controller.Write(OutputPin, PinValue.Low);
                Thread.Sleep(TimeSpan.FromMilliseconds(waitTime));
            }

            TimeSpan elapsed = w.Elapsed;
            Thread.Sleep(10);
            controller.UnregisterCallbackForPinValueChangedEvent(InputPin, Callback);
            // All pulses must be marked as rising edge
            Assert.Equal(numInterrupts, numRisingEdges);
            Assert.True(numInterrupts >= numPulses - 1, $"Expected at least {numPulses - 1}, got only {numInterrupts}"); // Allow one missing pulse
            // That's how long this test should last (at most).
            // It will usually be around 3s, but on Windows it's around 4s (for reasons unknown)
            double expectedMillis = numPulses * (pulseLength + waitTime);
            Assert.True(elapsed < TimeSpan.FromMilliseconds(expectedMillis * acceptableTimeFactor), $"Used {w.Elapsed.TotalMilliseconds}ms, expected max {expectedMillis * acceptableTimeFactor}ms");
            // Write the state of the test to the log, even if it passed (for detailed analysis)
            _testOutputHelper.WriteLine($"Got {numInterrupts} out of {numPulses} expected. Expected a duration of {expectedMillis}, actually used {elapsed.TotalMilliseconds}ms");
        }

        void Callback(object sender, PinValueChangedEventArgs pinValueChangedEventArgs)
        {
            numInterrupts++;
            // Console.WriteLine($"Seen pulse {numInterrupts}");
            Assert.Equal(InputPin, pinValueChangedEventArgs.PinNumber);
            if (pinValueChangedEventArgs.ChangeType == PinEventTypes.Rising)
            {
                numRisingEdges++;
            }
        }
    }

    [Fact]
    public void UsingPinAfterDriverDisposedCausesException()
    {
        var controller = new GpioController(GetTestDriver());
        var pin6 = controller.OpenPin(InputPin, PinMode.Input);
        controller.Dispose();
        bool correctExceptionSeen = false;
        try
        {
            pin6.Read();
        }
        catch (ObjectDisposedException)
        {
            correctExceptionSeen = true;
        }

        Assert.True(correctExceptionSeen);
    }

    [Fact]
    public void UsingControllerAfterDisposeCausesException()
    {
        var controller = new GpioController(GetTestDriver());
        var pin6 = controller.OpenPin(InputPin, PinMode.Input);
        controller.Dispose();
        Assert.Throws<ObjectDisposedException>(() => controller.OpenPin(InputPin, PinMode.Input));
    }

    protected abstract GpioDriver GetTestDriver();

    protected bool IsRaspi4()
    {
        if (File.Exists("/proc/device-tree/model"))
        {
            string model = File.ReadAllText("/proc/device-tree/model", Text.Encoding.ASCII);
            if (model.Contains("Raspberry Pi 4") || model.Contains("Raspberry Pi Compute Module 4"))
            {
                return true;
            }
        }

        return false;
    }
}
