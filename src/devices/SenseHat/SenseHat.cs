// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using UnitsNet;

namespace Iot.Device.SenseHat
{
    /// <summary>
    /// SenseHAT
    /// </summary>
    public class SenseHat : IDisposable
    {
        /// <summary>
        /// Default I2C bus id
        /// </summary>
        public const int DefaultI2cBusId = 1;

        private I2cBus _i2cBus;
        private bool _shouldDispose;
        private SenseHatLedMatrix _ledMatrix;
        private SenseHatJoystick _joystick;
        private SenseHatAccelerometerAndGyroscope _gyro;
        private SenseHatMagnetometer _mag;
        private SenseHatTemperatureAndHumidity _temp;
        private SenseHatPressureAndTemperature _press;

        /// <summary>
        /// Constructs SenseHat instance
        /// </summary>
        public SenseHat(I2cBus? i2cBus = null, bool shouldDispose = false)
        {
            _shouldDispose = shouldDispose || i2cBus == null;
            _i2cBus = i2cBus ?? I2cBus.Create(DefaultI2cBusId);

            Debug.Assert(SenseHatLedMatrixI2c.I2cAddress == SenseHatJoystick.I2cAddress, $"Default addresses for {nameof(SenseHatLedMatrixI2c)} and {nameof(SenseHatJoystick)} were expected to be the same");
            I2cDevice joystickAndLedMatrixI2cDevice = _i2cBus.CreateDevice(SenseHatLedMatrixI2c.I2cAddress);
            _ledMatrix = new SenseHatLedMatrixI2c(joystickAndLedMatrixI2cDevice);
            _joystick = new SenseHatJoystick(joystickAndLedMatrixI2cDevice);
            _gyro = new SenseHatAccelerometerAndGyroscope(_i2cBus.CreateDevice(SenseHatAccelerometerAndGyroscope.I2cAddress));
            _mag = new SenseHatMagnetometer(_i2cBus.CreateDevice(SenseHatMagnetometer.I2cAddress));
            _temp = new SenseHatTemperatureAndHumidity(_i2cBus.CreateDevice(SenseHatTemperatureAndHumidity.I2cAddress));
            _press = new SenseHatPressureAndTemperature(_i2cBus.CreateDevice(SenseHatPressureAndTemperature.I2cAddress));
        }

        // LED Matrix

        /// <summary>
        /// Fills LED matrix with specified color
        /// </summary>
        /// <param name="color">Color to fill LED matrix with</param>
        public void Fill(Color color = default(Color)) => _ledMatrix.Fill(color);

        /// <summary>
        /// Sets color on specified pixel on LED matrix
        /// </summary>
        /// <param name="x">X coordinate of the pixel</param>
        /// <param name="y">Y coordinate of the pixel</param>
        /// <param name="color">Color to set the pixel to</param>
        public void SetPixel(int x, int y, Color color) => _ledMatrix.SetPixel(x, y, color);

        /// <summary>
        /// Write colors to LED matrix
        /// </summary>
        /// <param name="colors">Buffer of colors</param>
        public void Write(ReadOnlySpan<Color> colors) => _ledMatrix.Write(colors);

        // JOYSTICK

        /// <summary>
        /// Reads joystick state
        /// </summary>
        public void ReadJoystickState() => _joystick.Read();

        /// <summary>
        /// Is Joystick being held left
        /// </summary>
        public bool HoldingLeft => _joystick.HoldingLeft;

        /// <summary>
        /// Is Joystick being held right
        /// </summary>
        public bool HoldingRight => _joystick.HoldingRight;

        /// <summary>
        /// Is Joystick being held up
        /// </summary>
        public bool HoldingUp => _joystick.HoldingUp;

        /// <summary>
        /// Is Joystick being held down
        /// </summary>
        public bool HoldingDown => _joystick.HoldingDown;

        /// <summary>
        /// Is Joystick button being held
        /// </summary>
        public bool HoldingButton => _joystick.HoldingButton;

        // GYRO

        /// <summary>
        /// Gets acceleration from the gyroscope
        /// </summary>
        public Vector3 Acceleration => _gyro.Acceleration;

        /// <summary>
        /// Gets angular rate from the gyroscope
        /// </summary>
        public Vector3 AngularRate => _gyro.AngularRate;

        // MAGNETOMETER

        /// <summary>
        /// Gets magnetic induction from the magnetometer
        /// </summary>
        public Vector3 MagneticInduction => _mag.MagneticInduction;

        // TEMPERATURE AND HUMIDITY

        /// <summary>
        /// Gets temperature from temperature and humidity sensor
        /// </summary>
        public Temperature Temperature => _temp.Temperature;

        /// <summary>
        /// Gets humidity from temperature and humidity sensor
        /// </summary>
        public RelativeHumidity Humidity => _temp.Humidity;

        // PRESSURE AND TEMPERATURE

        /// <summary>
        /// Gets pressure from pressure and temperature sensor
        /// </summary>
        public Pressure Pressure => _press.Pressure;

        /// <summary>
        /// Gets temperature from pressure and temperature sensor
        /// </summary>
        public Temperature Temperature2 => _press.Temperature;

        /// <inheritdoc/>
        public void Dispose()
        {
            if (_shouldDispose)
            {
                _i2cBus?.Dispose();
            }
            else
            {
                _ledMatrix?.Dispose();
                _ledMatrix = null!;

                _joystick?.Dispose();
                _joystick = null!;

                _gyro?.Dispose();
                _gyro = null!;

                _mag?.Dispose();
                _mag = null!;

                _temp?.Dispose();
                _temp = null!;

                _press?.Dispose();
                _press = null!;
            }

            _i2cBus = null!;
        }
    }
}
