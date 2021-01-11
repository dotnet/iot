// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Buffers.Binary;
using System.Device.Spi;
using System.Threading;
using UnitsNet;

namespace Iot.Device.Max31865
{
    /// <summary>
    /// MAX31865 Resistance Temperature Detector to Digital Converter
    /// </summary>
    /// <remarks>
    /// Documentation https://datasheets.maximintegrated.com/en/ds/MAX31865.pdf
    /// </remarks>
    public class Max31865 : IDisposable
    {
        // Callender Van Dusen coefficiants A/B for temperature conversion
        private const double TemperatureCoefficiantA = 3.9083e-3;
        private const double TemperatureCoefficiantB = -5.775e-7;

        private readonly PlatinumResistanceThermometerType _prtType;
        private readonly ResistanceTemperatureDetectorWires _rtdWires;
        private readonly ConversionFilterMode _filterMode;
        private readonly double _referenceResistor;
        private readonly bool _shouldDispose;
        private SpiDevice _spiDevice;

        /// <summary>
        /// MAX31865 Spi Clock Frequency
        /// </summary>
        public const int SpiClockFrequency = 5_000_000;

        /// <summary>
        /// MAX31865 SPI Mode 1
        /// </summary>
        public const SpiMode SpiMode1 = SpiMode.Mode1;

        /// <summary>
        /// MAX31865 SPI Mode 3
        /// </summary>
        public const SpiMode SpiMode3 = SpiMode.Mode3;

        /// <summary>
        /// MAX31865 SPI Data Flow
        /// </summary>
        public const DataFlow SpiDataFlow = DataFlow.MsbFirst;

        /// <summary>
        /// MAX31865 Temperature
        /// </summary>
        public Temperature Temperature { get => Temperature.FromDegreesCelsius(GetTemperature()); }

        /// <summary>
        /// Creates a new instance of the MAX31865.
        /// </summary>
        /// <param name="spiDevice">The communications channel to a device on a SPI bus</param>
        /// <param name="platinumResistanceThermometerType">The type of Platinum Resistance Thermometer</param>
        /// <param name="resistanceTemperatureDetectorWires">The number of wires the Platinum Resistance Thermometer has</param>
        /// <param name="referenceResistor">The reference resistor value in Ohms.</param>
        /// <param name="filterMode">Noise rejection filter mode</param>
        /// <param name="shouldDispose">True to dispose the SPI device</param>
        public Max31865(SpiDevice spiDevice, PlatinumResistanceThermometerType platinumResistanceThermometerType, ResistanceTemperatureDetectorWires resistanceTemperatureDetectorWires, ElectricResistance referenceResistor, ConversionFilterMode filterMode = ConversionFilterMode.Filter60Hz, bool shouldDispose = true)
        {
            _spiDevice = spiDevice ?? throw new ArgumentNullException(nameof(spiDevice));
            _prtType = platinumResistanceThermometerType;
            _rtdWires = resistanceTemperatureDetectorWires;
            _filterMode = filterMode;
            _referenceResistor = referenceResistor.Ohms;
            _shouldDispose = shouldDispose;
            Initialize();
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (_shouldDispose)
            {
                _spiDevice?.Dispose();
            }

            _spiDevice = null!;
        }

        /// <summary>
        /// The fault state of the sensor
        /// </summary>
        public FaultStatus Faults
        {
            get
            {
                Span<byte> readBuffer = stackalloc byte[2];
                WriteRead(Register.FaultStatus, readBuffer);

                return new FaultStatus(
                    (readBuffer[1] & 0x04) == 0x04,
                    (readBuffer[1] & 0x08) == 0x08,
                    (readBuffer[1] & 0x20) == 0x20,
                    (readBuffer[1] & 0x10) == 0x10,
                    (readBuffer[1] & 0x40) == 0x40,
                    (readBuffer[1] & 0x80) == 0x80);
            }
        }

        /// <summary>
        /// Standard initialization routine.
        /// </summary>
        /// <remarks>
        /// You can add new write lines if you want to alter the settings of the device. Settings can be found in the Technical Manual
        /// </remarks>
        private void Initialize()
        {
            ReadOnlySpan<byte> configurationSetting = stackalloc byte[]
            {
                (byte)Register.ConfigurationWrite,
                (byte)((byte)(_rtdWires == ResistanceTemperatureDetectorWires.ThreeWire ? Configuration.ThreeWire : Configuration.TwoFourWire) | (byte)(_filterMode == ConversionFilterMode.Filter50Hz ? Configuration.Filter50HZ : Configuration.Filter60HZ))
            };

            Write(configurationSetting);
        }

        /// <summary>
        /// Clears all the faults
        /// </summary>
        private void ClearFaults()
        {
            Span<byte> configuration = stackalloc byte[2];
            WriteRead(Register.ConfigurationRead, configuration);

            // Page 14 (Fault Status Clear (D1)) of the technical documentation
            configuration[1] = (byte)(configuration[1] & ~0x2C);
            configuration[1] |= (byte)Configuration.FaultStatus;

            configuration[0] = (byte)Register.ConfigurationWrite;
            Write(configuration);
        }

