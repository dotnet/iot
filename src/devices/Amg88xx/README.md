# AMG88xx Infrared Array Sensor Family

## Summary
The AMG88xx family of infrared array sensors has 64 thermophile pixels arranged in an 8×8 matrix. The sensor works as a thermal infrared camera. It can detect objects (e.g. human bodies) from a distance of up 5-7m. A pixel can measure object temperatures in a range of 0 to 80°C / -20 to 100°C with a resolution of 0.25°C and an accuracy of ±2.5°C / ±4.5°C. The sensor has a view field angle of 60° and a 7.5° view angle per pixel.

The manufacturer (Panasonic) names the following applications: home appliances (microwaves and air-conditioners), building automation (people counting, air conditioning control), home automation (people detection), factory automation (fault prevention). The sensor delivers a heat image through its digital interface (I2C) at a rate of 1 or 10 frames per second.
Additionally an interrupt pin can raise an event when any individual pixel goes above or below a configured threshold.
<br/><br/>
![Illustration of thermophile pixel array and heat map](./amg88xx.png)
*Illustration of thermophile pixel array and heat map*
<br/><br/>


## Device Family
**AMG88**: https://industrial.panasonic.com/cdbs/www-data/pdf/ADI8000/ADI8000COL13.pdf

The family consists of 4 members:
|Type   |Resolution|Gain |Vcc|Obj. Temp. Range|Resolution|Accuracy|
|-------|----------|-----|---|----------------|----------|--------|
|AMG8833| 8x8      | High|3V3|0-80°C          |0.25°C    |±2.5°C  |
|AMG8834| 8x8      | Low |3V3|-20-100°C       |0.25°C    |±4.5°C  |
|AMG8853| 8x8      | High|5V0|0-80°C          |0.25°C    |±2.5°C  |
|AMG8854| 8x8      | Low |5V0|-20-100°C       |0.25°C    |±4.5°C  |

<br/>
The sensor is equipped with an on-chip thermistor which can be read out.
The thermistor has a measurement range of -20...80°C at a resolution of 0.0625°C.
<br/><br/>

## Binding Notes
The Amg88xx binding provides a lean interface to retrieve the pixel array and to control the sensor. All sensor functions are covered. Any further processing, e.g. pattern recognition, is beyond the scope of the binding.
<br/><br/>

### Thermal image / Pixel array
The temperature readings of the pixel array can be read as a thermal image with 64 pixels arranged in an 8x8 matrix.
The pixel array can be readout at any time and speed. However, the sensor updates the corresponding registers depending on the configured frame rate.
<br/>The sensor has an integrated thermistor which can be readout, too.
environmental temperature.
<br/><br/>
The binding supports getting a thermal image as a two dimensional array in a floating point or two's complement representation of each pixels temperature.
```
public Temperature[,] GetThermalImage()
public int[,] GetRawImage()
```
**Note:** there is no statement in the reference specification regarding the synchronization between an update of the pixel registers and the readout operation. So, you may readout pixel data from two subsequent frames in one readout operation. However, for normal application this shouldn't be relevant.
<br/><br/>
```
public Temperature GetSensorTemperature()
```
**Note**: the thermistor temperature is <u>not</u> equivalent to the
<br/><br/>
### Operating Mode / Power Control
The sensor supports four operating modes to control power consumption:
* Normal
* Sleep Mode
* Stand-by with 60 seconds intermittence
* Stand-by with 10 seconds intermittence

```
public OperatingMode GetOperatingMode()
public void SetOperatingMode(OperatingMode operatingMode)
```

*Note*: refer to the reference specification for further details on mode transitions and sensor behavior.
<br/><br/>

### Reset
The sensor software can be reset in two extends.
* **Initial Reset:** Resets all flags and registers to default values
* **Flag Reset:** Resets all flags (status register, interrupt flag, interrupt table)
```
public void InitialReset()
public void FlagReset()
```
*Note*: resetting the interrupt related flags is only required if you want to clear flags while the readings are still within the hysteresis span. See interrupts section for further details on interrupt behavior.
<br/><br/>

