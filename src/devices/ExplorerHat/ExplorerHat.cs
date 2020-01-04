using System;
using System.Device.Gpio;
using Iot.Device.ExplorerHat.Lighting;
using Iot.Device.ExplorerHat.Motorization;
using Serilog;

namespace Iot.Device.ExplorerHat
{
    /// <summary>
    /// Pimoroni Explorer HAT for Raspberry Pi
    /// </summary>
    public class ExplorerHat : IDisposable
    {
        private readonly bool _shouldDispose;
        private GpioController _controller;

        /// <summary>
        /// Explorer HAT DCMotors collection
        /// </summary>
        public Motors Motors { get; private set; }

        /// <summary>
        /// Explorer HAT led array
        /// </summary>
        public Lights Lights { get; private set; }

        /// <summary>
        /// Initialize <see cref="ExplorerHat"/> instance
        /// </summary>
        public ExplorerHat(GpioController controller = null, bool shouldDispose = true)
        {
            _controller = controller;
            _shouldDispose = shouldDispose;
            Motors = new Motors(_controller);
            Lights = new Lights(_controller);

            if (_controller is null)
            {
                _controller = new GpioController(PinNumberingScheme.Logical);
            }

            if (_controller.NumberingScheme != PinNumberingScheme.Logical)
            {
                throw new ArgumentException("Invalid GpioController config: NumberingScheme value must be 'Logical'", nameof(controller));
            }
        }

        #region IDisposable Support

        private bool _disposedValue = false;

        /// <summary>
        /// Disposes the <see cref="ExplorerHat"/> instance
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    var featureName = "Explorer HAT resources";
                    var gpioFeatureName = "Managed GpioController resources";
                    Log.Debug("Disposing {featureName}", featureName);
                    Lights.Dispose();
                    Motors.Dispose();

                    if (_shouldDispose)
                    {
                        Log.Debug("Disposing {featureName}", gpioFeatureName);
                        _controller.Dispose();
                        Log.Debug("{featureName} disposed", gpioFeatureName);
                    }

                    Log.Information("{featureName} features disposed", featureName);
                }

                _disposedValue = true;
            }
        }

        /// <summary>
        /// Disposes the <see cref="ExplorerHat"/> instance
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        #endregion
    }
}
