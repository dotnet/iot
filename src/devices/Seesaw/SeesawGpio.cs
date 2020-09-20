// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Buffers.Binary;
using System.Device.Gpio;

namespace Iot.Device.Seesaw
{
    public partial class Seesaw : IDisposable
    {
        /// <summary>
        /// Set the PinMode for a GPIO Pin.
        /// </summary>
        /// <param name="pin">The pin that has its mode set.</param>
        /// <param name="mode">The pin mode to be set.</param>
        public void SetGpioPinMode(byte pin, PinMode mode)
        {
            if (pin < 0 || pin > 63)
            {
                throw new ArgumentOutOfRangeException("Gpio pin must be within 0-63 range.");
            }

            SetGpioPinModeBulk((ulong)(1 << pin), mode);
        }

        /// <summary>
        /// Set the PinMode for a number of GPIO pins
        /// </summary>
        /// <param name="pins">A 64bit integer containing 1 bit for each pin. If a bit is set to 1 then the pin mode is set for the associated pin.</param>
        /// <param name="mode">The pin mode to be set.</param>
        public void SetGpioPinModeBulk(ulong pins, PinMode mode)
        {
            byte[] pinArray;

            if (!HasModule(SeesawModule.Gpio))
            {
                throw new InvalidOperationException($"The hardware on I2C Bus {I2cDevice.ConnectionSettings.BusId}, Address 0x{I2cDevice.ConnectionSettings.DeviceAddress:X2} does not support Adafruit SeeSaw GPIO functionality");
            }

            pinArray = PinsToPinArray(pins);

            switch (mode)
            {
                case PinMode.Output:
                    Write(SeesawModule.Gpio, SeesawFunction.GpioDirsetBulk, pinArray);
                    break;

                case PinMode.Input:
                    Write(SeesawModule.Gpio, SeesawFunction.GpioDirclrBulk, pinArray);
                    break;

                case PinMode.InputPullUp:
                    Write(SeesawModule.Gpio, SeesawFunction.GpioDirclrBulk, pinArray);
                    Write(SeesawModule.Gpio, SeesawFunction.GpioPullenset, pinArray);
                    Write(SeesawModule.Gpio, SeesawFunction.GpioBulkSet, pinArray);
                    break;

                case PinMode.InputPullDown:
                    Write(SeesawModule.Gpio, SeesawFunction.GpioDirclrBulk, pinArray);
                    Write(SeesawModule.Gpio, SeesawFunction.GpioPullenset, pinArray);
                    Write(SeesawModule.Gpio, SeesawFunction.GpioBulkClr, pinArray);
                    break;
            }
        }

        /// <summary>
        /// Write a value to GPIO pin
        /// </summary>
        /// <param name="pin">The pin that has its value set.</param>
        /// <param name="value">The pin value to be set.</param>
        public void WriteGpioDigital(byte pin, bool value)
        {
            if (pin > 63)
            {
                throw new ArgumentOutOfRangeException("Gpio pin must be within 0-63 range.");
            }

            WriteGpioDigitalBulk((ulong)(1 << pin), value);
        }

        /// <summary>
        /// Write a value to a number of GPIO pins
        /// </summary>
        /// <param name="pins">A 64bit integer containing 1 bit for each pin. If a bit is set to 1 then the pin value is set for the associated pin.</param>
        /// <param name="value">The pin value to be set.</param>
        public void WriteGpioDigitalBulk(ulong pins, bool value)
        {
            if (!HasModule(SeesawModule.Gpio))
            {
                throw new InvalidOperationException($"The hardware on I2C Bus {I2cDevice.ConnectionSettings.BusId}, Address 0x{I2cDevice.ConnectionSettings.DeviceAddress:X2} does not support Adafruit SeeSaw GPIO functionality");
            }

            Write(SeesawModule.Gpio, value ? SeesawFunction.GpioBulkSet : SeesawFunction.GpioBulkClr, PinsToPinArray(pins));
        }

        /// <summary>
        /// Read a value from a GPIO pin.
        /// </summary>
        /// <param name="pin">The pin that has its value read.</param>
        /// <returns>A boolean value representaing the status of the GPIO pin.</returns>
        public bool ReadGpioDigital(byte pin)
        {
            if (pin > 63)
            {
                throw new ArgumentOutOfRangeException("Gpio pin must be within 0-63 range.");
            }

            return ((ReadGpioDigitalBulk((ulong)(1 << pin))) != 0);
        }

