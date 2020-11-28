// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Buffers.Binary;
using System.Device.Spi;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Iot.Device.GoPiGo3.Models;

namespace Iot.Device.GoPiGo3
{
    /// <summary>
    /// A GoPiGo3 class for a GoPiGo3 Dexter Industries robot kit
    /// </summary>
    public class GoPiGo : IDisposable
    {
        private const int DefaultMotorGearRatio = 120;
        private const int DefaultEncoderTicksPerRotation = 6;
        private const int GroveI2cLengthLimit = 32;
        private const string ManufacturerName = "Dexter Industries";
        private const string BoardName = "GoPiGo3";
        // this is used by GoPiGo3 when the data returned are correct
        private const byte SpiCorrectDataReturned = 0xA5;
        // This is used by GoPiGo3 when returning I2C data returned are correct
        private const byte I2cCorrectData = 0;

        private bool _shouldDispose;
        private SpiDevice _spiDevice;

        #region Properties

        /// <summary>
        /// Get the SPI address of GoPiGo3. By default, it is 8
        /// </summary>
        public byte SpiAddress { get; internal set; }

        /// <summary>
        /// Set or get a specific gear ratio for the encoder
        /// </summary>
        public int MotorGearRatio { get; set; }

        /// <summary>
        /// Set or get a specific ticks per rotation for the encoder
        /// </summary>
        public int EncoderTicksPerRotation { get; set; }

        /// <summary>
        /// Return the number of ticks per degree
        /// </summary>
        public double MotorTicksPerDegree => (MotorGearRatio * EncoderTicksPerRotation / 360.0);

        /// <summary>
        /// The 2 Grove Sensor
        /// </summary>
        public GroveSensor[] GroveSensor = new GroveSensor[2]
            {
                new(GrovePort.Grove1),
                new(GrovePort.Grove2)
            };

        /// <summary>
        /// The GoPiGo3 information including hardware, firmware, ID and Manufacturer
        /// </summary>
        public GoPiGoInfo? GoPiGo3Info { get; internal set; }

        /// <summary>
        /// Get the real 5V and Battery/VCC voltages
        /// </summary>
        public GoPiGoVoltage GoPiGoVoltage => new GoPiGoVoltage() { Voltage5V = GetVoltage5V(), VoltageBattery = GetVoltageBatteryVcc() };

        #endregion

        #region Constructor and Dispose

        /// <summary>
        /// Create a GoPiGo class
        /// </summary>
        /// <param name="spiDevice">The SpiDevice</param>
        /// <param name="spiAddress">The SPI address, by default 8</param>
        /// <param name="autoDetect">Try to autodetect the board</param>
        /// <param name="shouldDispose">True to dispose the SpiDevice when disposing the class</param>
        public GoPiGo(SpiDevice spiDevice, byte spiAddress = 8, bool autoDetect = true, bool shouldDispose = true)
        {
            _spiDevice = spiDevice ?? throw new ArgumentNullException(nameof(spiDevice));
            SpiAddress = spiAddress;
            _shouldDispose = shouldDispose;
            InitializeGoPiGo(autoDetect);
        }

        private void InitializeGoPiGo(bool autoDetect)
        {
            if (autoDetect == true)
            {
                try
                {
                    GoPiGo3Info = new GoPiGoInfo(
                        GetManufacturer(),
                        GetBoard(),
                        GetHardwareVersion(),
                        GetFirmwareVersion(),
                        GetIdHex());
                }
                catch (IOException ex)
                {
                    throw new IOException($"No SPI response. GoPiGo3 with address {SpiAddress} not connected.", ex);
                }
            }

            if ((GoPiGo3Info?.Manufacturer != ManufacturerName) || (GoPiGo3Info?.Board != BoardName))
            {
                throw new IOException($"GoPiGo3 with address {SpiAddress} not connected.");
            }

            MotorGearRatio = DefaultMotorGearRatio;
            EncoderTicksPerRotation = DefaultEncoderTicksPerRotation;
        }

        /// <summary>
        /// Reset everything
        /// </summary>
        public void Dispose()
        {
            // Reset Grove sensors
            SetGroveType(GrovePort.Both, GroveSensorType.Custom);
            SetGroveMode(GrovePort.Both, GroveInputOutput.InputDigital);
            // Turn off the motors
            SetMotorPower(MotorPort.Both, (byte)MotorSpeed.Float);
            // Reset the motor limits
            SetMotorLimits(MotorPort.Both, 0, 0);
            // Turn off the servos
            SetServo(ServoPort.Both, 0);
            // Turn off the LEDs
            SetLed((byte)GoPiGo3Led.LedEyeLeft + (byte)GoPiGo3Led.LedEyeRight + (byte)GoPiGo3Led.LedBlinkerLeft + (byte)GoPiGo3Led.LedBlinkerRight, Color.Black);
            if (_shouldDispose)
            {
                _spiDevice.Dispose();
            }
        }

