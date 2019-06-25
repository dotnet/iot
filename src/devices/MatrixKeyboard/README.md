# Matrix Keyboard

An M×N matrix keyboard driver.

(M is number of rows and N is number of columns.)

## Summary

These keyboards look like this:

![4x4-Keypad](http://www.waveshare.net/photo/accBoard/4x4-Keypad/4x4-Keypad-3.jpg)

This is a 4×4 keyboard. And [here is the schematic](http://www.waveshare.net/w/upload/3/3d/4x4-Keypad_schematic.pdf)

You can connect any M×N Matrix Keyboard, theoretically, by using M+N GPIO pins, N pull-down resistors (or similar network resistor), and a ground pin.

You can also use any compatible GPIO controller like [Mcp23xxx](../Mcp23xxx) instead of native controller.

## References

http://pcbheaven.com/wikipages/How_Key_Matrices_Works/

http://www.waveshare.net/shop/4x4-Keypad.htm
