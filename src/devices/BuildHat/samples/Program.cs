﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Iot.Device.BuildHat;
using Iot.Device.BuildHat.Models;
using Iot.Device.BuildHat.Motors;
using Iot.Device.BuildHat.Sensors;

Console.WriteLine("Hello, BuildHat!");

// On Windows, connected through a serial dongle:
Brick brick = new("COM3");
// On a Raspberry PI, you'll use:
// Brick brick = new("/dev/serial0");
var info = brick.BuildHatInformation;
Console.WriteLine($"version: {info.Version}, firmware date: {info.FirmwareDate}, signature:");
Console.WriteLine($"{BitConverter.ToString(info.Signature)}");
Console.WriteLine($"Vin = {brick.InputVoltage.Volts} V");
Console.WriteLine("Select what you want to test:");
Console.WriteLine(" 1. Display elements detials");
Console.WriteLine(" 2. Display connection/disconnecions");
Console.WriteLine(" 3. Move Motors on port A and D");
Console.WriteLine(" 4. PID with motors on port A");
Console.WriteLine(" 5. Read color sensor on Port C");
Console.WriteLine(" 6. Run train motor on Port B");

while (!Console.KeyAvailable)
{
    Thread.Sleep(100);
}

var choice = Console.ReadKey();

switch (choice.KeyChar)
{
    case '1':
        DisplayElementDetails();
        break;
    case '2':
        DisplayConnectionDisconnection();
        break;
    case '3':
        MoveMotorsAndBackToPosition();
        break;
    case '4':
        DriveMotors();
        break;
    case '5':
        ReadColorDistance();
        break;
    case '6':
        RunTrainMotor();
        break;
    default:
        break;
}

brick.Dispose();

void DisplayElementDetails()
{
    Console.WriteLine("Displaying details of all the connected elements");
    // Display all the details of all the sensors
    for (int i = 0; i < 4; i++)
    {
        SensorType sensor = brick.GetSensorType((SensorPort)i);
        Console.Write($"Port: {i} {(Brick.IsMotor(sensor) ? "Sensor" : "Motor")} type: {sensor} Connected: ");

        if (Brick.IsActiveSensor(sensor))
        {
            ActiveSensor activeSensor = brick.GetActiveSensor((SensorPort)i);
            Console.WriteLine($"{activeSensor.IsConnected}");
            foreach (var mode in activeSensor.ModeDetails)
            {
                Console.WriteLine($"  M{mode.Number} {mode.Name} {mode.Unit}");
                Console.WriteLine($"    format count={mode.NumberOfData} type={mode.DataType} chars={mode.NumberOfCharsToDisplay} dp={mode.DecimalPrecision}");
                foreach (var minmax in mode.MinimumMaximumValues)
                {
                    Console.WriteLine($"    {minmax.TypeValues} min={minmax.MinimumValue} max={minmax.MaximumValue}");
                }
            }

            foreach (var combi in activeSensor.CombiModes)
            {
                Console.Write($"  C{combi.Number} ");
                foreach (var m in combi.Modes)
                {
                    Console.Write($"{m} ");
                }

                Console.WriteLine();
            }

            Console.WriteLine($"Speed: {activeSensor.SpeedPid.Pid1} {activeSensor.SpeedPid.Pid2} {activeSensor.SpeedPid.Pid3} {activeSensor.SpeedPid.Pid4}");
            Console.WriteLine($"Position: {activeSensor.PositionPid.Pid1} {activeSensor.PositionPid.Pid2} {activeSensor.PositionPid.Pid3} {activeSensor.PositionPid.Pid4}");
        }
        else
        {
            var motor = (PassiveMotor)brick.GetMotor((SensorPort)i);
            Console.WriteLine(motor.IsConnected);
        }
    }

    Console.WriteLine("Press a key to continue");
}

void DisplayConnectionDisconnection()
{
    while (!Console.KeyAvailable)
    {
        Console.Clear();
        Console.CursorTop = 0;
        for (int i = 0; i < 4; i++)
        {
            Console.CursorLeft = 0;
            SensorType sensor = brick.GetSensorType((SensorPort)i);
            Console.Write($"Port: {i} {(Brick.IsMotor(sensor) ? "Sensor" : "Motor")} type: {sensor} Connected: ");
            if (sensor != SensorType.None)
            {
                if (Brick.IsMotor(sensor))
                {
                    if (Brick.IsActiveSensor(sensor))
                    {
                        var motor = (ActiveMotor)brick.GetMotor((SensorPort)i);
                        Console.WriteLine($"{motor.IsConnected}");
                    }
                    else
                    {
                        var motor = (PassiveMotor)brick.GetMotor((SensorPort)i);
                        Console.WriteLine(motor.IsConnected);
                    }
                }
                else
                {
                    if (Brick.IsActiveSensor(sensor))
                    {
                        var motor = (ActiveSensor)brick.GetSensor((SensorPort)i);
                        Console.WriteLine(motor.IsConnected);
                    }
                    else
                    {
                        var motor = (Sensor)brick.GetSensor((SensorPort)i);
                        Console.WriteLine(motor.IsConnected);
                    }
                }
            }
        }

        Thread.Sleep(100);
    }

    Console.ReadKey();
}

