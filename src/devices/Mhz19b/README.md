# MH-Z19B CO2-Sensor

## Summary
Binding for the MH-Z19B NDIR infrared gas module. The gas module measures the CO2 gas concentration in the ambient air.

## Binding Notes
The MH-Z19B gas module provides a serial communication interface (UART) which can be directly wired to a Raspberry PI board. The module is supplied with 5V. The UART level is at 3.3V and no level shifter is required.

|Function|	Raspi pin|	MH-Z19 pin|
|--------|-----------|------------|
|Vcc +5V |2 (+5V)	 |6 (Vin)     |
|GND	 |6 (GND)	 |7 (GND)     |
|UART	 |8 (TXD0)	 |2 (RXD)     |
|UART	 |10 (RXD0)	 |3 (TXD)     |
Table: MH-Z19B to RPi 3 connection

<br/>
The binding supports the connection through an UART interface (e.g. ```/dev/serial0```) or (serial port) stream.
When using the UART interface the binding instantiates the port with the required UART settings and opens it.
The use of an existing stream adds flexibility to the actual interface that used with the binding.
In either case the binding supports all commands of the module.
<br/><br/>

**Make sure that you read the datasheet carefully before altering the default calibration behaviour. 
Automatic baseline correction is enabled by default.**

## References 
[MH-Z19b Datasheet](https://www.winsen-sensor.com/d/files/infrared-gas-sensor/mh-z19b-co2-ver1_0.pdf)
