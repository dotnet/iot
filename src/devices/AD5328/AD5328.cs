// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Buffers.Binary;
using System.Device.Spi;
using UnitsNet;

namespace Iot.Device.DAC
{
    /// <summary>
    /// Driver for the AD5328 DAC.
    /// </summary>
    public class AD5328 : IDisposable
    {
        private SpiDevice _spiDevice;
        private ElectricPotential _referenceVoltageA;
        private ElectricPotential _referenceVoltageB;
        private bool _disposedValue = false;

        /// <summary>
        /// Initializes a new instance of the AD5328 device.
        /// </summary>
        /// <param name="spiDevice">The SPI device used for communication.</param>
        /// <param name="referenceVoltageA">The reference voltage for the first 4 channels</param>
        /// <param name="referenceVoltageB">The reference voltage for the last 4 channels</param>
        public AD5328(SpiDevice spiDevice, ElectricPotential referenceVoltageA, ElectricPotential referenceVoltageB)
        {
            _spiDevice = spiDevice;
            _referenceVoltageA = referenceVoltageA;
            _referenceVoltageB = referenceVoltageB;
        }

        /// <summary>
        /// Sets the voltage of a certain channel
        /// </summary>
        /// <param name="channel">The channel number. Zero based. channel A = 0</param>
        /// <param name="voltage">The voltage</param>
        public void SetVoltage(UInt16 channel, ElectricPotential voltage)
        {
            // Check what reference voltage is used: Channel 0..3 = refA, Channel 4..7 = refB
            var refV = (channel > 3) ? _referenceVoltageB : _referenceVoltageA;
            // Check if requested voltage is not higher than reference voltage
            if (voltage > refV)
            {
                throw new ArgumentOutOfRangeException(nameof(voltage), $"Value should be equal or lower than {refV.Volts} V");
            }

            // Calculate the DAC value of the voltage
            var dacvalue = (UInt16)Math.Round(voltage.Volts / (refV.Volts / 4095));
            // The 16-bit word consists of 1 control bit and 3 address bits followed by 12 bits of DAC data.
            // In the case of a DAC write, the MSB is a 0.
            // The next 3 address bits determine whether the data is for DAC A, DAC B,
            // DAC C, DAC D, DAC E, DAC F, DAC G, or DAC H.
            UInt16 temp = (UInt16)((channel << 12) | dacvalue);
            // Swap bytes, MSB should go out first
            Span<byte> tempBytes = stackalloc byte[2];
            BinaryPrimitives.WriteUInt16BigEndian(tempBytes, temp);
            _spiDevice.Write(tempBytes);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (!_disposedValue)
            {
               _spiDevice?.Dispose();
               _disposedValue = true;
            }

            GC.SuppressFinalize(this);
        }
    }
}
