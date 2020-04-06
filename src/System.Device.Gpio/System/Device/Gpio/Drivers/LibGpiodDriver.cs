// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System.Collections.Generic;
using System.Threading;
using System.Runtime.InteropServices;
using System.Collections.Concurrent;

namespace System.Device.Gpio.Drivers
{
    /// <summary>
    /// This driver uses the Libgpiod library to get user-level access to the gpio ports.
    /// It superseeds the SysFsDriver
    /// </summary>
    public class LibGpiodDriver : UnixDriver
    {
        private static string s_consumerName = System.Diagnostics.Process.GetCurrentProcess().ProcessName;
        private readonly object _pinNumberLock;
        private readonly ConcurrentDictionary<int, SafeLineHandle> _pinNumberToSafeLineHandle;
        private readonly ConcurrentDictionary<int, LibGpiodDriverEventHandler> _pinNumberToEventHandler;
        private readonly int _pinCount;
        private SafeChipHandle _chip;

        protected internal override int PinCount => _pinCount;

        // for use the bias flags we need libgpiod version 1.5 or later
        private static bool IsLibgpiodVersion1_5orHigher()
        {
            IntPtr libgpiodVersionPtr = Interop.libgpiod.gpiod_version_string();
            string libgpiodVersionMatch = Marshal.PtrToStringAnsi(libgpiodVersionPtr);
            Version libgpiodVersion = new Version(libgpiodVersionMatch);

            return (libgpiodVersion.Major >= 1 && libgpiodVersion.Minor >= 5);
        }

        private static bool s_isLibgpiodVersion1_5orHigher = IsLibgpiodVersion1_5orHigher();

        private enum RequestFlag : ulong
        {
            GPIOD_LINE_REQUEST_FLAG_OPEN_DRAIN = (1UL << 0),
            GPIOD_LINE_REQUEST_FLAG_OPEN_SOURCE = (1UL << 1),
            GPIOD_LINE_REQUEST_FLAG_ACTIVE_LOW = (1UL << 2),
            GPIOD_LINE_REQUEST_FLAG_BIAS_DISABLE = (1UL << 3),
            GPIOD_LINE_REQUEST_FLAG_BIAS_PULL_DOWN = (1UL << 4),
            GPIOD_LINE_REQUEST_FLAG_BIAS_PULL_UP = (1UL << 5)
        }

        /// <summary>
        /// Construct an instance
        /// </summary>
        /// <param name="gpioChip">Number of the gpio Chip. Default 0</param>
        public LibGpiodDriver(int gpioChip = 0)
        {
            if (Environment.OSVersion.Platform != PlatformID.Unix)
            {
                throw new PlatformNotSupportedException($"{GetType().Name} is only supported on Linux/Unix.");
            }

            try
            {
                _pinNumberLock = new object();
                _chip = Interop.libgpiod.gpiod_chip_open_by_number(gpioChip);
                if (_chip == null)
                {
                    throw ExceptionHelper.GetIOException(ExceptionResource.NoChipFound, Marshal.GetLastWin32Error());
                }

                _pinCount = Interop.libgpiod.gpiod_chip_num_lines(_chip);
                _pinNumberToEventHandler = new ConcurrentDictionary<int, LibGpiodDriverEventHandler>();
                _pinNumberToSafeLineHandle = new ConcurrentDictionary<int, SafeLineHandle>();
            }
            catch (DllNotFoundException)
            {
                throw ExceptionHelper.GetPlatformNotSupportedException(ExceptionResource.LibGpiodNotInstalled);
            }
        }

        /// <inheritdoc/>
        protected internal override void AddCallbackForPinValueChangedEvent(int pinNumber, PinEventTypes eventTypes, PinChangeEventHandler callback)
        {
            if ((eventTypes & PinEventTypes.Rising) != 0 || (eventTypes & PinEventTypes.Falling) != 0)
            {
                LibGpiodDriverEventHandler eventHandler = _pinNumberToEventHandler.GetOrAdd(pinNumber, PopulateEventHandler);

                if ((eventTypes & PinEventTypes.Rising) != 0)
                {
                    eventHandler.ValueRising += callback;
                }

                if ((eventTypes & PinEventTypes.Falling) != 0)
                {
                    eventHandler.ValueFalling += callback;
                }
            }
            else
            {
                throw ExceptionHelper.GetArgumentException(ExceptionResource.InvalidEventType);
            }
        }

