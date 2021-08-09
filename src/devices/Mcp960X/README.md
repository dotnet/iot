# MCP960X - device family of cold-junction compensated thermocouple to digital converter 

The MCP960X device family is an I2C interface cold-junction compensated thermocouple to digital converter

## Sensor Image
![Illustration of wiring from a Raspberry Pi device](device.png)

**Note:** _ThermocoupleType.K is configured for a K type thermocouple if you want to use a B,E,J,K,N,R,S, or T simply change the K to the thermocouple type of your choosing._

## Documentation 

**MCP960X** [datasheet](https://www.microchip.com/en-us/product/MCP9600)

## Usage

The sample reads two temperatures. One is a connected thermocouple reading which can be read using the  ```GetTemperature``` command and the other is the temperature of the device itself which can be read using the ```GetColdJunctionTemperature``` command. The Cold Junction Temperature is used internally to increase the accuracy of the thermocouple but can also be read if you find a use for it.

```csharp
using System;
using System.Device.I2c;
using System.Threading;
using Iot.Device.Mcp960x;

Console.WriteLine("Write I2C MCP960X - ADR 0x67 - Read ambient and hot junction temperature every 1 sec - Press Ctrl+C to end.");

// set I2C bus ID: 1
// 0x67 is the device address
I2cConnectionSettings settings = new I2cConnectionSettings(1, 0x67);
I2cDevice i2cDevice = I2cDevice.Create(settings);
Mcp960x mcp960X = new Mcp960x(i2cDevice, coldJunctionResolutionType: ColdJunctionResolutionType.N_0_25);

DeviceIDType deviceIDType;
byte major;
byte minor;
mcp960X.ReadDeviceID(out deviceIDType, out major, out minor);
Console.WriteLine($"device id: {(byte)deviceIDType} - major: {major} - minor: {minor}");

while (true)
{
    Console.WriteLine($"ambient temperture: {mcp960X.GetColdJunctionTemperature()}");
    Console.WriteLine($"hot junction temperture: {mcp960X.GetTemperature()}");

    Thread.Sleep(1000);
}
```