        #endregion

        #region SPI transfer

        /// <summary>
        /// Conduct a SPI transaction
        /// </summary>
        /// <param name="buffer">A byte array to send.The length of the array will determine how many bytes are transferred.</param>
        /// <returns>Returns an array of the bytes read.</returns>
        private byte[] SpiTransferArray(byte[] buffer)
        {
            byte[] result = new byte[buffer.Length];
            _spiDevice.TransferFullDuplex(buffer, result);
            return result;
        }

        /// <summary>
        /// Read a 32 bit value over SPI
        /// </summary>
        /// <param name="MessageType">The SPI message type</param>
        /// <returns>Returns the value read</returns>
        public int SpiRead32(SpiMessageType MessageType)
        {
            byte[] outArray = { SpiAddress, (byte)MessageType, 0, 0, 0, 0, 0, 0 };
            byte[] reply = SpiTransferArray(outArray);

            if (reply[3] == SpiCorrectDataReturned)
            {
                return BinaryPrimitives.ReadInt32BigEndian(new Span<byte>(reply).Slice(4));
            }

            throw new IOException($"{nameof(SpiRead32)} : no SPI response");
        }

        /// <summary>
        /// Read a 16 bit value over SPI
        /// </summary>
        /// <param name="MessageType">The SPI message type</param>
        /// <returns>Returns the value read</returns>
        public short SpiRead16(SpiMessageType MessageType)
        {
            byte[] outArray = { SpiAddress, (byte)MessageType, 0, 0, 0, 0, };
            byte[] reply = SpiTransferArray(outArray);

            if (reply[3] == SpiCorrectDataReturned)
            {
                return BinaryPrimitives.ReadInt16BigEndian(new Span<byte>(reply).Slice(4));
            }

            throw new IOException($"{nameof(SpiRead16)} : no SPI response");
        }

        /// <summary>
        /// Read a 8 bit value over SPI
        /// </summary>
        /// <param name="MessageType">The SPI message type</param>
        /// <returns>Returns the value read</returns>
        public byte SpiRead8(SpiMessageType MessageType)
        {
            byte[] outArray = { SpiAddress, (byte)MessageType, 0, 0, 0 };
            byte[] reply = SpiTransferArray(outArray);
            return (reply[3] == SpiCorrectDataReturned) ? reply[4] : throw new IOException($"{nameof(SpiRead16)} : no SPI response");
        }

        /// <summary>
        /// Send a 8 bit value over SPI
        /// </summary>
        /// <param name="MessageType">The SPI message type</param>
        /// <param name="Value">The value to be sent</param>
        public void SpiWrite8(SpiMessageType MessageType, byte Value)
        {
            byte[] outArray = { SpiAddress, (byte)MessageType, Value };
            SpiTransferArray(outArray);
        }

        /// <summary>
        /// Send a 16 bit value over SPI
        /// </summary>
        /// <param name="MessageType">The SPI message type</param>
        /// <param name="Value">The value to be sent</param>
        public void SpiWrite16(SpiMessageType MessageType, short Value)
        {
            byte[] outArray = { SpiAddress, (byte)MessageType, (byte)((Value >> 8) & 0xFF), (byte)Value };
            SpiTransferArray(outArray);
        }

        /// <summary>
        /// Send a 24 bit value over SPI
        /// </summary>
        /// <param name="MessageType">The SPI message type</param>
        /// <param name="Value">The value to be sent</param>
        public void SpiWrite24(SpiMessageType MessageType, int Value)
        {
            byte[] outArray = { SpiAddress, (byte)MessageType, (byte)((Value >> 16) & 0xFF), (byte)((Value >> 8) & 0xFF), (byte)Value };
            SpiTransferArray(outArray);
        }

        /// <summary>
        /// Send a 32 bit value over SPI
        /// </summary>
        /// <param name="MessageType">The SPI message type</param>
        /// <param name="Value">The value to be sent</param>
        public void SpiWrite32(SpiMessageType MessageType, int Value)
        {
            byte[] outArray = { SpiAddress, (byte)MessageType, (byte)((Value >> 24) & 0xFF), (byte)((Value >> 16) & 0xFF), (byte)((Value >> 8) & 0xFF), (byte)Value };
            SpiTransferArray(outArray);
        }

        #endregion

        #region Board elements

