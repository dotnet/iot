# Key Matrix

An M×N key matrix driver.

(M is number of output pins and N is number of input pins.)

## Summary

These key matrices look like this:

![4x4-Keypad](http://www.waveshare.net/photo/accBoard/4x4-Keypad/4x4-Keypad-3.jpg)

This is a 4×4 matrix. And [here is the schematic](http://www.waveshare.net/w/upload/3/3d/4x4-Keypad_schematic.pdf)

You can connect any M×N key matrix, theoretically, by using M+N GPIO pins.

You can also use any compatible GPIO controller like [Mcp23xxx](../Mcp23xxx) instead of native controller.

* Using diodes(eg. 1N4148) for each button prevents "ghosting" or "masking" problem.

* Input pins need pull-down resistors connect to ground if your MCU don't have it.

* If your key matrix doesn't work well, try to swap output and input pins.

## References

http://pcbheaven.com/wikipages/How_Key_Matrices_Works/

http://www.waveshare.net/shop/4x4-Keypad.htm
