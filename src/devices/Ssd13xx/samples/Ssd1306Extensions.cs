using System;
using System.Linq;
using System.Collections.Generic;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Iot.Device.Ssd13xx.Samples
{
    /// <summary>
    /// Extension methods for Ssd1306 class.
    /// </summary>
    public static class Ssd1306Extensions
    {
        // Port from https://github.com/adafruit/Adafruit_Python_SSD1306/blob/8819e2d203df49f2843059d981b7347d9881c82b/Adafruit_SSD1306/SSD1306.py#L184

        /// <summary>
        /// Extension method to display image using Ssd1306 device.
        /// </summary>
        /// <param name="s">Ssd1306 object.</param>
        /// <param name="image">Image to display.</param>
        internal static void DisplayImage(this Ssd1306 s, Image<Gray16> image)
        {
            Int16 width = 128;
            Int16 pages = 4;
            List<byte> buffer = new ();

            for (int page = 0; page < pages; page++)
            {
                for (int x = 0; x < width; x++)
                {
                    int bits = 0;
                    for (byte bit = 0; bit < 8; bit++)
                    {
                        bits = bits << 1;
                        bits |= image[x, page * 8 + 7 - bit].PackedValue > 0 ? 1 : 0;
                    }

                    buffer.Add((byte)bits);
                }
            }

            int chunk_size = 16;
            for (int i = 0; i < buffer.Count; i += chunk_size)
            {
                s.SendData(buffer.Skip(i).Take(chunk_size).ToArray());
            }
        }
    }
}
