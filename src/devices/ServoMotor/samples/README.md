# Servo Motor Sample
Connect the servo motor to the dev board with ground, power and control signal.  Be aware servo motors consume lots of current. You may need to power the servo motor to an external power source to prevent damage to the dev board.

**NOTE**: The following image shows the servo is connected to a regular GPIO and not a hardware PWM pin.

![schema](./servomotor.png)

## Sample Code

The sample code provides helpful examples where you can control the servo motor's position by pulse width or angle.

### Writing Pulse Width
This example allows you to manually view the full left and right positions based on the pulse width signal entered.  Many times, servo motors do not match the exact range based on datasheets and/or have to be calibrated using the screw on side of servo motor.

### Writing Angle
Once you determine the pulse width range using the example above, you can pass the values into the `ServoMotor` constructor's optional arguments that allow the position to move based on provided angle.

## Performance Issues

It is recommended to use hardware PWM channels instead of the software implementation as there may be timing issues caused by limited resources on the dev board.