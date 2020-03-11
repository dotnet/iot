// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System.Collections.Generic;
using System.Threading;
using System.Runtime.InteropServices;
using System.Collections.Concurrent;

namespace System.Device.Gpio.Drivers
{
    public class LibGpiodDriver : UnixDriver
    {
        private SafeChipHandle _chip;

        private Dictionary<int, SafeLineHandle> _pinNumberToSafeLineHandle;

        private ConcurrentDictionary<int, LibGpiodDriverEventHandler> _pinNumberToEventHandler = new ConcurrentDictionary<int, LibGpiodDriverEventHandler>();

        protected internal override int PinCount => Interop.libgpiod.gpiod_chip_num_lines(_chip);

        private static string s_consumerName = System.Diagnostics.Process.GetCurrentProcess().ProcessName;

        private enum RequestFlag : ulong {
            GPIOD_LINE_REQUEST_FLAG_OPEN_DRAIN = (1UL << 0),
            GPIOD_LINE_REQUEST_FLAG_OPEN_SOURCE = (1UL << 1),
            GPIOD_LINE_REQUEST_FLAG_ACTIVE_LOW = (1UL << 2),
            GPIOD_LINE_REQUEST_FLAG_BIAS_DISABLE = (1UL << 3),
            GPIOD_LINE_REQUEST_FLAG_BIAS_PULL_DOWN = (1UL << 4),
            GPIOD_LINE_REQUEST_FLAG_BIAS_PULL_UP = (1UL << 5)
        };

        public LibGpiodDriver(int gpioChip = 0)
        {
            try
            {
                _chip = Interop.libgpiod.gpiod_chip_open_by_number(gpioChip);
                if (_chip == null)
                {
                    throw ExceptionHelper.GetIOException(ExceptionResource.NoChipFound, Marshal.GetLastWin32Error());
                }

                _pinNumberToSafeLineHandle = new Dictionary<int, SafeLineHandle>(PinCount);
            }
            catch (DllNotFoundException)
            {
                throw ExceptionHelper.GetPlatformNotSupportedException(ExceptionResource.LibGpiodNotInstalled);
            }
        }

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

        protected internal override void ClosePin(int pinNumber)
        {
            if (_pinNumberToSafeLineHandle.TryGetValue(pinNumber, out SafeLineHandle pinHandle) && !IsListeningEvent(pinNumber))
            {
                pinHandle?.Dispose();
                _pinNumberToSafeLineHandle.Remove(pinNumber);
            }
        }

        private bool IsListeningEvent(int pinNumber)
        {
            return _pinNumberToEventHandler.ContainsKey(pinNumber);
        }

        protected internal override int ConvertPinNumberToLogicalNumberingScheme(int pinNumber) =>
            throw ExceptionHelper.GetPlatformNotSupportedException(ExceptionResource.ConvertPinNumberingSchemaError);

        protected internal override PinMode GetPinMode(int pinNumber)
        {
            if (!_pinNumberToSafeLineHandle.TryGetValue(pinNumber, out SafeLineHandle pinHandle))
            {
                throw ExceptionHelper.GetInvalidOperationException(ExceptionResource.PinNotOpenedError, pin: pinNumber);
            }

            return pinHandle.PinMode;
        }

