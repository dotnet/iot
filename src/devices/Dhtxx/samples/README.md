# Example of DHTxx

## Hardware Required

* DHT10/DHT11/DHT12/DHT21/DHT22
* Male/Female Jumper Wires

## Circuit

### 1-Wire Protocol

Simply connect your DHTxx data pin to GPIO26 (physical pin 37), the ground to the ground (physical pin 6) and the VCC to +5V (physical pin 2).

![schema](./dht22.png)

Some sensors are already sold with the 10K resistor. Connect the GPIO26 to the *data* pin, its position can vary depending on the integrator.

### I2C Protocol

![](DHT12_circuit_bb.png)

* SCL - SCL
* SDA - SDA
* VCC - 5V
* GND - GND

## Code

```csharp
// GPIO Pin
using (Dht11 dht = new Dht11(26))
{
    var temperature = dht.Temperature;
    var humidity = dht.Humidity;
    // You can only display temperature and humidity if the read is successful otherwise, this will raise an exception as
    // both temperature and humidity are NAN
    if (dht.IsLastReadSuccessful)
    {
        Console.WriteLine($"Temperature: {temperature.DegreesCelsius} \u00B0C, Humidity: {humidity.Percent} %");

        // WeatherHelper supports more calculations, such as saturated vapor pressure, actual vapor pressure and absolute humidity.
        Console.WriteLine(
            $"Heat index: {WeatherHelper.CalculateHeatIndex(temperature, humidity).Celsius:0.#}\u00B0C");
        Console.WriteLine(
            $"Dew point: {WeatherHelper.CalculateDewPoint(temperature, humidity).Celsius:0.#}\u00B0C");
    }
    else
    {
        Console.WriteLine("Error reading DHT sensor");
    }
}
```

## Sample application navigation

This sample application allows you to select either a DHT10 through I2C either any other supported DHT through GPIO:

```
Select the DHT sensor you want to use:
 1. DHT10 on I2C
 2. DHT11 on GPIO
 3. DHT12 on GPIO
 4. DHT21 on GPIO
 5. DHT22 on GPIO
```

Just select the sensor you want to test and use by typing the number. For example, if you want to test a DHT22, type 5.

Then, you are prompted to type the pin number in the logical schema: 

```
Which pin do you want to use in the logical pin schema?
```

If you want to use the pin 26, then type 26 and enter. This will then create a DHT22 sensor attached to pin 26 and start the measurement.

Please note that the few first measurements won't be correct, that's totally normal and related to the fact the sensor needs a bit of time to warm up and give data. Those sensors are very sensitive and too long wires, many perturbations, code compile as debug will increase the numbers of bad readings.


## Result

![dht22 output](./dht22ex.jpg)

Note: reading this sensor is sensitive, if you can't read anything, make sure you have it correctly cabled. Also note you'll get better results when running in ```Release``` mode.


