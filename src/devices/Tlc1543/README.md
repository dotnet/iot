# Tlc1543

## Summary

High-speed 10-bit switched-capacitor successive-approximation A/D Converter with 14 channels (11 inputs and 3 self-test channels)

## Device Family

**[TLC1542/3]**: [Datasheet](https://www.ti.com/lit/ds/symlink/tlc1543.pdf)

## Binding Notes

Only mode implemented is Fast Mode 1 (10 clocks and !ChipSelect high between conversion cycles). 
Respective timing diagram can be seen on figure 9 in datasheet.

It is possible to change ADC charge channel.

```c#
adc.ReadPreviousAndChargeChannel(channels[0]);
```

Using EndOfConversion mode is not yet supported.

### Available methods to use are:

#### int ReadChannel(Channel)

Simple way to poll one channel.
Uses 2 cycles:

- In first cycle sends address to read from

- In second one sends dummy ChargeChannel address and reads sent value