
# AD5328 - Digital to Analog Convertor

AD5328 is an Digital-to-Analog converter (DAC) with 12 bits of resolution.

## Usage
```csharp
using System.Device.Spi;
using System.Threading;
using Iot.Device.DAC;
using UnitsNet;

var spisettings = new SpiConnectionSettings(0, 1)
{
    Mode = SpiMode.Mode2
};

var spidev = SpiDevice.Create(spisettings);
var dac = new AD5328(spidev, ElectricPotential.FromVolts(2.5), ElectricPotential.FromVolts(2.5));
Thread.Sleep(1000);
dac.SetVoltage(0, ElectricPotential.FromVolts(1));
```

## References
https://www.analog.com/en/products/ad5328.html
