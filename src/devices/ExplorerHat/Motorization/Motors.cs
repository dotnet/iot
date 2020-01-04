using System;
using System.Collections.Generic;
using System.Device.Gpio;
using Serilog;

namespace Iot.Device.ExplorerHat.Motorization
{
    /// <summary>
    /// Represents the Explorer HAT DCMotors collection
    /// </summary>
    public class Motors : IDisposable
    {
        private const int MOTOR1_SPDPIN = 19;
        private const int MOTOR1_DIRPIN = 20;
        private const int MOTOR2_SPDPIN = 21;
        private const int MOTOR2_DIRPIN = 26;

        private GpioController _controller;

        private List<Motor> _motorArray;

        /// <summary>
        /// Gets the <see cref="Motor"/> at the specified index
        /// </summary>
        /// <param name="key">The zero-based (0 to 1) of the motor to get</param>
        /// <returns>The <see cref="Motor"/> at the specified index</returns>
        public Motor this[int key]
        {
            get
            {
                if (key < 0 || key > 1)
                {
                    throw new Exception("Motors are 0..1");
                }

                return _motorArray[key];
            }
        }

        /// <summary>
        /// Motor #1
        /// </summary>
        /// <value></value>
        public Motor One { get => _motorArray[0]; }

        /// <summary>
        /// Motor #2
        /// </summary>
        /// <value></value>
        public Motor Two { get => _motorArray[1]; }

        /// <summary>
        /// Both motors turns forwards at indicated speed
        /// </summary>
        /// <param name="speed">Indicated speed</param>
        public void Forwards(double speed = 1)
        {
            _motorArray[0].Forwards(speed);
            _motorArray[1].Forwards(speed);
        }

        /// <summary>
        /// Both motors turns backwards at indicated speed
        /// </summary>
        /// <param name="speed">Indicated speed</param>
        public void Backwards(double speed = 1)
        {
            _motorArray[0].Backwards(speed);
            _motorArray[1].Backwards(speed);
        }

        /// <summary>
        /// Both motors stops
        /// </summary>
        public void Stop()
        {
            _motorArray[0].Stop();
            _motorArray[1].Stop();
        }

        /// <summary>
        /// Initializes a <see cref="Motors"/> instance
        /// </summary>
        /// <param name="controller"><see cref="GpioController"/> used by <see cref="Motors"/> to manage GPIO resources</param>
        internal Motors(GpioController controller)
        {
            _controller = controller;

            _motorArray = new List<Motor>()
            {
                new Motor(1, MOTOR1_SPDPIN, MOTOR1_DIRPIN, _controller),
                new Motor(2, MOTOR2_SPDPIN, MOTOR2_DIRPIN, _controller)
            };
            var featureName = "Motorization";
            Log.Information("{featureName} initialized", featureName);
        }

        #region IDisposable Support

        private bool _disposedValue = false;

        /// <summary>
        /// Disposes the <see cref="Motors"/> instance
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    var featureName = "Motorization";
                    Log.Debug("Disposing {featureName} features", featureName);
                    _motorArray[0].Dispose();
                    _motorArray[1].Dispose();
                    Log.Information("{featureName} features disposed", featureName);
                }

                _disposedValue = true;
            }
        }

        /// <summary>
        /// Disposes the <see cref="Motors"/> instance
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        #endregion
    }
}
