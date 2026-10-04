// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Device.Gpio;

namespace Iot.Device.ExplorerHat
{
    /// <summary>
    /// Represents the Explorer HAT digital inputs collection
    /// </summary>
    /// <remarks>
    /// No pin is opened until an input is read, so the inputs that are not used can be used by other bindings.
    /// </remarks>
    public class Inputs : IDisposable, IEnumerable<DigitalInput>
    {
        private const int INPUT1_PIN = 23;
        private const int INPUT2_PIN = 22;
        private const int INPUT3_PIN = 24;
        private const int INPUT4_PIN = 25;

        private readonly List<DigitalInput> _inputArray;

        /// <summary>
        /// Input #1 (GPIO 23)
        /// </summary>
        public DigitalInput One => _inputArray[0];

        /// <summary>
        /// Input #2 (GPIO 22)
        /// </summary>
        public DigitalInput Two => _inputArray[1];

        /// <summary>
        /// Input #3 (GPIO 24)
        /// </summary>
        public DigitalInput Three => _inputArray[2];

        /// <summary>
        /// Input #4 (GPIO 25)
        /// </summary>
        public DigitalInput Four => _inputArray[3];

        /// <summary>
        /// Initializes a <see cref="Inputs"/> instance
        /// </summary>
        /// <param name="controller"><see cref="GpioController"/> used by <see cref="Inputs"/> to manage GPIO resources. It is not disposed</param>
        internal Inputs(GpioController controller)
        {
            _inputArray = new List<DigitalInput>()
            {
                new(INPUT1_PIN, controller),
                new(INPUT2_PIN, controller),
                new(INPUT3_PIN, controller),
                new(INPUT4_PIN, controller)
            };
        }

        /// <summary>
        /// Closes the pins of the inputs that were read
        /// </summary>
        public void Dispose()
        {
            foreach (DigitalInput input in _inputArray)
            {
                input.Dispose();
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection of inputs
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection of inputs</returns>
        public IEnumerator<DigitalInput> GetEnumerator() => _inputArray.GetEnumerator();

        /// <summary>
        /// Returns an enumerator that iterates through the collection of inputs
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection of inputs</returns>
        IEnumerator IEnumerable.GetEnumerator() => _inputArray.GetEnumerator();
    }
}