void MoveMotorsAndBackToPosition()
{
    Console.Clear();
    Console.WriteLine("Press a key to continue");
    brick.WaitForSensorToConnect(SensorPort.PortA);
    brick.WaitForSensorToConnect(SensorPort.PortD);
    var active = (ActiveMotor)brick.GetMotor(SensorPort.PortA);
    var active2 = (ActiveMotor)brick.GetMotor(SensorPort.PortD);
    active.Start(50);
    active2.Start(50);
    // Make sure you have an active motor plug in the port A and D
    while (!Console.KeyAvailable)
    {
        Console.CursorTop = 1;
        Console.CursorLeft = 0;
        Console.WriteLine($"Absolute: {active.AbsolutePosition}     ");
        Console.WriteLine($"Position: {active.Position}     ");
        Console.WriteLine($"Speed: {active.Speed}     ");
        Console.WriteLine();
        Console.WriteLine($"Absolute: {active2.AbsolutePosition}     ");
        Console.WriteLine($"Position: {active2.Position}     ");
        Console.WriteLine($"Speed: {active2.Speed}     ");
    }

    active.Stop();
    active2.Stop();

    Console.ReadKey();
    Console.Clear();
    Console.WriteLine("Driving back both motors to position 0, one after the other, both blocking");
    Console.WriteLine("Press a key to continue");

    active.TargetSpeed = 100;
    active2.TargetSpeed = 100;
    active.MoveToPosition(0, true);
    active2.MoveToPosition(0, true);
}

void DriveMotors()
{
    // MAke sure you have an active motor on port A
    brick.WaitForSensorToConnect(SensorPort.PortA);
    var active = (ActiveMotor)brick.GetMotor(SensorPort.PortA);
    active.TargetSpeed = 70;
    Console.WriteLine("Moving motor to position 0");
    active.MoveToPosition(0, true);
    Console.WriteLine("Moving motor to position 3600 (10 turns)");
    active.MoveToPosition(3600, true);
    Console.WriteLine("Moving motor to position -3600 (so 20 turns the other way");
    active.MoveToPosition(-3600, true);
    Console.WriteLine("Moving motor to absolute position 0, should rotate by 90°");
    active.MoveToAbsolutePosition(0, PositionWay.Shortest, true);
    Console.WriteLine("Moving motor to position 90");
    active.MoveToAbsolutePosition(90, PositionWay.Shortest, true);
    Console.WriteLine("Moving motor to position 179");
    active.MoveToAbsolutePosition(179, PositionWay.Shortest, true);
    Console.WriteLine("Moving motor to position -180");
    active.MoveToAbsolutePosition(-180, PositionWay.Shortest, true);
}

void ReadColorDistance()
{
    brick.WaitForSensorToConnect(SensorPort.PortC);

    var colorSensor = (ColorAndDistanceSensor)brick.GetActiveSensor(SensorPort.PortC);
    while (!Console.KeyAvailable)
    {
        var colorRead = colorSensor.GetColor();
        Console.WriteLine($"Color:     {colorRead}");
        var relected = colorSensor.GetReflectedLight();
        Console.WriteLine($"Reflected: {relected}");
        var ambiant = colorSensor.GetAmbiantLight();
        Console.WriteLine($"Ambiant:   {ambiant}");
        var distance = colorSensor.GetDistance();
        Console.WriteLine($"Distance: {distance}");
        var counter = colorSensor.GetCounter();
        Console.WriteLine($"Counter:  {counter}");
        Thread.Sleep(200);
    }
}

void RunTrainMotor()
{
    brick.WaitForSensorToConnect(SensorPort.PortB);
    var train = (PassiveMotor)brick.GetMotor(SensorPort.PortB);
    Console.WriteLine("This will run the motor for 20 secondes incrementing the PWM");
    train.SetPowerLimit(1.0);
    train.Start();
    for (int i = 0; i < 100; i++)
    {
        train.SetSpeed(i);
        Thread.Sleep(250);
    }

    Console.WriteLine("Stop the train for 2 seconds");
    train.Stop();
    Thread.Sleep(2000);
    Console.WriteLine("Full speed backward for 2 seconds");
    train.Start(-100);
    Thread.Sleep(2000);
    Console.WriteLine("Full speed foreward for 2 seconds");
    train.Start(100);
    Thread.Sleep(2000);
    Console.WriteLine("Stop the train");
    train.Stop();
}