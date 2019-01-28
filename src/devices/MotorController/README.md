# DC Motor Controller

## Summary

This is a generic class to control any DC motor.

DC motors are controlled by simply providing voltage on inputs (inverted voltage inverts the direction).

DC motors can be controlled with 1 or 2 pins:
- When 1 pin is used the speed is controlled with PWM, second pin is connected to the ground
- When 2 pins are used 1 of the pins is controlled with PWM and the second controlls the direction (PWM signal has to be inverted when direction is inverted)

It is not recommended to connect any motors directly to the controller and to use H-Bridge instead (i.e. L298N)

## Sample

```csharp
using (var motor = new DCMotorController(23, 24))
{
    while (true)
    {
        double t = timer.Elapsed.TotalSeconds;
        double speed = Math.Sin(2.0 * Math.PI * t / 5.0);
        motor.Speed = speed;
        Thread.Sleep(200);
        Console.WriteLine($"speed = {speed}");
    }
}
```
