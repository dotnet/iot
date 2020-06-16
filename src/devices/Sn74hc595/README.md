# SN74HC595 -- 8-Bit Shift Register

A shift register enables controlling or interacting with multiple devices, like LEDs, from a small number of pins. It requires using 3-5 pins to control 8 outputs. Shift registers can be daisy chained without requiring additional pins, enabling addressing a larger number of devices, limited only by voltage and the algorithms you define.

The [binding](Sn74hc595.cs) abstracts the interaction with the data register and the clock. It relies on specifying pinmapping for the various shift register pins.

![shift-register](https://user-images.githubusercontent.com/2608468/84733283-ac3bca00-af52-11ea-8520-67c91a45c0f0.png)

## Usage

```csharp
var controller = new GpioController();
var sr = new Sn74hc595(Sn74hc595.PinMapping.Standard, controller, true);

// Light up three of first four LEDs
sr.Shift(1);
sr.Shift(1);
sr.Shift(0);
sr.Shift(1);
sr.Latch();

// Clear register
sr.ShiftClear();

// Write to all 8 registers with a byte value
sr.ShiftByte(127);
sr.Latch()
```

[Sample](samples/README.md)

## Wiring diagram

![wiring diagram](sn74hc595-led-bar-graph_bb.png)

* [Fritzing file -- one shift register](sn74hc595-led-bar-graph.fzz)
* [Fritzing file -- two shift registers](sn74hc595-led-bar-graph-double-up.fzz)
* [Fritzing image -- two shift registers](sn74hc595-led-bar-graph-double-up_bb.png)

## Resources

* Datasheet: https://www.ti.com/lit/ds/symlink/sn74hc595.pdf
* Adafruit: https://www.adafruit.com/product/450
* Tutotial: https://www.youtube.com/watch?v=6fVbJbNPrEU