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
To provide flexibility in tailoring your measurements to specific conditions, the VCNL4040 allows you to fine-tune the integration time, which dictates how long light is sampled and influences measurement sensitivity in varying lighting environments. When using a shorter integration time like 80ms, the sensor collects data for a brief period, resulting in each recorded unit of light representing a more significant light intensity. Consequently, the maximum measurable lux for an 80ms integration time reaches 6553.5 lux.

Conversely, with an extended integration time, you wait longer for the same number of light measurements, implying a relatively lower rate of light events and indicating reduced ambient light. Therefore, at the maximum integration time of 640ms, the VCNL4040 can measure up to 819.2 lux.

It's important to note that increasing the integration time not only impacts sensitivity but also enhances measurement resolution within the specified range.

ÜBERARBEITEN:
The ALS INT is triggered once the ALS value is higher or lower than the threshold window. The ALS_PERS (1, 2, 4, 8 times)
parameter, sets the amount of consecutive hits needed, in order for an interrupt event to trigger.
n * IntegrationTime

## Proximity sensor configuration
Furthermore, aside from adjusting light sensitivity, you can modify the current and duty cycle of the IR LED responsible for proximity detection, allowing you to fine-tune the sensitivity of proximity measurements to better suit your requirements.

## Interrupt configuration

# Binding Documentation
The device binding provides an API to the VCNL4040 device and its functions.
The API hides the details of the device and provides methods for most (if not all) use cases.
The driver also implements rules regarding functional dependencies and conditions specified in the data sheet, where applicable. This assists the user of the API, eliminating the need for an extensive study of the data sheet. Subsequent documentation will describe these corresponding functions.

## General Interface

## Ambient Light Sensor Interface
### Illuminance measurement
The current illuminance reading can be get at anytime.
It is updated in the interval of the integration time and depends on detection range / resolution setting.

Property:
```
public Illuminance Reading
```

### Sensor configuration
In general, the resolution or range is configured once, before configuring the interrupt settings, enabling the interrupt, and finally activating the sensor. From that point forward, current values are read or interrupt(s) are triggered.

#### Detection range / resolution
Either the detection range (maximum illuminance) or detection resolution can be configured.
As described before these parameters depends on each other and determine the integration time for collecting light.
The API provides two properties to configure either range or resolution. The respective other property is adjusted depending on the resulting integration time.

**Important**: changing the detection range or resolution may cause an invalid interrut thresholds configuration. If the new range or resolution results in a detection range below the configured threshold, the interrupt is deactivated and must be reconfigured before reactivation.

Properties:

```
public AlsRange Range
public AlsResolution Resolution
```

READONLY: integration time

### Interrupt configuration
The interrupt thresholds and hit persistence



### Sensor controlling
#### Interrupt state
#### Power state

## Proximity Sensor Interface
