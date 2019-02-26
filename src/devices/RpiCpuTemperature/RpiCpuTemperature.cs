using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Iot.Device.Rpi
{
    public class CpuTemperature
    {
        public double? ReadTemperature()
        {
            double? _temperature = null;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) && File.Exists("/sys/class/thermal/thermal_zone0/temp"))
            {
                using (FileStream _fileStream = new FileStream("/sys/class/thermal/thermal_zone0/temp", FileMode.Open, FileAccess.Read))
                using (StreamReader _reader = new StreamReader(_fileStream))
                {
                    string _data = _reader.ReadLine();
                    if (!string.IsNullOrEmpty(_data))
                    {
                        int _temp;
                        if (int.TryParse(_data, out _temp))
                        {
                            _temperature = Math.Round(_temp / 1000F, 1);
                        }
                    }
                }
            }
            return _temperature;
        }
    }
}
