using Iot.Device.GrovePiDevice.Models;
using System;
using System.Buffers.Binary;
using System.Device.I2c;
using System.IO;
using System.Threading;

namespace Iot.Device.GrovePiDevice
{
    /// <summary>
    /// Create a GrovePi class
    /// </summary>
    public class GrovePi : IDisposable
    {        
        private I2cDevice _i2cDevice;
        private readonly bool _autoDispose;

        /// <summary>
        /// The default GrovePi I2C address is 0x04
        /// Other addresses can be use, see GrovePi documentation
        /// </summary>
        public static byte GrovePiSefaultI2cAddress => 0x04;

        /// <summary>
        /// The maximum ADC, 12 bit so 2013 on GrovePi
        /// </summary>
        public int MaxAdc => 1023;
        /// <summary>
        /// Contains the GrovePi key information
        /// </summary>
        public GrovePiInfo GrovePiInfo { get; internal set; }

        /// <summary>
        /// GrovePi constructor
        /// </summary>
        /// <param name="i2cDevice">The I2C device. Device address is 0x04</param>
        /// <param name="autoDispose">True to dispose the I2C device when disposing GrovePi</param>
        public GrovePi(I2cDevice i2cDevice, bool autoDispose = true)
        {
            _i2cDevice = i2cDevice ?? throw new ArgumentException("I2C device can't be null");
            _autoDispose = autoDispose;
            GrovePiInfo = new GrovePiInfo() { SoftwareVersion = GetFirmwareVerion() };
        }

        public void Dispose()
        {
            if(_autoDispose)
            {
                _i2cDevice.Dispose();
            }
        }

        /// <summary>
        /// Get the firmware version
        /// </summary>
        /// <returns>GroovePi firmware version</returns>
        public Version GetFirmwareVerion()
        {
            WriteCommand(GrovePiCommands.Version, 0, 0, 0);
            Thread.Sleep(10);
            var inArray = ReadCommand(GrovePiCommands.Version, 0);
            return new Version(inArray[1], inArray[2], inArray[2]);
        }

        /// <summary>
        /// Write a GrovePi command
        /// </summary>
        /// <param name="commands">The GrovePi command</param>
        /// <param name="pin">The pin to write the command</param>
        /// <param name="param1">First parameter</param>
        /// <param name="param2">Second parameter</param>
        public void WriteCommand(GrovePiCommands commands, GrovePort pin, byte param1, byte param2)
        {
            byte[] outArray = new byte[4] { (byte)commands, (byte)(pin), param1, param2 };
            try
            {
                _i2cDevice.Write(outArray.AsSpan());
            }
            catch (Exception ex) when (ex is IOException)
            {
                // Give it a second try
                _i2cDevice.Write(outArray.AsSpan());
            }
        }

        /// <summary>
        /// Read data from GrovePi
        /// </summary>
        /// <param name="commands">The GrovePi command</param>
        /// <param name="pin">The pin to read</param>
        /// <returns></returns>
        public byte[] ReadCommand(GrovePiCommands commands, GrovePort pin)
        {
            int numberBytesToRead = 0;
            switch (commands)
            {
                case GrovePiCommands.DigitalRead:
                    numberBytesToRead = 1;
                    break;
                case GrovePiCommands.AnalogRead:
                case GrovePiCommands.UltrasonicRead:
                case GrovePiCommands.LetBarGet:
                    numberBytesToRead = 3;
                    break;
                case GrovePiCommands.Version:
                    numberBytesToRead = 4;
                    break;
                case GrovePiCommands.DhtTemp:
                    numberBytesToRead = 9;
                    break;
                // No other commands are for read
                default:
                    return null;
            }
            Span<byte> inArray = new Span<byte>(new byte[numberBytesToRead]);
            _i2cDevice.Read(inArray);
            return inArray.ToArray();
        }

        /// <summary>
        /// Read a digital pin, equivalent of digitalRead on Arduino
        /// </summary>
        /// <param name="pin">The GroovePi pin to read</param>
        /// <returns>Returns the level either High or Low</returns>
        public PinLevel DigitalRead(GrovePort pin)
        {
            WriteCommand(GrovePiCommands.DigitalRead, pin, 0, 0);
            Thread.Sleep(10);
            try
            {
                return (PinLevel)_i2cDevice.ReadByte();
            }
            catch (Exception ex) when (ex is IOException)
            {
                // Give it a second try
                return (PinLevel)_i2cDevice.ReadByte();
            }
        }

        /// <summary>
        /// Write a digital pin, equivalent of digitalWrite on Arduino
        /// </summary>
        /// <param name="pin">The GroovePi pin to read</param>
        /// <param name="pinLevel">High to put the pin high, Low to put the pin low</param>
        public void DigitalWrite(GrovePort pin, PinLevel pinLevel)
        {
            WriteCommand(GrovePiCommands.DigitalWrite, pin, (byte)pinLevel, 0);
        }

        /// <summary>
        /// Setup the pin mode, equivalent of pinMod on Arduino
        /// </summary>
        /// <param name="pin">The GroovePi pin to setup</param>
        /// <param name="mode">THe mode to setup Intput or Output</param>
        public void PinMode(GrovePort pin, PinMode mode)
        {
            WriteCommand(GrovePiCommands.PinMode, pin, (byte)mode, 0);
        }

        /// <summary>
        /// Read an analog value on a pin, equivalent of analogRead on Arduino
        /// </summary>
        /// <param name="pin">The GroovePi pin to read</param>
        /// <returns></returns>
        public int AnalogRead(GrovePort pin)
        {
            WriteCommand(GrovePiCommands.AnalogRead, pin, 0, 0);
            Thread.Sleep(10);
            try
            {
                var inArray = ReadCommand(GrovePiCommands.AnalogRead, pin);
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
        public void AnalogWrite(GrovePort pin, byte value)
        {
            WriteCommand(GrovePiCommands.AnalogWrite, pin, value, 0);
        }
    }
}
