# LCD Display LCM1602C

## Summary
This device binding is really intended to work for the [TC1602A](https://cdn-shop.adafruit.com/datasheets/TC1602A-01T.pdf) display, but it will work for similar CharLCD displays which use a HD44780 controller. These LCD devices are relatively easy to work with, with the downside of using many Gpio pins. Most of these code was ported and adapted from [Adafruit](https://github.com/adafruit/Adafruit_Python_CharLCD/blob/master/Adafruit_CharLCD/Adafruit_CharLCD.py)

## Device Family
This binding has only been tested with the LCM1602A1 display on the CrowPi, but it should be compatible with any similar display which uses a [HD44780](https://www.sparkfun.com/datasheets/LCD/HD44780.pdf) controller.

## Binding Notes
These devices are controlled purely by GPIO. There are two different types of GPIO pins that are used, the control pins, and the data pins. The data pins are the ones that will send out the text that should be printed out on the LCD screen. This binding supports two different configurations for the data pins: using 4 data pins, and using 8 data pins. When using only 4 data pins, we will require two send two messages to the controller, each sending half of the byte that should be printed.

Here is a Hello World example of how to consume this binding:
```c#
using (var lcd = new Lcm1602a1(18, 5, new int[]{6, 16, 20, 21})) //using 4 data pins
{
    lcd.Print("Hello World!");
}
```

## References 
- Very complete tutorial on how to connect and work with one of these displays: https://learn.adafruit.com/drive-a-16x2-lcd-directly-with-a-raspberry-pi/overview
- Good guide explaining how the device works internally: http://www.site2241.net/november2014.htm
