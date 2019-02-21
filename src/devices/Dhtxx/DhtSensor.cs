// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device;
using System.Device.Gpio;
using System.Diagnostics;

namespace Iot.Device.DHTxx
{
    public class DHTSensor : ITemperatureSensor, IDisposable
    {
        private const int MAX_TIME = 85;
        private const uint MAX_WAIT = 255;
        private byte[] _dht11Val = new byte[5];
        private int _pin;
        private DhtType _dhtType;

        private GpioController _controller = new GpioController();

        private Stopwatch _stopwatch = new Stopwatch();

        /// <summary>
        /// Wait for a specific number of milliseconds
        ///
        /// </summary>
        /// <param name="milliseconds">Number of milliseconds to wait</param>
        /// <remarks>
        /// This function doesn't work if you want to wait for less than 100 microseconds.
        /// </remarks>
        private void Wait(double milliseconds)
        {
            long initialTick = _stopwatch.ElapsedTicks;
            long initialElapsed = _stopwatch.ElapsedMilliseconds;
            double desiredTicks = milliseconds / 1000.0 * Stopwatch.Frequency;
            double finalTick = initialTick + desiredTicks;
            while (_stopwatch.ElapsedTicks < finalTick)
            {
                //nothing than waiting
            }
        }

        /// <summary>
        /// How last read went, <c>true</c> for success, <c>false</c> for failure
        /// </summary>
        public bool IsLastReadSuccessful { get; internal set; }

        /// <summary>
        /// Get the last read temperature in Celsius
        /// </summary>
        /// <remarks>
        /// If last read was not successfull, it returns double.MaxValue
        /// </remarks>
        public double Temperature => (_dhtType == DhtType.Dht11) ? GetTempDht11() : GetTempDht22();

        /// <summary>
        /// Get the last read temperature in Farenheit
        /// </summary>
        /// <remarks>
        /// If last read was not successfull, it returns double.MaxValue
        /// </remarks>
        public double TemperatureInFarenheit => IsLastReadSuccessful ? (9.0 / 5.0 * Temperature + 32) : Double.MaxValue;

        /// <summary>
        /// Get the temperature in Celsius
        /// </summary>
        /// <param name="temperatureInCelsius">The temperature in Celsius</param>
        /// <returns>Returns <c>true</c> if the read is successful</returns>
        /// <remarks>
        /// If last read was not successfull, it returns double.MaxValue
        /// </remarks>
        public bool TryGetTemperature(out double temperatureInCelsius)
        {
            var ret = ReadData();
            temperatureInCelsius = Temperature;
            return ret;
        }

        /// <summary>
        /// Get the temperature in Farenheit
        /// </summary>
        /// <param name="temperatureInFarenheit">The temperature in Farenheit</param>
        /// <returns>Returns <c>true</c> if the read is successful</returns>
        /// <remarks>
        /// If last read was not successfull, it returns double.MaxValue
        /// </remarks>
        public bool TryGetTemperatureInFarenheit(out double temperatureInFarenheit)
        {
            var ret = ReadData();
            temperatureInFarenheit = TemperatureInFarenheit;
            return ret;
        }

        /// <summary>
        /// Get the last read of relative humidity in percentage
        /// </summary>
        /// <remarks>
        /// If last read was not successfull, it returns double.MaxValue
        /// </remarks>
        public double Humidity => (_dhtType == DhtType.Dht11) ? GetHumidityDht11() : GetHumidityDht22();

        /// <summary>
        /// Get the relative humidity in the air
        /// </summary>
        /// <param name="relativeHumidity">The percentage of relative humidity in the air</param>
        /// <returns>Returns <c>true</c> if the read is successful</returns>
        /// <remarks>
        /// If last read was not successfull, it returns double.MaxValue
        /// </remarks>
        public bool TryGetHumidity(out double relativeHumidity)
        {
            var ret = ReadData();
            relativeHumidity = Humidity;
            return ret;
        }

        /// <summary>
        /// Get the temperature in Celsius and the relative humidity in the air
        /// </summary>
        /// <param name="temperatureInCelsius">The temperature in Celsius</param>
        /// <param name="relativeHumidity">The percentage of relative humidity in the air</param>
        /// <returns>Returns <c>true</c> if the read is successful</returns>
        /// <remarks>
        /// If last read was not successfull, it returns double.MaxValue
        /// </remarks>
        public bool TryGetTemperatureAndHumidity(out double temperatureInCelsius, out double relativeHumidity)
        {
            var ret = ReadData();
            temperatureInCelsius = Temperature;
            relativeHumidity = Humidity;
            return ret;
        }

