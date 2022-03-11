// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Globalization;
using System.Reflection;
using Iot.Device.Arduino;

namespace ArduinoCsCompiler.Runtime
{
    [ArduinoReplacement(typeof(System.Activator), IncludingPrivates = true)]
    internal static class MiniActivator
    {
        [ArduinoImplementation("ActivatorCreateInstance")]
        public static object? CreateInstance(Type type, BindingFlags bindingAttr, Binder? binder, object?[]? args, CultureInfo? culture, object?[]? activationAttributes)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation]
        public static object? CreateInstance(Type type, bool nonPublic, bool wrapExceptions)
        {
            return CreateInstance(type, nonPublic ? BindingFlags.CreateInstance | BindingFlags.NonPublic | BindingFlags.Public : BindingFlags.CreateInstance | BindingFlags.Public,
                null, new object?[0], CultureInfo.CurrentCulture, null);
        }

        [ArduinoImplementation(IgnoreGenericTypes = true)]
        public static T? CreateInstance<T>()
        {
            return (T?)CreateInstance(typeof(T), BindingFlags.Default, null, new object?[0], CultureInfo.CurrentCulture, null);
        }
    }
}
