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
    Console.WriteLine("mV:" + uFireIse.ReadElectricPotential().Millivolts);
}
```

### Orp 
To read the ORP (OxidationReductionPotential) value use this example
   
```csharp
using (UFireOrp uFireOrp = new UFireOrp(device))
{
	if (uFireOrp.TryMeasureOxidationReductionPotential(out ElectricPotential orp))
	{
	    Console.WriteLine("Eh:" + orp.Millivolts);
	}
	else
	{
		Console.WriteLine("Not possible to measure pH");
	}
}
```

### Calibration 
Calibration of the probe using a single solution. 
Put the probe in a solution where the pH (Power of Hydrogen) value is known (in this example we assume it is 7).
The calibration are saved in μFire ISE Probe Interface, until you call ResetCalibration.
It is possible to run without calibration.
   
```csharp
 using (UFirePh uFire_pH = new UFirePh(device))
{
    uFire_pH.CalibrateSingle(7);
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

