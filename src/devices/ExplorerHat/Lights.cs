// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Device.Gpio;

namespace Iot.Device.ExplorerHat
{
    /// <summary>
    /// Represents the Explorer HAT led array
    /// </summary>
    public class Lights : IDisposable, IEnumerable<Led>
    {
        private const int LED1_PIN = 4;
        private const int LED2_PIN = 17;
        private const int LED3_PIN = 27;
        private const int LED4_PIN = 5;

        private GpioController _controller;

        private List<Led> LedArray { get; set; }

        /// <summary>
        /// Blue led (#1)
        /// </summary>
        public Led One { get => LedArray[0]; }

        /// <summary>
        /// Yellow led (#2)
        /// </summary>
        public Led Two { get => LedArray[1]; }

        /// <summary>
        /// Red led (#3)
        /// </summary>
        public Led Three { get => LedArray[2]; }

        /// <summary>
        /// Green led (#4)
        /// </summary>
        public Led Four { get => LedArray[3]; }

        /// <summary>
        /// Blue led (#1)
        /// </summary>
        public Led Blue { get => LedArray[0]; }

        /// <summary>
        /// Yellow led (#2)
        /// </summary>
        public Led Yellow { get => LedArray[1]; }

        /// <summary>
        /// Red led (#3)
        /// </summary>
        public Led Red { get => LedArray[2]; }

        /// <summary>
        /// Green led (#4)
        /// </summary>
        public Led Green { get => LedArray[3]; }

        /// <summary>
        /// Initializes a <see cref="Lights"/> instance
        /// </summary>
        /// <param name="controller"><see cref="GpioController"/> used by <see cref="Lights"/> to manage GPIO resources</param>
        /// <param name="shouldDispose">True to dispose the Gpio Controller</param>
        internal Lights(GpioController controller, bool shouldDispose = true)
        {
            _controller = controller;
            _shouldDispose = shouldDispose;

            LedArray = new List<Led>()
            {
                new Led(LED1_PIN, _controller),
                new Led(LED2_PIN, _controller),
                new Led(LED3_PIN, _controller),
                new Led(LED4_PIN, _controller)
            };
        }

        /// <summary>
        /// Switch on all led lights
        /// </summary>
        public void On()
        {
            LedArray[0].On();
            LedArray[1].On();
            LedArray[2].On();
            LedArray[3].On();
        }

        /// <summary>
        /// Switch off all led lights
        /// </summary>
        public void Off()
        {
            LedArray[0].Off();
            LedArray[1].Off();
            LedArray[2].Off();
            LedArray[3].Off();
        }

        #region IDisposable Support

        private bool _shouldDispose;
        // This to avoid double dispose
        private bool _disposedValue = false;

        /// <summary>
        /// Disposes the <see cref="Lights"/> instance
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    Off();
                    if (_shouldDispose)
                    {
                        _controller?.Dispose();
                        _controller = null;
                    }
                }

                _disposedValue = true;
            }
        }

        /// <summary>
        /// Disposes the <see cref="Lights"/> instance
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection of leds
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection of leds</returns>
        public IEnumerator<Led> GetEnumerator()
        {
            return ((IEnumerable<Led>)LedArray).GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection of leds
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection of leds</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<Led>)LedArray).GetEnumerator();
        }

        #endregion
    }
}
