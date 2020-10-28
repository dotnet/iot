# OpenHardwareMonitor

Client binding for OpenHardwareMonitor.

## Summary

Returns a set of sensor measurements for the current hardware. The values include CPU temperature(s), Fan Speed(s), GPU Temperature(s) and Hard Disk states. The set of values is hardware dependent.

## Binding Notes

This binding works on Windows only. It requires that OpenHardwareMonitor (https://openhardwaremonitor.org/) is running in the background. While that tool requires elevated permissions to work, 
the binding (and the application using it) does not. 

The binding supports some additional, "virtual" sensor measuments that are derived from other values. The following extra values are provided:

- For each sensor returning power, another is generated which calculates energy consumed (by integrating the values over time).
- From the CPU Package power sensor, the CPU heat flux is calculated (estimating the size of the die).
- If both a power and a voltage sensor are available for CPU Package, the current is calculated.

## References
