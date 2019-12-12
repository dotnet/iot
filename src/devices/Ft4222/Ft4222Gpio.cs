// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.Gpio;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

namespace Iot.Device.Ft4222
{
    /// <summary>
    /// GPIO driver for the FT4222
    /// </summary>
    public class Ft4222Gpio : GpioDriver
    {
        private const int PinCountConst = 4;

        private SafeFtHandle _ftHandle;
        private GpioPinMode[] _gpioDirections = new GpioPinMode[PinCountConst];
        private GpioTrigger[] _gpioTriggers = new GpioTrigger[PinCountConst];
        private PinChangeEventHandler[] _pinRisingHandlers = new PinChangeEventHandler[PinCountConst];
        private PinChangeEventHandler[] _pinFallingHandlers = new PinChangeEventHandler[PinCountConst];

        /// <inheritdoc/>
        protected override int PinCount => PinCountConst;

        /// <summary>
        /// Store the FTDI Device Information
        /// </summary>
        public DeviceInformation DeviceInformation { get; internal set; }

        /// <summary>
        /// Create a FT4222 GPIO driver
        /// </summary>
        /// <param name="deviceNumber"></param>
        public Ft4222Gpio(int deviceNumber = 0)
        {
            // Check device
            var devInfos = FtCommon.GetDevices();
            if (devInfos.Count == 0)
                throw new IOException("No FTDI device available");

            // Select the deviceNumber, only the last one in Mode 0 and Mode 1 can be open.
            // The last one is either B if in Mode 0 or D in mode 1.
            string strMode = devInfos[0].Type == FtDevice.Ft4222HMode1or2With4Interfaces ? "FT4222 D" : "FT4222 B";

            var devInfo = devInfos.Where(m => m.Description == strMode).ToArray();
            if ((devInfo.Length == 0) || (devInfo.Length < deviceNumber))
                throw new IOException($"Can't find a device to open GPIO on index {deviceNumber}");

            DeviceInformation = devInfo[deviceNumber];
            // Open device
            var ftStatus = FtFunction.FT_OpenEx(DeviceInformation.LocId, FtOpenType.OpenByLocation, out _ftHandle);

            if (ftStatus != FtStatus.Ok)
                throw new IOException($"Failed to open device {DeviceInformation.Description}, status: {ftStatus}");

            ftStatus = FtFunction.FT4222_SetSuspendOut(_ftHandle, false);
            if (ftStatus != FtStatus.Ok)
                throw new IOException($"Can't initialize GPIO for device {DeviceInformation.Description} changing Suspend out mode, status: {ftStatus}");

            ftStatus = FtFunction.FT4222_SetWakeUpInterrupt(_ftHandle, false);
            if (ftStatus != FtStatus.Ok)
                throw new IOException($"Can't initialize GPIO for device {DeviceInformation.Description} removing wake up interrupt, status: {ftStatus}");

            for (int i = 0; i < PinCountConst; i++)
                _gpioDirections[i] = GpioPinMode.Output;

            ftStatus = FtFunction.FT4222_GPIO_Init(_ftHandle, _gpioDirections);

            if (ftStatus != FtStatus.Ok)
                throw new IOException($"Can't initialize GPIO for device {DeviceInformation.Description}, status: {ftStatus}");
        }

        /// <inheritdoc/>
        protected override void ClosePin(int pinNumber)
        { }

        /// <inheritdoc/>
        protected override int ConvertPinNumberToLogicalNumberingScheme(int pinNumber) => pinNumber;

        /// <inheritdoc/>
        protected override PinMode GetPinMode(int pinNumber) => _gpioDirections[pinNumber] == GpioPinMode.Input ? PinMode.Input : PinMode.Output;

        /// <inheritdoc/>
        protected override bool IsPinModeSupported(int pinNumber, PinMode mode) => (mode == PinMode.Input) || (mode == PinMode.Output);

        /// <inheritdoc/>
        protected override void OpenPin(int pinNumber)
        { }

        /// <inheritdoc/>
        protected override PinValue Read(int pinNumber)
        {
            GpioPinValue pinVal;
            var status = FtFunction.FT4222_GPIO_Read(_ftHandle, (GpioPort)pinNumber, out pinVal);
            if (status != FtStatus.Ok)
                throw new IOException($"{nameof(Read)}: failed to write GPIO, status: {status}");
            return pinVal == GpioPinValue.High ? PinValue.High : PinValue.Low;
        }

