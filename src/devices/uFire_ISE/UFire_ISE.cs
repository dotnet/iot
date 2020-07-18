// Licensed to the.NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device;
using System.Device.I2c;

namespace Iot.Device.UFire
{
    /// <summary>
    /// μFire ISE Probe Interface controller
    /// </summary>
    public class UFire_ISE : IDisposable
    {
        private const int ISE_TEMP_MEASURE_TIME = 750;
        private const int ISE_MV_MEASURE_TIME = 250;

        private const bool ISE_DUALPOINT_CONFIG_BIT = false;
        private const int ISE_TEMP_COMPENSATION_CONFIG_BIT = 1;

        private I2cDevice _device;

        private float _mV = 0;
        private float _tempC = 0;
        private float _tempF = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="UFire_ISE"/> class.
        /// </summary>
        /// <param name="i2cDevice">The I2C device to be used</param>
        public UFire_ISE(I2cDevice i2cDevice)
        {
            _device = i2cDevice;
        }

        /// <summary>
        /// Measure mV
        /// </summary>
        /// <returns>mV read from ISE Probe Interface</returns>
        public float MeasuremV()
        {
            Send_command((byte)Command.ISE_MEASURE_MV);

            DelayHelper.DelayMilliseconds(ISE_MV_MEASURE_TIME, allowThreadYield: true);

            _mV = Read_register((byte)Register.ISE_MV_REGISTER);
            _mV = Convert.ToSingle(Math.Round(_mV, 2));

            return _mV;
        }

        /// <summary>
        /// Measure temperature
        /// </summary>
        /// <returns>Temperature in celsius</returns>
        public float MeasureTemp()
        {
            Send_command((byte)Command.ISE_MEASURE_TEMP);

            DelayHelper.DelayMilliseconds(ISE_TEMP_MEASURE_TIME, allowThreadYield: true);
            _tempC = Read_register((byte)Register.ISE_TEMP_REGISTER);

            if (_tempC == -127.0)
            {
                _tempF = -127.0F;
            }
            else
            {
                _tempF = ((_tempC * 9) / 5) + 32;
            }

            return _tempC;
        }

        /// <summary>
        /// Set temperature
        /// </summary>
        /// <param name="tempC">Temperature in celsius</param>
        public void SetTemp(float tempC)
        {
            Write_register((byte)Register.ISE_TEMP_REGISTER, _tempC);
            _tempC = tempC;
            _tempF = ((_tempC * 9) / 5) + 32;
        }

        /// <summary>
        /// Calibrates the probe using a single point using a mV value.
        /// </summary>
        /// <param name="solutionmV">mV value</param>
        public void CalibrateSingle(float solutionmV)
        {
            Write_register((byte)Register.ISE_SOLUTION_REGISTER, solutionmV);
            Send_command((byte)Command.ISE_CALIBRATE_SINGLE);

            DelayHelper.DelayMilliseconds(ISE_MV_MEASURE_TIME, allowThreadYield: true);
        }

        /// <summary>
        /// Calibrates the dual-point values for the low reading, in mV, and saves them in the devices's EEPROM.
        /// </summary>
        /// <param name="solutionmV">mV value</param>
        public void CalibrateProbeLow(float solutionmV)
        {
            Write_register((byte)Register.ISE_SOLUTION_REGISTER, solutionmV);
            Send_command((byte)Command.ISE_CALIBRATE_LOW);

            DelayHelper.DelayMilliseconds(ISE_MV_MEASURE_TIME, allowThreadYield: true);
        }

        /// <summary>
        /// Calibrates the dual-point values for the high reading, in mV, and saves them in the devices's EEPROM.
        /// </summary>
        /// <param name="solutionmV">mV value</param>
        public void CalibrateProbeHigh(float solutionmV)
        {
            Write_register((byte)Register.ISE_SOLUTION_REGISTER, solutionmV);
            Send_command((byte)Command.ISE_CALIBRATE_HIGH);

            DelayHelper.DelayMilliseconds(ISE_MV_MEASURE_TIME, allowThreadYield: true);
        }

        /// <summary>
        /// Returns the firmware version of the device.
        /// </summary>
        /// <returns></returns>
        public float GetVersion()
        {
            return Read_register((byte)Register.ISE_CALIBRATE_SINGLE_REGISTER);
        }

        /// <summary>
        /// Returns the dual-point calibration high-reference value.
        /// </summary>
        /// <returns></returns>
        public float GetCalibrateHighReference()
        {
            return Read_register((byte)Register.ISE_CALIBRATE_REFHIGH_REGISTER);
        }

