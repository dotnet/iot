using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Text;
using System.Threading;

#pragma warning disable CS1591
namespace Iot.Device.Arduino
{
    /// <summary>
    /// This dummy implementation represents the driver that runs physically on the Arduino when
    /// an instance of a GpioController is requested there
    /// </summary>
    public class ArduinoNativeGpioDriver : GpioDriver
    {
        private readonly IArduinoHardwareLevelAccess _hardwareLevelAccess;
        private int _pinCount;

        public ArduinoNativeGpioDriver(IArduinoHardwareLevelAccess hardwareLevelAccess)
        {
            _hardwareLevelAccess = hardwareLevelAccess;
            _pinCount = 15; // TODO...
        }

        protected override int PinCount
        {
            get { return _pinCount; }
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
            _hardwareLevelAccess.SetPinMode(pinNumber, PinMode.Input);
        }

        protected override void SetPinMode(int pinNumber, PinMode mode)
        {
            _hardwareLevelAccess.SetPinMode(pinNumber, mode);
        }

        protected override PinMode GetPinMode(int pinNumber)
        {
            throw new NotImplementedException();
        }

        protected override bool IsPinModeSupported(int pinNumber, PinMode mode)
        {
            throw new NotImplementedException();
        }

        protected override PinValue Read(int pinNumber)
        {
            return _hardwareLevelAccess.ReadPin(pinNumber);
        }

        protected override void Write(int pinNumber, PinValue value)
        {
            _hardwareLevelAccess.WritePin(pinNumber, value == PinValue.High ? 1 : 0);
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
