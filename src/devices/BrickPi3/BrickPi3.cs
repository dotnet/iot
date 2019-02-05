// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Iot.Device.BrickPi3.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Device.Spi;
using System.Device.Spi.Drivers;
using System.Linq;
using System.IO;

namespace Iot.Device.BrickPi3
{
    /// <summary>
    /// The main Brick class allowing low level access to motors and sensors
    /// </summary>
    public class Brick: IDisposable
    {
        // To store the Sensor types as well as date to be sent when it's an I2C sensor
        private SensorType[] _sensorType = { SensorType.None, SensorType.None, SensorType.None, SensorType.None };
        private byte[] _i2cInBytes = { 0, 0, 0, 0 };
        //Initernals to initalize the SPI
        private static SpiDevice _brickPiSPI = null;
        private BrickPiVoltage _brickPiVoltage = new BrickPiVoltage();

        #region Properties

        /// <summary>
        /// used to store the SPI Address
        /// used mainly when multiple bricks in a raw or not the default SPI address 
        /// up to 254 addresses supported
        /// </summary>
        public byte SpiAddress { get; set; }

        /// <summary>
        /// Stores all the information regarding the Brick
        /// This includes id, hardware and firmware versions
        /// </summary>
        public BrickPiInfo BrickPi3Info { get; internal set; }

        /// <summary>
        /// Stores the voltage information
        /// </summary>
        public BrickPiVoltage BrickPi3Voltage
        {
            get
            {
                _brickPiVoltage.Voltage3V3 = GetVoltage3V3();
                _brickPiVoltage.Voltage5V = GetVoltage5V();
                _brickPiVoltage.Voltage9V = GetVoltage9V();
                _brickPiVoltage.VoltageBattery = GetVoltageBatteryVcc();
                return _brickPiVoltage;
            }
        }

        #endregion

        #region Init and reset

        /// <summary>
        /// Initialize the brick including SPI communication
        /// </summary>
        /// <param name="spiAddress"></param>
        public Brick(byte spiAddress = 1, int busId = 0, int ChipSelectLine = 1)
        {
            try
            {
                SpiAddress = spiAddress;
                // SPI 0 is used on Raspberry with ChipSelectLine 1
                var settings = new SpiConnectionSettings(busId, ChipSelectLine);
                // 500K is the SPI communication with the brick
                settings.ClockFrequency = 500000;
                // see http://tightdev.net/SpiDev_Doc.pdf
                settings.Mode = SpiMode.Mode0;
                settings.DataBitLength = 8;
                // as the SPI is a static, checking if it has already be initialised
                if (_brickPiSPI == null)
                {
                    _brickPiSPI = new UnixSpiDevice(settings);
                }
                BrickPi3Info = new BrickPiInfo();
                BrickPi3Info.Manufacturer = GetManufacturer();
                BrickPi3Info.Board = GetBoard();
                BrickPi3Info.HardwareVersion = GetHardwareVersion();
                BrickPi3Info.SoftwareVersion = GetFirmwareVersion();
                BrickPi3Info.Id = GetId();
            }
            catch (Exception ex) when (ex is IOException)
            {
                Debug.Write($"Exception: {ex.Message}");
            }
        }

        /// <summary>
        /// Reset the brick, stop the motors, release sensors
        /// </summary>
        public void Dispose()
        {
            // Reset the BrickPi.Set all the sensors' type to NONE, set the motors to float, and motors' limits and constants to default, and return control of the LED to the firmware.
            // reset all sensors
            SetSensorType((byte)(SensorPort.Port1) + (byte)(SensorPort.Port2) + (byte)(SensorPort.Port3) + (byte)(SensorPort.Port4), SensorType.None);
            // turn off all motors
            byte allmotors = (byte)(MotorPort.PortA) + (byte)(MotorPort.PortB) + (byte)(MotorPort.PortC) + (byte)(MotorPort.PortD);
            SetMotorPower(allmotors, (byte)MotorSpeed.Float);
            // reset motor limits
            SetMotorLimits(allmotors);
            // reset motor kP and kD constants
            SetMotorPositionKP(allmotors);
            SetMotorPositionKD(allmotors);
            // return the LED to the control of the FW
            SetLed(255);
        }

        #endregion

        #region SPI transfer

        /// <summary>
        /// Conduct a SPI transaction
        /// </summary>
        /// <param name="dataOut">a list of bytes to send.The length of the list will determine how many bytes are transferred.</param>
        /// <returns>Returns an array of the bytes read.</returns>
        public byte[] SpiTransferArray(byte[] dataOut)
        {
            byte[] result = new byte[dataOut.Length];
            _brickPiSPI.TransferFullDuplex(dataOut, result);
            return result;
        }

