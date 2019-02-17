# Sense HAT

## Summary

Sense HAT is an add-on board for Raspberry Pi.

It consists of multiple devices. Currently supported devices:
- LED matrix

## Notes

When using SysFs implementation please make sure to enable Sense HAT in `/boot/config.txt` before using by adding following line:

```
dtoverlay=rpi-sense
```

This is not required when using I2c implementation. See more details in the samples about the differences.