// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace DeviceApiTester.Infrastructure
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class ImplementationTypeAttribute : Attribute
    {
        public ImplementationTypeAttribute(Type implementationType)
        {
            ImplementationType = implementationType;
        }

        public Type ImplementationType { get; }
    }
}
