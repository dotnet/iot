# Cpu Temperature

Device bindings for the CPU Temperature Sensor on Linux and Windows

## Summary

Returns the current temperature of the CPU Temperature Sensor. Useful telemetry in its own right, but also useful for calibrating the Raspberry Pi Sense HAT.

## Binding Notes

On Windows, the temperature returned by this binding may not be the actual CPU temperature, but one of the mainboard sensors instead. Therefore, depending on the mainboard, no data may be available. In either case, elevated permissions ("Admin rights") are required.

## References
