// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace System.Device.Gpio.Tests
{
    public abstract class GpioControllerTestBase
    {
        private const int LedPin = 18;
        private const int OutputPin = 16;
        private const int InputPin = 12;
        private static readonly int WaitMilliSeconds = 1000;

        [Fact]
        public void ControllerCanTurnOnLEDs()
        {
            using (GpioController controller = new GpioController(GetTestNumberingScheme(), GetTestDriver()))
            {
                controller.OpenPin(LedPin, PinMode.Output);
                Thread.Sleep(1_000);
                controller.Write(LedPin, PinValue.High);
                Thread.Sleep(TimeSpan.FromSeconds(1));
                controller.Write(LedPin, PinValue.Low);
            }
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
        public void IsPinOpenTest()
        {
            using (GpioController controller = new GpioController(GetTestNumberingScheme(), GetTestDriver()))
            {
                Assert.False(controller.IsPinOpen(LedPin));
                controller.OpenPin(LedPin);
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
                controller.RegisterCallbackForPinValueChangedEvent(InputPin, PinEventTypes.Rising, callback);
                controller.Write(OutputPin, PinValue.High);
                Thread.Sleep(WaitMilliSeconds);
                Assert.True(wasCalled);
            }

            void callback(object sender, PinValueChangedEventArgs pinValueChangedEventArgs)
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
                controller.RegisterCallbackForPinValueChangedEvent(InputPin, PinEventTypes.Falling, callback);
                controller.Write(OutputPin, PinValue.High);
                Thread.Sleep(WaitMilliSeconds);
                Assert.False(wasCalled);
            }

            void callback(object sender, PinValueChangedEventArgs pinValueChangedEventArgs)
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

                controller.RegisterCallbackForPinValueChangedEvent(InputPin, PinEventTypes.Rising, (o, e) => {
                    risingEventOccuredCount++;
                });                        
                controller.RegisterCallbackForPinValueChangedEvent(InputPin, PinEventTypes.Rising, callback);
                controller.RegisterCallbackForPinValueChangedEvent(InputPin, PinEventTypes.Rising, (o, e) => {
                    risingEventOccuredCount++;
                    if (fallingEventOccuredCount == 4)
                    {
                        controller.UnregisterCallbackForPinValueChangedEvent(InputPin, callback);
                    }
                });
                controller.RegisterCallbackForPinValueChangedEvent(InputPin, PinEventTypes.Falling, (o, e) => {
                    fallingEventOccuredCount++;
                });

                for (int i = 0; i < 10; i++)
                {
                    controller.Write(OutputPin, PinValue.High);
                    Thread.Sleep(WaitMilliSeconds);
                    controller.Write(OutputPin, PinValue.Low);
                    Thread.Sleep(WaitMilliSeconds);
                }

                Assert.Equal(25, risingEventOccuredCount);
                Assert.Equal(10, fallingEventOccuredCount);

                void callback(object sender, PinValueChangedEventArgs e)
                {
                    risingEventOccuredCount++;
                }
            }
        }

        [Fact]
        public void AddCallbackRemoveAllCallbackTest()
        {
            RetryHelper.Execute(() =>
            {
                int risingEventOccuredCount = 0, fallingEventOccuredCount = 0;
                using (GpioController controller = new GpioController(GetTestNumberingScheme(), GetTestDriver()))
                {
                    controller.OpenPin(InputPin, PinMode.Input);
                    controller.OpenPin(OutputPin, PinMode.Output);
                    controller.Write(OutputPin, PinValue.Low);

                    controller.RegisterCallbackForPinValueChangedEvent(InputPin, PinEventTypes.Falling, callback1);
                    controller.RegisterCallbackForPinValueChangedEvent(InputPin, PinEventTypes.Falling, callback2);
                    controller.RegisterCallbackForPinValueChangedEvent(InputPin, PinEventTypes.Falling, callback3);
                    controller.RegisterCallbackForPinValueChangedEvent(InputPin, PinEventTypes.Rising, callback4);

                    controller.Write(OutputPin, PinValue.High);
                    Thread.Sleep(WaitMilliSeconds);

                    controller.UnregisterCallbackForPinValueChangedEvent(InputPin, callback1);
                    controller.UnregisterCallbackForPinValueChangedEvent(InputPin, callback2);
                    controller.UnregisterCallbackForPinValueChangedEvent(InputPin, callback3);
                    controller.UnregisterCallbackForPinValueChangedEvent(InputPin, callback4);

                    Thread.Sleep(WaitMilliSeconds);
                    controller.Write(OutputPin, PinValue.Low);
                    Thread.Sleep(WaitMilliSeconds);
                    controller.Write(OutputPin, PinValue.High);

                    Assert.Equal(1, risingEventOccuredCount);
                    Assert.Equal(0, fallingEventOccuredCount);

                    void callback1(object sender, PinValueChangedEventArgs e)
                    {
                        fallingEventOccuredCount++;
                    }

                    void callback2(object sender, PinValueChangedEventArgs e)
                    {
                        fallingEventOccuredCount++;
                    }

                    void callback3(object sender, PinValueChangedEventArgs e)
                    {
                        fallingEventOccuredCount++;
                    }

                    void callback4(object sender, PinValueChangedEventArgs e)
                    {
                        risingEventOccuredCount++;
                    }
                }
            });
        }

        [Fact]
        [Trait("SkipOnTestRun", "Windows_NT")] // The windows driver is returning none as the event type.
        public void WaitForEventCancelAfter10MilliSecondsTest()
        {
            using (GpioController controller = new GpioController(GetTestNumberingScheme(), GetTestDriver()))
            {
                CancellationTokenSource tokenSource = new CancellationTokenSource(WaitMilliSeconds);
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
                    Thread.Sleep(WaitMilliSeconds);
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
                    Thread.Sleep(WaitMilliSeconds);
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
