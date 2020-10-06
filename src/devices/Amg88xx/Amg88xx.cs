// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
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
        public const int DeviceAddress = 0x68;

        /// <summary>
        /// Alternative device address
        /// (AD_SELECT pin is high, c.f. reference specification, pg. 11)
        /// </summary>
        public const int AlternativeDeviceAddress = 0x69;

        /// <summary>
        /// Number of sensor pixel array columns
        /// </summary>
        public const int Columns = 0x8;

        /// <summary>
        /// Number of sensor pixel array rows
        /// </summary>
        public const int Rows = 0x8;

        /// <summary>
        /// Number of bytes per pixel
        /// </summary>
        public const int BytesPerPixel = 2;

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
        /// Gets the current thermal image.
        /// </summary>
        /// <returns>Array of pixel temperatures [column, row]</returns>
        public Temperature[,] GetThermalImage()
        {
            // the readout process gets triggered by writing to pixel 0 of the sensor w/o any additional data
            _i2cDevice.WriteByte((byte)Register.T01L);

            Span<byte> buffer = stackalloc byte[Rows * Columns * BytesPerPixel];
            _i2cDevice.Read(buffer);

            var image = new Temperature[Columns, Rows];

            int idx = 0;
            for (int r = 0; r < Rows; r++)
            {
                for (int c = 0; c < Columns; c++)
                {
                    byte tl = buffer[idx++];
                    byte th = buffer[idx++];
                    image[c, r] = Amg88xxUtils.ConvertToTemperature(tl, th);
                }
            }

            return image;
        }

        /// <summary>
        /// Gets the current raw thermal image from the sensor.
        /// </summary>
        /// <returns>Thermal image in two's complement representation</returns>
        public int[,] GetThermalRawImage()
        {
            // the readout process gets triggered by writing to pixel 0 of the sensor w/o any additional data
            _i2cDevice.WriteByte((byte)Register.T01L);

            Span<byte> buffer = stackalloc byte[Rows * Columns * BytesPerPixel];
            _i2cDevice.Read(buffer);

            var image = new int[Columns, Rows];

            int idx = 0;
            for (int r = 0; r < Rows; r++)
            {
                for (int c = 0; c < Columns; c++)
                {
                    byte tl = buffer[idx++];
                    byte th = buffer[idx++];
                    image[c, r] = th << 8 | tl;
                }
            }

            return image;
        }

        /// <summary>
        /// Gets the temperature reading from the internal thermistor.
        /// </summary>
        /// <returns>Temperature reading</returns>
        public Temperature GetSensorTemperature()
        {
            byte tthl = GetRegister(Register.TTHL);
            byte tthh = GetRegister(Register.TTHH);
            return Amg88xxUtils.ConvertThermistorReading(tthl, tthh);
        }

        #endregion

        #region Status

        /// <summary>
        /// Gets the temperature overflow flag from the status register
        /// </summary>
        /// <returns>Temperature overflow flag</returns>
        public bool HasTemperatureOverflow()
        {
            return GetBit(Register.STAT, (byte)StatusFlagBit.OVF_IRS);
        }

        /// <summary>
        /// Clears the temperature overflow flag in the status register
        /// </summary>
        public void ClearTemperatureOverflow()
        {
            // only the bit to be cleared is set, the other bits need to be 0
            SetRegister(Register.SCLR, 1 << (byte)StatusClearBit.OVFCLR);
        }

        /// <summary>
        /// Gets the thermistor overflow flag from the status register.
        /// Note: the bit is only menthioned in early versions of the reference specification.
        /// It is not clear whether this is a specification error or a change in a newer
        /// revision of the sensor.
        /// </summary>
        /// <returns>Thermistor overflow flag</returns>
        public bool HasThermistorOverflow()
        {
            return GetBit(Register.STAT, (byte)StatusFlagBit.OVF_THS);
        }

        /// <summary>
        /// Clears the thermistor overflow flag in the status register
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
        /// Clears all flags in the status register
        /// </summary>
        public void ClearAllFlags()
        {
            // only the bit to be cleared is set, the other bits need to be 0
            SetRegister(Register.SCLR, (1 << (byte)StatusClearBit.OVFCLR) | (1 << (byte)StatusClearBit.OVFTHCLR) | (1 << (byte)StatusClearBit.INTCLR));
        }

        #endregion

        #region Moving average

        /// <summary>
        /// Get the state of the moving average mode
        /// </summary>
        /// <returns>True, if moving average mode is on</returns>
        public bool GetMovingAverageModeState()
        {
            return GetBit(Register.AVE, (byte)MovingAverageModeBit.MAMOD);
        }

        /// <summary>
        /// Sets the moving average mode state.
        /// </summary>
        /// <param name="state">True, to switch moving average on</param>
        public void SetMovingAverageModeState(bool state)
        {
            SetBit(Register.AVE, (byte)MovingAverageModeBit.MAMOD, state);
        }

        #endregion

        #region Frame Rate

        /// <summary>
        /// Get the current frame rate.
        /// </summary>
        /// <returns>Frame rate (either 1 or 10fps) </returns>
        public int GetFrameRate()
        {
            return GetBit(Register.FPSC, (byte)FrameRateBit.FPS) ? 1 : 10;
        }

        /// <summary>
        /// Sets the frame rate (either 1 or 10fps).
        /// </summary>
        /// <param name="frameRate">Frame rate</param>
        /// <exception cref="ArgumentException">Thrown when attempting to set a frame rate other than 1 or 10</exception>
        public void SetFrameRate(int frameRate)
        {
            if (frameRate != 1 && frameRate != 10)
            {
                throw new ArgumentException("Frame rate must either be 1 or 10.", nameof(frameRate));
            }

            SetBit(Register.FPSC, (byte)FrameRateBit.FPS, frameRate == 1);
        }
        #endregion

        #region Operating Mode / Power Control

        /// <summary>
        /// Gets the current operating mode
        /// </summary>
        /// <returns>Operating mode</returns>
        public OperatingMode GetOperatingMode()
        {
            return (OperatingMode)GetRegister(Register.PCLT);
        }

        /// <summary>
        /// Sets the operating mode
        /// </summary>
        /// <param name="operatingMode">Operating mode</param>
        public void SetOperatingMode(OperatingMode operatingMode)
        {
            SetRegister(Register.PCLT, (byte)operatingMode);
        }
        #endregion

        #region Reset

        /// <summary>
        /// Performs an initial reset of the sensor. The flags and all configuration registers
        /// are reset to default values.
        /// </summary>
        public void InitialReset()
        {
            // an initial reset (factory defaults) is initiated by writing 0x3f into the reset register (RST)
            SetRegister(Register.RST, (byte)ResetType.Initial);
        }

        /// <summary>
        /// Performs a reset of all flags
        /// </summary>
        public void FlagReset()
        {
            // a reset of all flags (status register, interrupt flag and interrupt table) is initiated by writing 0x30
            // into the reset register (RST)
            SetRegister(Register.RST, (byte)ResetType.Flag);
        }

        #endregion

        #region Interrupt control, levels and pixel flags

        /// <summary>
        /// Gets the interrupt mode
        /// </summary>
        /// <returns>Interrupt mode</returns>
        public InterruptMode GetInterruptMode()
        {
            return GetBit(Register.INTC, (byte)InterruptModeBit.INTMODE) ? InterruptMode.AbsoluteMode : InterruptMode.DifferenceMode;
        }

        /// <summary>
        /// Sets the interrupt mode
        /// </summary>
        /// <param name="mode">Interrupt mode</param>
        public void SetInterruptMode(InterruptMode mode)
        {
            SetBit(Register.INTC, (byte)InterruptModeBit.INTMODE, mode == InterruptMode.AbsoluteMode);
        }

        /// <summary>
        /// Enables the interrupt pin of the AMG88xx sensor.
        /// The pin is pulled down if an interrupt is active.
        /// </summary>
        public void EnableInterruptPin()
        {
            SetBit(Register.INTC, (byte)InterruptModeBit.INTEN, true);
        }

        /// <summary>
        /// Enables the interrupt pin of the AMG88xx sensor.
        /// The pin is pulled down if an interrupt is active.
        /// </summary>
        public void DisableInterruptPin()
        {
            SetBit(Register.INTC, (byte)InterruptModeBit.INTEN, false);
        }

        /// <summary>
        /// Gets the lower level interrupt temperature
        /// </summary>
        /// <returns>Temperature level</returns>
        public Temperature GetInterruptLowerLevel()
        {
            byte tl = GetRegister(Register.INTLL);
            byte th = GetRegister(Register.INTLH);
            return Amg88xxUtils.ConvertToTemperature(tl, th);
        }

        /// <summary>
        /// Sets the lower level interrupt temperature
        /// </summary>
        /// <param name="temperature">Temperature</param>
        public void SetInterruptLowerLevel(Temperature temperature)
        {
            (byte tl, byte th) = Amg88xxUtils.ConvertFromTemperature(temperature);
            SetRegister(Register.INTLL, tl);
            SetRegister(Register.INTLH, th);
        }

        /// <summary>
        /// Gets the upper level interrupt temperature
        /// </summary>
        /// <returns>Temperature level</returns>
        public Temperature GetInterruptUpperLevel()
        {
            byte tl = GetRegister(Register.INTHL);
            byte th = GetRegister(Register.INTHH);
            return Amg88xxUtils.ConvertToTemperature(tl, th);
        }

        /// <summary>
        /// Sets the upper level interrupt temperature
        /// </summary>
        /// <param name="temperature">Temperature</param>
        public void SetInterruptUpperLevel(Temperature temperature)
        {
            (byte tl, byte th) = Amg88xxUtils.ConvertFromTemperature(temperature);
            SetRegister(Register.INTHL, tl);
            SetRegister(Register.INTHH, th);
        }

        /// <summary>
        /// Gets the hysteresis level interrupt temperature
        /// </summary>
        /// <returns>Temperature level</returns>
        public Temperature GetInterruptHysteresisLevel()
        {
            byte tl = GetRegister(Register.INTSL);
            byte th = GetRegister(Register.INTSH);
            return Amg88xxUtils.ConvertToTemperature(tl, th);
        }

        /// <summary>
        /// Sets the hysteresis level interrupt temperature
        /// </summary>
        /// <param name="temperature">Temperature</param>
        public void SetInterruptHysteresisLevel(Temperature temperature)
        {
            (byte tl, byte th) = Amg88xxUtils.ConvertFromTemperature(temperature);
            SetRegister(Register.INTSL, tl);
            SetRegister(Register.INTSH, th);
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

            var flags = new bool[Columns, Rows];
            for (int row = 0; row < Rows; row++)
            {
                var flagRegister = flagRegisters.Dequeue();
                for (int col = 0; col < Columns; col++)
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
                _i2cDevice = null;
            }
        }
    }
}