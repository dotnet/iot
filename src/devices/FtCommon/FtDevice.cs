// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device;
using System.Device.Gpio;
using System.Device.I2c;
using System.Device.Pwm;
using System.Device.Spi;
using System.Globalization;
using System.Security.Cryptography;
using Iot.Device.Board;

namespace Iot.Device.FtCommon
{
    /// <summary>
    /// FT4222 device information
    /// </summary>
    public class FtDevice : Board.Board
    {
        /// <summary>
        /// Instantiates a DeviceInformation object.
        /// </summary>
        /// <param name="flags">Indicates device state.</param>
        /// <param name="type">Indicates the device type.</param>
        /// <param name="id">The Vendor ID and Product ID of the device.</param>
        /// <param name="locId">The physical location identifier of the device.</param>
        /// <param name="serialNumber">The device serial number.</param>
        /// <param name="description">The device description.</param>
        public FtDevice(FtFlag flags, FtDeviceType type, uint id, uint locId, string serialNumber, string description)
        {
            Flags = flags;
            Type = type;
            Id = id;
            LocId = locId;
            SerialNumber = serialNumber;
            Description = description;
        }

        /// <summary>
        /// Indicates device state.  Can be any combination of the following: FT_FLAGS_OPENED, FT_FLAGS_HISPEED
        /// </summary>
        public FtFlag Flags { get; set; }

        /// <summary>
        /// Indicates the device type.  Can be one of the following: FT_DEVICE_232R, FT_DEVICE_2232C, FT_DEVICE_BM, FT_DEVICE_AM, FT_DEVICE_100AX or FT_DEVICE_UNKNOWN
        /// </summary>
        public FtDeviceType Type { get; set; }

        /// <summary>
        /// The Vendor ID and Product ID of the device.
        /// </summary>
        public uint Id { get; set; }

        /// <summary>
        /// The physical location identifier of the device.
        /// </summary>
        public uint LocId { get; set; }

        /// <summary>
        /// The device serial number.
        /// </summary>
        public string SerialNumber { get; set; }

        /// <summary>
        /// The device description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Creates I2C bus related to this device
        /// </summary>
        /// <returns>I2cBus instance</returns>
        /// <remarks>You can create either an I2C, either an SPI device.</remarks>
        protected override I2cBusManager CreateI2cBusCore(int busNumber, int[]? pins)
        {
            throw new NotSupportedException("No I2C bus present on this device");
        }

        /// <inheritdoc />
        public override int GetDefaultI2cBusNumber()
        {
            return 0;
        }

        /// <inheritdoc />
        protected override PwmChannel CreateSimplePwmChannel(int chip, int channel, int frequency, double dutyCyclePercentage)
        {
            throw new NotSupportedException("No PWM capable pins present");
        }

        /// <inheritdoc />
        public override int[] GetDefaultPinAssignmentForI2c(int busId)
        {
            throw new NotSupportedException("No I2C bus devices present");
        }

        /// <inheritdoc />
        public override int GetDefaultPinAssignmentForPwm(int chip, int channel)
        {
            throw new NotSupportedException("No PWM devices present");
        }

        /// <inheritdoc />
        public override int[] GetDefaultPinAssignmentForSpi(SpiConnectionSettings connectionSettings)
        {
            throw new NotSupportedException("No SPI bus devices present");
        }

        /// <summary>
        /// Creates SPI device related to this device
        /// </summary>
        /// <param name="settings">The SPI settings</param>
        /// <param name="pins">The pins for this SPI instance</param>
        /// <returns>a SPI device</returns>
        /// <remarks>You can create either an I2C, either an SPI device.
        /// You can create multiple SPI devices, the first one will be the one used for the clock frequency.
        /// They all have to have different Chip Select. You can use any of the 3 to 15 pin for this function.</remarks>
        protected override SpiDevice CreateSimpleSpiDevice(SpiConnectionSettings settings, int[] pins) => throw new NotSupportedException("No SPI bus present on this device");

        /// <inheritdoc />
        public override ComponentInformation QueryComponentInformation()
        {
            var ret = base.QueryComponentInformation();
            ret.Properties["Description"] = Description;
            ret.Properties["SerialNumber"] = SerialNumber;
            ret.Properties["VendorId"] = Id.ToString(CultureInfo.InvariantCulture);
            ret.Properties["LocId"] = LocId.ToString(CultureInfo.InvariantCulture);
            return ret;
        }
    }
}
