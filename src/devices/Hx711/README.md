# Hx711 - 24-bit Analog-to-Digital Converter for Weigh Scales

## Summary
The Hx711 device provides a high-accuracy interface to a load-cell. Details about the HX711 can be found [here (PDF)](https://cdn.sparkfun.com/datasheets/Sensors/ForceFlex/hx711_english.pdf).

#### Example 

``` csharp
// Read a the value from the Hx771
using (GpioController controller = new GpioController())
{
    // Instantiate a new Hx711 using pin 5 for data and pin 6 for clock
    using (Hx711 hx711 = new Hx711(controller, 5, 6))
    {
        hx711.Enable();
        
        var value = hx711.ReadValue();

        Console.WriteLine($"Read '{value}'");

        hx711.Disable();
    }
}
```

## References
https://cdn.sparkfun.com/datasheets/Sensors/ForceFlex/hx711_english.pdf
