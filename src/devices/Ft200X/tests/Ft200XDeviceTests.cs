// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Iot.Device.FtCommon;
using Xunit;

namespace Iot.Device.Ft200X.Tests
{
    public class Ft200XDeviceTests
    {
        [Theory]
        [InlineData("FT200X USB I2C", FtDeviceType.FtXSeries, true)]
        [InlineData("ft200x", FtDeviceType.FtXSeries, true)]
        [InlineData("FT201X USB I2C", FtDeviceType.FtXSeries, false)]
        [InlineData("FT230X Basic UART", FtDeviceType.FtXSeries, false)]
        [InlineData("Custom adapter", FtDeviceType.FtXSeries, false)]
        [InlineData("FT200X USB I2C", FtDeviceType.Ft232H, false)]
        public void DiscoveryDoesNotSelectAllDevicesSharingTheVidPid(string description, FtDeviceType type, bool expected)
        {
            using var descriptor = Descriptor(description, type);
            Assert.Equal(expected, Ft200XDevice.IsFt200X(descriptor));
        }

        [Fact]
        public void RejectsNullDescriptorBeforeAccessingNativeLibrary()
        {
            Assert.Throws<ArgumentNullException>(() => new Ft200XDevice(null!));
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("ABC\0DEF")]
        public void RequiresASerialNumberSuitableForNativeSelection(string serial)
        {
            using var descriptor = Descriptor();
            descriptor.SerialNumber = serial;
            Assert.Throws<ArgumentException>(() => new Ft200XDevice(descriptor));
        }

        [Theory]
        [InlineData(0, 1000)]
        [InlineData(-1, 1000)]
        [InlineData(1000, 0)]
        [InlineData(1000, -1)]
        public void RejectsInvalidTimeoutsBeforeAccessingNativeLibrary(int readTimeout, int writeTimeout)
        {
            using var descriptor = Descriptor();
            Assert.Throws<ArgumentOutOfRangeException>(() => new Ft200XDevice(descriptor, readTimeout, writeTimeout));
        }

        [Fact]
        public void RejectsNonXSeriesDevices()
        {
            using var descriptor = Descriptor(type: FtDeviceType.Ft232H);
            Assert.Throws<ArgumentException>(() => new Ft200XDevice(descriptor));
        }

