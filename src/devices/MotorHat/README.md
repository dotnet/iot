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

## Resource management (disposing)

The `MotorHat` owns all the resources it creates. When you call `CreateDCMotor`, `CreateServoMotor` or `CreatePwmChannel`, the returned object uses PWM channels that belong to the `MotorHat`'s underlying PCA9685 controller.

Because of this ownership model:

- **Disposing the `MotorHat` is enough.** `MotorHat.Dispose()` stops every channel it handed out and then disposes the underlying PCA9685 (and its I2C device). You do not need to dispose the motors, servos or PWM channels separately.
- **Disposing a motor as well is safe.** Disposing a `DCMotor` created by `CreateDCMotor` only stops its PWM channels; it does not dispose them. Combined with the point above, disposing both the motor and the `MotorHat` will not throw or leave the board in a random state.
- **Order does not matter.** You can dispose the motors before or after the `MotorHat`; the result is the same.

The recommended pattern is to keep the objects you need alive for as long as the `MotorHat` and let a single `using` (or `Dispose`) on the `MotorHat` clean everything up:

```csharp
using (var motorHat = new MotorHat())
{
    var motor = motorHat.CreateDCMotor(1);

    motor.Speed = 1;

    // ... use the motor ...

    // No need to dispose 'motor' explicitly; disposing 'motorHat' releases it.
}
```

If you wrap the `MotorHat` in your own class, forward disposal to the `MotorHat`. Disposing the individual motors as well is harmless but not required:

```csharp
public class PumpController : IDisposable
{
    private MotorHat _motorHat = new MotorHat();
    private List<DCMotor> _motors = new List<DCMotor>();

    public void Initialize()
    {
        _motors.Add(_motorHat.CreateDCMotor(1));
    }

    // ... use the motors ...

    public void Dispose()
    {
        // Disposing the MotorHat stops and releases every motor/channel it created.
        // Order is not important and disposing the motors as well would be safe.
        _motorHat.Dispose();
    }
}
```

## Support

- Up to 4 DC Motors
- And Up to 4 PWM Xtra channels OR up to 4 [ServoMotors](../ServoMotor/README.md)
