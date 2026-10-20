// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.FtCommon
{
    /// <summary>
    /// Provides FIFO access and persistent configuration operations for FT-X I2C slave devices.
    /// </summary>
    internal interface IFtXSeriesI2cSlaveTransport : IFtI2cSlaveTransport
    {
        /// <summary>
        /// Reads the stored FT-X configuration and USB descriptor strings without programming memory.
        /// </summary>
        /// <returns>A configuration snapshot independent of subsequent reads.</returns>
        FtXSeriesConfiguration ReadConfiguration();

        /// <summary>
        /// Programs the supplied configuration into persistent MTP memory.
        /// </summary>
        /// <param name="configuration">The complete FT-X configuration and USB descriptor strings to program.</param>
        /// <remarks>This consumes programming endurance and does not reload or verify the configuration.</remarks>
        void ProgramConfiguration(FtXSeriesConfiguration configuration);
    }
}
