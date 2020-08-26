// Licensed to the.NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device;
using System.Device.I2c;
using UnitsNet;

namespace Iot.Device.UFire
{
    /// <summary>
    /// μFire ISE (Ion Specific Electrode) Probe Interface controller
    /// </summary>
    public class UFireIse : IDisposable
    {
        /// <summary>
        /// 0x3F is the default address of all sensors
        /// </summary>
        public static byte I2cAddress = 0x3F;

        // A temperature measurement takes 750ms, see https://www.ufire.co/docs/uFire_ISE/#use
        private const int IseTemperatureMeasureTime = 750;
        // A mV measurement takes 750 ms, see https://www.ufire.co/docs/uFire_ISE/#use
        private const int IseMvMeasureTime = 750;

        private const int IseCommunicationDelay = 10;

        private const bool IseDualPointConfigBit = false;
        private const int IseTemperatureCompensationConfigBit = 1;

        private I2cDevice _device;

        private float _measurement = 0;
        private Temperature _temperature = new Temperature();

        private bool _temperatureCompensation = false;

        /// <summary>
        /// Use temperature compensation
        /// </summary>
        public bool TemperatureCompensation
        {
            get
            {
                return _temperatureCompensation;
            }

            set
            {
                _temperatureCompensation = value;

                byte retval = ReadByte((byte)Register.ISE_CONFIG_REGISTER);

                retval = (byte)Bit_set((int)retval, IseTemperatureCompensationConfigBit, _temperatureCompensation);

                WriteByte((byte)Register.ISE_CONFIG_REGISTER, retval);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UFireIse"/> class.
        /// </summary>
        /// <param name="i2cDevice">The I2C device to be used</param>
        public UFireIse(I2cDevice i2cDevice)
        {
            _device = i2cDevice;
        }

        /// <summary>
        /// Measure a value from the ISE Probe Interface, typical measure are in the millivolt range.
        /// </summary>
        /// <returns>value from ISE Probe Interface, typical measure are in the millivolt range. On error it return -1 as value</returns>
        public ElectricPotential Measure()
        {
            SendCommand((byte)Command.ISE_MEASURE_MV);

            DelayHelper.DelayMilliseconds(IseMvMeasureTime, allowThreadYield: true);

            _measurement = ReadRegister((byte)Register.ISE_MV_REGISTER);
            _measurement = Convert.ToSingle(Math.Round(_measurement, 2));

            return new ElectricPotential(_measurement, UnitsNet.Units.ElectricPotentialUnit.Millivolt);
        }

        /// <summary>
        /// Measure temperature
        /// </summary>
        /// <returns>Temperature. Temperature range: -2 to 35 C. Temperature Precision: 0.05 C</returns>
        public Temperature MeasureTemperature()
        {
            SendCommand((byte)Command.ISE_MEASURE_TEMP);

            DelayHelper.DelayMilliseconds(IseTemperatureMeasureTime, allowThreadYield: true);
            _temperature = new Temperature(ReadRegister((byte)Register.ISE_TEMP_REGISTER), UnitsNet.Units.TemperatureUnit.DegreeCelsius);

            return _temperature;
        }

        /// <summary>
        /// Set temperature compensation
        /// </summary>
        /// <param name="temp">Compensation temperature</param>
        public void SetTemperatureCompensation(Temperature temp)
        {
            WriteRegister((byte)Register.ISE_TEMP_REGISTER, Convert.ToSingle(temp.DegreesCelsius));
            _temperature = temp;
        }

        /// <summary>
        /// Calibrates the probe using a single point using a mV value.
        /// </summary>
        /// <param name="solution">Value. Range: -1024 mV to 1024 mV</param>
        public void CalibrateSingle(ElectricPotential solution)
        {
            if (solution.Millivolts >= -1024 && solution.Millivolts <= 1024)
            {
                WriteRegister((byte)Register.ISE_SOLUTION_REGISTER, Convert.ToSingle(solution.Millivolts));
                SendCommand((byte)Command.ISE_CALIBRATE_SINGLE);

                DelayHelper.DelayMilliseconds(IseMvMeasureTime, allowThreadYield: true);
            }
        }

        /// <summary>
        /// Calibrates the dual-point values for the low reading, in mV, and saves them in the devices's EEPROM.
        /// </summary>
        /// <param name="solution">Value. Range: -1024 mV to 1024 mV</param>
        public void CalibrateProbeLow(ElectricPotential solution)
        {
            if (solution.Millivolts >= -1024 && solution.Millivolts <= 1024)
            {
                WriteRegister((byte)Register.ISE_SOLUTION_REGISTER, Convert.ToSingle(solution.Millivolts));
                SendCommand((byte)Command.ISE_CALIBRATE_LOW);

                DelayHelper.DelayMilliseconds(IseMvMeasureTime, allowThreadYield: true);
            }
        }

        /// <summary>
        /// Calibrates the dual-point values for the high reading, in mV, and saves them in the devices's EEPROM.
        /// </summary>
        /// <param name="solution">Value. Range: -1024 mV to 1024 mV</param>
        public void CalibrateProbeHigh(ElectricPotential solution)
        {
            if (solution.Millivolts >= -1024 && solution.Millivolts <= 1024)
            {
                WriteRegister((byte)Register.ISE_SOLUTION_REGISTER, Convert.ToSingle(solution.Millivolts));
                SendCommand((byte)Command.ISE_CALIBRATE_HIGH);

                DelayHelper.DelayMilliseconds(IseMvMeasureTime, allowThreadYield: true);
            }
        }

        /// <summary>
        /// Returns the firmware version of the device.
        /// </summary>
        /// <returns>Firmware version</returns>
        public Version GetVersion()
        {
            return new Version(ReadRegister((byte)Register.ISE_VERSION_REGISTER).ToString());
        }

        /// <summary>
        /// Dual point uses two measures for low and high points. It needs the measured value (reading value) and the known value (reference value). Calling calibrateProbeLow saves both the reading and reference value.
        /// When there are high and low calibration points, the device will automatically use them to adjust readings.To disable dual-point adjustment, call reset to remove all calibration data.
        /// </summary>
        /// <returns>The known value (reference value) for calibrate the high value</returns>
        public ElectricPotential GetCalibrateHighReference()
        {
            return new ElectricPotential(ReadRegister((byte)Register.ISE_CALIBRATE_REFHIGH_REGISTER), UnitsNet.Units.ElectricPotentialUnit.Millivolt);
        }

        /// <summary>
        /// Dual point uses two measures for low and high points. It needs the measured value (reading value) and the known value (reference value). Calling calibrateProbeLow saves both the reading and reference value.
        /// When there are high and low calibration points, the device will automatically use them to adjust readings.To disable dual-point adjustment, call reset to remove all calibration data.
        /// </summary>
        /// <returns>The known value (reference value) for calibrate the low value</returns>
        public ElectricPotential GetCalibrateLowReference()
        {
            return new ElectricPotential(ReadRegister((byte)Register.ISE_CALIBRATE_REFLOW_REGISTER), UnitsNet.Units.ElectricPotentialUnit.Millivolt);
        }

        /// <summary>
        /// Dual point uses two measures for low and high points. It needs the measured value (reading value) and the known value (reference value). Calling calibrateProbeLow saves both the reading and reference value.
        /// When there are high and low calibration points, the device will automatically use them to adjust readings.To disable dual-point adjustment, call reset to remove all calibration data.
        /// </summary>
        /// <returns>The measured value (reading value) for calibrate the high value</returns>
        public ElectricPotential GetCalibrateHighReading()
        {
            return new ElectricPotential(ReadRegister((byte)Register.ISE_CALIBRATE_READHIGH_REGISTER), UnitsNet.Units.ElectricPotentialUnit.Millivolt);

        }

        /// <summary>
        /// Dual point uses two measures for low and high points. It needs the measured value (reading value) and the known value (reference value). Calling calibrateProbeLow saves both the reading and reference value.
        /// When there are high and low calibration points, the device will automatically use them to adjust readings.To disable dual-point adjustment, call reset to remove all calibration data.
        /// </summary>
        /// <returns>The measured value (reading value) for calibrate the low value</returns>
        public ElectricPotential GetCalibrateLowReading()
        {
            return new ElectricPotential(ReadRegister((byte)Register.ISE_CALIBRATE_READLOW_REGISTER), UnitsNet.Units.ElectricPotentialUnit.Millivolt);
        }

        /// <summary>
        ///  Resets all the stored calibration information
        /// </summary>
        public void Reset()
        {
            WriteRegister((byte)Register.ISE_CALIBRATE_SINGLE_REGISTER, null);
            WriteRegister((byte)Register.ISE_CALIBRATE_REFHIGH_REGISTER, null);
            WriteRegister((byte)Register.ISE_CALIBRATE_REFLOW_REGISTER, null);
            WriteRegister((byte)Register.ISE_CALIBRATE_READHIGH_REGISTER, null);
            WriteRegister((byte)Register.ISE_CALIBRATE_READLOW_REGISTER, null);
        }

        /// <summary>
        /// Sets all the values, in mV, for dual point calibration and saves them in the devices's EEPROM
        /// </summary>
        /// <param name="refLow">the reference low point. Range: -1024 mV to 1024 mV</param>
        /// <param name="refHigh">the reference high point. Range: -1024 mV to 1024 mV</param>
        /// <param name="readLow">the measured low point. Range: -1024 mV to 1024 mV</param>
        /// <param name="readHigh">the measured high point. Range: -1024 mV to 1024 mV</param>
        public void SetDualPointCalibration(ElectricPotential refLow, ElectricPotential refHigh, ElectricPotential readLow, ElectricPotential readHigh)
        {
            if (refLow.Millivolts >= -1024 && refLow.Millivolts <= 1024 &&
                refHigh.Millivolts >= -1024 && refHigh.Millivolts <= 1024 &&
                readLow.Millivolts >= -1024 && readLow.Millivolts <= 1024 &&
                readHigh.Millivolts >= -1024 && readHigh.Millivolts <= 1024)
            {
                WriteRegister((byte)Register.ISE_CALIBRATE_REFLOW_REGISTER, Convert.ToSingle(refLow.Millivolts));
                WriteRegister((byte)Register.ISE_CALIBRATE_REFHIGH_REGISTER, Convert.ToSingle(refHigh));
                WriteRegister((byte)Register.ISE_CALIBRATE_READLOW_REGISTER, Convert.ToSingle(readLow));
                WriteRegister((byte)Register.ISE_CALIBRATE_READHIGH_REGISTER, Convert.ToSingle(readHigh));
            }
        }

        /// <summary>
        /// Changes the default I2C address
        /// </summary>
        /// <param name="i2cAddress">The new I2C address</param>
        public void SetI2CAddress(byte i2cAddress)
        {
            if (i2cAddress >= 1 && i2cAddress <= 127)
            {
                WriteRegister((byte)Register.ISE_SOLUTION_REGISTER, Convert.ToByte(i2cAddress));
                SendCommand((byte)Command.ISE_I2C);
            }
            else
            {
                throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Low level reads of EEPROM
        /// </summary>
        /// <param name="address">Address to read</param>
        /// <returns>Low level data from EEPROM</returns>
        public float ReadEeprom(byte address)
        {
            WriteRegister((byte)Register.ISE_SOLUTION_REGISTER, address);
            SendCommand((byte)Command.ISE_MEMORY_READ);

            return ReadRegister((byte)Register.ISE_BUFFER_REGISTER);
        }

        /// <summary>
        /// Low level write of EEPROM
        /// </summary>
        /// <param name="address">Address</param>
        /// <param name="data">Data to write to the address</param>
        public void WriteEeprom(byte address, float data)
        {
            WriteRegister((byte)Register.ISE_SOLUTION_REGISTER, address);
            WriteRegister((byte)Register.ISE_BUFFER_REGISTER, data);
            SendCommand((byte)Command.ISE_MEMORY_WRITE);
        }

        /// <summary>
        /// Get Firmware version
        /// </summary>
        /// <returns>The version of the firmware</returns>
        public Version GetFirmwareVersion()
        {
            return new Version(ReadByte((byte)Register.ISE_FW_VERSION_REGISTER).ToString());

        }

        private void SendCommand(byte data)
        {
            Span<byte> bytes = stackalloc byte[2];
            bytes[0] = (byte)Register.ISE_TASK_REGISTER;
            bytes[1] = data;
            _device.Write(bytes);
            DelayHelper.DelayMilliseconds(IseCommunicationDelay, allowThreadYield: true);
        }

        private float ReadRegister(byte register)
        {
            ChangeRegister(register);

            Span<byte> data = stackalloc byte[4]
            {
                0,
                0,
                0,
                0
            };
            _device.Read(data);

            return Convert.ToSingle(RoundTotalDigits(BitConverter.ToSingle(data.ToArray(), 0), 7));
        }

        private void WriteRegister(byte register, float? data)
        {
            Span<byte> bytes = stackalloc byte[4]
             {
                0,
                0,
                0,
                0
             };

            if (data != null)
            {
                bytes = BitConverter.GetBytes(RoundTotalDigits(data.Value));
            }

            ChangeRegister(register);
            _device.Write(bytes);
            DelayHelper.DelayMilliseconds(IseCommunicationDelay, allowThreadYield: true);
        }

        private void ChangeRegister(byte register)
        {
            _device.WriteByte(register);
            DelayHelper.DelayMilliseconds(IseCommunicationDelay, allowThreadYield: true);
        }

        private byte ReadByte(byte register)
        {
            ChangeRegister(register);
            DelayHelper.DelayMilliseconds(IseCommunicationDelay, allowThreadYield: true);

            return _device.ReadByte();
        }

        private void WriteByte(byte register, byte value)
        {
            ChangeRegister(register);

            _device.WriteByte(value);

            DelayHelper.DelayMilliseconds(IseCommunicationDelay, allowThreadYield: true);

        }

        private int Bit_set(int v, int index, bool x)
        {
            int mask = 1 << index;
            v &= ~mask;
            if (x)
            {
                v |= mask;
            }

            return v;
        }

        private int Magnitude(float x)
        {
            return float.IsNaN(x) || x == 0 ? 0 : (int)Math.Floor(Math.Log10(Math.Abs(x))) + 1;
        }

        private float RoundTotalDigits(float x, int? digits = 7)
        {
            int numberOfDigits = digits.Value - Magnitude(x);
            if (numberOfDigits > 15)
            {
                numberOfDigits = 15;

            }

            return (float)Math.Round(x, numberOfDigits);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _device?.Dispose();
            _device = null;
        }
    }
}
