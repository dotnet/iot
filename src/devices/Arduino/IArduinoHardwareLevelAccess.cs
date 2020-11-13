using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Text;

#pragma warning disable CS1591
namespace Iot.Device.Arduino
{
    public interface IArduinoHardwareLevelAccess
    {
        [ArduinoImplementation(4)]
        Int32 GetTickCount();

        [ArduinoImplementation(1)]
        void SetPinMode(int pin, PinMode mode);

        [ArduinoImplementation(2)]
        void WritePin(int pin, int value);

        [ArduinoImplementation(3)]
        int ReadPin(int pin);

        [ArduinoImplementation(5)]
        void SleepMicroseconds(int microseconds);

        [ArduinoImplementation(6)]
        UInt32 GetMicroseconds();

        [ArduinoImplementation(7)]
        void DebugValue(int data);
    }
}
