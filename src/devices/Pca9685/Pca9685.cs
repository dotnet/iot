// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Buffers.Binary;
using System.Device;
using System.Device.I2c;
using System.Device.Pwm;
using System.Diagnostics;
#if NETSTANDARD2_0
using Math = System.MathExtension;
#endif

namespace Iot.Device.Pwm
{
    using static Mode1;
    using static Iot.Device.Pwm.Mode2;
    using static Iot.Device.Pwm.Register;

    /// <summary>
    /// PCA9685 PWM LED/servo controller
    /// </summary>
    public class Pca9685 : IDisposable
    {
        /// <summary>
        /// I2C address base. Equal to actual address when all selectable address bits are equal to 0.
        /// </summary>
        public const byte I2cAddressBase = 0x40;

        private I2cDevice _device;

        private bool _usingExternalClock;
        private double _clockFrequency = 25_000_000;

        /// <summary>
        /// Get clock frequency (Hz). Only set if you are using external clock.
        /// </summary>
        public double ClockFrequency
        {
            get => _clockFrequency;
            set
            {
                if (!_usingExternalClock)
                    throw new InvalidOperationException("Clock frequency can only be set when using external oscillator.");

                _clockFrequency = value;
            }
        }

        /// <summary>
        /// Set PWM frequency or get effective value.
        /// </summary>
        /// <remarks>
        /// Value of the effective frequency may be different than desired frequency.
        /// Read the property in order to get the actual value
        /// </remarks>
        public double PwmFrequency
        {
            get => PrescaleToFrequency(_prescale);
            set => Prescale = FrequencyToPrescale(value);
        }

        private byte _prescale;

        /// <summary>
        /// Set PWM frequency using prescale value or get the value.
        /// </summary>
        protected byte Prescale
        {
            get => _prescale;
            set
            {
                var v = value < 3 ? (byte)3 : value;  // min value is 3
                SetPrescale(v);
                _prescale = v;
            }
        }

        /// <summary>
        /// Constructs Pca9685 instance
        /// </summary>
        /// <param name="i2cDevice">The I2C device to be used</param>
        /// <param name="pwmFrequency">Initial PWM frequency on all channels</param>
        /// <param name="dutyCycleAllChannels">Duty cycle set on all channels after constructing</param>
        /// <param name="usingExternalClock">
        /// When true specifies that external clock is used.
        /// Use <see cref="ClockFrequency"/> to set frequency of an external clock
        /// </param>
        /// <remarks>
        /// Default value for <paramref name="pwmFrequency"/> and <paramref name="dutyCycleAllChannels"/>
        /// is -1 which means to not change value already set on the device.
        /// </remarks>
        public Pca9685(I2cDevice i2cDevice, double pwmFrequency = -1, double dutyCycleAllChannels = -1, bool usingExternalClock = false)
        {
            _device = i2cDevice;
            _usingExternalClock = usingExternalClock;

            Mode1 mode1 = SLEEP | ALLCALL | AI;

            if (usingExternalClock)
            {
                mode1 |= EXTCLK;
            }

            Mode2 mode2 = OUTDRV;

            WriteByte(MODE1, (byte)mode1);
            WriteByte(MODE2, (byte)mode2);

            if (pwmFrequency == -1)
            {
                _prescale = ReadByte(PRESCALE);
            }
            else
            {
                PwmFrequency = pwmFrequency;
            }

            if (dutyCycleAllChannels != -1)
            {
                SetDutyCycleAllChannels(dutyCycleAllChannels);
            }

            mode1 &= ~SLEEP;
            WriteByte(MODE1, (byte)mode1);
            DelayHelper.DelayMicroseconds(500, allowThreadYield: true);
        }

