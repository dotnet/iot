// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using Iot.Device.Nmea0183;
using Iot.Device.Nmea0183.Sentences;
using Iot.Device.Gps;

namespace Iot.Device.Gps.NeoM8Samples
{
    class Program
    {
        static void Main(string[] args)
        {
            using (NeoM8 neoM8 = new NeoM8("/dev/ttyS0"))
            {
                bool gotRmc = false;
                while (!gotRmc)
                {
                    TalkerSentence sentence = neoM8.Read();

                    object typed = sentence.TryGetTypedValue();
                    if (typed == null)
                    {
                        Console.WriteLine($"Sentence identifier `{sentence.Id}` is not known.");
                    }
                    else if (typed is RecommendedMinimumNavigationInformation rmc)
                    {
                        gotRmc = true;

                        if (rmc.LatitudeDegrees.HasValue && rmc.LongitudeDegrees.HasValue)
                        {
                            Console.WriteLine($"Your location: {rmc.LatitudeDegrees.Value:0.00000}, {rmc.LongitudeDegrees.Value:0.00000}");
                        }
                        else
                        {
                            Console.WriteLine($"You cannot be located.");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Sentence of type `{typed.GetType().FullName}` not handled.");
                    }
                }
            }
        }
    }
}
