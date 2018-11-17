using System;
using System.Device.Gpio;
using System.Device.Gpio.Drivers;
using System.Linq;

namespace GpioRunner
{
    public static class DriverFactory
    {
        public static GpioDriver CreateFromEnum(DriverType driver)
        {
            try
            {
                var creatorAttribute = typeof(DriverType)
                    .GetMember(driver.ToString())?[0]
                    .GetCustomAttributes(typeof(ImplementationTypeAttribute), false)
                    .OfType<ImplementationTypeAttribute>()
                    .FirstOrDefault()
                    ?? throw new InvalidOperationException($"The {nameof(DriverType)}.{driver} enum value is not attributed with a {nameof(ImplementationTypeAttribute)}");

                return creatorAttribute.ImplementationType == null
                    ? null
                    : (GpioDriver)Activator.CreateInstance(creatorAttribute.ImplementationType);
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

    public enum DriverType
    {
        [ImplementationType(null)]
        Default,

        [ImplementationType(typeof(Windows10Driver))]
        Windows,

        [ImplementationType(typeof(UnixDriver))]
        UnixSysFs,

        [ImplementationType(typeof(HummingBoardDriver))]
        HummingBoard,

        [ImplementationType(typeof(RaspberryPi3Driver))]
        RPi3,
    }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class ImplementationTypeAttribute : Attribute
    {
        public ImplementationTypeAttribute(Type gpioDriverType)
        {
            ImplementationType = gpioDriverType;
        }

        public Type ImplementationType { get; }
    }
}
