// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Device;
using System.Device.Gpio;
using System.Device.I2c;
using System.Threading;
using Iot.Units;
using Sht1x;

namespace Iot.Device.Sht3x
{
    /// <summary>
    /// SHT1x Humidity and Temperature Sensor
    /// </summary>
    public class Sht1x : IDisposable
    {
        private readonly byte[] _crcLookup =
        {
               0, 49, 98, 83, 196, 245, 166, 151, 185, 136, 219, 234, 125, 76, 31, 46, 67, 114, 33, 16, 135,
               182, 229, 212, 250, 203, 152, 169, 62, 15, 92, 109, 134, 183, 228, 213, 66, 115, 32, 17, 63,
               14, 93, 108, 251, 202, 153, 168, 197, 244, 167, 150, 1, 48, 99, 82, 124, 77, 30, 47, 184, 137,
               218, 235, 61, 12, 95, 110, 249, 200, 155, 170, 132, 181, 230, 215, 64, 113, 34, 19, 126, 79,
               28, 45, 186, 139, 216, 233, 199, 246, 165, 148, 3, 50, 97, 80, 187, 138, 217, 232, 127, 78, 29,
               44, 2, 51, 96, 81, 198, 247, 164, 149, 248, 201, 154, 171, 60, 13, 94, 111, 65, 112, 35, 18,
               133, 180, 231, 214, 122, 75, 24, 41, 190, 143, 220, 237, 195, 242, 161, 144, 7, 54, 101, 84,
               57, 8, 91, 106, 253, 204, 159, 174, 128, 177, 226, 211, 68, 117, 38, 23, 252, 205, 158, 175,
               56, 9, 90, 107, 69, 116, 39, 22, 129, 176, 227, 210, 191, 142, 221, 236, 123, 74, 25, 40, 6,
               55, 100, 85, 194, 243, 160, 145, 71, 118, 37, 20, 131, 178, 225, 208, 254, 207, 156, 173, 58,
               11, 88, 105, 4, 53, 102, 87, 192, 241, 162, 147, 189, 140, 223, 238, 121, 72, 27, 42, 193, 240,
               163, 146, 5, 52, 103, 86, 120, 73, 26, 43, 188, 141, 222, 239, 130, 179, 224, 209, 70, 119, 36,
               21, 59, 10, 89, 104, 255, 206, 157, 172
        };

        private readonly Dictionary<SuppliedVoltage, double> _temperatureConversionTable = new Dictionary<SuppliedVoltage, double>
        {
            { SuppliedVoltage.V_5, -40.1 },
            { SuppliedVoltage.V_4, -39.8 },
            { SuppliedVoltage.V_3_5, -39.7 },
            { SuppliedVoltage.V_3, -39.6 },
            { SuppliedVoltage.V_2_5, -39.4 }
        };

        #region prop

        /// <summary>
        /// Perform CRC ckecks on data
        /// </summary>
        public bool CrcCheck { get; set; } = true;

        private Temperature _temperature;

        /// <summary>
        /// Temperature
        /// </summary>
        public Temperature Temperature
        {
            get
            {
                ReadTemperature();
                return _temperature;
            }
        }

        private double _humidity;

        /// <summary>
        /// Relative Humidity (%)
        /// </summary>
        public double Humidity
        {
            get
            {
                ReadHumidity();
                return _humidity;
            }
        }

        private byte _statusRegister;

        private int _clockPin;
        private int _dataPin;

        private SuppliedVoltage _supplyVoltage;
        private Resolution _resolution;

        private GpioController _controller;

        #endregion

        /// <summary>
        /// Creates a new instance of the SHT1x sensor
        /// </summary>
        public Sht1x(int clockPin, int dataPin, Resolution resolution = Resolution.High, SuppliedVoltage voltage = SuppliedVoltage.V_3_5, GpioController controller = null)
        {
            _clockPin = clockPin;
            _dataPin = dataPin;
            _controller = controller ?? new GpioController();
            _resolution = resolution;
            _supplyVoltage = voltage;

            Reset();
        }

        /// <summary>
        /// Cleanup
        /// </summary>
        public void Dispose()
        {
            _controller?.Dispose();
            _controller = null;
        }

        private void Reset()
        {
            _controller.SetPinMode(_dataPin, PinMode.Output);
            _controller.SetPinMode(_clockPin, PinMode.Output);

            TogglePin(_dataPin, PinValue.High);

            // See 3.4 of datasheet
            for (int i = 0; i <= 10; ++i)
            {
                TogglePin(_clockPin, PinValue.High);
                TogglePin(_clockPin, PinValue.Low);
            }
        }

        private void TogglePin(int pin, PinValue state)
        {
            _controller.Write(pin, state);
            if (pin == _clockPin)
            {
                // Technically we need to delay for 100 nanoseconds
                DelayHelper.DelayMicroseconds(1, false);
            }
        }

        /// <summary>
        /// Performs a soft reset of the sensor
        /// </summary>
        public void SoftReset()
        {
            SendCommand(Command.SoftReset, measurement: false);
            DelayHelper.DelayMilliseconds(15, true);
            _statusRegister = 0;
        }

        private void SendCommand(Command command, bool measurement = true)
        {
            TransmissionStart();
            SendByte((byte)command);
            GetAck();

            if (measurement)
            {
                PinValue ack = _controller.Read(_dataPin);
                if (ack == PinValue.Low)
                {
                    throw new Exception("The sensor failed to change into the proper measurement state.");
                }
            }
        }

