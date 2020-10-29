# Mettler Toledo

## Summary
This binding provides a higher-level interface for interacting with [Mettler Toledo](https://www.mt.com)-brand scales.

## Device Family
This binding should work with any MT-SICS-compatible scale that uses MT-SICS Level 0. Some Level 1 commands are also available.

## Binding Notes
This binding operates over the serial protocol with any MT-SICS-compatible device. Due to it's serial protocol, this library should work on any platform that ``System.Net.Iot`` supports.

"Unit 1" refers to the default unit of the scale. All S commands are given in Unit 1, but are converted using ``Units.NET``. This term is kept to stay inline with the Reference Manual.

Here is how you would interact with a scale:
```csharp
// Create a scale object
var scale = new MettlerToledoDevice("/dev/ttyUSB1");

// Reset the scale
scale.Reset();

// Now fetch a reading, when the scale is stable. This means the scale is confident in the weight.
MettlerToledoWeightReading reading = scale.GetStableWeight();

// Print reading to console
Console.WriteLine($"Received a weight of {reading.Weight.Grams} grams.");
```

This binding also allows you to receive an event every time the scale detects a new weight. This event will stop being fired any time ``GetStableWeight()`` or ``GetWeightImmediately()`` is called.

```csharp
scale.WeightUpdated += (s, e) =>
{
    Console.WriteLine($"Scale reported a new weight of {reading.Weight.Grams} grams.");
};
scale.SubscribeToWeightChangeEvents();
```

## References 
**Official Reference Manual**: [MT-SICS Command Set](https://www.mt.com/mt_ext_files/Editorial/Generic/7/MT-SICS_for_Excellence_Balances_BA_Editorial-Generic_1116311007471_files/Excellence-SICS-BA-e-11780711B.pdf)
