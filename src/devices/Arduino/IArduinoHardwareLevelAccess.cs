using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Text;

#pragma warning disable CS1591
namespace Iot.Device.Arduino
{
    public interface IArduinoHardwareLevelAccess
    {
        [ArduinoImplementation(NativeMethod.HardwareLevelAccessSetPinMode)]
        void SetPinMode(int pin, PinMode mode);

        [ArduinoImplementation(NativeMethod.HardwareLevelAccessWritePin)]
        void WritePin(int pin, int value);

        [ArduinoImplementation(NativeMethod.HardwareLevelAccessReadPin)]
        int ReadPin(int pin);

        [ArduinoImplementation(NativeMethod.HardwareLevelAccessGetPinMode)]
        PinMode GetPinMode(int pinNumber);

        [ArduinoImplementation(NativeMethod.HardwareLevelAccessIsPinModeSupported)]
        bool IsPinModeSupported(int pinNumber, PinMode mode);

        [ArduinoImplementation(NativeMethod.HardwareLevelAccessGetPinCount)]
        int GetPinCount();
    }
}
