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

## Ambient light sensor
The VCNL4040 allows you to configure the integration time, which dictates how long light is sampled and influences measurement sensitivity in varying lighting environments. When using a shorter integration time like 80 ms, the sensor collects data for a brief period, resulting in each recorded unit of light representing a more significant light intensity. Consequently, the maximum measurable lux for an 80ms integration time reaches 6553.5 lux.

Conversely, with an extended integration time, you wait longer for the same number of light measurements, implying a relatively lower rate of light events and indicating reduced ambient light. Therefore, at the maximum integration time of 640ms, the VCNL4040 can measure up to 819.2 lux.

**Important**: increasing the integration time not only impacts sensitivity but also enhances measurement resolution within the specified range.

The parameter dependency is:
|Integration time [ms]|Resolution [lux]|Detection range [lux]|
|-|-|-|
|80|0.1|6335.5|
|160|0.05|3276.7|
|320|0.025|1638.3|
|640|0.0125|819.1|

The ambient light sensor can be turned off to reduce power consumption of the chip.

For details refer to the official datasheet.

## Proximity sensor
Current and duty ratio of the IR LED responsible for proximity detection can be configured, allowing you to adjust the sensitivity of proximity measurements. The higher the duty
ratio, the faster the response time achieved with higher power consumption.

The proximity sensor can be turned off to reduce power consumption of the chip.

## Interrupt configuration
An interrrupt event can be triggered when the detected illuminance goes above or below a configured threshold.
The ALS INT is triggered once the ALS value is higher or lower than the threshold window. The ALS_PERS (1, 2, 4, 8 times)
parameter, sets the amount of consecutive hits needed, in order for an interrupt event to trigger.

If the event occurs a flag in the interrupt flag register is set to indicate the source and the INT-pin of the device is pulled low.

Note: for the ALS it may take some time before the interrupt event occurs, depending on the integration time and the persistence setting. If the integration time is 640 ms and the persistence setting is 8, it takes 8 * 640 ms = 5120 ms for the interrupt to occur.

# Binding Documentation

## Basic principles
The binding provides an API to the VCNL4040 device and its functions.
The API hides the details of the device and provides methods for most (if not all) use cases.
The driver also implements rules regarding functional dependencies and conditions specified in the data sheet, where applicable. This assists the user of the API, eliminating the need for an extensive study of the data sheet.

Even though most use cases should require a one-time configuration only it can be configured at any time. Most parameters can be changed independently. Only the configuration of the integration time (and, in the case of the ambient light sensor, the dependent parameters of Range and Resolution) also affects other functions. The binding handles the dependencies and checks consistency.

## Bus load reduction
It is necessary for the binding to be aware of the currently configured integration time. This is required to convert measurement values according to the resolution and perform consistency checks when configuring interrupts.

The binding can either fetch the integration time when needed from the device or use an internally stored value. Retrieving it from the device increases I2C bus load, particularly with frequent measurements.
To reduce bus load, the binding has a *load reduction mode* where it stores the last configured value for integration time, whether it's set directly or indirectly via range or resolution. However, this may cause inconsistencies if the device is externally configured or after a power cycle, impacting measurement accuracy and other functions.

## General Interface

|Area|Function|API|Comments|
|-|-|-|-|
|Common|Check basic functionality|```public void VerifyDevice();```|Verifies whether a functional I2C connection to the device exists and checks the identification for device recognition. Should be used before any other function of the binding is used.
|Common|Reduce bus load|```public bool LoadReductionModeEnabled {get; set;}```|Before enabling load reduction mode, the current configuration is retrieved from the device.|


## Ambient Light Sensor Interface

In general, the integration time, resolution or range is configured once, before configuring the interrupt settings, enabling the interrupt, and finally powering the sensor. From that point forward, current values are read or interrupt(s) are triggered. The interrupt configuration, as well as the power state and interrupt enable / disable state can be changed at any time afterwards.

As described in section [Ambient light sensor configuration](Ambient-light-sensor-configuration) the parameters integration time, range and resolution depend on each other. The API provides three properties to set these parameters and adjusting the depending parameters at the same time.

|Area|Function|API|Comments|
|-|-|-|-|
|General|Control power state|```public bool PowerOn {get; set;}```|The ambient light sensor can be turned off to reduce power consumption of the chip.|
|Measurement|Get latest reading|```public Illuminance Reading {get;}```|The current illuminance reading can be get at anytime. The update interval of the reading depends on the integration time and persistence setting.*Interval = integration time * persistence*, e.g. 320 ms * 4 = 1280 ms. The maximum value and resolution depends on the configured integration time, range or resolution.|
|Configuration|Configure integration time|```public AlsIntegrationTime IntegrationTime {get; init;}```|**Note:** the property will always get the current value from the device.<br>Configuring the integration changes the dependent parameters, range and resolution, too. The initialization of these three parameters is mutually exclusive for each one of them. The changed integration time, and the resulting changes of range and resolution, have to be considered when configuring interrupts. Changing the integration time will adjust the interrupt configuration  |
|Configuration|Configure range|```public AlsRange Range {get; init;}```|bla bla|
|Configuration|Configure resolution|```public AlsResolution Resolution {get; init;}```|bla bla|

### Configuration

#### Integration time / detection range / resolution

**Important**: changing one of these parameters may cause an invalid interrut thresholds configuration. If the new range or resolution results in a detection range below the configured threshold, the interrupt is deactivated and must be reconfigured before reactivation.


### Interrupt configuration
The interrupt of the ALS is configured by setting the lower and upper thresholds and the hit persistence.
When setting the thresholds, the binding checks the validity of the values with respect to the configured integration time, resolution, or measurement range. Setting a threshold outside the valid measurement range results in an exception, as does using negative values. This safeguards the user against inadvertent misconfigurations.

**Note**: changing the integration time, detection range or resolution result in deactivating the interrupt and requires reconfiguration before reactivation.

```
public void ConfigureInterrupt(Illuminance lowerThreshold, Illuminace upperThreshold, alalalalal persistence)
```

### Sensor controlling
#### Interrupt state

## Proximity Sensor Interface

### General
#### Power state
The proximity sensor can be turned off to reduce power consumption of the chip.
```
public bool PowerOn {get; set;}
```

### Configuration
#### IR LED current and duty ratio

**Important:** make sure that your power source is able to support higher LED currents before configuring! The host system (e.g. RPi) may otherwise become unstable or crash.
```
public PsDuty DutyRatio
public PsLedCurrent LedCurrent
```

#### Output length

```
public PsOutput OutputSize
```

#### Integration time

```
public PsIntegrationTime IntegrationTime
```

# Samples
## Simple
The "Simple" sample application demonstrates the fundamental usage of the binding. Initially, it configures both the ambient light sensor and the proximity sensor. Subsequently, it retrieves the current sensor readings at a 200 ms interval and displays them in the console. In the same loop, it clears all interrupts, causing any LED connected to the INT-pin of the VCNL4040 device to briefly blink if the configured interrupt conditions are met. The application can be terminated by pressing a key.

## Explorer
The **Explorer** sample application allows the user to experiment with all the features of the Binding API. Sensor configurations can be modified at any time. Additionally, the status of the interrupt can be manually read and reset, and individual sensors can be turned on and off. The application includes a simple menu for accessing API functions, and straightforward prompts allow parameter input at runtime.
