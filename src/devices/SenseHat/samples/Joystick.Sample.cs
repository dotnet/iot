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
            using (var j = new SenseHatJoystick())
            {
                while (true)
                {
                    j.Read();

                    Console.Clear();
                    if (j.HoldingUp)
                    {
                        Console.Write("U");
                    }

                    if (j.HoldingDown)
                    {
                        Console.Write("D");
                    }

                    if (j.HoldingLeft)
                    {
                        Console.Write("L");
                    }

                    if (j.HoldingRight)
                    {
                        Console.Write("R");
                    }

                    if (j.HoldingButton)
                    {
                        Console.Write("!");
                    }
                }
            }
        }
    }
}
