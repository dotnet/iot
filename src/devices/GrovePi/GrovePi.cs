// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Buffers.Binary;
using System.Device.Gpio;
using System.Device.I2c;
using System.IO;
using System.Threading;
using Iot.Device.GrovePiDevice.Models;

namespace Iot.Device.GrovePiDevice
{
    /// <summary>
    /// Create a GrovePi class
    /// </summary>
    public class GrovePi : IDisposable
    {
        private const byte MaxRetries = 4;
        private readonly bool _shouldDispose;
        private I2cDevice _i2cDevice;

        /// <summary>
        /// The default GrovePi I2C address is 0x04
        /// Other addresses can be use, see GrovePi documentation
        /// </summary>
        public const byte DefaultI2cAddress = 0x04;

        /// <summary>
        /// The maximum ADC, 10 bit so 1023 on GrovePi
        /// </summary>
        public int MaxAdc => 1023;

        /// <summary>
        /// Contains the GrovePi key information
        /// </summary>
        public Info GrovePiInfo { get; internal set; }

        /// <summary>
        /// GrovePi constructor
        /// </summary>
        /// <param name="i2cDevice">The I2C device. Device address is 0x04</param>
        /// <param name="shouldDispose">True to dispose the I2C device when disposing GrovePi</param>
        public GrovePi(I2cDevice i2cDevice, bool shouldDispose = true)
        {
            _i2cDevice = i2cDevice ?? throw new ArgumentNullException(nameof(i2cDevice));
            _shouldDispose = shouldDispose;
            GrovePiInfo = new Info(GetFirmwareVerion());
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (_shouldDispose)
            {
                _i2cDevice?.Dispose();
                _i2cDevice = null!;
            }
        }

        /// <summary>
        /// Get the firmware version
        /// </summary>
        /// <returns>GroovePi firmware version</returns>
        public Version GetFirmwareVerion()
        {
            WriteCommand(GrovePiCommand.Version, 0, 0, 0);
#pragma warning disable SA1011
            byte[]? inArray = ReadCommand(GrovePiCommand.Version, 0);
            if (inArray is object)
            {
                return new Version(inArray[1], inArray[2], inArray[3]);
            }

            throw new Exception("Unknown firmware version");
        }

        /// <summary>
        /// Write a GrovePi command
        /// </summary>
        /// <param name="command">The GrovePi command</param>
        /// <param name="pin">The pin to write the command</param>
        /// <param name="param1">First parameter</param>
        /// <param name="param2">Second parameter</param>
        public void WriteCommand(GrovePiCommand command, GrovePort pin, byte param1, byte param2)
        {
            Span<byte> outArray = stackalloc byte[4]
            {
                (byte)command, (byte)(pin), param1, param2
            };
            byte tries = 0;
            IOException? innerEx = null;
            // When writing/reading to the I2C port, GrovePi doesn't respond on time in some cases
            // So we wait a little bit before retrying
            // In most cases, the I2C read/write can go thru without waiting
            while (tries < MaxRetries)
            {
                try
                {
                    _i2cDevice.Write(outArray);
                    return;
                }
                catch (IOException ex)
                 {
                    innerEx = ex;
                    tries++;
                    Thread.Sleep(10);
                }
            }

            throw new IOException($"{nameof(WriteCommand)}: Failed to write command {command}", innerEx);
        }

        /// <summary>
        /// Read data from GrovePi
        /// </summary>
        /// <param name="command">The GrovePi command</param>
        /// <param name="pin">The pin to read</param>
        /// <returns></returns>
        public byte[]? ReadCommand(GrovePiCommand command, GrovePort pin)
        {
            int numberBytesToRead = command switch
            {
                GrovePiCommand.DigitalRead => 1,
                GrovePiCommand.AnalogRead | GrovePiCommand.UltrasonicRead | GrovePiCommand.LetBarGet => 3,
                GrovePiCommand.Version => 4,
                GrovePiCommand.DhtTemp => 9,
                _ => 0,
            };

            if (numberBytesToRead == 0)
            {
                return null;
            }

            byte[] outArray = new byte[numberBytesToRead];
            byte tries = 0;
            IOException? innerEx = null;
            // When writing/reading the I2C port, GrovePi doesn't respond on time in some cases
            // So we wait a little bit before retrying
            // In most cases, the I2C read/write can go thru without waiting
            while (tries < MaxRetries)
            {
                try
                {
                    _i2cDevice.Read(outArray);
                    return outArray;
                }
                catch (IOException ex)
                {
                    // Give it another try
                    innerEx = ex;
                    tries++;
                    Thread.Sleep(10);
                }
            }

            throw new IOException($"{nameof(ReadCommand)}: Failed to write command {command}", innerEx);
        }

        /// <summary>
        /// Read a digital pin, equivalent of digitalRead on Arduino
        /// </summary>
        /// <param name="pin">The GroovePi pin to read</param>
        /// <returns>Returns the level either High or Low</returns>
        public PinValue DigitalRead(GrovePort pin)
        {
            WriteCommand(GrovePiCommand.DigitalRead, pin, 0, 0);
            byte tries = 0;
            IOException? innerEx = null;
            // When writing/reading to the I2C port, GrovePi doesn't respond on time in some cases
            // So we wait a little bit before retrying
            // In most cases, the I2C read/write can go thru without waiting
            while (tries < MaxRetries)
            {
                try
                {
                    return (PinValue)_i2cDevice.ReadByte();
                }
                catch (IOException ex)
                {
                    // Give it another try
                    innerEx = ex;
                    tries++;
                    Thread.Sleep(10);
                }
            }

            throw new IOException($"{nameof(DigitalRead)}: Failed to read byte with command {GrovePiCommand.DigitalRead}", innerEx);
        }

        /// <summary>
        /// Write a digital pin, equivalent of digitalWrite on Arduino
        /// </summary>
        /// <param name="pin">The GroovePi pin to read</param>
        /// <param name="pinLevel">High to put the pin high, Low to put the pin low</param>
        public void DigitalWrite(GrovePort pin, PinValue pinLevel) => WriteCommand(GrovePiCommand.DigitalWrite, pin, (byte)pinLevel, 0);

        /// <summary>
        /// Setup the pin mode, equivalent of pinMod on Arduino
        /// </summary>
        /// <param name="pin">The GroovePi pin to setup</param>
        /// <param name="mode">THe mode to setup Intput or Output</param>
        public void PinMode(GrovePort pin, PinMode mode) => WriteCommand(GrovePiCommand.PinMode, pin, (byte)mode, 0);

        /// <summary>
        /// Read an analog value on a pin, equivalent of analogRead on Arduino
        /// </summary>
        /// <param name="pin">The GroovePi pin to read</param>
        /// <returns></returns>
        public int AnalogRead(GrovePort pin)
        {
            WriteCommand(GrovePiCommand.AnalogRead, pin, 0, 0);
            try
            {
                var inArray = ReadCommand(GrovePiCommand.AnalogRead, pin);
                return BinaryPrimitives.ReadInt16BigEndian(inArray.AsSpan(1, 2));
            }
            catch (IOException)
            {
                return -1;
            }
        }

        /// <summary>
        /// Write an analog pin (PWM pin), equivalent of analogWrite on Arduino
        /// </summary>
        /// <param name="pin">The GroovePi pin to write</param>
        /// <param name="value">The value to write between 0 and 255</param>
        public void AnalogWrite(GrovePort pin, byte value) => WriteCommand(GrovePiCommand.AnalogWrite, pin, value, 0);
    }
}
