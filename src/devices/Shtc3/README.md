# SHTC3 - Temperature & Humidity Sensor
SHTC3 is a digital humidity and temperature sensor designed especially for battery-driven high-volume consumer electronics application.
To reduce power cosumption this project use capability of sensor to allow measurement in low power mode and active sleep mode.

## Usage
```C#
I2cConnectionSettings settings = new I2cConnectionSettings(1, Iot.Device.Shtc3.Shtc3.I2cAddress);
I2cDevice device = I2cDevice.Create(settings);

using (Shtc3 sensor = new Shtc3(device))
{
    if (sensor.TryGetTempAndHumi(out var measure))
    {
        // temperature (℃)
        double temperature = measure.Temperature.Celsius;
        // humidity (%)
        double humidity = measure.Humidity;
    }

    // Make sensor in sleep mode
    sensor.Status = Status.Sleep;
}
```

## References
https://www.sensirion.com/fileadmin/user_upload/customers/sensirion/Dokumente/2_Humidity_Sensors/Datasheets/Sensirion_Humidity_Sensors_SHTC3_Datasheet.pdf
