// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Device.Ft4222;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

namespace System.Device.Gpio
{
    /// <summary>
    /// GPIO driver for the FT4222
    /// </summary>
    public class Ft4222Gpio : GpioDriver
    {
        private const int PinCountConst = 4;

        private SafeFtHandle _ftHandle = new SafeFtHandle();
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        private GpioPinMode[] _gpioDirections = new GpioPinMode[PinCountConst];

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
            string strMode = devInfos[0].Type == FtDevice.Ft4222HMode1or2With4Interfaces ? "D" : "B";
            
            var devInfo = devInfos.Where(m => m.SerialNumber == strMode).ToArray();
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
        {
            if ((pinNumber < 0) || (pinNumber >= PinCountConst))
                throw new ArgumentException($"Pin number must be between 0 and {PinCountConst - 1}");
        }

        /// <inheritdoc/>
        protected override int ConvertPinNumberToLogicalNumberingScheme(int pinNumber)
        {
            return pinNumber;
        }

        /// <inheritdoc/>
        protected override PinMode GetPinMode(int pinNumber)
        {
            if ((pinNumber < 0) || (pinNumber >= PinCountConst))
                throw new ArgumentException($"Pin number must be between 0 and {PinCountConst - 1}");

            return _gpioDirections[pinNumber] == GpioPinMode.Input ? PinMode.Input : PinMode.Output;
        }

        /// <inheritdoc/>
        protected override bool IsPinModeSupported(int pinNumber, PinMode mode)
        {
            if ((pinNumber < 0) || (pinNumber >= PinCountConst))
                throw new ArgumentException($"Pin number must be between 0 and {PinCountConst - 1}");

            if ((mode == PinMode.Input) || (mode == PinMode.Output))
                return true;

            return false;
        }

        /// <inheritdoc/>
        protected override void OpenPin(int pinNumber)
        {
            if ((pinNumber < 0) || (pinNumber >= PinCountConst))
                throw new ArgumentException($"Pin number must be between 0 and {PinCountConst - 1}");
        }

        /// <inheritdoc/>
        protected override PinValue Read(int pinNumber)
        {
            if ((pinNumber < 0) || (pinNumber >= PinCountConst))
                throw new ArgumentException($"Pin number must be between 0 and {PinCountConst - 1}");

            GpioPinValue pinVal;
            var status = FtFunction.FT4222_GPIO_Read(_ftHandle, (GpioPort)pinNumber, out pinVal);
            if (status != FtStatus.Ok)
                throw new IOException($"{nameof(Read)}: failed to write GPIO, status: {status}");
            return pinVal == GpioPinValue.High ? PinValue.High : PinValue.Low;
        }

        /// <inheritdoc/>
        protected override void AddCallbackForPinValueChangedEvent(int pinNumber, PinEventTypes eventTypes, PinChangeEventHandler callback)
        {
            // TODO: implement callback
            // This can be done using the function FT4222_GPIO_SetInputTrigger to set them up
            // Then calling in an infinite loop FT4222_GPIO_GetTriggerStatus
            // and finally FT4222_GPIO_ReadTriggerQueue to understand what has trigged
            throw new PlatformNotSupportedException();
        }

        /// <inheritdoc/>
        protected override void RemoveCallbackForPinValueChangedEvent(int pinNumber, PinChangeEventHandler callback)
        {
            // TODO: to implement. See the add callback to understand the mechanism
            throw new PlatformNotSupportedException();
        }

        /// <inheritdoc/>
        protected override WaitForEventResult WaitForEvent(int pinNumber, PinEventTypes eventTypes, CancellationToken cancellationToken)
        {
            // TODO: see the add callback function comments
            throw new PlatformNotSupportedException();
        }

        /// <inheritdoc/>
        protected override void SetPinMode(int pinNumber, PinMode mode)
        {
            if ((pinNumber < 0) || (pinNumber >= PinCountConst))
                throw new ArgumentException($"Pin number must be between 0 and {PinCountConst - 1}");

            _gpioDirections[pinNumber] = mode == PinMode.Output ? GpioPinMode.Output : GpioPinMode.Input;
            var status = FtFunction.FT4222_GPIO_Init(_ftHandle, _gpioDirections);
            if (status != FtStatus.Ok)
                throw new IOException($"{nameof(SetPinMode)}: failed to set pin number {pinNumber} to {mode}, status: {status}");
        }

        /// <inheritdoc/>
        protected override void Write(int pinNumber, PinValue value)
        {
            if ((pinNumber < 0) || (pinNumber >= PinCountConst))
                throw new ArgumentException($"Pin number must be between 0 and {PinCountConst - 1}");

            var status = FtFunction.FT4222_GPIO_Write(_ftHandle, (GpioPort)pinNumber, value == PinValue.High ? GpioPinValue.High : GpioPinValue.Low);
            if (status != FtStatus.Ok)
                throw new IOException($"{nameof(Write)}: failed to write GPIO, status: {status}");
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (!_ftHandle.IsClosed)
                _ftHandle.Close();

            base.Dispose(disposing);
        }
    }
}
