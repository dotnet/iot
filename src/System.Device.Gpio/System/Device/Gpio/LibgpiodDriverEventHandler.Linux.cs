// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace System.Device.Gpio.Drivers
{
    internal sealed class LibGpiodDriverEventHandler : IDisposable
    {
        public event PinChangeEventHandler ValueRising;

        public event PinChangeEventHandler ValueFalling;

        private int _pinNumber;

        public CancellationTokenSource CancellationTokenSource;

        private Task _task;

        private bool _disposing = false;

        public LibGpiodDriverEventHandler(int pinNumber, SafeLineHandle safeLineHandle) {
            _pinNumber = pinNumber;
            CancellationTokenSource = new CancellationTokenSource();
            SubscribeForEvent(safeLineHandle);
            _task = InitializeEventDetectionTask(CancellationTokenSource.Token, safeLineHandle);
        }

        private void SubscribeForEvent(SafeLineHandle pinHandle)
        {
            int eventSuccess = Interop.libgpiod.gpiod_line_request_both_edges_events(pinHandle, $"Listen {_pinNumber} for both edge event");

            if (eventSuccess < 0)
            {
                throw ExceptionHelper.GetIOException(ExceptionResource.RequestEventError, _pinNumber, Marshal.GetLastWin32Error());
            }
        }


        private Task InitializeEventDetectionTask(CancellationToken token, SafeLineHandle pinHandle)
        {
            return Task.Run(() =>
            {
                while (!(token.IsCancellationRequested || _disposing))
                {
                    // WaitEventResult can be TimedOut, EventOccured or Error, in case of TimedOut will continue waiting
                    TimeSpec timeout = new TimeSpec
                    {
                        TvSec = new IntPtr(0),
                        TvNsec = new IntPtr(1000000)
                    };

                    WaitEventResult waitResult = Interop.libgpiod.gpiod_line_event_wait(pinHandle, ref timeout);
                    if (waitResult == WaitEventResult.Error)
                    {
                        throw ExceptionHelper.GetIOException(ExceptionResource.EventWaitError, Marshal.GetLastWin32Error(), _pinNumber);
                    }

                    if (waitResult == WaitEventResult.EventOccured)
                    {
                        GpioLineEvent eventResult = new GpioLineEvent();
                        int checkForEvent = Interop.libgpiod.gpiod_line_event_read(pinHandle, ref eventResult);
                        if (checkForEvent == -1)
                        {
                            throw ExceptionHelper.GetIOException(ExceptionResource.EventReadError, Marshal.GetLastWin32Error());
                        }

                        PinEventTypes eventType = (eventResult.event_type == 1) ? PinEventTypes.Rising : PinEventTypes.Falling;
                        this?.OnPinValueChanged(new PinValueChangedEventArgs(eventType, _pinNumber), eventType);
                    }
                }
            }, token);
        }

        public void OnPinValueChanged(PinValueChangedEventArgs args, PinEventTypes detectionOfEventTypes)
        {
            if (detectionOfEventTypes == PinEventTypes.Rising && args.ChangeType == PinEventTypes.Rising)
                ValueRising?.Invoke(this, args);
            if (detectionOfEventTypes == PinEventTypes.Falling && args.ChangeType == PinEventTypes.Falling)
                ValueFalling?.Invoke(this, args);
        }

        public bool IsCallbackListEmpty()
        {
            return ValueRising == null && ValueFalling == null;
        }

        public void Dispose()
        {
            _disposing = true;
            CancellationTokenSource.Cancel();
            _task?.Wait();
            ValueRising = null;
            ValueFalling = null;
        }
    }
}
