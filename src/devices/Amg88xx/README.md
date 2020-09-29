# AMG88xx Infrared Array Sensor Family

## Summary
The AMG88xx family of high precision infrared array sensors supports temperature detection on a two dimensional area with 8×8 (64) thermophile sensor pixels. The sensor is an IT camera with a viewing field angle of 60°, a 7.5° view angle per pixel and a detection distance up to 5 to 7m. 
The resolution is 0.25°C per pixel.
The manufacturer (Panasonic) names the following applications:  Home appliances (Microwaves and air-conditioners), Building automation (People counting, Air conditioning control), Home automation (People detection), Factory automation (Fault prevention). The sensor delivers a heat image through its digital interface (I2C) at a rate of 1 or 10 frames per second.
<br/><br/>
![Illustration of thermophile array and heat map](./amg88xx.png)
<br/><br/>


## Device Family
**AMG88**: https://industrial.panasonic.com/cdbs/www-data/pdf/ADI8000/ADI8000COL13.pdf

The family consists of 4 members:
|Type   |Resolution|Gain |Vcc|Obj. Temp. Range|Resolution|
|-------|----------|-----|---|----------------|----------|
|AMG8833| 8x8      | High|3V3|0-80°C          |0.25°C    |
|AMG8834| 8x8      | Low |3V3|-20-100°C       |0.25°C    |
|AMG8853| 8x8      | High|5V0|0-80°C          |0.25°C    |
|AMG8854| 8x8      | Low |5V0|-20-100°C       |0.25°C    |

<br/>
The sensor is also equiped with an on-chip thermistor which can be read out, too.
The thermistor has a measurement range of -20...80°C at a resolution of 0.0625°C.
<br/><br/>
The sensor supports three operating modes:

* *Normal mode* - Normal operation mode
* *Stand-by mode* - 1 frame detection every 10sec or 60sec. Energy saving.
* *Sleep mode* - Detection is off and no data can be read. Maximum energy saving.
<br/><br>

## Binding Notes
The binding implements the AMG88 communication protocol and provides a lean interface to retrieve the pixel array. Any further processing, e.g. pattern recognition, is beyond the scope of the binding. 

<br/>

## References 
**Product Homepage**: https://industry.panasonic.eu/components/sensors/grid-eye

**Product Flyer**: https://eu.industrial.panasonic.com/sites/default/pidseu/files/downloads/files/grid-eye_flyer_english_web.pdf

**Reference Specification**: https://mediap.industry.panasonic.eu/assets/custom-upload/Components/Sensors/Industrial%20Sensors/Infrared%20Array%20Sensor%20Grid-EYE/grid_eye_reference_specifications_160205.pdf

**FAQ**: https://mediap.industry.panasonic.eu/assets/custom-upload/Components/Sensors/Industrial%20Sensors/Infrared%20Array%20Sensor%20Grid-EYE/faqs_grideye_v1.0.pdf

**Application Note**: https://mediap.industry.panasonic.eu/assets/custom-upload/Components/Sensors/Industrial%20Sensors/Infrared%20Array%20Sensor%20Grid-EYE/application_notes_grid-eye_0.pdf

