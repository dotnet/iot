// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.IO;
using System.Device.Gpio;
using System.Device.I2c;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Device.I2c.Drivers;
using System.Drawing;
using System.Numerics;

namespace Iot.Device.SenseHat
{
    public class SenseHat : IDisposable
    {
        private SenseHatLedMatrix _ledMatrix;
        private SenseHatJoystick _joystick;
        private SenseHatAccelerometerAndGyroscope _gyro;
        private SenseHatMagnetometer _mag;
        private SenseHatTemperatureAndHumidity _temp;
        private SenseHatPressureAndTemperature _press;

        public SenseHat()
        {
            _ledMatrix = new SenseHatLedMatrixI2c();
            _joystick = new SenseHatJoystick();
            _gyro = new SenseHatAccelerometerAndGyroscope();
            _mag = new SenseHatMagnetometer();
            _temp = new SenseHatTemperatureAndHumidity();
            _press = new SenseHatPressureAndTemperature();
        }

        // LED Matrix
        public void Fill(Color color = default(Color)) => _ledMatrix.Fill(color);
        public void SetPixel(int x, int y, Color color) => _ledMatrix.SetPixel(x, y, color);
        public void Write(ReadOnlySpan<Color> colors) => _ledMatrix.Write(colors);

        // JOYSTICK
        public void ReadJoystickState() => _joystick.Read();
        public bool HoldingLeft => _joystick.HoldingLeft;
        public bool HoldingRight => _joystick.HoldingRight;
        public bool HoldingUp => _joystick.HoldingUp;
        public bool HoldingDown => _joystick.HoldingDown;
        public bool HoldingButton => _joystick.HoldingButton;

        // GYRO
        public Vector3 Acceleration => _gyro.Acceleration;
        public Vector3 AngularRate => _gyro.AngularRate;

        // MAGNETOMETER
        public Vector3 MagneticInduction => _mag.MagneticInduction;

        // TEMPERATURE AND HUMIDITY
        public float Temperature => _temp.Temperature;
        public float Humidity => _temp.Humidity;

        // PRESSURE AND TEMPERATURE
        public float Pressure => _press.Pressure;
        public float Temperature2 => _press.Temperature;


        public void Dispose()
        {
            _ledMatrix?.Dispose();
            _ledMatrix = null;

            _joystick?.Dispose();
            _joystick = null;

            _gyro?.Dispose();
            _gyro = null;

            _mag?.Dispose();
            _mag = null;

            _temp?.Dispose();
            _temp = null;

            _press?.Dispose();
            _press = null;
        }
    }
}
