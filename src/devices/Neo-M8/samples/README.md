# NEO-M8 sample

## Notes

Serial port name will need to be adjusted for this to work correctly.
You can find list of your ports using `System.IO.Ports.SerialPort.GetPortNames()` in `System.IO.Ports` package.
Serial port in the sample below is a default UART on Raspberry PI.

Sample might print several messages that identifier is not known, those can be safely ignored but you might be missing out on some of the information your device is providing.
Ideally in the future all of those sentences should be implemented but note that total number of all possible sentences is very large.
Please refer to [NMEA0183 description](../../Nmea0183/README.md) on how to add support for new sentences.

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