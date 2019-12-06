// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace System.Device.Ft4222
{
    /// <summary>
    /// Common static functions for the FT4222
    /// </summary>
    public class FtCommon
    {
        /// <summary>
        /// Returns the list of FT4222 connected
        /// </summary>
        /// <returns>A list of devices connected</returns>
        public static List<DeviceInformation> GetDevices()
        {
            List<DeviceInformation> devInfos = new List<DeviceInformation>(); ;
            FtStatus ftStatus = 0;

            // Check device
            uint numOfDevices = 0;
            ftStatus = FtFunction.FT_CreateDeviceInfoList(ref numOfDevices);

            Debug.WriteLine($"Number of devices: {numOfDevices}");
            if (numOfDevices == 0)
                throw new IOException($"No device found");

            byte[] sernum = new byte[16];
            byte[] desc = new byte[64];
            uint flags = 0;
            for (int i = 0; i < numOfDevices; i++)
            {
                var devInfo = new DeviceInformation();
                ftStatus = FtFunction.FT_GetDeviceInfoDetail(0, ref flags, ref devInfo.Type, ref devInfo.Id, ref devInfo.LocId, sernum, desc, ref devInfo.FtHandle);
                devInfo.Flags = (FtFlag)flags;
                devInfo.SerialNumber = Encoding.ASCII.GetString(sernum, 0, 16);
                devInfo.Description = Encoding.ASCII.GetString(desc, 0, 64);
                devInfo.SerialNumber = devInfo.SerialNumber.Substring(0, devInfo.SerialNumber.IndexOf("\0"));
                devInfo.Description = devInfo.Description.Substring(0, devInfo.Description.IndexOf("\0"));
                devInfos.Add(devInfo);
            }
            return devInfos;
        }

        /// <summary>
        /// Get the versions of the chipset and dll
        /// </summary>
        /// <returns>Both the chipset and dll versions</returns>
        public static (Version chip, Version dll) GetVersions()
        {
            // First, let's find a device
            var devices = GetDevices();
            if (devices.Count == 0)
                return (null, null);
            // Check if the first not open device
            int idx = 0;
            for (idx = 0; idx < devices.Count; idx++)
            {
                if ((devices[idx].Flags & FtFlag.PortOpened) != FtFlag.PortOpened)
                    break;
            }
            if (idx == devices.Count)
                throw new Exception($"Can't find any closed device to check the versions");

            IntPtr ftHandle = IntPtr.Zero;
            var ftStatus = FtFunction.FT_OpenEx(devices[idx].LocId, FtOpenType.OpenByLocation, ref ftHandle);
            if (ftStatus != FtStatus.Ok)
                throw new IOException($"Can't open the device to check chipset version, status: {ftStatus}");

            FtVersion ftVersion = new FtVersion();
            ftStatus = FtFunction.FT4222_GetVersion(ftHandle, ref ftVersion);
            if (ftStatus != FtStatus.Ok)
                throw new IOException($"Can't find versions of chipset and FT4222 dll, status: {ftStatus}");

            ftStatus = FtFunction.FT_Close(ftHandle);
            if (ftStatus != FtStatus.Ok)
                throw new IOException($"Can't close the device to check chipset version, status: {ftStatus}");


            Version chip = new Version((int)((ftVersion.ChipVersion >> 24) & 0xFF), (int)((ftVersion.ChipVersion >> 16) & 0xFF), (int)((ftVersion.ChipVersion >> 8) & 0xFF), (int)(ftVersion.ChipVersion & 0xFF));
            Version dll = new Version((int)((ftVersion.DllVersion >> 24) & 0xFF), (int)((ftVersion.DllVersion >> 16) & 0xFF), (int)((ftVersion.DllVersion >> 8) & 0xFF), (int)(ftVersion.DllVersion & 0xFF));

            return (chip, dll);
        }
    }
}
