// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Drawing;
using Iot.Device.Graphics;
using Iot.Device.Ws28xx;

namespace LEDStripSample
{
    /// <summary>
    /// This type defines several animation examples.
    /// </summary>
    public class Animations
    {
        private int _ledCount;
        private Iot.Device.Ws28xx.Ws28xx _ledStrip;

        /// <summary>
        /// Initializes a new instance of the <see cref="Animations"/> class.
        /// </summary>
        /// <param name="ledStrip">The led strip.</param>
        /// <param name="ledCount">The led count.</param>
        public Animations(Iot.Device.Ws28xx.Ws28xx ledStrip, int ledCount)
        {
            _ledStrip = ledStrip;
            _ledCount = ledCount;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the attached strips supports a separate white LED (warmwhite/coldwhite).
        /// </summary>
        /// <value>
        ///   <c>true</c> if [supports separate white]; otherwise, <c>false</c>.
        /// </value>
        public virtual bool SupportsSeparateWhite { get; set; } = false;

        /// <summary>
        /// Wipes the selected color.
        /// </summary>
        /// <param name="color">The color.</param>
        public void ColorWipe(Color color)
        {
            var img = _ledStrip.Image;
            for (var i = 0; i < _ledCount; i++)
            {
                img.SetPixel(i, 0, color);
                _ledStrip.Update();
                Thread.Sleep(25);
            }
        }

        /// <summary>
        /// Filters the color in regards of the .
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns></returns>
        public Color FilterColor(Color source)
        {
            return SupportsSeparateWhite ? Color.FromArgb(0, source.R, source.G, source.B) : source;
        }

        /// <summary>
        /// Animation similar to "Knight Rider".
        /// </summary>
        /// <param name="token">The token.</param>
        public async void KnightRider(CancellationToken token)
        {
            var img = _ledStrip.Image;
            var downDirection = false;

            var beamLength = 15;

            var index = 0;
            while (!token.IsCancellationRequested)
            {
                for (int i = 0; i < _ledCount; i++)
                {
                    img.SetPixel(i, 0, Color.FromArgb(0, 0, 0, 0));
                }

                if (downDirection)
                {
                    for (int i = 0; i <= beamLength; i++)
                    {
                        if (index + i < _ledCount && index + i >= 0)
                        {
                            var redValue = (beamLength - i) * (255 / (beamLength + 1));
                            img.SetPixel(index + i, 0, Color.FromArgb(0, redValue, 0, 0));
                        }
                    }

                    index--;
                    if (index < -beamLength)
                    {
                        downDirection = false;
                        index = 0;
                    }
                }
                else
                {
                    for (int i = beamLength - 1; i >= 0; i--)
                    {
                        if (index - i >= 0 && index - i < _ledCount)
                        {
                            var redValue = (beamLength - i) * (255 / (beamLength + 1));
                            img.SetPixel(index - i, 0, Color.FromArgb(0, redValue, 0, 0));
                        }
                    }

                    index++;
                    if (index - beamLength >= _ledCount)
                    {
                        downDirection = true;
                        index = _ledCount - 1;
                    }
                }

                _ledStrip.Update();
                await Task.Delay(10).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Rainbows the specified count.
        /// </summary>
        /// <param name="token">The token.</param>
        public void Rainbow(CancellationToken token)
        {
            RawPixelContainer img = _ledStrip.Image;
            while (!token.IsCancellationRequested)
            {
                for (var i = 0; i < 255; i++)
                {
                    if (token.IsCancellationRequested)
                    {
                        break;
                    }

                    for (var j = 0; j < _ledCount; j++)
                    {
                        if (token.IsCancellationRequested)
                        {
                            break;
                        }

                        img.SetPixel(j, 0, Wheel((i + j) & 255));
                    }

                    _ledStrip.Update();
                    Thread.Sleep(25);
                }
            }
        }

        /// <summary>
        /// Sets the color of the entire strip.
        /// </summary>
        /// <param name="color">The color.</param>
        /// <param name="count">The count.</param>
        public void SetColor(Color color, int count)
        {
            RawPixelContainer img = _ledStrip.Image;
            for (var i = 0; i < count; i++)
            {
                img.SetPixel(i, 0, color);
            }

            _ledStrip.Update();
        }

        /// <summary>
        /// Sets the white value using a percentag.
        /// </summary>
        /// <param name="colorPercentage">The color percentage.</param>
        /// <param name="separateWhite">if set to <c>true</c> [separate white].</param>
        public void SetWhiteValue(float colorPercentage, bool separateWhite = false)
        {
            var color = Color.FromArgb(separateWhite ? (int)(255 * colorPercentage) : 0, !separateWhite ? (int)(255 * colorPercentage) : 0, !separateWhite ? (int)(255 * colorPercentage) : 0, !separateWhite ? (int)(255 * colorPercentage) : 0);
            SetColor(color, _ledCount);
        }

        /// <summary>
        /// Switches the LEDs off.
        /// </summary>
        public void SwitchOffLeds()
        {
            var img = _ledStrip.Image;
            img.Clear();
            _ledStrip.Update();
        }

        /// <summary>
        /// Theatre Chase animation.
        /// </summary>
        /// <param name="color">The color.</param>
        /// <param name="blankColor">Color of the blank.</param>
        /// <param name="token">The token.</param>
        public void TheatreChase(Color color, Color blankColor, CancellationToken token)
        {
            RawPixelContainer img = _ledStrip.Image;
            while (!token.IsCancellationRequested)
            {
                for (var j = 0; j < 3; j++)
                {
                    for (var k = 0; k < _ledCount; k += 3)
                    {
                        img.SetPixel(j + k, 0, color);
                    }

                    _ledStrip.Update();
                    Thread.Sleep(100);

                    for (var k = 0; k < _ledCount; k += 3)
                    {
                        img.SetPixel(j + k, 0, blankColor);
                    }
                }
            }
        }

        private Color Wheel(int position)
        {
            if (position < 85)
            {
                return Color.FromArgb(0, position * 3, 255 - position * 3, 0);
            }
            else if (position < 170)
            {
                position -= 85;
                return Color.FromArgb(0, 255 - position * 3, 0, position * 3);
            }
            else
            {
                position -= 170;
                return Color.FromArgb(0, 0, position * 3, 255 - position * 3);
            }
        }
    }
}
