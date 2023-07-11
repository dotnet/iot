// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Linq;
using System.Threading;
using Iot.Device.KeyMatrix;

Console.WriteLine("Please enter your output pins separated by a coma. For example: 27,22,25,6");
var line = Console.ReadLine();
IEnumerable<int> outputs = string.IsNullOrEmpty(line) ? new int[] { 27, 22, 25, 6 } : line.Split(',').Select(m => int.Parse(m));
Console.WriteLine("Please enter your input pins separated by a coma. For example: 17,23,24,5");
line = Console.ReadLine();
IEnumerable<int> inputs = string.IsNullOrEmpty(line) ? new int[] { 17, 23, 24, 5 } : line.Split(',').Select(m => int.Parse(m));
Console.WriteLine("Please enter the scanning interval in milliseconds. For example: 15");
line = Console.ReadLine();
int interval = int.TryParse(line, out int i) ? i : 15;
Console.WriteLine("Please enter the number of keys you want to read individually events. For example: 20");
line = Console.ReadLine();
int count = int.TryParse(line, out int c) ? c : 20;

// initialize keyboard
KeyMatrix mk = new KeyMatrix(outputs, inputs, TimeSpan.FromMilliseconds(interval));

// define the cancellation token.
CancellationTokenSource source = new CancellationTokenSource();
CancellationToken token = source.Token;

// read key events
for (int n = 0; n < count; n++)
{
    Console.WriteLine($"Waiting for matrix keyboard event... {n}/{count}");
    KeyMatrixEvent? key = mk.ReadKey();
    if (key is not object)
    {
        Console.WriteLine("No key pressed");
        continue;
    }

    ShowKeyMatrixEvent(mk, key);
}

Console.WriteLine("This will now start listening to events and display them. Press a key to finish.");
mk.KeyEvent += KeyMatrixEventReceived;
mk.StartListeningKeyEvent();
while (!Console.KeyAvailable)
{
    Thread.Sleep(1);
}

mk.StopListeningKeyEvent();

// dispose
Console.WriteLine("Dispose after 2 seconds...");
Thread.Sleep(2000);
mk.Dispose();

void KeyMatrixEventReceived(object sender, KeyMatrixEvent keyMatrixEvent)
{
    ShowKeyMatrixEvent((KeyMatrix)sender, keyMatrixEvent);
}

void ShowKeyMatrixEvent(KeyMatrix sender, KeyMatrixEvent pinValueChangedEventArgs)
{
    Console.WriteLine($"{DateTime.Now:yyyy/MM/dd HH:mm:ss.fff} {pinValueChangedEventArgs.Output}, {pinValueChangedEventArgs.Input}, {pinValueChangedEventArgs.EventType}");
    Console.WriteLine();

    // print keyboard status
    for (int r = 0; r < sender.OutputPins.Count(); r++)
    {
        ReadOnlySpan<PinValue> rv = sender[r];
        for (int c = 0; c < sender.InputPins.Count(); c++)
        {
            Console.Write(rv[c] == PinValue.Low ? " ." : " #");
        }

        Console.WriteLine();
    }
}
