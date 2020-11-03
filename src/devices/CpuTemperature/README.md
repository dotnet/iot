# Cpu Temperature

Device bindings for the CPU Temperature Sensor

## Summary

Returns the current temperature of the CPU Temperature Sensor. Useful telemetry in its own right, but also useful for calibrating the Raspberry Pi Sense HAT.

## Binding Notes

On Windows, this tries to use the OpenHardwareMonitor binding (see there for details). 
If it is not available, some guesswork is done to get a temperature sensor. However, the temperature returned by this binding may not be the actual CPU temperature, but one of the mainboard sensors instead. Therefore, depending on the mainboard, no data may be available. Unless OpenHardwareMonitor can be used, elevated permissions ("Admin rights") are required.

## References
