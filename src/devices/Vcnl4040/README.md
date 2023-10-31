# Device Description
The VCNL4040 is a proximity and ambient light sensor module designed for various applications that require accurate measurement of proximity and light levels.

## Key features of the sensor
  * **Distance measuring**: It detects the presence of objects in close proximity to the sensor. It uses an integrated IR emitter and a photodiode to measure the reflected infrared light, allowing it to determine the distance to the object.
  **Measurig range is 0 to 200 mm**.
  * **Ambient light measuring**: It measures ambient light levels in its environment. It can provide digital outputs or analog voltage readings that correspond to the illuminance (light intensity) detected.
  **Measuring range is 0.0125 to 6553 lux**.
  * It is **adjustable** with application specific settings, such as proximity and ambient light measurement resolution, by configuring its internal registers.
  * It can be configured to **trigger interrupts** based on user-defined proximity or light thresholds.

Official manufacturer documentation: [Datasheet](https://www.vishay.com/docs/84274/vcnl4040.pdf)

## Ambient light sensor configuration
### Measuring
To provide flexibility in tailoring your measurements to specific conditions, the VCNL4040 allows you to fine-tune the integration time, which dictates how long light is sampled and influences measurement sensitivity in varying lighting environments. When using a shorter integration time like 80ms, the sensor collects data for a brief period, resulting in each recorded unit of light representing a more significant light intensity. Consequently, the maximum measurable lux for an 80ms integration time reaches 6553.5 lux.

Conversely, with an extended integration time, you wait longer for the same number of light measurements, implying a relatively lower rate of light events and indicating reduced ambient light. Therefore, at the maximum integration time of 640ms, the VCNL4040 can measure up to 819.2 lux.

It's important to note that increasing the integration time not only impacts sensitivity but also enhances measurement resolution within the specified range.

The parameter dependency is:
|Integration time [ms]|Resolution [lux]|Detection range [lux]|
|-|-|-|
|80|0.1|6335.5|
|160|0.05|3276.7|
|320|0.025|1638.3|
|640|0.0125|819.1|

For details refer to the official datasheet.

## Proximity sensor configuration
Furthermore, aside from adjusting light sensitivity, you can modify the current and duty cycle of the IR LED responsible for proximity detection, allowing you to fine-tune the sensitivity of proximity measurements to better suit your requirements.

## Interrupt configuration
An interrrupt event can be triggered when the detected illuminance goes above or below a configured threshold.
The ALS INT is triggered once the ALS value is higher or lower than the threshold window. The ALS_PERS (1, 2, 4, 8 times)
parameter, sets the amount of consecutive hits needed, in order for an interrupt event to trigger.

If the event occurs a flag in the interrupt flag register is set to indicate the source and the INT-pin of the device is pulled low.

Note: for the ALS it may take some time before the interrupt event occurs, depending on the integration time and the persistence setting. If the integration time is 640 ms and the persistence setting is 8, it takes 8 * 640 ms = 5120 ms for the interrupt to occur.

# Binding Documentation
The device binding provides an API to the VCNL4040 device and its functions.
The API hides the details of the device and provides methods for most (if not all) use cases.
The driver also implements rules regarding functional dependencies and conditions specified in the data sheet, where applicable. This assists the user of the API, eliminating the need for an extensive study of the data sheet. Subsequent documentation will describe these corresponding functions.

## General Interface

## Ambient Light Sensor Interface

### General
#### Power state

### Measurement
The current illuminance reading can be get at anytime.
It is updated in the interval of the integration time.

Property:
```
public Illuminance Reading
```

### Configuration
In general, the resolution or range is configured once, before configuring the interrupt settings, enabling the interrupt, and finally activating the sensor. From that point forward, current values are read or interrupt(s) are triggered.

#### Integration time / detection range / resolution
As described in section [Ambient light sensor configuration](Ambient-light-sensor-configuration) these parameters depend on each other. The API provides three properties to set these parameters and adjusting the depending parameters at the same time.

**Important**: changing one of these parameters may cause an invalid interrut thresholds configuration. If the new range or resolution results in a detection range below the configured threshold, the interrupt is deactivated and must be reconfigured before reactivation.

Properties:

```
public AlsIntegrationTime IntegrationTime
public AlsRange Range
public AlsResolution Resolution
```

### Interrupt configuration
The interrupt of the ALS is configured by setting the lower and upper thresholds and the hit persistence.
When setting the thresholds, the binding checks the validity of the values with respect to the configured integration time, resolution, or measurement range. Setting a threshold outside the valid measurement range results in an exception, as does using negative values. This safeguards the user against inadvertent misconfigurations.

**Note**: changing the integration time, detection range or resolution result in deactivating the interrupt and requires reconfiguration before reactivation.

```
public void ConfigureInterrupt(Illuminance lowerThreshold, Illuminace upperThreshold, alalalalal persistence)
```

### Helper


### Sensor controlling
#### Interrupt state

## Proximity Sensor Interface
