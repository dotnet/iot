// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Device.I2c;
using System.Threading;

namespace Iot.Device.Display
{
    /// <summary>
    /// Represents an IS31FL3731 LED Matrix driver
    /// </summary>
    // Datasheet: https://cdn-shop.adafruit.com/product-files/3017/31FL3730.pdf
    // Product: https://shop.pimoroni.com/products/microdot-phat
    // Related repo: https://github.com/pimoroni/microdot-phat
    public class Is31fl3730 : IDisposable
    {
        // Function register
        // table 2 in datasheet
        private const byte CONFIGURATION_REGISTER = 0x0;
        private const byte MATRIX_1_REGISTER = 0x1;
        private const int MATRIX_1_REGISTER_LENGTH = MATRIX_2_REGISTER - MATRIX_1_REGISTER;
        private const byte MATRIX_2_REGISTER = 0xE;
        private const int MATRIX_2_REGISTER_LENGTH = UPDATE_COLUMN_REGISTER - MATRIX_2_REGISTER;
        private const byte UPDATE_COLUMN_REGISTER = 0xC;
        private const byte LIGHTING_EFFECT_REGISTER = 0xD;
        private const byte PWM_REGISTER = 0x19;
        private const byte RESET_REGISTER = 0xC;

        // Configuration register
        // table 3 in datasheet
        private const byte SHUTDOWN = 0x80;
        private const byte DISPLAY_MATRIX_ONE = 0x0;
        private const byte DISPLAY_MATRIX_TWO = 0x8;
        private const byte DISPLAY_MATRIX_BOTH = 0x18;
        private const byte MATRIX_8x8 = 0x0;
        private const byte MATRIX_7x9 = 0x1;
        private const byte MATRIX_6x10 = 0x2;
        private const byte MATRIX_5x11 = 0x3;

        // Values
        private readonly byte[] _disable_all_leds_data = new byte[8];
        private readonly byte[] _enable_all_leds_data = new byte[MATRIX_1_REGISTER_LENGTH];
        private byte[] _matrix1 = new byte[MATRIX_1_REGISTER_LENGTH];
        private byte[] _matrix2 = new byte[MATRIX_1_REGISTER_LENGTH];
        private List<byte[]> _matrices = new();

        private I2cDevice _i2cDevice;

       /// <summary>
        /// Initialize IS31FL3730 device
        /// </summary>
        /// <param name="i2cDevice">The <see cref="System.Device.I2c.I2cDevice"/> to create with.</param>
        public Is31fl3730(I2cDevice i2cDevice)
        {
            _i2cDevice = i2cDevice;
            _enable_all_leds_data.AsSpan().Fill(0xff);
            _matrices.Add(_matrix1);
            _matrices.Add(_matrix2);
        }

        /// <summary>
        /// Initialize IS31FL3730 device
        /// </summary>
        /// <param name="i2cDevice">The <see cref="System.Device.I2c.I2cDevice"/> to create with.</param>
        /// <param name="width">The width of the LED matrix.</param>
        /// <param name="height">The height of the LED matrix.</param>
        public Is31fl3730(I2cDevice i2cDevice, int width, int height)
        : this(i2cDevice)
        {
            Width = width;
            Height = height;
        }

        /// <summary>
        /// Default I2C address for device.
        /// </summary>
        public static readonly int DefaultI2cAddress = 0x63;

        /// <summary>
        /// Width of LED matrix (x axis).
        /// </summary>
        public readonly int Width = 16;

        /// <summary>
        /// Height of LED matrix (y axis).
        /// </summary>
        public readonly int Height = 9;

        private int _configurationValue = 0;

        /// <summary>
        /// Initialize LED driver.
        /// </summary>
        public void Initialize()
        {
            // Reset device
            Reset();
            SetDisplayMode(true, true);
            SetMatrixMode(MatrixMode.Size8x8);
            WriteConfiguration();
            DisableAllLeds();
        }

        /// <summary>
        /// Indexer for updating matrix, with PWM register.
        /// </summary>
        public int this[int x, int y, int matrix]
        {
            // get => ReadLedPwm(x, y);
            set => WriteLed(matrix, x, y, value);
        }

        /// <summary>
        /// Enable all LEDs.
        /// </summary>
        public void EnableAllLeds()
        {
            Write(MATRIX_1_REGISTER, _enable_all_leds_data);
            Write(MATRIX_2_REGISTER, _enable_all_leds_data);
            WriteUpdateRegister();
        }

        /// <summary>
        /// Disable all LEDs.
        /// </summary>
        public void DisableAllLeds()
        {
            Write(MATRIX_1_REGISTER, _disable_all_leds_data);
            Write(MATRIX_2_REGISTER, _disable_all_leds_data);
            WriteUpdateRegister();
        }

        /// <summary>
        /// Reset device.
        /// </summary>
        public void Reset()
        {
            // Reset device
            Shutdown(true);
            Thread.Sleep(10);
            Shutdown(false);
        }

        /// <summary>
        /// Set the shutdown mode.
        /// </summary>
        /// <param name="shutdown">Set the showdown mode. `true` sets device into shutdown mode. `false` sets device into normal operation.</param>
        public void Shutdown(bool shutdown)
        {
            // mode values
            // 0 = normal operation
            // 1 = shutdown mode
            if (shutdown)
            {
                _configurationValue |= SHUTDOWN;
            }
            else
            {
                _configurationValue &= ~SHUTDOWN;
            }

            WriteUpdateRegister();
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _i2cDevice?.Dispose();
            _i2cDevice = null!;
        }

        private void SetDisplayMode(bool matrixOne, bool matrixTwo)
        {
            _configurationValue |= (matrixOne, matrixTwo) switch
            {
                (true, true) => DISPLAY_MATRIX_BOTH,
                (true, _) => DISPLAY_MATRIX_ONE,
                (_, true) => DISPLAY_MATRIX_TWO,
                _ => throw new Exception("Invalid input.")
            };
        }

        private void SetMatrixMode(MatrixMode mode)
        {
            _configurationValue |= mode switch
            {
                MatrixMode.Size5x11 => MATRIX_5x11,
                MatrixMode.Size6x10 => MATRIX_6x10,
                MatrixMode.Size7x9 => MATRIX_7x9,
                MatrixMode.Size8x8 => MATRIX_8x8,
                _ => throw new Exception("Invalid input.")
            };
        }

        private void WriteConfiguration()
        {
            Write(CONFIGURATION_REGISTER, (byte)_configurationValue);
        }

        private void Write(byte address, byte[] value)
        {
            byte[] data = new byte[value.Length + 1];
            data[0] = address;
            value.CopyTo(data.AsSpan(1));
            _i2cDevice.Write(data);
        }

        private void Write(byte address, byte value)
        {
            _i2cDevice.Write(new byte[] { address, value });
        }

        private void WriteLed(int matrix, int x, int y, int enable)
        {
            byte[] m = _matrices[matrix];
            byte mask = (byte)(1 >> x);

            if (enable is 1)
            {
                m[y] |= mask;
            }
            else
            {
                m[y] &= (byte)(~mask);
            }

            UpdateMatrixRegisters();
        }

        private void WriteUpdateRegister()
        {
            Write(UPDATE_COLUMN_REGISTER, 0x80);
        }

        private void UpdateMatrixRegisters()
        {
            Write(MATRIX_1_REGISTER, _matrix1);
            Write(MATRIX_2_REGISTER, _matrix2);
            WriteUpdateRegister();
        }
    }
}
