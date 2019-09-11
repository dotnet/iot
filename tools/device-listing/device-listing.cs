// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace Iot.Tools.DeviceListing
{
    class Program
    {
        static string[] s_categoriesToDisplay = new string[]
        {
            "adc",
            "accelerometer",
            "voc",
            "gas",
            "light",
            "barometer",
            "altimeter",
            "thermometer",
            "gyroscope",
            "compass",
            "lego",
            "motor",
            "imu",
            "magnetometer",
            "lcd",
            "hygrometer",
            "clock",
            "sonar",
            "distance",
            "pir",
            "motion",
            "display",
            "io-expander",
            "canbus",
            "proximity",
            "touch",
            "wireless",
            "pwm",
            "joystick",
            "color",
            "led",
            "spi",
        };

        static Dictionary<string, string> s_categoriesDescriptions = new Dictionary<string, string>()
        {
            { "adc", "Analog/Digital converters" },
            { "accelerometer", "Accelerometers" },
            { "voc", "Volatile Organic Compound sensors" },
            { "gas", "Gas sensors" },
            { "light", "Light sensor" },
            { "barometer", "Barometers" },
            { "altimeter", "Altimeters" },
            { "thermometer", "Thermometers" },
            { "gyroscope", "Gyroscopes" },
            { "compass", "Compasses" },
            { "lego", "Lego related devices" },
            { "motor", "Motor controllers/drivers" },
            { "imu", "Inertial Measurement Units" },
            { "magnetometer", "Magnetometers" },
            { "lcd", "Liquid Crystal Displays" },
            { "hygrometer", "Hygrometers" },
            { "rtc", "Real Time Clocks" },
            { "clock", "Clocks" },
            { "sonar", "Sonars" },
            { "distance", "Distance sensors" },
            { "pir", "Passive InfraRed (motion) sensors" },
            { "motion", "Motion sensors" },
            { "display", "Displays" },
            { "segment", "Segment displays" },
            { "io-expander", "GPIO Expanders" },
            { "canbus", "CAN BUS libraries/modules" },
            { "proximity", "Proximity sensors" },
            { "touch", "Touch sensors" },
            { "wireless", "Wireless communication modules" },
            { "radio", "Radio modules" },
            { "pwm", "PWM libraries/modules" },
            { "spi", "SPI libraries/modules" },
            { "joystick", "Joysticks" },
            { "color", "Color sensors" },
            { "led", "LED drivers" },
            { "characterlcd", null },
            { "brickpi3", null },
            { "buzzer", null },
            { "gopigo3", null },
            { "grovepi", null },
        };

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
                string categories = Path.Combine(directory, "category.txt");
                if (File.Exists(readme))
                {
                    var device = new DeviceInfo(readme, categories);

                    if (device.Title == null)
                    {
                        Console.WriteLine($"Warning: Directory `{directory}` contains readme file without title on the first line.");
                        continue;
                    }

                    devices.Add(device);
                }
                else
                {
                    Console.WriteLine($"Warning: Directory `{directory}` does not have a README.md file.");
                }
            }

            devices.Sort();

            var allCategories = new HashSet<string>();

            foreach (DeviceInfo device in devices)
            {
                foreach (string category in device.Categories)
                {
                    if (allCategories.Add(category))
                    {
                        if (!s_categoriesDescriptions.ContainsKey(category))
                        {
                            Console.WriteLine($"Warning: Category `{category}` is missing description (`{device.Title}`).");
                        }
                    }
                }
            }

            string alphabeticalDevicesIndex = Path.Combine(devicesPath, "Device-Index.md");
            string deviceListing = GetDeviceListing(devicesPath, devices);
            ReplacePlaceholder(alphabeticalDevicesIndex, "devices", deviceListing);

            string categorizedDeviceListing = GetCategorizedDeviceListing(devicesPath, devices);
            string devicesReadme = Path.Combine(devicesPath, "README.md");
            ReplacePlaceholder(devicesReadme, "categorizedDevices", categorizedDeviceListing);
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

        private static string GetCategorizedDeviceListing(string devicesPath, IEnumerable<DeviceInfo> devices)
        {
            var deviceListing = new StringBuilder();
            foreach (string categoryToDisplay in s_categoriesToDisplay)
            {
                deviceListing.AppendLine($"### {s_categoriesDescriptions[categoryToDisplay]}");
                deviceListing.AppendLine();

                string listingInCurrentCategory = GetDeviceListing(devicesPath, devices.Where((d) => d.Categories.Contains(categoryToDisplay)));
                deviceListing.AppendLine(listingInCurrentCategory);
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
            return dirName == "Common" || dirName == "Units" || dirName == "Interop";
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
