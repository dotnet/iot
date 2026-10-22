// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Threading;
using Xunit;

namespace Iot.Device.Bmp3xx.Tests
{
    /// <summary>
    /// Tests of the simulated chip itself, so a failing binding test can't be blamed on the simulation.
    /// They talk to it through the plain <see cref="System.Device.I2c.I2cDevice"/> API, as the binding does.
    /// </summary>
    public class SimulatedBmp3xxTests
    {
        private static readonly byte[] Calibration = Convert.FromHexString("3f6b3849f6ca0092f623007e62e575f3f6c54213c4");

        [Fact]
        public void BurstRead_CrossesRegisters()
        {
            using var chip = new SimulatedBmp3xx(0x60, Calibration);
            byte[] data = new byte[6];
            byte[] calibration = new byte[21];

            chip.WriteRead(new byte[] { 0x04 }, data);
            chip.WriteRead(new byte[] { 0x31 }, calibration);

            // Data registers after reset: 0x800000 for pressure and temperature, least significant byte first.
            Assert.Equal(new byte[] { 0x00, 0x00, 0x80, 0x00, 0x00, 0x80 }, data);
            Assert.Equal(Calibration, calibration);
        }

        [Fact]
        public void Write_IsRegisterValuePairs()
        {
            using var chip = new SimulatedBmp3xx(0x60, Calibration);

            chip.Write(new byte[] { 0x1C, 0x05, 0x1F, 0x04 });

            Assert.Equal(0x05, chip.GetRegister(0x1C));
            Assert.Equal(0x04, chip.GetRegister(0x1F));
            Assert.Equal(new (byte, byte)[] { (0x1C, 0x05), (0x1F, 0x04) }, chip.WriteLog);
        }

        [Fact]
        public void SoftReset_RestoresResetValues_KeepsCalibration_AndCountsResets()
        {
            using var chip = new SimulatedBmp3xx(0x60, Calibration);
            chip.Write(new byte[] { 0x1C, 0x05 });

            chip.Write(new byte[] { 0x7E, 0xB6 });

            Assert.Equal(1, chip.ResetCount);
            Assert.Equal(0x02, chip.GetRegister(0x1C)); // OSR reset value
            Assert.Equal(0x01, chip.GetRegister(0x10)); // por_detected
            Assert.Equal(Calibration[0], chip.GetRegister(0x31));
        }

        [Fact]
        public void ForcedMode_MeasuresOnce_AndReturnsToSleep()
        {
            using var chip = new SimulatedBmp3xx(0x60, Calibration);
            chip.RawPressure = 0x6ACD3B;
            chip.RawTemperature = 0x7F4A12;

            chip.Write(new byte[] { 0x1B, 0x13 }); // press_en | temp_en | forced

            Assert.Equal(0x70, chip.GetRegister(0x03)); // cmd_rdy | drdy_press | drdy_temp
            Assert.Equal(0x03, chip.GetRegister(0x1B)); // mode back to sleep, sensors still enabled
            byte[] data = new byte[6];
            chip.WriteRead(new byte[] { 0x04 }, data);
            Assert.Equal(new byte[] { 0x3B, 0xCD, 0x6A, 0x12, 0x4A, 0x7F }, data);
            Assert.Equal(0x10, chip.GetRegister(0x03)); // reading the data cleared both data-ready flags
        }

        [Fact]
        public void ModeWrite_RightAfterLeavingNormalMode_IsIgnored()
        {
            using var chip = new SimulatedBmp3xx(0x60, Calibration);
            chip.Write(new byte[] { 0x1B, 0x33 }); // normal mode

            chip.Write(new byte[] { 0x1B, 0x03 }); // sleep: the running measurement still has to finish
            chip.Write(new byte[] { 0x1B, 0x33 }); // normal again, too early

            Assert.Equal(0x03, chip.GetRegister(0x1B));
            Assert.Equal(0x00, chip.GetRegister(0x02)); // no error is reported
        }

        [Fact]
        public void ModeWrite_AfterTheRunningMeasurement_IsAccepted()
        {
            using var chip = new SimulatedBmp3xx(0x60, Calibration);
            chip.Write(new byte[] { 0x1B, 0x33 });
            chip.Write(new byte[] { 0x1B, 0x03 });

            Thread.Sleep(20); // x1/x1 with the reset value of OSR (x4 pressure) takes 10.9 ms

            chip.Write(new byte[] { 0x1B, 0x33 });
            Assert.Equal(0x33, chip.GetRegister(0x1B));
        }

        [Fact]
        public void ModeWrite_AfterOversamplingWasRaisedAndLowered_WaitsForTheLongerMeasurement()
        {
            using var chip = new SimulatedBmp3xx(0x60, Calibration);
            chip.Write(new byte[] { 0x1C, 0x00 }); // x1/x1
            chip.Write(new byte[] { 0x1B, 0x33 }); // normal mode
            chip.Write(new byte[] { 0x1C, 0x05 }); // x32 pressure: about 67 ms per measurement
            chip.Write(new byte[] { 0x1C, 0x00 }); // back to x1/x1 while that measurement may still run
            chip.Write(new byte[] { 0x1B, 0x03 }); // sleep

            Thread.Sleep(20); // longer than an x1/x1 measurement (4.8 ms), shorter than the x32 one

            chip.Write(new byte[] { 0x1B, 0x33 });
            Assert.Equal(0x03, chip.GetRegister(0x1B));
        }

        [Fact]
        public void NormalMode_WithConfigurationError_ReturnsToSleep()
        {
            using var chip = new SimulatedBmp3xx(0x60, Calibration) { ConfigurationErrorOnNormalMode = true };

            chip.Write(new byte[] { 0x1B, 0x33 });

            Assert.Equal(0x03, chip.GetRegister(0x1B));
            Assert.Equal(0x04, chip.GetRegister(0x02));
        }

        [Fact]
        public void ReadingErrorRegister_ClearsCommandAndConfigurationErrors()
        {
            using var chip = new SimulatedBmp3xx(0x60, Calibration) { ConfigurationErrorOnNormalMode = true, SoftResetFails = true };
            chip.Write(new byte[] { 0x1B, 0x33 }); // normal mode -> conf_err
            chip.Write(new byte[] { 0x7E, 0xB6 }); // failing soft reset -> cmd_err
            byte[] error = new byte[1];

            chip.WriteRead(new byte[] { 0x02 }, error);

            Assert.Equal(0x06, error[0]);
            Assert.Equal(0x00, chip.GetRegister(0x02));
            Assert.Equal(0, chip.ResetCount);
        }
    }
}
