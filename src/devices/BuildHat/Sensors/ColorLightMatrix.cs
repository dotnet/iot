// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Text;
using Iot.Device.BuildHat.Models;

namespace Iot.Device.BuildHat.Sensors
{
    /// <summary>
    /// SPIKE 3x3 color light matrix.
    /// </summary>
    public class ColorLightMatrix : ActiveSensor
    {
        /// <inheritdoc/>
        public override string SensorName => "SPIKE 3x3 color light matrix";

        /// <summary>
        /// Color light matrix.
        /// </summary>
        /// <param name="brick">The brick.</param>
        /// <param name="port">The port.</param>
        protected internal ColorLightMatrix(Brick brick, SensorPort port)
            : base(brick, port, SensorType.SpikeEssential3x3ColorLightMatrix)
        {
            Brick.SendRawCommand($"port {(byte)Port} ; plimit 1 ; set -1\r");
        }

        /// <summary>
        /// Displays a progress bar style from 0 to 9.
        /// </summary>
        /// <param name="progress">The progress bar from 0 to 9.</param>
        public void DisplayProgressBar(byte progress)
        {
            if (progress > 9)
            {
                throw new ArgumentException("Progress can only be from 0 to 9");
            }

            Brick.SendRawCommand($"port {(byte)Port} ;  select 0 ; write1 c0 {progress}");
        }

        /// <summary>
        /// Displays the 9 leds with the same color.
        /// </summary>
        /// <param name="color">The color to use.</param>
        public void DisplayColor(LedColor color)
        {
            Brick.SendRawCommand($"port {(byte)Port} ;  select 1 ; write1 c1 {(byte)color:X}");
        }

        /// <summary>
        /// Displays each pixel with a color and brightness
        /// </summary>
        /// <param name="brightness">The brichtness from 0 (off) to 10 (full). Must be 9 elements.</param>
        /// <param name="colors">The color. Must be 9 elements.</param>
        public void DisplayColorPerPixel(ReadOnlySpan<byte> brightness, ReadOnlySpan<LedColor> colors)
        {
            if ((brightness.Length != 9) || (colors.Length != 9))
            {
                throw new ArgumentException("Brightness and colors must be 9 elements.");
            }

            StringBuilder command = new($"port {(byte)Port} ; select 2 ; write1 c2 ");
            for (int i = 0; i < brightness.Length; i++)
            {
                if (brightness[i] > 10)
                {
                    throw new ArgumentException("Brightness must be between 0 and 10");
                }

                command.Append($"{brightness[i]:X}{(byte)colors[i]:X} ");
            }

            command.Append("\r");
            Brick.SendRawCommand(command.ToString());
        }
    }
}
