// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Iot.Device.BoardLed
{
    /// <summary>
    /// On-board LED on the device.
    /// </summary>
    public class BoardLed
    {
        private const string DefaultDevicePath = "/sys/class/leds";

        /// <summary>
        /// The name of the LED.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The kernel controls the trigger of the LED.
        /// </summary>
        public string Trigger { get => GetTrigger(); set => SetTrigger(value); }

        /// <summary>
        /// The current brightness of the LED.
        /// </summary>
        public int Brightness { get => GetBrightness("brightness"); set => SetBrightness(value); }

        /// <summary>
        /// The max brightness of the LED.
        /// </summary>
        public int MaxBrightness { get => GetBrightness("max_brightness"); }

        /// <summary>
        /// Creates a new instance of the BoardLed.
        /// </summary>
        /// <param name="name">The name of the LED to control.</param>
        public BoardLed(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Get all triggers of current LED.
        /// </summary>
        /// <returns>The name of triggers.</returns>
        public IEnumerable<string> EnumerateTriggers()
        {
            using StreamReader reader = new StreamReader(File.Open($"{DefaultDevicePath}/{Name}/trigger", FileMode.Open, FileAccess.Read));

            return Regex.Replace(reader.ReadToEnd(), @"\[|\]", "")      // remove selected chars
                .Split(' ');
        }

        /// <summary>
        /// Get all BoardLed instances of on-board LEDs.
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<BoardLed> EnumerateLeds()
        {
            var infos = Directory.GetDirectories(DefaultDevicePath)
                .Where(x => !x.Contains(':'))       // remove items like "mmc0::"
                .Select(x => new DirectoryInfo(x));

            return infos.Select(x => new BoardLed(x.Name));
        }

        private int GetBrightness(string fileName)
        {
            using StreamReader reader = new StreamReader(File.Open($"{DefaultDevicePath}/{Name}/{fileName}", FileMode.Open, FileAccess.Read));

            return int.Parse(reader.ReadToEnd());
        }

        private void SetBrightness(int value)
        {
            if (value > 255)
            {
                value = 255;
            }
            else if (value < 0)
            {
                value = 0;
            }

            using StreamWriter writer = new StreamWriter(File.Open($"{DefaultDevicePath}/{Name}/brightness", FileMode.Open));

            writer.Write(value);
        }

        private string GetTrigger()
        {
            using StreamReader reader = new StreamReader(File.Open($"{DefaultDevicePath}/{Name}/trigger", FileMode.Open, FileAccess.Read));

            return Regex.Match(reader.ReadToEnd(), @"(?<=\[)(.*)(?=\])").Value;
        }

        private void SetTrigger(string name)
        {
            using StreamWriter writer = new StreamWriter(File.Open($"{DefaultDevicePath}/{Name}/trigger", FileMode.Open));

            writer.Write(name);
        }
    }
}
