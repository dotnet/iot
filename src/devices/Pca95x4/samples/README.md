# Pca95x4 Samples

## Write value to LEDs

This example shows how to write value out to 8 LEDs using a PCA95x4 device and a RPi3.

1. Set Configuration register to all outputs (0x00).
2. Write value to Output register.  A LOW value will turn LEDs ON in this example.

![](Pca95x4_I2c_WriteLeds.png)