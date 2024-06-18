// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Drawing;
using Iot.Device.Graphics.SkiaSharpAdapter;
using Iot.Device.Ssd13xx;
using SkiaSharp;

namespace Ssd13xx.Samples.Simulations
{
    /// <summary>
    /// A demo simulation of falling sand for SSD1309 displays
    /// </summary>
    public class FallingSandSimulation : Ssd1309Simulation
    {
        private readonly Random _random;
        private readonly int _grainsPerPour;

        private int _grainsPoured;
        private int _pourColumn;

        private SKCanvas _canvas;

        /// <summary>
        /// A demo simulation of falling sand for SSD1309 displays
        /// </summary>
        /// <param name="display">Ssd1309 display device</param>
        /// <param name="fps">Frames-per-second of the simulation</param>
        /// <param name="grainsPerPour">Number of grains to pour per column before moving to a random column</param>
        /// <param name="debug">Toggle buffer debug logging on each frame</param>
        public FallingSandSimulation(Ssd1309 display, int fps = 1, int grainsPerPour = 20, bool debug = false)
            : base(display, fps, debug: debug)
        {
            _grainsPerPour = grainsPerPour;
            _random = new Random();
            _pourColumn = _random.Next(_display.ScreenWidth);
            var drawingApi = _renderState.GetDrawingApi();
            _canvas = drawingApi.GetCanvas();
            drawingApi.DrawText("Hello Sand!", "DejaVu Sans", 18, Color.White, new Point(15, 0));
        }

        /// <summary>
        /// Generates the next frame of the falling sand simulation.
        /// </summary>
        protected override void Update()
        {
            if (_grainsPoured < _grainsPerPour)
            {
                // Pour a single grain of sand in the current pour position
                SetPixel(_pourColumn, 0, SKColors.White);
                _grainsPoured++;
            }
            else
            {
                // Move the pour position to a random column
                _pourColumn = _random.Next(_display.ScreenWidth);
                _grainsPoured = 0;
            }

            // Iterate starting at the bottom and working up
            for (int y = _display.ScreenHeight - 1; y >= 0; y--)
            {
                for (int x = 0; x < _display.ScreenWidth; x++)
                {
                    // If current pixel is sand, take action
                    if (GetPixelWithinBounds(x, y).Equals(SKColors.White))
                    {
                        // Move sand down if space is empty
                        if (GetPixelWithinBounds(x, y + 1).Equals(SKColors.Black))
                        {
                            SetPixel(x, y, SKColors.Black);
                            SetPixel(x, y + 1, SKColors.White);
                        }

                        // Move sand down-left if space is empty
                        else if (GetPixelWithinBounds(x - 1, y + 1).Equals(SKColors.Black))
                        {
                            SetPixel(x, y, SKColors.Black);
                            SetPixel(x - 1, y + 1, SKColors.White);
                        }

                        // Move sand down-right if space is empty
                        else if (GetPixelWithinBounds(x + 1, y + 1).Equals(SKColors.Black))
                        {
                            SetPixel(x, y, SKColors.Black);
                            SetPixel(x + 1, y + 1, SKColors.White);
                        }
                    }
                }
            }
        }

        private void SetPixel(int x, int y, SKColor color)
        {
            _canvas.DrawPoint(new SKPoint(x, y), color);
        }

        /// <returns>Null if the pixel is outside the bounds of the display</returns>
        private SKColor? GetPixelWithinBounds(int x, int y)
        {
            if (x >= 0 && x < _display.ScreenWidth - 1)
            {
                if (y >= 0 && y < _display.ScreenHeight - 1)
                {
                    return ConvertColor(_renderState.GetPixel(x, y));
                }
            }

            return null;
        }

        private SKColor ConvertColor(Color c)
        {
            return new SKColor(c.R, c.G, c.B, c.A);
        }
    }
}
