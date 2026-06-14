// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Device;
using System.Device.I2c;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Iot.Device.FtCommon;

namespace Iot.Device.Ft4222
{
    /// <summary>
    /// FT4222 I2C Device
    /// </summary>
    public class Ft4222I2cBus : I2cBus
    {
        /// <summary>
        /// The default I2C master clock frequency in kbps used when none is specified.
        /// </summary>
        public const uint DefaultI2cMasterFrequencyKbps = 400;

        /// <summary>
        /// The minimum I2C master clock frequency in kbps supported by the FT4222.
        /// </summary>
        public const uint MinimumI2cMasterFrequencyKbps = 60;

        /// <summary>
        /// The maximum I2C master clock frequency in kbps supported by the FT4222.
        /// </summary>
        public const uint MaximumI2cMasterFrequencyKbps = 3400;

        private readonly Dictionary<int, I2cDevice> _usedAddresses = new Dictionary<int, I2cDevice>();
        private SafeFtHandle _ftHandle;

        /// <summary>
        /// Gets the I2C master clock frequency in kbps used by this bus.
        /// </summary>
        public uint I2cMasterFrequencyKbps { get; }

        /// <summary>
        /// Store the FTDI Device Information
        /// </summary>
        public Ft4222Device DeviceInformation { get; private set; }

        /// <summary>
        /// Create a FT4222 I2C device using the default I2C master clock frequency.
        /// </summary>
        /// <param name="deviceInformation">Device information. Use FtCommon.GetDevices to get it.</param>
        public Ft4222I2cBus(Ft4222Device deviceInformation)
            : this(deviceInformation, DefaultI2cMasterFrequencyKbps)
        {
        }

        /// <summary>
        /// Create a FT4222 I2C device using the specified I2C master clock frequency.
        /// </summary>
        /// <param name="deviceInformation">Device information. Use FtCommon.GetDevices to get it.</param>
        /// <param name="i2cMasterFrequencyKbps">The I2C master clock frequency in kbps. Supported range is 60 to 3400 kbps.</param>
        public Ft4222I2cBus(Ft4222Device deviceInformation, uint i2cMasterFrequencyKbps)
        {
            if (i2cMasterFrequencyKbps < MinimumI2cMasterFrequencyKbps || i2cMasterFrequencyKbps > MaximumI2cMasterFrequencyKbps)
            {
                throw new ArgumentOutOfRangeException(nameof(i2cMasterFrequencyKbps), i2cMasterFrequencyKbps, $"I2C master clock frequency must be between {MinimumI2cMasterFrequencyKbps} and {MaximumI2cMasterFrequencyKbps} KHz.");
            }

            I2cMasterFrequencyKbps = i2cMasterFrequencyKbps;

            switch (deviceInformation.Type)
            {
                case FtDeviceType.Ft4222HMode0or2With2Interfaces:
                case FtDeviceType.Ft4222HMode1or2With4Interfaces:
                case FtDeviceType.Ft4222HMode3With1Interface:
                    break;
                default: throw new ArgumentException($"Unknown device type: {deviceInformation.Type}");
            }

            DeviceInformation = deviceInformation;
            // Open device
            var ftStatus = FtFunction.FT_OpenEx(DeviceInformation.LocId, FtOpenType.OpenByLocation, out _ftHandle);

            if (ftStatus != FtStatus.Ok)
            {
                throw new IOException($"Failed to open device {DeviceInformation.Description}, status: {ftStatus}");
            }

            // Set the clock
            FtClockRate ft4222Clock = FtClockRate.Clock24MHz;

            ftStatus = FtFunction.FT4222_SetClock(_ftHandle, ft4222Clock);
            if (ftStatus != FtStatus.Ok)
            {
                throw new IOException($"Failed set clock rate {ft4222Clock} on device: {DeviceInformation.Description}, status: {ftStatus}");
            }

            // Set the device as I2C Master
            ftStatus = FtFunction.FT4222_I2CMaster_Init(_ftHandle, I2cMasterFrequencyKbps);
            if (ftStatus != FtStatus.Ok)
            {
                throw new IOException($"Failed to initialize I2C Master mode on device: {DeviceInformation.Description}, status: {ftStatus}");
            }
        }

        internal void Read(int deviceAddress, Span<byte> buffer)
        {
            if (deviceAddress < 0 || deviceAddress > ushort.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(deviceAddress));
            }

            if (buffer.Length > ushort.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(buffer), $"Buffer is too large. Maximum length is {ushort.MaxValue}.");
            }

            ushort bytesRead;
            var ftStatus = FtFunction.FT4222_I2CMaster_Read(_ftHandle, (ushort)deviceAddress, in MemoryMarshal.GetReference(buffer), (ushort)buffer.Length, out bytesRead);
            if (ftStatus != FtStatus.Ok)
            {
                throw new IOException($"{nameof(Read)} failed to read, error: {ftStatus}");
            }

            if (bytesRead != buffer.Length)
            {
                throw new IOException($"Number of bytes read ({bytesRead}) doesn't match length of the buffer ({buffer.Length}).");
            }
        }

        internal void Write(int deviceAddress, ReadOnlySpan<byte> buffer)
        {
            if (deviceAddress < 0 || deviceAddress > ushort.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(deviceAddress));
            }

            if (buffer.Length > ushort.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(buffer), $"Buffer is too large. Maximum length is {ushort.MaxValue}.");
            }

            ushort bytesSent;
            var ftStatus = FtFunction.FT4222_I2CMaster_Write(_ftHandle, (ushort)deviceAddress, in MemoryMarshal.GetReference(buffer), (ushort)buffer.Length, out bytesSent);
            if (ftStatus != FtStatus.Ok)
            {
                throw new IOException($"{nameof(Write)} failed to write, error: {ftStatus}");
            }
        }

        /// <inheritdoc/>
        public override I2cDevice CreateDevice(int deviceAddress)
        {
            if (_usedAddresses.ContainsKey(deviceAddress))
            {
                throw new ArgumentException($"Device with address 0x{deviceAddress,0X2} is already open.", nameof(deviceAddress));
            }

            var ret = new Ft4222I2c(this, deviceAddress);
            _usedAddresses.Add(deviceAddress, ret);
            return ret;
        }

        /// <inheritdoc/>
        public override void RemoveDevice(int deviceAddress)
        {
            if (!_usedAddresses.Remove(deviceAddress))
            {
                throw new ArgumentException($"Device with address 0x{deviceAddress,0X2} was not open.", nameof(deviceAddress));
            }
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            _ftHandle?.Dispose();
            _ftHandle = null!;
        }

        /// <inheritdoc />
        public override ComponentInformation QueryComponentInformation()
        {
            var self = new ComponentInformation(this, "FT4222 I2C Bus driver");
            self.Properties["BusNo"] = "0";
            self.Properties["Description"] = DeviceInformation.Description;
            self.Properties["SerialNumber"] = DeviceInformation.SerialNumber;
            foreach (var device in _usedAddresses)
            {
                self.AddSubComponent(device.Value.QueryComponentInformation());
            }

            return self;
        }
    }
}
