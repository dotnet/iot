// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Device.I2c;
using System.IO;

namespace Iot.Device.Lp55231
{
    /// <summary>
    /// Raspberry Pi Motor Hat based on PCA9685 PWM controller
    /// </summary>
    public class Lp55231 : IDisposable
    {
        /// <summary>
        /// Default I2C address of Motor Hat
        /// </summary>
        public const int I2cAddressBase = 0x32;

        /// <summary>
        /// Gets the channel of the red element of the specific instance
        /// </summary>
        /// <param name="instance">The RGB instance (0-2)</param>
        /// <returns>The channel number</returns>
        public static byte RedChannel(byte instance) => (byte)(6 + instance);

        /// <summary>
        /// Gets the channel of the green element of the specific instance
        /// </summary>
        /// <param name="instance">The RGB instance (0-2)</param>
        /// <returns>The channel number</returns>
        public static byte GreenChannel(byte instance) => (byte)(instance * 2);

        /// <summary>
        /// Gets the channel of the blue element of the specific instance
        /// </summary>
        /// <param name="instance">The RGB instance (0-2)</param>
        /// <returns>The channel number</returns>
        public static byte BlueChannel(byte instance) => (byte)(instance * 2 + 1);

        private readonly I2cDevice _device;
        private readonly byte[] _channelIntensity;
        private readonly IReadOnlyList<RgbLed> _rgbLeds;

        /// <summary>
        /// Initializes a new instance of the <see cref="Lp55231"/> class with the specified I2C settings.
        /// </summary>
        /// <param name="settings">The I2C settings of the Lp55231.</param>
        /// <remarks>
        /// The default i2c address is 0x60, but the HAT can be configured in hardware to any address from 0x60 to 0x7f.
        /// The PWM hardware used by this HAT is a PCA9685. It has a total possible frequency range of 24 to 1526 Hz.
        /// Setting the frequency above or below this range will cause PWM hardware to be set at its maximum or minimum setting.
        /// </remarks>
        public Lp55231(I2cConnectionSettings? settings = default)
        {
            settings ??= new I2cConnectionSettings(1, I2cAddressBase);

            _device = I2cDevice.Create(settings);
            _channelIntensity = new byte[9];

            _rgbLeds = new[]
            {
                new RgbLed(0, SetIntensity),
                new RgbLed(1, SetIntensity),
                new RgbLed(2, SetIntensity)
            };
        }

        private byte ReadRegister(Register register)
        {
            return ReadRegister((byte)register);
        }

        private byte ReadRegister(byte register)
        {
            _device.WriteByte((byte)register);
            return _device.ReadByte();
        }

        private void WriteRegister(Register register, byte data)
        {
            WriteRegister((byte)register, data);
        }

        private void WriteRegister(byte register, byte data)
        {
            Span<byte> bytes = stackalloc byte[2];
            bytes[0] = (byte)register;
            bytes[1] = data;
            _device.Write(bytes);
        }

        private void SetIntensity(byte index, byte value)
        {
            byte register = (byte)(Register.REG_D1_PWM + index);
            WriteRegister(register, value);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _device.Dispose();
        }

        /// <summary>
        /// Resets the Lp55231
        /// </summary>
        /// <remarks>
        /// You should delay after calling this method
        /// </remarks>
        public void Reset()
        {
            try
            {
                WriteRegister(Register.REG_RESET, 0xFF);
            }
            catch (IOException)
            {
                // Resetting will prevent the expected ack resulting
                // in an IOException. It's fine to swallow this.
            }
        }

        /// <summary>
        /// Gets/sets whether the Lp55231 is enabled.
        /// </summary>
        /// <remarks>
        /// Setting this value will stop eny programs currently running.
        /// </remarks>
        public bool Enabled
        {
            get
            {
                var register = ReadRegister(Register.REG_CNTRL1);
                return (register & (byte)Control1RegisterFlags.Enabled) > 0;
            }
            set
            {
                byte flags = value
                    ? (byte)Control1RegisterFlags.Enabled
                    : (byte)0x00;

                WriteRegister(Register.REG_CNTRL1, flags);
            }
        }

        /// <summary>
        /// Gets/sets miscellaneous control flags
        /// </summary>
        public MiscFlags Misc
        {
            get
            {
                var flags = ReadRegister(Register.REG_MISC);
                return (MiscFlags)flags;
            }
            set
            {
                var flags = (byte)value;
                WriteRegister(Register.REG_MISC, flags);
            }
        }

        /// <summary>
        /// Gets a <see cref="IReadOnlyCollection{T}"/> of <see cref="RgbLed"/> instances.
        /// </summary>
        public IReadOnlyList<RgbLed> RgbLeds => _rgbLeds;
    }
}
