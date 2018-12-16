// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Threading.Tasks;

namespace Iot.Device
{
    public interface IBmp280
    {
        void Dispose();
        ushort Read16(byte register);
        uint Read24(byte register);
        byte Read8(byte register);
        Task<double> ReadAltitudeAsync(double seaLevelPressure);
        Bmp280.PowerMode ReadPowerMode();
        Task<double> ReadPressureAsync();
        Bmp280.Sampling ReadPressureSampling();
        Task<double> ReadTemperatureAsync();
        Bmp280.Sampling ReadTemperatureSampling();
        void SetModeForced();
        void SetModeNormal();
        void SetModeSleep();
        void SetPressureSampling(Bmp280.Sampling sampling);
        void SetTemperatureSampling(Bmp280.Sampling sampling);
    }
}