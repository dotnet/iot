# MBI5027 -- 16-bit shift register with error detection

[MBI5027](http://archive.fairchip.com/pdf/MACROBLOCK/MBI5027.pdf) is a 16-bit shift register. Per the datasheet, the MBI5027 is a "16-bit Constant Current LED Sink Driver with Open/Short Circuit Detection". The [`Mbi5027` binding](Mbi5027.cs) is based on and is compatible with the more general [`ShiftRegister`](../ShiftRegister/README.md) binding. The `Mbi5027` binding adds error detection functionality. Either binding can be used to control the MBI5027.

![MBI5027](https://user-images.githubusercontent.com/2608468/89208974-4216cd00-d572-11ea-98eb-14a9a9b4614f.png)

The MBI5027 is similar to the commonly used [SN74HC595](../Sn74hc595/README.md) shift register, with some key differences.

* The MBI5027 has 16 outputs while the SN74HC595 has 8.
* The MBI5027 is a current sink (as opposed to source), which means you connect the cathode (ground), not anode (power) legs, to the outputs. The current comes towards the sink, not away from it.
* The MBI5027 provides a configurable constant current, which means that resistors are not needed per output. A single resistor is used, connected to R-EXT, to configure the currrent.
* The MBI5027 provides the ability to detect errors, per output.
* The SN74HC595 provides a storage register clear capability, which the MBI5027 lacks.

Note: The [MBI5168](http://archive.fairchip.com/pdf/MACROBLOCK/MBI5168.pdf) is an 8-bit constant current sink without error detection, making it a more direct comparison to the SN74HC595.

The [MBI5027 sample](samples/README.md) demonstrates how to use the shift register. The [generic shift register sample](../ShiftRegister/samples/README.md) is more extensive and is compatible with the MBI5027.

## Usage

The following example code demonstrates how to use the MBI5027 with its most basic functions.

```csharp
var sr = new Mbi5027(Mbi5027PinMapping.Standard);

// Light up three of first four LEDs
sr.ShiftBit(1);
sr.ShiftBit(1);
sr.ShiftBit(0);
sr.ShiftBit(1);
sr.Latch();

// Clear register
sr.ShiftClear();

// Write to all 8 registers with a byte value
sr.ShiftByte(0b_1010_1010);
```

If you want to use SPI, see the [`ShiftRegister`](../ShiftRegister/README.md) binding, which includes more information on SPI.

## Example circuit

The following breadboard circuit demonstrates the correct wiring pattern, including error detection.

![MBI5027_BB_topview](https://user-images.githubusercontent.com/2608468/93656940-22811a00-f9e3-11ea-84db-94615a2e1a2b.png)


## Error detection

The MBI5027 provides the ability to detect errors, per output. This is very useful for remote deployments, to determine if repairs are required (for a traffic sign, for example). The MBI5027 requires transitioning to an error detection mode to detect errors and then back to normal mode for normal operation.

The following example code demonstrates how to detect output errors with the MBI5027.

```csharp
var sr = new Mbi5027(Mbi5027PinMapping.Standard);

// switch to error detection mode
sr.EnableDetectionMode();

// read error states, per output
var index = sr.BitLength - 1;
foreach (var value in sr.ReadOutputErrorStatus())
{
    Console.WriteLine($"Bit {index--}: {value}");
}

// switch back to normal mode
sr.EnableNormalMode();
```

Per the datasheet, data can be shifted into the storage register while reading the output error status and before re-entering normal mode.

When all 16 outputs are in use, and no errors are detected, you will see the following output. A `Low` state would be shown if the output is unused, is misconfigured or other error condition.

```console
Bit 15: High
Bit 14: High
Bit 13: High
Bit 12: High
Bit 11: High
Bit 10: High
Bit 9: High
Bit 8: High
Bit 7: High
Bit 6: High
Bit 5: High
Bit 4: High
Bit 3: High
Bit 2: High
Bit 1: High
Bit 0: High
```

Note: Error detection was found to work only with 5v power. When 3.3v power was used, error detection did not work correctly.

## Resources

* Datasheet: http://archive.fairchip.com/pdf/MACROBLOCK/MBI5027.pdf
* Purchase: Not widely available. [aliexpress.com/](https://www.aliexpress.com/) was used to purchase the unit used to write this binding.
