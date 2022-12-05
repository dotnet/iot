// See https://aka.ms/new-console-template for more information
using Iot.Device.HX711;
using System.Device.Gpio;
using UnitsNet;
using UnitsNet.Units;

Console.WriteLine("Hello, World!");
Console.ReadLine();

int pinDout = 23;
int pinPD_Sck = 24;

using (var controller = new GpioController())
{
    using (var hx711 = new HX711(pinDout, pinPD_Sck, gpioController: controller, shouldDispose: false))
    {
        hx711.PowerUp();
        Console.WriteLine("HX711 is on.");

        Console.WriteLine("Known weight (in grams) currently on the scale:");
        var weightCalibration = int.Parse(Console.ReadLine());

        hx711.StartCalibration(new Mass(weightCalibration, MassUnit.Gram));
        Thread.Sleep(30_000);

        Console.WriteLine("Known weight (in grams) currently on the scale:");
        weightCalibration = int.Parse(Console.ReadLine());
        hx711.StartCalibration(new Mass(weightCalibration, MassUnit.Gram));
        Thread.Sleep(30_000);

        Console.WriteLine("Known weight (in grams) currently on the scale:");
        weightCalibration = int.Parse(Console.ReadLine());
        hx711.StartCalibration(new Mass(weightCalibration, MassUnit.Gram));
        Thread.Sleep(30_000);

        hx711.Tare();
        Console.WriteLine($"Tare set. Value: {hx711.TareValue}gr");

        var weight = hx711.GetWeight();
        Console.WriteLine($"Weight: {weight}gr");

        hx711.PowerDown();
        Console.WriteLine("HX711 is off.");

        _ = Console.ReadLine();
    }
}