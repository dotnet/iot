# ADS1115 - Analog to Digital Converter
ADS1115 is an Analog-to-Digital converter (ADC) with 16 bits of resolution.

## Sensor Image
![](sensor.jpg)

## Usage
```C#
// set I2C bus ID: 1
// ADS1115 Addr Pin connect to GND
I2cConnectionSettings settings = new I2cConnectionSettings(1, (int)I2cAddress.GND);
// get I2cDevice (in Linux)
UnixI2cDevice device = new UnixI2cDevice(settings);
// get I2cDevice (in Win10)
//Windows10I2cDevice device = new Windows10I2cDevice(settings);

// pass in I2cDevice
// measure the voltage AIN0
// set the maximum range to 6.144V
using (Ads1115 adc = new Ads1115.Ads1115(device, Config.ADS1015_REG_CONFIG_MUX_SINGLE_0 | Config.ADS1015_REG_CONFIG_PGA_6_144V))
{
    // read raw data form the sensor
    short raw = adc.ReadRaw();
    // raw data convert to voltage
    double voltage = adc.RawToVoltage(raw);
}
```

## References
https://cdn-shop.adafruit.com/datasheets/ads1115.pdf
