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
            ManualResetEvent mre = new ManualResetEvent(false);
            bool wasCalled = false;
            using (GpioController controller = new GpioController(GetTestNumberingScheme(), GetTestDriver()))
            {
                controller.OpenPin(InputPin, PinMode.Input);
                controller.OpenPin(OutputPin, PinMode.Output);
                controller.RegisterCallbackForPinValueChangedEvent(InputPin, PinEventTypes.Rising, callback);
                controller.Write(OutputPin, PinValue.High);
                mre.WaitOne(TimeSpan.FromSeconds(5));
                Assert.True(wasCalled);
            }

            void callback(object sender, PinValueChangedEventArgs pinValueChangedEventArgs)
            {
                wasCalled = true;
                mre.Set();
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
                    Thread.Sleep(10);
                    controller.Write(OutputPin, PinValue.Low);
                    Thread.Sleep(10);
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
        public void WaitForEventTest()
        {
            using (GpioController controller = new GpioController(GetTestNumberingScheme(), GetTestDriver()))
            {
                bool cancelled = false;
                CancellationTokenSource tokenSource = new CancellationTokenSource();
                controller.OpenPin(InputPin, PinMode.Input);
                controller.OpenPin(LedPin, PinMode.Input);
                controller.OpenPin(OutputPin, PinMode.Output);
                controller.Write(OutputPin, PinValue.Low);

                controller.RegisterCallbackForPinValueChangedEvent(InputPin, PinEventTypes.Rising, (o, e) =>
                {
                        tokenSource.Cancel();
                        cancelled = true;
                });

                Task.Run(() =>
                {
                    controller.Write(OutputPin, PinValue.High);
                    Thread.Sleep(10);
                    controller.Write(OutputPin, PinValue.Low);
                });

                WaitForEventResult result = controller.WaitForEvent(LedPin, PinEventTypes.Rising, tokenSource.Token);
                Assert.True(cancelled);
                Assert.True(result.TimedOut);
            }
        }
        protected abstract GpioDriver GetTestDriver();
        protected abstract PinNumberingScheme GetTestNumberingScheme();
    }
}
