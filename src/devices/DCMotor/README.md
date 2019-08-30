# DC Motor Controller

## Summary

This is a generic class to control any DC motor.

DC motors are controlled by simply providing voltage on the inputs (inverted voltage inverts the direction).

DC motors can be controlled with 1, 2 or 3 pins.
Please refer to the [sample](samples/README.md) to see how to connect it.

**Never connect DC motor directly to your board, instead use i.e. H-bridge**

## 3- vs 1/2-pin mode

2/1-pin mode should be used only if H-bridge allows the inputs to be changed frequently
otherwise excessive heat or damage may occur which may reduce life-time of the H-bridge.
It may also cause increased energy consumption due to energy being converted into heat.