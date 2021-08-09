# Solomon Systech Ssd1351 - CMOS OLED

The SSD1351 is a single-chip CMOS OLED driver with controller for organic/polymer light emitting diode dot-matrix graphic display system. It consists of 384 segments and 128 commons. This IC is designed for Common Cathode type OLED panel.

## Documentation

[Adafruit SSD1351 Arduino Library](https://github.com/adafruit/Adafruit-SSD1351-library)

### Device Family

- SSD1351 [datasheet](https://cdn-shop.adafruit.com/datasheets/SSD1351-Revision+1.3.pdf)

### Related Devices

- [OLED Breakout Board - 16-bit Color 1.5"](https://www.adafruit.com/product/1431)
- [OLED Breakout Board - 16-bit Color 1.27"](https://www.adafruit.com/product/1673)

## Board

![Schematics](Ssd1351.Sample.png)

This uses AdaFruit breakount board that is wired to a Raspberry Pi as below

| Function      | Raspberry Pi | SSD 1351  |
|:------------- |:-------------| -----:|
| 5v Power | Pin2 - 5v | + |
| Ground | Pin4 - Gnd      |  G |
| SPI Output | Pin19 - MOSI_0      | SI |
| SPI Clock | Pin23 - SCLK_0 | CL |
| /Data Code | Pin16 - GPIO23     | DC |
| /Reset | Pin18 - GPIO24 | R |
| SPI Enable | Pin24  CEO_0 | OC |

## Usage

```csharp
using System.Device.Spi;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Iot.Device.Ssd1351;

const int pinID_DC = 23;
const int pinID_Reset = 24;

using Bitmap dotnetBM = new(128, 128);
using Graphics g = Graphics.FromImage(dotnetBM);
using SpiDevice displaySPI = SpiDevice.Create(new SpiConnectionSettings(0, 0) { Mode = SpiMode.Mode3, DataBitLength = 8, ClockFrequency = 12_000_000 /* 12MHz */ });
using Ssd1351 ssd1351 = new(displaySPI, pinID_DC, pinID_Reset);
ssd1351.ResetDisplayAsync().Wait();
InitDisplayForAdafruit(ssd1351);

while (true)
{
    foreach (string filepath in Directory.GetFiles(@"images", "*.png").OrderBy(f => f))
    {
        using Bitmap bm = (Bitmap)Bitmap.FromFile(filepath);
        g.Clear(Color.Black);
        g.DrawImageUnscaled(bm, 0, 0);
        ssd1351.SendBitmap(dotnetBM);
        Task.Delay(1000).Wait();
    }
}

void InitDisplayForAdafruit(Ssd1351 device)
{
    device.Unlock();  // Unlock OLED driver IC MCU interface from entering command
    device.MakeAccessible(); // Command A2,B1,B3,BB,BE,C1 accessible if in unlock state
    device.SetDisplayOff(); // Turn on sleep mode
    device.SetDisplayClockDivideRatioOscillatorFrequency(0x01, 0x0F); // 7:4 = Oscillator Frequency, 3:0 = CLK Div Ratio (A[3:0]+1 = 1..16)
    device.SetMultiplexRatio(); // Use all 128 common lines by default....
    device.SetSegmentReMapColorDepth(ColorDepth.ColourDepth65K, CommonSplit.OddEven, Seg0Common.Column127); // 0x74 Color Depth = 64K, Enable COM Split Odd Even, Scan from COM[N-1] to COM0. Where N is the Multiplex ratio., Color sequence is normal: B -> G -> R
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
```

## Binding Notes

This binding currently only supports commands and raw data. Eventually, the plan is to create a graphics library that can send text and images to the device.

The following connection types are supported by this binding.

- [X] SPI
