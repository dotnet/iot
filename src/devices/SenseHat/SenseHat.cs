// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using System.Device.Model;
using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using UnitsNet;

namespace Iot.Device.SenseHat
{
    /// <summary>
    /// SenseHAT
    /// </summary>
    [Interface("SenseHAT")]
    public class SenseHat : IDisposable
    {
        /// <summary>
        /// Default I2C bus id
        /// </summary>
        public const int DefaultI2cBusId = 1;

        private I2cBus _i2cBus;
        private bool _shouldDispose;

        /// <summary>
        /// LED Matrix
        /// </summary>
        public SenseHatLedMatrix LedMatrix { get; private set; }

        /// <summary>
        /// Joystick
        /// </summary>
        public SenseHatJoystick Joystick { get; private set; }

        /// <summary>
        /// Gyroscope
        /// </summary>
        [Component]
        public SenseHatAccelerometerAndGyroscope Gyroscope { get; private set; }

        /// <summary>
        /// Magnetometer
        /// </summary>
        [Component]
        public SenseHatMagnetometer Magnetometer { get; private set; }

        /// <summary>
        /// Temperature and humidity sensor
        /// </summary>
        [Component]
        public SenseHatTemperatureAndHumidity TemperatureAndHumidity { get; private set; }

        /// <summary>
        /// Pressure and temperature sensor
        /// </summary>
        [Component]
        public SenseHatPressureAndTemperature PressureAndTemperature { get; private set; }

        /// <summary>
        /// Constructs SenseHat instance
        /// </summary>
        public SenseHat(I2cBus? i2cBus = null, bool shouldDispose = false)
        {
            _shouldDispose = shouldDispose || i2cBus == null;
            _i2cBus = i2cBus ?? I2cBus.Create(DefaultI2cBusId);

            Debug.Assert(SenseHatLedMatrixI2c.I2cAddress == SenseHatJoystick.I2cAddress, $"Default addresses for {nameof(SenseHatLedMatrixI2c)} and {nameof(SenseHatJoystick)} were expected to be the same");
            I2cDevice joystickAndLedMatrixI2cDevice = _i2cBus.CreateDevice(SenseHatLedMatrixI2c.I2cAddress);
            LedMatrix = new SenseHatLedMatrixI2c(joystickAndLedMatrixI2cDevice);
            Joystick = new SenseHatJoystick(joystickAndLedMatrixI2cDevice);
            Gyroscope = new SenseHatAccelerometerAndGyroscope(_i2cBus.CreateDevice(SenseHatAccelerometerAndGyroscope.I2cAddress));
            Magnetometer = new SenseHatMagnetometer(_i2cBus.CreateDevice(SenseHatMagnetometer.I2cAddress));
            TemperatureAndHumidity = new SenseHatTemperatureAndHumidity(_i2cBus.CreateDevice(SenseHatTemperatureAndHumidity.I2cAddress));
            PressureAndTemperature = new SenseHatPressureAndTemperature(_i2cBus.CreateDevice(SenseHatPressureAndTemperature.I2cAddress));
        }

        // LED Matrix

        /// <summary>
        /// Fills LED matrix with specified color
        /// </summary>
        /// <param name="color">Color to fill LED matrix with</param>
        public void Fill(Color color = default(Color)) => LedMatrix.Fill(color);

        /// <summary>
        /// Sets color on specified pixel on LED matrix
        /// </summary>
        /// <param name="x">X coordinate of the pixel</param>
        /// <param name="y">Y coordinate of the pixel</param>
        /// <param name="color">Color to set the pixel to</param>
        public void SetPixel(int x, int y, Color color) => LedMatrix.SetPixel(x, y, color);

        /// <summary>
        /// Write colors to LED matrix
        /// </summary>
        /// <param name="colors">Buffer of colors</param>
        public void Write(ReadOnlySpan<Color> colors) => LedMatrix.Write(colors);

        // JOYSTICK

        /// <summary>
        /// Reads joystick state
        /// </summary>
        public void ReadJoystickState() => Joystick.Read();

        /// <summary>
        /// Is Joystick being held left
        /// </summary>
        public bool HoldingLeft => Joystick.HoldingLeft;

        /// <summary>
        /// Is Joystick being held right
        /// </summary>
        public bool HoldingRight => Joystick.HoldingRight;

        /// <summary>
        /// Is Joystick being held up
        /// </summary>
        public bool HoldingUp => Joystick.HoldingUp;

        /// <summary>
        /// Is Joystick being held down
        /// </summary>
        public bool HoldingDown => Joystick.HoldingDown;

        /// <summary>
        /// Is Joystick button being held
        /// </summary>
        public bool HoldingButton => Joystick.HoldingButton;

        // GYRO

        /// <summary>
        /// Gets acceleration from the gyroscope
        /// </summary>
        public Vector3 Acceleration => Gyroscope.Acceleration;

        /// <summary>
        /// Gets angular rate from the gyroscope
        /// </summary>
        public Vector3 AngularRate => Gyroscope.AngularRate;

        // MAGNETOMETER

        /// <summary>
        /// Gets magnetic induction from the magnetometer
        /// </summary>
        public Vector3 MagneticInduction => Magnetometer.MagneticInduction;

        // TEMPERATURE AND HUMIDITY

        /// <summary>
        /// Gets temperature from temperature and humidity sensor
        /// </summary>
        public Temperature Temperature => TemperatureAndHumidity.Temperature;

        /// <summary>
        /// Gets humidity from temperature and humidity sensor
        /// </summary>
        public RelativeHumidity Humidity => TemperatureAndHumidity.Humidity;

        // PRESSURE AND TEMPERATURE

        /// <summary>
        /// Gets pressure from pressure and temperature sensor
        /// </summary>
        public Pressure Pressure => PressureAndTemperature.Pressure;

        /// <summary>
        /// Gets temperature from pressure and temperature sensor
        /// </summary>
        public Temperature Temperature2 => PressureAndTemperature.Temperature;

        /// <inheritdoc/>
        public void Dispose()
        {
            if (_shouldDispose)
            {
                _i2cBus?.Dispose();
            }
            else
            {
                LedMatrix?.Dispose();
                LedMatrix = null!;

                Joystick?.Dispose();
                Joystick = null!;

                Gyroscope?.Dispose();
                Gyroscope = null!;

                Magnetometer?.Dispose();
                Magnetometer = null!;

                TemperatureAndHumidity?.Dispose();
                TemperatureAndHumidity = null!;

                PressureAndTemperature?.Dispose();
                PressureAndTemperature = null!;
            }

            _i2cBus = null!;
        }
    }
}