        /// <summary>
        /// Read a 32 - bit value over SPI
        /// </summary>
        /// <param name="MessageType">the SPI message type</param>
        /// <returns>the value read</returns>
        public int SpiRead32(SpiMessageType MessageType)
        {
            int retVal = -1;
            byte[] outArray = { SpiAddress, (byte)MessageType, 0, 0, 0, 0, 0, 0 };
            byte[] reply = SpiTransferArray(outArray);

            if (reply[3] == 0xA5)
            {
                retVal = (int)(reply[4] << 24) | (reply[5] << 16) | (reply[6] << 8) | reply[7];
            }
            else
            {
                throw new IOException($"{nameof(SpiRead32)} : no SPI response");
            }

            return retVal;
        }

        /// <summary>
        /// Read a 16 - bit value over SPI
        /// </summary>
        /// <param name="MessageType">the SPI message type</param>
        /// <returns>the vallue read</returns>
        public int SpiRead16(SpiMessageType MessageType)
        {
            int retVal = -1;
            byte[] outArray = { SpiAddress, (byte)MessageType, 0, 0, 0, 0, };
            byte[] reply = SpiTransferArray(outArray);

            if (reply[3] == 0xA5)
            {
                retVal = (int)(reply[4] << 8) | reply[5];
            }
            else
            {
                throw new IOException($"{nameof(SpiRead16)} : no SPI response");
            }

            return retVal;
        }

        /// <summary>
        /// Send a 8 - bit value over SPI
        /// </summary>
        /// <param name="MessageType">the SPI message type</param>
        /// <param name="Value">the value to be sent</param>
        public void SpiWrite8(SpiMessageType MessageType, int Value)
        {
            byte[] outArray = { SpiAddress, (byte)MessageType, (byte)(Value & 0xFF) };
            SpiTransferArray(outArray);
        }

        /// <summary>
        /// Send a 16 - bit value over SPI
        /// </summary>
        /// <param name="MessageType">the SPI message type</param>
        /// <param name="Value">the value to be sent</param>
        public void SpiWrite16(SpiMessageType MessageType, int Value)
        {
            byte[] outArray = { SpiAddress, (byte)MessageType, (byte)((Value >> 8) & 0xFF), (byte)(Value & 0xFF) };
            SpiTransferArray(outArray);
        }

        /// <summary>
        /// Send a 24 - bit value over SPI
        /// </summary>
        /// <param name="MessageType">the SPI message type</param>
        /// <param name="Value">the value to be sent</param>
        public void SpiWrite24(SpiMessageType MessageType, int Value)
        {
            byte[] outArray = { SpiAddress, (byte)MessageType, (byte)((Value >> 16) & 0xFF), (byte)((Value >> 8) & 0xFF), (byte)(Value & 0xFF) };
            SpiTransferArray(outArray);
        }

        /// <summary>
        /// Send a 32 - bit value over SPI
        /// </summary>
        /// <param name="MessageType">the SPI message type</param>
        /// <param name="Value">the value to be sent</param>
        public void SpiWrite32(SpiMessageType MessageType, int Value)
        {
            byte[] outArray = { SpiAddress, (byte)MessageType, (byte)((Value >> 24) & 0xFF), (byte)((Value >> 16) & 0xFF), (byte)((Value >> 8) & 0xFF), (byte)(Value & 0xFF) };
            SpiTransferArray(outArray);
        }

        #endregion

        #region Board elements

