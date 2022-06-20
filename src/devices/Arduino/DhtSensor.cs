// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnitsNet;

namespace Iot.Device.Arduino
{
    /// <summary>
    /// This class supports the DhtFirmata extensions to read DHT sensors over the firmata protocol.
    /// </summary>
    public class DhtSensor : ExtendedCommandHandler
    {
        /// <summary>
        /// Initializes a new instance of the DhtSensor extended command handler.
        /// This handler requires pins with DHT support. This is typically the case for all GPIO pins if the
        /// firmata client has the DhtFirmata extension loaded.
        /// </summary>
        public DhtSensor()
        : base(SupportedMode.Dht)
        {
        }

        /// <summary>
        /// Special function to read DHT sensor, if supported
        /// </summary>
        /// <param name="pinNumber">Pin Number</param>
        /// <param name="dhtType">Type of DHT Sensor: 11 = DHT11, 22 = DHT22, etc.</param>
        /// <param name="temperature">Temperature</param>
        /// <param name="humidity">Relative humidity</param>
        /// <returns>True on success, false otherwise</returns>
        public bool TryReadDht(int pinNumber, int dhtType, out Temperature temperature, out RelativeHumidity humidity)
        {
            IReadOnlyList<SupportedPinConfiguration> pinConfiguration = Board.SupportedPinConfigurations;
            if (!pinConfiguration[pinNumber].PinModes.Contains(SupportedMode.Dht))
            {
                temperature = default;
                humidity = default;
                return false;
            }

            return TryReadDhtInternal(pinNumber, dhtType, out temperature, out humidity);
        }

        private bool TryReadDhtInternal(int pinNumber, int dhtType, out Temperature temperature, out RelativeHumidity humidity)
        {
            temperature = default;
            humidity = default;

            FirmataCommandSequence dhtCommandSequence = new();
            dhtCommandSequence.WriteByte((byte)FirmataSysexCommand.DHT_SENSOR_DATA_REQUEST);
            dhtCommandSequence.WriteByte((byte)dhtType);
            dhtCommandSequence.WriteByte((byte)pinNumber);
            dhtCommandSequence.WriteByte((byte)FirmataCommand.END_SYSEX);
            byte[] reply = SendCommandAndWait(dhtCommandSequence);

            // Command, pin number and 2x2 bytes data (+ END_SYSEX byte)
            if (reply.Length < 7)
            {
                return false;
            }

            if (reply[0] != (byte)FirmataSysexCommand.DHT_SENSOR_DATA_REQUEST && reply[1] != 0)
            {
                return false;
            }

            int t = reply[3] | reply[4] << 7;
            int h = reply[5] | reply[6] << 7;

            temperature = Temperature.FromDegreesCelsius(t / 10.0);
            humidity = RelativeHumidity.FromPercent(h / 10.0);

            return true;
        }
    }
}
