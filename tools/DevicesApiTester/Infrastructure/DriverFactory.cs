// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Linq;

namespace DeviceApiTester.Infrastructure
{
    public static class DriverFactory
    {
        public static InstanceType? CreateFromEnum<InstanceType, EnumType>(EnumType driver, params object[] parameters)
            where InstanceType : class
        {
            try
            {
                string name = driver?.ToString() ?? "Foo";
                ImplementationTypeAttribute creatorAttribute = typeof(EnumType)
                    .GetMember(name)[0]
                    .GetCustomAttributes(typeof(ImplementationTypeAttribute), false)
                    .OfType<ImplementationTypeAttribute>()
                    .FirstOrDefault()
                    ?? throw new InvalidOperationException($"The {typeof(EnumType).Name}.{driver} enum value is not attributed with an {nameof(ImplementationTypeAttribute)}.");

                if (creatorAttribute.ImplementationType is null)
                {
                    return null;
                }

                return Activator.CreateInstance(creatorAttribute.ImplementationType, parameters) as InstanceType;
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                {
                    throw ex.InnerException;
                }

                throw;
            }
        }
    }
}
