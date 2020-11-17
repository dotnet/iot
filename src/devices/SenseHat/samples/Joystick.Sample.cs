// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Iot.Device.SenseHat.Samples
{
    internal class Joystick
    {
        public static void Run()
        {
            using SenseHatJoystick j = new();
            while (true)
            {
                char state = j.GetState() switch
                {
                    JoystickState.Up => 'U',
                    JoystickState.Down => 'D',
                    JoystickState.Left => 'L',
                    JoystickState.Right => 'R',
                    JoystickState.Button => '!',
                    _ => '@',
                };

                Console.Write(state);
            }
        }
    }
}
