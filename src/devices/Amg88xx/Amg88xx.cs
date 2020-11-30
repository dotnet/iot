// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Device.I2c;
using UnitsNet;

namespace Iot.Device.Amg88xx
{
    /// <summary>
    /// Binding for the AMG88xx family of infrared array sensors
    /// </summary>
    public class Amg88xx : IDisposable
    {
        /// <summary>
        /// Standard device address
        /// (AD_SELECT pin is low, c.f. reference specification, pg. 11)
        /// </summary>
        public const int DeafultI2cAddress = 0x68;

        /// <summary>
        /// Alternative device address
        /// (AD_SELECT pin is high, c.f. reference specification, pg. 11)
        /// </summary>
        public const int AlternativeI2cAddress = 0x69;

        /// <summary>
        /// Number of sensor pixel array columns
        /// </summary>
        public const int Width = 0x8;

        /// <summary>
        /// Number of sensor pixel array rows
        /// </summary>
        public const int Height = 0x8;

        /// <summary>
        /// Total number of pixels.
        /// </summary>
        public const int PixelCount = Width * Height;

        /// <summary>
        /// Number of bytes per pixel
        /// </summary>
        private const int BytesPerPixel = 2;

        /// <summary>
        /// Temperature resolution of thermistor (in degrees Celsius)
        /// </summary>
        private const double ThermistorTemperatureResolution = 0.0625;

        /// <summary>
        /// Internal storage for the most recently image read from the sensor
        /// </summary>
        private readonly byte[] _imageData = new byte[PixelCount * BytesPerPixel];

        private I2cDevice _i2cDevice;

        /// <summary>
        /// Initializes a new instance of the <see cref="Amg88xx"/> binding.
        /// </summary>
        public Amg88xx(I2cDevice i2cDevice)
        {
            _i2cDevice = i2cDevice ?? throw new ArgumentNullException(nameof(i2cDevice));
        }

        #region Infrared sensor

        /// <summary>
        /// Gets temperature of the specified pixel from the current thermal image.
        /// </summary>
        /// <param name="x">The x-coordinate of the pixel to retrieve.</param>
        /// <param name="y">The y-coordinate of the pixel to retrieve.</param>
        /// <exception cref="ArgumentException">x is less than 0, or greater than or equal to Width.</exception>
        /// <exception cref="ArgumentException">y is less than 0, or greater than or equal to Height.</exception>
        /// <returns>Temperature of the specified pixel.</returns>
        public Temperature this[int x, int y]
        {
            get
            {
                if (x < 0 || x >= Width)
                {
                    throw new ArgumentOutOfRangeException(nameof(x));
                }

                if (y < 0 || y >= Height)
                {
                    throw new ArgumentOutOfRangeException(nameof(y));
                }

                Span<byte> buffer = _imageData;
                return Amg88xxUtils.ConvertToTemperature(buffer.Slice(BytesPerPixel * (Width * y + x), BytesPerPixel));
            }
        }

        /// <summary>
        /// Gets temperature for all pixels from the current thermal image as a two-dimensional array.
        /// First index specifies the x-coordinate of the pixel and second index specifies y-coordinate of the pixel.
        /// </summary>
        /// <returns>Temperature as a two-dimensional array.</returns>
        public Temperature[,] TemperatureMatrix
        {
            get
            {
                Span<byte> buffer = _imageData;
                Temperature[,] temperatureMatrix = new Temperature[Width, Height];
                for (int y = 0; y < Height; y++)
                {
                    for (int x = 0; x < Width; x++)
                    {
                        temperatureMatrix[x, y] = this[x, y];
                    }
                }

                return temperatureMatrix;
            }
        }

        /// <summary>
        /// Gets raw reading (12-bit two's complement format) of the specified pixel from the current thermal image.
        /// </summary>
        /// <param name="n">The number of the pixel to retrieve.</param>
        /// <exception cref="ArgumentException">n is less than 0, or greater than or equal to PixelCount.</exception>
        /// <returns>Reading of the specified pixel.</returns>
        public Int16 this[int n]
        {
            get
            {
                if (n < 0 || n >= PixelCount)
                {
                    throw new ArgumentOutOfRangeException(nameof(n));
                }

                Span<byte> buffer = _imageData;
                return BinaryPrimitives.ReadInt16LittleEndian(buffer.Slice(n * BytesPerPixel, BytesPerPixel));
            }
        }

