// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iot.Device.Gpio
{
    /// <summary>
    /// A virtual GPIO controller that serves primarily for a pin mapping on existing controllers.
    /// Unlike <see cref="VirtualGpioController"/> it takes controllers and pin mappings
    /// and thus also supports <see cref="GpioController.OpenPin(int)"/> and its overloads.
    /// </summary>
    public class MappingGpioController : VirtualGpioController
    {
        private readonly List<(GpioController Controller, Func<int, int> FromVirtualRoRealMapping)> _mappings;

        /// <summary>
        /// Create a controller that by default uses the given controller to open a pin, using the given mapping.
        /// Note that you can still use <see cref="VirtualGpioController.Add(int, GpioPin)"/> to manually add pins from another controller.
        /// </summary>
        /// <param name="defaultController">The default controller</param>
        /// <param name="fromVirtualToRealMapping">A mapping function from virtual to logical. This should return -1 for unknown pins.</param>
        public MappingGpioController(GpioController defaultController, Func<int, int> fromVirtualToRealMapping)
        {
            ArgumentNullException.ThrowIfNull(defaultController);
            ArgumentNullException.ThrowIfNull(fromVirtualToRealMapping);
            _mappings = new List<(GpioController Controller, Func<int, int> FromVirtualRoRealMapping)>();
            _mappings.Add((defaultController, fromVirtualToRealMapping));
        }

        /// <summary>
        /// Sets up a virtual GPIO controller that maps to different real controllers.
        /// On every request to open a pin, the given mapping functions are tried until one returns with a valid real pin.
        /// If the virtual pin numbers in the mapping aren't unique, the first one wins.
        /// </summary>
        /// <param name="mappings">A list of controllers with their corresponding mapping functions</param>
        public MappingGpioController(IEnumerable<(GpioController Controller, Func<int, int> FromVirtualToRealMapping)> mappings)
        {
            ArgumentNullException.ThrowIfNull(mappings);
            _mappings = new List<(GpioController Controller, Func<int, int> FromVirtualRoRealMapping)>();
            _mappings.AddRange(mappings);
        }

        /// <summary>
        /// Opens a pin from the default controller
        /// </summary>
        /// <param name="pinNumber">The pin number</param>
        protected override void OpenPinCore(int pinNumber)
        {
            var (controller, realPinNumber) = MapToRealPin(pinNumber);
            if (controller == null)
            {
                throw new InvalidOperationException($"Virtual Pin Number {pinNumber} is unknown");
            }

            var pin = controller.OpenPin(realPinNumber);
            if (!Add(pinNumber, pin))
            {
                controller.ClosePin(realPinNumber);
                throw new InvalidOperationException($"Virtual Pin Number {pinNumber} failed to open.");
            }
        }

        /// <summary>
        /// Returns the real pin associated with this virtual pin
        /// </summary>
        /// <param name="virtualPin">The virtual pin to query</param>
        /// <returns>A real pin number or <code>default</code> if the provided virtual pin number was unknown</returns>
        public virtual (GpioController Controller, int RealPinNumber) MapToRealPin(int virtualPin)
        {
            foreach (var m in _mappings)
            {
                int mapped = m.FromVirtualRoRealMapping(virtualPin);
                if (mapped != -1)
                {
                    return (m.Controller, mapped);
                }
            }

            return default;
        }
    }
}
