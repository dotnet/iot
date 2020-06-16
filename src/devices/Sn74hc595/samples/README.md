# SN74HC595 -- 8-Bit Shift Register Test app

This [test application](Program.cs) tests the [SN74HC595 binding](../README.md).

![shift-register](https://user-images.githubusercontent.com/2608468/84733283-ac3bca00-af52-11ea-8520-67c91a45c0f0.png)

For one shift register:

```csharp
var sr = new Sn74hc595(Sn74hc595.PinMapping.Standard, controller, true);
```

For two shift registers:

```csharp
var sr = new Sn74hc595(Sn74hc595.PinMapping.Standard, controller, true, 2);
```
