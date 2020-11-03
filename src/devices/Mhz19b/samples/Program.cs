// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.IO;
using System.IO.Ports;
using System.Text;
using System.Threading;
using Iot.Device.Mhz19b;
using UnitsNet;

// create serial port using the setting acc. to datasheet, pg. 7, sec. general settings
var serialPort = new SerialPort("/dev/serial0", 9600, Parity.None, 8, StopBits.One)
{
    Encoding = Encoding.ASCII,
    ReadTimeout = 1000,
    WriteTimeout = 1000
};
serialPort.Open();
Mhz19b sensor = new Mhz19b(serialPort.BaseStream, true);

// Alternatively you can let the binding create the serial port stream:
// Mhz19b sensor = new Mhz19b("/dev/serial0");

// Switch ABM on (default).
// sensor.SetAutomaticBaselineCorrection(AbmState.On);

// Set sensor detection range to 2000ppm (default).
// sensor.SetSensorDetectionRange(DetectionRange.Range2000);

// Perform calibration
// Step #1: perform zero point calibration
// Step #2: perform span point calibration at 2000ppm
// CAUTION: enable the following lines only if you know exactly what you do.
//          Consider also that zero point and span point calibration are performed
//          at different concentrations. The sensor requires up to 20 min to be
//          saturated at the target level.
// sensor.PerformZeroPointCalibration();
// ---- Now change to target concentration for span point.
// sensor.PerformSpanPointCalibration(VolumeConcentration.FromPartsPerMillion(2000));

// Continously read current concentration
while (true)
{
    try
    {
        VolumeConcentration reading = sensor.GetCo2Reading();
        Console.WriteLine($"{reading.PartsPerMillion:F0} ppm");
    }
    catch (IOException e)
    {
        Console.WriteLine("Concentration couldn't be read");
        Console.WriteLine(e.Message);
        Console.WriteLine(e.InnerException?.Message);
    }

    Thread.Sleep(1000);
}
