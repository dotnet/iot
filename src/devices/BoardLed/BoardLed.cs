// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Iot.Device.BoardLed
{
    /// <summary>
    /// On-board LED on the device.
    /// </summary>
    public class BoardLed : IDisposable
    {
        private const string DefaultDevicePath = "/sys/class/leds";

        private StreamReader _brightnessReader;
        private StreamWriter _brightnessWriter;
        private StreamReader _triggerReader;
        private StreamWriter _triggerWriter;
        private StreamReader _maxBrightnessReader;

        /// <summary>
        /// The name of the LED.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The kernel controls the trigger of the LED.
        /// </summary>
        /// <remarks>
        /// The kernel provides some triggers which let the kernel control the LED.
        /// For example, the red light of Raspberry Pi, whose trigger is "default-on", which makes it keep lighting up.
        /// If you want to operate the LED, you need to remove the trigger, which is to set its trigger to none.
        /// </remarks>
        public string Trigger { get => GetTrigger(); set => SetTrigger(value); }

        /// <summary>
        /// The current brightness of the LED.
        /// </summary>
        public int Brightness { get => GetBrightness(); set => SetBrightness(value); }

        /// <summary>
        /// The max brightness of the LED.
        /// </summary>
        public int MaxBrightness { get => GetMaxBrightness(); }

        /// <summary>
        /// Creates a new instance of the BoardLed.
        /// </summary>
        /// <param name="name">The name of the LED to control.</param>
        public BoardLed(string name)
        {
            Name = name;

            Initialize();
        }

        /// <summary>
        /// Get all triggers of current LED.
        /// </summary>
        /// <returns>The name of triggers.</returns>
        public IEnumerable<string> EnumerateTriggers() => Regex.Replace(_triggerReader.ReadToEnd(), @"\[|\]", "")      // remove selected chars
                .Split(' ');

        /// <summary>
        /// Get all BoardLed instances of on-board LEDs.
        /// </summary>
        /// <returns>BoardLed instances.</returns>
        public static IEnumerable<BoardLed> EnumerateLeds()
        {
            var infos = Directory.GetDirectories(DefaultDevicePath)
                .Where(x => !x.Contains(':'))       // remove items like "mmc0::"
                .Select(x => new DirectoryInfo(x));

            return infos.Select(x => new BoardLed(x.Name));
        }

        /// <summary>
        /// Cleanup
        /// </summary>
        public void Dispose()
        {
            _brightnessReader?.Dispose();
            _brightnessWriter?.Dispose();    
            _triggerReader?.Dispose();
            _triggerWriter?.Dispose();
            _maxBrightnessReader?.Dispose();

            _brightnessReader = null;
            _brightnessWriter = null;
            _triggerReader = null;
            _triggerWriter = null;
            _maxBrightnessReader = null;
        }

        private int GetBrightness() => int.Parse(_brightnessReader.ReadToEnd());

        private int GetMaxBrightness() => int.Parse(_maxBrightnessReader.ReadToEnd());

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

            _brightnessWriter.Write(value);
            _brightnessWriter.Flush();
        }

        private string GetTrigger() => Regex.Match(_triggerReader.ReadToEnd(), @"(?<=\[)(.*)(?=\])").Value;

        private void SetTrigger(string name)
        {
            _triggerWriter.Write(name);
            _triggerWriter.Flush();
        }

        private void Initialize()
        {
            _brightnessReader = new StreamReader(File.Open($"{DefaultDevicePath}/{Name}/brightness", FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
            _brightnessWriter = new StreamWriter(File.Open($"{DefaultDevicePath}/{Name}/brightness", FileMode.Open, FileAccess.Write, FileShare.ReadWrite));
            _triggerReader = new StreamReader(File.Open($"{DefaultDevicePath}/{Name}/trigger", FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
            _triggerWriter = new StreamWriter(File.Open($"{DefaultDevicePath}/{Name}/trigger", FileMode.Open, FileAccess.Write, FileShare.ReadWrite));
            _maxBrightnessReader = new StreamReader(File.Open($"{DefaultDevicePath}/{Name}/max_brightness", FileMode.Open, FileAccess.Read));
        }
    }
}