        /// <summary>
        /// Read the 20 characters of GoPiGo3 manufacturer name
        /// </summary>
        /// <returns>Returns the GoPiGo3 manufacturer name string</returns>
        public string GetManufacturer()
        {
            byte[] outArray =
            {
                SpiAddress, (byte)SpiMessageType.GetManufacturer, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            };
            byte[] reply = SpiTransferArray(outArray);
            if (reply[3] != SpiCorrectDataReturned)
            {
                throw new IOException("No SPI response");
            }

            return Encoding.ASCII.GetString(reply.Skip(4).Where(c => c != 0).ToArray());
        }

        /// <summary>
        /// Read the 20 characters of GoPiGo3 board name
        /// </summary>
        /// <returns>Returns the GoPiGo3 board name string</returns>
        public string GetBoard()
        {
            byte[] outArray =
            {
                SpiAddress, (byte)SpiMessageType.GetName, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            };
            byte[] reply = SpiTransferArray(outArray);

            if (reply[3] != SpiCorrectDataReturned)
            {
                throw new IOException("No SPI response");
            }

            return Encoding.ASCII.GetString(reply.Skip(4).Where(c => c != 0).ToArray());
        }

        /// <summary>
        /// Read the hardware version
        /// </summary>
        /// <returns>Returns the hardware version as a string</returns>
        public Version GetHardwareVersion()
        {
            int version = SpiRead32(SpiMessageType.GetHardwareVersion);
            return new Version(version / 1000000, (version / 1000) % 1000, version % 1000);
        }

        /// <summary>
        /// Read the 128 bit GoPiGo3 hardware serial number
        /// </summary>
        /// <returns>Returns the serial number as 32 char HEX formatted string</returns>
        public string GetIdHex() => string.Join(string.Empty, GetId().Select((b) => b.ToString("X2")));

        /// <summary>
        /// Read the 128 bit GoPiGo3 hardware serial number
        /// </summary>
        /// <returns>Returns the serial number as a byte array</returns>
        public byte[] GetId()
        {
            byte[] outArray =
            {
                SpiAddress, (byte)SpiMessageType.GetId, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            };
            byte[] reply = SpiTransferArray(outArray);

            if (reply[3] != SpiCorrectDataReturned)
            {
                throw new IOException("No SPI response");
            }

            return reply.Skip(4).Take(16).ToArray();
        }

        /// <summary>
        /// Read the firmware version
        /// </summary>
        /// <returns>Returns the firmware version</returns>
        public Version GetFirmwareVersion()
        {
            int version = SpiRead32(SpiMessageType.GetFirmwareVersion);
            return new Version(version / 1000000, (version / 1000) % 1000, version % 1000);
        }

        /// <summary>
        /// Set the led color
        /// </summary>
        /// <param name="led">The led, either Left, Right, Blinky Left, Blinky Right and Wifi. Note you should only control the wifi one if you are sure to be connected to wifi</param>
        /// <param name="ledColor">The Color of the <paramref name="led"/></param>
        public void SetLed(byte led, Color ledColor)
        {
            byte[] outArray = { SpiAddress, (byte)SpiMessageType.SetLed, led, ledColor.R, ledColor.G, ledColor.B };
            var reply = SpiTransferArray(outArray);
        }

        /// <summary>
        /// Get the 5v circuit voltage
        /// </summary>
        /// <returns>Returns the real 5v circuit voltage</returns>
        public double GetVoltage5V()
        {
            var value = SpiRead16(SpiMessageType.GetVoltage5V);
            return (value / 1000.0);
        }

        /// <summary>
        /// Get the battery voltage
        /// </summary>
        /// <returns>Returns the real battery/Vcc voltage</returns>
        public double GetVoltageBatteryVcc()
        {
            var value = SpiRead16(SpiMessageType.GetVoltageVcc);
            return (value / 1000.0);
        }

        #endregion

        #region Servo and motors

        /// <summary>
        /// Move the servo motor with a specific pulse in microseconds
        /// </summary>
        /// <param name="servo">The servo port Servo1 or Servo2</param>
        /// <param name="pulseMicroseconds">The pulse in microseconds</param>
        public void SetServo(ServoPort servo, int pulseMicroseconds)
        {
            byte[] outArray = { SpiAddress, (byte)SpiMessageType.SetServo, (byte)servo, (byte)((pulseMicroseconds >> 8) & 0xFF), (byte)(pulseMicroseconds & 0xFF) };
            var reply = SpiTransferArray(outArray);
        }

        /// <summary>
        /// Set the motor power in percent
        /// </summary>
        /// <param name="port">The Motor port to use, can be MotorLeft and/or MotorRight</param>
        /// <param name="power">The power from - 100 to 100, or -128 for float</param>
        public void SetMotorPower(MotorPort port, int power)
        {
            power = Math.Clamp(power, -128, 127);
            byte bPower = (byte)(power & 0xFF);
            byte[] outArray = { SpiAddress, (byte)SpiMessageType.SetMotorPower, (byte)port, bPower };
            var ret = SpiTransferArray(outArray);
        }