        /// <summary>
        /// Get the temperature in Farenheit and the relative humidity in the air
        /// </summary>
        /// <param name="temperatureInFarenheit">The temperature in Farenheit</param>
        /// <param name="relativeHumidity">The percentage of relative humidity in the air</param>
        /// <returns>Returns <c>true</c> if the read is successful</returns>
        /// <remarks>
        /// If last read was not successfull, it returns double.MaxValue
        /// </remarks>
        public bool TryGetTemperatureInFarenheitAndHumidity(out double temperatureInFarenheit, out double relativeHumidity)
        {
            var ret = ReadData();
            temperatureInFarenheit = TemperatureInFarenheit;
            relativeHumidity = Humidity;
            return ret;
        }

        /// <summary>
        /// Create a DHT sensor
        /// </summary>
        /// <param name="pin">The pin number (GPIO number)</param>
        /// <param name="dhtType">The DHT Type, either Dht11 or Dht22</param>
        public DHTSensor(int pin, DhtType dhtType)
        {
            this._pin = pin;
            this._dhtType = dhtType;
            _controller.OpenPin(pin);
        }

        /// <summary>
        /// Start a reading
        /// </summary>
        /// <returns>
        /// <c>true</c> if read is successfull, otherwise <c>false</c>.
        /// </returns>
        public bool ReadData()
        {
            // Set the max value for waiting micro second
            // 27 = debug
            // 99 = release
            byte waitMS = 99;
#if DEBUG
            waitMS = 27;
#endif
            _stopwatch.Start();
            PinValue lststate = PinValue.High;
            uint counter = 0;
            byte j = 0, i;
            for (i = 0; i < 5; i++)
                _dht11Val[i] = 0;

            // write on the pin
            _controller.SetPinMode(_pin, PinMode.Output);
            _controller.Write(_pin, PinValue.Low);
            //wait 18 milliseconds
            Wait(18);
            _controller.Write(_pin, PinValue.High);
            // Wait about 40 microseconds
            Wait(0.03);
            _controller.SetPinMode(_pin, PinMode.Input);

            for (i = 0; i < MAX_TIME; i++)
            {
                counter = 0;
                while (_controller.Read(_pin) == lststate)
                {
                    counter++;
                    // This wait about 1 microsecond
                    // No other way to do it for such a precision
                    for (byte wt = 0; wt < waitMS; wt++)
                        ;
                    if (counter == MAX_WAIT)
                        break;
                }

                lststate = _controller.Read(_pin);
                if (counter == MAX_WAIT)
                    break;

                // top 3 transistions are ignored
                if ((i >= 4) && (i % 2 == 0))
                {
                    _dht11Val[j / 8] <<= 1;
                    if (counter > 16)
                        _dht11Val[j / 8] |= 1;
                    j++;
                }
            }

            _stopwatch.Stop();
            if ((j >= 40) && (_dht11Val[4] == ((_dht11Val[0] + _dht11Val[1] + _dht11Val[2] + _dht11Val[3]) & 0xFF)))
            {
                IsLastReadSuccessful = (_dht11Val[0] != 0) || (_dht11Val[2] != 0);
            }
            else
            {
                IsLastReadSuccessful = false;
            }

            return IsLastReadSuccessful;
        }

        // Convertion for DHT11
        private double GetTempDht11() => IsLastReadSuccessful ? (double)(_dht11Val[2] + _dht11Val[3] / 10) : double.MaxValue;

        private double GetHumidityDht11() => IsLastReadSuccessful ? (double)(_dht11Val[0] + _dht11Val[1] / 10) : double.MaxValue;

        // convertion for DHT22
        private double GetTempDht22()
        {
            if (IsLastReadSuccessful)
            {
                var temp = (((_dht11Val[2] & 0x7F) << 8) | _dht11Val[3]) * 0.1F;
                // if MSB = 1 we have negative temperature
                return ((_dht11Val[2] & 0x80) == 0 ? temp : -temp);
            }
            else
                return (double.MaxValue);
        }

        private double GetHumidityDht22() => IsLastReadSuccessful ? (double)((_dht11Val[0] << 8) | _dht11Val[1]) * 0.1F : double.MaxValue;

        public void Dispose()
        {
            _controller.ClosePin(_pin);
        }
    }
}
