// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArduinoCsCompiler.Runtime
{
    [ArduinoReplacement(typeof(System.IO.Ports.SerialPort))]
    internal class MiniSerialPort
    {
        [ArduinoImplementation]
        public static string[] GetPortNames()
        {
            // Todo: This is not yet so clearly defined, because ArduinoNativeBoard.TryFindBoard() ignores the port name list.
            // However, we should still somehow allow access to the real serial ports, other than trough the console.
            return new string[]
            {
                "/dev/tty0"
            };
        }
    }
}
