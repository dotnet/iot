// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.Gpio;

namespace Iot.Device.Hcsr501
{
    public class Hcsr501ValueChangedEventArgs : EventArgs
    {
        public readonly PinValue PinValue;
        public Hcsr501ValueChangedEventArgs(PinValue value)
        {
            PinValue = value;
        }
    }

    public class Hcsr501 : IDisposable
    {
        private GpioController _controller;
        private readonly int _pinOut;
        private readonly PinNumberingScheme _pinNumberingScheme;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="pin">OUT Pin</param>
        /// <param name="pinNumberingScheme">Pin Numbering Scheme</param>
        public Hcsr501(int pin, PinNumberingScheme pinNumberingScheme)
        {
            _pinOut = pin;
            _pinNumberingScheme = pinNumberingScheme;
        }

        /// <summary>
        /// Initialize the sensor
        /// </summary>
        public void Initialize()
        {
            _controller = new GpioController(_pinNumberingScheme);

            _controller.OpenPin(_pinOut, PinMode.Input);

            _controller.RegisterCallbackForPinValueChangedEvent(_pinOut, PinEventTypes.None, Sensor_ValueChanged);
        }

        /// <summary>
        /// Read from the sensor
        /// </summary>
        /// <returns>If a motion is detected, return true.</returns>
        public bool Read()
        {
            if (_controller.Read(_pinOut) == PinValue.High)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Get the sensor GpioController
        /// </summary>
        /// <returns>GpioController</returns>
        public GpioController GetDevice()
        {
            return _controller;
        }

        /// <summary>
        /// Cleanup
        /// </summary>
        public void Dispose()
        {
            _controller.Dispose();
        }

        public delegate void Hcsr501ValueChangedHandle(object sender, Hcsr501ValueChangedEventArgs e);

        /// <summary>
        /// Triggering when HC-SR501 value changes
        /// </summary>
        public event Hcsr501ValueChangedHandle Hcsr501ValueChanged;

        private void Sensor_ValueChanged(object sender, PinValueChangedEventArgs e)
        {
            Hcsr501ValueChanged(sender, new Hcsr501ValueChangedEventArgs(_controller.Read(_pinOut)));
        }
    }
}
