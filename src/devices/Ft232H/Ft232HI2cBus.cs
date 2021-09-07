// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Device.I2c;
using System.IO;

namespace Iot.Device.Ft232H
{
    /// <summary>
    /// I2C Bus for FT232H
    /// </summary>
    internal class Ft232HI2cBus : I2cBus
    {
        private HashSet<int> _usedAddresses = new HashSet<int>();

        /// <summary>
        /// Store the FTDI Device Information
        /// </summary>
        public Ft232HDevice DeviceInformation { get; private set; }

        /// <summary>
        /// Creates anI2C Bus
        /// </summary>
        /// <param name="deviceInformation">a FT232H device</param>
        public Ft232HI2cBus(Ft232HDevice deviceInformation)
        {
            DeviceInformation = deviceInformation;
            DeviceInformation.I2cInitialize();
        }

        /// <inheritdoc/>
        public override I2cDevice CreateDevice(int deviceAddress)
        {
            if (!_usedAddresses.Add(deviceAddress))
            {
                throw new ArgumentException($"Device with address 0x{deviceAddress,0X2} is already open.", nameof(deviceAddress));
            }

            return new Ft232HI2c(this, deviceAddress);
        }

        /// <inheritdoc/>
        public override void RemoveDevice(int deviceAddress)
        {
            if (!_usedAddresses.Remove(deviceAddress))
            {
                throw new ArgumentException($"Device with address 0x{deviceAddress,0X2} was not open.", nameof(deviceAddress));
            }
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
    }
}
