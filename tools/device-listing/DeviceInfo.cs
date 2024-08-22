// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace Iot.Tools.DeviceListing
{
    internal class DeviceInfo : IComparable<DeviceInfo>
    {
        public string Title { get; private set; }
        public string ReadmePath { get; private set; }
        public HashSet<string> Categories { get; private set; } = new HashSet<string>();
        public string CategoriesFilePath { get; private set; }
        public bool CategoriesFileExists { get; private set; }

        public DeviceInfo(string readmePath, string categoriesFilePath)
        {
            ReadmePath = readmePath;
            Title = GetTitle(readmePath) ?? "Error";
            CategoriesFilePath = categoriesFilePath;
            CategoriesFileExists = File.Exists(categoriesFilePath);

            ImportCategories();
        }

        public int CompareTo(DeviceInfo? other)
        {
            return Title.CompareTo(other?.Title);
        }

        private static string? GetTitle(string readmePath)
        {
            string[] lines = File.ReadAllLines(readmePath);
            if (lines[0].StartsWith("# "))
            {
                return lines[0].Substring(2);
            }

            return null;
        }

        private void ImportCategories()
        {
            if (!CategoriesFileExists)
            {
                Console.WriteLine($"Warning: Category file is missing. [{CategoriesFilePath}]");
                return;
            }

            foreach (string line in File.ReadAllLines(CategoriesFilePath))
            {
                if (line is not { Length: > 0 })
                {
                    continue;
                }

                if (!Categories.Add(line))
                {
                    Console.WriteLine($"Warning: Category `{line}` is duplicated in `{CategoriesFilePath}`");
                }
            }
        }
    }
}
