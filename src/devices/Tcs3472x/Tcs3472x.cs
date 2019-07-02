// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Buffers.Binary;
using System.Device.I2c;
using System.Drawing;
using System.Threading;

namespace Iot.Device.Tcs3472x
{
    public class Tcs3472x : IDisposable
    {
        /// <summary>
        /// Default I2C address for TCS3472x familly
        /// </summary>
        public const byte DefaultI2cAddress = 0x29;
        private I2cDevice _i2cDevice;
        private byte _integrationTimeByte;
        private double _integrationTime;
        private bool _isLongTime;
        private bool _autoDisposable;
        private Gain _gain;

        /// <summary>
        /// Set/Get the time to wait for the sensor to read the data
        /// Minimum time is 0.0024 s
        /// Maximum time is 7.4 s
        /// Be aware that it is not a linear function
        /// </summary>
        public double IntegrationTime
        {
            get { return _integrationTime; }
            set
            {
                _integrationTime = value;
                SetIntegrationTime(_integrationTime);
            }
        }

        /// <summary>
        /// Set/Get the gain
        /// </summary>
        public Gain Gain
        {
            get { return _gain; }
            set
            {
                _gain = value;
                WriteRegister(Registers.CONTROL, (byte)_gain);
            }
        }

        /// <summary>
        /// Get the type of sensor
        /// </summary>
        public TCS3472Type ChipId { get; internal set; }

        /// <summary>
        /// Create a TCS4272x sensor
        /// </summary>
        /// <param name="i2cDevice">The I2C Device class</param>
        /// <param name="integrationTime">The time to wait for sensor to read the data, minimum is 0.024 seconds, maximum in the constructor is 0.7 seconds</param>
        /// <param name="gain">The gain when integrating the color measurement</param>
        /// <param name="autoDisposable">true to dispose the I2C Device class at dispose</param>
        public Tcs3472x(I2cDevice i2cDevice, double integrationTime = 0.0024, Gain gain = Gain.Gain16X, bool autoDisposable = true)
        {
            _i2cDevice = i2cDevice ?? throw new ArgumentNullException(nameof(i2cDevice));
            // Maximum is 700 ms for the initialization. Value can be changed for a long one but not during this initialization phase            
            _autoDisposable = autoDisposable;
            _i2cDevice.WriteByte((byte)(Registers.COMMAND_BIT | Registers.ID));
            ChipId = (TCS3472Type)_i2cDevice.ReadByte();
            _isLongTime = false;
            IntegrationTime = Math.Clamp(integrationTime, 0.0024, 0.7);
            SetIntegrationTime(integrationTime);
            Gain = gain;
            PowerOn();
        }

        /// <summary>
        /// Get true is there are valid data
        /// </summary>
        public bool IsValidData
        {
            get
            {
                _i2cDevice.WriteByte((byte)(Registers.COMMAND_BIT | Registers.STATUS));
                var stat = _i2cDevice.ReadByte();
                return ((Registers)(stat & (byte)Registers.STATUS_AVALID) == Registers.STATUS_AVALID);
            }
        }

        /// <summary>
        /// Get true if RGBC is clear channel interrupt
        /// </summary>        
        public bool IsClearInterrupt
        {
            get
            {
                _i2cDevice.WriteByte((byte)(Registers.COMMAND_BIT | Registers.STATUS));
                var stat = _i2cDevice.ReadByte();
                return ((Registers)(stat & (byte)Registers.STATUS_AINT) == Registers.STATUS_AINT);
            }
        }

        /// <summary>
        /// Set the integration (sampling) time for the sensor
        /// </summary>
        /// <param name="timeSeconds">Time in seconds for each sample. 0.0024 second(2.4ms) increments.Clipped to the range of 0.0024 to 0.6144 seconds.</param>
        private void SetIntegrationTime(double timeSeconds)
        {
            if (timeSeconds <= 700)
            {
                if (_isLongTime)
                {
                    SetConfigLongTime(false);
                }
                _isLongTime = false;
                var timeByte = Math.Clamp((int)(0x100 - (timeSeconds / 0.0024)), 0, 255);
                WriteRegister(Registers.ATIME, (byte)timeByte);
                _integrationTimeByte = (byte)timeByte;
            }
            else
            {
                if (!_isLongTime)
                {
                    SetConfigLongTime(true);
                }
                _isLongTime = true;
                var timeByte = (int)(0x100 - (timeSeconds / 0.029));
                timeByte = Math.Clamp(timeByte, 0, 255);
                WriteRegister(Registers.WTIME, (byte)timeByte);
                _integrationTimeByte = (byte)timeByte;
            }
        }

