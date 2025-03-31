// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading;
using Iot.Device.Arduino;

namespace ArduinoCsCompiler.Runtime
{
    [ArduinoReplacement(typeof(ThreadPool), IncludingPrivates = true)]
    internal sealed class MiniThreadPool
    {
        public static bool UseWindowsThreadPool
        {
            [ArduinoImplementation]
            get
            {
                return false;
            }
        }

        /// <summary>
        /// See https://github.com/dotnet/runtime/blob/24e7a4a1a101d91b6666dc6f44137574246fdd9c/src/coreclr/vm/comthreadpool.cpp for an explanation of how this function is used
        /// from the ThreadPool cctor. TODO: Still need to decide whether using the PortableThreadPool is the right thing to do
        /// </summary>
        /// <param name="configVariableIndex">Index of the next variable to read</param>
        /// <param name="configValue">The value of the variable obtained</param>
        /// <param name="isBoolean">True if the obtained value is boolean</param>
        /// <param name="appContextConfigName">Name of the parameter (c-string)</param>
        /// <returns>The next index to query or -1 if there are no more parameters (note: only return -1 if the current parameter is already out of bounds)</returns>
        [ArduinoImplementation]
        public static unsafe int GetNextConfigUInt32Value(
            int configVariableIndex,
            out uint configValue,
            out bool isBoolean,
            out char* appContextConfigName)
        {
            if (configVariableIndex == 0)
            {
                configValue = 1;
                isBoolean = true;
                appContextConfigName = null;
                return 1;
            }

            configValue = 0;
            isBoolean = false;
            appContextConfigName = null;
            return -1;
        }
    }
}