        private LibGpiodDriverEventHandler PopulateEventHandler(int pinNumber)
        {
            lock (_pinNumberLock)
            {
            if (_pinNumberToSafeLineHandle.TryGetValue(pinNumber, out SafeLineHandle pinHandle))
            {
                if (!Interop.libgpiod.gpiod_line_is_free(pinHandle))
                {
                    pinHandle.Dispose();
                    pinHandle = Interop.libgpiod.gpiod_chip_get_line(_chip, pinNumber);
                    _pinNumberToSafeLineHandle[pinNumber] = pinHandle;
                }
            }

            return new LibGpiodDriverEventHandler(pinNumber, pinHandle);
        }
        }

        /// <inheritdoc/>
        protected internal override void ClosePin(int pinNumber)
        {
            lock (_pinNumberLock)
            {
                if (_pinNumberToSafeLineHandle.TryGetValue(pinNumber, out SafeLineHandle pinHandle) &&
                    !IsListeningEvent(pinNumber))
                {
                pinHandle?.Dispose();
                    // We know this works
                    _pinNumberToSafeLineHandle.TryRemove(pinNumber, out _);
            }
        }
        }

        private bool IsListeningEvent(int pinNumber)
        {
            return _pinNumberToEventHandler.ContainsKey(pinNumber);
        }

        /// <inheritdoc/>
        protected internal override int ConvertPinNumberToLogicalNumberingScheme(int pinNumber) =>
            throw ExceptionHelper.GetPlatformNotSupportedException(ExceptionResource.ConvertPinNumberingSchemaError);

        /// <inheritdoc/>
        protected internal override PinMode GetPinMode(int pinNumber)
        {
            lock (_pinNumberLock)
            {
            if (!_pinNumberToSafeLineHandle.TryGetValue(pinNumber, out SafeLineHandle pinHandle))
            {
                    throw ExceptionHelper.GetInvalidOperationException(ExceptionResource.PinNotOpenedError,
                        pin: pinNumber);
            }

            return pinHandle.PinMode;
        }
        }

        /// <inheritdoc/>
        protected internal override bool IsPinModeSupported(int pinNumber, PinMode mode)
        {
            switch (mode)
            {
                case PinMode.Input:
                    return true;
                case PinMode.InputPullDown:
                case PinMode.InputPullUp:
                    return s_isLibgpiodVersion1_5orHigher;
                case PinMode.Output:
                    return true;
                default:
                    return false;
        }
        }

        /// <inheritdoc/>
        protected internal override void OpenPin(int pinNumber)
        {
            lock (_pinNumberLock)
            {
                if (_pinNumberToSafeLineHandle.TryGetValue(pinNumber, out _))
                {
                    return;
                }

            SafeLineHandle pinHandle = Interop.libgpiod.gpiod_chip_get_line(_chip, pinNumber);
            if (pinHandle == null)
            {
                throw ExceptionHelper.GetIOException(ExceptionResource.OpenPinError, Marshal.GetLastWin32Error());
            }

                _pinNumberToSafeLineHandle.TryAdd(pinNumber, pinHandle);
        }
        }

        /// <inheritdoc/>
        protected internal override PinValue Read(int pinNumber)
        {
            if (_pinNumberToSafeLineHandle.TryGetValue(pinNumber, out SafeLineHandle pinHandle))
            {
                int result = Interop.libgpiod.gpiod_line_get_value(pinHandle);
                if (result == -1)
                {
                    throw ExceptionHelper.GetIOException(ExceptionResource.ReadPinError, Marshal.GetLastWin32Error(), pinNumber);
                }

                return result;
            }

            throw ExceptionHelper.GetInvalidOperationException(ExceptionResource.PinNotOpenedError, pin: pinNumber);
        }

        /// <inheritdoc/>
        protected internal override void RemoveCallbackForPinValueChangedEvent(int pinNumber, PinChangeEventHandler callback)
        {
            if (_pinNumberToEventHandler.TryGetValue(pinNumber, out LibGpiodDriverEventHandler eventHandler))
            {
                eventHandler.ValueFalling -= callback;
                eventHandler.ValueRising -= callback;
                if (eventHandler.IsCallbackListEmpty())
                {
                    _pinNumberToEventHandler.TryRemove(pinNumber, out eventHandler);
                    eventHandler.Dispose();
                }
            }
            else
            {
                throw ExceptionHelper.GetInvalidOperationException(ExceptionResource.NotListeningForEventError);
            }
        }

