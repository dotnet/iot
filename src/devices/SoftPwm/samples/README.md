# Led PWM with SoftPwm

## Schematic

This example shows how to use the software PWM with a Led. Simply connect the Led with a 100 ohms resistor on GPIO 17 (physical pin 11).

![schema](./pwmled.png)

## Code

To initialize the software with frequency of 200 on pin 17 and duty cycle of 50% (optional, duty cycle is value from 0.0 to 1.0 where 0.0 is 0% and 1.0 is 100%), you need to add ```using System.Device.Pwm``` and use following code:

```csharp
var channel = new SoftwarePwmChannel(17, 200, 0.5);
channel.Start();        
```

Then you can change the duty cycle during the execution (75% used in the example below):

```csharp
channel.DutyCyclePercentage = 0.75;
```

Here is a full example:

```csharp
using System;
using System.Device.Pwm.Drivers;
using System.Threading;

class Program
{

    static void Main(string[] args)
    {
        Console.WriteLine("Hello PWM!");

        using (var pwmChannel = new SoftwarePwmChannel(17, 200, 0))
        {
            pwmChannel.Start();
            for (double fill = 0.0; fill <= 1.0; fill += 0.01)
            {
                pwmChannel.DutyCyclePercentage = fill;
                Thread.Sleep(500);
            }
        }
    }
}

```

## Other Example 

You will find another example of SoftPwm in the [Servo Motor class](/src/devices/Servo/samples/README.md). This Servomotor sample uses a precision timer.
