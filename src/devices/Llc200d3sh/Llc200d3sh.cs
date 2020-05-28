using System;
using System.Device.Gpio;

namespace Iot.Device.Llc200d3sh
{
    /// <summary>
    /// Optomax digital liquid level switch
    /// </summary>
    public class Llc200d3sh : IDisposable
    {
        private readonly int _pin;
        private readonly bool _shouldDispose;

        private GpioController _controller;

        /// <summary>Initializes a new instance of the <see cref="Llc200d3sh" /> class.</summary>
        /// <param name="pin">The data pin</param>
        /// <param name="pinNumberingScheme">Use the logical or physical pin layout</param>
        /// <param name="gpioController">A Gpio Controller if you want to use a specific one</param>
        /// <param name="shouldDispose">True to dispose the Gpio Controller</param>
        public Llc200d3sh(int pin, GpioController gpioController = null,
                          PinNumberingScheme pinNumberingScheme = PinNumberingScheme.Logical, bool shouldDispose = true)
        {
            _controller = gpioController != null ? gpioController : new GpioController(pinNumberingScheme);
            _pin = pin;

            _controller.OpenPin(_pin, PinMode.Input);

            _shouldDispose = gpioController == null ? true : shouldDispose;
        }

        /// <summary>Read the digital liquid level switch value</summary>
        /// <returns></returns>
        public DetectionType ReadValue()
        {
            var value = _controller.Read(_pin);

            return value == PinValue.Low ? DetectionType.Liquid : DetectionType.Air;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (_shouldDispose)
            {
                _controller?.Dispose();
                _controller = null;
            }
        }
    }
}
