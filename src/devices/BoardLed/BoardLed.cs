// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
        /// Use <see cref="EnumerateTriggers()"/> to get all triggers.
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
        public IEnumerable<string> EnumerateTriggers()
        {
            _triggerReader.BaseStream.Position = 0;

            // Remove selected chars
            return Regex.Replace(_triggerReader.ReadToEnd(), @"\[|\]", string.Empty)
                .Split(' ');
        }

        /// <summary>
        /// Get all BoardLed instances of on-board LEDs.
        /// </summary>
        /// <returns>BoardLed instances.</returns>
        public static IEnumerable<BoardLed> EnumerateLeds()
        {
            IEnumerable<DirectoryInfo> infos = Directory.GetDirectories(DefaultDevicePath)
                .Select(x => new DirectoryInfo(x));

            // Make sure it's a real LED
            return infos.Where(x => x.EnumerateFiles().Select(f => f.Name).Contains("brightness"))
                .Select(x => new BoardLed(x.Name));
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

        private int GetBrightness()
        {
            _brightnessReader.BaseStream.Position = 0;

            return int.Parse(_brightnessReader.ReadToEnd());
        }

        private int GetMaxBrightness()
        {
            _maxBrightnessReader.BaseStream.Position = 0;

            return int.Parse(_maxBrightnessReader.ReadToEnd());
        }

        private void SetBrightness(int value)
        {
            value = Math.Clamp(value, 0, 255);

            _brightnessWriter.BaseStream.SetLength(0);

            _brightnessWriter.Write(value);
            _brightnessWriter.Flush();
        }

        private string GetTrigger()
        {
            _triggerReader.BaseStream.Position = 0;

            return Regex.Match(_triggerReader.ReadToEnd(), @"(?<=\[)(.*)(?=\])").Value;
        }

        private void SetTrigger(string name)
        {
            IEnumerable<string> triggers = EnumerateTriggers();

            if (!triggers.Contains(name))
            {
                throw new ArgumentException($"System does not contain a trigger called {name}.");
            }

            _triggerWriter.BaseStream.SetLength(0);

            _triggerWriter.Write(name);
            _triggerWriter.Flush();
        }

        private void Initialize()
        {
            FileStream brightnessStream = File.Open($"{DefaultDevicePath}/{Name}/brightness", FileMode.Open);
            _brightnessReader = new StreamReader(stream: brightnessStream, encoding: Encoding.ASCII, detectEncodingFromByteOrderMarks: false, bufferSize: 4, leaveOpen: true);
            _brightnessWriter = new StreamWriter(stream: brightnessStream, encoding: Encoding.ASCII, bufferSize: 4, leaveOpen: false);

            FileStream triggerStream = File.Open($"{DefaultDevicePath}/{Name}/trigger", FileMode.Open);
            _triggerReader = new StreamReader(stream: triggerStream, encoding: Encoding.ASCII, detectEncodingFromByteOrderMarks: false, bufferSize: 512, leaveOpen: true);
            _triggerWriter = new StreamWriter(stream: triggerStream, encoding: Encoding.ASCII, bufferSize: 512, leaveOpen: false);

            _maxBrightnessReader = new StreamReader(File.Open($"{DefaultDevicePath}/{Name}/max_brightness", FileMode.Open, FileAccess.Read));
        }
    }
}
