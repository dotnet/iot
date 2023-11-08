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
The API hides the details of the device and provides methods for most common use cases.
The binding also implements rules regarding functional dependencies and conditions specified in the data sheet, where applicable. This assists the user of the API, eliminating the need for an extensive study of the data sheet and preventing from inadvertent misconfiguration resulting in an unexpected behaviour.

Even though most use cases should require a one-time configuration only it can be configured at any time. Most parameters can be changed independently. Only the configuration of the integration time (and, in the case of the ambient light sensor, the dependent parameters of range and resolution) also affects other functions. The binding handles the dependencies, particularly regarding the interrupt function.

In most cases the use of binding and device follows this sequence:
* Verify device
* Configure ambient light and proximity sensor, particularly the parameter *integration time*
* Configure and enable interrupts (*optionally*)
* Retrieve sensor readings

## Bus load reduction
It is necessary for the binding to be aware of the currently configured integration time. This is required to convert measurement values according to the resolution and perform consistency checks when configuring interrupts.

The binding can either fetch the integration time when needed from the device or use an internally stored value. Retrieving it from the device increases I2C bus load, particularly with frequent measurements.
To reduce bus load, the binding has a *load reduction mode* where it stores the last configured value for integration time, whether it's set directly or indirectly via range or resolution. However, this may cause inconsistencies if the device is externally configured or after a power cycle, impacting measurement accuracy and other functions.

If bus load is not an issue **do not** enable load reduction mode.

## General Interface

|Area|Function|API|Comments|
|-|-|-|-|
|Common|Check basic functionality|```void VerifyDevice();```|Verifies whether a functional I2C connection to the device exists and checks the identification for device recognition. Should be used before any other function of the binding is used.

## Ambient Light Sensor Interface
|Area|Function|API|Comments|
|-|-|-|-|
|General|Control power state|```bool PowerOn {get; set;}```|The ambient light sensor can be turned off to reduce power consumption of the chip.|
|General|Control load reduction mode|```bool LoadReductionModeEnabled {get; set;}```|If enabled a local copy of the integration time is used for calculations. Before enabling load reduction mode, the current configuration is retrieved from the device.|
|Measurement|Get latest reading|```Illuminance Reading {get;}```|The current value can be get at anytime. The device internal update interval depends on the configured integration time. The integration time determines the period during which the sensor collects light before an updated value is available.<br>The maximum value and resolution depends on the configured integration time, range or resolution.|
|Configuration|Configure integration time|```AlsIntegrationTime IntegrationTime {get; set;}```|**Important**: configuring the integration time, and the depedent range, would affect any configured interrupt thresholds. Therefore interrupts are implicitly disabled and required to be configured and enabled again.<br>**Note:** the property will always get the current value from the device, even when in load reduction mode.<br>**Note:**: changing the property will implicitly adjust the dependent properties Range and Resolution as well. For details refer to section [Ambient light sensor configuration](Ambient-light-sensor-configuration).<br>|
|Configuration|Configure range|```AlsRange Range {get; set;}```|This property depends on the IntegrationTime property. All comments apply accordingly.|
|Configuration|Configure resolution|```AlsResolution Resolution {get; set;}```|This property depends on the IntegrationTime property. All comments apply accordingly.|
|Interrupt|Configure and enable interrupts|```void EnableInterrupt(Illuminance lowerThreshold, Illuminance upperThreshold, AlsInterruptPersistence persistence)```|Thresholds are checked for validity with respect to the configured integration time and its resulting resolution and range. Setting a threshold outside the valid measurement range results in an exception, as does using negative values.|
|Interrupt|Disable interrupts|```void DisableInterrupts```||
|Interrupt|InterruptsEnabled|bool InterruptEnabled {get;}|Gets the current interrupt enabled state from the device|
|Convenience|Get range in physical unit|```RangeAsIlluminance {get;} ```||
|Convenience|Get resolution in physical unit|```ResolutionAsIlluminance {get;} ```||

## Proximity Sensor Interface
|Area|Function|API|Comments|
|-|-|-|-|
|General|Control power state|```bool PowerOn {get; set;}```|The ambient light sensor can be turned off to reduce power consumption of the chip.|
|General|Control load reduction mode|```bool LoadReductionModeEnabled {get; set;}```|If enabled a local copy of the integration time is used for calculations. Before enabling load reduction mode, the current configuration is retrieved from the device.|
|Measurement|Get latest reading|```int Reading {get;}```|The current proximity value (counts) can be get at anytime. The device internal update interval depends on the configured integration time.|
|Configuration|Enable extended 16-bit output range|```bool ExtendedOutputRange {get;}```|Controls the extended output range, which changes the measurement value size from 12-bit (0..4095 counts) to 16-bit (0..65535 counts). This may be necessary depending on the surface that reflects the light for the measurement.|
|Configuration|Enable active force mode|```bool ActiveForceMode {get;}```|Controls the active force mode. If in active force mode the sensor measures the distance only on demand. The binding will request a measurement when getting a reading using the ```Reading```-property.|

# Samples
## Simple
The "Simple" sample application demonstrates the fundamental usage of the binding. Initially, it configures both the ambient light sensor and the proximity sensor. Subsequently, it retrieves the current sensor readings at a 200 ms interval and displays them in the console. In the same loop, it clears all interrupts, causing any LED connected to the INT-pin of the VCNL4040 device to briefly blink if the configured interrupt conditions are met. The application can be terminated by pressing a key.

## Explorer
The **Explorer** sample application allows the user to experiment with all the features of the Binding API. Sensor configurations can be modified at any time. Additionally, the status of the interrupt can be manually read and reset, and individual sensors can be turned on and off. The application includes a simple menu for accessing API functions, and straightforward prompts allow parameter input at runtime.
