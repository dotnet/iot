# Using Fsr408 (Force sensitive resistor)

## Summary
Force sensitive resistors change its resistivity depening on how much its pressed. This feature allows you to detect physical pressure, squeezing and weight. This sample demonstrates use of FSR Interlink 402 model, other types of FSR sensors usage will be pretty identical.


**[FSR]**: [https://cdn-learn.adafruit.com/assets/assets/000/010/126/original/fsrguide.pdf]

## Binding Notes

As FSR generates analog signal depending on pressure, for controllers not having analog input we will need to use ADC converter
You can use ADC device for reading in your project to access analog devices. [Reading Analog Input from a Potentiometer](samples/README.md) demonstrates a concrete example using this class.




## References 
The sample is based on following resources:

* [Reading Analog Input from a Potentiometer](https://github.com/dotnet/iot/tree/master/src/devices/Mcp3008/samples) 
* [Using an FSR](https://learn.adafruit.com/force-sensitive-resistor-fsr/using-an-fsr)
* [Basic Resistor Sensor Reading on Raspberry Pi](https://learn.adafruit.com/basic-resistor-sensor-reading-on-raspberry-pi)

