// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.I2c;
using System.Device.I2c.Drivers;
using System.Threading;

namespace Iot.Device.Pca9685
{
    /// <summary>
    /// PCA9685 PWM LED/servo controller
    /// </summary>
    public class Pca9685 : IDisposable
    {
        // Registers/etc:
        const int PCA9685_ADDRESS = 0x40;
        const int MODE1 = 0x00;
        const int MODE2 = 0x01;
        const int SUBADR1 = 0x02;
        const int SUBADR2 = 0x03;
        const int SUBADR3 = 0x04;
        const int PRESCALE = 0xFE;
        const int LED0_ON_L = 0x06;
        const int LED0_ON_H = 0x07;
        const int LED0_OFF_L = 0x08;
        const int LED0_OFF_H = 0x09;
        const int ALL_LED_ON_L = 0xFA;
        const int ALL_LED_ON_H = 0xFB;
        const int ALL_LED_OFF_L = 0xFC;
        const int ALL_LED_OFF_H = 0xFD;

        // Bits:
        const int RESTART = 0x80;
        const int SLEEP = 0x10;
        const int ALLCALL = 0x01;
        const int INVRT = 0x10;
        const int OUTDRV = 0x04;

        // I2C Device
        private readonly I2cDevice device;

        public int Address { get; private set; }
        public int BusId { get; private set; }
        public double ClockRate { get; set; } = 25000000;

        /// <summary>
        /// Initialize PCA9685
        /// </summary>
        public Pca9685(int address = PCA9685_ADDRESS, int busId = 1)
        {
            Address = address;
            BusId = busId;

            // Setup I2C interface for the device.
            {
                var settings = new I2cConnectionSettings(busId, address);
                try
                {
                    device = new UnixI2cDevice(settings);
                }
                catch
                {
                    try
                    {
                        device = new Windows10I2cDevice(settings);
                    }
                    catch
                    {
                        throw new NotImplementedException();
                    }
                }
            }

            Init();

        }

        /// <summary>
        /// Initialize PCA9685
        /// </summary>
        public Pca9685(I2cDevice i2cDevice)
        {
            Address = i2cDevice.ConnectionSettings.DeviceAddress;
            BusId = i2cDevice.ConnectionSettings.BusId;

            // Setup I2C interface for the device.
            device = i2cDevice;

            Init();
        }

        private void Init()
        {

            SetPwm(0, 0);
            device.Write(new byte[] { MODE2, OUTDRV });
            device.Write(new byte[] { MODE1, ALLCALL });
            Thread.Sleep(5); // wait for oscillator

            int mode1 = device.ReadByte();
            mode1 = mode1 & ~SLEEP; // wake up (reset sleep)
            device.Write(new byte[] { MODE1, (byte)mode1 });
            Thread.Sleep(5); // wait for oscillator

        }

        /// <summary>
        /// Set PWM frequency
        /// </summary>
        public void SetPwmFrequency(double freq_hz)
        {
            SetPwmFrequency(GetPrescale(freq_hz));
        }

        /// <summary>
        /// Get prescale of specified PWM frequency
        /// </summary>
        public byte GetPrescale(double freq_hz)
        {
            //var prescaleval = 25000000.0; // 25MHz
            //prescaleval /= 4096.0; // 12-bit
            //prescaleval /= freq_hz;
            //prescaleval -= 1.0;

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
        public double GetFreq(byte prescale)
        {
            return ClockRate / 4096 / (prescale + 1);
        }

        /// <summary>
        /// Set PWM frequency by using prescale
        /// </summary>
        public void SetPwmFrequency(byte prescale)
        {
            var oldmode = device.ReadByte();
            var newmode = (oldmode & 0x7F) | 0x10; // sleep
            device.Write(new byte[] { MODE1, (byte)newmode }); // go to sleep
            device.Write(new byte[] { PRESCALE, prescale });
            device.Write(new byte[] { MODE1, oldmode });
            Thread.Sleep(5);

            device.Write(new byte[] { MODE1, (byte)(oldmode | 0x80) });
        }

        /// <summary>
        /// Set a single PWM channel
        /// </summary>
        public void SetPwm(int on, int off, int channel)
        {
            device.Write(new byte[] { (byte)(LED0_ON_L + 4 * channel), (byte)(on & 0xFF) });
            device.Write(new byte[] { (byte)(LED0_ON_H + 4 * channel), (byte)(on >> 8) });
            device.Write(new byte[] { (byte)(LED0_OFF_L + 4 * channel), (byte)(off & 0xFF) });
            device.Write(new byte[] { (byte)(LED0_OFF_H + 4 * channel), (byte)(off >> 8) });
        }

        /// <summary>
        /// Set all PWM channels
        /// </summary>
        public void SetPwm(int on, int off)
        {
            device.Write(new byte[] { (byte)ALL_LED_ON_L, (byte)(on & 0xFF) });
            device.Write(new byte[] { (byte)ALL_LED_ON_H, (byte)(on >> 8) });
            device.Write(new byte[] { (byte)ALL_LED_OFF_L, (byte)(off & 0xFF) });
            device.Write(new byte[] { (byte)ALL_LED_OFF_H, (byte)(off >> 8) });
        }

        public void Dispose()
        {
            device.Dispose();
        }
    }
}