        private void SetConfigLongTime(bool setLong)
        {
            WriteRegister(Registers.CONFIG, setLong ? (byte)(Registers.CONFIG_WLONG) : (byte)0x00);
        }

        private void PowerOn()
        {

            WriteRegister(Registers.ENABLE, (byte)Registers.ENABLE_PON);
            Thread.Sleep(10);
            WriteRegister(Registers.ENABLE, (byte)(Registers.ENABLE_PON | Registers.ENABLE_AEN));
        }

        private void PowerOff()
        {
            var powerState = I2cRead8(Registers.ENABLE);
            powerState = (byte)(powerState & ~(byte)(Registers.ENABLE_PON | Registers.ENABLE_AEN));
            WriteRegister(Registers.ENABLE, powerState);
        }

        /// <summary>
        /// Set/Clear the colors and clear interrupts
        /// </summary>
        /// <param name="state">true to set all interrupts, false to clear</param>
        public void SetInterrupt(bool state)
        {
            SetInterrupt(InterruptState.All, state);
        }

        /// <summary>
        /// Set/clear a specific interrupt persistence
        /// This is used to have more than 1 cycle before generating an
        /// interruption. 
        /// </summary>
        /// <param name="interupt">The percistence cycles</param>
        /// <param name="state">True to set the interrupt, false to clear</param>
        public void SetInterrupt(InterruptState interupt, bool state)
        {
            WriteRegister(Registers.PERS, (byte)interupt);
            var enable = I2cRead8(Registers.ENABLE);
            enable = state ? enable |= (byte)Registers.ENABLE_AIEN : enable = (byte)(enable & ~(byte)Registers.ENABLE_AIEN);
            WriteRegister(Registers.ENABLE, enable);
        }

        /// <summary>
        /// Get the color
        /// </summary>
        /// <param name="delay">Wait to read the data that the integration time is passed</param>
        /// <returns></returns>
        public Color GetColor(bool delay = true)
        {
            // To have a new reading, you need to wait for integration time to happen
            // If you don't wait, then you'll read the previous value
            if (delay)
                Thread.Sleep((int)(IntegrationTime * 1000));
            var divide = ((256 - _integrationTimeByte) * 1024);
            // If we are in long wait, we'll need to divide even more
            if (_isLongTime)
                divide *= 12;
            int r = (int)(I2cRead16(Registers.RDATAL) * 255 / divide);
            r = Math.Clamp(r, 0, 255);
            int g = (int)(I2cRead16(Registers.GDATAL) * 255 / divide);
            g = Math.Clamp(g, 0, 255);
            int b = (int)(I2cRead16(Registers.BDATAL) * 255 / divide);
            b = Math.Clamp(b, 0, 255);
            int a = (int)(I2cRead16(Registers.CDATAL) * 255 / divide);
            a = Math.Clamp(a, 0, 255);
            return Color.FromArgb(a, r, g, b);
        }

        /// <summary>
        /// Get the color
        /// </summary>
        public Color Color => GetColor();

        private ushort I2cRead16(Registers reg)
        {
            _i2cDevice.WriteByte((byte)(Registers.COMMAND_BIT | reg));
            Span<byte> outArray = stackalloc byte[2] { 0, 0 };
            _i2cDevice.Read(outArray);
            return BinaryPrimitives.ReadUInt16BigEndian(outArray);
        }

        private byte I2cRead8(Registers reg)
        {
            _i2cDevice.WriteByte((byte)(Registers.COMMAND_BIT | reg));
            return _i2cDevice.ReadByte();
        }

        private void WriteRegister(Registers reg, byte data)
        {
            _i2cDevice.Write(new byte[] { (byte)(Registers.COMMAND_BIT | reg), data });
        }

        public void Dispose()
        {
            PowerOff();
            if (_autoDisposable)
            {
                _i2cDevice?.Dispose();
                _i2cDevice = null;
            }
        }
    }
}
