// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace Iot.Tools.DeviceListing
{
    class DeviceInfo : IComparable<DeviceInfo>
    {
        public string Title { get; private set; }
        public string ReadmePath { get; private set; }
        public HashSet<string> Categories { get; private set; } = new HashSet<string>();

        public DeviceInfo(string readmePath, string categoriesFilePath)
        {
            ReadmePath = readmePath;
            Title = GetTitle(readmePath);

            ImportCategories(categoriesFilePath);
        }

        public int CompareTo(DeviceInfo other)
        {
            return Title.CompareTo(other.Title);
        }

        private void ImportCategories(string categoriesFilePath)
        {
            if (!File.Exists(categoriesFilePath))
            {
                Console.WriteLine($"Warning: {categoriesFilePath} is missing");
                return;
            }

            foreach (string line in File.ReadAllLines(categoriesFilePath))
            {
                if (string.IsNullOrEmpty(line))
                {
                    continue;
                }

                if (!Categories.Add(line))
                {
                    Console.WriteLine($"Warning: Category `{line}` is duplicated in `{categoriesFilePath}`");
                }
            }
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
