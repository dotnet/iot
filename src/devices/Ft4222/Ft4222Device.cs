// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Device.I2c;
using System.Device.Spi;
using Iot.Device.Board;
using Iot.Device.FtCommon;

namespace Iot.Device.Ft4222
{
    /// <summary>
    /// FT4222 device information
    /// </summary>
    public class Ft4222Device : FtDevice
    {
        /// <summary>
        /// Gets all the FT4222 connected
        /// </summary>
        /// <returns>A list of FT4222</returns>
        public static List<Ft4222Device> GetFt4222()
        {
            List<Ft4222Device> ft4222s = new List<Ft4222Device>();
            var devices = FtCommon.FtCommon.GetDevices(
                new FtDeviceType[] { FtDeviceType.Ft4222HMode0or2With2Interfaces, FtDeviceType.Ft4222HMode1or2With4Interfaces, FtDeviceType.Ft4222HMode3With1Interface, FtDeviceType.Ft4222OtpProgrammerBoard });
            foreach (var device in devices)
            {
                ft4222s.Add(new Ft4222Device(device));
            }

            return ft4222s;
        }

        /// <summary>
        /// Instantiates a FT4222 Device object.
        /// </summary>
        /// <param name="flags">Indicates device state.</param>
        /// <param name="type">Indicates the device type.</param>
        /// <param name="id">The Vendor ID and Product ID of the device.</param>
        /// <param name="locId">The physical location identifier of the device.</param>
        /// <param name="serialNumber">The device serial number.</param>
        /// <param name="description">The device description.</param>
        public Ft4222Device(FtFlag flags, FtDeviceType type, uint id, uint locId, string serialNumber, string description)
        : base(flags, type, id, locId, serialNumber, description)
        {
        }

        /// <summary>
        /// Instantiates a FT4222 Device object.
        /// </summary>
        /// <param name="ftdevice">a FT Device</param>
        public Ft4222Device(FtDevice ftdevice)
            : base(ftdevice.Flags, ftdevice.Type, ftdevice.Id, ftdevice.LocId, ftdevice.SerialNumber, ftdevice.Description)
        {
        }

        /// <summary>
        /// Creates an I2C Bus
        /// </summary>
        /// <param name="busNumber">The I2C bus number to create. Must be 0</param>
        /// <param name="pins">The pins for I2C (0 and 1)</param>
        /// <returns>An I2C bus</returns>
        protected override I2cBusManager CreateI2cBusCore(int busNumber, int[]? pins)
        {
            if (busNumber != 0)
            {
                throw new ArgumentOutOfRangeException(nameof(busNumber));
            }

            return new I2cBusManager(this, busNumber, pins, new Ft4222I2cBus(this));
        }

        /// <inheritdoc />
        public override int GetDefaultI2cBusNumber()
        {
            return 0;
        }

        /// <inheritdoc />
        public override int[] GetDefaultPinAssignmentForI2c(int busId)
        {
            return new[] { 0, 1 };
        }

        /// <inheritdoc />
        public override int[] GetDefaultPinAssignmentForSpi(SpiConnectionSettings connectionSettings)
        {
            return new[] { 0, 1, 2, 3 };
        }

        /// <summary>
         /// Creates SPI device related to this device
         /// </summary>
         /// <param name="settings">The SPI settings</param>
         /// <param name="pins">The pins used for SPI (0-3)</param>
         /// <returns>a SPI device</returns>
         /// <remarks>You can create either an I2C or an SPI device.
         /// You can create multiple SPI devices, the first one will be the one used for the clock frequency.
         /// They all have to have different Chip Select. You can use any of the 3 to 15 pin for this function.</remarks>
        protected override SpiDevice CreateSimpleSpiDevice(SpiConnectionSettings settings, int[] pins)
        {
            return new Ft4222Spi(settings);
        }

        /// <summary>
        /// Creates the <see cref="Ft4222Gpio"/> controller
        /// </summary>
        /// <returns>A new GPIO driver</returns>
        protected override GpioDriver? TryCreateBestGpioDriver()
        {
            return new Ft4222Gpio(this);
        }
    }
}
