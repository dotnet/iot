# MotorHat

Motor HAT is an add-on board for Raspberry Pi.

It consists of a pca9685 PWM driver and two motor controller chips, that together support up to 4 DC motors, or 2 Stepper motors.
It also provides 4 extra PWM Outputs, that can be used for anything that requires PWM, (controlling a LED, a ServoMotor, etc)

## Documentation

- [Adafruit](https://www.adafruit.com/product/2348)
- [Aliexpress](http://s.click.aliexpress.com/e/mTB4ZB2s)
- [Waveshare](https://www.waveshare.com/wiki/Motor_Driver_HAT)

## Usage

### DC Motors

The following example show how to create a DCMotor.

```csharp
using (var motorHat = new MotorHat())
{
    var motor = motorHat.CreateDCMotor(1); // MotorNumber can be 1, 2, 3 or 4, following the labbelling in the board: M1, M2, M3 or M4

    motor.Speed = 1 // Speed goes from -1 to 1, where -1 is max backward speed, 1 is max forward speed and 0 means stopping the motor
}
```

### ServoMotor

The following example show how to create a ServoMotor.

```csharp
using (var motorHat = new MotorHat())
{
    var servoMotor = motorHat.CreateServoMotor(0); // channelNumber can be 0, 1, 14 or 15, depending on wich of those xtra channels you connected your servo

    ...
}
```

Check the [ServoMotor documentation](../ServoMotor/README.md) for examples on how to use the ServoMotor class

## Support

- Up to 4 DC Motors
- And Up to 4 PWM Xtra channels OR up to 4 [ServoMotors](../ServoMotor/README.md)
