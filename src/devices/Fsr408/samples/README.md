# Using Fsr408 (Force sensitive resistor)

Force sensitive resistors change its resistivity depening on how much its pressed. This feature allows you to detect physical pressure, squeezing and weight. This sample demonstrates use of FSR Interlink 402 model, other types of FSR sensors usage will be pretty identical.


**[FSR]**: [https://cdn-learn.adafruit.com/assets/assets/000/010/126/original/fsrguide.pdf]

## Binding Notes

As FSR generates analog signal depending on pressure, you will need a controller with analog input, for controllers not having analog input you can use ADC converter or even use collecting capasitor and measure its fill up time. From my experience if you need more accurate measurement you better use analog reading device. If you only need to check/measure if it is pressed or not using capacitor can work, but for measuring how much it is pressed capacitor was not accurate and had lots of noise.


## References 
The sample is based on following resources:

* [Reading Analog Input from a Potentiometer](https://github.com/dotnet/iot/tree/master/src/devices/Mcp3008/samples) 
* [Using an FSR](https://learn.adafruit.com/force-sensitive-resistor-fsr/using-an-fsr)
* [Basic Resistor Sensor Reading on Raspberry Pi](https://learn.adafruit.com/basic-resistor-sensor-reading-on-raspberry-pi)