        /// <inheritdoc/>
        protected override void AddCallbackForPinValueChangedEvent(int pinNumber, PinEventTypes eventTypes, PinChangeEventHandler callback)
        {
            if (eventTypes == PinEventTypes.None)
                throw new ArgumentException($"{PinEventTypes.None} is an invalid value.", nameof(eventTypes));

            if (eventTypes.HasFlag(PinEventTypes.Falling))
            {
                _gpioTriggers[pinNumber] |= GpioTrigger.Falling;
                _pinFallingHandlers[pinNumber] += callback;
            }
            if (eventTypes.HasFlag(PinEventTypes.Rising))
            {
                _gpioTriggers[pinNumber] |= GpioTrigger.Rising;
                _pinRisingHandlers[pinNumber] += callback;
            }
            FtFunction.FT4222_GPIO_SetInputTrigger(_ftHandle, (GpioPort)pinNumber, _gpioTriggers[pinNumber]);
        }

        /// <inheritdoc/>
        protected override void RemoveCallbackForPinValueChangedEvent(int pinNumber, PinChangeEventHandler callback)
        {
            _pinFallingHandlers[pinNumber] -= callback;
            _pinRisingHandlers[pinNumber] -= callback;
            if (_pinFallingHandlers == null)
            {
                _gpioTriggers[pinNumber] &= ~GpioTrigger.Falling;
            }
            if (_pinRisingHandlers == null)
            {
                _gpioTriggers[pinNumber] &= ~GpioTrigger.Rising;
            }
            FtFunction.FT4222_GPIO_SetInputTrigger(_ftHandle, (GpioPort)pinNumber, _gpioTriggers[pinNumber]);
        }

        /// <inheritdoc/>
        protected override WaitForEventResult WaitForEvent(int pinNumber, PinEventTypes eventTypes, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                ushort queueSize;
                var ftStatus = FtFunction.FT4222_GPIO_GetTriggerStatus(_ftHandle, (GpioPort)pinNumber, out queueSize);
                if (ftStatus != FtStatus.Ok)
                    throw new IOException($"Can't get trigger status, error {ftStatus}");

                if (queueSize > 0)
                {
                    Span<GpioTrigger> gpioTriggers = stackalloc GpioTrigger[queueSize];
                    ushort readTrigger;
                    ftStatus = FtFunction.FT4222_GPIO_ReadTriggerQueue(_ftHandle, (GpioPort)pinNumber, in MemoryMarshal.GetReference(gpioTriggers), queueSize, out readTrigger);
                    if (ftStatus != FtStatus.Ok)
                        throw new IOException($"Can't read trigger status, error {ftStatus}");

                    switch (eventTypes)
                    {
                        case PinEventTypes.Rising:
                            if (_gpioTriggers[pinNumber].HasFlag(GpioTrigger.Rising))
                            {
                                if (gpioTriggers.ToArray().Where(m => m == GpioTrigger.Rising).Count() > 0)
                                {
                                    return new WaitForEventResult()
                                    {
                                        EventTypes = PinEventTypes.Rising,
                                        TimedOut = false
                                    };
                                }
                            }
                            break;
                        case PinEventTypes.Falling:
                            if (_gpioTriggers[pinNumber].HasFlag(GpioTrigger.Falling))
                            {
                                if (gpioTriggers.ToArray().Where(m => m == GpioTrigger.Falling).Count() > 0)
                                {
                                    return new WaitForEventResult()
                                    {
                                        EventTypes = PinEventTypes.Falling,
                                        TimedOut = false
                                    };
                                }
                            }
                            break;
                        case PinEventTypes.None:
                        default:
                            break;
                    }
                }
            }

            return new WaitForEventResult()
            {
                EventTypes = PinEventTypes.None,
                TimedOut = true
            };
        }

        /// <inheritdoc/>
        protected override void SetPinMode(int pinNumber, PinMode mode)
        {
            _gpioDirections[pinNumber] = mode == PinMode.Output ? GpioPinMode.Output : GpioPinMode.Input;
            var status = FtFunction.FT4222_GPIO_Init(_ftHandle, _gpioDirections);
            if (status != FtStatus.Ok)
                throw new IOException($"{nameof(SetPinMode)}: failed to set pin number {pinNumber} to {mode}, status: {status}");
        }

        /// <inheritdoc/>
        protected override void Write(int pinNumber, PinValue value)
        {
            var status = FtFunction.FT4222_GPIO_Write(_ftHandle, (GpioPort)pinNumber, value == PinValue.High ? GpioPinValue.High : GpioPinValue.Low);
            if (status != FtStatus.Ok)
                throw new IOException($"{nameof(Write)}: failed to write GPIO, status: {status}");
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            _ftHandle.Dispose();
            base.Dispose(disposing);
        }
    }
}