        /// <summary>
        /// Sets duty cycle on specified channel.
        /// </summary>
        /// <param name="channel">Selected channel</param>
        /// <param name="dutyCycle">Value to set duty cycle to</param>
        /// <remarks>Throws <see cref="InvalidOperationException"/> if specified channel is created with <see cref="CreatePwmChannel"/></remarks>
        public void SetDutyCycle(int channel, double dutyCycle)
        {
            CheckChannel(channel);

            if (IsChannelCreated(channel))
            {
                throw new InvalidOperationException("Cannot set duty cycle directly when PwmChannel is created. Use PwmChannel instance instead.");
            }

            SetDutyCycleInternal(channel, dutyCycle);
        }

        /// <summary>
        /// Gets duty cycle on specified channel
        /// </summary>
        /// <param name="channel">selected channel</param>
        /// <returns>Value of duty cycle in 0.0 - 1.0 range</returns>
        public double GetDutyCycle(int channel)
        {
            CheckChannel(channel);

            int offset = 4 * channel;

            ushort on = ReadUInt16((Register)((byte)LED0_ON_L + offset));
            ushort off = ReadUInt16((Register)((byte)LED0_OFF_L + offset));
            return OnOffToDutyCycle(on, off);
        }

        /// <summary>
        /// Sets duty cycles on all channels
        /// </summary>
        /// <param name="dutyCycle">Duty cycle value (0.0 - 1.0)</param>
        /// <remarks>Throws <see cref="InvalidOperationException"/> if any of the channels is created with <see cref="CreatePwmChannel"/></remarks>
        public void SetDutyCycleAllChannels(double dutyCycle)
        {
            if (IsAnyChannelCreated())
            {
                throw new InvalidOperationException("Cannot set duty cycle directly when any of the channels has corresponding PwmChannel instance created.");
            }

            CheckDutyCycle(dutyCycle);
            (ushort on, ushort off) = DutyCycleToOnOff(dutyCycle);
            SetOnOffTimeAllChannels(on, off);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _device?.Dispose();
            _device = null;
        }

        /// <summary>
        /// Creates PwmChannel instance from selected channel
        /// </summary>
        /// <param name="channel">Channel number (0-15)</param>
        /// <returns>PwmChannel instance</returns>
        /// <remarks>Channel is already started when constructed.</remarks>
        public PwmChannel CreatePwmChannel(int channel)
        {
            CheckChannel(channel);

            if (IsChannelCreated(channel))
                throw new ArgumentException("Only one instance of the channel can be created at the same time.", nameof(channel));

            SetChannelAsCreated(channel);

            return new Pca9685PwmChannel(this, channel);
        }

        internal void SetDutyCycleInternal(int channel, double dutyCycle)
        {
            CheckDutyCycle(dutyCycle);

            (ushort on, ushort off) = DutyCycleToOnOff(dutyCycle);
            SetOnOffTime(channel, on, off);
        }

        private void SetOnOffTime(int channel, ushort on, ushort off)
        {
            // on and off are 13-bit values (12 bit value + 1bit full on/off override)
            Debug.Assert((on & 0b1111111111111) == on);
            Debug.Assert((off & 0b1111111111111) == off);
            Debug.Assert((channel & 0xF) == channel);

            int offset = 4 * channel;

            WriteUInt16((Register)((byte)LED0_ON_L + offset), on);
            WriteUInt16((Register)((byte)LED0_OFF_L + offset), off);
        }

        private void SetOnOffTimeAllChannels(ushort on, ushort off)
        {
            // on and off are 13-bit values (12 bit value + 1bit full on/off override)
            Debug.Assert((on & 0b1111111111111) == on);
            Debug.Assert((off & 0b1111111111111) == off);

            WriteUInt16(ALL_LED_ON_L, on);
            WriteUInt16(ALL_LED_OFF_L, off);
        }

        private byte FrequencyToPrescale(double freqHz)
        {
            var desiredPrescale = Math.Round(ClockFrequency / 4096 / freqHz - 1);
            return (byte)Math.Clamp(desiredPrescale, byte.MinValue, byte.MaxValue);
        }

