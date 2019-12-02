using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Iot.Device.Pigpio
{
    /// <summary>
    /// 
    /// </summary>
    public class Driver : GpioDriver
    {
        private readonly IPEndPoint _endpoint;
        private readonly PigpiodIf _proxy;
        private readonly List<int> _openPins;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="endpoint"></param>
        public Driver(IPEndPoint endpoint)
        {
            _endpoint = endpoint;
            _proxy = new PigpiodIf();

            _openPins = new List<int>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Task ConnectAsync()
        {
            _proxy.pigpio_start(_endpoint.Address.ToString(), _endpoint.Port.ToString());

            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        protected override int PinCount => PigpiodIf.PI_MAX_USER_GPIO;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pinNumber"></param>
        /// <param name="eventTypes"></param>
        /// <param name="callback"></param>
        protected override void AddCallbackForPinValueChangedEvent(int pinNumber, PinEventTypes eventTypes, PinChangeEventHandler callback)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        protected override void ClosePin(int pinNumber)
        {
            if (_openPins.Contains(pinNumber))
            {
                _openPins.Remove(pinNumber);
            }
            else
            {
                throw new InvalidOperationException($"Pin '{pinNumber}' hasn't been opened");
            }
        }

        /// <inheritdoc/>
        protected override int ConvertPinNumberToLogicalNumberingScheme(int pinNumber)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        protected override PinMode GetPinMode(int pinNumber)
        {
            var mode = _proxy.get_mode((uint)pinNumber);

            switch (mode)
            {
                case PigpiodIf.PI_INPUT: return PinMode.Input;
                case PigpiodIf.PI_OUTPUT: return PinMode.Output;
                default: throw new ArgumentException($"Unknown PinMode value '{mode}'");
            }
        }

        /// <inheritdoc/>
        protected override bool IsPinModeSupported(int pinNumber, PinMode mode)
        {
            switch (mode)
            {
                case PinMode.Input: return true;
                case PinMode.InputPullUp: return true;
                case PinMode.InputPullDown: return true;
                case PinMode.Output: return true;
                default: return false; // Only input & output supported ATM. Should be increased to support input-pullup/pulldown
            }
        }

        /// <inheritdoc/>
        protected override void OpenPin(int pinNumber)
        {
            if (!_openPins.Contains(pinNumber))
            {
                _openPins.Add(pinNumber);
            }
            else
            {
                throw new InvalidOperationException($"Pin '{pinNumber}' is already been open");
            }
        }

        /// <inheritdoc/>
        protected override PinValue Read(int pinNumber)
        {
            return _proxy.gpio_read((uint)pinNumber);
        }

        /// <inheritdoc/>
        protected override void RemoveCallbackForPinValueChangedEvent(int pinNumber, PinChangeEventHandler callback)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        protected override void SetPinMode(int pinNumber, PinMode pinMode)
        {
            var mode = pinMode.AsMode();
            var pud = pinMode.AsPullUpDown();

            _proxy.set_mode((uint)pinNumber, mode);
            _proxy.set_pull_up_down((uint)pinNumber, pud);
        }

        /// <inheritdoc/>
        protected override WaitForEventResult WaitForEvent(int pinNumber, PinEventTypes eventTypes, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        protected override void Write(int pinNumber, PinValue pinValue)
        {
            var value = pinValue.AsValue();

            _proxy.gpio_write((uint)pinNumber, value);
        }
    }
}
