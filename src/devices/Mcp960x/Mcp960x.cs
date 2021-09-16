// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using UnitsNet;

namespace Iot.Device.Mcp960x
{
    /// <summary>
    /// MCP960X - cold-junction compensated thermocouple to digital converter
    /// </summary>
    public class Mcp960x : IDisposable
    {
        private readonly byte _adcMeasurementResolutionType;
        private readonly byte _burstModeTemperatureSamplesType;
        private readonly byte _coldJunctionResolutionType;
        private readonly byte _digitalFilterCoefficientsType;
        private readonly byte _shutdownModesType;
        private readonly byte _thermocoupleType;

        private I2cDevice _i2cDevice;

        /// <summary>
        /// Returns absolute thermocouple temperature in Celsius
        /// </summary>
        /// <remarks>
        /// Returns the cold-junction compensated and error-corrected thermocouple temperature in degree Celsius
        /// </remarks>
        public Temperature GetTemperature()
        {
            Span<byte> data = stackalloc byte[2] { 0, 0 };
            ReadBytes(Register.READ_TH, data);

            return CalcTemperaturFromRegisterData(data, 0x7F);
        }

        /// <summary>
        /// Returns the error corrected thermocouple hot junction temperature without the cold junction compensation in Celsius
        /// </summary>
        /// <remarks>
        /// The temperatur is the error corrected thermocouple hot junction temperature without the cold junction compensation
        /// </remarks>
        public Temperature GetHotJunctionTemperature()
        {
            Span<byte> data = stackalloc byte[2] { 0, 0 };
            ReadBytes(Register.READ_TDELTA, data);

            return CalcTemperaturFromRegisterData(data, 0x7F);
        }

        /// <summary>
        /// Return cold junction / ambient temperature in Celsius
        /// </summary>
        /// <remarks>
        /// The cold junction temperatur equals to the ambient temperature from the device
        /// </remarks>
        public Temperature GetColdJunctionTemperature()
        {
            Span<byte> data = stackalloc byte[2] { 0, 0 };
            ReadBytes(Register.READ_TC, data);

            return CalcTemperaturFromRegisterData(data, 0x0F);
        }

        /// <summary>
        /// Return device id, revision major and revision minor
        /// </summary>
        /// <param name="deviceID">Returns the I2C device id.</param>
        /// <param name="revisionMajor">Returns the revision major.</param>
        /// <param name="revisionMinor">Returns the revision minor.</param>
        public void ReadDeviceID(out DeviceIDType deviceID, out byte revisionMajor, out byte revisionMinor)
        {
            Span<byte> data = stackalloc byte[2] { 0, 0 };
            ReadBytes(Register.READ_DEVICE_ID, data);

            deviceID = (DeviceIDType)data[0];
            revisionMajor = (byte)(data[1] >> 4);
            revisionMinor = (byte)(data[1] & 0xF);
        }

        /// <summary>
        /// Returns the status of the device
        /// </summary>
        /// <param name="burstComplete">Returns the burst mode conversions status flag.</param>
        /// <param name="thUpdate">Returns the temperature update/conversion complete flag.</param>
        /// <param name="shortCircuit">MCP9601/L01/RL01 only: Returns the short circuit detection flag.</param>
        /// <param name="openCircuitOrInputRange">MCP960X/L0X/RL0X: Returns the temperature range detection flag - MCP9601/L01/RL01: Returns the open circuit detection flag.</param>
        /// <param name="Alert4Status">Returns Alert 4 status bit.</param>
        /// <param name="Alert3Status">Returns Alert 3 status bit.</param>
        /// <param name="Alert2Status">Returns Alert 2 status bit.</param>
        /// <param name="Alert1Status">Returns Alert 1 status bit.</param>
        /// <remarks>
        /// burstComplete: Once Burst mode is enabled, this bit is normally set after the first burst is complete. User can clear it and poll the bit periodically until the next burst of temperature conversions is complete
        ///
        /// thUpdate: This bit is normally set. User can clear it and poll the bit until the next temperature conversion is complete.
        ///
        /// shortCircuit:
        ///   MCP9601/L01/RL01 only:
        ///     1 = Thermocouple Shorted to VDD or VSS
        ///     0 = Normal operation
        ///   The VSENSE pin must be connected to the Thermocouple.
        ///
        /// openCircuitOrInputRange:
        ///   MCP960X/L0X/RL0X:
        ///      1 = The ADC input Voltage (EMF) or the temperature data from the TH register exceeds the measurement range for the selected thermocouple type
        ///      0 = The ADC input Voltage(EMF) or the temperature data from the TH register is within the measurement range for the selected thermocouple type
        ///      If this bit is set, then the MCP960X/L0X/RL0X input voltage (EMF) to Degree Celsius conversion may be bypassed.
        ///   MCP9601/L01/RL01:
        ///      Indicates whether the Thermocouple is disconnected from the inputs.The VSENSE pin must be connected to the Thermocouple.
        ///
        ///   AlertXYZStatus:
        ///     Alert XYZ status bit
        ///       1 = TX &gt;  Temperature ALERT&lt;Y&gt;
        ///       0 = TX &lt;= Temperature ALERT&lt;Y&gt;
        ///     Where: TX is either TH or TC
        /// </remarks>
        public void ReadStatus(out bool burstComplete, out bool thUpdate, out bool shortCircuit, out bool openCircuitOrInputRange,
            out bool Alert4Status, out bool Alert3Status, out bool Alert2Status, out bool Alert1Status)
        {
            var data = ReadByte(Register.READ_WRITE_STATUS);

            burstComplete = Convert.ToBoolean(data & 0b1000_0000);
            thUpdate = Convert.ToBoolean(data & 0b0100_0000);
            shortCircuit = Convert.ToBoolean(data & 0b0010_0000);
            openCircuitOrInputRange = Convert.ToBoolean(data & 0b0001_0000);
            Alert4Status = Convert.ToBoolean(data & 0b0000_1000);
            Alert3Status = Convert.ToBoolean(data & 0b0000_0100);
            Alert2Status = Convert.ToBoolean(data & 0b0000_0010);
            Alert1Status = Convert.ToBoolean(data & 0b0000_0001);
        }

