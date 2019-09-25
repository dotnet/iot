// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Device.Gpio.Drivers;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace System.Device.Gpio.Tests
{
    public abstract class GpioControllerTestBase
    {
        private const int LedPin = 5;
        private const int OutputPin = 5;
        private const int InputPin = 6;
        private static readonly int WaitMilliseconds = 1000;

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
        public void ThrowsIfReadingFromOutputPin()
        {
            using (GpioController controller = new GpioController(GetTestNumberingScheme(), GetTestDriver()))
            {
                controller.OpenPin(OutputPin, PinMode.Output);
                Assert.Throws<InvalidOperationException>(() => controller.Read(OutputPin));
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
            using (GpioController controller = new GpioController(GetTestNumberingScheme(), GetTestDriver()))
            {
                controller.OpenPin(InputPin, PinMode.Input);
                controller.OpenPin(OutputPin, PinMode.Output);
                controller.Write(OutputPin, PinValue.Low);
                controller.RegisterCallbackForPinValueChangedEvent(InputPin, PinEventTypes.Falling, Callback);
                controller.Write(OutputPin, PinValue.High);
                Thread.Sleep(WaitMilliseconds);
                Assert.False(wasCalled);
            }

            void Callback(object sender, PinValueChangedEventArgs pinValueChangedEventArgs)
            {
                Debug.WriteLine("Oops I was called!");
                wasCalled = true;
            }
        }

        [Fact]
        public void AddCallbackRemoveCallbackTest()
        {
            int risingEventOccuredCount = 0, fallingEventOccuredCount = 0;
            using (GpioController controller = new GpioController(GetTestNumberingScheme(), GetTestDriver()))
            {
                controller.OpenPin(InputPin, PinMode.Input);
                controller.OpenPin(OutputPin, PinMode.Output);
                controller.Write(OutputPin, PinValue.Low);

                controller.RegisterCallbackForPinValueChangedEvent(InputPin, PinEventTypes.Rising, (o, e) =>
                {
                    risingEventOccuredCount++;
                });
                controller.RegisterCallbackForPinValueChangedEvent(InputPin, PinEventTypes.Rising, Callback);
                controller.RegisterCallbackForPinValueChangedEvent(InputPin, PinEventTypes.Rising, (o, e) =>
                {
                    risingEventOccuredCount++;
                    if (fallingEventOccuredCount == 4)
                    {
                        controller.UnregisterCallbackForPinValueChangedEvent(InputPin, Callback);
                    }
                });
                controller.RegisterCallbackForPinValueChangedEvent(InputPin, PinEventTypes.Falling, (o, e) =>
                {
                    fallingEventOccuredCount++;
                });

                for (int i = 0; i < 10; i++)
                {
                    controller.Write(OutputPin, PinValue.High);
                    Thread.Sleep(WaitMilliseconds);
                    controller.Write(OutputPin, PinValue.Low);
                    Thread.Sleep(WaitMilliseconds);
                }

                Assert.Equal(25, risingEventOccuredCount);
                Assert.Equal(10, fallingEventOccuredCount);

                void Callback(object sender, PinValueChangedEventArgs e)
                {
                    risingEventOccuredCount++;
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
                int risingEventOccuredCount = 0, fallingEventOccuredCount = 0;
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

                    Assert.Equal(1, risingEventOccuredCount);
                    Assert.Equal(0, fallingEventOccuredCount);

                    void Callback1(object sender, PinValueChangedEventArgs e)
                    {
                        fallingEventOccuredCount++;
                    }

                    void Callback2(object sender, PinValueChangedEventArgs e)
                    {
                        fallingEventOccuredCount++;
                    }

                    void Callback3(object sender, PinValueChangedEventArgs e)
                    {
                        fallingEventOccuredCount++;
                    }

                    void Callback4(object sender, PinValueChangedEventArgs e)
                    {
                        risingEventOccuredCount++;
                    }
                }
            });
        }

        [Fact]
        [Trait("SkipOnTestRun", "Windows_NT")] // The windows driver is returning none as the event type.
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
                Assert.Equal(PinEventTypes.Falling, result.EventTypes);
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

        protected abstract GpioDriver GetTestDriver();
        protected abstract PinNumberingScheme GetTestNumberingScheme();
    }
}
