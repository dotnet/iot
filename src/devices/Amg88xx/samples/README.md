# Demonstration of AMG8xx binding usage

## Overview
The sample application demonstrates the key functions of the sensor and the binding
* thermal image readout
* interrupt triggering based on temperature levels incl. hysteresis
* sensor states
* noise reduction by using the sensor's moving average function

There are AMG88xx breakout boards available from a variety of vendors. You can use any of them as long as it provides access to the I2C interface of the sensor.
<br/><br/>
**Note:** There are also boards available with additional interfaces or even with an integrated Arduino or compatible circuit. You can use this binding only if the boards gives you access to the I2C interface only.
<br/><br/>

## Wiring
For demonstration purpose the INT-pin of the sensor is connected to GPIO PIN 5 of the RPi. Additionally an LED is connected to GPIO PIN 6 of the RPi. The LED signals the occurrence of an interrupt.

![Wiring of a sensor breakout and LED for the sample](./amg8833sample.png)



