// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using System.Threading;
using Iot.Device.Common;
using Iot.Device.DHTxx;

Console.WriteLine("Hello DHT!");
Console.WriteLine("Select the DHT sensor you want to use:");
Console.WriteLine(" 1. DHT10 on I2C");
Console.WriteLine(" 2. DHT11 on GPIO");
Console.WriteLine(" 3. DHT12 on GPIO");
Console.WriteLine(" 4. DHT21 on GPIO");
Console.WriteLine(" 5. DHT22 on GPIO");
var choice = Console.ReadKey();
Console.WriteLine();
if (choice.KeyChar == '1')
{
    Console.WriteLine("Press any key to stop the reading");
    // Init DHT10 through I2C
    I2cConnectionSettings settings = new I2cConnectionSettings(1, Dht10.DefaultI2cAddress);
    I2cDevice device = I2cDevice.Create(settings);

    using Dht10 dht = new Dht10(device);
    Dht(dht);
    return;
}

Console.WriteLine("Which pin do you want to use in the logical pin schema?");
var pinChoise = Console.ReadLine();
int pin;
try
{
    pin = Convert.ToInt32(pinChoise);
}
catch (Exception ex) when (ex is FormatException || ex is OverflowException)
{
    Console.WriteLine("Can't convert pin number.");
    return;
}

Console.WriteLine("Press any key to stop the reading");

switch (choice.KeyChar)
{
    case '2':
        Console.WriteLine($"Reading temperature and humidity on DHT11, pin {pin}");
        using (var dht11 = new Dht11(pin))
        {
            Dht(dht11);
        }

        break;
    case '3':
        Console.WriteLine($"Reading temperature and humidity on DHT12, pin {pin}");
        using (var dht12 = new Dht12(pin))
        {
            Dht(dht12);
        }

        break;
    case '4':
        Console.WriteLine($"Reading temperature and humidity on DHT21, pin {pin}");
        using (var dht21 = new Dht21(pin))
        {
            Dht(dht21);
        }

        break;
    case '5':
        Console.WriteLine($"Reading temperature and humidity on DHT22, pin {pin}");
        using (var dht22 = new Dht22(pin))
        {
            Dht(dht22);
        }

        break;
    default:
        Console.WriteLine("Please select one of the option.");
        break;
}

void Dht(DhtBase dht)
{
    while (!Console.KeyAvailable)
    {
        var temp = dht.Temperature;
        var hum = dht.Humidity;
        // You can only display temperature and humidity if the read is successful otherwise, this will raise an exception as
        // both temperature and humidity are NAN
        if (dht.IsLastReadSuccessful)
        {
            Console.WriteLine($"Temperature: {temp.DegreesCelsius}\u00B0C, Relative humidity: {hum.Percent}%");

            // WeatherHelper supports more calculations, such as saturated vapor pressure, actual vapor pressure and absolute humidity.
            Console.WriteLine(
                $"Heat index: {WeatherHelper.CalculateHeatIndex(temp, hum).DegreesCelsius:0.#}\u00B0C");
            Console.WriteLine(
                $"Dew point: {WeatherHelper.CalculateDewPoint(temp, hum).DegreesCelsius:0.#}\u00B0C");
        }
        else
        {
            Console.WriteLine("Error reading DHT sensor");
        }

        // You must wait some time before trying to read the next value
        Thread.Sleep(2000);
    }
}
