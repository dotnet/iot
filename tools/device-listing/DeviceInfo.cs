// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.IO;
using System.Text;
using System.Threading;

namespace Iot.Tools.DeviceListing
{
    class DeviceInfo : IComparable<DeviceInfo>
    {
        public string Title { get; private set; }
        public string ReadmePath { get; private set; }

        public DeviceInfo(string readmePath)
        {
            ReadmePath = readmePath;
            Title = GetTitle(readmePath);
        }

        public int CompareTo(DeviceInfo other)
        {
            return Title.CompareTo(other.Title);
        }

        private static string GetTitle(string readmePath)
        {
            string[] lines = File.ReadAllLines(readmePath);
            if (lines[0].StartsWith("# "))
            {
                return lines[0].Substring(2);
            }

            return null;
        }
    }
}
