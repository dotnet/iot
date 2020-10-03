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
    /// Add documentation here
    /// </summary>
    public class Amg88xx : IDisposable
    {
        /// <summary>
        /// Standard device address (AD_SELECT pin is low, c.f. reference specification, pg. 11)
        /// </summary>
        public const int DeviceAddress = 0x68;

        /// <summary>
        /// Alternative device address (AD_SELECT pin is high, c.f. reference specification, pg. 11)
        /// </summary>
        public const int AlternativeDeviceAddress = 0x69;

        /// <summary>
        /// Number of columns of the sensor array
        /// </summary>
        public const int Columns = 0x8;

        /// <summary>
        /// Number of rows of the sensor array
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

        /// <summary>
        /// Gets the temperature reading from the sensor's internal thermistor.
        /// </summary>
        /// <returns>Temperature reading</returns>
        public Temperature GetSensorTemperature()
        {
            _i2cDevice.WriteByte((byte)Register.TTHL);
            byte tthl = _i2cDevice.ReadByte();
            _i2cDevice.WriteByte((byte)Register.TTHH);
            byte tthh = _i2cDevice.ReadByte();

            return Amg88xxUtils.ConvertThermistorReading(tthl, tthh);
        }

        /// <summary>
        /// Gets the current thermal image from the sensor as temperature per pixel.
        /// </summary>
        /// <returns>Thermal image</returns>
        public Temperature[,] GetThermalImage()
        {
            var rawImage = GetRawImage();
            var temperatureImage = new Temperature[Columns, Rows];

            for (int r = 0; r < Rows; r++)
            {
                for (int c = 0; c < Columns; c++)
                {
                    temperatureImage[c, r] = Amg88xxUtils.ConvertToTemperature((byte)(rawImage[c, r] & 0xff), (byte)(rawImage[c, r] >> 8));
                }
            }

            return temperatureImage;
        }

        /// <summary>
        /// Gets the current thermal image from the sensor as 12-bit two's complement per pixel.
        /// </summary>
        /// <returns>Thermal image</returns>
        public int[,] GetRawImage()
        {
            var image = new int[Columns, Rows];

            _i2cDevice.WriteByte((byte)Register.T01L);
            Span<byte> buffer = stackalloc byte[Rows * Columns * BytesPerPixel];
            _i2cDevice.Read(buffer);

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

        #region Status

        /// <summary>
        /// Gets the temperature overflow flag from the status register.
        /// </summary>
        /// <returns>Temperature overflow flag</returns>
        public bool HasTemperatureOverflow()
        {
            _i2cDevice.WriteByte((byte)Register.STAT);
            return (_i2cDevice.ReadByte() & (byte)StatusFlag.OVF_IRS) != 0;
        }

        /// <summary>
        /// Clears the temperature overflag of the status register.
        /// </summary>
        public void ClearTemperatureOverflow()
        {
            SetRegister((byte)Register.SCLR, (byte)StatusFlag.OVF_IRS);
        }

        /// <summary>
        /// Gets the thermistor overflow flag from the status register.
        /// </summary>
        /// <returns>Thermistor overflow flag</returns>
        public bool HasThermistorOverflow()
        {
            _i2cDevice.WriteByte((byte)Register.STAT);
            return (_i2cDevice.ReadByte() & (byte)StatusFlag.OVF_THS) != 0;
        }

        /// <summary>
        /// Clears the thermistor overflag of the status register.
        /// </summary>
        public void ClearThermistorOverflow()
        {
            SetRegister((byte)Register.SCLR, (byte)StatusFlag.OVF_THS);
        }

        /// <summary>
        /// Gets the interrupt flag from the status register.
        /// </summary>
        /// <returns>Interrupt flag</returns>
        public bool HasInterrupt()
        {
            _i2cDevice.WriteByte((byte)Register.STAT);
            return (_i2cDevice.ReadByte() & (byte)StatusFlag.INTF) != 0;
        }

        /// <summary>
        /// Clears the interrupt flag of the status register.
        /// </summary>
        public void ClearInterrupt()
        {
            SetRegister((byte)Register.SCLR, (byte)StatusFlag.INTF);
        }

        /// <summary>
        /// Clears the status register.
        /// </summary>
        public void ClearAllStatus()
        {
            _i2cDevice.WriteByte((byte)Register.SCLR);
            _i2cDevice.WriteByte((byte)(StatusFlag.INTF | StatusFlag.OVF_IRS | StatusFlag.OVF_THS));
        }

        #endregion

        #region Moving average

        /// <summary>
        /// Get the state of the moving average mode.
        /// </summary>
        /// <returns>True, if moving average is active</returns>
        public bool GetMovingAverageMode()
        {
            _i2cDevice.WriteByte((byte)Register.AVE);
            // if bit 5 of AVE register is set average mode is on
            return (_i2cDevice.ReadByte() & 0b0001_0000) != 0;
        }

        /// <summary>
        /// Sets the moving average mode.
        /// </summary>
        /// <param name="mode">True, to switch moving average on</param>
        public void SetMovingAverageMode(bool mode)
        {
            // bit 5: if set, average mode is on
            SetRegister((byte)Register.AVE, (byte)(mode ? 0b0001_0000 : 0));
        }

        #endregion

        #region Frame Rate

        /// <summary>
        /// Get the current frame rate.
        /// </summary>
        /// <returns>Frame rate (1 or 10 fps) </returns>
        public FrameRate GetFrameRate()
        {
            _i2cDevice.WriteByte((byte)Register.FPSC);
            // if bit 0 of FPSC register is set the frame rate is 1 otherwise 10 fps.
            return (_i2cDevice.ReadByte() & 0b0000_0001) != 0 ? FrameRate.FPS1 : FrameRate.FPS10;
        }

        /// <summary>
        /// Sets the frame rate (1 or 10 fps).
        /// </summary>
        /// <param name="frameRate">Frame rate</param>
        public void SetFrameRate(FrameRate frameRate)
        {
            // if bit 0 of FPSC register is set the frame rate is 1 otherwise 10 fps.
            SetRegister((byte)Register.FPSC, (byte)(frameRate == FrameRate.FPS1 ? 0b0000_0001 : 0));
        }
        #endregion

        #region Operating Mode / Power Control

        /// <summary>
        /// Get the current operating mode.
        /// </summary>
        /// <returns>Operating mode</returns>
        public OperatingMode GetOperatingMode()
        {
            _i2cDevice.WriteByte((byte)Register.PCLT);
            return (OperatingMode)_i2cDevice.ReadByte();
        }

        /// <summary>
        /// Sets the operating mode
        /// </summary>
        /// <param name="operatingMode">Operating mode</param>
        public void SetOperatingMode(OperatingMode operatingMode)
        {
            SetRegister((byte)Register.PCLT, (byte)operatingMode);
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
            SetRegister((byte)Register.RST, 0x3f);
        }

        /// <summary>
        /// Performs a reset of all flags.
        /// </summary>
        public void FlagReset()
        {
            // a reset of all flags (status register, interrupt flag and interrupt table) is initiated by writing 0x30
            // into the reset register (RST)
            SetRegister((byte)Register.RST, 0x30);
        }

        #endregion

        #region Interrupt control, levels and pixel flags

        /// <summary>
        /// Gets the interrupt mode
        /// </summary>
        /// <returns>Interrupt mode</returns>
        public InterruptMode GetInterruptMode()
        {
            // bit 1 represents the interrupt mode (not set: difference mode, set: absolute mode)
            return (GetRegister((byte)Register.INTC) & 0b0000_0010) == 0 ? InterruptMode.DifferenceMode : InterruptMode.AbsoluteMode;
        }

        /// <summary>
        /// Sets the interrupt mode
        /// </summary>
        /// <param name="mode">Interrupt mode</param>
        public void SetInterruptMode(InterruptMode mode)
        {
            byte value = GetRegister((byte)Register.INTC);
            // bit 1 represents the interrupt mode (not set: difference mode, set: absolute mode)
            switch (mode)
            {
                case InterruptMode.AbsoluteMode:
                    value |= 0b0000_0010;
                    break;
                case InterruptMode.DifferenceMode:
                    value &= 0b1111_1101;
                    break;
            }

            SetRegister((byte)Register.INTC, value);
        }

        /// <summary>
        /// Gets the lower level interrupt temperature
        /// </summary>
        /// <returns>Temperature level</returns>
        public Temperature GetInterruptLowerLevel()
        {
            byte tl = GetRegister((byte)Register.INTLL);
            byte th = GetRegister((byte)Register.INTLH);
            return Amg88xxUtils.ConvertToTemperature(tl, th);
        }

        /// <summary>
        /// Sets the lower level interrupt temperature
        /// </summary>
        /// <param name="temperature">Temperature</param>
        public void SetInterruptLowerLevel(Temperature temperature)
        {
            (byte tl, byte th) = Amg88xxUtils.ConvertFromTemperature(temperature);
            SetRegister((byte)Register.INTLL, tl);
            SetRegister((byte)Register.INTLH, th);
        }

        /// <summary>
        /// Gets the upper level interrupt temperature
        /// </summary>
        /// <returns>Temperature level</returns>
        public Temperature GetInterruptUpperLevel()
        {
            byte tl = GetRegister((byte)Register.INTHL);
            byte th = GetRegister((byte)Register.INTHH);
            return Amg88xxUtils.ConvertToTemperature(tl, th);
        }

        /// <summary>
        /// Sets the upper level interrupt temperature
        /// </summary>
        /// <param name="temperature">Temperature</param>
        public void SetInterruptUpperLevel(Temperature temperature)
        {
            (byte tl, byte th) = Amg88xxUtils.ConvertFromTemperature(temperature);
            SetRegister((byte)Register.INTHL, tl);
            SetRegister((byte)Register.INTHH, th);
        }

        /// <summary>
        /// Gets the hysteresis level interrupt temperature
        /// </summary>
        /// <returns>Temperature level</returns>
        public Temperature GetInterruptHysteresisLevel()
        {
            byte tl = GetRegister((byte)Register.INTSL);
            byte th = GetRegister((byte)Register.INTSH);
            return Amg88xxUtils.ConvertToTemperature(tl, th);
        }

        /// <summary>
        /// Sets the hysteresis level interrupt temperature
        /// </summary>
        /// <param name="temperature">Temperature</param>
        public void SetInterruptHysteresisLevel(Temperature temperature)
        {
            (byte tl, byte th) = Amg88xxUtils.ConvertFromTemperature(temperature);
            SetRegister((byte)Register.INTSL, tl);
            SetRegister((byte)Register.INTSH, th);
        }

        /// <summary>
        /// Gets the interrupt flags of all pixels.
        /// </summary>
        /// <returns>Interrupt flags</returns>
        public bool[,] GetInterruptFlagTable()
        {
            var addresses = new byte[]
            {
                (byte)Register.INT0, (byte)Register.INT1, (byte)Register.INT2, (byte)Register.INT3,
                (byte)Register.INT4, (byte)Register.INT5, (byte)Register.INT6, (byte)Register.INT7,
            };

            // read all registers from the sensor
            var flagRegisters = new Queue<byte>();
            foreach (byte address in addresses)
            {
                flagRegisters.Enqueue(GetRegister(address));
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

        /// <inheritdoc />
        public void Dispose()
        {
            if (_i2cDevice != null)
            {
                _i2cDevice?.Dispose();
                _i2cDevice = null;
            }
        }

        private byte GetRegister(byte address)
        {
            _i2cDevice.WriteByte(address);
            return _i2cDevice.ReadByte();
        }

        private void SetRegister(byte address, byte value)
        {
            Span<byte> buffer = stackalloc byte[2]
            {
                address,
                value
            };

            _i2cDevice.Write(buffer);
        }
    }
}