        protected internal override bool IsPinModeSupported(int pinNumber, PinMode mode)
        {
            switch (mode)
            {
                case PinMode.Input:
                    return true;
                case PinMode.InputPullDown:
                case PinMode.InputPullUp:
                    // for use the bias flags we need libgpiod version 1.5 or later
                    IntPtr libgpiodVersionPtr = Interop.libgpiod.gpiod_version_string();
                    string libgpiodVersionMatch = Marshal.PtrToStringAnsi(libgpiodVersionPtr);
                    Version libgpiodVersion = new Version(libgpiodVersionMatch);
                    bool isLibgpiod1dot5 = (libgpiodVersion.Major >= 1 && libgpiodVersion.Minor >= 5);

                    // for use the bias flags we need Kernel version 5.5 or later
                    string[] linuxVersionMatch = RuntimeInformation.OSDescription.Replace("Linux ", "").Split('.');
                    Version linuxVersion = new Version();
                    bool isKernelLinux5dot5 = false;

                    if (linuxVersionMatch.Length > 1)
                    {
                        linuxVersion = new Version($"{linuxVersionMatch[0]}.{linuxVersionMatch[1]}");
                        isKernelLinux5dot5 = (linuxVersion.Major >= 5 && libgpiodVersion.Minor >= 5);
                    }

                    // check if we have the correct versions
                    if (isKernelLinux5dot5 && isLibgpiod1dot5)
                        return true;
                    else
                        Console.Error.WriteLine($"Using Kernel v{linuxVersion} but v5.5 or later is required.\n" +
                                                $"Using libgpiod v{libgpiodVersion} but v1.5 or later is required.");

                    return false;
                case PinMode.Output:
                    return true;
                default:
                    return false;
            }
        }

        protected internal override void OpenPin(int pinNumber)
        {
            SafeLineHandle pinHandle = Interop.libgpiod.gpiod_chip_get_line(_chip, pinNumber);
            if (pinHandle == null)
            {
                throw ExceptionHelper.GetIOException(ExceptionResource.OpenPinError, Marshal.GetLastWin32Error());
            }

            _pinNumberToSafeLineHandle.Add(pinNumber, pinHandle);
        }

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

        protected internal override void SetPinMode(int pinNumber, PinMode mode)
        {
            int requestResult = -1;
            if (_pinNumberToSafeLineHandle.TryGetValue(pinNumber, out SafeLineHandle pinHandle))
            {
                int flags;

                switch (mode)
                {
                    case PinMode.Input:
                        requestResult = Interop.libgpiod.gpiod_line_request_input(pinHandle, s_consumerName);
                        break;
                    case PinMode.InputPullDown:
                        flags = (int) RequestFlag.GPIOD_LINE_REQUEST_FLAG_BIAS_PULL_DOWN;
                        requestResult = Interop.libgpiod.gpiod_line_request_input_flags(pinHandle, s_consumerName, flags);
                        break;
                    case PinMode.InputPullUp:
                        flags = (int) RequestFlag.GPIOD_LINE_REQUEST_FLAG_BIAS_PULL_UP;
                        requestResult = Interop.libgpiod.gpiod_line_request_input_flags(pinHandle, s_consumerName, flags);
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
                void Callback(object o, PinValueChangedEventArgs e)
                {
                    eventOccurred = true;
                }

                WaitForEventResult(cancellationToken, eventHandler.CancellationTokenSource.Token, ref eventOccurred);
                RemoveCallbackForPinValueChangedEvent(pinNumber, Callback);

                return new WaitForEventResult
                {
                    TimedOut = !eventOccurred,
                    EventTypes = eventTypes
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

        protected internal override void Write(int pinNumber, PinValue value)
        {
            if (!_pinNumberToSafeLineHandle.TryGetValue(pinNumber, out SafeLineHandle pinHandle))
            {
                throw ExceptionHelper.GetInvalidOperationException(ExceptionResource.PinNotOpenedError, pin: pinNumber);
            }

            Interop.libgpiod.gpiod_line_set_value(pinHandle, (value == PinValue.High) ? 1 : 0);
        }

        protected override void Dispose(bool disposing)
        {
            if (_pinNumberToEventHandler != null)
            {
                foreach (KeyValuePair<int, LibGpiodDriverEventHandler> kv in _pinNumberToEventHandler)
                {
                    int pin = kv.Key;
                    LibGpiodDriverEventHandler eventHandler = kv.Value;
                    eventHandler.Dispose();
                }

                _pinNumberToEventHandler = null;
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

                _pinNumberToSafeLineHandle = null;
            }

            _chip?.Dispose();
            _chip = null;

            base.Dispose(disposing);
        }
    }
}
