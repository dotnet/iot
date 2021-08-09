// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Iot.Device.FtCommon;

namespace Iot.Device.Ft4222
{
    /// <summary>
    /// Common static functions for the FT4222
    /// </summary>
    public class Ft4222Common
    {
        /// <summary>
        /// Get the versions of the chipset and dll
        /// </summary>
        /// <returns>Both the chipset and dll versions</returns>
        public static (Version? Chip, Version? Dll) GetVersions()
        {
            // First, let's find a device
            var devices = FtCommon.FtCommon.GetDevices();
            if (devices.Count == 0)
            {
                return (null, null);
            }

            // Check if the first not open device
            int idx = 0;
            for (idx = 0; idx < devices.Count; idx++)
            {
                if ((devices[idx].Flags & FtFlag.PortOpened) != FtFlag.PortOpened)
                {
                    break;
                }
            }

            if (idx == devices.Count)
            {
                throw new InvalidOperationException($"Can't find any open device to check the versions");
            }

            var ftStatus = FtFunction.FT_OpenEx(devices[idx].LocId, FtOpenType.OpenByLocation, out SafeFtHandle ftHandle);
            if (ftStatus != FtStatus.Ok)
            {
                throw new IOException($"Can't open the device to check chipset version, status: {ftStatus}");
            }

            FtVersion ftVersion;
            ftStatus = FtFunction.FT4222_GetVersion(ftHandle, out ftVersion);
            if (ftStatus != FtStatus.Ok)
            {
                throw new IOException($"Can't find versions of chipset and FT4222, status: {ftStatus}");
            }

            ftHandle.Dispose();

            Version chip = new Version((int)(ftVersion.ChipVersion >> 24), (int)((ftVersion.ChipVersion >> 16) & 0xFF),
                (int)((ftVersion.ChipVersion >> 8) & 0xFF), (int)(ftVersion.ChipVersion & 0xFF));
            Version dll = new Version((int)(ftVersion.DllVersion >> 24), (int)((ftVersion.DllVersion >> 16) & 0xFF),
                (int)((ftVersion.DllVersion >> 8) & 0xFF), (int)(ftVersion.DllVersion & 0xFF));

            return (chip, dll);
        }
    }
}
