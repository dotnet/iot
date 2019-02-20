// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.I2c;
using System.Threading;
using static Iot.Device.Pca9685.Register;
using static Iot.Device.Pca9685.Bit;

namespace Iot.Device.Pca9685
{
    /// <summary>
    /// PCA9685 PWM LED/servo controller
    /// </summary>
    public class Pca9685 : IDisposable
    {

        /// <summary>
        /// I2C Device
        /// </summary>
        public I2cDevice Device { get; private set; }

        /// <summary>
        /// Get default clock rate. Set if you are using external clock.
        /// </summary>
        public double ClockRate { get; set; } = 25000000;   // 25MHz

        /// <summary>
        /// Set PWM frequency or get effective value.
        /// </summary>
        public double PwmFrequency
        {
            get => _pwmFrequency;
            set
            {
                Prescale = GetPrescale(value);
            }
        }

        /// <summary>
        /// Set PWM frequency using prescale value or get the value.
        /// </summary>
        public byte Prescale
        {
            get => _prescale;
            set
            {
                var v = value < 3 ? (byte)3 : value;  // min value is 3
                SetPwmFrequency(v);
                _prescale = v;
                _pwmFrequency = GetFreq(v);
            }
        }

        private double _pwmFrequency;
        private byte _prescale;

        /// <summary>
        /// Initialize PCA9685
        /// </summary>
        /// <param name="i2cDevice">The I2C device to be used</param>
        public Pca9685(I2cDevice i2cDevice)
        {
            // Setup I2C interface for the device.
            Device = i2cDevice;

            SetPwm(0, 0);

            Span<byte> buffer = stackalloc byte[2];

            buffer[0] = (byte)MODE2;
            buffer[1] = (byte)OUTDRV;
            Device.Write(buffer);

            buffer[0] = (byte)MODE1;
            buffer[1] = (byte)ALLCALL;
            Device.Write(buffer);

            Thread.Sleep(5); // wait for oscillator

            int mode1 = Device.ReadByte();
            mode1 &= ~(byte)SLEEP; // wake up (reset sleep)

            buffer[1] = (byte)mode1;
            Device.Write(buffer);

            Thread.Sleep(5); // wait for oscillator

        }

        /// <summary>
        /// Set a single PWM channel
        /// </summary>
        /// <param name="on">The turn-on time of specfied channel</param>
        /// <param name="off">The turn-off time of specfied channel</param>
        /// <param name="channel">target channel</param>
        public void SetPwm(int on, int off, int channel)
        {
            Span<byte> buffer = stackalloc byte[2];

            buffer[0] = (byte)((byte)LED0_ON_L + 4 * channel);
            buffer[1] = (byte)(on & 0xFF);
            Device.Write(buffer);

            buffer[0] = (byte)((byte)LED0_ON_H + 4 * channel);
            buffer[1] = (byte)(on >> 8);
            Device.Write(buffer);

            buffer[0] = (byte)((byte)LED0_OFF_L + 4 * channel);
            buffer[1] = (byte)(off & 0xFF);
            Device.Write(buffer);

            buffer[0] = (byte)((byte)LED0_OFF_H + 4 * channel);
            buffer[1] = (byte)(off >> 8);
            Device.Write(buffer);
        }

        /// <summary>
        /// Set all PWM channels
        /// </summary>
        /// <param name="on">The turn-on time of all channels</param>
        /// <param name="off">The turn-on time of all channels</param>
        public void SetPwm(int on, int off)
        {
            Span<byte> buffer = stackalloc byte[2];

            buffer[0] = (byte)ALL_LED_ON_L;
            buffer[1] = (byte)(on & 0xFF);
            Device.Write(buffer);

            buffer[0] = (byte)ALL_LED_ON_H;
            buffer[1] = (byte)(on >> 8);
            Device.Write(buffer);

            buffer[0] = (byte)ALL_LED_OFF_L;
            buffer[1] = (byte)(off & 0xFF);
            Device.Write(buffer);

            buffer[0] = (byte)ALL_LED_OFF_H;
            buffer[1] = (byte)(off >> 8);
            Device.Write(buffer);
        }

        public void Dispose()
        {
            Device?.Dispose();
            Device = null;
        }

        /// <summary>
        /// Get prescale of specified PWM frequency
        /// </summary>
        private byte GetPrescale(double freq_hz)
        {

            var prescaleval = ClockRate / 4096 / freq_hz - 1;
            //Debug.Print($"Setting PWM frequency to {freq_hz} Hz");
            //Debug.Print($"Estimated pre-scale: {prescaleval}");

            var prescale = (byte)Math.Round(prescaleval);
            //Debug.Print($"Final pre-scale: {prescale}");

            return prescale;
        }

        /// <summary>
        /// Get PWM frequency of specified prescale
        /// </summary>
        private double GetFreq(byte prescale)
        {
            return ClockRate / 4096 / (prescale + 1);
        }

        /// <summary>
        /// Set PWM frequency by using prescale
        /// </summary>
        private void SetPwmFrequency(byte prescale)
        {
            var oldmode = Device.ReadByte();
            var newmode = (oldmode & 0x7F) | 0x10; // sleep

            Span<byte> buffer = stackalloc byte[2];

            buffer[0] = (byte)MODE1;
            buffer[1] = (byte)newmode;
            Device.Write(buffer); // go to sleep

            buffer[0] = (byte)PRESCALE;
            buffer[1] = prescale;
            Device.Write(buffer);


            buffer[0] = (byte)MODE1;
            buffer[1] = oldmode;
            Device.Write(buffer);

            Thread.Sleep(5);

            buffer[1] = (byte)(oldmode | 0x80);
            Device.Write(buffer);

        }

    }
}
