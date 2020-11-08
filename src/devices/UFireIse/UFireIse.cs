// Licensed to the.NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

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
        // A measurement takes 750 ms, see https://www.ufire.co/docs/uFire_ISE/#use
        private const int IseMvMeasureTime = 750;

        // Implement short delays, required for the device to operate reliably, see https://www.ufire.co/docs/uFire_ISE/#use
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
            get => _temperatureCompensation;
            set
            {
                _temperatureCompensation = value;

                byte retval = ReadByte(Register.ISE_CONFIG_REGISTER);

                retval = (byte)Bit_set((int)retval, IseTemperatureCompensationConfigBit, _temperatureCompensation);

                WriteByte(Register.ISE_CONFIG_REGISTER, retval);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UFireIse"/> class.
        /// </summary>
        /// <param name="i2cDevice">The I2C device to be used</param>
        public UFireIse(I2cDevice i2cDevice) =>
            _device = i2cDevice ?? throw new ArgumentException($"{nameof(i2cDevice)} cannot be null");

        /// <summary>
        /// Read a value from the ISE Probe Interface, typical measure are in the millivolt range.
        /// </summary>
        /// <returns>value from ISE Probe Interface, typical measure are in the millivolt range. On error it return -1 as value</returns>
        public ElectricPotential ReadElectricPotential()
        {
            SendCommand((byte)Command.ISE_MEASURE_MV);

            DelayHelper.DelayMilliseconds(IseMvMeasureTime, allowThreadYield: true);

            _measurement = ReadFloat(Register.ISE_MV_REGISTER);
            _measurement = Convert.ToSingle(Math.Round(_measurement, 2));

            return new ElectricPotential(_measurement, UnitsNet.Units.ElectricPotentialUnit.Millivolt);
        }

        /// <summary>
        /// Read temperature
        /// </summary>
        /// <returns>Temperature. Temperature range: -2 to 35 C. Temperature Precision: 0.05 C</returns>
        public Temperature ReadTemperature()
        {
            SendCommand((byte)Command.ISE_MEASURE_TEMP);

            DelayHelper.DelayMilliseconds(IseTemperatureMeasureTime, allowThreadYield: true);
            _temperature = new Temperature(ReadFloat(Register.ISE_TEMP_REGISTER), UnitsNet.Units.TemperatureUnit.DegreeCelsius);

            return _temperature;
        }

        /// <summary>
        /// It sets the a temperature used as compensation. The compensation temperature is to correlate the mV input to the correct pH value, see https://assets.tequipment.net/assets/1/26/Yokogawa_Temperature_Compensation.pdf
        /// </summary>
        /// <param name="temp">The temperature used for compensation</param>
        public void SetTemperatureCompensation(Temperature temp)
        {
            WriteRegister(Register.ISE_TEMP_REGISTER, Convert.ToSingle(temp.DegreesCelsius));
            _temperature = temp;
        }

        /// <summary>
        /// Calibrates the probe using a single solution. Put the probe in a solution where the pH (Power of Hydrogen) is known.
        /// </summary>
        /// <param name="solution">The known pH value in mV. Range: -1024 mV to 1024 mV</param>
        public void CalibrateFromSingleValue(ElectricPotential solution)
        {
            if (solution.Millivolts >= -1024 && solution.Millivolts <= 1024)
            {
                WriteRegister(Register.ISE_SOLUTION_REGISTER, Convert.ToSingle(solution.Millivolts));
                SendCommand((byte)Command.ISE_CALIBRATE_SINGLE);

                DelayHelper.DelayMilliseconds(IseMvMeasureTime, allowThreadYield: true);
            }
            else
            {
                throw new ArgumentOutOfRangeException("The value range is -1024 mV to 1024 mV");
            }
        }

        /// <summary>
        /// The lower value when calibrating from two solutions. Put the probe in a solution with the lowest pH (Power of Hydrogen) value.
        /// </summary>
        /// <param name="solution">The known pH value in mV. Range: -1024 mV to 1024 mV</param>
        public void CalibrateFromTwoValuesLowValue(ElectricPotential solution)
        {
            if (solution.Millivolts >= -1024 && solution.Millivolts <= 1024)
            {
                WriteRegister(Register.ISE_SOLUTION_REGISTER, Convert.ToSingle(solution.Millivolts));
                SendCommand((byte)Command.ISE_CALIBRATE_LOW);

                DelayHelper.DelayMilliseconds(IseMvMeasureTime, allowThreadYield: true);
            }
            else
            {
                throw new ArgumentOutOfRangeException("The value range is -1024 mV to 1024 mV");
            }
        }

        /// <summary>
        ///  The highest value when calibrating from two solutions. Put the probe in a solution with the highest pH (Power of Hydrogen) value.
        /// </summary>
        /// <param name="solution">The known pH value in mV. Range: -1024 mV to 1024 mV</param>
        public void CalibrateFromTwoValuesHighValue(ElectricPotential solution)
        {
            if (solution.Millivolts >= -1024 && solution.Millivolts <= 1024)
            {
                WriteRegister(Register.ISE_SOLUTION_REGISTER, Convert.ToSingle(solution.Millivolts));
                SendCommand((byte)Command.ISE_CALIBRATE_HIGH);

                DelayHelper.DelayMilliseconds(IseMvMeasureTime, allowThreadYield: true);
            }
            else
            {
                throw new ArgumentOutOfRangeException("The value range is -1024 mV to 1024 mV");
            }
        }

        /// <summary>
        /// Returns the firmware version of the device. The manufacturer do not provide any information about the format of the version number, see https://www.ufire.co/docs/uFire_ISE/api.html#getversion
        /// </summary>
        /// <returns>Firmware version</returns>
        public byte GetVersion() =>
            ReadByte(Register.ISE_VERSION_REGISTER);

        /// <summary>
        /// Dual point uses two measures for low and high points. It needs the measured value (reading value) and the known value (reference value). Calling SetDualPointCalibration saves both the reading and reference value.
        /// When there are high and low calibration points, the device will automatically use them to adjust readings.To disable dual-point adjustment, call ResetCalibration to remove all calibration data.
        /// </summary>
        /// <returns>The known value (reference value) for calibrate the high value</returns>
        public ElectricPotential GetCalibrateHighReference() =>
            new ElectricPotential(ReadFloat(Register.ISE_CALIBRATE_REFHIGH_REGISTER), UnitsNet.Units.ElectricPotentialUnit.Millivolt);

        /// <summary>
        /// Dual point uses two measures for low and high points. It needs the measured value (reading value) and the known value (reference value). Calling SetDualPointCalibration saves both the reading and reference value.
        /// When there are high and low calibration points, the device will automatically use them to adjust readings.To disable dual-point adjustment, call ResetCalibration to remove all calibration data.
        /// </summary>
        /// <returns>The known value (reference value) for calibrate the low value</returns>
        public ElectricPotential GetCalibrateLowReference() =>
            new ElectricPotential(ReadFloat(Register.ISE_CALIBRATE_REFLOW_REGISTER), UnitsNet.Units.ElectricPotentialUnit.Millivolt);

        /// <summary>
        /// Dual point uses two measures for low and high points. It needs the measured value (reading value) and the known value (reference value). Calling SetDualPointCalibration saves both the reading and reference value.
        /// When there are high and low calibration points, the device will automatically use them to adjust readings.To disable dual-point adjustment, call ResetCalibration to remove all calibration data.
        /// </summary>
        /// <returns>The measured value (reading value) for calibrate the high value</returns>
        public ElectricPotential GetCalibrateHighReading() =>
            new ElectricPotential(ReadFloat(Register.ISE_CALIBRATE_READHIGH_REGISTER), UnitsNet.Units.ElectricPotentialUnit.Millivolt);

        /// <summary>
        /// Dual point uses two measures for low and high points. It needs the measured value (reading value) and the known value (reference value). Calling SetDualPointCalibration saves both the reading and reference value.
        /// When there are high and low calibration points, the device will automatically use them to adjust readings.To disable dual-point adjustment, call ResetCalibration to remove all calibration data.
        /// </summary>
        /// <returns>The measured value (reading value) for calibrate the low value</returns>
        public ElectricPotential GetCalibrateLowReading() =>
            new ElectricPotential(ReadFloat(Register.ISE_CALIBRATE_READLOW_REGISTER), UnitsNet.Units.ElectricPotentialUnit.Millivolt);

        /// <summary>
        ///  Resets all the stored calibration information.It is possible to run without calibration.
        /// </summary>
        public void ResetCalibration()
        {
            WriteRegister(Register.ISE_CALIBRATE_SINGLE_REGISTER);
            WriteRegister(Register.ISE_CALIBRATE_REFHIGH_REGISTER);
            WriteRegister(Register.ISE_CALIBRATE_REFLOW_REGISTER);
            WriteRegister(Register.ISE_CALIBRATE_READHIGH_REGISTER);
            WriteRegister(Register.ISE_CALIBRATE_READLOW_REGISTER);
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
                WriteRegister(Register.ISE_CALIBRATE_REFLOW_REGISTER, Convert.ToSingle(refLow.Millivolts));
                WriteRegister(Register.ISE_CALIBRATE_REFHIGH_REGISTER, Convert.ToSingle(refHigh));
                WriteRegister(Register.ISE_CALIBRATE_READLOW_REGISTER, Convert.ToSingle(readLow));
                WriteRegister(Register.ISE_CALIBRATE_READHIGH_REGISTER, Convert.ToSingle(readHigh));
            }
            else
            {
                throw new ArgumentOutOfRangeException("The value range is -1024 mV to 1024 mV for refLow, refHigh, readLow and readLow");
            }
        }

        /// <summary>
        /// Changes the default I2C address
        /// </summary>
        /// <param name="i2cAddress">The new I2C address. Range: 1 to 127 </param>
        public void SetI2cAddressAndDispose(byte i2cAddress)
        {
            if (i2cAddress >= 1 && i2cAddress <= 127)
            {
                WriteRegister(Register.ISE_SOLUTION_REGISTER, Convert.ToByte(i2cAddress));
                SendCommand((byte)Command.ISE_I2C);
            }
            else
            {
                throw new ArgumentOutOfRangeException("The value range is 1 to 127");
            }

            Dispose();
        }

        /// <summary>
        /// Low level reads of EEPROM
        /// </summary>
        /// <param name="address">Address to read</param>
        /// <returns>Low level data from EEPROM</returns>
        internal float ReadEeprom(Register address)
        {
            WriteRegister(Register.ISE_SOLUTION_REGISTER, (byte)address);
            SendCommand((byte)Command.ISE_MEMORY_READ);

            return ReadFloat(Register.ISE_BUFFER_REGISTER);
        }

        /// <summary>
        /// Low level write of EEPROM
        /// </summary>
        /// <param name="address">Address</param>
        /// <param name="data">Data to write to the address</param>
        internal void WriteEeprom(Register address, float data)
        {
            WriteRegister(Register.ISE_SOLUTION_REGISTER, (byte)address);
            WriteRegister(Register.ISE_BUFFER_REGISTER, data);
            SendCommand((byte)Command.ISE_MEMORY_WRITE);
        }

        /// <summary>
        /// Get Firmware version.The manufacturer do not provide any information about the format of the version number, see https://www.ufire.co/docs/uFire_ISE/api.html#getversion
        /// </summary>
        /// <returns>The version of the firmware</returns>
        public byte GetFirmwareVersion()
        {
            return ReadByte(Register.ISE_FW_VERSION_REGISTER);
        }

        private void SendCommand(byte data)
        {
            Span<byte> bytes = stackalloc byte[2];
            bytes[0] = (byte)Register.ISE_TASK_REGISTER;
            bytes[1] = data;
            _device.Write(bytes);
            DelayHelper.DelayMilliseconds(IseCommunicationDelay, allowThreadYield: true);
        }

        private float ReadFloat(Register register)
        {
            ChangeRegister(register);

            Span<byte> data = stackalloc byte[4];

            _device.Read(data);

            return Convert.ToSingle(BitConverter.ToSingle(data.ToArray(), 0));
        }

        private void WriteRegister(Register register, float data = 0)
        {
            // μFire ISE (Ion Specific Electrode) is 0 as default
            Span<byte> bytes = stackalloc byte[4]
             {
                0,
                0,
                0,
                0
             };

            if (BitConverter.TryWriteBytes(bytes, data))
            {
                ChangeRegister(register);
                _device.Write(bytes);
                DelayHelper.DelayMilliseconds(IseCommunicationDelay, allowThreadYield: true);
            }
            else
            {
                throw new Exception("Not possible to write bytes");
            }
        }

        /// <summary>
        /// Change the register in μFire ISE (Ion Specific Electrode). This makes sure that the next time it writes, it writes to the correct location
        /// </summary>
        /// <param name="register">The register to change μFire ISE (Ion Specific Electrode) to write to next times it writes</param>
        private void ChangeRegister(Register register)
        {
            _device.WriteByte((byte)register);
            DelayHelper.DelayMilliseconds(IseCommunicationDelay, allowThreadYield: true);
        }

        private byte ReadByte(Register register)
        {
            ChangeRegister(register);
            DelayHelper.DelayMilliseconds(IseCommunicationDelay, allowThreadYield: true);

            return _device.ReadByte();
        }

        private void WriteByte(Register register, byte value)
        {
            ChangeRegister(register);

            _device.WriteByte(value);

            DelayHelper.DelayMilliseconds(IseCommunicationDelay, allowThreadYield: true);

        }

        private int Bit_set(int v, int index, bool x)
        {
            int mask = 1 << index;
            if (x)
            {
                v |= mask;
            }
            else
            {
                v &= ~mask;
            }

            return v;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _device?.Dispose();
            _device = null!;
        }
    }
}
