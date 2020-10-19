# AHT10/15/20 Temperature and Humidity Sensor Modules

## Summary
The AHT10/15 and AHT20 sensors are high-precision, calibrated temperature and relative humidity sensor modules with an I2C digital interface.

## Binding Notes
### Supported Devices
The binding supports the following types:
* AHT10 - http://www.aosong.com/en/products-40.html
* AHT15 - http://www.aosong.com/en/products-45.html
* AHT20 - http://www.aosong.com/en/products-32.html

### Functions
The binding supports the following sensor functions:
* acquiring the temperature and relative humidty readings
* reading status
* issueing calibration and reset commands


### Sensor classes
You need to choose the class depending on the sensor type.

|Sensor|Required class|
|-----|---------------|
|AHT10|Aht10          |
|Aht15|Aht10          |
|Aht20|Aht20          |


### Basic Usage

The binding gets instantiated using an existing ```I2cDevice`` instance. The AHT-sensor modules support only the default I2C address.

Setup for an AHT20 sensor module:
```
const int I2cBus = 1;
I2cConnectionSettings i2cSettings = new I2cConnectionSettings(I2cBus, Aht20.DefaultI2cAddress);
I2cDevice i2cDevice = I2cDevice.Create(i2cSettings);
Aht20 sensor = new Aht20(i2cDevice);
```

The temperature and humidity readings are acquired by using the following methods:

```
public Temperature GetTemperature()
public Ratio GetHumidity()
```

Refer to the sample application for a complete example.
