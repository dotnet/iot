// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Device;
using System.Device.I2c;
using System.IO;
using System.Net.Http.Headers;
using Iot.Device.FtCommon;

namespace Iot.Device.Ft232H
{
    /// <summary>
    /// I2C Bus for FT232H
    /// </summary>
    internal class Ft232HI2cBus : I2cBus
    {
        private readonly Dictionary<int, I2cDevice> _usedAddresses;

        /// <summary>
        /// Store the FTDI Device Information
        /// </summary>
        public Ftx232HDevice DeviceInformation { get; private set; }

        /// <summary>
        /// Creates anI2C Bus
        /// </summary>
        /// <param name="deviceInformation">a FT232H device</param>
        public Ft232HI2cBus(Ftx232HDevice deviceInformation)
        {
            DeviceInformation = deviceInformation;
            DeviceInformation.I2cInitialize();
            _usedAddresses = new Dictionary<int, I2cDevice>();
        }

        /// <inheritdoc/>
        public override I2cDevice CreateDevice(int deviceAddress)
        {
            if (_usedAddresses.ContainsKey(deviceAddress))
            {
                throw new ArgumentException($"Device with address 0x{deviceAddress,0X2} is already open.", nameof(deviceAddress));
            }

            I2cDevice ret = CreateDeviceNoCheck(deviceAddress);
            _usedAddresses[deviceAddress] = ret;
            return ret;
        }

        internal I2cDevice CreateDeviceNoCheck(int deviceAddress)
        {
            return new Ft232HI2cDevice(this, deviceAddress);
        }

        /// <inheritdoc/>
        public override void RemoveDevice(int deviceAddress)
        {
            if (!RemoveDeviceNoCheck(deviceAddress))
            {
                throw new ArgumentException($"Device with address 0x{deviceAddress,0X2} was not open.", nameof(deviceAddress));
            }
        }

        internal bool RemoveDeviceNoCheck(int deviceAddress)
        {
            return _usedAddresses.Remove(deviceAddress);
        }

        internal void Read(int deviceAddress, Span<byte> buffer)
        {
            DeviceInformation.I2cStart();
            var ack = DeviceInformation.I2cSendDeviceAddrAndCheckACK((byte)deviceAddress, true);
            if (!ack)
            {
                DeviceInformation.I2cStop();
                throw new IOException($"Error reading device while setting up address");
            }

            for (int i = 0; i < buffer.Length - 1; i++)
            {
                buffer[i] = DeviceInformation.I2CReadByte(true);
            }

            if (buffer.Length > 0)
            {
                buffer[buffer.Length - 1] = DeviceInformation.I2CReadByte(false);
            }

            DeviceInformation.I2cStop();
        }

        internal void Write(int deviceAddress, ReadOnlySpan<byte> buffer)
        {
            DeviceInformation.I2cStart();
            var ack = DeviceInformation.I2cSendDeviceAddrAndCheckACK((byte)deviceAddress, false);
            if (!ack)
            {
                DeviceInformation.I2cStop();
                throw new IOException($"Error writing device while setting up address");
            }

            for (int i = 0; i < buffer.Length; i++)
            {
                ack = DeviceInformation.I2cSendByteAndCheckACK(buffer[i]);
                if (!ack)
                {
                    DeviceInformation.I2cStop();
                    throw new IOException($"Error writing device on byte {i}");
                }
            }

            DeviceInformation.I2cStop();
        }

        public override ComponentInformation QueryComponentInformation()
        {
            var self = new ComponentInformation(this, "Ftx232HI2c I2C Bus driver");
            self.Properties["BusNo"] = "0";
            foreach (var device in _usedAddresses)
            {
                self.AddSubComponent(device.Value.QueryComponentInformation());
            }

            return self;
        }
    }
}