        /// <summary>
        /// Enable/Disable the bias voltage on the resistance temperature detector sensor
        /// </summary>
        private void EnableBias(bool enable)
        {
            Span<byte> configuration = stackalloc byte[2];
            WriteRead(Register.ConfigurationRead, configuration);

            configuration[1] = enable ? (byte)(configuration[1] | (byte)Configuration.Bias) : (byte)(configuration[1] & ~(byte)Configuration.Bias);

            configuration[0] = (byte)Register.ConfigurationWrite;
            Write(configuration);
        }

        /// <summary>
        /// Enable/Disable the one shot mode on the resistance temperature detector sensor
        /// </summary>
        private void EnableOneShot(bool enable)
        {
            Span<byte> configuration = stackalloc byte[2];
            WriteRead(Register.ConfigurationRead, configuration);

            configuration[1] = enable ? (byte)(configuration[1] | (byte)Configuration.OneShot) : (byte)(configuration[1] & ~(byte)Configuration.OneShot);

            configuration[0] = (byte)Register.ConfigurationWrite;
            Write(configuration);
        }

        /// <summary>
        /// Reads the raw resistance temperature detector value and converts it to a temperature using the Callender Van Dusen temperature conversion of 15 bit ADC resistance ratio data.
        /// </summary>
        /// <remarks>
        /// This math originates from: http://www.analog.com/media/en/technical-documentation/application-notes/AN709_0.pdf
        /// </remarks>
        /// <returns>Temperature in degrees celsius</returns>
        private double GetTemperature()
        {
            short rtdNominal = (short)_prtType;
            double z1, z2, z3, z4;
            double temperature;

            double resistance = GetResistance();

            z1 = -TemperatureCoefficiantA;
            z2 = TemperatureCoefficiantA * TemperatureCoefficiantA - (4 * TemperatureCoefficiantB);
            z3 = (4 * TemperatureCoefficiantB) / rtdNominal;
            z4 = 2 * TemperatureCoefficiantB;

            temperature = z2 + (z3 * resistance);
            temperature = (Math.Sqrt(temperature) + z1) / z4;

            if (temperature < 0)
            {
                // For the following math to work, nominal resistance temperature detector resistance must be normalized to 100 ohms
                resistance /= rtdNominal;
                resistance *= 100;

                double rpoly = resistance;

                temperature = -242.02;
                temperature += 2.2228 * rpoly;
                rpoly *= resistance; // square
                temperature += 2.5859e-3 * rpoly;
                rpoly *= resistance; // ^3
                temperature -= 4.8260e-6 * rpoly;
                rpoly *= resistance; // ^4
                temperature -= 2.8183e-8 * rpoly;
                rpoly *= resistance; // ^5
                temperature += 1.5243e-10 * rpoly;
            }

            return temperature;
        }

        /// <summary>
        /// Read the resistance of the resistance temperature detector.
        /// </summary>
        /// <returns>Resistance in Ohms</returns>
        private double GetResistance()
        {
            double rtd = ReadRawRTD();
            rtd /= 32768;
            rtd *= _referenceResistor;

            return rtd;
        }

        /// <summary>
        /// Read the raw 16-bit value from the resistance temperature detector reistance registers in one shot mode
        /// </summary>
        /// <returns>The raw 16-bit value, NOT temperature</returns>
        private ushort ReadRawRTD()
        {
            ClearFaults();
            EnableBias(true);

            // Page 3 (Bias Voltage Startup Time) & 4 (see note 4) of the technical documentation
            Thread.Sleep(10);

            EnableOneShot(true);

            // Page 3 (Temperature Conversion Time) of the technical documentation
            Thread.Sleep((short)_filterMode);

            Span<byte> readBuffer = stackalloc byte[3];
            WriteRead(Register.RTDMSB, readBuffer);

            EnableBias(false); // Disable Bias current again to reduce selfheating.

            return (ushort)(BinaryPrimitives.ReadUInt16BigEndian(readBuffer.Slice(1, 2)) >> 1);
        }

        /// <summary>
        /// Writes the Data to the Spi Device
        /// </summary>
        /// <remarks>
        /// Takes the data input byte and writes it to the spi device
        /// </remarks>
        /// <param name="data">Data to write to the device</param>
        private void Write(ReadOnlySpan<byte> data) => _spiDevice.Write(data);

        /// <summary>
        /// Full Duplex Read of the Data on the Spi Device
        /// </summary>
        /// <remarks>
        /// Writes the read address of the register and outputs a byte list of the length provided
        /// </remarks>
        /// <param name="register">Register location to write to which starts the device reading</param>
        /// <param name="readBuffer">Number of bytes being read</param>
        private void WriteRead(Register register, Span<byte> readBuffer)
        {
            Span<byte> regAddrBuf = stackalloc byte[readBuffer.Length];

            regAddrBuf[0] = (byte)(register);
            _spiDevice.TransferFullDuplex(regAddrBuf, readBuffer);
        }
    }
}
