// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Disposables;

namespace System.Device.Gpio
{
    /// <summary>
    /// Extensions for <see cref="IGpioController"/> interface.
    /// </summary>
    public static class IGpioControllerExtensions
    {
        /// <summary>
        /// Opens a pin in order for it to be ready to use.
        /// </summary>
        /// <param name="controller">The GPioController.</param>
        /// <param name="pinNumber">The pin number in the controller's numbering scheme.</param>
        /// <returns>A disposable that will close the pin if disposed.</returns>
        public static IDisposable OpenPinAsDisposable(this IGpioController controller, int pinNumber)
        {
            controller.OpenPin(pinNumber);
            return Disposable.Create(() => controller.ClosePin(pinNumber));
        }

        /// <summary>
        /// Opens a pin and sets it to a specific mode.
        /// </summary>
        /// <param name="controller">The GPioController.</param>
        /// <param name="pinNumber">The pin number in the controller's numbering scheme.</param>
        /// <param name="mode">The mode to be set.</param>
        /// <returns>A disposable that will close the pin if disposed.</returns>
        public static IDisposable OpenPinAsDisposable(this IGpioController controller, int pinNumber, PinMode mode)
        {
            controller.OpenPin(pinNumber, mode);
            return Disposable.Create(() => controller.ClosePin(pinNumber));
        }

        /// <summary>
        /// Adds a callback that will be invoked when pinNumber has an event of type eventType.
        /// </summary>
        /// <param name="controller">The GPioController.</param>
        /// <param name="pinNumber">The pin number in the controller's numbering scheme.</param>
        /// <param name="eventTypes">The event types to wait for.</param>
        /// <param name="callback">The callback method that will be invoked.</param>
        /// <returns>A disposable object that will remove the added callback </returns>
        public static IDisposable RegisterCallbackForPinValueChangedEventAsDisposable(this IGpioController controller, int pinNumber,
            PinEventTypes eventTypes, PinChangeEventHandler callback)
        {
            controller.RegisterCallbackForPinValueChangedEvent(pinNumber, eventTypes, callback);
            return Disposable.Create(() => controller.UnregisterCallbackForPinValueChangedEvent(pinNumber, callback));
        }
    }
}
