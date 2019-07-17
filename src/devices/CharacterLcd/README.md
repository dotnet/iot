# Character LCD (Liquid Crystal Display)

## Summary
This device binding is meant to work with character LCD displays which use a HD44780 compatible controller. Almost all character LCDs fall into this category. Simple wrappers for 16x2 and 20x4 variants are included.

## Device Family
This binding has been tested with a variety of 16x2 and 20x4 displays both in 4bit and 8bit mode and via i2C adapters (such as on the CrowPi). It should work with any character LCD with a 5x8 size character. Common names are 1602LCD and 2004LCD.

Also supports [Grove - LCD RGB Backlight](http://wiki.seeedstudio.com/Grove-LCD_RGB_Backlight/).

## Binding Notes
These devices are controlled purely by GPIO (except Grove LCD RGB Backlight). There are two different types of GPIO pins that are used, the control pins, and the data pins. The data pins are the ones that will send out the text that should be printed out on the LCD screen. This binding supports two different configurations for the data pins: using 4 data pins, and using 8 data pins. When using only 4 data pins, we will require two send two messages to the controller, each sending half of the byte that should be printed.

Here is a Hello World example of how to consume this binding:
```c#
using (var lcd = new Lcd1602(18, 5, new int[]{6, 16, 20, 21})) //using 4 data pins
{
    lcd.Write("Hello World!");
}
```

Grove LCD RGB Backlight uses two i2c devices:
- the device to control LCD (address 0x3E)
- the device to control RGB backlight (address 0x62)

(Make sure the Grove-LCD RGB Backlight is connected to a I2C port. Not the Digital port!)
Here is a Hello World example of how to consume Grove LCD RGB Backlight binding:
```c#
var i2cLcdDevice = I2cDevice.Create(new I2cConnectionSettings(busId: 1, deviceAddress: 0x3E));
var i2cRgbDevice = I2cDevice.Create(new I2cConnectionSettings(busId: 1, deviceAddress: 0x62));
using (var lcd = new LcdRgb1602(i2cLcdDevice, i2cRgbDevice))
{
    lcd.Write("Hello World!");
    lcd.SetBacklightColor(Color.Azure);
}
```

PCF8574T/PCF8574AT Sample
The I2C backpack based on the PCF8574T/AT IC uses specific pin mapping, to consume this device binding on this backpack use like so
```c#
var i2cDevice = I2cDevice.Create(new I2cConnectionSettings(busId: 1, deviceAddress: 0x27));
var controller = new Pcf8574(i2cDevice);
var lcd = new Lcd1602(registerSelectPin: 0, enablePin: 2, dataPins: new int[] { 4, 5, 6, 7}, backlightPin: 3, readWritePin: 1, controller: controller);
```
there is a full working example in the samples directory called Pcf8574tSample.cs
For PCF8574T i2c addresses can be between 0x27 and 0x20 depending on bridged solder jumpers and for PCF8574AT i2c addresses can be between 0x3f and 0x38 depending on bridged solder jumpers

## References
- Very complete tutorial on how to connect and work with one of these displays: https://learn.adafruit.com/drive-a-16x2-lcd-directly-with-a-raspberry-pi/overview
- Good guide explaining how the device works internally: http://www.site2241.net/november2014.htm
- Seeedstudio, Grove - LCD RGB Backlight library: https://github.com/Seeed-Studio/Grove_LCD_RGB_Backlight
- PCF8574T information https://alselectro.wordpress.com/2016/05/12/serial-lcd-i2c-module-pcf8574/
