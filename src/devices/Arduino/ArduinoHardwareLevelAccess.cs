using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Text;

#pragma warning disable CS1591
namespace Iot.Device.Arduino
{
    public class ArduinoHardwareLevelAccess
    {
        [ArduinoImplementation(NativeMethod.HardwareLevelAccessSetPinMode)]
        public void SetPinMode(int pin, PinMode mode)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation(NativeMethod.HardwareLevelAccessWritePin)]
        public void WritePin(int pin, int value)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation(NativeMethod.HardwareLevelAccessReadPin)]
        public int ReadPin(int pin)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation(NativeMethod.HardwareLevelAccessGetPinMode)]
        public PinMode GetPinMode(int pinNumber)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation(NativeMethod.HardwareLevelAccessIsPinModeSupported)]
        public bool IsPinModeSupported(int pinNumber, PinMode mode)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation(NativeMethod.HardwareLevelAccessGetPinCount)]
        public int GetPinCount()
        {
            throw new NotImplementedException();
        }
    }
}
