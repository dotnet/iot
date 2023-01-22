# Board abstraction

A "board" is a piece of hardware that offers low-level interfaces to other devices. Typically, it has GPIO pins and one or multiple SPI or I2C busses.
There should be exactly one instance of a board class per hardware component in an application, but it is possible to work with multiple boards at once (i.e. when having a GPIO expander connected to the Raspberry Pi).

```csharp
using Board b = Board.Create();

using GpioController controller = b.CreateGpioController();
controller.OpenPin(1);
// ...

using bus = b.CreateOrGetI2cBus(1);
// ...
```
