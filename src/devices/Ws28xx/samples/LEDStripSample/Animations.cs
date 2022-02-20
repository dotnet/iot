using Iot.Device.Graphics;
using Iot.Device.Ws28xx;
using System.Drawing;

namespace LEDStripSample
{
    public class Animations
    {
        protected int ledCount;
        protected Ws28xx ledStrip;

        public Animations(Ws28xx ledStrip, int ledCount)
        {
            this.ledStrip = ledStrip;
            this.ledCount = ledCount;
        }

        public virtual bool SupportsSeparateWhite { get; set; } = false;

        public void ColorWipe(Color color)
        {
            var img = ledStrip.Image;
            for (var i = 0; i < ledCount; i++)
            {
                img.SetPixel(i, 0, color);
                ledStrip.Update();
                Thread.Sleep(25);
            }
        }
        public void SetWhiteValue(float colorPercentage, bool separateWhite = false)
        {
            var color = Color.FromArgb(separateWhite ? (int)(255 * colorPercentage): 0, !separateWhite ? (int)(255 * colorPercentage) : 0, !separateWhite ? (int)(255 * colorPercentage) : 0, !separateWhite ? (int)(255 * colorPercentage) : 0);
            SetColor(color, ledCount);
        }

        public async void KnightRider(CancellationToken token)
        {
            // https://youtu.be/oNyXYPhnUIs

            var img = ledStrip.Image;
            var downDirection = false;

            var beamLength = 15;

            var index = 0;
            while (!token.IsCancellationRequested)
            {
                for (int i = 0; i < ledCount; i++)
                {
                    img.SetPixel(i, 0, Color.FromArgb(0, 0, 0, 0));
                }

                if (downDirection)
                {
                    for (int i = 0; i <= beamLength; i++)
                    {
                        if (index + i < ledCount && index + i >= 0)
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
                        if (index - i >= 0 && index - i < ledCount)
                        {
                            var redValue = (beamLength - i) * (255 / (beamLength + 1));
                            img.SetPixel(index - i, 0, Color.FromArgb(0, redValue, 0, 0));
                        }
                    }

                    index++;
                    if (index - beamLength >= ledCount)
                    {
                        downDirection = true;
                        index = ledCount - 1;
                    }
                }

                ledStrip.Update();
                await Task.Delay(10).ConfigureAwait(false);
            }
        }

        public void Rainbow(int count, CancellationToken token)
        {
            BitmapImage img = ledStrip.Image;
            while (!token.IsCancellationRequested)
            {
                for (var i = 0; i < 255; i++)
                {
                    if (token.IsCancellationRequested)
                    {
                        break;
                    }
                    for (var j = 0; j < count; j++)
                    {
                        if (token.IsCancellationRequested)
                        {
                            break;
                        }
                        img.SetPixel(j, 0, Wheel((i + j) & 255));
                    }

                    ledStrip.Update();
                    Thread.Sleep(25);
                }
            }
        }

        public void SetColor(Color color, int count)
        {
            BitmapImage img = ledStrip.Image;
            for (var i = 0; i < count; i++)
            {
                img.SetPixel(i, 0, color);
            }
            ledStrip.Update();
        }

        public void SwitchOffLeds()
        {
            var img = ledStrip.Image;
            img.Clear();
            ledStrip.Update();
        }

        public void TheatreChase(Color color, Color blankColor, CancellationToken token)
        {
            BitmapImage img = ledStrip.Image;
            while (!token.IsCancellationRequested)
            {
                for (var j = 0; j < 3; j++)
                {
                    for (var k = 0; k < ledCount; k += 3)
                    {
                        img.SetPixel(j + k, 0, color);
                    }

                    ledStrip.Update();
                    Thread.Sleep(100);

                    for (var k = 0; k < ledCount; k += 3)
                    {
                        img.SetPixel(j + k, 0, blankColor);
                    }
                }
            }
        }

        public Color Wheel(int position)
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

        public Color FilterColor(Color source)
        {
            return this.SupportsSeparateWhite ? Color.FromArgb(0, source.R, source.G, source.B) : source;
        }
    }
}