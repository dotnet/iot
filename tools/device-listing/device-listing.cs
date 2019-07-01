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
    class Program
    {
        static void Main(string[] args)
        {
            string repoRoot = FindRepoRoot(Environment.CurrentDirectory);

            if (repoRoot == null)
            {
                Console.WriteLine("Error: not in a git repository");
                return;
            }

            string devicesPath = Path.Combine(repoRoot, "src", "devices");

            List<DeviceInfo> devices = new List<DeviceInfo>();

            foreach (string directory in Directory.EnumerateDirectories(devicesPath))
            {
                if (IsIgnoredDevice(directory))
                {
                    continue;
                }

                string readme = Path.Combine(directory, "README.md");
                if (File.Exists(readme))
                {
                    var device = new DeviceInfo(readme);

                    if (device.Title == null)
                    {
                        Console.WriteLine($"Directory `{directory}` contains readme file without title on the first line.");
                        continue;
                    }

                    devices.Add(device);
                }
                else
                {
                    Console.WriteLine($"Directory `{directory}` does not have a README.md file.");
                }
            }

            devices.Sort();

            string deviceListing = GetDeviceListing(devicesPath, devices);
            ReplacePlaceholder(Path.Combine(devicesPath, "README.md"), "devices", deviceListing);
        }

        private static string GetDeviceListing(string devicesPath, IEnumerable<DeviceInfo> devices)
        {
            var deviceListing = new StringBuilder();
            foreach (DeviceInfo device in devices)
            {
                deviceListing.AppendLine($"* [{device.Title}]({GetRelativePathSimple(device.ReadmePath, devicesPath)})");
            }

            return deviceListing.ToString();
        }

        private static string FindRepoRoot(string dir)
        {
            if (!string.IsNullOrEmpty(dir))
            {
                if (Directory.Exists(Path.Combine(dir, ".git")))
                {
                    return dir;
                }
                else
                {
                    DirectoryInfo parentDir = new DirectoryInfo(dir).Parent;
                    return parentDir == null ? null : FindRepoRoot(parentDir.FullName);
                }
            }

            return null;
        }

        // simple means it won't try to use ".."
        private static string GetRelativePathSimple(string path, string parentPath)
        {
            if (path.StartsWith(parentPath))
            {
                int i = parentPath.Length;
                if (path[i] == '/' || path[i] == '\\')
                {
                    i++;
                }

                return path.Substring(i);
            }
            else
            {
                throw new Exception($"No common path between `{path}` and `{parentPath}`");
            }
        }

        private static bool IsIgnoredDevice(string path)
        {
            string dirName = new DirectoryInfo(path).Name;
            return dirName == "Common" || dirName == "Units";
        }

        private static void ReplacePlaceholder(string filePath, string placeholderName, string newContent)
        {
            string fileContent = File.ReadAllText(filePath);

            string startTag = $"<{placeholderName}>";
            string endTag = $"</{placeholderName}>";

            int startIdx = fileContent.IndexOf(startTag);
            int endIdx = fileContent.IndexOf(endTag);

            if (startIdx == -1 || endIdx == -1)
            {
                throw new Exception($"`{startTag}` not found in `{filePath}`");
            }

            startIdx += startTag.Length;

            File.WriteAllText(
                filePath,
                fileContent.Substring(0, startIdx) +
                Environment.NewLine +
                // Extra empty line is needed so that github does not break bullet points
                Environment.NewLine +
                newContent +
                fileContent.Substring(endIdx));
        }
    }
}
