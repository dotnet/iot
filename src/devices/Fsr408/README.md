# Using Fsr 408 (Force sensitive resistor)

Force sensitive resistors change its resistivity depending on how much it is pressed. This feature allows you to detect physical pressure, squeezing and weight. This sample demonstrates use of FSR Interlink 402 model, other types of FSR sensors usage will be pretty identical.


**[FSR]**: [https://cdn-learn.adafruit.com/assets/assets/000/010/126/original/fsrguide.pdf]

## Binding Notes

As FSR generates analog signal depending on pressure for controllers not having analog input you can use ADC converter or can use collecting capacitor and measure its fill up time. From my experience if you only need to check/determine if FSR is pressed use of capacitor could work well, but if you need more fine tuned measurement better use Analog to Digital Converter.


![Fsr408 with Mcp3008, Raspberry Pi and Breadboard diagram](samples/Fsr408_Mcp3008_RaspPi.png)
