// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Device.Spi;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Iot.Device.Ssd1351.Samples
{
    class Program
    {
        private static void InitDisplayForAdafruit(Ssd1351 device)
        {
            device.Unlock();  // Unlock OLED driver IC MCU interface from entering command
            device.MakeAccessible(); // Command A2,B1,B3,BB,BE,C1 accessible if in unlock state  
            device.SetDisplayOff(); // Turn on sleep mode
            device.SetDisplayClockDivideRatioOscillatorFrequency(0x01, 0x0F); // 7:4 = Oscillator Frequency, 3:0 = CLK Div Ratio (A[3:0]+1 = 1..16) 
            device.SetMultiplexRatio(); // Use all 128 common lines by default....
            device.SetSegmentReMapColorDepth(ColorDepth.ColourDepth65K, CommonSplit.OddEven, Seg0Common.Column127); //0x74 Color Depth = 64K, Enable COM Split Odd Even, Scan from COM[N-1] to COM0. Where N is the Multiplex ratio., Color sequence is normal: B -> G -> R
            device.SetColumnAddress(); // Columns = 0 -> 127
            device.SetRowAddress(); // Rows = 0 -> 127
            device.SetDisplayStartLine(); // set startline to to 0
            device.SetDisplayOffset(0); // Set vertical scroll by Row to 0-127.
            device.SetGpio(GpioMode.Disabled, GpioMode.Disabled); // Set all GPIO to Input disabled
            device.SetVDDSource(); // Enable internal VDD regulator
            device.SetPreChargePeriods(2, 3); // Phase 1 period of 5 DCLKS,  Phase 2 period of 3 DCLKS
            device.SetNormalDisplay(); // Reset to Normal Display
            device.SetVcomhDeselectLevel(); // 0.82 x VCC
            device.SetContrastABC(0xC8, 0x80, 0xC8); // Contrast A = 200, B = 128, C = 200
            device.SetMasterContrast(); // No Change = 15
            device.SetVSL(); // External VSL
            device.Set3rdPreChargePeriod(0x01); // Set Second Pre-charge Period = 1 DCLKS
            device.SetDisplayOn(); //--turn on oled panel
            device.ClearScreen();
        }

        static void Main(string[] args)
        {
            const int pinID_DC = 23;
            const int pinID_Reset = 24;

            using (Bitmap dotnetBM = new Bitmap(128, 128))
            using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(dotnetBM))
            using (SpiDevice displaySPI = SpiDevice.Create(new SpiConnectionSettings(0, 0) { Mode = SpiMode.Mode3, DataBitLength = 8, ClockFrequency = 12_000_000 /* 12MHz */ }))
            using (Ssd1351 ssd1351 = new Ssd1351(displaySPI, pinID_DC, pinID_Reset))
            {
                ssd1351.ResetDisplayAsync().Wait();
                InitDisplayForAdafruit(ssd1351);

                while (true)
                {
                    foreach (string filepath in Directory.GetFiles(@"images", "*.png").OrderBy(f => f))
                    {
                        using (Bitmap bm = (Bitmap)Bitmap.FromFile(filepath))
                        {
                            g.Clear(Color.Black);
                            g.DrawImageUnscaled(bm, 0, 0);
                            ssd1351.SendBitmap(dotnetBM);
                            Task.Delay(1000).Wait();
                        }
                    }

                }
 
            }

        }
    }
}
