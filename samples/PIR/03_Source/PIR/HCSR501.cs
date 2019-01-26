using System;
using System.Collections.Generic;
using System.Devices.Gpio;
using System.Text;

namespace PIR
{
    public class HCSR501ValueChangedEventArgs : EventArgs
    {
        public readonly PinValue PinValue;
        public HCSR501ValueChangedEventArgs(PinValue value)
        {
            PinValue = value;
        }
    }

    public class HCSR501 : IDisposable
    {
        private GpioPin sensor;
        private readonly int pinOut;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="pin">OUT Pin</param>
        public HCSR501(int pin)
        {
            pinOut = pin;
        }

        /// <summary>
        /// Initialize the sensor
        /// </summary>
        public void Initialize()
        {
            GpioController controller = new GpioController(PinNumberingScheme.Gpio);

            sensor = controller.OpenPin(pinOut, PinMode.Input);

            sensor.ValueChanged += Sensor_ValueChanged;
        }

        /// <summary>
        /// Read from the sensor
        /// </summary>
        /// <returns>Is Detected</returns>
        public bool Read()
        {
            if (sensor.Read() == PinValue.High)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Get the sensor raw GpioPin
        /// </summary>
        /// <returns>GpioPin</returns>
        public GpioPin GetRaw()
        {
            return sensor;
        }

        /// <summary>
        /// Cleanup
        /// </summary>
        public void Dispose()
        {
            sensor.Dispose();
        }

        public delegate void HCSR501ValueChangedHandle(object sender, HCSR501ValueChangedEventArgs e);

        /// <summary>
        /// Triggering when HC-SR501 value changes
        /// </summary>
        public event HCSR501ValueChangedHandle HCSR501ValueChanged;

        private void Sensor_ValueChanged(object sender, PinValueChangedEventArgs e)
        {
            HCSR501ValueChanged(sender, new HCSR501ValueChangedEventArgs(sensor.Read()));
        }
    }
}
