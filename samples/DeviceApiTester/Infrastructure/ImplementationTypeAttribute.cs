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