        [Theory]
        [InlineData(1)]
        [InlineData(512)]
        [InlineData(513)]
        [InlineData(65536)]
        public void WritePassesOnlyPayloadBytesWithoutMasterCommands(int length)
        {
            var transport = new FakeTransport();
            using var device = new Ft200XDevice(transport);
            byte[] payload = Enumerable.Range(0, length).Select(i => (byte)i).ToArray();
            device.Write(payload);
            Assert.Equal(payload, transport.Written);
            Assert.Equal(1, transport.WriteCalls);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(2)]
        [InlineData(4)]
        public void ReadReturnsTheActualCountIncludingTimeouts(int count)
        {
            var transport = new FakeTransport { Received = new byte[] { 1, 2, 3, 4 }.Take(count).ToArray() };
            using var device = new Ft200XDevice(transport);
            byte[] buffer = Enumerable.Repeat((byte)0xFF, 4).ToArray();
            Assert.Equal(count, device.Read(buffer));
            Assert.Equal(transport.Received, buffer.Take(count));
            Assert.All(buffer.Skip(count), value => Assert.Equal(0xFF, value));
            Assert.Equal(1, transport.ReadCalls);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(5)]
        public void RejectsInvalidReadCounts(int count)
        {
            var transport = new FakeTransport { ReadCount = count };
            using var device = new Ft200XDevice(transport);
            Assert.Throws<IOException>(() => device.Read(new byte[4]));
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        [InlineData(2)]
        [InlineData(5)]
        public void ShortOrInvalidWritesThrowWithoutRetrying(int count)
        {
            var transport = new FakeTransport { WriteCount = count };
            using var device = new Ft200XDevice(transport);
            IOException error = Assert.Throws<IOException>(() => device.Write(new byte[] { 1, 2, 3, 4 }));
            Assert.Contains($"{count} of 4", error.Message);
            Assert.Equal(1, transport.WriteCalls);
        }

        [Fact]
        public void EmptyTransfersDoNotAccessTransport()
        {
            var transport = new FakeTransport();
            using var device = new Ft200XDevice(transport);
            device.Write(Array.Empty<byte>());
            Assert.Equal(0, device.Read(Array.Empty<byte>()));
            Assert.Equal(0, transport.ReadCalls);
            Assert.Equal(0, transport.WriteCalls);
        }

        [Fact]
        public void BytesAvailableReportsTheUsbReceiveQueueWithoutReadingIt()
        {
            var transport = new FakeTransport { QueuedBytes = 123 };
            using var device = new Ft200XDevice(transport);
            Assert.Equal(123u, device.BytesAvailable);
            Assert.Equal(0, transport.ReadCalls);
            Assert.Equal(0, transport.WriteCalls);
        }

        [Fact]
        public void NativeFailuresPropagateWithoutRetriesOrSuccessFallbacks()
        {
            var error = new IOException("Simulated D2XX failure.");
            var transport = new FakeTransport { Error = error };
            using var device = new Ft200XDevice(transport);
            Assert.Same(error, Assert.Throws<IOException>(() => device.Read(new byte[1])));
            Assert.Same(error, Assert.Throws<IOException>(() => device.Write(new byte[1])));
            Assert.Same(error, Assert.Throws<IOException>(() => device.BytesAvailable));
            Assert.Equal(1, transport.ReadCalls);
            Assert.Equal(1, transport.WriteCalls);
        }

        [Fact]
        public void DisposalIsIdempotentAndAllOperationsRejectDisposedDevices()
        {
            var transport = new FakeTransport();
            var device = new Ft200XDevice(transport);
            device.Dispose();
            device.Dispose();
            Assert.Equal(1, transport.DisposeCalls);
            Assert.Throws<ObjectDisposedException>(() => device.Read(new byte[1]));
            Assert.Throws<ObjectDisposedException>(() => device.Write(new byte[1]));
            Assert.Throws<ObjectDisposedException>(() => device.Read(Array.Empty<byte>()));
            Assert.Throws<ObjectDisposedException>(() => device.Write(Array.Empty<byte>()));
            Assert.Throws<ObjectDisposedException>(() => device.BytesAvailable);
            Assert.Equal(0, transport.ReadCalls);
            Assert.Equal(0, transport.WriteCalls);
        }

        [Fact]
        public async Task DisposalWaitsForAnOutstandingRead()
        {
            var transport = new FakeTransport();
            using var device = new Ft200XDevice(transport);
            using var reading = new ManualResetEventSlim();
            using var release = new ManualResetEventSlim();
            using var disposing = new ManualResetEventSlim();
            transport.OnRead = () =>
            {
                reading.Set();
                Assert.True(release.Wait(TimeSpan.FromSeconds(5)));
                Assert.Equal(0, transport.DisposeCalls);
            };

            Task read = Task.Run(() => device.Read(new byte[1]));
            Task? dispose = null;
            try
            {
                Assert.True(reading.Wait(TimeSpan.FromSeconds(5)));
                dispose = Task.Run(() =>
                {
                    disposing.Set();
                    device.Dispose();
                });
                Assert.True(disposing.Wait(TimeSpan.FromSeconds(5)));
                Assert.Equal(0, transport.DisposeCalls);
            }
            finally
            {
                release.Set();
                await read;
                if (dispose is not null)
                {
                    await dispose;
                }
            }

            Assert.Equal(1, transport.DisposeCalls);
        }

        [Fact]
        public void EepromLayoutMatchesTheD2xxNativeAbi()
        {
            Assert.Equal(16, Marshal.SizeOf<FtEepromHeader>());
            Assert.Equal(56, Marshal.SizeOf<FtXSeriesEeprom>());
            Assert.Equal(10, Marshal.OffsetOf<FtEepromHeader>(nameof(FtEepromHeader.MaxPower)).ToInt32());
            Assert.Equal(16, Marshal.OffsetOf<FtXSeriesEeprom>(nameof(FtXSeriesEeprom.AcSlowSlew)).ToInt32());
            Assert.Equal(40, Marshal.OffsetOf<FtXSeriesEeprom>(nameof(FtXSeriesEeprom.I2cSlaveAddress)).ToInt32());
            Assert.Equal(44, Marshal.OffsetOf<FtXSeriesEeprom>(nameof(FtXSeriesEeprom.I2cDeviceId)).ToInt32());
            Assert.Equal(54, Marshal.OffsetOf<FtXSeriesEeprom>(nameof(FtXSeriesEeprom.DriverType)).ToInt32());
        }

        [Fact]
        public void ReadingTheProgrammedAddressDoesNotWriteOrDispose()
        {
            var transport = new FakeTransport { Configuration = Configuration() };
            using var device = new Ft200XDevice(transport);
            Assert.Equal(0x22, device.ProgrammedI2cAddress);
            Assert.Equal(1, transport.ConfigurationReads);
            Assert.Equal(0, transport.ConfigurationWrites);
            Assert.Equal(0, transport.DisposeCalls);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        [InlineData(7)]
        [InlineData(0x78)]
        [InlineData(0x7F)]
        [InlineData(0x80)]
        [InlineData(0x123)]
        public void ProgrammingRejectsReservedAndInvalidAddressesBeforeReadingMtp(int address)
        {
            var transport = new FakeTransport();
            using var device = new Ft200XDevice(transport);
            Assert.Throws<ArgumentOutOfRangeException>(() => device.ProgramI2cAddress(address));
            Assert.Equal(0, transport.ConfigurationReads);
            Assert.Equal(0, transport.ConfigurationWrites);
            Assert.Equal(0, transport.DisposeCalls);
        }

        [Fact]
        public void MatchingAddressSkipsProgrammingAndKeepsTheConnectionOpen()
        {
            var transport = new FakeTransport { Configuration = Configuration() };
            using var device = new Ft200XDevice(transport);
            Assert.False(device.ProgramI2cAddress(0x22));
            Assert.Equal(1, transport.ConfigurationReads);
            Assert.Equal(0, transport.ConfigurationWrites);
            Assert.Equal(0, transport.DisposeCalls);
            Assert.Equal(0u, device.BytesAvailable);
        }

        [Theory]
        [InlineData(0x08)]
        [InlineData(0x23)]
        [InlineData(0x77)]
        public void ProgrammingChangesOnlyTheAddressAndVerifiesBeforeClosing(int address)
        {
            FtXSeriesConfiguration original = Configuration();
            var transport = new FakeTransport { Configuration = original };
            using var device = new Ft200XDevice(transport);
            Assert.True(device.ProgramI2cAddress(address));
            Assert.Equal(2, transport.ConfigurationReads);
            Assert.Equal(1, transport.ConfigurationWrites);
            Assert.Equal(1, transport.DisposeCalls);
            FtXSeriesEeprom programmed = transport.Configuration.Data;
            Assert.Equal(address, programmed.I2cSlaveAddress);
            programmed.I2cSlaveAddress = original.Data.I2cSlaveAddress;
            Assert.Equal(original.Data, programmed);
            Assert.Equal(original.Manufacturer, transport.Configuration.Manufacturer);
            Assert.Equal(original.ManufacturerId, transport.Configuration.ManufacturerId);
            Assert.Equal(original.Description, transport.Configuration.Description);
            Assert.Equal(original.SerialNumber, transport.Configuration.SerialNumber);
            Assert.Equal(0x22, original.Data.I2cSlaveAddress);
            Assert.Throws<ObjectDisposedException>(() => device.ProgrammedI2cAddress);
            Assert.Throws<ObjectDisposedException>(() => device.ProgramI2cAddress(address));
            Assert.Throws<ObjectDisposedException>(() => device.Write(new byte[1]));
        }

        [Theory]
        [InlineData("address")]
        [InlineData("cbus")]
        [InlineData("vid")]
        [InlineData("i2c-id")]
        [InlineData("manufacturer")]
        [InlineData("manufacturer-id")]
        [InlineData("description")]
        [InlineData("serial")]
        public void VerificationDetectsUnintendedChangesWithoutRetrying(string field)
        {
            var transport = new FakeTransport { Configuration = Configuration() };
            transport.AfterProgram = configuration =>
            {
                FtXSeriesEeprom data = configuration.Data;
                switch (field)
                {
                    case "address":
                        data.I2cSlaveAddress = 0x22;
                        break;
                    case "cbus":
                        data.Cbus0++;
                        break;
                    case "vid":
                        data.Common.VendorId++;
                        break;
                    case "i2c-id":
                        data.I2cDeviceId++;
                        break;
                    case "manufacturer":
                        configuration.Manufacturer[0]++;
                        break;
                    case "manufacturer-id":
                        configuration.ManufacturerId[0]++;
                        break;
                    case "description":
                        configuration.Description[0]++;
                        break;
                    case "serial":
                        configuration.SerialNumber[0]++;
                        break;
                }

                configuration.Data = data;
            };

            using var device = new Ft200XDevice(transport);
            Assert.Throws<IOException>(() => device.ProgramI2cAddress(0x23));
            Assert.Equal(1, transport.ConfigurationWrites);
            Assert.Equal(1, transport.DisposeCalls);
        }

        [Theory]
        [InlineData(1, 0, 0)]
        [InlineData(2, 1, 1)]
        public void ReadFailuresNeverTriggerAutomaticProgrammingOrRollback(int failedRead, int writes, int disposed)
        {
            var failure = new IOException("Simulated EEPROM read failure.");
            var transport = new FakeTransport
            {
                Configuration = Configuration(),
                ConfigurationReadError = failure,
                FailConfigurationRead = failedRead
            };
            using var device = new Ft200XDevice(transport);
            Assert.Same(failure, Assert.Throws<IOException>(() => device.ProgramI2cAddress(0x23)));
            Assert.Equal(writes, transport.ConfigurationWrites);
            Assert.Equal(disposed, transport.DisposeCalls);
        }

        [Fact]
        public void ProgramFailureClosesTheConnectionWithoutRetrying()
        {
            var failure = new IOException("Simulated EEPROM programming failure.");
            var transport = new FakeTransport { Configuration = Configuration(), ConfigurationProgramError = failure };
            using var device = new Ft200XDevice(transport);
            Assert.Same(failure, Assert.Throws<IOException>(() => device.ProgramI2cAddress(0x23)));
            Assert.Equal(1, transport.ConfigurationReads);
            Assert.Equal(1, transport.ConfigurationWrites);
            Assert.Equal(1, transport.DisposeCalls);
        }

        [Theory]
        [InlineData("type")]
        [InlineData("manufacturer")]
        [InlineData("manufacturer-id")]
        [InlineData("description")]
        [InlineData("serial")]
        public void InvalidSnapshotsCannotBeProgrammed(string field)
        {
            FtXSeriesConfiguration configuration = Configuration();
            switch (field)
            {
                case "type":
                    FtXSeriesEeprom data = configuration.Data;
                    data.Common.DeviceType = FtDeviceType.Ft232H;
                    configuration.Data = data;
                    break;
                case "manufacturer":
                    Array.Fill(configuration.Manufacturer, (byte)1);
                    break;
                case "manufacturer-id":
                    Array.Fill(configuration.ManufacturerId, (byte)1);
                    break;
                case "description":
                    Array.Fill(configuration.Description, (byte)1);
                    break;
                case "serial":
                    Array.Fill(configuration.SerialNumber, (byte)1);
                    break;
            }

            var transport = new FakeTransport { Configuration = configuration };
            using var device = new Ft200XDevice(transport);
            Assert.Throws<IOException>(() => device.ProgramI2cAddress(0x23));
            Assert.Equal(0, transport.ConfigurationWrites);
            Assert.Equal(0, transport.DisposeCalls);
        }

        private static FtXSeriesConfiguration Configuration()
        {
            byte[] bytes = Enumerable.Range(1, 56).Select(i => (byte)i).ToArray();
            FtXSeriesEeprom data = MemoryMarshal.Read<FtXSeriesEeprom>(bytes);
            data.Common.DeviceType = FtDeviceType.FtXSeries;
            data.I2cSlaveAddress = 0x22;
            var configuration = new FtXSeriesConfiguration { Data = data };
            // Keep non-ASCII descriptor bytes intact rather than decoding and re-encoding them.
            new byte[] { 0xC4, 0x54, 0x44, 0x49 }.CopyTo(configuration.Manufacturer, 0);
            new byte[] { 0x46, 0x54 }.CopyTo(configuration.ManufacturerId, 0);
            new byte[] { 0x46, 0x54, 0x32, 0x30, 0x30, 0x58 }.CopyTo(configuration.Description, 0);
            new byte[] { 0x31, 0x32, 0x33 }.CopyTo(configuration.SerialNumber, 0);
            return configuration;
        }

        private static FtDevice Descriptor(string description = "FT200X USB I2C", FtDeviceType type = FtDeviceType.FtXSeries)
        {
            return new FtDevice(0, type, 0x04036015, 0, "TEST123", description);
        }

        private sealed class FakeTransport : IFtXSeriesI2cSlaveTransport
        {
            public byte[] Received { get; set; } = Array.Empty<byte>();

            public byte[] Written { get; private set; } = Array.Empty<byte>();

            public int? ReadCount { get; set; }

            public int? WriteCount { get; set; }

            public uint QueuedBytes { get; set; }

            public IOException? Error { get; set; }

            public Action? OnRead { get; set; }

            public int ReadCalls { get; private set; }

            public int WriteCalls { get; private set; }

            public int DisposeCalls { get; private set; }

            public FtXSeriesConfiguration Configuration { get; set; } = new FtXSeriesConfiguration();

            public int ConfigurationReads { get; private set; }

            public int ConfigurationWrites { get; private set; }

            public int? FailConfigurationRead { get; set; }

            public IOException? ConfigurationReadError { get; set; }

            public IOException? ConfigurationProgramError { get; set; }

            public Action<FtXSeriesConfiguration>? AfterProgram { get; set; }

            public uint BytesAvailable => Error is null ? QueuedBytes : throw Error;

            public int Read(Span<byte> buffer)
            {
                ReadCalls++;
                if (Error is not null)
                {
                    throw Error;
                }

                OnRead?.Invoke();
                Received.CopyTo(buffer);
                return ReadCount ?? Received.Length;
            }

            public int Write(ReadOnlySpan<byte> buffer)
            {
                WriteCalls++;
                if (Error is not null)
                {
                    throw Error;
                }

                Written = buffer.ToArray();
                return WriteCount ?? buffer.Length;
            }

            public FtXSeriesConfiguration ReadConfiguration()
            {
                ConfigurationReads++;
                if (ConfigurationReadError is not null && ConfigurationReads == FailConfigurationRead)
                {
                    throw ConfigurationReadError;
                }

                return Configuration.Clone();
            }

            public void ProgramConfiguration(FtXSeriesConfiguration configuration)
            {
                ConfigurationWrites++;
                if (ConfigurationProgramError is not null)
                {
                    throw ConfigurationProgramError;
                }

                Configuration = configuration.Clone();
                AfterProgram?.Invoke(Configuration);
            }

            public void Dispose()
            {
                DisposeCalls++;
            }
        }
    }
}
