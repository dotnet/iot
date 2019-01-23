# Servomotor

## Summary

This is a generic class to control any Servomotor. Servomotor are controlled with a PWM with 50Hz frequency. The pulse width determine the position of the Servo Motor.

All servomotors have specific minimum and maximum pulse frequency for their operation. This can be found in the servomotor documentation. It is generally between 1 and 2 milliseconds. The ```ServoMotorDefinition``` class provides a way to pass those specific settings to the ```ServoMotor``` class. Servomotors need PWM to be piloted, so there is a dependency on both software and hardware PWM.

## Device Family

**Servomotor**: this family is broadly used in various application. More information on [wikipedia](https://en.wikipedia.org/wiki/Servomotor).

## Binding Notes

The ```ServoMotor``` class can use either software either hardware PWM. this is done fully transparently by the initialization.

If you want to use the software PWM, you have to specify the GPIO pin you want to use as the first parameter in the constructor. Use the value -1 for the second one. This will force usage of the software PWM as it is not a valid value for hardware PWM.

To use the hardware PWM, make sure you reference correctly the chip and channel you want to use. The ```ServoMotor``` class will always try first to open a hardware PWM then a software PWM. 

```csharp
// example of software PWM piloted Servo
ServoMotor servoSoft = new ServoMotor(21, -1, new ServoMotorDefinition(540, 2470));
// example of hardware PWM piloted Servo
ServoMotor servoHard = new ServoMotor(0, 0, new ServoMotorDefinition(540, 2470));
```

### Servomotor Definition

The ```ServoMotorDefinition``` is needed to pass the specific Servomotor settings.

- ```MinimumDuration```: minimum pulse duration expressed microseconds for the servomotor.
- ```MaximumDuration```: maximum pulse duration expressed microseconds for the servomotor.
- ```Period```: Period length expressed microseconds. By default, for servomotors, this is 20000 which is 50Hz. It is not recommended to change it but for specific application and specific servomotors, you may need to adjust it.
- ```MaximumAngle```: maximum angle for the servomotor in °, default is 360°. 0° will always be the minimum angle and will correspond to the minimum pulse. MaximumAngle will always correspond to the maximum pulse.

Those settings are known for each servomotor.
For example, the [Hitec HS-300BB](https://servodatabase.com/servo/hitec/hs-300bb) servo has the following specifications:
- MinimumDuration = 900
- MaximumDuration = 2100
- Period = 20000
- MaximumAngle = 180

Tip: setting a maximum angle to 100 will act like a percentage. Setting the ```Angle``` property to 50 will rotate the servo motor half of his capacity.

### Moving a Servomotor

To move a servomotor, you have to adjust its angle. In the below example, you will turn it physically by 120° assuming you've created it with a maximum angle of 360°.

```csharp
servo.Angle = 120;
```

Notes: You should adjust the maximum angle based on the specification of your servomotor. Most servomotors can only turn by 180° maximum. This will better reflect the reality.

The second option to move the servo motor is to directly setup a pulse width:

```csharp
servo.SetPulse(2000);
```

This will turn the servo motor using a 2 milliseconds pulse.

## Dependencies

This class has a dependency on ```SoftPwm```. Please make user you add this software PWM in your project.

