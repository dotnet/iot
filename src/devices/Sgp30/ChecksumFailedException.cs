using System;

namespace Iot.Device.Sgp30
{
    /// <summary>
    /// Exception raised where an SGP30 Checksum Validation fails during a device read operation.
    /// </summary>
    public class ChecksumFailedException : Exception
    {
    }
}
