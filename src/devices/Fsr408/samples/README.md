# Using Fsr 408 (Force sensitive resistor)

Force sensitive resistors change its resistivity depending on how much it is pressed. This feature allows you to detect physical pressure, squeezing and weight. This sample demonstrates use of FSR Interlink 402 model, other types of FSR sensors usage will be pretty identical.

**[FSR]**: [https://cdn-learn.adafruit.com/assets/assets/000/010/126/original/fsrguide.pdf]

## Detecting pressure/squeezing with Fsr408, Mcp3008 and Rasp Pi 

As FSR generates analog signal depending on pressure for controllers not having analog input you can use ADC converter or can use collecting capacitor and measure its fill up time. Below example used MCP3008 analog to digital converter for measuring analog input. And calculating voltage, resistance and pressure force approxinmately. You will need to create Mcp3008 depending on how you connected it to the controller, [please refer this for more about binding MCP3008](https://github.com/dotnet/iot/tree/master/src/devices/Mcp3008/samples)

You can use the following code to [Detecting pressure/squeezing with Fsr408 and ADC converter MCP3008](Fsr408.Sample.cs#L31-L45):

```csharp
    fsr408.AdcConverter = new Mcp3008.Mcp3008(18, 23, 24, 25);

    fsr408.PowerSupplied = 3300; // should set in milli volts, default is 5000 (5V)
    fsr408.Resistance = 10000;   // set the pull down resistor resistance, default is 10000 (10k ohm)

    Console.WriteLine("Reading from Mcp");
    while (true)
    {
        int value = fsr408.ReadFromMcp3008();
        int voltage = fsr408.CalculateVoltage(value);
        int resistance = fsr408.CalculateFsrResistance(voltage);
        int force = fsr408.CalculateForce(resistance);
        Console.WriteLine($"Read value: {value}, voltage: {voltage}, resistance: {resistance}, approximate force in Newtons: {force}");
        Thread.Sleep(500);
    }
```
		
![Fsr408 with Mcp3008 and Raspberry Pi diagram](samples/Fsr408_Mcp3008_RaspPi.png)
		
Also you can read voltage, resistance and pressure force directly by calling fsr408.ReadVotlageUsingMcp3008(), fsr408.ReadFsrResistanceUsingMcp3008() and fsr408.ReadPressureForceUsingMcp3008() methods respectively

## Hardware elements

The following elements are used in this sample:

* [Force Sensitive Resistor](https://www.adafruit.com/product/166)
* [MCP3008](https://www.adafruit.com/product/856)
* [Pull down Resistor 10 kOhm](https://www.adafruit.com/product/2784)


## Detecting touch/squeezing with Fsr408, capacitor and Rasp Pi 

Using capasitor for reading analog input was producing kind of noisy signal, so from my experience if you only need to check/determine if FSR is pressed use of capacitor could work very well, but if you need more fine tuned measurement better use Analog to Digital Converter. References saying use capacitors up to 1 micro Farade, i tried with 0.1 micro farade and 1 micro farade, the less capacitor the more sensitivity i got. As we are counting duration capacitor got charged the more value counted means the less FSR pressed. And counting threshold is 3000 which basically shows FSR not pressed.

You can use the following code to [Detect if Force Sensitive Resistore is pressed - using capacitor](Fsr408.Sample.cs#L53-L65):

```csharp
    while (true)
    {
        int value = fsr408.ReadCapacitorChargingDuration();
        if (value == 30000)
        {   
            Console.WriteLine("Not pressed");
        }
        else
        {
            Console.WriteLine($"Read {value}");
        }
        Thread.Sleep(500);
    }
```

![Fsr408 with Capacitor and Raspberry Pi diagram](samples/Fsr408_Capacitor_RaspPi.png)

Here we connected input to pin 18, you can change that at your will and set new pin number using constractor argument ```Fsr408(int pinNumber)```.

## Hardware elements

The following elements are used in this sample:

* [Force Sensitive Resistor](https://www.adafruit.com/product/166)
* [Capacitor 0.1 micro Farade]

## References 
The sample is based on following resources:

* [Reading Analog Input from a Potentiometer](https://github.com/dotnet/iot/tree/master/src/devices/Mcp3008/samples) 
* [Using an FSR](https://learn.adafruit.com/force-sensitive-resistor-fsr/using-an-fsr)
* [Basic Resistor Sensor Reading on Raspberry Pi](https://learn.adafruit.com/basic-resistor-sensor-reading-on-raspberry-pi)
