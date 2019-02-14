# 28BYJ-48 Stepper Motor 5V 4-Phase 5-Wire & ULN2003 Driver Board

## Summary
A stepper motor is an electromechanical device which converts electrical pulses into discrete mechanical movements. The shaft or spindle of a stepper motor rotates in discrete step increments when electrical command pulses are applied to it in the proper sequence. The motors rotation has several direct relationships to these applied input pulses. The sequence of the applied pulses is directly related to the direction of motor shafts rotation. The speed of the motor shafts rotation is directly related to the frequency of the input pulses and the length of rotation is directly related to the number of input pulses applied.One of the most significant advantages of a stepper motor is its ability to be accurately controlled in an open loop system. Open loop control means no feedback information about position is needed. This type of control eliminates the need for expensive sensing and feedback devices such as optical encoders. Your position is known simply by keeping track of the input step pulses.

## Device Family

The 28BYJ-48 is a small stepper motor suitable for a large range of applications. More information [here](https://components101.com/motors/28byj-48-stepper-motor)

![](SM28BYJ48.png)

**[Stepper Motor 28BYJ-48]**: http://www.geeetech.com/Documents/Stepper%20motor%20datasheet.pdf

**[ULN2003]**: http://www.geeetech.com/Documents/ULN2003%20datasheet.pdf

## Usage
```C#
// Pinout for Raspberry Pi 3
const int bluePin = 4;
const int pinkPin = 17;
const int yellowPin = 27;
const int orangePin = 22;

using (StepperMotor motor = new StepperMotor(bluePin, pinkPin, yellowPin, orangePin))
{
  // The motor turns one direction for postive 2048 and the reverse direction for negative 2048 (180 degrees).
  while (true)
  {
    motor.Step(2048);
    motor.Step(-2048);
  } 
}
```