        /// <summary>
        /// Set the motor target position in degrees
        /// </summary>
        /// <param name="port">The Motor port to use, can be MotorLeft and/or MotorRight</param>
        /// <param name="positionDegree">The target position in degree</param>
        public void SetMotorPosition(MotorPort port, int positionDegree)
        {
            byte[] outArray = { SpiAddress, (byte)SpiMessageType.SetMotorPosition, (byte)port, (byte)((positionDegree >> 24) & 0xFF), (byte)((positionDegree >> 16) & 0xFF), (byte)((positionDegree >> 8) & 0xFF), (byte)(positionDegree & 0xFF) };
            var ret = SpiTransferArray(outArray);
        }

        /// <summary>
        /// Set the motor target position KP constant
        /// If you set KP higher, the motor will be more responsive to errors in position, at the cost of perhaps overshooting and oscillating.
        /// KD slows down the motor as it approaches the target, and helps to prevent overshoot.
        /// In general, if you increase KP, you should also increase KD to keep the motor from overshooting and oscillating.
        /// See as well https://en.wikipedia.org/wiki/PID_controller
        /// </summary>
        /// <param name="port">The Motor port to use, can be MotorLeft and/or MotorRight</param>
        /// <param name="kp">The KP constant (default 25)</param>
        public void SetMotorPositionKP(MotorPort port, byte kp = 25)
        {
            byte[] outArray = { SpiAddress, (byte)SpiMessageType.SetMotorPositionKp, (byte)port, kp };
            var ret = SpiTransferArray(outArray);
        }

        /// <summary>
        /// Set the motor target position KD constant
        /// If you set KP higher, the motor will be more responsive to errors in position, at the cost of perhaps overshooting and oscillating.
        /// KD slows down the motor as it approaches the target, and helps to prevent overshoot.
        /// In general, if you increase kp, you should also increase KD to keep the motor from overshooting and oscillating.
        /// See as well https://en.wikipedia.org/wiki/PID_controller
        /// </summary>
        /// <param name="port">The Motor port to use, can be MotorLeft and/or MotorRight</param>
        /// <param name="kd">The KD constant (default 70)</param>
        public void SetMotorPositionKD(MotorPort port, byte kd = 70)
        {
            byte[] outArray = { SpiAddress, (byte)SpiMessageType.SetMotorPositionKd, (byte)port, kd };
            var ret = SpiTransferArray(outArray);
        }

        /// <summary>
        /// Set the motor target speed in degrees per second
        /// </summary>
        /// <param name="port">The Motor port to use, can be MotorLeft and/or MotorRight</param>
        /// <param name="dps">The target speed in degrees per second</param>
        public void SetMotorDps(MotorPort port, int dps)
        {
            byte[] outArray = { SpiAddress, (byte)SpiMessageType.SetMotorDps, (byte)port, (byte)((dps >> 8) & 0xFF), (byte)(dps & 0xFF) };
            var ret = SpiTransferArray(outArray);
        }

        /// <summary>
        /// Set the motor speed limit
        /// </summary>
        /// <param name="port">The Motor port to use, can be MotorLeft and/or MotorRight</param>
        /// <param name="powerPercent">The power limit in percent (0 to 100), with 0 being no limit (100)</param>
        /// <param name="dps">The speed limit in degrees per second, with 0 being no limit</param>
        public void SetMotorLimits(MotorPort port, byte powerPercent = 0, int dps = 0)
        {
            byte[] outArray = { SpiAddress, (byte)SpiMessageType.SetMotorLimits, (byte)port, powerPercent, (byte)((dps >> 8) & 0xFF), (byte)(dps & 0xFF) };
            var ret = SpiTransferArray(outArray);
        }

