# Tlc1543

## Summary
High-speed 10-bit switched-capacitor successive-approximation A/D Converter with 14 channels (11 inputs and 3 self-test channels)

## Device Family
**[TLC1542/3]**: [Datasheet](https://www.ti.com/lit/ds/symlink/tlc1543.pdf)

## Binding Notes
(For now) Implemented Fast Mode 1 which doesn't need EOC pin connected. Respective timing diagram can be seen on figure 9 in datasheet.

It is possible to change ADC charge channel.
```c#
_adc.ChargeChannel = Tlc1543.Channel.SelfTest0;
```
