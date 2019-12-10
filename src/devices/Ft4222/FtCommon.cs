// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
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
            uint numOfDevices;
            ftStatus = FtFunction.FT_CreateDeviceInfoList(out numOfDevices);

            Debug.WriteLine($"Number of devices: {numOfDevices}");
            if (numOfDevices == 0)
                throw new IOException($"No device found");

            for (uint i = 0; i < numOfDevices; i++)
            {
                Span<byte> sernum = stackalloc byte[16];
                Span<byte> desc = stackalloc byte[64];
                uint flags = 0;
                var devInfo = new DeviceInformation();
                FtDevice ftDevice;
                uint id;
                uint locId;
                IntPtr handle;
                ftStatus = FtFunction.FT_GetDeviceInfoDetail(i, out flags, out ftDevice, out id, out locId, in MemoryMarshal.GetReference(sernum), in MemoryMarshal.GetReference(desc), out handle);
                if (ftStatus != FtStatus.Ok)
                    throw new IOException($"Can't read device information on device index {i}, error {ftStatus}");
                devInfo.Type = ftDevice;
                devInfo.Id = id;
                devInfo.LocId = locId;
                devInfo.FtHandle = handle;
                devInfo.Flags = (FtFlag)flags;
                devInfo.SerialNumber = Encoding.ASCII.GetString(sernum.ToArray(), 0, 16);
                devInfo.Description = Encoding.ASCII.GetString(desc.ToArray(), 0, 64);
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
                throw new InvalidOperationException($"Can't find any open device to check the versions");

            SafeFtHandle ftHandle = new SafeFtHandle();
            var ftStatus = FtFunction.FT_OpenEx(devices[idx].LocId, FtOpenType.OpenByLocation, out ftHandle);
            if (ftStatus != FtStatus.Ok)
                throw new IOException($"Can't open the device to check chipset version, status: {ftStatus}");

            FtVersion ftVersion;
            ftStatus = FtFunction.FT4222_GetVersion(ftHandle, out ftVersion);
            if (ftStatus != FtStatus.Ok)
                throw new IOException($"Can't find versions of chipset and FT4222 dll, status: {ftStatus}");

            ftStatus = FtFunction.FT_Close(ftHandle);
            if (ftStatus != FtStatus.Ok)
                throw new IOException($"Can't close the device to check chipset version, status: {ftStatus}");


            Version chip = new Version((int)(ftVersion.ChipVersion >> 24), (int)((ftVersion.ChipVersion >> 16) & 0xFF), (int)((ftVersion.ChipVersion >> 8) & 0xFF), (int)(ftVersion.ChipVersion & 0xFF));
            Version dll = new Version((int)(ftVersion.dllVersion >> 24), (int)((ftVersion.dllVersion >> 16) & 0xFF), (int)((ftVersion.dllVersion >> 8) & 0xFF), (int)(ftVersion.dllVersion & 0xFF));

            return (chip, dll);
        }
    }
}
