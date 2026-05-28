// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Concurrent;
using System.Device.I2c;
using System.IO;
using System.Linq;
using System.Threading;
using Iot.Device.Bmxx80;
using Iot.Device.Bmxx80.PowerMode;
using Iot.Device.Board;
using Iot.Device.Common;
using Iot.Device.HardwareMonitor;
using UnitsNet;

namespace Iot.Device.Arduino.Sample
{
    internal sealed class SensorHandling : IDisposable
    {
        private readonly object _displayLock;
        public const string BmpSensor = "BMP";
        public const string DhtSensor = "DHT";
        public const string Cpu = "CPU";
        public const string Gpu = "GPU";
        private readonly I2cBus _bus;
        private readonly Bmp280? _bmp280;
        private readonly DhtSensor? _dht;
        private readonly OpenHardwareMonitor _hardwareMonitor;

        private readonly Thread _thread;
        private readonly CancellationTokenSource _cancellationTokenSource;

        private readonly ConcurrentDictionary<string, SensorValues> _sensorValues;
        private TimeSliceFilter<Pressure> _rawPressureFilter;

        public SensorHandling(ArduinoBoard board, object displayLock)
        {
            _displayLock = displayLock;
            _cancellationTokenSource = new CancellationTokenSource();
            _sensorValues = new ConcurrentDictionary<string, SensorValues>();
            _sensorValues.TryAdd(BmpSensor, new SensorValues(BmpSensor));
            _sensorValues.TryAdd(DhtSensor, new SensorValues(DhtSensor));
            _sensorValues.TryAdd(Cpu, new SensorValues(Cpu));
            _sensorValues.TryAdd(Gpu, new SensorValues(Gpu));
            _rawPressureFilter =
                new TimeSliceFilter<Pressure>(TimeSpan.FromSeconds(0.5), TimeSliceFilter<Pressure>.AverageFilter);

            Console.WriteLine("Scanning for I2C devices...");
            _bus = board.CreateOrGetI2cBus(board.GetDefaultI2cBusNumber());
            var scanned = _bus.PerformBusScan(lowest: 0x71);

            int assumedBmp280Address = Bmp280.DefaultI2cAddress;
            if (scanned.Contains(Bmp280.DefaultI2cAddress))
            {
                assumedBmp280Address = Bmp280.DefaultI2cAddress;
            }
            else if (scanned.Contains(Bmp280.SecondaryI2cAddress))
            {
                assumedBmp280Address = Bmp280.SecondaryI2cAddress;
            }

            var device = board.CreateI2cDevice(new I2cConnectionSettings(0, assumedBmp280Address));
            try
            {
                _bmp280 = new Bmp280(device);
                _bmp280.StandbyTime = StandbyTime.Ms250;
                _bmp280.SetPowerMode(Bmx280PowerMode.Normal);
                Console.WriteLine($"Found BMP280 at {assumedBmp280Address}");
            }
            catch (IOException)
            {
                _bmp280 = null;
                Console.WriteLine($"BMP280 not available at detected address {assumedBmp280Address}");
            }

            _dht = board.GetCommandHandler<DhtSensor>();
            if (_dht == null)
            {
                // Note that this is a software error, hardware support is not tested here.
                Console.WriteLine("DHT Sensor module missing");
            }

            _hardwareMonitor = new OpenHardwareMonitor();
            _hardwareMonitor.EnableDerivedSensors();
            _thread = new Thread(SensorThread);
            _thread.Start();
        }

        public SensorValues GetSensor(string name)
        {
            return _sensorValues[name] with { };
        }

        private void SensorThread()
        {
            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                lock (_displayLock)
                {
                    if (_bmp280 != null && _bmp280.TryReadPressure(out var p) && _bmp280.TryReadTemperature(out var t))
                    {
                        _sensorValues[BmpSensor].Pressure = p;
                        _sensorValues[BmpSensor].Temperature = t;
                    }

                    if (_dht != null && _dht.TryReadDht(3, 11, out var t2, out var rh2))
                    {
                        _sensorValues[DhtSensor].Humidity = rh2;
                        _sensorValues[DhtSensor].Temperature = t2;
                    }
                }

                if (_hardwareMonitor.GetSensorList().Count > 0)
                {
                    if (_hardwareMonitor.TryGetAverageCpuTemperature(out var t3))
                    {
                        _sensorValues[Cpu].Temperature = t3;
                    }

                    _sensorValues[Cpu].Load = _hardwareMonitor.GetCpuLoad();

                    if (_hardwareMonitor.TryGetAverageGpuTemperature(out var t4))
                    {
                        _sensorValues[Gpu].Temperature = t4;
                    }

                    var powerSources = _hardwareMonitor.GetSensorList().Where(x => x.SensorType == SensorType.Power);
                    Power totalPower = Power.Zero;
                    foreach (var power in powerSources)
                    {
                        if (power.Name != "CPU Cores" &&
                            power.TryGetValue(out Power powerConsumption)) // included in CPU Package
                        {
                            totalPower = totalPower + powerConsumption;
                        }
                    }

                    _sensorValues[Cpu].Power = totalPower;

                    var energySources = _hardwareMonitor.GetSensorList().Where(x => x.SensorType == SensorType.Energy);
                    Energy totalEnergy = Energy.FromWattHours(0); // Set up the desired output unit
                    foreach (var e in energySources)
                    {
                        if (!e.Name.StartsWith("CPU Cores") &&
                            e.TryGetValue(out Energy powerConsumption)) // included in CPU Package
                        {
                            totalEnergy = totalEnergy + powerConsumption;
                        }
                    }

                    _sensorValues[Cpu].Energy = totalEnergy;
                }
                else
                {
                    _sensorValues[Cpu].Clear();
                    _sensorValues[Gpu].Clear();
                }

                Thread.Sleep(100);
            }
        }

        public void Dispose()
        {
            _cancellationTokenSource?.Cancel();
            _thread.Join();
            _hardwareMonitor.Dispose();
            _bmp280?.Dispose();
            _dht?.Dispose();
            _bus.Dispose();
        }
    }
}
