// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Device.Gpio.Drivers;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace System.Device.Gpio.Tests
{
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

        [Fact]
        [Trait("SkipOnTestRun", "Windows_NT")] // The WindowsDriver is kept as High when disposed.
        public void PinValueReturnsToLowAfterDispose()
        {
            using (GpioController controller = new GpioController(GetTestNumberingScheme(), GetTestDriver()))
            {
                controller.OpenPin(OutputPin, PinMode.Output);
                controller.OpenPin(InputPin, PinMode.Input);
                controller.Write(OutputPin, PinValue.High);
                Thread.SpinWait(100);
                Assert.Equal(PinValue.High, controller.Read(InputPin));
            }

            using (GpioController controller = new GpioController(GetTestNumberingScheme(), GetTestDriver()))
            {
                controller.OpenPin(OutputPin, PinMode.Output);
                controller.OpenPin(InputPin, PinMode.Input);
                Assert.Equal(PinValue.Low, controller.Read(InputPin));
            }
        }

        [Fact]
        public void PinValueReadAndWrite()
        {
            using (GpioController controller = new GpioController(GetTestNumberingScheme(), GetTestDriver()))
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
        public void ThrowsInvalidOperationExceptionWhenPinIsNotOpened()
        {
            using (GpioController controller = new GpioController(GetTestNumberingScheme(), GetTestDriver()))
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
            using (GpioController controller = new GpioController(GetTestNumberingScheme(), GetTestDriver()))
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
            using (GpioController controller = new GpioController(GetTestNumberingScheme(), GetTestDriver()))
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
        public void ThrowsIfWritingOnInputPin()
        {
            using (GpioController controller = new GpioController(GetTestNumberingScheme(), GetTestDriver()))
            {
                controller.OpenPin(InputPin, PinMode.Input);
                Assert.Throws<InvalidOperationException>(() => controller.Write(InputPin, PinValue.High));
            }
        }

        [Fact]
        public void CanReadFromOutputPin()
        {
            using (GpioController controller = new GpioController(GetTestNumberingScheme(), GetTestDriver()))
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
            using (GpioController controller = new GpioController(GetTestNumberingScheme(), GetTestDriver()))
            {
                Assert.Throws<InvalidOperationException>(() => controller.Read(OutputPin));
            }
        }

        [Fact]
        public void ThrowsIfWritingClosedPin()
        {
            using (GpioController controller = new GpioController(GetTestNumberingScheme(), GetTestDriver()))
            {
                Assert.Throws<InvalidOperationException>(() => controller.Write(OutputPin, PinValue.High));
            }
        }

        [Fact]
        [Trait("SkipOnTestRun", "Windows_NT")] // Currently, the Windows Driver is defaulting to InputPullDown instead of Input when Closed/Opened.
        public void OpenPinDefaultsModeToInput()
        {
            using (GpioController controller = new GpioController(GetTestNumberingScheme(), GetTestDriver()))
            {
                controller.OpenPin(OutputPin);
                Assert.Equal(PinMode.Input, controller.GetPinMode(OutputPin));
                controller.SetPinMode(OutputPin, PinMode.Output);
                controller.ClosePin(OutputPin);
                controller.OpenPin(OutputPin);
                Assert.Equal(PinMode.Input, controller.GetPinMode(OutputPin));
            }
        }

        [Fact]
        public void AddCallbackTest()
        {
            bool wasCalled = false;
            using (GpioController controller = new GpioController(GetTestNumberingScheme(), GetTestDriver()))
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
            bool wasCalled = false;
            AutoResetEvent ev = new AutoResetEvent(false);
            using (GpioController controller = new GpioController(GetTestNumberingScheme(), GetTestDriver()))
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
            using (GpioController controller = new GpioController(GetTestNumberingScheme(), GetTestDriver()))
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
            GpioDriver testDriver = GetTestDriver();
            // Skipping the test for now when using the SysFsDriver or the RaspberryPi3Driver given that this test is flaky for those drivers.
            // Issue tracking this problem is https://github.com/dotnet/iot/issues/629
            if (testDriver is SysFsDriver || testDriver is RaspberryPi3Driver)
            {
                return;
            }

            RetryHelper.Execute(() =>
            {
                int risingEventOccurredCount = 0, fallingEventOccurredCount = 0;
                using (GpioController controller = new GpioController(GetTestNumberingScheme(), testDriver))
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
        public void WaitForEventCancelAfter10MillisecondsTest()
        {
            using (GpioController controller = new GpioController(GetTestNumberingScheme(), GetTestDriver()))
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
            using (GpioController controller = new GpioController(GetTestNumberingScheme(), GetTestDriver()))
            {
                Assert.Throws<InvalidOperationException>(() => controller.WaitForEvent(InputPin, PinEventTypes.Falling, CancellationToken.None));
            }
        }

        [Fact]
        public void WaitForEventRisingEdgeTest()
        {
            using (GpioController controller = new GpioController(GetTestNumberingScheme(), GetTestDriver()))
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
            using (GpioController controller = new GpioController(GetTestNumberingScheme(), GetTestDriver()))
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
        }

        [Fact]
        public void WaitForEventBothEdgesTest()
        {
            using (GpioController controller = new GpioController(GetTestNumberingScheme(), GetTestDriver()))
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
                Assert.True(task.Wait(TimeSpan.FromSeconds(30))); // Should end long before that
                tokenSource.Dispose();
            }
        }

        [Fact]
        // [Trait("SkipOnTestRun", "Windows_NT")] // This should work on windows as well...
        public void FastInterruptHandling()
        {
            const int numPulses = 100;
            // These are in ms
            const int pulseLength = 1;
            const int waitTime = 20;
            const double acceptableTimeFactor = 1.7;

            int numInterrupts = 0;
            int numRisingEdges = 0;
            using (GpioController controller = new GpioController(GetTestNumberingScheme(), GetTestDriver()))
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

        protected abstract GpioDriver GetTestDriver();
        protected abstract PinNumberingScheme GetTestNumberingScheme();
    }
}
