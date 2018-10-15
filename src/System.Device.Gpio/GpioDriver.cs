// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Device.Gpio;

namespace System.Device.Gpio
{
    public abstract class GpioDriver : IDisposable
    {
        public event EventHandler<PinValueChangedEventArgs> ValueChanged;

        protected internal abstract int PinCount { get; }

        protected internal abstract int ConvertPinNumber(int pinNumber, PinNumberingScheme from, PinNumberingScheme to);

        protected internal abstract bool IsPinModeSupported(PinMode mode);

        protected internal abstract void OpenPin(int gpioPinNumber);

        protected internal abstract void OpenPWMPin(int chip, int channel);

        protected internal abstract void ClosePin(int gpioPinNumber);

        protected internal abstract void ClosePWMPin(int chip, int channel);

        protected internal abstract void SetPinMode(int gpioPinNumber, PinMode mode);

        protected internal abstract PinMode GetPinMode(int gpioPinNumber);

        protected internal abstract void Output(int gpioPinNumber, PinValue value);

        protected internal abstract void PWMWrite(int chip, int channel, PWMMode mode, int period, int dutyCycle);

        protected internal abstract PinValue Input(int gpioPinNumber);

        protected internal abstract void SetDebounce(int gpioPinNumber, TimeSpan timeout);

        protected internal abstract TimeSpan GetDebounce(int gpioPinNumber);

        protected internal abstract void SetPinEventsToDetect(int gpioPinNumber, PinEvent events);

        protected internal abstract PinEvent GetPinEventsToDetect(int gpioPinNumber);

        protected internal abstract void SetEnableRaisingPinEvents(int gpioPinNumber, bool enable);

        protected internal abstract bool GetEnableRaisingPinEvents(int gpioPinNumber);

        protected internal abstract bool WaitForPinEvent(int gpioPinNumber, TimeSpan timeout);

        protected internal void OnPinValueChanged(int gpioPinNumber)
        {
            var e = new PinValueChangedEventArgs(gpioPinNumber);
            ValueChanged?.Invoke(this, e);
        }

        public abstract void Dispose();
    }
}
