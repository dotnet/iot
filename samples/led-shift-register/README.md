# Blink LEDS using a Shift Register

The following sample demonstrates using the [`ShiftRegister` binding](../../src/devices/ShiftRegister/README.md).

```csharp
ShiftRegister sr = new(ShiftRegisterPinMapping.Minimal, 8);

// Light up three of first four LEDs
sr.ShiftBit(1);
sr.ShiftBit(1);
sr.ShiftBit(0);
sr.ShiftBit(1);
sr.Latch();

// Display for 1s
Thread.Sleep(1000);

// Write to all 8 registers with a byte value
// ShiftByte latches data by default
sr.ShiftByte(0b_1000_1101);
```

The example is intended to show lighting up 8 LEDs almost the same way with `ShiftBit` and `ShiftByte` APIs. The `ShiftBit` (and `Latch`) calls light three of the first four LEDs. The `ShiftByte` call lights up the first four LEDs exactly the same way, and in addition lights up the eighth LED.

Imagine you have a horizontal line of eight connected LEDs. After the four calls to `ShiftBit` above, you will see the first four LEDs in the following lit/unlit configuration, left to right: `1011`. The outcome is the opposite order in which the code is written because the shift register behaves like a [first-in-first-out (FIFO) queue](https://en.wikipedia.org/wiki/Queue_(abstract_data_type)). `ShiftByte` works the same way. If you shift in `0b_1010_1010`, you will see the following lit/unlit configuration, left to right: `10110001`. The `ShiftByte` [algorithm](https://github.com/dotnet/iot/blob/0b7733e5580ad55fc80e76e0587740d95c5ea0c2/src/devices/ShiftRegister/ShiftRegister.cs#L143-L152) starts from the most-significant byte (left-most) and ends with the least-significant byte (right-most).

You can use this binding with any shift register that supports the same protocol. This repo also has bindings for the [SN74HC595](../../src/devices/ShiftRegister/README.md) or [MBI5027](../../src/devices/Mbi5027/README.md) shift registers, which expose additional functionality.
