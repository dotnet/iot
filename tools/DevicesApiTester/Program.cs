// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using DeviceApiTester.Infrastructure;

namespace DeviceApiTester
{
    internal class Program : CommandLineProgram
    {
        private static int Main(string[] args)
        {
            return new Program().Run(args);
        }
    }
}
