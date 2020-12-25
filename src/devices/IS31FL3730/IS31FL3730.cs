// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Gpio;
using System.Device.I2c;

namespace Iot.Device.IS31FL3730
{
    /// <summary>
    /// I2C LED Matrix Controller
    /// </summary>
    public class IS31FL3730 : IDisposable
    {
        /// <summary>
        /// Default I2C Address, up to four IS31FL3730's can be on the same I2C Bus.
        /// </summary>
        public const byte DefaultI2cAddress = 0x61;

        /// <summary>
        /// I2C Device instance to communicate with the IS31FL3730.
        /// </summary>
        protected I2cDevice _i2cDevice;

        /// <summary>
        /// IS31FL3730 LED Matrix controller configuration.
        /// </summary>
        protected DriverConfiguration _configuration;

        /// <summary>
        /// Raw matrix values for matrix 1.
        /// </summary>
        protected byte[] _matrix1 = new byte[8];

        /// <summary>
        /// Raw matrix values for matrix 2.
        /// </summary>
        protected byte[] _matrix2 = new byte[8];

        /// <summary>
        /// Initializes a new instance of the <see cref="IS31FL3730"/> class.
        /// </summary>
        public IS31FL3730(I2cDevice i2cDevice, DriverConfiguration configuration)
        {
            _i2cDevice = i2cDevice;
            _configuration = configuration;

            Reset();
            SetConfigurationRegister();
        }

        /// <summary>
        /// Reset the IS31FL3730 LED Matrix Controller.
        /// </summary>
        public void Reset()
        {
            _matrix1 = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            _matrix2 = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

            _i2cDevice.WriteByte(0xFF);
            _i2cDevice.WriteByte(0x00);
        }

        /// <summary>
        /// Reset the IS31FL3730 device configuration.
        /// </summary>
        /// <param name="configuration">New configuration settings.</param>
        public void SetConfigurationRegister(DriverConfiguration configuration)
        {
            _configuration = configuration;
            SetConfigurationRegister();
        }

        /// <summary>
        /// Sets the IS31FL3730 Configuration Register.
        /// </summary>
        protected void SetConfigurationRegister()
        {
            // TODO: Clean-up magic numbers
            byte configuration = 0x00;

            configuration = _configuration.IsShutdown ? (byte)(configuration | 0b10000000) : configuration;
            configuration = _configuration.IsAudioInputEnabled ? (byte)(configuration | 0b00000100) : configuration;

            switch (_configuration.Layout)
            {
                case MatrixLayout.Matrix8by8:
                    configuration = (byte)(configuration | 0b00000000);
                    break;
                case MatrixLayout.Matrix7by9:
                    configuration = (byte)(configuration | 0b00000001);
                    break;
                case MatrixLayout.Matrix6by10:
                    configuration = (byte)(configuration | 0b00000010);
                    break;
                case MatrixLayout.Matrix5by11:
                    configuration = (byte)(configuration | 0b00000011);
                    break;
            }

            switch (_configuration.Mode)
            {
                case MatrixMode.Matrix1Only:
                    configuration = (byte)(configuration | 0b00000000);
                    break;
                case MatrixMode.Matrix2Only:
                    configuration = (byte)(configuration | 0b00001000);
                    break;
                case MatrixMode.Both:
                    configuration = (byte)(configuration | 0b00011000);
                    break;
            }

            _i2cDevice.WriteByte(0x00);
            _i2cDevice.WriteByte(configuration);
        }

        /// <summary>
        /// Set the matrix output.
        /// </summary>
        /// <param name="matrix">Which matrix to set.</param>
        /// <param name="display">Values to load into the matrix.</param>
        public void SetMatrix(MatrixMode matrix, byte[] display)
        {
            switch (matrix)
            {
                case MatrixMode.Matrix1Only:
                    _matrix1 = display;
                    break;
                case MatrixMode.Matrix2Only:
                    _matrix2 = display;
                    break;
                case MatrixMode.Both:
                    _matrix1 = display;
                    _matrix2 = display;
                    break;
            }
        }

        /// <summary>
        /// Show the internally stored matrix data on the LED displays.
        /// </summary>
        protected void UpdateDisplay()
        {
            for (byte i = 0; i <= 7; i++)
            {
                _i2cDevice.WriteByte((byte)(0x01 + i));
                _i2cDevice.WriteByte(_matrix1[i]);
                _i2cDevice.WriteByte((byte)(0x0E + i));
                _i2cDevice.WriteByte(_matrix2[i]);
            }

            _i2cDevice.WriteByte(0x0C);
            _i2cDevice.WriteByte(0x00);
        }

        /// <inheritdoc />
        public void Dispose()
        {
        }
    }
}
