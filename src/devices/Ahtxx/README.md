# AHT10/15/20 Temperature and Humidity Sensor Modules

## Summary
The AHT10/15 and AHT20 sensors are high-precision, calibrated temperature and relative humidity sensor modules with an I2C digital interface.
<br/><br/>


## Binding Notes
### Supported Devices
The binding supports the following types:
* AHT10 - http://www.aosong.com/en/products-40.html
* AHT15 - http://www.aosong.com/en/products-45.html
* AHT20 - http://www.aosong.com/en/products-32.html
<br/><br/>

### Functions
The binding supports the following sensor functions:
* acquiring the temperature and relative humidty readings
* reading status
* issueing calibration and reset commands
<br/><br/>

### Sensor classes
You need to choose the class depending on the sensor type.

|Sensor|Required class|
|-----|---------------|
|AHT10|Aht10          |
|Aht15|Aht10          |
|Aht20|Aht20          |










