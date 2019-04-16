# DHTxx - Digital-Output Relative Humidity & Temperature Sensor Module

The DHT temperature and humidity sensors are very popular. This projects support DHT11, DHT12, DHT21(AM2301), DHT22(AM2302).

## Usage

### 1-Wire Protocol

```csharp
// GPIO Pin, DHT Type
using (DhtSensor dht = new DhtSensor(26, DhtType.DHT22))
{
    Temperature temperature = dht.Temperature;
    double humidity = dht.Humidity;
}
```

### I2C Protocol

Only DHT12 can use I2C protocol.

```csharp
I2cConnectionSettings settings = new I2cConnectionSettings(1, DhtSensor.DefaultI2cAddressDht12);
UnixI2cDevice device = new UnixI2cDevice(settings);

using (DhtSensor dht = new DhtSensor(device))
{
    Temperature temperature = dht.Temperature;
    double humidity = dht.Humidity;
}
```

## References

* **DHT11** [datasheet](https://cdn.datasheetspdf.com/pdf-down/D/H/T/DHT11-Aosong.pdf)
* **DHT12** [datasheet](https://cdn.datasheetspdf.com/pdf-down/D/H/T/DHT12-Aosong.pdf)
* **DHT21** [datasheet](https://cdn.datasheetspdf.com/pdf-down/A/M/2/AM2301-Aosong.pdf)
* **DHT22** [datasheet](https://cdn-shop.adafruit.com/datasheets/DHT22.pdf)