        /// <summary>
        /// Returns the dual-point calibration low-reference value.
        /// </summary>
        /// <returns></returns>
        public float GetCalibrateLowReference()
        {
            return Read_register((byte)Register.ISE_CALIBRATE_REFLOW_REGISTER);
        }

        /// <summary>
        /// Returns the dual-point calibration high-reference value.
        /// </summary>
        /// <returns></returns>
        public float GetCalibrateHighReading()
        {
            return Read_register((byte)Register.ISE_CALIBRATE_READHIGH_REGISTER);
        }

        /// <summary>
        /// Returns the dual-point calibration low-reading value.
        /// </summary>
        /// <returns></returns>
        public float GetCalibrateLowReading()
        {
            return Read_register((byte)Register.ISE_CALIBRATE_READLOW_REGISTER);
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
        /// <param name="refLow">the reference low point</param>
        /// <param name="refHigh">the reference high point</param>
        /// <param name="readLow">the measured low point</param>
        /// <param name="readHigh">the measured high point</param>
        public void SetDualPointCalibration(float refLow, float refHigh, float readLow, float readHigh)
        {
            Write_register((byte)Register.ISE_CALIBRATE_REFLOW_REGISTER, refLow);
            Write_register((byte)Register.ISE_CALIBRATE_REFHIGH_REGISTER, refHigh);
            Write_register((byte)Register.ISE_CALIBRATE_READLOW_REGISTER, readLow);
            Write_register((byte)Register.ISE_CALIBRATE_READHIGH_REGISTER, readHigh);
        }

        /// <summary>
        /// Changes the default I2C address
        /// </summary>
        /// <param name="i2cAddress">The new I2C address</param>
        public void SetI2CAddress(float i2cAddress)
        {
            if (i2cAddress >= 1 && i2cAddress <= 127)
            {
                Write_register((byte)Register.ISE_SOLUTION_REGISTER, i2cAddress);
                Send_command((byte)Command.ISE_I2C);
            }
        }

        /// <summary>
        /// Use temperature compensation
        /// </summary>
        /// <param name="b">true to use temperature compensation, else false</param>
        public void UseTemperatureCompensation(bool b)
        {
            byte retval = Read_byte((byte)Register.ISE_CONFIG_REGISTER);

            retval = (byte)Bit_set((int)retval, ISE_TEMP_COMPENSATION_CONFIG_BIT, b);

            Write_byte((byte)Register.ISE_CONFIG_REGISTER, retval);
        }

        /// <summary>
        /// Reads EEPROM
        /// </summary>
        /// <param name="address">Address to read</param>
        /// <returns></returns>
        public float ReadEEPROM(float address)
        {
            Write_register((byte)Register.ISE_SOLUTION_REGISTER, address);
            Send_command((byte)Command.ISE_MEMORY_READ);
            return Read_register((byte)Register.ISE_BUFFER_REGISTER);
        }

        /// <summary>
        /// Write EEPROM
        /// </summary>
        /// <param name="address">Address</param>
        /// <param name="value">Value</param>
        public void WriteEEPROM(float address, float value)
        {
            Write_register((byte)Register.ISE_SOLUTION_REGISTER, address);
            Write_register((byte)Register.ISE_BUFFER_REGISTER, value);
            Send_command((byte)Command.ISE_MEMORY_WRITE);
        }

        /// <summary>
        /// Get Firmware version
        /// </summary>
        /// <returns></returns>
        public double GetFirmware()
        {
            return Read_byte((byte)Register.ISE_FW_VERSION_REGISTER);

        }

        private void Send_command(byte data)
        {
            Span<byte> bytes = stackalloc byte[2];
            bytes[0] = (byte)Register.ISE_TASK_REGISTER;
            bytes[1] = data;
            _device.Write(bytes);
            DelayHelper.DelayMilliseconds(10, allowThreadYield: true);
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

            Span<byte> bytes = new Span<byte>(new byte[4]);
            bytes[0] = dataArray[0];
            bytes[1] = dataArray[1];
            bytes[2] = dataArray[2];
            bytes[3] = dataArray[3];

            Change_register(register);
            _device.Write(bytes);
            DelayHelper.DelayMilliseconds(10, allowThreadYield: true);
        }

        private void Change_register(byte register)
        {
            _device.WriteByte(register);
            DelayHelper.DelayMilliseconds(10, allowThreadYield: true);
        }

        private byte Read_byte(byte register)
        {
            Change_register(register);
            DelayHelper.DelayMilliseconds(10, allowThreadYield: true);

            return _device.ReadByte();
        }

        private void Write_byte(byte register, byte value)
        {
            Change_register(register);

            _device.WriteByte(value);

            DelayHelper.DelayMilliseconds(10, allowThreadYield: true);

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
