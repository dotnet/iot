// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

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
        public Led One => LedArray[0];

        /// <summary>
        /// Yellow led (#2)
        /// </summary>
        public Led Two => LedArray[1];

        /// <summary>
        /// Red led (#3)
        /// </summary>
        public Led Three => LedArray[2];

        /// <summary>
        /// Green led (#4)
        /// </summary>
        public Led Four => LedArray[3];

        /// <summary>
        /// Blue led (#1)
        /// </summary>
        public Led Blue => LedArray[0];

        /// <summary>
        /// Yellow led (#2)
        /// </summary>
        public Led Yellow => LedArray[1];

        /// <summary>
        /// Red led (#3)
        /// </summary>
        public Led Red => LedArray[2];

        /// <summary>
        /// Green led (#4)
        /// </summary>
        public Led Green => LedArray[3];

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
                new (LED1_PIN, _controller),
                new (LED2_PIN, _controller),
                new (LED3_PIN, _controller),
                new (LED4_PIN, _controller)
            };
        }

        /// <summary>
        /// Switch on all led lights
        /// </summary>
        public void On()
        {
            foreach (Led led in LedArray)
            {
                led.On();
            }
        }

        /// <summary>
        /// Switch off all led lights
        /// </summary>
        public void Off()
        {
            foreach (Led led in LedArray)
            {
                led.Off();
            }
        }

        private bool _shouldDispose;

        /// <summary>
        /// Disposes the <see cref="Lights"/> instance
        /// </summary>
        public void Dispose()
        {
            Off();
            if (_shouldDispose)
            {
                _controller?.Dispose();
                _controller = null!;
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection of leds
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection of leds</returns>
        public IEnumerator<Led> GetEnumerator() => LedArray.GetEnumerator();

        /// <summary>
        /// Returns an enumerator that iterates through the collection of leds
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection of leds</returns>
        IEnumerator IEnumerable.GetEnumerator() => LedArray.GetEnumerator();
    }
}
