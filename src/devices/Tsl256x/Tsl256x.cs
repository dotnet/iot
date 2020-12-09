// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Buffers.Binary;
using System.Device.I2c;
using System.Threading;
using UnitsNet;
using UnitsNet.Units;

namespace IoT.Device.Tsl256x
{
    /// <summary>
    /// Light to Digital Converter TSL2560 and TSL2561
    /// </summary>
    public class Tsl256x : IDisposable
    {
        // All information from datasheet https://cdn-shop.adafruit.com/datasheets/TSL2561.pdf
        private I2cDevice _i2cDevice;
        private IntegrationTime _integrationTime;
        private Gain _gain;
        private InterruptControl _interruptControl;
        private InterruptPersistence _interruptPersistence;
        private PackageType _packageType;

        /// <summary>
        /// When the address select pin is to ground
        /// </summary>
        public const int SecondI2cAddress = 0x29;

        /// <summary>
        /// When the address select pin if float
        /// </summary>
        public const int DefaultI2cAddress = 0x39;

        /// <summary>
        /// When the select pin is to VDD
        /// </summary>
        public const int ThirdI2cAddress = 0x49;

        /// <summary>
        /// Initializes a new instance of the <see cref="Tsl256x"/> class.
        /// </summary>
        /// <param name="i2cDevice">And I2C Device</param>
        /// <param name="packageType">The type of package to have a proper illuminance calculation</param>
        public Tsl256x(I2cDevice i2cDevice, PackageType packageType)
        {
            _i2cDevice = i2cDevice ?? throw new ArgumentNullException($"I2C Device can't be null");
            _packageType = packageType;
            IntegrationTime = IntegrationTime.Integration402Milliseconds;
            Gain = Gain.Normal;
            Enabled = false;
        }

        /// <summary>
        /// Set power On or Off
        /// </summary>
        public bool Enabled
        {
            // 0x03 is power on, see doc page 14
            get => (ReadByte(Register.CONTROL) & 0x03) == 0x03;
            set => WriteByte(Register.CONTROL, value ? 0x03 : 0x00);
        }

        /// <summary>
        /// Get the version 0 for major for TSL2560 and 1 for TSL2561, minor is devision number
        /// </summary>
        /// <returns>The version</returns>
        public Version Version
        {
            get
            {
                var ver = ReadByte(Register.ID);
                return new Version(ver >> 4, ver & 0b0000_1111);
            }
        }

        /// <summary>
        /// Get or Set the integration time
        /// </summary>
        public IntegrationTime IntegrationTime
        {
            get => (IntegrationTime)(ReadByte(Register.TIMING) & (byte)IntegrationTime.Manual); // _integrationTime;
            set
            {
                _integrationTime = value;
                WriteByte(Register.TIMING, (byte)((byte)_integrationTime | (byte)_gain));
            }
        }

        /// <summary>
        /// Get or Set the gain
        /// </summary>
        public Gain Gain
        {
            get => (Gain)(ReadByte(Register.TIMING) & (byte)Gain.High); // _gain;
            set
            {
                _gain = value;
                WriteByte(Register.TIMING, (byte)((byte)_integrationTime | (byte)_gain));
            }
        }

        /// <summary>
        /// Start the manual integration
        /// </summary>
        public void StartManualIntegration()
        {
            _integrationTime = IntegrationTime.Manual;
            WriteByte(Register.TIMING, (byte)((byte)_integrationTime | (byte)_gain | 0b0000_1000));
            Enabled = true;
        }

        /// <summary>
        /// Stop the manual integration
        /// </summary>
        public void StopManualIntegration()
        {
            WriteByte(Register.TIMING, (byte)((byte)_integrationTime | (byte)_gain));
            Enabled = false;
        }

        /// <summary>
        /// This will set the threshold and enable the interrupt
        /// </summary>
        /// <param name="low">The low threshold</param>
        /// <param name="high">The high threshold</param>
        public void SetThreshold(ushort low, ushort high)
        {
            WriteByte(Register.THRESHLOWLOW, (byte)(low & 0xFF));
            WriteByte(Register.THRESHLOWHIGH, (byte)((low >> 8) & 0xFF));
            WriteByte(Register.THRESHHIGHLOW, (byte)(high & 0xFF));
            WriteByte(Register.THRESHHIGHHIGH, (byte)((high >> 8) & 0xFF));
        }

        /// <summary>
        /// Get or Set the interrupt Control Select
        /// </summary>
        /// <remarks>Interrupts are only on Channel 0</remarks>
        public InterruptControl InterruptControl
        {
            get => _interruptControl;
            set
            {
                _interruptControl = value;
                WriteByte(Register.INTERRUPT, (byte)((byte)_interruptControl | (byte)_interruptPersistence));
            }
        }

        /// <summary>
        /// Get or Set the interrupt Persistence Select
        /// </summary>
        /// <remarks>Interrupts are only on Channel 0</remarks>
        public InterruptPersistence InterruptPersistence
        {
            get => _interruptPersistence;
            set
            {
                _interruptPersistence = value;
                WriteByte(Register.INTERRUPT, (byte)((byte)_interruptControl | (byte)_interruptPersistence));
            }
        }

