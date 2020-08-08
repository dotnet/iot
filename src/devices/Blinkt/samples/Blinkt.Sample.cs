// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Iot.Device.Blinkt.Samples
{
    /// <summary>
    /// Samples for Blinkt
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Main entry point
        /// </summary>
        public static void Main(string[] args)
        {
            List<TypeInfo> sampleTypes = Assembly.GetExecutingAssembly().DefinedTypes
                .Where(t => t.DeclaredMethods.Any(m => m.Name == "Run")).ToList();

            while (true)
            {
                Console.WriteLine("Select a sample to run:\n\n");

                foreach ((int index, TypeInfo sampleType) in sampleTypes.WithIndex())
                {
                    Console.WriteLine($"[{index}] {sampleType.Name}");
                }

                Console.WriteLine("\nOr to quit at anytime, press Enter.");

                var input = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(input))
                {
                    break;
                }

                if (int.TryParse(input, out int sampleIndex))
                {
                    if (sampleIndex >= 0 && sampleIndex < sampleTypes.Count)
                    {
                        TypeInfo sampleType = sampleTypes[sampleIndex];

                        MethodInfo runMethod = sampleType.GetDeclaredMethod("Run");

                        var obj = Activator.CreateInstance(sampleTypes[sampleIndex]);

                        try
                        {
                            Console.WriteLine($"Running {sampleType.Name}");
                            _ = runMethod.Invoke(obj, null);
                            Console.ReadKey(false);
                        }
                        finally
                        {
                            if (obj is IDisposable disposableObj)
                            {
                                disposableObj.Dispose();
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("Not a valid index\n");
                    }
                }
                else
                {
                    Console.WriteLine("Not a valid number\n");
                }
            }

        }
    }
}
