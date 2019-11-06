# MotorHat

## Summary

Motor HAT is an add-on board for Raspberry Pi.

It consists of a pca9685 PWM driver and two motor controller chips, that together support up to 4 DC motors, or 2 Stepper motors.
It also provides 4 extra PWM Outputs, that can be used for anything that requires PWM, (controlling a LED, a ServoMotor, etc)

You can find in depth documentation on the hat in [Adafruit website](https://www.adafruit.com/product/2348)
Or you can also get it at [Aliexpress](http://s.click.aliexpress.com/e/mTB4ZB2s)

###Currently supported devices:
- Up to 4 DC Motors
- Up to 4 PWM Xtra channels
- Up to 4 [ServoMotors](https://github.com/dotnet/iot/blob/master/src/devices/ServoMotor/README.md)

###Not yet supported devices:
- Up to 2 Stepper Motors


# Notes

##DC Motors
   The following example show how to create a DCMotor.

```C#   
using (var motorHat = new MotorHat())
{
    var motor = motorHat.CreateDCMotor(1); // MotorNumber can be 1, 2, 3 or 4, following the labbelling in the board: M1, M2, M3 or M4

    motor.Speed = 1 // Speed goes from -1 to 1, where -1 is max backward speed, 1 is max forward speed and 0 means stopping the motor
}
```

##ServoMotor
   The following example show how to create a ServoMotor.

```C#   
using (var motorHat = new MotorHat())
{
    var servoMotor = motorHat.CreateServoMotor(0); // channelNumber can be 0, 1, 14 or 15, depending on wich of those xtra channels you connected your servo

    ...
}
```
Check the [ServoMotor documentation](https://github.com/dotnet/iot/tree/master/src/devices/ServoMotor) for examples on how to use the ServoMotor class
