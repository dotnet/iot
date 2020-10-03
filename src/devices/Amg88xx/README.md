# AMG88xx Infrared Array Sensor Family

## Summary
The AMG88xx family of infrared array sensors supports temperature detection on a two dimensional area with 8×8 (64) thermophile sensor pixels. The sensor is an IT camera with a viewing field angle of 60°, a 7.5° view angle per pixel and a detection distance up to 5 to 7m. The resolution is 0.25°C per pixel.
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

## Binding Notes
The binding implements the AMG88 provides a lean interface to retrieve the pixel array and to control the sensor. Any further processing, e.g. pattern recognition, is beyond the scope of the binding.
<br/>

### Sensor Status
The sensor status indicates if any pixel or the chip internal thermistor overran the upper or lower limit. It also flags on the occurence of an interrupt. The status can be read out and reset per flag:
```
public bool HasTemperatureOverflow()
public void ClearTemperatureOverflow()

public bool HasThermistorOverflow()
public void ClearThermistorOverflow()

public bool HasInterrupt()
public void ClearInterrupt()

public void ClearAllStatus()
```
<br/>

### Moving average
The sensor can be switched into a moving average mode. In this mode the sensor builds the twice moving average for each pixel. The moving average mode can be applied for both frame rates, 1 fps and 10 fps. If the sensor frame rate is set to 10 fps (default) it takes the average of the readings n and n+1 and yields their average as output. If the sensor operates at 1 fps it takes the readings of 10 frames (as the sensor runs internally always at 10 fps) and builds their average. This is done twice, resulting in a final average which is used for the pixel output.
The noise per pixel will decrease to 1/sqrt2 by using the moving average mode.

```
public bool GetMovingAverageMode()
public void SetMovingAverageMode(bool mode)
```
***Important***: the reference specification states that the current mode can be read, but this seems that is not working at the time being. The bit is always read as 0.

<br/>

### Frame Rate
The sensor supports a frame rates of 1 FPS and 10 FPS. The default value is 10 frames per second.

```
public FrameRate GetFrameRate()
public void SetFrameRate(FrameRate)
```
<br/>

### Operating Mode / Power Control
The sensor supports four operating modes for controlling power consumption:
* Normal
* Sleep Mode
* Stand-by with 60 seconds intermittence
* Stand-by with 10 seconds intermittence

*Note*: refer to the datasheet für further details

*Note*: in sleep mode reading operations are not supported, writing operations are not supported except switching back to ```normal``` mode.
<br/>

### Reset
The sensor software can be reset in two ways.
* **Initial Reset:** Resets all flags and registers to default values
* **Flag Reset:** Resets all flags (status register, interrupt flag, interrupt table)
```
public void InitialReset()
public void FlagReset()
```
<br/>

### Interrupt control, levels and pixel flags
The sensor can raise an interrupt if any pixel passes a given value. The event is signaled by the interrupt flag of the status register. Additionally the INT pin of the sensor can be pulled low.

```
public void GetInterruptModeTest(InterruptMode expectedMode, byte registerValue)
public void SetInterruptModeTest(InterruptMode mode, bool modeBitIsSet)
```
The interrupt levels can be configured using the Interrupt Level register. Both lower limit, upper limit, and the hysteresis level can be set and read. Initially the register is filled with zeroes. The levels apply to all pixels.

```
public Temperature GetInterruptLowerLevel()
public Temperature SetInterruptLowerLevel()

public Temperature GetInterruptUpperLevel()
public Temperature SetInterruptUpperLevel()

public Temperature GetInterruptHysteresisLevel()
public Temperature SetInterruptHysteresisLevel()
```
After the sensor raised an interrupt the triggering pixels can be readout from the interrupt table register.
The table can be reset by performing a ```FlagReset()```.
```
public bool[] GetInterruptFlagTable()
```
<br/>


## References
**Product Homepage**: https://industry.panasonic.eu/components/sensors/grid-eye

**Product Flyer**: https://eu.industrial.panasonic.com/sites/default/pidseu/files/downloads/files/grid-eye_flyer_english_web.pdf

**Reference Specification**: https://mediap.industry.panasonic.eu/assets/custom-upload/Components/Sensors/Industrial%20Sensors/Infrared%20Array%20Sensor%20Grid-EYE/grid_eye_reference_specifications_160205.pdf

**FAQ**: https://mediap.industry.panasonic.eu/assets/custom-upload/Components/Sensors/Industrial%20Sensors/Infrared%20Array%20Sensor%20Grid-EYE/faqs_grideye_v1.0.pdf

**Application Note**: https://mediap.industry.panasonic.eu/assets/custom-upload/Components/Sensors/Industrial%20Sensors/Infrared%20Array%20Sensor%20Grid-EYE/application_notes_grid-eye_0.pdf

