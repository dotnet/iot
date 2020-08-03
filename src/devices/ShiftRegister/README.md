# Generic shift register binding

A shift register enables controlling multiple devices, like LEDs, using a small number of pins (minimum of 3). Shift registers can be daisy-chained without requiring additional pins, enabling addressing a larger number of devices, limited only by voltage and the algorithms you define.

![shift-register](https://user-images.githubusercontent.com/2608468/84733283-ac3bca00-af52-11ea-8520-67c91a45c0f0.png)

The [ShiftRegister](ShiftRegister.cs) binding is used as the base class for [Sn74hc595](../Sn74hc595/README.md) and [Mbi5027](../Mbi5027/README.md) bindings. It can be used directly, or you can rely on it as an implementation detail of hose other, more specific, bindings. It has been tested with with SN74HC595, MBI5027, and MBI5168 shift registers.

The [binding](ShiftRegister.cs) abstracts the interaction with the data register, the register clock and other shift register capabilities. The binding enables interaction via GPIO or SPI.

The [sample](samples/README.md) demonstrates how to use the shift register in some basic ways.

## Using GPIO

The binding can use `GpioController` pins to control the shift register. It relies on a GPIO [pin mapping](ShiftRegisterMapping.cs) to describe the pins that will be used.

The following example code demonstrates how to use a shift register with GPIO.

```csharp
var sr = new ShiftRegister(ShiftRegisterPinMapping.Standard);

// Light up three of first four LEDs
sr.ShiftBit(1);
sr.ShiftBit(1);
sr.ShiftBit(0);
sr.ShiftBit(1);
sr.Latch();

// Clear register
sr.ShiftClear();

// Write to all 8 registers with a byte value
sr.ShiftByte(0b_1010_1010); //same as integer 170
```

## Using SPI

The bindings can use a `SpiDevice` to control the shift register. The SPI protocol maps to the three required GPIO pins.

The following example code demonstrates how to use a shift register with SPI.

```csharp
var settings = new SpiConnectionSettings(0, 0);
var spiDevice = SpiDevice.Create(settings);
var sr = new ShiftRegister(spiDevice);

// Light up three of first four LEDs
// The Shift() method is dissallowed when using SPI
ShiftByte(11);

// Clear register
sr.ShiftClear();

// Write to all 8 registers with a byte value
sr.ShiftByte(0b_1010_1010);
```

The following diagram demonstrates the required wiring using SPI. If GPIO is also used, then two more wires would be required for shift register pins 13 and 10, which would be controlled with a `GpioController`. A constructor that takes both an `SpiDevice` and `GpioController` is provided for that case.

![sn74hc595-led-bar-graph-spi_bb](sn74hc595-led-bar-graph-spi_bb.png)

## Daisy-chaining

The binding supports daisy chaining, using either GPIO or SPI. The GPIO-based example below demonstrates how to declare support for controlling/addressing two -- daisy-chained -- shift registers. This is specified by the last (integer) value in the constructor. You can use the same approach if using SPI to control the shift register.

```csharp
using var controller = new GpioController();
var sr = new Sn74hc595(Sn74hc595.PinMapping.Matching, controller, false, 2);
```


You can write to multiple daisy chained device in one of several ways, as demonstrated in the following code. You wouldn't typically use of all these approaches, but pick one.

```csharp
// Write a value to each register bit
// And latch
// Only works with GPIO
for (int i = 0; i < sr.Bits; i++)
{
    sr.ShiftBit(1);
}
sr.Latch();

// Prints the following pattern to each register: 10101010
// 170 is the same as the binary literal: 0b10101010
for (int i = 0; i < sr.DeviceCount; i++)
{
    sr.ShiftByte(170);
}

// Downshift a 32-bit number to the desired number of daisy-chained devices
// Same thing could be done with a 64-bit integer if you have more than four shift registers
// Prints the following pattern across two registers (order will be reversed): 0001001110001000
// 5000 is the same as binary literal: 0b0001001110001000
int value = 0b0001001110001000; // 5000
for (int i = sr.DeviceCount - 1; i > 0; i--)
{
    int shift = i * 8;
    int downShiftedValue = value >> shift;
    sr.ShiftByte((byte)downShiftedValue);
}

sr.ShiftByte((byte)value);

// Print array of bytes
// Results in the same outcome as the "5000" example above
// The order has to be reversed (compared to example above); last byte will be left-most printed
var bytes = new byte[] { 0b10001000, 0b00010011};
foreach (var b in bytes)
{
    sr.ShiftByte(b);
}
```

The following wiring diagram can be used to support 2 shift registers.

![sn74hc595-led-bar-graph-spi_bb](sn74hc595-led-bar-graph-double-up_bb.png)

## Fritizing diagrams


* [One shift register -- GPIO](sn74hc595-led-bar-graph.fzz)
* [Two shift registers -- GPIO](sn74hc595-led-bar-graph-double-up.fzz)
* [One shift register -- SPI](sn74hc595-led-bar-graph-spi.fzz)

## Resources

* Datasheet: https://www.ti.com/lit/ds/symlink/sn74hc595.pdf
* Adafruit: https://www.adafruit.com/product/450
* Tutotial: https://www.youtube.com/watch?v=6fVbJbNPrEU