        /// <summary>
        /// Read a motor status
        /// </summary>
        /// <param name="port">The Motor port to use, can be MotorLeft or MotorRight</param>
        /// <returns>Returns MotorStatus containing the status of the motor</returns>
        public MotorStatus GetMotorStatus(MotorPort port)
        {
            MotorStatus motorStatus = new MotorStatus();
            SpiMessageType message_type = port == MotorPort.MotorRight ? SpiMessageType.GetMotorStatusRight : SpiMessageType.GetMotorStatusLeft;
            byte[] outArray = { SpiAddress, (byte)message_type, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            var reply = SpiTransferArray(outArray);
            if (reply[3] == SpiCorrectDataReturned)
            {
                motorStatus.Speed = reply[5];
                if ((motorStatus.Speed & 0x80) > 0)
                {
                    motorStatus.Speed = -motorStatus.Speed;
                }

                motorStatus.Encoder = (int)(BinaryPrimitives.ReadInt32BigEndian(new Span<byte>(reply).Slice(6)) / MotorTicksPerDegree);
                motorStatus.Dps = ((reply[10] << 8) | reply[11]);
                if ((motorStatus.Dps & 0x8000) > 0)
                {
                    motorStatus.Dps = motorStatus.Dps - 0x10000;
                }

                motorStatus.Flags = (MotorStatusFlags)reply[4];
            }
            else
            {
                throw new IOException($"{nameof(GetMotorStatus)} error: no SPI response");
            }

            return motorStatus;
        }

        /// <summary>
        /// Offset a motor encoder
        /// </summary>
        /// <param name="port">The Motor port to use, can be MotorLeft and/or MotorRight</param>
        /// <param name="positionOffset">The encoder offset. Zero the encoder by offsetting it by the current position</param>
        public void OffsetMotorEncoder(MotorPort port, int positionOffset)
        {
            positionOffset = (int)(positionOffset * MotorTicksPerDegree);
            byte[] outArray = new byte[] { SpiAddress, (byte)SpiMessageType.OffsetMotorEncoder, (byte)port, (byte)((positionOffset >> 24) & 0xFF), (byte)((positionOffset >> 16) & 0xFF), (byte)((positionOffset >> 8) & 0xFF), (byte)(positionOffset & 0xFF) };
            SpiTransferArray(outArray);
        }

        /// <summary>
        /// Read a motor encoder in degrees
        /// </summary>
        /// <param name="port">The Motor port to use, can be MotorLeft or MotorRight</param>
        /// <returns>Returns the encoder position in degrees</returns>
        public int GetMotorEncoder(MotorPort port)
        {
            SpiMessageType message_type;
            if (port == MotorPort.MotorLeft)
            {
                message_type = SpiMessageType.GetMotorEncoderLeft;
            }
            else if (port == MotorPort.MotorRight)
            {
                message_type = SpiMessageType.GetMotorEncoderRight;
            }
            else
            {
                throw new IOException($"{nameof(GetMotorEncoder)} error. Must be one motor port at a time, PortMotorLeft or PortMotorRight");
            }

            var encoder = SpiRead32(message_type);
            if ((encoder & 0x80000000) > 0)
            {
                encoder = (int)(encoder - 0x100000000);
            }

            return (int)(encoder / MotorTicksPerDegree);
        }

        #endregion

        #region Grove

        /// <summary>
        /// Set grove type
        /// </summary>
        /// <param name="port">The grove port(s). Grove1 and/or Grove2</param>
        /// <param name="type">The grove device type, refer to GroveSensorType</param>
        public void SetGroveType(GrovePort port, GroveSensorType type)
        {
            if ((port == GrovePort.Grove1) || (port == GrovePort.Both))
            {
                GroveSensor[0].SensorType = type;
            }

            if ((port == GrovePort.Grove2) || (port == GrovePort.Both))
            {
                GroveSensor[1].SensorType = type;
            }

            byte[] outArray = { SpiAddress, (byte)SpiMessageType.SetGroveType, (byte)port, (byte)type };
            SpiTransferArray(outArray);
        }

        /// <summary>
        /// Set grove analog digital pin mode as input or output
        /// </summary>
        /// <param name="port">The Grove Pin, can be any combination of Grove1Pin1, Grove1Pin2, Grove2Pin1 and/or Grove2Pin2</param>
        /// <param name="mode">The Grove pin mode, refere to GroveInputOutput</param>
        public void SetGroveMode(GrovePort port, GroveInputOutput mode)
        {
            byte[] outArray = { SpiAddress, (byte)SpiMessageType.SetGroveMode, (byte)port, (byte)mode };
            SpiTransferArray(outArray);
        }

        /// <summary>
        /// Set grove output pin low or high
        /// </summary>
        /// <param name="port">The Grove Pin, can be any combination of Grove1Pin1, Grove1Pin2, Grove2Pin1 and/or Grove2Pin2</param>
        /// <param name="state">The pin state. false for low or true for high.</param>
        public void SetGroveState(GrovePort port, bool state)
        {
            byte[] outArray = { SpiAddress, (byte)SpiMessageType.SetGroveState, (byte)port, (byte)(state ? 1 : 0) };
            SpiTransferArray(outArray);
        }

        /// <summary>
        /// Set grove output pin PWM
        /// </summary>
        /// <param name="port">The Grove Pin, can be any combination of Grove1Pin1, Grove1Pin2, Grove2Pin1 and/or Grove2Pin2</param>
        /// <param name="duty">The PWM duty cycle in percent from 0.0 to 100.0, 1 floating point precision</param>
        public void SetGrovePwmDuty(GrovePort port, double duty)
        {
            duty = Math.Clamp(duty, (byte)0, (byte)100);

            var duty_value = (UInt16)(duty * 10.0);
            byte[] outArray = { SpiAddress, (byte)SpiMessageType.SetGrovePwmDuty, (byte)port, (byte)((duty_value >> 8) & 0xFF), (byte)(duty_value & 0xFF) };
            SpiTransferArray(outArray);
        }

        /// <summary>
        /// Set grove PWM frequency
        /// </summary>
        /// <param name="port">The grove port(s).GROVE_1 and / or GROVE_2.</param>
        /// <param name="freq">The PWM frequency.Range is 3 through 48000Hz.Default is 24000(24kHz). Limit to 48000, which is the highest frequency supported for 0.1% resolution.</param>
        public void GetGrovePwmFrequency(GrovePort port, uint freq = 24000)
        {
            freq = Math.Clamp(freq, 3, 48000);
            byte[] outArray = { SpiAddress, (byte)SpiMessageType.SetGrovePwmFrequency, (byte)port, (byte)((freq >> 8) & 0xFF), (byte)(freq & 0xFF) };
            SpiTransferArray(outArray);
        }

        /// <summary>
        /// Conduct an I2C transaction
        /// </summary>
        /// <param name="port">The grove port. GROVE_1 or GROVE_2</param>
        /// <param name="addr">The I2C address of the slave to be addressed.</param>
        /// <param name="arrayToSend">An array of bytes to send.</param>
        /// <param name="inBytes">The number of bytes to read.</param>
        /// <returns>Returns a byte array with what has been read from the I2C element</returns>
        public byte[] GroveI2cTransfer(GrovePort port, byte addr, byte[] arrayToSend, byte inBytes = 0)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            long timeout = stopwatch.ElapsedMilliseconds + 5;
            while (true)
            {
                try
                {
                    GroveI2cStart(port, addr, arrayToSend, inBytes);
                    break;
                }
                catch (IOException ex)
                {
                    if (stopwatch.ElapsedMilliseconds > timeout)
                    {
                        throw new IOException($"{nameof(GroveI2cTransfer)} error: timeout while transfering the I2C data", ex);
                    }
                }
            }

            // Wait for the sensors to be read
            // In theory 115µs per byte sent
            int towait = 0;
            if (arrayToSend is object)
            {
                if (arrayToSend.Length != 0)
                {
                    towait += 1 + arrayToSend.Length;
                }
            }

            if (inBytes > 0)
            {
                towait += 1 + inBytes;
            }

            timeout += (int)(0.115 * towait);
            // but make sure we wait a minimum of 1 ms
            if (towait > 0)
            {
                timeout = Math.Clamp(timeout, 1, 32);
                Thread.Sleep((int)timeout);
            }

            timeout = stopwatch.ElapsedMilliseconds + 5;
            while (true)
            {
                try
                {
                    return GetGroveValue(port);
                }
                catch (IOException ex)
                {
                    throw new IOException($"{nameof(GroveI2cTransfer)} error: timeout while transfering the I2C data", ex);
                }
            }
        }