        /// <summary>
        /// Read the 20 charactor BrickPi3 manufacturer name
        /// </summary>
        /// <returns>BrickPi3 manufacturer name string</returns>
        public string GetManufacturer()
        {
            string retVal = string.Empty;
            byte[] outArray = {SpiAddress, (byte)SpiMessageType.GetManufacturer,
                  0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            byte[] reply = SpiTransferArray(outArray);
            if (reply[3] != 0xA5)
            {
                throw new IOException("No SPI response");
            }
            else
            {
                for (int ndx = 4; ndx < 24; ++ndx)
                {
                    if (reply[ndx] != 0)
                    {
                        retVal += (char)reply[ndx];
                    }
                    else
                    {
                        break;
                    }
                }
            }
            return retVal;
        }

        /// <summary>
        /// Read the 20 charactor BrcikPi3 board name
        /// </summary>
        /// <returns>BrcikPi3 board name string</returns>
        public string GetBoard()
        {
            string retVal = string.Empty;
            byte[] outArray = {SpiAddress, (byte)SpiMessageType.GetName,
                    0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            byte[] reply = SpiTransferArray(outArray);

            if (reply[3] == 0xA5)
            {
                for (int ndx = 4; ndx < 24; ++ndx)
                {
                    if (reply[ndx] != 0)
                    {
                        retVal += (char)(reply[ndx]);
                    }
                    else
                    {
                        break;
                    }
                }
            }
            else
            {
                throw new IOException("No SPI response");
            }
            return retVal;
        }

        /// <summary>
        /// Read the hardware version
        /// </summary>
        /// <returns>hardware version</returns>
        public string GetHardwareVersion()
        {
            string retVal = string.Empty;
            int version = SpiRead32(SpiMessageType.GetHardwareVersion);
            retVal = $"{version / 1000000}.{(version / 1000) % 1000}.{version % 1000}";
            return retVal;
        }

        /// <summary>
        /// Read the 128 - bit BrcikPi3 hardware serial number
        /// </summary>
        /// <returns>serial number as 32 char HEX formatted string</returns>
        public string GetId()
        {
            string retVal = string.Empty;
            byte[] outArray = {SpiAddress, (byte)SpiMessageType.GetId,
                     0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            byte[] reply = SpiTransferArray(outArray);

            if (reply[3] == 0xA5)
            {
                retVal = string.Join("", reply.Skip(4).Take(16).Select((b) => b.ToString("X2")));
            }
            else
            {
                throw new IOException("No SPI response");
            }
            return retVal;
        }

        /// <summary>
        /// Read the firmware version
        /// </summary>
        /// <returns>firmware version</returns>
        public string GetFirmwareVersion()
        {
            string retVal = string.Empty;
            int version = SpiRead32(SpiMessageType.GetFirmwareVersion);
            retVal = string.Format("{0}.{1}.{2}", (version / 1000000), ((version / 1000) % 1000), (version % 1000));
            return retVal;
        }

        /// <summary>
        /// Set the SPI address of the BrickPi3
        /// </summary>
        /// <param name="address">the new SPI address to use(1 to 255)</param>
        /// <param name="id">the BrickPi3's unique serial number ID (so that the address can be set while multiple BrickPi3s are stacked on a Raspberry Pi).</param>
        public void SetAddress(byte address, string id)
        {
            byte[] id_arr = new byte[16];
            if (id.Length != 32)
            {
                if (id != "") throw new IOException("brickpi3.set_address error: wrong serial number id length. Must be a 32-digit hex string.");
            }
            if (id.Length == 32)
            {
                var isok = false;
                for (int i = 0; i < 16; i++)
                {
                    isok = byte.TryParse(id.Substring(i * 2, i * 2 + 1), System.Globalization.NumberStyles.HexNumber, null as IFormatProvider, out id_arr[i]);
                    if (!isok)
                        break;
                }
                if (!isok) throw new IOException("brickpi3.set_address error: unknown serial number id problem. Make sure to use a valid 32-digit hex string serial number.");
            }
            byte[] outArray = new byte[19];
            outArray[0] = 0;
            outArray[1] = (byte)SpiMessageType.SetAddress;
            outArray[2] = address;
            for (int i = 0; i < 16; i++)
                outArray[3 + i] = id_arr[i];
            var ret = SpiTransferArray(outArray);
            if (ret[3] == 0xA5)
                SpiAddress = address;
        }

        /// <summary>
        /// Set the Led intensity from 0 (off) to 100 (fully bright), 255 used to return the led to BrickPi3 management
        /// </summary>
        /// <param name="intensity"></param>
        public void SetLed(byte intensity)
        {
            byte[] outArray = { SpiAddress, (byte)SpiMessageType.SetLed, intensity };
            var reply = SpiTransferArray(outArray);
        }

        /// <summary>
        /// Get the 3.3v circuit voltage
        /// </summary>
        /// <returns>3.3v circuit voltage</returns>
        public double GetVoltage3V3()
        {
            var value = SpiRead16(SpiMessageType.GetVoltage3V3);
            return (value / 1000.0);
        }

        /// <summary>
        /// Get the 5v circuit voltage
        /// </summary>
        /// <returns>5v circuit voltage</returns>
        public double GetVoltage5V()
        {
            var value = SpiRead16(SpiMessageType.GetVoltage5V);
            return (value / 1000.0);
        }

        /// <summary>
        /// Get the 9v circuit voltage
        /// </summary>
        /// <returns>9v circuit voltage</returns>
        public double GetVoltage9V()
        {
            var value = SpiRead16(SpiMessageType.GetVoltage9V);
            return (value / 1000.0);
        }

        /// <summary>
        /// Get the battery voltage
        /// </summary>
        /// <returns>battery/Vcc voltage</returns>
        public double GetVoltageBatteryVcc()
        {
            var value = SpiRead16(SpiMessageType.GetVoltageVcc);
            return (value / 1000.0);
        }

        #endregion

        #region Sensors

        /// <summary>
        /// Get the sensor data from a specific port
        /// The following sensor types each return a single value:
        ///   NONE----------------------- 0
        ///   TOUCH---------------------- 0 or 1(released or pressed)
        ///   NXT_TOUCH------------------ 0 or 1(released or pressed)
        ///   EV3_TOUCH------------------ 0 or 1(released or pressed)
        ///   NXT_ULTRASONIC------------- distance in CM
        ///   NXT_LIGHT_ON--------------- reflected light
        ///   NXT_LIGHT_OFF-------------- ambient light
        ///   NXT_COLOR_RED-------------- red reflected light
        ///   NXT_COLOR_GREEN------------ green reflected light
        ///   NXT_COLOR_BLUE------------- blue reflected light
        ///   NXT_COLOR_OFF-------------- ambient light
        ///   EV3_GYRO_ABS--------------- absolute rotation position in degrees
        ///   EV3_GYRO_DPS--------------- rotation rate in degrees per second
        ///   EV3_COLOR_REFLECTED-------- red reflected light
        ///   EV3_COLOR_AMBIENT---------- ambient light
        ///   EV3_COLOR_COLOR------------ detected color
        ///   EV3_ULTRASONIC_CM---------- distance in CM
        ///   EV3_ULTRASONIC_INCHES------ distance in inches
        ///   EV3_ULTRASONIC_LISTEN------ 0 or 1(no other ultrasonic sensors or another ultrasonic sensor detected)
        ///   EV3_INFRARED_PROXIMITY----- distance 0 - 100 %
        /// The following sensor types each return a list of values
        ///   CUSTOM--------------------- Pin 1 ADC(5v scale from 0 to 4095), Pin 6 ADC(3.3v scale from 0 to 4095), Pin 5 digital, Pin 6 digital
        ///   I2C------------------------ the I2C bytes read
        ///   NXT_COLOR_FULL------------- detected color, red light reflected, green light reflected, blue light reflected, ambient light
        ///   EV3_GYRO_ABS_DPS----------- absolute rotation position in degrees, rotation rate in degrees per second
        ///   EV3_COLOR_RAW_REFLECTED---- red reflected light, unknown value(maybe a raw ambient value ?)
        ///   EV3_COLOR_COLOR_COMPONENTS- red reflected light, green reflected light, blue reflected light, unknown value(maybe a raw value ?)
        ///   EV3_INFRARED_SEEK---------- a list for each of the four channels.For each channel heading(-25 to 25), distance(-128 or 0 to 100)
        ///   EV3_INFRARED_REMOTE-------- a list for each of the four channels.For each channel red up, red down, blue up, blue down, boadcast
        /// </summary>
        /// <param name="port">The sensor port(one at a time). Port 1, 2, 3, or 4</param>
        /// <returns>Returns the value(s) for the specified sensor.</returns>
        public byte[] GetSensor(byte port)
        {
            string IOErrorMessage = $"{nameof(GetSensor)} error: No SPI response";
            string SensorErrorInvalidData = $"{nameof(GetSensor)} error: Invalid sensor data";

            byte port_index = 0;
            SpiMessageType message_type = SpiMessageType.None;

            if (port == (byte)SensorPort.Port1)
            {
                message_type = SpiMessageType.GetSensor1;
                port_index = 0;
            }
            else if (port == (byte)SensorPort.Port2)
            {
                message_type = SpiMessageType.GetSensor2;
                port_index = 1;
            }
            else if (port == (byte)SensorPort.Port3)
            {
                message_type = SpiMessageType.GetSensor3;
                port_index = 2;
            }
            else if (port == (byte)SensorPort.Port4)
            {
                message_type = SpiMessageType.GetSensor4;
                port_index = 3;
            }
            else
                throw new IOException($"{nameof(GetSensor)} error. Must be one sensor port at a time. PORT_1, PORT_2, PORT_3, or PORT_4.");

            List<byte> outArray = new List<byte>();
            byte[] reply;

            if (_sensorType[port_index] == Models.SensorType.Custom)
            {
                outArray.AddRange(new byte[] { SpiAddress, (byte)message_type, 0, 0, 0, 0, 0, 0, 0, 0 });
                reply = SpiTransferArray(outArray.ToArray());
                if (reply[3] == 0xA5)
                {
                    if ((reply[4] == (int)_sensorType[port_index]) && (reply[5] == (int)SensorState.ValidData))
                        return new byte[] { (byte)(((reply[8] & 0x0F) << 8) | reply[9]), (byte)(((reply[8] >> 4) & 0x0F) | (reply[7] << 4)), (byte)(reply[6] & 0x01), (byte)((reply[6] >> 1) & 0x01) };
                    else
                        throw new IOException(SensorErrorInvalidData);
                }
                else
                    throw new IOException(IOErrorMessage);
            }
            else if (_sensorType[port_index] == Models.SensorType.I2C)
            {
                outArray.AddRange(new byte[] { SpiAddress, (byte)message_type, 0, 0, 0, 0 });
                for (int b = 0; b < _i2cInBytes[port_index]; b++)
                    outArray.Add(0);

                reply = SpiTransferArray(outArray.ToArray());
                if (reply[3] == 0xA5)
                {
                    if ((reply[4] == (int)_sensorType[port_index]) && (reply[5] == (int)SensorState.ValidData) && ((reply.Length - 6) == _i2cInBytes[port_index]))
                    {
                        List<byte> values = new List<byte>();
                        for (int b = 6; b < _i2cInBytes[port_index]; b++)
                            values.Add(reply[b]);

                        return values.ToArray();
                    }
                    else
                        throw new IOException(SensorErrorInvalidData);
                }
                else
                    throw new IOException(IOErrorMessage);
            }
            else if ((_sensorType[port_index] == Models.SensorType.Touch)
                || (_sensorType[port_index] == Models.SensorType.NXTTouch)
                || (_sensorType[port_index] == Models.SensorType.EV3Touch)
                || (_sensorType[port_index] == Models.SensorType.NXTUltrasonic)
                || (_sensorType[port_index] == Models.SensorType.EV3ColorReflected)
                || (_sensorType[port_index] == Models.SensorType.EV3ColorAmbient)
                || (_sensorType[port_index] == Models.SensorType.EV3ColorColor)
                || (_sensorType[port_index] == Models.SensorType.EV3UltrasonicListen)
                || (_sensorType[port_index] == Models.SensorType.EV3InfraredProximity))
            {
                outArray.AddRange(new byte[] { SpiAddress, (byte)message_type, 0, 0, 0, 0, 0 });
                reply = SpiTransferArray(outArray.ToArray());
                if (reply[3] == 0xA5)
                {                    
                    if (((reply[4] == (int)_sensorType[port_index]) || ((_sensorType[port_index] == Models.SensorType.Touch) && ((reply[4] == (int)Models.SensorType.NXTTouch)
                        || (reply[4] == (int)Models.SensorType.EV3Touch)))) && (reply[5] == (int)SensorState.ValidData))
                        return new byte[] { reply[6] };
                    else
                        throw new IOException(SensorErrorInvalidData);
                }
                else
                    throw new IOException(IOErrorMessage);
            }
            else if (_sensorType[port_index] == Models.SensorType.NXTColorFull)
            {
                outArray.AddRange(new byte[] { SpiAddress, (byte)message_type, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });
                reply = SpiTransferArray(outArray.ToArray());
                if (reply[3] == 0xA5)
                {
                    if ((reply[4] == (int)_sensorType[port_index]) && (reply[5] == (int)SensorState.ValidData))

                        return new byte[] { reply[6], (byte)((reply[7] << 2) | ((reply[11] >> 6) & 0x03)), (byte)((reply[8] << 2) | ((reply[11] >> 4) & 0x03)),
                            (byte)((reply[9] << 2) | ((reply[11] >> 2) & 0x03)), (byte)((reply[10] << 2) | (reply[11] & 0x03)) };
                    else
                        throw new IOException(SensorErrorInvalidData);
                }
                else
                    throw new IOException(IOErrorMessage);
            }
            else if ((_sensorType[port_index] == SensorType.NXTLightOn)
              || (_sensorType[port_index] == SensorType.NXTLightOff)
              || (_sensorType[port_index] == SensorType.NXTColorRed)
              || (_sensorType[port_index] == SensorType.NXTColorGreen)
              || (_sensorType[port_index] == SensorType.NXTColorBlue)
              || (_sensorType[port_index] == SensorType.NXTColorOff)
              || (_sensorType[port_index] == SensorType.EV3GyroAbs)
              || (_sensorType[port_index] == SensorType.EV3GyroDps)
              || (_sensorType[port_index] == SensorType.EV3UltrasonicCentimeter)
              || (_sensorType[port_index] == SensorType.EV3UltrasonicInches))
            {
                outArray.AddRange(new byte[] { SpiAddress, (byte)message_type, 0, 0, 0, 0, 0, 0 });
                reply = SpiTransferArray(outArray.ToArray());
                if (reply[3] == 0xA5)
                {
                    if ((reply[4] == (int)_sensorType[port_index]) && (reply[5] == (int)SensorState.ValidData))
                    {
                        var value = (int)((reply[6] << 8) | reply[7]);
                        if (((_sensorType[port_index] == SensorType.EV3GyroAbs)
                        || (_sensorType[port_index] == SensorType.EV3GyroDps))
                        && ((value & 0x8000) > 0))
                            value = value - 0x10000;
                        else if ((_sensorType[port_index] == SensorType.EV3UltrasonicCentimeter)
                          || (_sensorType[port_index] == SensorType.EV3UltrasonicInches))
                            value = value / 10;
                        // convert back the value to a byte array
                        return new byte[] { (byte)((value >> 8) & 0xFF), (byte)(value & 0xFF) };
                    }
                    else
                        throw new IOException(SensorErrorInvalidData);
                }
                else
                    throw new IOException(IOErrorMessage);
            }
            else if ((_sensorType[port_index] == SensorType.EV3ColorRawReflected)
              || (_sensorType[port_index] == SensorType.EV3GyroAbsDps))
            {
                outArray.AddRange(new byte[] { SpiAddress, (byte)message_type, 0, 0, 0, 0, 0, 0, 0, 0 });
                reply = SpiTransferArray(outArray.ToArray());
                if (reply[3] == 0xA5)
                {
                    if ((reply[4] == (int)_sensorType[port_index]) && (reply[5] == (int)SensorState.ValidData))
                    {
                        ushort[] results = new ushort[] { (ushort)((reply[6] << 8) | reply[7]), (ushort)((reply[8] << 8) | reply[9]) };
                        if (_sensorType[port_index] == SensorType.EV3GyroAbsDps)                            
                            for (int r = 0; r < results.Length; r++)
                                if (results[r] >= 0x8000)
                                    results[r] = (ushort)(results[r] - 0x10000);
                        
                        // convert back the value to a byte array
                        // we know the length is 2
                        return new byte[] { (byte)((results[1] >> 8) & 0xFF), (byte)(results[1] & 0xFF), (byte)((results[0] >> 8) & 0xFF), (byte)(results[0] & 0xFF) };
                    }
                    else
                        throw new IOException(SensorErrorInvalidData);
                }
                else
                    throw new IOException(IOErrorMessage);
            }
            else if ((_sensorType[port_index] == SensorType.EV3ColorColorComponents))
            {
                outArray.AddRange(new byte[] { SpiAddress, (byte)message_type, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });
                reply = SpiTransferArray(outArray.ToArray());
                if (reply[3] == 0xA5)
                {
                    if ((reply[4] == (int)_sensorType[port_index]) || (reply[5] == (int)SensorState.ValidData))

                        return new byte[] { reply[6], reply[7], reply[8], reply[9], reply[10], reply[11], reply[12], reply[13] };
                    else

                        throw new IOException(SensorErrorInvalidData);
                }
                else
                    throw new IOException(IOErrorMessage);
            }
            else if (_sensorType[port_index] == SensorType.EV3InfraredSeek)
            {
                outArray.AddRange(new byte[] { SpiAddress, (byte)message_type, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });

                reply = SpiTransferArray(outArray.ToArray());

                if (reply[3] == 0xA5)
                {
                    if ((reply[4] == (int)_sensorType[port_index]) && (reply[5] == (int)SensorState.ValidData))

                    {
                        return new byte[] { (reply[6]), (reply[7]), (reply[8]), (reply[9]), (reply[10]), (reply[11]), (reply[12]), (reply[13]) };
                    }
                    else
                        throw new IOException(SensorErrorInvalidData);
                }
                else
                    throw new IOException(IOErrorMessage);
            }
            else if (_sensorType[port_index] == Models.SensorType.EV3InfraredRemote)
            {
                outArray.AddRange(new byte[] { SpiAddress, (byte)message_type, 0, 0, 0, 0, 0, 0, 0, 0 });
                reply = SpiTransferArray(outArray.ToArray());
                if (reply[3] == 0xA5)
                {
                    if ((reply[4] == (int)_sensorType[port_index]) && (reply[5] == (int)SensorState.ValidData))
                    {
                        byte[] results = new byte[] { 0, 0, 0, 0 };
                        for (int r = 0; r < results.Length; r++)
                        {
                            var value = reply[6 + r];
                            if (value == 1)
                                results[r] = 0b10000;
                            else if (value == 2)
                                results[r] = 0b01000;
                            else if (value == 3)
                                results[r] = 0b00100;
                            else if (value == 4)
                                results[r] = 0b00010;
                            else if (value == 5)
                                results[r] = 0b10100;
                            else if (value == 6)
                                results[r] = 0b10010;
                            else if (value == 7)
                                results[r] = 0b01100;
                            else if (value == 8)
                                results[r] = 0b01010;
                            else if (value == 9)
                                results[r] = 0b00001;
                            else if (value == 10)
                                results[r] = 0b11000;
                            else if (value == 11)
                                results[r] = 0b00110;
                            else
                                results[r] = 0b00000;
                        }
                        return results;
                    }
                    else
                        throw new IOException(SensorErrorInvalidData);
                }
                else
                    throw new IOException(IOErrorMessage);
            }
            throw new IOException($"{nameof(GetSensor)}  error: Sensor not configured or not supported.");
        }

        /// <summary>
        /// Set the sensor type
        /// </summary>
        /// <param name="port">The sensor port(s). Port 1, 2, 3, and/or 4</param>
        /// <param name="type">The sensor type</param>
        /// <param name="param">param is used only for some sensors and can be ignore for the others
        /// params is used for the following sensor types:
        ///   CUSTOM-- a 16 - bit integer used to configure the hardware.
        ///   I2C-- a list of settings:
        ///     params[0]-- Settings / flags
        ///     params[1] -- target Speed in microseconds(0-255). Realistically the speed will vary.
        ///     if SENSOR_I2C_SETTINGS_SAME flag set in I2C Settings:
        ///        params[2] -- Delay in microseconds between transactions.
        ///        params[3] -- Address
        ///        params[4] -- List of bytes to write
        ///        params[5] -- Number of bytes to read
        /// </param>
        public void SetSensorType(byte port, SensorType type, int[] param = null)
        {
            for (int p = 0; p < 4; p++)
            {
                if ((port & (1 << p)) > 0)
                    _sensorType[p] = type;
            }

            List<byte> outArray = new List<byte>();
            if (type == Models.SensorType.Custom)
            {
                outArray.AddRange(new byte[] { SpiAddress, (byte)SpiMessageType.SetSensorType, port, (byte)type, (byte)((param[0] >> 8) & 0xFF), (byte)(param[0] & 0xFF) });
            }
            else if (type == Models.SensorType.I2C)
            {
                if (param.Length >= 2)
                {
                    outArray.AddRange(new byte[] { SpiAddress, (byte)SpiMessageType.SetSensorType, port, (byte)type, (byte)param[0], (byte)param[1] });  //# Settings, SpeedUS

                    if ((param[0] == (int)SensorI2CSettings.Same) && (param.Length >= 6))
                    {
                        // Delay in micro second
                        outArray.Add((byte)((param[2] >> 24) & 0xFF));
                        outArray.Add((byte)((param[2] >> 16) & 0xFF));
                        outArray.Add((byte)((param[2] >> 8) & 0xFF));
                        outArray.Add((byte)(param[2] & 0xFF));
                        // Address
                        outArray.Add((byte)(param[3] & 0xFF));
                        // In bytes
                        outArray.Add((byte)(param[5] & 0xFF));

                        for (int p = 0; p < 4; p++)
                        {
                            if ((port & (1 << p)) > 0)
                                _i2cInBytes[p] = (byte)(param[5] & 0xFF);
                        }
                        // TODO: need to test more this part
                        outArray.Add((byte)param[4]);
                        // outArray.Add((byte)param[4]);         
                    }
                }
            }
            else
                outArray.AddRange(new byte[] { SpiAddress, (byte)SpiMessageType.SetSensorType, port, (byte)type });

            SpiTransferArray(outArray.ToArray());
        }

        #endregion

        #region Motors

        /// <summary>
        /// Set the motor power in percent
        /// </summary>
        /// <param name="port">The Motor port(s). PortA, PortB, PortC, and/or PortD.</param>
        /// <param name="powerPercent">The power from - 100 to 100, or -128 for float</param>
        public void SetMotorPower(byte port, int powerPercent)
        {
            powerPercent = powerPercent > 127 ? (byte)127 : powerPercent;
            powerPercent = powerPercent < -128 ? -128 : powerPercent;
            byte bPower = (byte)(powerPercent & 0xFF);
            byte[] outArray = { SpiAddress, (byte)SpiMessageType.SetMotorPower, port, bPower };
            var ret = SpiTransferArray(outArray);
        }

        /// <summary>
        /// Set the motor target position in degrees
        /// </summary>
        /// <param name="port">The Motor port(s). PortA, PortB, PortC, and/or PortD.</param>
        /// <param name="positionDegree">The target position in degree</param>
        public void SetMotorPosition(byte port, int positionDegree)
        {
            byte[] outArray = { SpiAddress, (byte)SpiMessageType.SetMotorPosition, port, (byte)((positionDegree >> 24) & 0xFF), (byte)((positionDegree >> 16) & 0xFF), (byte)((positionDegree >> 8) & 0xFF), (byte)(positionDegree & 0xFF) };
            var ret = SpiTransferArray(outArray);
        }

        /// <summary>
        /// Set the motor target position KP constant
        /// If you set KP higher, the motor will be more responsive to errors in position, at the cost of perhaps overshooting and oscillating.
        /// KP slows down the motor as it approaches the target, and helps to prevent overshoot.
        /// In general, if you increase KP, you should also increase KD to keep the motor from overshooting and oscillating.
        /// </summary>
        /// <param name="port">The Motor port(s). PortA, PortB, PortC, and/or PortD.</param>
        /// <param name="kp">The KP constant (default 25)</param>
        public void SetMotorPositionKP(byte port, byte kp = 25)
        {
            byte[] outArray = { SpiAddress, (byte)SpiMessageType.SetMotorPositionKP, port, kp };
            var ret = SpiTransferArray(outArray);
        }

        /// <summary>
        /// Set the motor target position KD constant
        /// If you set KP higher, the motor will be more responsive to errors in position, at the cost of perhaps overshooting and oscillating.
        /// KD slows down the motor as it approaches the target, and helps to prevent overshoot.
        /// In general, if you increase kp, you should also increase KD to keep the motor from overshooting and oscillating.
        /// </summary>
        /// <param name="port">The Motor port(s). PortA, PortB, PortC, and/or PortD.</param>
        /// <param name="kd">The KD constant (default 70)</param>
        public void SetMotorPositionKD(byte port, byte kd = 70)
        {
            byte[] outArray = { SpiAddress, (byte)SpiMessageType.SetMotorPositionKD, port, kd };
            var ret = SpiTransferArray(outArray);
        }

        /// <summary>
        /// Set the motor target speed in degrees per second
        /// </summary>
        /// <param name="port">The Motor port(s). PortA, PortB, PortC, and/or PortD.</param>
        /// <param name="dps">The target speed in degrees per second</param>
        public void SetMotorDps(byte port, int dps)
        {
            byte[] outArray = { SpiAddress, (byte)SpiMessageType.setMotorDps, port, (byte)((dps >> 8) & 0xFF), (byte)(dps & 0xFF) };
            var ret = SpiTransferArray(outArray);
        }

        /// <summary>
        /// Set the motor speed limit
        /// </summary>
        /// <param name="port">The Motor port(s). PortA, PortB, PortC, and/or PortD.</param>
        /// <param name="powerPercent">The power limit in percent (0 to 100), with 0 being no limit (100)</param>
        /// <param name="dps">The speed limit in degrees per second, with 0 being no limit</param>
        public void SetMotorLimits(byte port, byte powerPercent = 0, int dps = 0)
        {
            byte[] outArray = { SpiAddress, (byte)SpiMessageType.setMotorLimits, port, powerPercent, (byte)((dps >> 8) & 0xFF), (byte)(dps & 0xFF) };
            var ret = SpiTransferArray(outArray);
        }

        /// <summary>
        /// Read a motor status
        /// </summary>
        /// <param name="port">The motor port (one at a time). PortA, PortB, PortC, or PortD.</param>
        /// <returns>MotorStatus containing the status of the motor</returns>
        public MotorStatus GetMotorStatus(byte port)
        {  
            MotorStatus motorStatus = new MotorStatus();
            SpiMessageType message_type = SpiMessageType.None;
            if (port == (byte)MotorPort.PortA)
                message_type = SpiMessageType.GetMotorAStatus;
            else if (port == (byte)MotorPort.PortB)
                message_type = SpiMessageType.GetMotorBStatus;
            else if (port == (byte)MotorPort.PortC)
                message_type = SpiMessageType.GetMotorCStatus;
            else if (port == (byte)MotorPort.PortD)
                message_type = SpiMessageType.GetMotorDStatus;
            else
            {
                throw new IOException($"{nameof(GetMotorStatus)} error. Must be one motor port at a time. PORT_A, PORT_B, PORT_C, or PORT_D.");
            }
            byte[] outArray = { SpiAddress, (byte)message_type, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            var reply = SpiTransferArray(outArray);
            if (reply[3] == 0xA5)
            {
                motorStatus.Speed = reply[5];
                if ((motorStatus.Speed & 0x80) > 0)
                    motorStatus.Speed = -motorStatus.Speed;
                motorStatus.Encoder = (reply[6] << 24) | (reply[7] << 16) | (reply[8] << 8) | reply[9];
                //negative should be managed
                //if ((motorStatus.Encoder & 0x80000000) > 0)
                //    motorStatus.Encoder = motorStatus.Encoder - 0x100000000;
                motorStatus.Dps = ((reply[10] << 8) | reply[11]);
                if ((motorStatus.Dps & 0x8000) > 0)
                    motorStatus.Dps = motorStatus.Dps - 0x10000;
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
        /// <param name="port">The Motor port(s). PortA, PortB, PortC, and/or PortD.</param>
        /// <param name="positionOffset">The encoder offset. Zero the encoder by offsetting it by the current position</param>
        public void OffsetMotorEncoder(byte port, int positionOffset)
        {
            byte[] outArray = new byte[] { SpiAddress, (byte)SpiMessageType.OffsetMotorEncoder, port, (byte)((positionOffset >> 24) & 0xFF), (byte)((positionOffset >> 16) & 0xFF), (byte)((positionOffset >> 8) & 0xFF), (byte)(positionOffset & 0xFF) };
            SpiTransferArray(outArray);
        }

        /// <summary>
        /// Read a motor encoder in degrees
        /// </summary>
        /// <param name="port">The motor port (one at a time). PortA, PortB, PortC, or PortD.</param>
        /// <returns>Returns the encoder position in degrees</returns>
        public int GetMotorEncoder(byte port)
        {
            SpiMessageType message_type;
            if (port == (byte)MotorPort.PortA)
                message_type = SpiMessageType.GetMotorAEncoder;
            else if (port == (byte)MotorPort.PortB)
                message_type = SpiMessageType.GetMotorBEncoder;
            else if (port == (byte)MotorPort.PortC)
                message_type = SpiMessageType.GetMotorCEncoder;
            else if (port == (byte)MotorPort.PortD)
                message_type = SpiMessageType.GetMotorDEncoder;
            else
                throw new IOException($"{nameof(GetMotorEncoder)} error. Must be one motor port at a time. PORT_A, PORT_B, PORT_C, or PORT_D.");

            var encoder = SpiRead32(message_type);
            if ((encoder & 0x80000000) > 0)
                encoder = (int)(encoder - 0x100000000);
            return encoder;
        }

        #endregion

    }
}
