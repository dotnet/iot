// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Device.Gpio;

namespace Iot.Device.ExplorerHat
{
    /// <summary>
    /// Represents the Explorer HAT open collector outputs collection
    /// </summary>
    /// <remarks>
    /// No pin is opened until an output is switched on or off, so the outputs that are not used can be used by
    /// other bindings.
    /// </remarks>
    public class Outputs : IDisposable, IEnumerable<DigitalOutput>
    {
        private const int OUTPUT1_PIN = 6;
        private const int OUTPUT2_PIN = 12;
        private const int OUTPUT3_PIN = 13;
        private const int OUTPUT4_PIN = 16;

        private readonly List<DigitalOutput> _outputArray;

        /// <summary>
        /// Output #1 (GPIO 6)
        /// </summary>
        public DigitalOutput One => _outputArray[0];

        /// <summary>
        /// Output #2 (GPIO 12)
        /// </summary>
        public DigitalOutput Two => _outputArray[1];

        /// <summary>
        /// Output #3 (GPIO 13)
        /// </summary>
        public DigitalOutput Three => _outputArray[2];

        /// <summary>
        /// Output #4 (GPIO 16)
        /// </summary>
        public DigitalOutput Four => _outputArray[3];

        /// <summary>
        /// Initializes a <see cref="Outputs"/> instance
        /// </summary>
        /// <param name="controller"><see cref="GpioController"/> used by <see cref="Outputs"/> to manage GPIO resources. It is not disposed</param>
        internal Outputs(GpioController controller)
        {
            _outputArray = new List<DigitalOutput>()
            {
                new(OUTPUT1_PIN, controller),
                new(OUTPUT2_PIN, controller),
                new(OUTPUT3_PIN, controller),
                new(OUTPUT4_PIN, controller)
            };
        }

        /// <summary>
        /// Switches off the outputs that were used and closes their pins
        /// </summary>
        public void Dispose()
        {
            foreach (DigitalOutput output in _outputArray)
            {
                output.Dispose();
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection of outputs
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection of outputs</returns>
        public IEnumerator<DigitalOutput> GetEnumerator() => _outputArray.GetEnumerator();

        /// <summary>
        /// Returns an enumerator that iterates through the collection of outputs
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection of outputs</returns>
        IEnumerator IEnumerable.GetEnumerator() => _outputArray.GetEnumerator();
    }
}
