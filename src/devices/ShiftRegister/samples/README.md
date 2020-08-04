# Shift Register driver app

This [test application](Program.cs) tests the [ShiftRegister binding](../README.md).

It supports GPIO and SPI. It supports daisy chaining. The test app supports up to 4 shift registers (up to 32-bits). It would be easy to extend to a support more shift registers (switch from Int32 to Int64).