        /// <summary>
        /// Reads the current image from the sensor
        /// </summary>
        public void ReadImage()
        {
            // the readout process gets triggered by writing to pixel 0 of the sensor w/o any additional data
            _i2cDevice.WriteByte((byte)Register.T01L);
            _i2cDevice.Read(_imageData);
        }

        /// <summary>
        /// Gets the temperature reading from the internal thermistor.
        /// </summary>
        /// <value>Temperature reading</value>
        public Temperature SensorTemperature
        {
            get
            {
                byte tthl = GetRegister(Register.TTHL);
                byte tthh = GetRegister(Register.TTHH);

                int reading = (tthh & 0x7) << 8 | tthl;
                reading = tthh >> 3 == 0 ? reading : -reading;

                // The LSB is equivalent to 0.0625℃.
                return Temperature.FromDegreesCelsius(reading * ThermistorTemperatureResolution);
            }
        }

        #endregion

        #region Status

        /// <summary>
        /// Gets whether any pixel measured a temperature higher than the normal operation range.
        /// The event of an overflow does not prevent from continuing reading the sensor.
        /// The overflow indication will last even if all pixels are returned to readings within normal range.
        /// The indicator is reset using <see cfref="ClearTemperatureOverflow"/>.
        /// </summary>
        /// <returns>True, if an overflow occured</returns>
        public bool HasTemperatureOverflow()
        {
            return GetBit(Register.STAT, (byte)StatusFlagBit.OVF_IRS);
        }

        /// <summary>
        /// Clears the temperature overflow indication.
        /// </summary>
        public void ClearTemperatureOverflow()
        {
            // only the bit to be cleared is set, the other bits need to be 0
            SetRegister(Register.SCLR, 1 << (byte)StatusClearBit.OVFCLR);
        }

        /// <summary>
        /// Gets the thermistor overflow flag from the status register.
        /// The overflow indication will last even if the thermistor temperature returned to normal range.
        /// The event of an overflow does not prevent from continuing reading the sensor.
        /// The indicator is reset using <see cfref="ClearThermistorOverflow"/>.
        /// Note: the bit is only menthioned in early versions of the reference specification.
        /// It is not clear whether this is a specification error or a change in a newer
        /// revision of the sensor.
        /// </summary>
        /// <returns>True, if an overflow occured</returns>
        public bool HasThermistorOverflow()
        {
            return GetBit(Register.STAT, (byte)StatusFlagBit.OVF_THS);
        }

        /// <summary>
        /// Clears the temperature overflow indication.
        /// </summary>
        public void ClearThermistorOverflow()
        {
            // only the bit to be cleared is set, the other bits need to be 0
            SetRegister(Register.SCLR, 1 << (byte)StatusClearBit.OVFTHCLR);
        }

        /// <summary>
        /// Gets the interrupt flag from the status register
        /// </summary>
        /// <returns>Interrupt flag</returns>
        public bool HasInterrupt()
        {
            return GetBit(Register.STAT, (byte)StatusFlagBit.INTF);
        }

        /// <summary>
        /// Clears the interrupt flag in the status register
        /// </summary>
        public void ClearInterrupt()
        {
            // only the bit to be cleared is set, the other bits need to be 0
            SetRegister(Register.SCLR, 1 << (byte)StatusClearBit.INTCLR);
        }

        /// <summary>
        /// Clears all flags in the status register.
        /// Note: it does not clear the interrupt flags of the individual pixels.
        /// </summary>
        public void ClearAllFlags()
        {
            // only the bit to be cleared is set, the other bits need to be 0
            SetRegister(Register.SCLR, (1 << (byte)StatusClearBit.OVFCLR) | (1 << (byte)StatusClearBit.OVFTHCLR) | (1 << (byte)StatusClearBit.INTCLR));
        }