        private double PrescaleToFrequency(byte prescale)
        {
            return ClockFrequency / 4096 / (prescale + 1.0);
        }

        private void SetPrescale(byte prescale)
        {
            Mode1 oldmode = (Mode1)ReadByte(MODE1);

            if (oldmode.HasFlag(SLEEP))
            {
                WriteByte(PRESCALE, prescale);
            }
            else
            {
                WriteByte(MODE1, (byte)(oldmode | SLEEP));
                WriteByte(PRESCALE, prescale);
                WriteByte(MODE1, (byte)oldmode);
                DelayHelper.DelayMicroseconds(500, allowThreadYield: true);
            }
        }

        private ushort _createdChannelsMask = 0;

        private bool IsChannelCreated(int channel)
            => (_createdChannelsMask & (1 << channel)) != 0;

        private bool IsAnyChannelCreated()
            => _createdChannelsMask != 0;

        private void SetChannelAsCreated(int channel)
            => _createdChannelsMask |= (ushort)(1 << channel);

        internal void SetChannelAsDestroyed(int channel)
            => _createdChannelsMask &= (ushort)(~(1 << channel));

        private byte ReadByte(Register register)
        {
            _device.WriteByte((byte)register);
            return _device.ReadByte();
        }

        private ushort ReadUInt16(Register register)
        {
            _device.WriteByte((byte)register);

            Span<byte> bytes = stackalloc byte[2];
            _device.Read(bytes);

            return BinaryPrimitives.ReadUInt16LittleEndian(bytes);
        }

        private void WriteByte(Register register, byte data)
        {
            Span<byte> bytes = stackalloc byte[2];
            bytes[0] = (byte)register;
            bytes[1] = data;
            _device.Write(bytes);
        }

        private void WriteUInt16(Register register, ushort value)
        {
            WriteByte(register, (byte)value);
            WriteByte(register + 1, (byte)(value >> 8));

            Span<byte> bytes = stackalloc byte[3];
            bytes[0] = (byte)register;
            BinaryPrimitives.WriteUInt16LittleEndian(bytes.Slice(1), value);
            _device.Write(bytes);
        }

        private static (ushort on, ushort off) DutyCycleToOnOff(double dutyCycle)
        {
            Debug.Assert(dutyCycle >= 0.0 && dutyCycle <= 1.0);

            // there are actually 4097 values in the set but we can do edge values
            // using 13th bit which overrides to always on/off
            ushort dutyCycleSampled = (ushort)Math.Round(dutyCycle * 4096);

            if (dutyCycleSampled == 0)
            {
                return (0, 1 << 12);
            }
            else if (dutyCycleSampled == 4096)
            {
                return (1 << 12, 0);
            }
            else
            {
                return (0, dutyCycleSampled);
            }
        }

        private static double OnOffToDutyCycle(ushort on, ushort off)
        {
            ushort OnOffToDutyCycleSampled(ushort onCycles, ushort offCycles)
            {
                const ushort Max = (ushort)(1 << 12);
                if (onCycles == 0)
                {
                    return  (offCycles == Max) ? (ushort)0 : offCycles;
                }
                else if (onCycles == Max && offCycles == 0)
                {
                    return 4096;
                }

                // we didn't set this value anywhere in the code
                throw new InvalidOperationException($"Unexpected value of duty cycle ({onCycles}, {offCycles})");
            }

            return OnOffToDutyCycleSampled(on, off) / 4096.0;
        }

        private static void CheckDutyCycle(double dutyCycle)
        {
            if (dutyCycle < 0.0 || dutyCycle > 1.0)
            {
                throw new ArgumentOutOfRangeException(nameof(dutyCycle), dutyCycle, "Value must be between 0.0 and 1.0.");
            }
        }

        private static void CheckChannel(int channel)
        {
            if (channel < 0 || channel >= 16)
            {
                throw new ArgumentOutOfRangeException(nameof(channel), channel, "Channel must be a value from 0 to 15.");
            }
        }
    }
}
