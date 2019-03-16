// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace force_sensitive_resistor
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello Fsr408 capacitor Sample!");
            // Use this sample when using ADC for reading
/*            FsrWithAdcSample fsrWithAdc = new FsrWithAdcSample();
            fsrWithAdc.StartReading();
*/
            // Use this sample if using capacitor for reading
            FsrWithCapacitorSample fsrWithCapacitor = new FsrWithCapacitorSample();
            fsrWithCapacitor.StartReading();

        }
    }
}
