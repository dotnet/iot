# Bh1750

## Summary
Bh1750 is a digital Ambient Light Sensor IC for the I2c bus interface. Its main pourpose is to detect ambient light data, and it is able to detect wide ranges of light when measuring at high resolution.

## Data Sheet

**BH1750FVI**: [DataSheet](http://www.elechouse.com/elechouse/images/product/Digital%20light%20Sensor/bh1750fvi-e.pdf)

## Binding Notes

The `ReadLight()` method will use High Resolution when performing the measurement. By looking at the datasheet, there are other types of measurements that are supported but they aren't includded in this binding for now.
