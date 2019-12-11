// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Buffers.Binary;
using System.Device.I2c;
using System.Drawing;
using System.Linq;

namespace Iot.Device.Bh1745
{
    /// <summary>
    /// Digital color sensor Bh1745.
    /// </summary>
    public class Bh1745 : IDisposable
    {
        private byte ManufacturerId => Read8BitsFromRegister((byte)Register.MANUFACTURER_ID);
        private byte PartId => (byte)(Read8BitsFromRegister((byte)Register.SYSTEM_CONTROL) & (byte)Mask.PART_ID);

        private I2cDevice _i2cDevice;
        private MeasurementTime _measurementTime;
        private bool _measurementIsActive;
        private AdcGain _adcGain;
        private LatchBehavior _latchBehavior;
        private InterruptSource _interruptSource;
        private bool _interruptIsEnabled;
        private InterruptPersistence _interruptPersistence;
        private ushort _lowerInterruptThreshold;
        private ushort _higherInterruptThreshold;

        /// <summary>
        /// Digital color sensor Bh1745.
        /// </summary>
        /// <param name="device">The used I2c communication device.</param>
        public Bh1745(I2cDevice device)
        {
            _i2cDevice = device;
            ChannelCompensationMultipliers = new ChannelCompensationMultipliers
            {
                Red = 2.2, Green = 1.0, Blue = 1.8, Clear = 10.0
            };

            // reset device and set default configuration
            InitDevice();
        }

        /// <summary>
        /// The primary I2c address of the BH1745
        /// </summary>
        public const byte DefaultI2cAddress = 0x38;

        /// <summary>
        /// The secondary I2c address of the BH1745
        /// </summary>
        public const byte SecondaryI2cAddress = 0x39;

        /// <summary>
        /// Gets or sets the state of the interrupt pin.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if invalid InterruptStatus is set.</exception>
        public InterruptStatus InterruptReset
        {
            get
            {
                var intReset = Read8BitsFromRegister((byte)Register.SYSTEM_CONTROL);
                intReset = (byte)((intReset & (byte)Mask.INT_RESET) >> 6);
                return (InterruptStatus)intReset;
            }
            set
            {
                if (!Enum.IsDefined(typeof(InterruptStatus), value))
                {
                    throw new ArgumentOutOfRangeException();
                }

                var intReset = Read8BitsFromRegister((byte)Register.SYSTEM_CONTROL);
                intReset = (byte)((intReset & (byte)~Mask.INT_RESET) | (byte)value << 6);

                Write8BitsToRegister((byte)Register.SYSTEM_CONTROL, intReset);
            }
        }

        /// <summary>
        /// Gets or sets the currently set measurement time.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if invalid MeasurementTime is set.</exception>
        public MeasurementTime MeasurementTime
        {
            get => _measurementTime;
            set
            {
                if (!Enum.IsDefined(typeof(MeasurementTime), value))
                {
                    throw new ArgumentOutOfRangeException();
                }

                var time = Read8BitsFromRegister((byte)Register.MODE_CONTROL1);
                time = (byte)((time & (byte)~Mask.MEASUREMENT_TIME) | (byte)value);

                Write8BitsToRegister((byte)Register.MODE_CONTROL1, time);
                _measurementTime = value;
            }
        }

        /// <summary>
        /// Gets or sets whether the measurement is active.
        /// </summary>
        public bool MeasurementIsActive
        {
            get => _measurementIsActive;
            set
            {
                var active = Read8BitsFromRegister((byte)Register.MODE_CONTROL2);
                active = (byte)((active & (byte)~Mask.RGBC_EN) | Convert.ToByte(value) << 4);

                Write8BitsToRegister((byte)Register.MODE_CONTROL2, active);
                _measurementIsActive = value;
            }
        }

        /// <summary>
        /// Gets or sets the adc gain of the sensor.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if invalid AdcGain is set.</exception>
        public AdcGain AdcGain
        {
            get => _adcGain;
            set
            {
                if (!Enum.IsDefined(typeof(AdcGain), value))
                {
                    throw new ArgumentOutOfRangeException();
                }

                var adcGain = Read8BitsFromRegister((byte)Register.MODE_CONTROL2);
                adcGain = (byte)((adcGain & (byte)~Mask.ADC_GAIN) | (byte)value);

                Write8BitsToRegister((byte)Register.MODE_CONTROL2, adcGain);
                _adcGain = value;
            }
        }

