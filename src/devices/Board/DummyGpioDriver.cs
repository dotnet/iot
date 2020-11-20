using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Text;
using System.Threading;

#pragma warning disable CS1591
namespace Iot.Device.Board
{
    /// <summary>
    /// A GPIO driver that has zero pins. Use to fulfill the interface.
    /// </summary>
    public class DummyGpioDriver : GpioDriver
    {
        protected override int PinCount => 0;
        protected override int ConvertPinNumberToLogicalNumberingScheme(int pinNumber)
        {
            return 0;
        }

        protected override void OpenPin(int pinNumber)
        {
            throw new NotSupportedException("No such pin");
        }

        protected override void ClosePin(int pinNumber)
        {
            throw new NotSupportedException("No such pin");
        }

        protected override void SetPinMode(int pinNumber, PinMode mode)
        {
            throw new NotSupportedException("No such pin");
        }

        protected override PinMode GetPinMode(int pinNumber)
        {
            throw new NotSupportedException("No such pin");
        }

        protected override bool IsPinModeSupported(int pinNumber, PinMode mode)
        {
            throw new NotSupportedException("No such pin");
        }

        protected override PinValue Read(int pinNumber)
        {
            throw new NotSupportedException("No such pin");
        }

        protected override void Write(int pinNumber, PinValue value)
        {
            throw new NotSupportedException("No such pin");
        }

        protected override WaitForEventResult WaitForEvent(int pinNumber, PinEventTypes eventTypes, CancellationToken cancellationToken)
        {
            throw new NotSupportedException("No such pin");
        }

        protected override void AddCallbackForPinValueChangedEvent(int pinNumber, PinEventTypes eventTypes, PinChangeEventHandler callback)
        {
            throw new NotSupportedException("No such pin");
        }

        protected override void RemoveCallbackForPinValueChangedEvent(int pinNumber, PinChangeEventHandler callback)
        {
            throw new NotSupportedException("No such pin");
        }
    }
}
