// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Iot.Device.Common;
using Iot.Device.FtCommon;

namespace Iot.Device.Ft4232H
{
    /// <summary>
    /// FT4232H Device
    /// </summary>
    public class Ft4232HDevice : Ftx232HDevice
    {
        /// <summary>
        /// Gets all the FT4232H connected
        /// </summary>
        /// <returns>A list of FT4232H</returns>
        public static List<Ft4232HDevice> GetFt2232H()
        {
            List<Ft4232HDevice> ft4232s = new List<Ft4232HDevice>();
            var devices = FtCommon.FtCommon.GetDevices(new FtDeviceType[] { FtDeviceType.Ft4232H, FtDeviceType.Ft4232HA, FtDeviceType.Ft4232HP });
            foreach (var device in devices)
            {
                ft4232s.Add(new Ft4232HDevice(device));
            }

            return ft4232s;
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
                case "BDBUS0":
                case "CDBUS0":
                case "DDBUS0":
                case "D0":
                case "TCK":
                case "SK":
                case "CLK":
                case "SDL":
                    return 0;
                case "ADBUS1":
                case "BDBUS1":
                case "CDBUS1":
                case "DDBUS1":
                case "D1":
                case "DO":
                case "TDI":
                case "SDA":
                case "MOSI":
                    return 1;
                case "ADBUS2":
                case "BDBUS2":
                case "CDBUS2":
                case "DDBUS2":
                case "D2":
                case "DI":
                case "TDO":
                case "MISO":
                    return 2;
                case "ADBUS3":
                case "BDBUS3":
                case "CDBUS3":
                case "DDBUS3":
                case "D3":
                case "TMS":
                case "CS":
                    return 3;
                case "ADBUS4":
                case "BDBUS4":
                case "CDBUS4":
                case "DDBUS4":
                case "D4":
                case "GPIOL0":
                    return 4;
                case "ADBUS5":
                case "BDBUS5":
                case "CDBUS5":
                case "DDBUS5":
                case "D5":
                case "GPIOL1":
                    return 5;
                case "ADBUS6":
                case "BDBUS6":
                case "CDBUS6":
                case "DDBUS6":
                case "D6":
                case "GPIOL2":
                    return 6;
                case "ADBUS7":
                case "BDBUS7":
                case "CDBUS7":
                case "DDBUS7":
                case "D7":
                case "GPIOL3":
                    return 7;
                default:
                    return -1;
            }
        }

        /// <summary>
        /// Instantiates a FT4232H device object.
        /// </summary>
        /// <param name="flags">Indicates device state.</param>
        /// <param name="type">Indicates the device type.</param>
        /// <param name="id">The Vendor ID and Product ID of the device.</param>
        /// <param name="locId">The physical location identifier of the device.</param>
        /// <param name="serialNumber">The device serial number.</param>
        /// <param name="description">The device description.</param>
        public Ft4232HDevice(FtFlag flags, FtDeviceType type, uint id, uint locId, string serialNumber, string description)
            : base(flags, type, id, locId, serialNumber, description)
        {
        }

        /// <summary>
        /// Instantiates a FT4232H device object.
        /// </summary>
        /// <param name="ftDevice">a FT Device</param>
        public Ft4232HDevice(FtDevice ftDevice)
            : base(ftDevice.Flags, ftDevice.Type, ftDevice.Id, ftDevice.LocId, ftDevice.SerialNumber, ftDevice.Description)
        {
        }
    }
}