        /// <summary>
        /// Read a value from a number of GPIO pins.
        /// </summary>
        /// <param name="pins">A 64bit integer containing 1 bit for each pin. If a bit is set to 1 then the pin value is read for the associated pin.</param>
        /// <returns>A 64bit integer containing 1 bit for each pin status requested.</returns>
        public ulong ReadGpioDigitalBulk(ulong pins)
        {
            if (!HasModule(SeesawModule.Gpio))
            {
                throw new InvalidOperationException($"The hardware on I2C Bus {I2cDevice.ConnectionSettings.BusId}, Address 0x{I2cDevice.ConnectionSettings.DeviceAddress:X2} does not support Adafruit SeeSaw GPIO functionality");
            }

            return PinArrayToPins(Read(SeesawModule.Gpio, SeesawFunction.GpioBulk, 8)) & pins;
        }

        /// <summary>
        /// Enable or disable interrupts for a GPIO pin.
        /// </summary>
        /// <param name="pins">A 32bit integer containing 1 bit for each pin. If a bit is set to 1 then the interrupt is enabled for the associated pin..</param>
        /// <param name="enable">A boolean value that indicates that interrupts are enabled when true or disabled when false.</param>
        public void SetGpioInterrupts(uint pins, bool enable)
        {
            if (!HasModule(SeesawModule.Gpio))
            {
                throw new InvalidOperationException($"The hardware on I2C Bus {I2cDevice.ConnectionSettings.BusId}, Address 0x{I2cDevice.ConnectionSettings.DeviceAddress:X2} does not support Adafruit SeeSaw GPIO functionality");
            }

            Span<byte> buffer = stackalloc byte[4];

            BinaryPrimitives.WriteUInt32BigEndian(buffer, pins);

            Write(SeesawModule.Gpio, enable ? SeesawFunction.GpioIntenset : SeesawFunction.GpioIntenclr, buffer);
        }

        /// <summary>
        /// Read all the Gpio interrupt flags. Clears any flags when read.
        /// </summary>
        /// <returns>
        /// A 32bit integer containing 1 bit for each for the first 32 bins. If a bit is set to 1 then the interrupt is signaled for the associated pin..
        /// </returns>
        public uint ReadGpioInterruptFlags()
        {
            if (!HasModule(SeesawModule.Gpio))
            {
                throw new InvalidOperationException($"The hardware on I2C Bus {I2cDevice.ConnectionSettings.BusId}, Address 0x{I2cDevice.ConnectionSettings.DeviceAddress:X2} does not support Adafruit SeeSaw GPIO functionality");
            }

            return BinaryPrimitives.ReadUInt32BigEndian(Read(SeesawModule.Gpio, SeesawFunction.GpioIntflag, 4));
        }

        /// <summary>
        /// Takes an array of bytes read from the Seesaw device and converts to a 64bit value where each bit represents a pin
        /// </summary>
        /// <remarks>
        /// Pin         22222233 11112222 00111111 00000000 55556666 44555555 44444444 33333333
        ///             45678901 67890123 89012345 01234567 67890123 89012345 01234567 23456789
        ///
        /// Byte Array
        /// Byte Index  00000000 11111111 22222222 33333333 44444444 55555555 66666666 77777777
        /// Bit         01234567 01234567 01234567 01234567 01234567 01234567 01234567 01234567
        /// </remarks>
        /// <param name="pinArray">A byte array read from a Seesaw device</param>
        /// <returns>A ulong representing the 64 Gpio pins</returns>
        private ulong PinArrayToPins(byte[] pinArray) => ((ulong)pinArray[4] << 56) | ((ulong)pinArray[5] << 48) | ((ulong)pinArray[6] << 40) | ((ulong)pinArray[7] << 32) | ((ulong)pinArray[0] << 24) | ((ulong)pinArray[1] << 16) | ((ulong)pinArray[2] << 8) | pinArray[3];

        /// <summary>
        /// Taks a 64 bit value where each bit represents a pin and converts it to a byte array suitable for writing to a seesaw device
        /// </summary>
        /// <remarks>
        /// Pin         22222233 11112222 00111111 00000000 55556666 44555555 44444444 33333333
        ///             45678901 67890123 89012345 01234567 67890123 89012345 01234567 23456789
        ///
        /// Byte Array
        /// Byte Index  00000000 11111111 22222222 33333333 44444444 55555555 66666666 77777777
        /// Bit         01234567 01234567 01234567 01234567 01234567 01234567 01234567 01234567
        /// </remarks>
        /// <param name="pins">A ulong representing the 64 Gpio pins</param>
        /// <returns>A byte array to write to a Seesaw device</returns>
        private byte[] PinsToPinArray(ulong pins) => new byte[] { (byte)(pins >> 24), (byte)(pins >> 16), (byte)(pins >> 8), (byte)pins, (byte)(pins >> 56), (byte)(pins >> 48), (byte)(pins >> 40), (byte)(pins >> 32) };
    }
}