        /// <summary>
        /// Start an I2C transaction
        /// </summary>
        /// <param name="port">The Grove Port, one at the time Grove1 or Grove2</param>
        /// <param name="addr">The I2C address of the slave to be addressed.</param>
        /// <param name="arrayToSend">An array of bytes to send.</param>
        /// <param name="inBytes">The number of bytes to read.</param>
        public void GroveI2cStart(GrovePort port, byte addr, byte[] arrayToSend, byte inBytes = 0)
        {
            SpiMessageType message_type;
            byte port_index;
            if (port == GrovePort.Grove1)
            {
                message_type = SpiMessageType.StartGrove1I2c;
                port_index = 0;
            }
            else if (port == GrovePort.Grove2)
            {
                message_type = SpiMessageType.StartGrove2I2c;
                port_index = 1;
            }
            else
            {
                throw new ArgumentException(nameof(port), $"Port unsupported. Must be either {nameof(GrovePort.Grove1)} or {nameof(GrovePort.Grove2)}.");
            }

            var address = ((addr & 0x7F) << 1);
            if (inBytes > GroveI2cLengthLimit)
            {
                throw new ArgumentException(nameof(addr), $"Read length error. Up to {GroveI2cLengthLimit} bytes can be read in a single transaction.");
            }

            if (arrayToSend.Length > GroveI2cLengthLimit)
            {
                throw new ArgumentException(nameof(arrayToSend), $"Write length error. Up to {GroveI2cLengthLimit}  bytes can be written in a single transaction.");
            }

            byte[] outArray = { SpiAddress, (byte)message_type, (byte)address, inBytes, (byte)arrayToSend.Length };
            Array.Resize(ref outArray, outArray.Length + arrayToSend.Length);
            Array.Copy(arrayToSend, 0, outArray, outArray.Length - arrayToSend.Length, arrayToSend.Length);
            byte[] reply = SpiTransferArray(outArray);

            GroveSensor[port_index].I2cDataLength = inBytes;
            if (reply[3] != SpiCorrectDataReturned)
            {
                throw new IOException($"{nameof(GroveI2cStart)} error: No SPI response");
            }

            if (reply[4] != I2cCorrectData)
            {
                throw new IOException($"{nameof(GroveI2cStart)} error: Not ready to start I2C transaction");
            }
        }