        /// <summary>
        /// Gets whether the interrupt signal is active.
        /// </summary>
        public bool InterruptSignalIsActive
        {
            get
            {
                var intStatus = Read8BitsFromRegister((byte)Register.INTERRUPT);
                intStatus = (byte)((intStatus & (byte)Mask.INT_STATUS) >> 7);
                return Convert.ToBoolean(intStatus);
            }
        }

        /// <summary>
        /// Gets or sets how the interrupt pin latches.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if invalid LatchBehavior is set.</exception>
        public LatchBehavior LatchBehavior
        {
            get => _latchBehavior;
            set
            {
                if (!Enum.IsDefined(typeof(LatchBehavior), value))
                {
                    throw new ArgumentOutOfRangeException();
                }

                var intLatch = Read8BitsFromRegister((byte)Register.INTERRUPT);
                intLatch = (byte)((intLatch & (byte)~Mask.INT_LATCH) | (byte)value << 4);

                Write8BitsToRegister((byte)Register.INTERRUPT, intLatch);
                _latchBehavior = value;
            }
        }

        /// <summary>
        /// Gets or sets the source channel of the interrupt.
        /// </summary>
        public InterruptSource InterruptSource
        {
            get => _interruptSource;
            set
            {
                if (!Enum.IsDefined(typeof(InterruptSource), value))
                {
                    throw new ArgumentOutOfRangeException();
                }

                var intSource = Read8BitsFromRegister((byte)Register.INTERRUPT);
                intSource = (byte)((intSource & (byte)~Mask.INT_SOURCE) | (byte)value << 2);

                Write8BitsToRegister((byte)Register.INTERRUPT, intSource);
                _interruptSource = value;
            }
        }

        /// <summary>
        /// Gets or sets whether the interrupt pin is enabled.
        /// </summary>
        public bool InterruptIsEnabled
        {
            get => _interruptIsEnabled;
            set
            {
                var intPin = Read8BitsFromRegister((byte)Register.INTERRUPT);
                intPin = (byte)((intPin & (byte)~Mask.INT_ENABLE) | Convert.ToByte(value));

                Write8BitsToRegister((byte)Register.INTERRUPT, intPin);
                _interruptIsEnabled = value;
            }
        }

        /// <summary>
        /// Gets or sets the persistence function of the interrupt.
        /// </summary>
        public InterruptPersistence InterruptPersistence
        {
            get => _interruptPersistence;
            set
            {
                if (!Enum.IsDefined(typeof(InterruptPersistence), value))
                {
                    throw new ArgumentOutOfRangeException();
                }

                var intPersistence = Read8BitsFromRegister((byte)Register.PERSISTENCE);
                intPersistence = (byte)((intPersistence & (byte)~Mask.PERSISTENCE) | (byte)value);

                Write8BitsToRegister((byte)Register.PERSISTENCE, intPersistence);
                _interruptPersistence = value;
            }
        }

        /// <summary>
        /// Gets or sets the lower interrupt threshold.
        /// </summary>
        public ushort LowerInterruptThreshold
        {
            get => _lowerInterruptThreshold;
            set
            {
                WriteShortToRegister((byte)Register.TL, value);
                _lowerInterruptThreshold = value;
            }
        }

        /// <summary>
        /// Gets or sets the higher interrupt threshold.
        /// </summary>
        public ushort HigherInterruptThreshold
        {
            get => _higherInterruptThreshold;
            set
            {
                WriteShortToRegister((byte)Register.TH, value);
                _higherInterruptThreshold = value;
            }
        }

        /// <summary>
        /// Gets or sets the channel compensation multipliers which are used to compensate the measurements.
        /// </summary>
        public ChannelCompensationMultipliers ChannelCompensationMultipliers { get; set; }

        /// <summary>
        /// Resets the device to the default configuration.
        /// On reset the sensor goes to power down mode.
        /// </summary>
        public void Reset()
        {
            var status = Read8BitsFromRegister((byte)Register.SYSTEM_CONTROL);
            status = (byte)((status & (byte)~Mask.SW_RESET) | 0x01 << 7);

            Write8BitsToRegister((byte)Register.SYSTEM_CONTROL, status);

            // set default measurement configuration
            MeasurementTime = MeasurementTime.Ms160;
            AdcGain = AdcGain.X1;
            MeasurementIsActive = true;
            InterruptIsEnabled = true;

            // set fields to reset state
            _interruptPersistence = InterruptPersistence.UpdateMeasurementEnd;
            _latchBehavior = LatchBehavior.LatchUntilReadOrInitialized;
            _interruptSource = InterruptSource.RedChannel;
            _interruptIsEnabled = false;
            _lowerInterruptThreshold = 0x0000;
            _higherInterruptThreshold = 0xFFFF;

            // write default value to Mode_Control3
            Write8BitsToRegister((byte)Register.MODE_CONTROL3, 0x02);
        }

