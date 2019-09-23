# Board LEDs

This binding is used to control on-board LEDs.

## Usage

```C#
// Get all BoardLed instances of on-board LEDs.
IEnumerable<BoardLed> leds = BoardLed.EnumerateLeds();
// Open the LED with the specified name.
BoardLed led = new BoardLed("led0");
// Get all triggers of current LED.
IEnumerable<string> triggers = led.EnumerateTriggers();
// Get the max brightness of current LED.
int maxBrightness = led.MaxBrightness;
// Set trigger.
led.Trigger = "none";
// Set brightness.
led.Brightness = 255;
```