        private void SendByte(byte data)
        {
            _controller.SetPinMode(_dataPin, PinMode.Output);
            _controller.SetPinMode(_clockPin, PinMode.Output);

            for (int i = 0; i < 8; ++i)
            {
                TogglePin(_dataPin, data & (1 << (7 - i)));
                TogglePin(_clockPin, PinValue.High);
                TogglePin(_clockPin, PinValue.Low);
            }
        }

        /// <summary>
        /// Sends the transmission start sequence to the sensor.
        /// </summary>
        private void TransmissionStart()
        {
            _controller.SetPinMode(_dataPin, PinMode.Output);
            _controller.SetPinMode(_clockPin, PinMode.Output);

            TogglePin(_dataPin,  PinValue.High);
            TogglePin(_clockPin, PinValue.High);
            TogglePin(_dataPin,  PinValue.Low);
            TogglePin(_clockPin, PinValue.Low);
            TogglePin(_clockPin, PinValue.High);
            TogglePin(_dataPin,  PinValue.High);
            TogglePin(_clockPin, PinValue.Low);
        }

        /// <summary>
        /// Sends skip ACK by keeping the DATA line high to bypass CRC and end transmission.
        /// </summary>
        private void TransmissionEnd()
        {
            _controller.SetPinMode(_dataPin, PinMode.Output);
            _controller.SetPinMode(_clockPin, PinMode.Output);

            TogglePin(_dataPin, PinValue.High);
            TogglePin(_clockPin, PinValue.High);
            TogglePin(_clockPin, PinValue.Low);
        }

        private void GetAck()
        {
            _controller.SetPinMode(_dataPin, PinMode.Input);
            _controller.SetPinMode(_clockPin, PinMode.Output);

            TogglePin(_clockPin, PinValue.High);

            PinValue ack = _controller.Read(_dataPin);
            if (ack == PinValue.High)
            {
                throw new Exception("Failed to get ack for command.");
            }

            TogglePin(_clockPin, PinValue.Low);
        }

        private void SendAck()
        {
            _controller.SetPinMode(_dataPin, PinMode.Output);
            _controller.SetPinMode(_clockPin, PinMode.Output);

            TogglePin(_dataPin, PinValue.High);
            TogglePin(_dataPin, PinValue.Low);
            TogglePin(_clockPin, PinValue.High);
            TogglePin(_clockPin, PinValue.Low);
        }

        private void WaitForResult()
        {
            // We need to wait for 320ms, or less if the resolution is changed.
            for (int i = 0; i <= 35; ++i)
            {
                DelayHelper.DelayMilliseconds(10, true);
                PinValue status = _controller.Read(_dataPin);
                if (status == PinValue.Low)
                {
                    return;
                }
            }

            throw new Exception("Timed out waiting for sensor to respond with result.");
        }

        private byte GetByte()
        {
            _controller.SetPinMode(_dataPin, PinMode.Input);
            _controller.SetPinMode(_clockPin, PinMode.Output);

            byte data = 0;
            for (int i = 0; i < 8; ++i)
            {
                TogglePin(_clockPin, PinValue.High);
                data |= (byte)((byte)_controller.Read(_dataPin) << (7 - i));
                TogglePin(_clockPin, PinValue.Low);
            }

            return data;
        }

        private void ReadTemperature()
        {
            SendCommand(Command.Temperature);
            var rawValue = ReadMeasurement(Command.Temperature);
            _temperature = Temperature.FromCelsius((rawValue * (_resolution == Resolution.High ? 0.01 : 0.04)) + _temperatureConversionTable[_supplyVoltage]);
        }

        private void ReadHumidity()
        {
            if (_temperature == default)
            {
                ReadTemperature();
            }

            SendCommand(Command.Humidity);
            var rawValue = ReadMeasurement(Command.Humidity);
            var linearHumidity = -2.0468 + ((_resolution == Resolution.High ? 0.0367 : 0.5872) * rawValue) + ((_resolution == Resolution.High ? -0.0000015955 : -0.00040845) * Math.Pow(rawValue, 2));

            _humidity = Math.Round(((_temperature.Celsius - 25) * (0.01 + ((_resolution == Resolution.High ? 0.00008 : 0.00128) * rawValue))) + linearHumidity, 2);
        }

        private ushort ReadMeasurement(Command command)
        {
            ushort value = GetByte();
            value <<= 8;

            SendAck();
            value |= GetByte();

            if (CrcCheck)
            {
                ValidateCrc(command, value);
            }
            else
            {
                TransmissionEnd();
            }

            return value;
        }

        private void ValidateCrc(Command command, uint data, bool measurement = true)
        {
            SendAck();
            var crcValue = GetByte();
            TransmissionEnd();

            var crcStartValue = ReverseStatusRegister();
            var lookup = _crcLookup[crcStartValue ^ (byte)command];
            byte final;

            if (measurement)
            {
                lookup = _crcLookup[lookup ^ (data >> 8)];
                final = _crcLookup[lookup ^ (data & 0b0000000011111111)];
            }
            else
            {
                final = _crcLookup[lookup ^ data];
            }

            var crcFinalReversed = ReverseByte(final);
            if (crcValue != crcFinalReversed)
            {
                SoftReset();
                throw new Exception("CRC error occurred; sensor has been reset.");
            }
        }

        private byte ReverseStatusRegister()
        {
            return (byte)((ReverseByte(_statusRegister) >> 4) << 4);
        }

        /// <summary>
        /// Reverses the byte, using the method from Rich Schroeppel: http://graphics.stanford.edu/~seander/bithacks.html#ReverseByteWith64BitsDiv
        /// </summary>
        private byte ReverseByte(byte value) => (byte)(((value * 8623620610) & 1136090292240) % 1023);

    }
}
