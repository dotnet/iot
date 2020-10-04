# Demonstration of AMG8xx binding usage

## Introduction
There are AMG88xx breakout boards available from a variety of the known vendors. You can use nay of them as long as it provides the I2C interface of the sensor directly.
<br/><br/>
**Note:** There are also boards available with additional interfaces or even an integrated Arduino compatible curcuit. You probably can't use them as the binding supports a direct I2C connection only.

## Wiring
For demonstration purpose the INT-pin of the sensor ist connected to GPIO PIN05 of the RPi. Additionally an LED is connected to GPIO PIN06 of the RPi.

![Wiring of a sensor breakout and LED for the sample](./amg8833sample.png)

## Funtional description of the sample

