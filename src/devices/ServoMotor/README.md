# Servo Motor

## Summary

This is a generic binding to control many servo motors using a hardware or software `PwmChannel`.  Servo motors are usually based on a signal frequency of 50Hz.  They also require a minimum/maximum pulse width to determine the position.  The pulse width is generally between 1 and 2 milliseconds, where 1ms is approximately 0 degrees, 1.5ms is the rest position, and 2ms is 180 degrees.  This information can be found in each servo motor's datasheet.  

One thing to be aware of is the wiring as the servo motor connector is usually a 3-pin connector.  The pinout is shown below where colors can vary.

| Pin Number | Signal         | Color                     |
|------------|----------------|---------------------------|
| 1          | Ground         | Black or Brown            |
| 2          | Power Supply   | Brown or Red              |
| 3          | Control Signal | Orange or White or Yellow |

## Device Family

There are many servo motor sizes available that offer both standard and continuous rotation.  Below are a few links where to purchase servo motors.

[Adafruit Servo Motor Accessories](https://www.adafruit.com/?q=servo)  
[Sparkfun Servo Motor Accessories](https://www.sparkfun.com/categories/245)  

## Binding Notes

The `ServoMotor` binding offers an easy way to begin controlling a servo motor.  The quickest approach is to provide the `ServoMotor` object a `PwmChannel` using the default values for other optional arguments.

```csharp
// Example of hardware PWM using chip 0 and channel 0 on a dev board.
ServoMotor servoMotor = new ServoMotor(PwmChannel.Create(0, 0, 50));
servoMotor.Start();  // Enable control signal.

// Move position.  Pulse width argument is in microseconds.
servoMotor.WritePulseWidth(1000); // 1ms; Approximately 0 degrees.
servoMotor.WritePulseWidth(1500); // 1.5ms; Approximately 90 degrees.
servoMotor.WritePulseWidth(2000); // 2ms; Approximately 180 degrees.

servoMotor.Stop(); // Disable control signal.
```

The position of servo motor can also be adjusted by the angle.  The `ServoMotor` constructor's optional arguments must be set according to device's specs.  NOTE: These are usually an approximation, so you may need to manually tweak to determine exact values.

For example, the [Hitec HS-300BB](https://servodatabase.com/servo/hitec/hs-300bb) servo has the following specifications:
- MaximumAngle = 180
- MinimumPulseWidthMicroseconds = 900
- MaximumPulseWidthMicroseconds = 2100
- Frequency 50Hz; Period 20000uS

```csharp
// Example of hardware PWM using chip 0 and channel 0 on a dev board.
ServoMotor servoMotor = new ServoMotor(
    PwmChannel.Create(0, 0, 50),
    180,
    900,
    2100);

servoMotor.Start();  // Enable control signal.

// Move position.
servoMotor.WriteAngle(0); // ~0.9ms; Approximately 0 degrees.
servoMotor.WritePulseWidth(90); // ~1.5ms; Approximately 90 degrees.
servoMotor.WritePulseWidth(180); // ~2.1ms; Approximately 180 degrees.

servoMotor.Stop(); // Disable control signal.
```

## Calibration

Calibration or finding minimum and maximum pulse width and angle range `WritePulseWidth` method should be used.
To make it easier to write applications which allow calibration method `Calibrate` can be used to change calibration parameters.

You can refer to [servo sample](../Pca9685/samples/Pca9685.Sample.cs) for example usage (i.e. `CalibrateServo` utility).

## References

[Wikipedia Servo Motor](https://en.wikipedia.org/wiki/Servomotor)  
[Hobby Servo Tutorial](https://learn.sparkfun.com/tutorials/hobby-servo-tutorial/all)  
[How Servo Motors Work & How To Control Servos Using Arduino](https://howtomechatronics.com/how-it-works/how-servo-motors-work-how-to-control-servos-using-arduino/)  
[Arduino Servo Library](https://www.arduino.cc/en/Reference/Servo)  
[Raspberry Pi Lesson 28: Controlling a Servo on Raspberry Pi with Python](http://www.toptechboy.com/raspberry-pi/raspberry-pi-lesson-28-controlling-a-servo-on-raspberry-pi-with-python/)  
