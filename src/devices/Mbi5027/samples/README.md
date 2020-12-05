# MBI5027 -- 16-Bit shift register driver app

This [test application](Program.cs) tests the [MBI5027 binding](../README.md).

It supports GPIO and SPI. It supports daisy chaining. The test app supports up to 2 shift registers (up to 32-bits). It would be easy to extend to a support more shift registers.

The following example demonstrates the sample with the last two output not connected. They are detected as open or short, with a `Low` reading.

```console
pi@raspberrypineapple:~/iot/src/devices/Mbi5027/samples $ ./bin/Debug/netcoreapp3.1/linux-arm/Mbi5027-driver
Driver for Mbi5027
Register bit length: 16
Checking circuit
Bit 15: Low
Bit 14: Low
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
Write 0 through 1000
Checking circuit
Bit 15: Low
Bit 14: Low
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