        /// <summary>
        /// Creates a new instance of the MCP960X.
        /// </summary>
        /// <param name="i2cDevice">The I2C device used for communication.</param>
        /// <param name="adcMeasurementResolutionType">ADC Measurement Resolution. It defaults to 18bit.</param>
        /// <param name="burstModeTemperatureSamplesType">Number of Burst Mode Temperature Samples. It defaults to 1 sample.</param>
        /// <param name="coldJunctionResolutionType">Cold junction resolution. It defaults to 0.0625°C.</param>
        /// <param name="digitalFilterCoefficientsType">Digital filter. It defaults to MID filter.</param>
        /// <param name="shutdownModesType">Shutdown Mode. It defaults to Normal operation.</param>
        /// <param name="thermocoupleType">Thermocouple type. It defaults to K.</param>
        /// <remarks>
        /// alerts are disabled
        /// </remarks>
        public Mcp960x(I2cDevice i2cDevice,
            ADCMeasurementResolutionType adcMeasurementResolutionType = ADCMeasurementResolutionType.R18,
            BurstModeTemperatureSamplesType burstModeTemperatureSamplesType = BurstModeTemperatureSamplesType.S1,
            ColdJunctionResolutionType coldJunctionResolutionType = ColdJunctionResolutionType.N_0_0625,
            DigitalFilterCoefficientsType digitalFilterCoefficientsType = DigitalFilterCoefficientsType.N4,
            ShutdownModesType shutdownModesType = ShutdownModesType.Normal,
            ThermocoupleType thermocoupleType = ThermocoupleType.K)
        {
            _i2cDevice = i2cDevice ?? throw new ArgumentNullException(nameof(i2cDevice));
            int deviceAddress = i2cDevice.ConnectionSettings.DeviceAddress;
            if (deviceAddress < 0x60 || deviceAddress > 0x67)
            {
                throw new ArgumentOutOfRangeException(nameof(i2cDevice), "The MCP960X address must be between 96 (0x60) and 103 (0x67).");
            }

            _adcMeasurementResolutionType = (byte)adcMeasurementResolutionType;
            _burstModeTemperatureSamplesType = (byte)burstModeTemperatureSamplesType;
            _coldJunctionResolutionType = (byte)coldJunctionResolutionType;
            _digitalFilterCoefficientsType = (byte)digitalFilterCoefficientsType;
            _shutdownModesType = (byte)shutdownModesType;
            _thermocoupleType = (byte)thermocoupleType;

            Initialize();
        }

