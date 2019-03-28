# DC Motor Controller

## Summary

This is a generic class to control any DC motor.

DC motors are controlled by simply providing voltage on the inputs (inverted voltage inverts the direction).

DC motors can be controlled with 1, 2 or 3 pins.
Please refer to the [sample](samples/README.md) to see how to connect it.

**Recommended setup is 3-pin mode, 1 and 2 pin mode may cause excessive heat or damage your H-bridge (larger engines are more likely to damage).**
**Never connect DC motor directly to your board**

By default 3 pin mode will be used. To turn on 1 or 2 pin mode set `UseEnableAsPwm = false` in the DCMotorSettings.
This means you won't connect anything to enable pin on the H-bridge.
