# Example of stepper motor 28BYJ-48

## Circuit
![](SM28BYJ48.png)


On scheme is used ULN2003APG driver. Therefore, external power is needed (9V).

You can use just 5V Rapberry Pi PIN instead (Physical pin 2,4) if you use ULN2003A driver.

## Code

You can create a stepper motor with the following line:

```C#
StepperMotor motor = new StepperMotor(bluePin, pinkPin, yellowPin, orangePin)
```
In the constructor, you will need to pass the number of used PINs. Also you can pass stepper mode (HalfStep, FullStepSinglePhase, FullStepDualPhase) and revolutions per minute value.

```C#
// Pinout for Raspberry Pi 3
const int bluePin = 4;
const int pinkPin = 17;
const int yellowPin = 27;
const int orangePin = 22;

static void Main(string[] args)
{
    Console.WriteLine($"Let's go!");
    using (StepperMotor motor = new StepperMotor(bluePin, pinkPin, yellowPin, orangePin))
    {

        Console.CancelKeyPress += (object sender, ConsoleCancelEventArgs eventArgs) =>
        {
            motor.Dispose();
        };

        // The motor turns one direction for postive 2048 and the reverse direction for negative 2048 (180 degrees for 28BYJ-48).
        while (true)
        {
            motor.Step(2048);
            motor.Step(-2048);
        }
    }
}
```