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
        private const int ISE_TEMP_MEASURE_TIME = 750;
        private const int ISE_MV_MEASURE_TIME = 250;
        private const int ISE_COMMUNICATION_DELAY = 10;

        private const bool ISE_DUALPOINT_CONFIG_BIT = false;
        private const int ISE_TEMP_COMPENSATION_CONFIG_BIT = 1;

        private I2cDevice _device;

        private float _mV = 0;
        private Temperature _temp = new Temperature();

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

                byte retval = Read_byte((byte)Register.ISE_CONFIG_REGISTER);

                retval = (byte)Bit_set((int)retval, ISE_TEMP_COMPENSATION_CONFIG_BIT, _temperatureCompensation);

                Write_byte((byte)Register.ISE_CONFIG_REGISTER, retval);
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
        /// Measure mV
        /// </summary>
        /// <returns>mV read from ISE Probe Interface</returns>
        public ElectricPotential MeasuremV()
        {
            Send_command((byte)Command.ISE_MEASURE_MV);

            DelayHelper.DelayMilliseconds(ISE_MV_MEASURE_TIME, allowThreadYield: true);

            _mV = Read_register((byte)Register.ISE_MV_REGISTER);
            _mV = Convert.ToSingle(Math.Round(_mV, 2));

            return new ElectricPotential(_mV, UnitsNet.Units.ElectricPotentialUnit.Millivolt);
        }

        /// <summary>
        /// Measure temperature
        /// </summary>
        /// <returns>Temperature. Temperature range: -2 to 35 C. Temperature Precision: 0.05 C</returns>
        public Temperature MeasureTemp()
        {
            Send_command((byte)Command.ISE_MEASURE_TEMP);

            DelayHelper.DelayMilliseconds(ISE_TEMP_MEASURE_TIME, allowThreadYield: true);
            _temp = new Temperature(Read_register((byte)Register.ISE_TEMP_REGISTER), UnitsNet.Units.TemperatureUnit.DegreeCelsius);

            return _temp;
        }

        /// <summary>
        /// Set temperature
        /// </summary>
        /// <param name="temp">Temperature</param>
        public void SetTemp(Temperature temp)
        {
            Write_register((byte)Register.ISE_TEMP_REGISTER, Convert.ToSingle(temp.DegreesCelsius));
            _temp = temp;
        }

        /// <summary>
        /// Calibrates the probe using a single point using a mV value.
        /// </summary>
        /// <param name="solution">Value. Range: -1024 mV to 1024 mV</param>
        public void CalibrateSingle(ElectricPotential solution)
        {
            if (solution.Millivolts >= -1024 && solution.Millivolts <= 1024)
            {
                Write_register((byte)Register.ISE_SOLUTION_REGISTER, Convert.ToSingle(solution.Millivolts));
                Send_command((byte)Command.ISE_CALIBRATE_SINGLE);

                DelayHelper.DelayMilliseconds(ISE_MV_MEASURE_TIME, allowThreadYield: true);
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
                Write_register((byte)Register.ISE_SOLUTION_REGISTER, Convert.ToSingle(solution.Millivolts));
                Send_command((byte)Command.ISE_CALIBRATE_LOW);

                DelayHelper.DelayMilliseconds(ISE_MV_MEASURE_TIME, allowThreadYield: true);
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
                Write_register((byte)Register.ISE_SOLUTION_REGISTER, Convert.ToSingle(solution.Millivolts));
                Send_command((byte)Command.ISE_CALIBRATE_HIGH);

                DelayHelper.DelayMilliseconds(ISE_MV_MEASURE_TIME, allowThreadYield: true);
            }
        }

        /// <summary>
        /// Returns the firmware version of the device.
        /// </summary>
        /// <returns>Firmware version</returns>
        public Version GetVersion()
        {
            return new Version(Read_register((byte)Register.ISE_VERSION_REGISTER).ToString());
        }

        /// <summary>
        /// Dual point uses two measures for low and high points. It needs the measured value (reading value) and the known value (reference value). Calling calibrateProbeLow saves both the reading and reference value.
        /// When there are high and low calibration points, the device will automatically use them to adjust readings.To disable dual-point adjustment, call reset to remove all calibration data.
        /// </summary>
        /// <returns>The known value (reference value) for calibrate the high value</returns>
        public ElectricPotential GetCalibrateHighReference()
        {
            return new ElectricPotential(Read_register((byte)Register.ISE_CALIBRATE_REFHIGH_REGISTER), UnitsNet.Units.ElectricPotentialUnit.Millivolt);
        }

        /// <summary>
        /// Dual point uses two measures for low and high points. It needs the measured value (reading value) and the known value (reference value). Calling calibrateProbeLow saves both the reading and reference value.
        /// When there are high and low calibration points, the device will automatically use them to adjust readings.To disable dual-point adjustment, call reset to remove all calibration data.
        /// </summary>
        /// <returns>The known value (reference value) for calibrate the low value</returns>
        public ElectricPotential GetCalibrateLowReference()
        {
            return new ElectricPotential(Read_register((byte)Register.ISE_CALIBRATE_REFLOW_REGISTER), UnitsNet.Units.ElectricPotentialUnit.Millivolt);
        }

        /// <summary>
        /// Dual point uses two measures for low and high points. It needs the measured value (reading value) and the known value (reference value). Calling calibrateProbeLow saves both the reading and reference value.
        /// When there are high and low calibration points, the device will automatically use them to adjust readings.To disable dual-point adjustment, call reset to remove all calibration data.
        /// </summary>
        /// <returns>The measured value (reading value) for calibrate the high value</returns>
        public ElectricPotential GetCalibrateHighReading()
        {
            return new ElectricPotential(Read_register((byte)Register.ISE_CALIBRATE_READHIGH_REGISTER), UnitsNet.Units.ElectricPotentialUnit.Millivolt);

        }

        /// <summary>
        /// Dual point uses two measures for low and high points. It needs the measured value (reading value) and the known value (reference value). Calling calibrateProbeLow saves both the reading and reference value.
        /// When there are high and low calibration points, the device will automatically use them to adjust readings.To disable dual-point adjustment, call reset to remove all calibration data.
        /// </summary>
        /// <returns>The measured value (reading value) for calibrate the low value</returns>
        public ElectricPotential GetCalibrateLowReading()
        {
            return new ElectricPotential(Read_register((byte)Register.ISE_CALIBRATE_READLOW_REGISTER), UnitsNet.Units.ElectricPotentialUnit.Millivolt);
        }

        /// <summary>
        ///  Resets all the stored calibration information
        /// </summary>
        public void Reset()
        {
            Write_register((byte)Register.ISE_CALIBRATE_SINGLE_REGISTER, null);
            Write_register((byte)Register.ISE_CALIBRATE_REFHIGH_REGISTER, null);
            Write_register((byte)Register.ISE_CALIBRATE_REFLOW_REGISTER, null);
            Write_register((byte)Register.ISE_CALIBRATE_READHIGH_REGISTER, null);
            Write_register((byte)Register.ISE_CALIBRATE_READLOW_REGISTER, null);
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
                Write_register((byte)Register.ISE_CALIBRATE_REFLOW_REGISTER, Convert.ToSingle(refLow.Millivolts));
                Write_register((byte)Register.ISE_CALIBRATE_REFHIGH_REGISTER, Convert.ToSingle(refHigh));
                Write_register((byte)Register.ISE_CALIBRATE_READLOW_REGISTER, Convert.ToSingle(readLow));
                Write_register((byte)Register.ISE_CALIBRATE_READHIGH_REGISTER, Convert.ToSingle(readHigh));
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
                Write_register((byte)Register.ISE_SOLUTION_REGISTER, Convert.ToByte(i2cAddress));
                Send_command((byte)Command.ISE_I2C);
            }
        }

        /// <summary>
        /// Low level reads of EEPROM
        /// </summary>
        /// <param name="address">Address to read</param>
        /// <returns>Low level data from EEPROM</returns>
        public float ReadEEPROM(float address)
        {
            if (address < 0xff)
            {
                Write_register((byte)Register.ISE_SOLUTION_REGISTER, address);
                Send_command((byte)Command.ISE_MEMORY_READ);
                return Read_register((byte)Register.ISE_BUFFER_REGISTER);
            }

            return 0;
        }

        /// <summary>
        /// Low level write of EEPROM
        /// </summary>
        /// <param name="address">Address</param>
        /// <param name="data">Data to write to the address</param>
        public void WriteEEPROM(float address, float data)
        {
            if (address < 0xff)
            {
                Write_register((byte)Register.ISE_SOLUTION_REGISTER, address);
                Write_register((byte)Register.ISE_BUFFER_REGISTER, data);
                Send_command((byte)Command.ISE_MEMORY_WRITE);
            }
        }

        /// <summary>
        /// Get Firmware version
        /// </summary>
        /// <returns>The version of the firmware</returns>
        public Version GetFirmwareVersion()
        {
            return new Version(Read_byte((byte)Register.ISE_FW_VERSION_REGISTER).ToString());

        }

        private void Send_command(byte data)
        {
            Span<byte> bytes = stackalloc byte[2];
            bytes[0] = (byte)Register.ISE_TASK_REGISTER;
            bytes[1] = data;
            _device.Write(bytes);
            DelayHelper.DelayMilliseconds(ISE_COMMUNICATION_DELAY, allowThreadYield: true);
        }

        private float Read_register(byte register)
        {
            byte[] data = { 0, 0, 0, 0 };

            Change_register(register);

            data[0] = _device.ReadByte();
            data[1] = _device.ReadByte();
            data[2] = _device.ReadByte();
            data[3] = _device.ReadByte();

            return Convert.ToSingle(Round_total_digits(BitConverter.ToSingle(data, 0), 7));
        }

        private void Write_register(byte register, float? data)
        {
            byte[] dataArray = { 0, 0, 0, 0 };

            if (data != null)
            {
                dataArray = BitConverter.GetBytes(Round_total_digits(data.Value));
            }

            Span<byte> bytes = stackalloc byte[4];
            bytes[0] = dataArray[0];
            bytes[1] = dataArray[1];
            bytes[2] = dataArray[2];
            bytes[3] = dataArray[3];

            Change_register(register);
            _device.Write(bytes);
            DelayHelper.DelayMilliseconds(ISE_COMMUNICATION_DELAY, allowThreadYield: true);
        }

        private void Change_register(byte register)
        {
            _device.WriteByte(register);
            DelayHelper.DelayMilliseconds(ISE_COMMUNICATION_DELAY, allowThreadYield: true);
        }

        private byte Read_byte(byte register)
        {
            Change_register(register);
            DelayHelper.DelayMilliseconds(ISE_COMMUNICATION_DELAY, allowThreadYield: true);

            return _device.ReadByte();
        }

        private void Write_byte(byte register, byte value)
        {
            Change_register(register);

            _device.WriteByte(value);

            DelayHelper.DelayMilliseconds(ISE_COMMUNICATION_DELAY, allowThreadYield: true);

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

        private float Round_total_digits(float x, int? digits = 7)
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