        /// <summary>
        /// Get a grove port value
        /// </summary>
        /// <param name="port">The Grove Port, one at the time Grove1 or Grove2</param>
        /// <returns>Returns a byte array containing the read data</returns>
        public byte[] GetGroveValue(GrovePort port)
        {
            string errorSpi = $"{nameof(GetGroveValue)} error: No SPI response";
            string errorInvalidValue = $"{nameof(GetGroveValue)} error: Invalid value";
            SpiMessageType message_type;
            byte port_index;
            if (port == GrovePort.Grove1)
            {
                message_type = SpiMessageType.GetGrove1Value;
                port_index = 0;
            }
            else if (port == GrovePort.Grove2)
            {
                message_type = SpiMessageType.GetGrove2Value;
                port_index = 1;
            }
            else
            {
                throw new ArgumentException(nameof(port), $"Port unsupported. Must be either {nameof(GrovePort.Grove1)} or {nameof(GrovePort.Grove2)}.");
            }

            byte[]? outArray = null;
            byte[]? reply = null;
            switch (GroveSensor[port_index].SensorType)
            {
                case GroveSensorType.InfraredRemote:
                    outArray = new byte[] { SpiAddress, (byte)message_type, 0, 0, 0, 0, 0 };
                    reply = SpiTransferArray(outArray);
                    if (reply[3] == SpiCorrectDataReturned)
                    {
                        if ((reply[4] == (byte)GroveSensor[port_index].SensorType) && (reply[5] == I2cCorrectData))
                        {
                            return new byte[] { reply[6] };
                        }
                        else
                        {
                            throw new IOException(errorInvalidValue);
                        }
                    }
                    else
                    {
                        throw new IOException(errorSpi);
                    }

                case GroveSensorType.InfraredEV3Remote:
                    outArray = new byte[] { SpiAddress, (byte)message_type, 0, 0, 0, 0, 0, 0, 0, 0 };
                    reply = SpiTransferArray(outArray);
                    if (reply[3] == SpiCorrectDataReturned)
                    {
                        if ((reply[4] == (byte)GroveSensor[port_index].SensorType) && (reply[5] == I2cCorrectData))
                        {
                            return new byte[] { reply[6], reply[7], reply[8], reply[9] };
                        }
                        else
                        {
                            throw new IOException(errorInvalidValue);
                        }
                    }
                    else
                    {
                        throw new IOException(errorSpi);
                    }

                case GroveSensorType.Ultrasonic:
                    outArray = new byte[] { SpiAddress, (byte)message_type, 0, 0, 0, 0, 0, 0 };
                    reply = SpiTransferArray(outArray);
                    if (reply[3] == SpiCorrectDataReturned)
                    {
                        if ((reply[4] == (byte)GroveSensor[port_index].SensorType) && (reply[5] == I2cCorrectData))
                        {
                            return new byte[] { reply[6], reply[7] };
                        }
                        else
                        {
                            throw new IOException(errorInvalidValue);
                        }
                    }
                    else
                    {
                        throw new IOException(errorSpi);
                    }

                case GroveSensorType.I2c:
                    outArray = new byte[6 + GroveSensor[port_index].I2cDataLength];
                    outArray[0] = SpiAddress;
                    outArray[1] = (byte)message_type;
                    // next 4 bytes are 0 then fill with the number of inbytes data
                    for (int i = 0; i < 4 + GroveSensor[port_index].I2cDataLength; i++)
                    {
                        outArray[2 + i] = 0;
                    }

                    reply = SpiTransferArray(outArray);
                    if (reply[3] == SpiCorrectDataReturned)
                    {
                        if (reply[4] == (byte)GroveSensor[port_index].SensorType)
                        {
                            if (reply[5] == (byte)GroveSensorState.ValidData)
                            {
                                return reply.Skip(5).ToArray();
                            }
                            else if (reply[5] == (byte)GroveSensorState.I2cError)
                            {
                                throw new IOException($"{nameof(GetGroveValue)} error: I2C bus error");
                            }
                            else
                            {
                                throw new IOException(errorInvalidValue);
                            }
                        }
                        else
                        {
                            throw new IOException($"{nameof(GetGroveValue)} error: Grove type mismatch");
                        }
                    }
                    else
                    {
                        throw new IOException(errorSpi);
                    }

                case GroveSensorType.None:
                case GroveSensorType.Custom:
                default:
                    return new byte[] { (byte)SpiRead8(message_type) };
            }
        }