        #endregion

        #region Moving average

        /// <summary>
        /// Get or sets the state of the moving average mode
        /// Important: the reference specification states that the current mode can be read,
        /// but it doesn't seem to work at the time being.
        /// In this case the property is always read as ```false```.
        /// </summary>
        /// <value>True if the moving average should be calculated; otherwise, false. The default is false.</value>
        public bool UseMovingAverageMode
        {
            get => GetBit(Register.AVE, (byte)MovingAverageModeBit.MAMOD);
            set => SetBit(Register.AVE, (byte)MovingAverageModeBit.MAMOD, value);
        }

        #endregion

        #region Frame Rate

        /// <summary>
        /// Get or sets the frame rate of the sensor internal thermal image update.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown when attempting to set a frame rate other than 1 or 10 frames per second</exception>
        /// <value>The frame rate for the pixel update interval (either 1 or 10fps). The default is 10fps.</value>
        public FrameRate FrameRate
        {
            get => GetBit(Register.FPSC, (byte)FrameRateBit.FPS) ? FrameRate.Rate1FramePerSecond : FrameRate.Rate10FramesPerSecond;
            set
            {
                if (value != FrameRate.Rate1FramePerSecond && value != FrameRate.Rate10FramesPerSecond)
                {
                    throw new ArgumentException("Frame rate must either be 1 or 10.");
                }

                SetBit(Register.FPSC, (byte)FrameRateBit.FPS, value == FrameRate.Rate1FramePerSecond);
            }
        }

        #endregion

        #region Operating Mode / Power Control

        /// <summary>
        /// Gets or sets the current operating mode
        /// Refer to the sensor reference specification for a description of the mode
        /// depending sensor bevaviour and the valid mode transistions.
        /// </summary>
        /// <value>The operating mode of the sensor. The default is Normal.</value>
        public OperatingMode OperatingMode
        {
            get => (OperatingMode)GetRegister(Register.PCLT);
            set => SetRegister(Register.PCLT, (byte)value);
        }

        #endregion

        #region Reset

        /// <summary>
        /// Performs an reset of the sensor. The flags and all configuration registers
        /// are reset to default values.
        /// </summary>
        public void Reset()
        {
            // a reset (factory defaults) is initiated by writing 0x3f into the reset register (RST)
            SetRegister(Register.RST, (byte)ResetType.Initial);
        }

        /// <summary>
        /// Performs a reset of all flags (status register, interrupt flag and interrupt table).
        /// This method is useful, if using the interrupt mechanism for pixel temperatures.
        /// If an upper and lower level has been set along with a hysteresis this reset can clear the interrupt state of all pixels
        /// which are within the range between upper and lower level, but still above/below the hystersis level.
        /// If this applies to ALL pixels the interrupt flag gets cleared as well.
        /// Refer to the binding documentation for more details on interrupt level, hysteresis and flagging.
        /// </summary>
        public void ResetAllFlags()
        {
            // a reset of all flags (status register, interrupt flag and interrupt table) is initiated by writing 0x30
            // into the reset register (RST)
            SetRegister(Register.RST, (byte)ResetType.Flag);
        }

        #endregion

        #region Interrupt control, levels and pixel flags

        /// <summary>
        /// Gets or sets the pixel temperature interrupt mode.
        /// </summary>
        /// <value>The interrupt mode, which is either aboslute or differential. The default is ```Difference```.</value>
        public InterruptMode InterruptMode
        {
            get => GetBit(Register.INTC, (byte)InterruptModeBit.INTMODE) ? InterruptMode.Absolute : InterruptMode.Difference;
            set => SetBit(Register.INTC, (byte)InterruptModeBit.INTMODE, value == InterruptMode.Absolute);
        }

