# NEO-M8

## Summary

u-blox M8 is Global Navigation Satellite System (GNSS).
It is using NMEA0183 protocol for communication.

## Binding Notes

This device uses UART and therefore regular PC with RS232 to TTL converter can be used (i.e. Raspberry PI is not required).
When using Raspberry PI use `raspi-config` to disable login shell and enable serial port (interfacing options).

### Communication methods

NEO-M8 supports multiple communication methods but only UART is currently supported:

- [X] UART
- [ ] USB
- [ ] SPI
- [ ] DDC (I2C compliant)

### Supported sentences

Current support for sentences is limited to only `RMC` (NMEA0183's Recommended Minimum Navigation Information). Please refer to [sentence directory](../Nmea0183/Sentences) for latest information what is currently supported.

Full list of supported sentences varies by device.

## References 

- https://www.u-blox.com/sites/default/files/NEO-M8_DataSheet_%28UBX-13003366%29.pdf