        /// <summary>
        /// Get a grove input pin state
        /// </summary>
        /// <param name="port">The Grove Pin, one at the time Grove1Pin1, Grove1Pin2, Grove2Pin1 or Grove2Pin2</param>
        /// <returns>Returns the pin state</returns>
        public byte GetGroveState(GrovePort port)
        {
            SpiMessageType message_type = port switch
            {
                GrovePort.Grove1Pin1 => SpiMessageType.GetGrove1Pin1State,
                GrovePort.Grove1Pin2 => SpiMessageType.GetGrove1Pin2State,
                GrovePort.Grove2Pin1 => SpiMessageType.GetGrove2Pin1State,
                GrovePort.Grove2Pin2 => SpiMessageType.GetGrove2Pin2State,
                _ => throw new ArgumentException(nameof(port), "Pin(s) unsupported. Must get one at a time."),
            };

            byte[] outArray = { SpiAddress, (byte)message_type, 0, 0, 0, 0 };
            var reply = SpiTransferArray(outArray);
            if (reply[3] == SpiCorrectDataReturned)
            {
                if (reply[4] == (byte)GroveSensorState.ValidData)
                {
                    return reply[5];
                }
                else
                {
                    throw new IOException($"{nameof(GetGroveState)} error: Invalid value");
                }
            }
            else
            {
                throw new IOException($"{nameof(GetGroveState)} error: Grove type mismatch");
            }
        }

        /// <summary>
        /// Get a grove input pin analog voltage
        /// </summary>
        /// <param name="port">The Grove Pin, one at the time Grove1Pin1, Grove1Pin2, Grove2Pin1 or Grove2Pin2</param>
        /// <returns>Returns the voltage in V</returns>
        public double GetGroveVoltage(GrovePort port)
        {
            SpiMessageType message_type = port switch
            {
                GrovePort.Grove1Pin1 => SpiMessageType.GetGrove1Pin1Voltage,
                GrovePort.Grove1Pin2 => SpiMessageType.GetGrove1Pin2Voltage,
                GrovePort.Grove2Pin1 => SpiMessageType.GetGrove2Pin1Voltage,
                GrovePort.Grove2Pin2 => SpiMessageType.GetGrove2Pin2Voltage,
                _ => throw new ArgumentException(nameof(port), "Pin(s) unsupported. Must get one at a time."),
            };

            byte[] outArray = { SpiAddress, (byte)message_type, 0, 0, 0, 0, 0 };
            var reply = SpiTransferArray(outArray);
            if (reply[3] == SpiCorrectDataReturned)
            {
                if (reply[4] == (byte)GroveSensorState.ValidData)
                {
                    return ((reply[5] << 8) + reply[6]) / 1000.0;
                }
                else
                {
                    throw new IOException($"{nameof(GetGroveVoltage)} error: Invalid value");
                }
            }
            else
            {
                throw new IOException($"{nameof(GetGroveVoltage)} error: Grove type mismatch");
            }
        }

        /// <summary>
        /// Get a grove input pin 12-bit raw ADC reading
        /// </summary>
        /// <param name="port">The Grove Pin, one at the time Grove1Pin1, Grove1Pin2, Grove2Pin1 or Grove2Pin2</param>
        /// <returns>Returns the analogic read</returns>
        public int GetGroveAnalog(GrovePort port)
        {
            SpiMessageType message_type = port switch
            {
                GrovePort.Grove1Pin1 => SpiMessageType.GetGrove1Pin1Analog,
                GrovePort.Grove1Pin2 => SpiMessageType.GetGrove1Pin2Analog,
                GrovePort.Grove2Pin1 => SpiMessageType.GetGrove2Pin1Analog,
                GrovePort.Grove2Pin2 => SpiMessageType.GetGrove2Pin2Analog,
                _ => throw new ArgumentException(nameof(port), "Pin(s) unsupported. Must get one at a time."),
            };

            byte[] outArray = { SpiAddress, (byte)message_type, 0, 0, 0, 0, 0 };
            var reply = SpiTransferArray(outArray);
            if (reply[3] == SpiCorrectDataReturned)
            {
                if (reply[4] == (byte)GroveSensorState.ValidData)
                {
                    return ((reply[5] << 8) + reply[6]);
                }
                else
                {
                    throw new IOException($"{nameof(GetGroveAnalog)} error: Invalid value");
                }
            }
            else
            {
                throw new IOException($"{nameof(GetGroveAnalog)} error: Grove type mismatch");
            }
        }

        #endregion
    }
}