        /// <summary>
        /// Get or sets whether the interrupt output  pin of the sensor is enabled.
        /// If enabled, the pin is pulled down if an interrupt is active.
        /// </summary>
        /// <value>True, if the INT pin sould be enabled; otherwise false. The default is false."</value>
        public bool InterruptPinEnabled
        {
            get => GetBit(Register.INTC, (byte)InterruptModeBit.INTEN);
            set => SetBit(Register.INTC, (byte)InterruptModeBit.INTEN, value);
        }

        /// <summary>
        /// Gets or sets the pixel temperature lower interrupt level.
        /// </summary>
        /// <value>Temperature level to trigger an interrupt if the any pixel falls below. The default is 0.</value>
        public Temperature InterruptLowerLevel
        {
            get
            {
                byte tl = GetRegister(Register.INTLL);
                byte th = GetRegister(Register.INTLH);
                return Amg88xxUtils.ConvertToTemperature(tl, th);
            }

            set
            {
                (byte tl, byte th) = Amg88xxUtils.ConvertFromTemperature(value);
                SetRegister(Register.INTLL, tl);
                SetRegister(Register.INTLH, th);
            }
        }

        /// <summary>
        /// Gets or sets the pixel temperature upper interrupt level.
        /// </summary>
        /// <value>Temperature level to trigger an interrupt if the any pixel exceeds. The default is 0.</value>
        public Temperature InterruptUpperLevel
        {
            get
            {
                byte tl = GetRegister(Register.INTHL);
                byte th = GetRegister(Register.INTHH);
                return Amg88xxUtils.ConvertToTemperature(tl, th);
            }

            set
            {
                (byte tl, byte th) = Amg88xxUtils.ConvertFromTemperature(value);
                SetRegister(Register.INTHL, tl);
                SetRegister(Register.INTHH, th);
            }
        }

        /// <summary>
        /// Gets or sets the pixel temperature interrupt hysteresis.
        /// </summary>
        /// <value>Temperature hysteresis for lower and upper interrupt triggering. The default is 0.</value>
        public Temperature InterruptHysteresis
        {
            get
            {
                byte tl = GetRegister(Register.INTSL);
                byte th = GetRegister(Register.INTSH);
                return Amg88xxUtils.ConvertToTemperature(tl, th);
            }

            set
            {
                (byte tl, byte th) = Amg88xxUtils.ConvertFromTemperature(value);
                SetRegister(Register.INTSL, tl);
                SetRegister(Register.INTSH, th);
            }
        }

        /// <summary>
        /// Gets the interrupt flags of all pixels.
        /// </summary>
        /// <returns>Interrupt flags</returns>
        public bool[,] GetInterruptFlagTable()
        {
            var registers = new Register[]
            {
                Register.INT0, Register.INT1, Register.INT2, Register.INT3,
                Register.INT4, Register.INT5, Register.INT6, Register.INT7,
            };

            // read all registers from the sensor
            var flagRegisters = new Queue<byte>();
            foreach (Register register in registers)
            {
                flagRegisters.Enqueue(GetRegister(register));
            }

            var flags = new bool[Width, Height];
            for (int row = 0; row < Height; row++)
            {
                var flagRegister = flagRegisters.Dequeue();
                for (int col = 0; col < Width; col++)
                {
                    flags[col, row] = (flagRegister & (1 << col)) > 0;
                }
            }

            return flags;
        }
        #endregion

        private byte GetRegister(Register register)
        {
            _i2cDevice.WriteByte((byte)register);
            return _i2cDevice.ReadByte();
        }

        private bool GetBit(Register register, byte bit)
        {
            return (GetRegister(register) & (1 << bit)) > 0;
        }

        private void SetRegister(Register register, byte value)
        {
            Span<byte> buffer = stackalloc byte[2]
            {
                (byte)register,
                value
            };

            _i2cDevice.Write(buffer);
        }

        private void SetBit(Register register, byte bit, bool state)
        {
            var b = GetRegister(register);
            b = (byte)(state ? (b | (1 << bit)) : (b & (~(1 << bit))));
            SetRegister(register, b);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (_i2cDevice != null)
            {
                _i2cDevice?.Dispose();
                _i2cDevice = null!;
            }
        }
    }
}
