// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System.Collections.Generic;
using System.Threading;
using System.Runtime.InteropServices;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace System.Device.Gpio.Drivers
{
    public class LibGpiodDriver : UnixDriver
    {
        private SafeChipHandle _chip;

        private Dictionary<int, SafeLineHandle> _pinNumberToSafeLineHandle;

        private ConcurrentDictionary<int, LibGpiodDriverEventHandler> _pinNumberToEventHandler = new ConcurrentDictionary<int, LibGpiodDriverEventHandler>();

        protected internal override int PinCount => Interop.GetNumberOfLines(_chip);

        public LibGpiodDriver()
        {
            SafeChipIteratorHandle iterator = Interop.GetChipIterator();
            if (iterator == null)
            {
                throw ExceptionHelper.GetIOException(ExceptionResource.NoChipIteratorFound, Marshal.GetLastWin32Error());
            }

            _chip = Interop.GetNextChipFromChipIterator(iterator);
            if (_chip == null)
            {
                throw ExceptionHelper.GetIOException(ExceptionResource.NoChipFound, Marshal.GetLastWin32Error());
            }

            // Freeing other chips opened
            Interop.FreeChipIteratorNoCloseCurrentChip(iterator);
            _pinNumberToSafeLineHandle = new Dictionary<int, SafeLineHandle>(PinCount);
        }

        private SafeLineHandle SubscribeForEvent(int pinNumber, PinEventTypes eventType)
        {
            int eventSuccess = -1; 
            SafeLineHandle pinHandle = Interop.GetChipLineByOffset(_chip, pinNumber);
            if (pinHandle != null)
            {
                if (eventType.HasFlag(PinEventTypes.Rising) || eventType.HasFlag(PinEventTypes.Falling))
                {
                    eventSuccess = Interop.RequestBothEdgeEventForLine(pinHandle, $"Listen {pinNumber} for both edge event");
                }
                else
                {
                    throw ExceptionHelper.GetArgumentException(ExceptionResource.InvalidEventType);
                }
            }

            if (eventSuccess < 0)
            {
                throw ExceptionHelper.GetIOException(ExceptionResource.RequestEventError, pinNumber, Marshal.GetLastWin32Error());
            }
            return pinHandle;
        }

        private Task InitializeEventDetectionTask(LibGpiodDriverEventHandler eventHandler, CancellationToken token)
        {
            Task pinListeningThread = Task.Run(() =>
            {       
                int waitResult = -2;
                while (!token.IsCancellationRequested)
                {
                    waitResult = Interop.WaitForEventOnLine(eventHandler.PinHandle);
                    if (waitResult == -1)
                    {
                        throw ExceptionHelper.GetIOException(ExceptionResource.EventWaitError, Marshal.GetLastWin32Error());
                    }

                    if (waitResult == 1)
                    {
                        int readResult = Interop.ReadEventForLine(eventHandler.PinHandle);
                        if (readResult == -1)
                        {
                            throw ExceptionHelper.GetIOException(ExceptionResource.EventReadError, Marshal.GetLastWin32Error());
                        }

                        PinEventTypes eventType = (readResult == 1) ? PinEventTypes.Rising : PinEventTypes.Falling;
                        var args = new PinValueChangedEventArgs(eventType, eventHandler.PinNumber);
                        eventHandler?.OnPinValueChanged(args, eventType);
                    }
                }
            });
            return pinListeningThread;
        }

        protected internal override void AddCallbackForPinValueChangedEvent(int pinNumber, PinEventTypes eventType, PinChangeEventHandler callback)
        {
            if (!_pinNumberToEventHandler.TryGetValue(pinNumber, out LibGpiodDriverEventHandler eventHandler))
            {
                eventHandler = new LibGpiodDriverEventHandler(pinNumber, new CancellationTokenSource());
                if (_pinNumberToEventHandler.TryAdd(pinNumber, eventHandler))
                {
                    eventHandler.PinHandle = SubscribeForEvent(pinNumber, eventType);
                    eventHandler.Task = InitializeEventDetectionTask(eventHandler, eventHandler.CancellationTokenSource.Token);
                }
            }

            if (eventType.HasFlag(PinEventTypes.Rising))
            {
                eventHandler.ValueRising += callback;
            }
            else if (eventType.HasFlag(PinEventTypes.Falling))
            {
                eventHandler.ValueFalling += callback;
            }
        }
        
        protected internal override void ClosePin(int pinNumber)
        {
            if (_pinNumberToSafeLineHandle.TryGetValue(pinNumber, out SafeLineHandle pinHandle))
            {
                Interop.ReleaseGpiodLine(pinHandle);
                _pinNumberToSafeLineHandle.Remove(pinNumber);
            }
        }

        protected internal override int ConvertPinNumberToLogicalNumberingScheme(int pinNumber) => 
            throw ExceptionHelper.GetPlatformNotSupportedException(ExceptionResource.ConvertPinNumberingSchemaError);

        protected internal override PinMode GetPinMode(int pinNumber)
        {
            if (!_pinNumberToSafeLineHandle.TryGetValue(pinNumber, out SafeLineHandle pinHandle))
            {
                throw ExceptionHelper.GetInvalidOperationException(ExceptionResource.PinNotOpenedError, pin: pinNumber);
            }
            return (Interop.GetLineDirection(pinHandle) == 1) ? PinMode.Input : PinMode.Output;
        }

        protected internal override bool IsPinModeSupported(int pinNumber, PinMode mode)
        {
            // Libgpiod Api do not support pull up or pull down resistors for now.
            return mode != PinMode.InputPullDown && mode != PinMode.InputPullUp;
        }

        protected internal override void OpenPin(int pinNumber)
        {
            SafeLineHandle pinHandle = Interop.GetChipLineByOffset(_chip, pinNumber);
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
                int result = Interop.GetGpiodLineValue(pinHandle);
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
                    if (_pinNumberToEventHandler.TryRemove(pinNumber, out eventHandler))
                    {
                        eventHandler.CancellationTokenSource.Cancel();
                        eventHandler.Task.Wait();
                        Interop.ReleaseGpiodLine(eventHandler.PinHandle);
                        eventHandler.Dispose();
                    }
                }
            }
            else
            {
                throw ExceptionHelper.GetInvalidOperationException(ExceptionResource.NotListeningForEventError);
            }
        }

        protected internal override void SetPinMode(int pinNumber, PinMode mode)
        {
            int requestResult  = -1;
            if (_pinNumberToSafeLineHandle.TryGetValue(pinNumber, out SafeLineHandle pinHandle))
            {
                string consumer = pinNumber.ToString();
                if (mode == PinMode.Input)
                {
                    requestResult = Interop.RequestLineInput(pinHandle, consumer);
                }
                else
                {
                    requestResult = Interop.RequestLineOutput(pinHandle, consumer);
                }
            }

            if (requestResult == -1)
            {
                throw ExceptionHelper.GetIOException(ExceptionResource.SetPinModeError, Marshal.GetLastWin32Error(), pinNumber);
            } 
        }

        protected internal override WaitForEventResult WaitForEvent(int pinNumber, PinEventTypes eventType, CancellationToken cancellationToken)
        {
            if (!_pinNumberToEventHandler.TryGetValue(pinNumber, out LibGpiodDriverEventHandler eventHandler))
            {
                eventHandler = new LibGpiodDriverEventHandler(pinNumber, new CancellationTokenSource());
                if (_pinNumberToEventHandler.TryAdd(pinNumber, eventHandler))
                {
                    eventHandler.PinHandle = SubscribeForEvent(pinNumber, eventType);
                    eventHandler.Task = InitializeEventDetectionTask(eventHandler, eventHandler.CancellationTokenSource.Token);
                }
            }

            CancellationTokenSource cancelWhenEventOccured = new CancellationTokenSource();
            PinChangeEventHandler callback = (o, e) => {
                cancelWhenEventOccured.Cancel();
            };
            if (eventType.HasFlag(PinEventTypes.Rising))
            {
                eventHandler.ValueRising += callback;
            }
            else if (eventType.HasFlag(PinEventTypes.Falling))
            {
                eventHandler.ValueFalling += callback;
            }

            WaitForEventResult(cancellationToken, eventHandler.CancellationTokenSource.Token, cancelWhenEventOccured.Token, eventType);
            RemoveCallbackForPinValueChangedEvent(pinNumber, callback);

            return new WaitForEventResult
            {
                TimedOut = !cancelWhenEventOccured.IsCancellationRequested,
                EventType = eventType
            };
        }

        private void WaitForEventResult(CancellationToken sourceToken, CancellationToken parentToken, CancellationToken eventOccuredToken,  PinEventTypes eventType)
        {
            while (!(sourceToken.IsCancellationRequested || parentToken.IsCancellationRequested || eventOccuredToken.IsCancellationRequested))
            {
                Thread.Sleep(1_000);
            }
        }

        protected internal override void Write(int pinNumber, PinValue value)
        {
            if (!_pinNumberToSafeLineHandle.TryGetValue(pinNumber, out SafeLineHandle pinHandle))
            {
                throw ExceptionHelper.GetInvalidOperationException(ExceptionResource.PinNotOpenedError, pin: pinNumber);
            }

            Interop.SetGpiodLineValue(pinHandle, (value == PinValue.High) ? 1 : 0);
        }

        protected override void Dispose(bool disposing)
        {
            foreach (int pin in _pinNumberToSafeLineHandle.Keys)
            {
                if (_pinNumberToSafeLineHandle.TryGetValue(pin, out SafeLineHandle pinHandle))
                {
                    Interop.ReleaseGpiodLine(pinHandle);
                    _pinNumberToSafeLineHandle.Remove(pin);
                }
            }

            foreach (int pin in _pinNumberToEventHandler.Keys)
            {
                if (_pinNumberToEventHandler.TryRemove(pin, out LibGpiodDriverEventHandler pinEventHandler)) {
                    pinEventHandler.CancellationTokenSource.Cancel();
                    pinEventHandler.Task.Wait();
                    pinEventHandler.Dispose();
                }
            }

            if (_chip != null)
            {
                _chip.Dispose();
                _chip = null;
            }

            base.Dispose(disposing);
        }
    }
}
