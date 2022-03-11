// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Text;

namespace ArduinoCsCompiler
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
        [ArduinoImplementation("HardwareLevelAccessSetPinMode", 0x100)]
        public void SetPinMode(int pin, PinMode mode)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Write a value to a pin
        /// </summary>
        [ArduinoImplementation("HardwareLevelAccessWritePin", 0x101)]
        public void WritePin(int pin, int value)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Read the value of a pin
        /// </summary>
        [ArduinoImplementation("HardwareLevelAccessReadPin", 0x102)]
        public int ReadPin(int pin)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Get the mode of a pin
        /// </summary>
        [ArduinoImplementation("HardwareLevelAccessGetPinMode", 0x103)]
        public PinMode GetPinMode(int pinNumber)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Pin mode support
        /// </summary>
        [ArduinoImplementation("HardwareLevelAccessIsPinModeSupported", 0x104)]
        public bool IsPinModeSupported(int pinNumber, PinMode mode)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Get number of pins
        /// </summary>
        [ArduinoImplementation("HardwareLevelAccessGetPinCount", 0x105)]
        public int GetPinCount()
        {
            throw new NotImplementedException();
        }
    }
}
