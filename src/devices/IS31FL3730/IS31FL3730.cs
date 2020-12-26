// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Device.I2c;
using System.Linq;

namespace Iot.Device.IS31FL3730
{
    /// <summary>
    /// I2C LED Matrix Controller
    /// </summary>
    public class IS31FL3730 : IDisposable
    {
        // Matrix Modes - Layout (8x8, 7x9, 6x10 or 5x11)
        private const byte MATRIX_MODE_8X8 = 0b00000000;
        private const byte MATRIX_MODE_7X9 = 0b00000001;
        private const byte MATRIX_MODE_6X10 = 0b00000010;
        private const byte MATRIX_MODE_5X11 = 0b00000011;

        // Matrix Modes - Matrix 1 Only, Matrix 2 Only, Both Matrices
        private const byte MATRIX_MODE_1ONLY = 0b00000000;
        private const byte MATRIX_MODE_2ONLY = 0b00001000;
        private const byte MATRIX_MODE_BOTH = 0b00011000;

        // Matrix Modes - Soft Shutdown
        private const byte MATRIX_MODE_SOFT_SHUTDOWN = 0b10000000;

        // Matrix Modes - Audio Input
        private const byte MATRIX_MODE_AUDIO_INPUT = 0b00000100;

        /// <summary>
        /// Default I2C Address, up to four IS31FL3730's can be on the same I2C Bus.
        /// </summary>
        public const byte DefaultI2cAddress = 0x61;

        // Matrix Commands
        private const byte MATRIX_COMMAND_CONFIGURATION_REGISTER = 0x00;
        private const byte MATRIX_COMMAND_DRIVE_STRENGTH = 0x0D;
        private const byte MATRIX_COMMAND_MATRIX1 = 0x01;
        private const byte MATRIX_COMMAND_MATRIX2 = 0x0E;
        private static readonly IReadOnlyList<byte> MATRIX_COMMAND_RESET = new byte[] { 0xFF, 0x00 };
        private static readonly IReadOnlyList<byte> MATRIX_COMMAND_UPDATE = new byte[] { 0x0C, 0x01 };

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
        protected byte[] _matrix1 = new byte[11];

        /// <summary>
        /// Raw matrix values for matrix 2.
        /// </summary>
        protected byte[] _matrix2 = new byte[11];

        /// <summary>
        /// Initializes a new instance of the <see cref="IS31FL3730"/> class.
        /// </summary>
        public IS31FL3730(I2cDevice i2cDevice, DriverConfiguration configuration)
        {
            _i2cDevice = i2cDevice;
            _configuration = configuration;

            Reset();
            SetConfigurationRegister();
            SetDriveStrength();
        }

        /// <summary>
        /// Reset the IS31FL3730 LED Matrix Controller.
        /// </summary>
        public void Reset()
        {
            _matrix1 = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            _matrix2 = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

            _i2cDevice.Write(new ReadOnlySpan<byte>(MATRIX_COMMAND_RESET.ToArray<byte>()));
            UpdateDisplay();
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
            byte configuration = 0x00;

            configuration = _configuration.IsShutdown ? (byte)(configuration | MATRIX_MODE_SOFT_SHUTDOWN) : configuration;
            configuration = _configuration.IsAudioInputEnabled ? (byte)(configuration | MATRIX_MODE_AUDIO_INPUT) : configuration;

            switch (_configuration.Layout)
            {
                case MatrixLayout.Matrix8by8:
                    configuration = (byte)(configuration | MATRIX_MODE_8X8);
                    break;
                case MatrixLayout.Matrix7by9:
                    configuration = (byte)(configuration | MATRIX_MODE_7X9);
                    break;
                case MatrixLayout.Matrix6by10:
                    configuration = (byte)(configuration | MATRIX_MODE_6X10);
                    break;
                case MatrixLayout.Matrix5by11:
                    configuration = (byte)(configuration | MATRIX_MODE_5X11);
                    break;
            }

            switch (_configuration.Mode)
            {
                case MatrixMode.Matrix1Only:
                    configuration = (byte)(configuration | MATRIX_MODE_1ONLY);
                    break;
                case MatrixMode.Matrix2Only:
                    configuration = (byte)(configuration | MATRIX_MODE_2ONLY);
                    break;
                case MatrixMode.Both:
                    configuration = (byte)(configuration | MATRIX_MODE_BOTH);
                    break;
            }

            _i2cDevice.Write(new ReadOnlySpan<byte>(new byte[] { MATRIX_COMMAND_CONFIGURATION_REGISTER, configuration }));
        }

        /// <summary>
        /// Sets the LED Drive Strength register.
        /// </summary>
        public void SetDriveStrength(DriveStrength driveStrength)
        {
            _configuration.DriveStrength = driveStrength;
            SetDriveStrength();
        }

        /// <summary>
        /// Sets the LED Drive Strength register.
        /// </summary>
        protected void SetDriveStrength()
        {
            _i2cDevice.Write(new ReadOnlySpan<byte>(new byte[] { MATRIX_COMMAND_DRIVE_STRENGTH, (byte)_configuration.DriveStrength }));
        }

        /// <summary>
        /// Set the matrix output.
        /// </summary>
        /// /// <param name="matrix">Which matrix to set.</param>
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

            UpdateDisplay();
        }

        /// <summary>
        /// Show the internally stored matrix data on the LED displays.
        /// </summary>
        protected void UpdateDisplay()
        {
            _i2cDevice.Write(new ReadOnlySpan<byte>((new byte[] { MATRIX_COMMAND_MATRIX1 }).Concat(_matrix1).ToArray()));
            _i2cDevice.Write(new ReadOnlySpan<byte>(MATRIX_COMMAND_UPDATE.ToArray<byte>()));

            _i2cDevice.Write(new ReadOnlySpan<byte>((new byte[] { MATRIX_COMMAND_MATRIX2 }).Concat(_matrix2).ToArray()));
            _i2cDevice.Write(new ReadOnlySpan<byte>(MATRIX_COMMAND_UPDATE.ToArray<byte>()));
        }

        /// <inheritdoc />
        public void Dispose()
        {
        }
    }
}
