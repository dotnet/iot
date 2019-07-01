# Solomon Systech Ssd1306

## Summary
The SSD1306 is a single-chip CMOS OLED/PLED driver with controller for organic/polymer light emitting diode dot-matrix graphic display system. It consists of 128 segments and 64 commons. This IC is designed for Common Cathode type OLED panel.

## Device Family
**SSD1306**: https://cdn-shop.adafruit.com/datasheets/SSD1306.pdf

**SSD1327**: [SSD1327 datasheet pdf](https://github.com/SeeedDocument/Grove_OLED_1.12/raw/master/resources/SSD1327_datasheet.pdf)

### Related Devices
- [Adafruit PiOLED - 128x32 Monochrome OLED Add-on for Raspberry Pi](https://www.adafruit.com/product/3527)
- [SunFounder 0.96" Inch Blue I2C IIC Serial 128x64 OLED LCD LED SSD1306 Modul](https://www.amazon.com/SunFounder-SSD1306-Arduino-Raspberry-Display/dp/B014KUB1SA)
- [Diymall 0.96" Inch Blue and Yellow I2c IIC Serial Oled LCD LED Module 12864 128X64 for Arduino Display Raspberry PI 51 Msp420 Stim32 SCR](https://www.amazon.com/Diymall-Yellow-Arduino-Display-Raspberry/dp/B00O2LLT30)

## Binding Notes

This binding currently only supports commands and raw data.  Eventually, the plan is to create a graphics library that can send text and images to the device.

### Connection Type

The following connection types are supported by this binding.

- [X] I2C
- [ ] SPI

## References 
[Adafruit Python SSD1306](https://github.com/adafruit/Adafruit_Python_SSD1306)  
https://github.com/adafruit/Adafruit_Python_SSD1306/blob/master/examples/stats.py  
https://github.com/stefangordon/IoTCore-SSD1306-Driver  
