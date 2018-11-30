using System;
using System.Linq;

namespace DeviceApiTester.Infrastructure
{
    public static class DriverFactory
    {
        public static InstanceType CreateFromEnum<InstanceType, EnumType>(EnumType driver, params object[] parameters)
            where InstanceType : class
        {
            try
            {
                var creatorAttribute = typeof(EnumType)
                    .GetMember(driver.ToString())?[0]
                    .GetCustomAttributes(typeof(ImplementationTypeAttribute), false)
                    .OfType<ImplementationTypeAttribute>()
                    .FirstOrDefault()
                    ?? throw new InvalidOperationException($"The {typeof(EnumType).Name}.{driver} enum value is not attributed with an {nameof(ImplementationTypeAttribute)}");

                return creatorAttribute.ImplementationType == null
                    ? null
                    : (InstanceType)Activator.CreateInstance(creatorAttribute.ImplementationType, parameters);
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
