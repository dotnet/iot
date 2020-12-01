using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Text;
using System.Threading;

#pragma warning disable CS1591
namespace Iot.Device.Arduino
{
    /// <summary>
    /// This represents the driver that runs physically on the Arduino when
    /// an instance of a GpioController is requested there.
    /// </summary>
    public class ArduinoNativeGpioDriver : GpioDriver
    {
        // This reference stays a null pointer forever. The implementation ignores it.
        private readonly IArduinoHardwareLevelAccess _hardwareLevelAccess;

        public ArduinoNativeGpioDriver()
        {
            _hardwareLevelAccess = null!;
        }

        protected override int PinCount
        {
            get
            {
                return _hardwareLevelAccess.GetPinCount();
            }
        }

        protected override int ConvertPinNumberToLogicalNumberingScheme(int pinNumber)
        {
            return pinNumber;
        }

        protected override void OpenPin(int pinNumber)
        {
        }

        protected override void ClosePin(int pinNumber)
        {
            _hardwareLevelAccess?.SetPinMode(pinNumber, PinMode.Input);
        }

        protected override void SetPinMode(int pinNumber, PinMode mode)
        {
            _hardwareLevelAccess?.SetPinMode(pinNumber, mode);
        }

        protected override PinMode GetPinMode(int pinNumber)
        {
            return _hardwareLevelAccess.GetPinMode(pinNumber);
        }

        protected override bool IsPinModeSupported(int pinNumber, PinMode mode)
        {
            return _hardwareLevelAccess.IsPinModeSupported(pinNumber, mode);
        }

        protected override PinValue Read(int pinNumber)
        {
            // We should not be throwing here, as this results in unnecessary dependencies
            return _hardwareLevelAccess?.ReadPin(pinNumber) ?? PinValue.Low;
        }

        protected override void Write(int pinNumber, PinValue value)
        {
            _hardwareLevelAccess?.WritePin(pinNumber, value == PinValue.High ? 1 : 0);
        }

        protected override WaitForEventResult WaitForEvent(int pinNumber, PinEventTypes eventTypes, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        protected override void AddCallbackForPinValueChangedEvent(int pinNumber, PinEventTypes eventTypes, PinChangeEventHandler callback)
        {
            throw new NotImplementedException();
        }

        protected override void RemoveCallbackForPinValueChangedEvent(int pinNumber, PinChangeEventHandler callback)
        {
            throw new NotImplementedException();
        }
    }
}
