// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Iot.Device.Gpio
{
    /// <summary>
    /// A virtual GPIO controller that serves primarily for a pin mapping on an existing controller.
    /// Unlike <see cref="VirtualGpioController"/> it has a default controller rather than individual pins
    /// and thus also supports <see cref="GpioController.OpenPin(int)"/>.
    /// </summary>
    public class VirtualGpioControllerWithDefault : VirtualGpioController
    {
        private readonly GpioController _defaultController;
        private readonly Func<int, int> _fromVirtualToRealMapping;

        /// <summary>
        /// Create a controller that by default uses the given controller to open a pin, using the given mapping.
        /// Note that you can still use <see cref="VirtualGpioController.Add(int, GpioPin)"/> to manually add pins from another controller.
        /// </summary>
        /// <param name="defaultController">The default controller</param>
        /// <param name="fromVirtualToRealMapping">A mapping function from virtual to logical. This should return -1 for unknown pins.</param>
        public VirtualGpioControllerWithDefault(GpioController defaultController, Func<int, int> fromVirtualToRealMapping)
        {
            _defaultController = defaultController ?? throw new ArgumentNullException(nameof(defaultController));
            _fromVirtualToRealMapping = fromVirtualToRealMapping ?? throw new ArgumentNullException(nameof(fromVirtualToRealMapping));
        }

        /// <summary>
        /// Opens a pin from the default controller
        /// </summary>
        /// <param name="pinNumber">The pin number</param>
        protected override void OpenPinCore(int pinNumber)
        {
            int realPinNumber = MapToRealPin(pinNumber);
            if (realPinNumber == -1)
            {
                throw new InvalidOperationException($"Virtual Pin Number {pinNumber} is unknown");
            }

            var pin = _defaultController.OpenPin(realPinNumber);
            if (!Add(pinNumber, pin))
            {
                _defaultController.ClosePin(realPinNumber);
                throw new InvalidOperationException($"Virtual Pin Number {pinNumber} is in use already");
            }
        }

        public int MapToRealPin(int virtualPin)
        {
            return _fromVirtualToRealMapping(virtualPin);
        }
    }
}