### Sensor Status
The sensor status indicates if any pixel or the chip internal thermistor overran the upper or lower operating range limit. It also flags on the occurrence of an interrupt. The status can be read out and reset per flag:
```
public bool HasTemperatureOverflow()
public void ClearTemperatureOverflow()

public bool HasThermistorOverflow()
public void ClearThermistorOverflow()

public bool HasInterrupt()
public void ClearInterrupt()

public void ClearAllFlags()
```
*Note*: resetting the interrupt flag is only required if you want to clear flags while the readings are still within the hysteresis span. See interrupts section for further details on interrupt behavior.
*Note*: the thermistor overflow flag is only menthioned in early versions of the reference specification.
It is not clear whether this is a specification error or a change in a newer revision of the sensor.
<br/><br/>

### Frame Rate
**Default:** 10fps<br/>
The sensor supports frame rates of 1fps and 10fps.<br/>
```
public FrameRate GetFrameRate()
public void SetFrameRate(FrameRate)
```
<br/>

### Moving average
**Default:** off<br/>
The sensor supports a moving average mode. In this mode it builds the twice moving average for each pixel.
* If the frame rate is set to 10fps the sensor takes the average of the readings *n* and *n+1* and yields their average as output.
* If the frame rate is set to 1fps the sensor takes the readings of 10 frames (as the sensor runs internally always at 10fps) and builds the average.
The average of two averages of 10 readings is the resulting output.

<br/>

![Moving average principle](./movingavg.png)
<br/>*Moving Average*
<br/>

The noise per pixel will decrease to 1/sqrt2 when using the moving average mode.<br/>
```
public bool GetMovingAverageModeState()
public void SetMovingAverageModeState(bool state)
```
***Important***: the reference specification states that the current mode can be read, but it doesn't seem to work at the time being. The bit is always read as 0.
<br/><br/>




### Interrupt control, levels and pixel flags
The sensor can raise an interrupt if any pixel passes a given value. The event is signaled by the interrupt flag of the status register. Additionally the INT pin of the sensor can be pulled low.

```
public void GetInterruptModeTest(InterruptMode expectedMode, byte registerValue)
public void SetInterruptModeTest(InterruptMode mode, bool modeBitIsSet)
```
The interrupt levels can be configured. The lower and upper limit as well as the hysteresis level can be set and read. Initially the register is filled with zeros. The levels apply to all pixels equally.

```
public Temperature GetInterruptLowerLevel()
public Temperature SetInterruptLowerLevel()

public Temperature GetInterruptUpperLevel()
public Temperature SetInterruptUpperLevel()

public Temperature GetInterruptHysteresisLevel()
public Temperature SetInterruptHysteresisLevel()
```
After the sensor raised an interrupt the triggering pixels can be readout from the interrupt table register.
```
public bool[,] GetInterruptFlagTable()
```
<br/>

**Note**
* be aware that the interrupt flag in the status register is reset automatically if no pixel temperature exceeds the lower or upper threshold. It is <u>not</u> required to reset the flag manually.
* any flag in the interrupt flag table is automatically reset if the corresponding pixel is no long exceed the lower or upper threshold.
* if a hysteresis level is applied and the reading of a pixel is not passing the threshold anymore, while at the same time the reading is still within the hysteresis span, the interrupt flag can be cleared by using the ```FlagReset``` method.
<br/><br/>

## References
**Product Homepage**: https://industry.panasonic.eu/components/sensors/grid-eye

**Product Flyer**: https://eu.industrial.panasonic.com/sites/default/pidseu/files/downloads/files/grid-eye_flyer_english_web.pdf

**Reference Specification**: https://mediap.industry.panasonic.eu/assets/custom-upload/Components/Sensors/Industrial%20Sensors/Infrared%20Array%20Sensor%20Grid-EYE/grid_eye_reference_specifications_160205.pdf

**FAQ**: https://mediap.industry.panasonic.eu/assets/custom-upload/Components/Sensors/Industrial%20Sensors/Infrared%20Array%20Sensor%20Grid-EYE/faqs_grideye_v1.0.pdf

**Application Note**: https://mediap.industry.panasonic.eu/assets/custom-upload/Components/Sensors/Industrial%20Sensors/Infrared%20Array%20Sensor%20Grid-EYE/application_notes_grid-eye_0.pdf

