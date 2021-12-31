# NEO-M8

## Summary

u-blox M8 is Global Navigation Satellite System (GNSS).
It is using NMEA0183 protocol for communication.

## Binding Notes

This device uses UART and therefore regular PC with RS232 to TTL converter can be used (i.e. Raspberry PI is not required).
When using Raspberry PI use `raspi-config` to disable login shell and enable serial port (interfacing options).

### Communication methods

NEO-M8 supports multiple communication methods but only UART is currently supported:

- [X] UART
- [ ] USB
- [ ] SPI
- [ ] DDC (I2C compliant)

## NMEA 0183

Further information about supported NMEA sentences as well as advanced parsing methods can be found [here](../Nmea0183/README.md).

## Notes

Serial port name will need to be adjusted for this to work correctly.
You can find list of your ports using `System.IO.Ports.SerialPort.GetPortNames()` in `System.IO.Ports` package.
Serial port in the sample below is a default UART on Raspberry PI.

## Example code

```csharp
using System;
using Iot.Device.Nmea0183;
using Iot.Device.Nmea0183.Sentences;
using Iot.Device.Gps;

namespace Iot.Device.Gps.NeoM8Samples
{
    class Program
    {
        static void Main(string[] args)
        {
            using (NeoM8 neoM8 = new NeoM8("/dev/ttyS0"))
            {
                bool gotRmc = false;
                while (!gotRmc)
                {
                    TalkerSentence sentence = neoM8.Read();

                    object typed = sentence.TryGetTypedValue();
                    if (typed == null)
                    {
                        Console.WriteLine($"Sentence identifier `{sentence.Id}` is not known.");
                    }
                    else if (typed is RecommendedMinimumNavigationInformation rmc)
                    {
                        gotRmc = true;

                        if (rmc.LatitudeDegrees.HasValue && rmc.LongitudeDegrees.HasValue)
                        {
                            Console.WriteLine($"Your location: {rmc.LatitudeDegrees.Value:0.00000}, {rmc.LongitudeDegrees.Value:0.00000}");
                        }
                        else
                        {
                            Console.WriteLine($"You cannot be located.");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Sentence of type `{typed.GetType().FullName}` not handled.");
                    }
                }
            }
        }
    }
}
```

## References 

- https://www.u-blox.com/sites/default/files/NEO-M8_DataSheet_%28UBX-13003366%29.pdf