        /// <inheritdoc/>
        protected internal override void SetPinMode(int pinNumber, PinMode mode)
        {
            int requestResult = -1;
            if (_pinNumberToSafeLineHandle.TryGetValue(pinNumber, out SafeLineHandle pinHandle))
            {
                switch (mode)
                {
                    case PinMode.Input:
                    requestResult = Interop.libgpiod.gpiod_line_request_input(pinHandle, s_consumerName);
                        break;
                    case PinMode.InputPullDown:
                        requestResult = Interop.libgpiod.gpiod_line_request_input_flags(pinHandle, s_consumerName, (int)RequestFlag.GPIOD_LINE_REQUEST_FLAG_BIAS_PULL_DOWN);
                        break;
                    case PinMode.InputPullUp:
                        requestResult = Interop.libgpiod.gpiod_line_request_input_flags(pinHandle, s_consumerName, (int)RequestFlag.GPIOD_LINE_REQUEST_FLAG_BIAS_PULL_UP);
                        break;
                    case PinMode.Output:
                    requestResult = Interop.libgpiod.gpiod_line_request_output(pinHandle, s_consumerName);
                        break;
                }

                pinHandle.PinMode = mode;
            }

            if (requestResult == -1)
            {
                throw ExceptionHelper.GetIOException(ExceptionResource.SetPinModeError, Marshal.GetLastWin32Error(), pinNumber);
            }
        }

        /// <inheritdoc/>
        protected internal override WaitForEventResult WaitForEvent(int pinNumber, PinEventTypes eventTypes, CancellationToken cancellationToken)
        {
            if ((eventTypes & PinEventTypes.Rising) != 0 || (eventTypes & PinEventTypes.Falling) != 0)
            {
                LibGpiodDriverEventHandler eventHandler = _pinNumberToEventHandler.GetOrAdd(pinNumber, PopulateEventHandler);

                if ((eventTypes & PinEventTypes.Rising) != 0)
                {
                    eventHandler.ValueRising += Callback;
                }

                if ((eventTypes & PinEventTypes.Falling) != 0)
                {
                    eventHandler.ValueFalling += Callback;
                }

                bool eventOccurred = false;
                PinEventTypes typeOfEventOccured = PinEventTypes.None;
                void Callback(object o, PinValueChangedEventArgs e)
                {
                    eventOccurred = true;
                    typeOfEventOccured = e.ChangeType;
                }

                WaitForEventResult(cancellationToken, eventHandler.CancellationTokenSource.Token, ref eventOccurred);
                RemoveCallbackForPinValueChangedEvent(pinNumber, Callback);

                return new WaitForEventResult
                {
                    TimedOut = !eventOccurred,
                    EventTypes = eventOccurred ? typeOfEventOccured : PinEventTypes.None,
                };
            }
            else
            {
                throw ExceptionHelper.GetArgumentException(ExceptionResource.InvalidEventType);
            }
        }

        private void WaitForEventResult(CancellationToken sourceToken, CancellationToken parentToken, ref bool eventOccurred)
        {
            while (!(sourceToken.IsCancellationRequested || parentToken.IsCancellationRequested || eventOccurred))
            {
                Thread.Sleep(1);
            }
        }

        /// <inheritdoc/>
        protected internal override void Write(int pinNumber, PinValue value)
        {
            if (!_pinNumberToSafeLineHandle.TryGetValue(pinNumber, out SafeLineHandle pinHandle))
            {
                throw ExceptionHelper.GetInvalidOperationException(ExceptionResource.PinNotOpenedError,
                    pin: pinNumber);
            }

            Interop.libgpiod.gpiod_line_set_value(pinHandle, (value == PinValue.High) ? 1 : 0);
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (_pinNumberToEventHandler != null)
            {
                foreach (KeyValuePair<int, LibGpiodDriverEventHandler> kv in _pinNumberToEventHandler)
                {
                    LibGpiodDriverEventHandler eventHandler = kv.Value;
                    eventHandler.Dispose();
                }

                _pinNumberToEventHandler.Clear();
            }

            if (_pinNumberToSafeLineHandle != null)
            {
                foreach (int pin in _pinNumberToSafeLineHandle.Keys)
                {
                    if (_pinNumberToSafeLineHandle.TryGetValue(pin, out SafeLineHandle pinHandle))
                    {
                        pinHandle?.Dispose();
                    }
                }

                _pinNumberToSafeLineHandle.Clear();
            }

            _chip?.Dispose();
            _chip = null;

            base.Dispose(disposing);
        }
    }
}
