// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Iot.Device.FtCommon
{
    /// <summary>
    /// Common static functions for the FT4222
    /// </summary>
    public class FtCommon
    {
        /// <summary>
        /// Returns the list of FTDI device connected
        /// </summary>
        /// <returns>A list of devices connected</returns>
        public static List<FtDevice> GetDevices()
        {
            List<FtDevice> devInfos = new List<FtDevice>();
            FtStatus ftStatus = 0;

            // Check device
            uint numOfDevices;
            ftStatus = FtFunction.FT_CreateDeviceInfoList(out numOfDevices);

            Debug.WriteLine($"Number of devices: {numOfDevices}");
            if (numOfDevices == 0)
            {
                return devInfos;
            }

            Span<byte> sernum = stackalloc byte[16];
            Span<byte> desc = stackalloc byte[64];
            for (uint i = 0; i < numOfDevices; i++)
            {
                uint flags = 0;
                FtDeviceType ftDevice;
                uint id;
                uint locId;
                IntPtr handle;
                ftStatus = FtFunction.FT_GetDeviceInfoDetail(i, out flags, out ftDevice, out id, out locId,
                    in MemoryMarshal.GetReference(sernum), in MemoryMarshal.GetReference(desc), out handle);
                if (ftStatus != FtStatus.Ok)
                {
                    throw new IOException($"Can't read device information on device index {i}, error {ftStatus}");
                }

                var devInfo = new FtDevice(
                    (FtFlag)flags,
                    ftDevice,
                    id,
                    locId,
                    Encoding.ASCII.GetString(sernum.ToArray(), 0, FindFirstZero(sernum)),
                    Encoding.ASCII.GetString(desc.ToArray(), 0, FindFirstZero(desc)));
                devInfos.Add(devInfo);
            }

            return devInfos;
        }

        /// <summary>
        /// Get specific list of devices
        /// </summary>
        /// <param name="ftDeviceTypes">The types of devices</param>
        /// <returns></returns>
        internal static List<FtDevice> GetDevices(FtDeviceType[] ftDeviceTypes)
        {
            List<FtDevice> ftdevices = new List<FtDevice>();
            var devices = GetDevices();
            foreach (var deviceType in ftDeviceTypes.Distinct())
            {
                ftdevices.AddRange(devices.Where(m => m.Type == deviceType));
            }

            return ftdevices;
        }

        private static int FindFirstZero(ReadOnlySpan<byte> span)
        {
            for (int i = 0; i < span.Length; i++)
            {
                if (span[i] == 0)
                {
                    return i;
                }
            }

            return span.Length;
        }
    }
}
