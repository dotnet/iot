# μFire ISE Probe Interface - I2C Driver

## Summary

The μFire ISE Probe Interface is an I²C sensor that can read a pH probe. Attach a waterproof temperature sensor for temperature compensation with the attached connector.

## Usage

You can find an example in the [sample](./samples/uFireIse.Sample.cs) directory. 


### Basic
   It is possible to read the basic value (Electric Potential) from the probe.
   
```csharp
using (UFireIse uFireIse = new UFireIse(device))
	{
		Console.WriteLine("mV:" + uFireIse.Measure().Millivolts);
	}
```

### Orp 
   To read the ORP (OxidationReducationPotential) value use this example
   
```csharp
using (UFireOrp uFireOrp = new UFireOrp(device))
	{
		if (uFireOrp.TryMeasureOxidationReducationPotential(out ElectricPotential orp))
		{
			Console.WriteLine("Eh:" + orp.Millivolts);
		}
		else
		{
			Console.WriteLine("Not possible to measure pH");
		}
	}
```

### Ph 
   To read the Ph (Power of Hydrogen) value use this example
   
```csharp
using (UFirePh uFire_pH = new UFirePh(device))
	{
		Console.WriteLine("mV:" + uFire_pH.Measure().Millivolts);

		if (uFire_pH.TryMeasurepH(out float pH))
		{
			Console.WriteLine("pH:" + pH);
			Console.WriteLine("pOH:" + uFire_pH.Poh);
		}
		else
		{
			Console.WriteLine("Not possible to measure pH");
		}
	}
```

## Data Sheets from uFire

https://www.ufire.co/docs/uFireIse/#connections

## References 

https://www.ufire.co/docs/uFireIse/api.html#class-functions

