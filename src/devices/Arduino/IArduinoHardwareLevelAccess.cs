using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Text;

#pragma warning disable CS1591
namespace Iot.Device.Arduino
{
    public interface IArduinoHardwareLevelAccess
    {
        [ArduinoImplementation(ArduinoImplementation.GetTickCount)]
        Int32 GetTickCount();

        [ArduinoImplementation(ArduinoImplementation.SetPinMode)]
        void SetPinMode(int pin, PinMode mode);

        [ArduinoImplementation(ArduinoImplementation.WritePin)]
        void WritePin(int pin, int value);

        [ArduinoImplementation(ArduinoImplementation.ReadPin)]
        int ReadPin(int pin);

        [ArduinoImplementation(ArduinoImplementation.SleepMicroseconds)]
        void SleepMicroseconds(int microseconds);

        [ArduinoImplementation(ArduinoImplementation.GetMicroseconds)]
        UInt32 GetMicroseconds();

        [ArduinoImplementation(ArduinoImplementation.Debug)]
        void DebugValue(int data);

        [ArduinoImplementation(ArduinoImplementation.GetPinMode)]
        PinMode GetPinMode(int pinNumber);

        [ArduinoImplementation(ArduinoImplementation.IsPinModeSupported)]
        bool IsPinModeSupported(int pinNumber, PinMode mode);

        [ArduinoImplementation(ArduinoImplementation.GetPinCount)]
        int GetPinCount();
    }
}
