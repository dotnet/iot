// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.I2c;
using System.Threading;
using static Iot.Device.Pca9685.Register;
using static Iot.Device.Pca9685.Mode1;
using static Iot.Device.Pca9685.Mode2;

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
        private I2cDevice _device;

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
            _device = i2cDevice;

            SetPwm(0, 0);

            Span<byte> buffer = stackalloc byte[2] { (byte)MODE2, (byte)OUTDRV };
            _device.Write(buffer);

            buffer = stackalloc byte[2] { (byte)MODE1, (byte)ALLCALL };
            _device.Write(buffer);

            Thread.Sleep(5); // wait for oscillator

            int mode1 = _device.ReadByte();
            mode1 &= ~(byte)SLEEP; // wake up (reset sleep)

            buffer = stackalloc byte[2] { (byte)MODE1, (byte)mode1 };
            _device.Write(buffer);

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
            on &= 0xFFF;
            off &= 0xFFF;
            channel &= 0xF;

            Span<byte> buffer = stackalloc byte[2] { (byte)((byte)LED0_ON_L + 4 * channel), (byte)on };
            _device.Write(buffer);

            buffer = stackalloc byte[2] { (byte)((byte)LED0_ON_H + 4 * channel), (byte)(on >> 8) };
            _device.Write(buffer);

            buffer = stackalloc byte[2] { (byte)((byte)LED0_OFF_L + 4 * channel), (byte)off };
            _device.Write(buffer);

            buffer = stackalloc byte[2] { (byte)((byte)LED0_OFF_H + 4 * channel), (byte)(off >> 8) };
            _device.Write(buffer);
        }

        /// <summary>
        /// Set all PWM channels
        /// </summary>
        /// <param name="on">The turn-on time of all channels</param>
        /// <param name="off">The turn-on time of all channels</param>
        public void SetPwm(int on, int off)
        {
            on &= 0xFFF;
            off &= 0xFFF;

            Span<byte> buffer = stackalloc byte[2] { (byte)ALL_LED_ON_L, (byte)on };
            _device.Write(buffer);

            buffer = stackalloc byte[2] { (byte)ALL_LED_ON_H, (byte)(on >> 8) };
            _device.Write(buffer);

            buffer = stackalloc byte[2] { (byte)ALL_LED_OFF_L, (byte)off };
            _device.Write(buffer);

            buffer = stackalloc byte[2] { (byte)ALL_LED_OFF_H, (byte)(off >> 8) };
            _device.Write(buffer);
        }

        public void Dispose()
        {
            _device?.Dispose();
            _device = null;
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
            var oldmode = _device.ReadByte();
            var newmode = (sbyte)oldmode | (byte)SLEEP;

            Span<byte> buffer = stackalloc byte[2] { (byte)MODE1, (byte)newmode };
            _device.Write(buffer); // go to sleep

            buffer = stackalloc byte[2] { (byte)PRESCALE, prescale };
            _device.Write(buffer);


            buffer = stackalloc byte[2] { (byte)MODE1, oldmode };
            _device.Write(buffer);

            Thread.Sleep(5);

            buffer = stackalloc byte[2] { (byte)MODE1, (byte)(oldmode | (byte)RESTART) };
            _device.Write(buffer);

        }

    }
}