        /// <summary>
        /// Reads whether the last measurement is valid.
        /// </summary>
        public bool ReadMeasurementIsValid()
        {
            var valid = Read8BitsFromRegister((byte)Register.MODE_CONTROL2);
            valid = (byte)(valid & (byte)Mask.VALID);
            return Convert.ToBoolean(valid);
        }

        /// <summary>
        /// Reads the red data register of the sensor.
        /// </summary>
        /// <returns></returns>
        public ushort ReadRedDataRegister() => Read16BitsFromRegister((byte)Register.RED_DATA);

        /// <summary>
        /// Reads the green data register of the sensor.
        /// </summary>
        /// <returns></returns>
        public ushort ReadGreenDataRegister() => Read16BitsFromRegister((byte)Register.GREEN_DATA);

        /// <summary>
        /// Reads the blue data register of the sensor.
        /// </summary>
        /// <returns></returns>
        public ushort ReadBlueDataRegister() => Read16BitsFromRegister((byte)Register.BLUE_DATA);

        /// <summary>
        /// Reads the clear data register of the sensor.
        /// </summary>
        /// <returns></returns>
        public ushort ReadClearDataRegister() => Read16BitsFromRegister((byte)Register.CLEAR_DATA);

        /// <summary>
        /// Gets the compensated color reading from the sensor.
        /// </summary>
        /// <returns></returns>
        public Color GetCompensatedColor()
        {
            var clearDataRaw = ReadClearDataRegister();
            if (clearDataRaw == 0)
            {
                return Color.FromArgb(0, 0, 0);
            }

            // apply channel multipliers and normalize
            double compensatedRed = ReadRedDataRegister() * ChannelCompensationMultipliers.Red / MeasurementTime.ToMilliseconds() * 360;
            double compensatedGreen = ReadGreenDataRegister() * ChannelCompensationMultipliers.Green / MeasurementTime.ToMilliseconds() * 360;
            double compensatedBlue = ReadBlueDataRegister() * ChannelCompensationMultipliers.Blue / MeasurementTime.ToMilliseconds() * 360;
            double compensatedClear = clearDataRaw * ChannelCompensationMultipliers.Clear / MeasurementTime.ToMilliseconds() * 360;

            // scale against clear channel
            int redScaled = (int)Math.Min(255, compensatedRed / compensatedClear * 255);
            int greenScaled = (int)Math.Min(255, compensatedGreen / compensatedClear * 255);
            int blueScaled = (int)Math.Min(255, compensatedBlue / compensatedClear * 255);

            return Color.FromArgb(redScaled, greenScaled, blueScaled);
        }

        private void InitDevice()
        {
            // check manufacturer and part Id
            if (ManufacturerId != 0xE0)
            {
                throw new Exception($"Manufacturer ID {ManufacturerId} is not the same as expected 224. Please check if you are using the right device.");
            }

            if (PartId != 0x0b)
            {
                throw new Exception($"Part ID {PartId} is not the same as expected 11. Please check if you are using the right device.");
            }

            // soft reset sensor
            Reset();
        }

        private byte Read8BitsFromRegister(byte register)
        {
            _i2cDevice.WriteByte(register);
            var value = _i2cDevice.ReadByte();
            return value;
        }

        private ushort Read16BitsFromRegister(byte register)
        {
            Span<byte> bytes = stackalloc byte[2];

            _i2cDevice.WriteByte(register);
            _i2cDevice.Read(bytes);

            return BinaryPrimitives.ReadUInt16LittleEndian(bytes);
        }

        private void WriteShortToRegister(byte register, ushort value)
        {
            var bytes = new byte[3];
            var source = !BitConverter.IsLittleEndian
                ? BitConverter.GetBytes(value).Reverse().ToArray()
                : BitConverter.GetBytes(value);

            bytes[0] = register;
            Buffer.BlockCopy(source, 0, bytes, 1, source.Length);

            _i2cDevice.Write(bytes);
        }

        private void Write8BitsToRegister(byte register, byte data)
        {
            Span<byte> command = stackalloc[]
            {
                register, data
            };
            _i2cDevice.Write(command);
        }

        /// <summary>
        /// Disposes the Bh1745 resources.
        /// </summary>
        public void Dispose()
        {
            _i2cDevice?.Dispose();
            _i2cDevice = null;
        }
    }
}