        /// <summary>
        /// Standard initialization routine for Sensore Configuration Register, Device Configuration Register and Alert 1,2,3,4 Configuration Register
        /// </summary>
        /// <remarks>
        /// alerts are disabled
        /// </remarks>
        private void Initialize()
        {
            // bit 7, 3 Unimplemented: Read as ‘0’
            byte sensorRegisterValue = _thermocoupleType;               // bit 6-4
            sensorRegisterValue |= _digitalFilterCoefficientsType;      // bit 2-0

            WriteRegister(Register.READ_WRITE_CONFIGURATION_SENSOR, sensorRegisterValue);

            byte deviceRegisterValue = _coldJunctionResolutionType;     // bit 7
            deviceRegisterValue |= _adcMeasurementResolutionType;       // bit 6-5
            deviceRegisterValue |= _burstModeTemperatureSamplesType;    // bit 4-2
            deviceRegisterValue |= _shutdownModesType;                  // bit 1-0

            WriteRegister(Register.READ_WRITE_CONFIGURATION_DEVICE, deviceRegisterValue);

            // disable alerts
            WriteRegister(Register.READ_WRITE_ALERT_CONFIGURATION_1, 0x0);
            WriteRegister(Register.READ_WRITE_ALERT_CONFIGURATION_2, 0x0);
            WriteRegister(Register.READ_WRITE_ALERT_CONFIGURATION_3, 0x0);
            WriteRegister(Register.READ_WRITE_ALERT_CONFIGURATION_4, 0x0);
        }

        /// <summary>
        /// Returns the temperture in Celsius
        /// </summary>
        /// <param name="data">byte array of size 2, where UpperByte=data[0] and LowerByte=data[1].</param>
        /// <param name="valueBitPattern">Bit pattern to filter out the sign bits in the UpperByte.</param>
        /// <remarks>
        /// Temperature &gt;= 0°C
        ///    Temp = (UpperByte x 16 + LowerByte/16)
        /// Temperature &lt;  0°C
        ///    Temp = (UpperByte x 16 + LowerByte/16) – 4096
        ///
        /// In case of reading register TH, TDELTA the UpperByte Bit 7 is used as sign bit -> valueBitPattern = 0x7F.
        /// In case of reading register TC the UpperByte Bit 7-4 is used as sign bit -> valueBitPattern = 0x0F.
        ///
        /// So Bit 7 of UpperByte can always be used to determin the sign of the value.
        /// </remarks>
        private Temperature CalcTemperaturFromRegisterData(Span<byte> data, byte valueBitPattern)
        {
            if (data.Length != 2)
            {
                throw new IndexOutOfRangeException();
            }

            var rawValue = ((data[0] & valueBitPattern) * 16) + (data[1] * 0.0625);

            // checks if the temp is negative
            if ((data[0] & 0x80) == 0x80)
            {
                rawValue -= 4096;
            }

            // converts raw temperature value to struct Temperature as Degrees C
            var temperatureOut = Temperature.FromDegreesCelsius(rawValue);

            return temperatureOut;
        }

        /// <summary>
        /// Writes the Data to the Spi Device
        /// </summary>
        /// <remarks>
        /// Takes the data input byte and writes it to the spi device
        /// </remarks>
        /// <param name="register">Register location to write to which starts the device reading</param>
        /// <param name="data">Data to write to the device</param>
        private void WriteRegister(Register register, byte data)
        {
            Span<byte> dataout = stackalloc byte[]
            {
                (byte)register,
                data
            };

            _i2cDevice.Write(dataout);
        }

        /// <summary>
        /// Writes the Data to the Spi Device
        /// </summary>
        /// <remarks>
        /// Takes the data input byte array and writes it to the spi device
        /// </remarks>
        /// <param name="register">Register location to write to which starts the device reading</param>
        /// <param name="data">Data to write to the device</param>
        private void WriteRegister(Register register, ReadOnlySpan<byte> data)
        {
            Span<byte> toSend = stackalloc byte[data.Length + 1];
            toSend[0] = (byte)register;
            data.CopyTo(toSend.Slice(1));
            _i2cDevice.Write(toSend);
        }

        /// <summary>
        /// Read of the Datas on the Device
        /// </summary>
        /// <remarks>
        /// Writes the read address of the register and returns a byte
        /// </remarks>
        /// <param name="register">Register location to write to which starts the device reading</param>
        private byte ReadByte(Register register)
        {
            _i2cDevice.WriteByte((byte)register);
            return _i2cDevice.ReadByte();
        }

        /// <summary>
        /// Read of the Datas on the Device
        /// </summary>
        /// <remarks>
        /// Writes the read address of the register and outputs a byte list of the length provided
        /// </remarks>
        /// <param name="register">Register location to write to which starts the device reading</param>
        /// <param name="readBytes">bytes being read</param>
        private void ReadBytes(Register register, Span<byte> readBytes)
        {
            _i2cDevice.WriteByte((byte)register);
            _i2cDevice.Read(readBytes);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _i2cDevice?.Dispose();
            _i2cDevice = null!;
        }
    }
}
