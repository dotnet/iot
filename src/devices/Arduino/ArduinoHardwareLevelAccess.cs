using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Text;

namespace Iot.Device.Arduino
{
    /// <summary>
    /// Internal use only: Abstraction level for Arduino support.
    /// Do not instantiate externally.
    /// </summary>
    /// <remarks>
    /// Currently public because needs to be known by the compiler
    /// </remarks>
    public class ArduinoHardwareLevelAccess
    {
        /// <inheritdoc cref="GpioDriver.SetPinMode(int, PinMode)"/>
        [ArduinoImplementation("HardwareLevelAccessSetPinMode")]
        public void SetPinMode(int pin, PinMode mode)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Write a value to a pin
        /// </summary>
        [ArduinoImplementation("HardwareLevelAccessWritePin")]
        public void WritePin(int pin, int value)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Read the value of a pin
        /// </summary>
        [ArduinoImplementation("HardwareLevelAccessReadPin")]
        public int ReadPin(int pin)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Get the mode of a pin
        /// </summary>
        [ArduinoImplementation("HardwareLevelAccessGetPinMode")]
        public PinMode GetPinMode(int pinNumber)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Pin mode support
        /// </summary>
        [ArduinoImplementation("HardwareLevelAccessIsPinModeSupported")]
        public bool IsPinModeSupported(int pinNumber, PinMode mode)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Get number of pins
        /// </summary>
        [ArduinoImplementation("HardwareLevelAccessGetPinCount")]
        public int GetPinCount()
        {
            throw new NotImplementedException();
        }
    }
}
