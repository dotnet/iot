// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Device.I2c;
using System.IO;

namespace Iot.Device.Lp55231
{
    /// <summary>
    /// Lp55231 9 channel I2C PWM LED controller
    /// </summary>
    public class Lp55231 : IDisposable
    {
        /// <summary>
        /// Default I2C address of Motor Hat
        /// </summary>
        public const int DefaultI2cAddress = 0x32;

        /// <summary>
        /// Gets the channel of the red element of the specific instance
        /// </summary>
        /// <param name="instance">The RGB instance (0-2)</param>
        /// <returns>The channel number</returns>
        internal static byte RedChannel(byte instance) => (byte)(6 + instance);

        /// <summary>
        /// Gets the channel of the green element of the specific instance
        /// </summary>
        /// <param name="instance">The RGB instance (0-2)</param>
        /// <returns>The channel number</returns>
        internal static byte GreenChannel(byte instance) => (byte)(instance * 2);

        /// <summary>
        /// Gets the channel of the blue element of the specific instance
        /// </summary>
        /// <param name="instance">The RGB instance (0-2)</param>
        /// <returns>The channel number</returns>
        internal static byte BlueChannel(byte instance) => (byte)(instance * 2 + 1);

        private readonly IReadOnlyList<RgbLed> _rgbLeds;
        private I2cDevice _i2cDevice;

        /// <summary>
        /// Initializes a new instance of the <see cref="Lp55231"/> class with the specified <see cref="I2cDevice"/>.
        /// </summary>
        /// <param name="i2cDevice">The I2C device to use to communicate with the Lp55231.</param>
        /// <remarks>
        /// The default i2c address is 0x32.
        /// </remarks>
        public Lp55231(I2cDevice i2cDevice)
        {
            _i2cDevice = i2cDevice ?? throw new ArgumentNullException(nameof(i2cDevice));

            _rgbLeds = new[]
            {
                new RgbLed(0, SetIntensity),
                new RgbLed(1, SetIntensity),
                new RgbLed(2, SetIntensity)
            };
        }

        private byte ReadRegister(Register register)
        {
            _i2cDevice.WriteByte((byte)register);
            return _i2cDevice.ReadByte();
        }

        private void WriteRegister(Register register, byte data)
        {
            Span<byte> bytes = stackalloc byte[2];
            bytes[0] = (byte)register;
            bytes[1] = data;
            _i2cDevice.Write(bytes);
        }

        private void SetIntensity(byte index, byte value)
        {
            WriteRegister(Register.REG_D1_PWM + index, value);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _i2cDevice?.Dispose();
            _i2cDevice = null!;
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
