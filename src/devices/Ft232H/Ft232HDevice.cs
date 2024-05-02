// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Device;
using System.Device.Gpio;
using System.Device.I2c;
using System.Device.Spi;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using Iot.Device.Board;
using Iot.Device.Common;
using Iot.Device.FtCommon;

namespace Iot.Device.Ft232H
{
    /// <summary>
    /// FT232H Device
    /// </summary>
    public class Ft232HDevice : Ftx232HDevice, IDisposable
    {
        /// <summary>
        /// Gets all the FT232H connected
        /// </summary>
        /// <returns>A list of FT232H</returns>
        public static List<Ft232HDevice> GetFt232H()
        {
            List<Ft232HDevice> ft232s = new List<Ft232HDevice>();
            var devices = FtCommon.FtCommon.GetDevices(new FtDeviceType[] { FtDeviceType.Ft232H, FtDeviceType.Ft232HP });
            foreach (var device in devices)
            {
                ft232s.Add(new Ft232HDevice(device));
            }

            return ft232s;
        }

        /// <summary>
        /// Gets the pin number from a string
        /// </summary>
        /// <param name="pin">A string</param>
        /// <returns>The pin number, -1 in case it's not found.</returns>
        /// <remarks>Valid pins are ADBUS0 to 7, D0 to 7, ACBUS0 to 7, C0 to 7,
        /// TCK, SK, CLK, MOSI, MISO, SDA, TDI, DI, TDO, DO,
        /// TMS, CS, GPIOL0 to 3, GPIOH0 to 7</remarks>
        public static int GetPinNumberFromString(string pin)
        {
            pin = pin.ToUpper();
            switch (pin)
            {
                case "ADBUS0":
                case "D0":
                case "TCK":
                case "SK":
                case "CLK":
                case "SDL":
                    return 0;
                case "ADBUS1":
                case "D1":
                case "DO":
                case "TDI":
                case "SDA":
                case "MOSI":
                    return 1;
                case "ADBUS2":
                case "D2":
                case "DI":
                case "TDO":
                case "MISO":
                    return 2;
                case "ADBUS3":
                case "D3":
                case "TMS":
                case "CS":
                    return 3;
                case "ADBUS4":
                case "D4":
                case "GPIOL0":
                    return 4;
                case "ADBUS5":
                case "D5":
                case "GPIOL1":
                    return 5;
                case "ADBUS6":
                case "D6":
                case "GPIOL2":
                    return 6;
                case "ADBUS7":
                case "D7":
                case "GPIOL3":
                    return 7;
                case "ACBUS0":
                case "C0":
                case "GPIOH0":
                    return 8;
                case "ACBUS1":
                case "C1":
                case "GPIOH1":
                    return 9;
                case "ACBUS2":
                case "C2":
                case "GPIOH2":
                    return 10;
                case "ACBUS3":
                case "C3":
                case "GPIOH3":
                    return 11;
                case "ACBUS4":
                case "C4":
                case "GPIOH4":
                    return 12;
                case "ACBUS5":
                case "C5":
                case "GPIOH5":
                    return 13;
                case "ACBUS6":
                case "C6":
                case "GPIOH6":
                    return 14;
                case "ACBUS7":
                case "C7":
                case "GPIOH7":
                    return 15;
                default:
                    return -1;
            }
        }

        /// <summary>
        /// Instantiates a FT232H device object.
        /// </summary>
        /// <param name="flags">Indicates device state.</param>
        /// <param name="type">Indicates the device type.</param>
        /// <param name="id">The Vendor ID and Product ID of the device.</param>
        /// <param name="locId">The physical location identifier of the device.</param>
        /// <param name="serialNumber">The device serial number.</param>
        /// <param name="description">The device description.</param>
        public Ft232HDevice(FtFlag flags, FtDeviceType type, uint id, uint locId, string serialNumber, string description)
        : base(flags, type, id, locId, serialNumber, description)
        {
        }

        /// <summary>
        /// Instantiates a FT232H device object.
        /// </summary>
        /// <param name="ftDevice">a FT Device</param>
        public Ft232HDevice(FtDevice ftDevice)
            : base(ftDevice.Flags, ftDevice.Type, ftDevice.Id, ftDevice.LocId, ftDevice.SerialNumber, ftDevice.Description)
        {
        }

        /// <inheritdoc />
        public override ComponentInformation QueryComponentInformation()
        {
            return base.QueryComponentInformation() with { Description = "Ftx232 Device" };
        }
    }
}