        /// <summary>
        /// Get the raw data from both channels
        /// </summary>
        /// <param name="channel0">Channel 0</param>
        /// <param name="channel1">Channel 1</param>
        public void GetRawChannels(out ushort channel0, out ushort channel1)
        {
            Span<byte> channel = stackalloc byte[2];
            // We clear the register as well when reading Channel 0
            channel[0] = ReadByte(Register.DATA0LOW | Register.CLEAR);
            channel[1] = ReadByte(Register.DATA0HIGH);
            channel0 = BinaryPrimitives.ReadUInt16BigEndian(channel);
            channel[0] = ReadByte(Register.DATA1LOW);
            channel[1] = ReadByte(Register.DATA1HIGH);
            channel1 = BinaryPrimitives.ReadUInt16BigEndian(channel);
        }

        /// <summary>
        /// Get the raw luminosity for a specific channel
        /// </summary>
        /// <param name="channel">The channel to get the luminosity</param>
        /// <returns>The raw luminosity from the ADC</returns>
        public ushort GetRawLuminosity(Channel channel)
        {
            GetRawChannels(out ushort channel0, out ushort channel1);
            switch (channel)
            {
                case Channel.VisibleInfrared:
                    return channel0;
                case Channel.Infrared:
                    return channel1;
                case Channel.Visible:
                    return (ushort)(channel0 - channel1);
                default:
                    throw new ArgumentException($"Wrong channel");
            }
        }

        /// <summary>
        /// Measure the illuminance, will wait for the measurement based on integration time
        /// </summary>
        /// <returns>The illuminance</returns>
        public Illuminance MeasureAndGetIlluminance()
        {
            Enabled = true;
            switch (_integrationTime)
            {
                case IntegrationTime.Integration13_7Milliseconds:
                    Thread.Sleep(14);
                    break;
                case IntegrationTime.Integration101Milliseconds:
                    Thread.Sleep(101);
                    break;
                case IntegrationTime.Integration402Milliseconds:
                    Thread.Sleep(402);
                    break;
                case IntegrationTime.Manual:
                default:
                    throw new ArgumentOutOfRangeException($"Only non manual integration time are supported");
            }

            Enabled = false;
            return GetIlluminance();
        }

        /// <summary>
        /// Get the calculated Illuminance. Default range is Lux
        /// </summary>
        /// <returns>The illuminance</returns>
        /// <remarks>If you have used the manual integration, you won't be able to use this formula</remarks>
        public Illuminance GetIlluminance()
        {
            // See documentation page 23 and following for the constant and the calculation
            // Integration time scaling
            // scale channel values by 2^10
            const int ChannelScale = 10;
            // 322/11 * 2^CH_SCALE
            const int ChannelScale13_7 = 0x7517;
            // 322/81 * 2^CH_SCALE
            const int ChannelScale101 = 0x0fe7;

            GetRawChannels(out ushort ch0, out ushort ch1);
            if (ch0 == 0)
            {
                return Illuminance.FromLux(0);
            }

            double ratio = ch1 / ch0;
            // Calculate the integration and scaling
            ulong scale = 0;
            switch (_integrationTime)
            {
                case IntegrationTime.Integration13_7Milliseconds:
                    scale = ChannelScale13_7;
                    break;
                case IntegrationTime.Integration101Milliseconds:
                    scale = ChannelScale101;
                    break;
                case IntegrationTime.Integration402Milliseconds:
                case IntegrationTime.Manual:
                default:
                    scale = 1 << ChannelScale;
                    break;
            }

            scale = _gain == Gain.Normal ? scale : scale << 4;

            double channel0 = (ch0 * scale) >> ChannelScale;
            double channel1 = (ch1 * scale) >> ChannelScale;

            double lux = 0;
            if (_packageType == PackageType.PackageCs)
            {
                if (ratio <= 0.52)
                {
                    lux = 0.0315 * channel0 - 0.0593 * channel0 * Math.Pow(ratio, 1.4);
                }
                else if ((ratio > 0.52) && (ratio <= 0.65))
                {
                    lux = 0.0229 * channel0 - 0.0291 * channel1;
                }
                else if ((ratio > 0.65) && (ratio <= 0.80))
                {
                    lux = 0.0157 * channel0 - 0.0180 * channel1;
                }
                else if ((ratio > 0.80) && (ratio <= 1.30))
                {
                    lux = 0.00338 * channel0 - 0.00260 * channel1;
                }
                else if (ratio > 1.30)
                {
                    lux = 0;
                }
            }
            else
            {
                if (ratio <= 0.50)
                {
                    lux = 0.0304 * channel0 - 0.062 * channel0 * Math.Pow(ratio, 1.4);
                }
                else if ((ratio > 0.50) && (ratio <= 0.61))
                {
                    lux = 0.0224 * channel0 - 0.031 * channel1;
                }
                else if ((ratio > 0.61) && (ratio <= 0.80))
                {
                    lux = 0.0128 * channel0 - 0.0153 * channel1;
                }
                else if ((ratio > 0.80) && (ratio <= 1.30))
                {
                    lux = 0.00146 * channel0 - 0.00112 * channel1;
                }
                else if (ratio > 1.30)
                {
                    lux = 0;
                }
            }

            return Illuminance.FromLux(lux);
        }

        private byte ReadByte(Register reg)
        {
            _i2cDevice.WriteByte((byte)reg);
            return _i2cDevice.ReadByte();
        }

        private void WriteByte(Register reg, byte toWrite)
        {
            Span<byte> toSend = stackalloc byte[2]
            {
                (byte)(Register.CMD | reg),
                toWrite
            };

            _i2cDevice.Write(toSend);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _i2cDevice?.Dispose();
        }
    }
}
