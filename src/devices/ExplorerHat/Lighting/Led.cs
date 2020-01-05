using System;
using System.Device.Gpio;
using ExplorerHat.Gpio;

namespace Iot.Device.ExplorerHat.Lighting
{
    /// <summary>
    /// Represents a led light
    /// </summary>
    public class Led : IDisposable
    {
        private GpioController _controller;

        /// <summary>
        /// GPIO pin to which led is attached
        /// </summary>
        public int Pin { get; private set; }

        /// <summary>
        /// Led number on Hat
        /// </summary>
        public int Number { get; private set; }

        /// <summary>
        /// Led name on Hat
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets if led is switched on or not
        /// </summary>
        /// <value></value>
        public bool IsOn { get; set; }

        /// <summary>
        /// Initializes a <see cref="Led"/> instance
        /// </summary>
        /// <param name="number">Led number</param>
        /// <param name="name">Led name</param>
        /// <param name="pin">Underlying rpi GPIO pin number</param>
        /// <param name="controller"><see cref="GpioController"/> used by <see cref="Led"/> to manage GPIO resources</param>
        internal Led(int number, string name, int pin, GpioController controller)
        {
            _controller = controller;
            Number = number;
            Name = name;
            Pin = pin;
            IsOn = false;
        }

        /// <summary>
        /// Switch on this led light
        /// </summary>
        public void On()
        {
            if (!IsOn)
            {
                _controller.EnsureOpenPin(Pin, PinMode.Output);
                _controller.Write(Pin, PinValue.High);
                IsOn = true;
            }
        }

        /// <summary>
        /// Switch off this led light
        /// </summary>
        public void Off()
        {
            if (IsOn)
            {
                _controller.EnsureOpenPin(Pin, PinMode.Output);
                _controller.Write(Pin, PinValue.Low);
                IsOn = false;
            }
        }

        #region IDisposable Support

        private bool _disposedValue = false;

        /// <summary>
        /// Disposes the <see cref="Led"/> instance
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    Off();
                }

                _disposedValue = true;
            }
        }

        /// <summary>
        /// Disposes the <see cref="Led"/> instance
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        #endregion
    }
}
