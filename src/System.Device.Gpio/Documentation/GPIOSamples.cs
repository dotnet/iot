// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace System.Devices.Gpio
{
    class GPIOSamples_Basic
    {
        // Blink - Hello World in GPIO terms
        public void BlinkBasic()
        {
            using (GPIOController controller = new GPIOController()) // BOARD numbering by default
            using (GPIOPin ledPin = controller.OpenPin(7, GPIOPinMode.Output))
            {
                while(true)
                {
                    ledPin.Write(true);
                    Thread.Sleep(500);
                    ledPin.Write(false);
                    Thread.Sleep(500);
                }
            }
        }

        // Use pullup/pulldown with basic reads
        public void PUD()
        {
            using (GPIOController controller = new GPIOController()) // BOARD numbering by default
            using (GPIOPin pullUpPin = controller.OpenPin(7, GPIOPinMode.Input))
            {
                if (pullUpPin.IsPinModeSupported(GPIOPinMode.Pull_Up))
                {
                    pullUpPin.PinMode = GPIOPinMode.Pull_Up | GPIOPinMode.Input;
                }

                while (true)
                {
                    Console.WriteLine(pullUpPin.Read());
                    Thread.Sleep(1000);
                }
            }
        }
    }

    class GPIOSamples_Intermediate
    {
        // Poll a read until the pin is set to HIGH/on
        public void PollRead()
        {
            using (GPIOController controller = new GPIOController()) // BOARD numbering by default
            using (GPIOPin pollPin = controller.OpenPin(7, GPIOPinMode.Input))
            {
                pollPin.ReadWait(new TimeSpan(0, 1, 0));
            }
        }

        // Listen for a pin to be set to LOW and print a message when it happens

        // Set up a Debounce timeout to ignore high frequency repeated events
    }

    class GPIOSamples_Advanced
    {
        // Control the PWM
        public void PWM()
        {
            using (GPIOController controller = new GPIOController()) // BOARD numbering by default
            using (GPIOPin pwmPin = controller.OpenPin(12, GPIOPinMode.Output | GPIOPinMode.PWM))
            {
                pwmPin.PWMMode = PWMMode.MARK_SPACE;
                pwmPin.PWMRange = 100; // Splits up the max frequency into 100 equal segments 
                pwmPin.PWMFrequency = 10 * 1000 * 1000 ; // Sets the max frequency (i.e. at 100% duty cycle) to 10 mhz

                // Cycle from 0 to the max PWM frequency
                for (int i = 0; i < pwmPin.PWMRange; i++)
                {
                    pwmPin.WritePWM(i);
                    pwmPin.Sleep(100);
                }
            }
        }
    }

    class GPIOSamples_MultiPin
    {

    }
}
