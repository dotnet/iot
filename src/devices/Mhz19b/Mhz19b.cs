// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Model;
using System.IO;
using System.IO.Ports;
using System.Text;
using System.Threading;
using UnitsNet;

namespace Iot.Device.Mhz19b
{
    /// <summary>
    /// MH-Z19B CO2 concentration sensor binding
    /// </summary>
    [Interface("MH-Z19B CO2 concentration sensor binding")]
    public sealed class Mhz19b : IDisposable
    {
        private const int MessageBytes = 9;
        private bool _shouldDispose = false;
        private SerialPort? _serialPort;
        private Stream _serialPortStream;

        /// <summary>
        /// Initializes a new instance of the <see cref="Mhz19b"/> class using an existing (serial port) stream.
        /// </summary>
        /// <param name="stream">Existing stream</param>
        /// <param name="shouldDispose">If true, the stream gets disposed when disposing the binding</param>
        public Mhz19b(Stream stream, bool shouldDispose)
        {
            _serialPortStream = stream ?? throw new ArgumentNullException(nameof(stream));
            _shouldDispose = shouldDispose;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Mhz19b"/> class and creates a new serial port stream.
        /// </summary>
        /// <param name="uartDevice">Path to the UART device / serial port, e.g. /dev/serial0</param>
        /// <exception cref="System.ArgumentException">uartDevice is null or empty</exception>
        public Mhz19b(string uartDevice)
        {
            if (uartDevice is not { Length: > 0 })
            {
                throw new ArgumentException($"{nameof(uartDevice)} can't be null or empty.", nameof(uartDevice));
            }

            // create serial port using the setting acc. to datasheet, pg. 7, sec. general settings
            _serialPort = new SerialPort(uartDevice, 9600, Parity.None, 8, StopBits.One)
            {
                Encoding = Encoding.ASCII,
                ReadTimeout = 1000,
                WriteTimeout = 1000
            };

            _serialPort.Open();
            _serialPortStream = _serialPort.BaseStream;
            _shouldDispose = true;
        }

        /// <summary>
        /// Gets the current CO2 concentration from the sensor.
        /// </summary>
        /// <returns>CO2 volume concentration</returns>
        /// <exception cref="IOException">Communication with sensor failed</exception>
        /// <exception cref="TimeoutException">A timeout occurred while communicating with the sensor</exception>
        [Telemetry("Co2Concentration")]
        public VolumeConcentration GetCo2Reading()
        {
            // send read command request
            byte[] request = CreateRequest(Command.ReadCo2Concentration);
            request[(int)MessageFormat.Checksum] = Checksum(request);
            _serialPortStream.Write(request, 0, request.Length);

            // read complete response (9 bytes expected)
            byte[] response = new byte[MessageBytes];

            long endTicks = DateTime.Now.AddMilliseconds(250).Ticks;
            int bytesRead = 0;
            while (DateTime.Now.Ticks < endTicks && bytesRead < MessageBytes)
            {
                bytesRead += _serialPortStream.Read(response, bytesRead, response.Length - bytesRead);
                Thread.Sleep(1);
            }

            if (bytesRead < MessageBytes)
            {
                throw new TimeoutException($"Communication with sensor failed.");
            }

            // check response and return calculated concentration if valid
            if (response[(int)MessageFormat.Checksum] == Checksum(response))
            {
                return VolumeConcentration.FromPartsPerMillion((int)response[(int)MessageFormat.DataHighResponse] * 256 + (int)response[(int)MessageFormat.DataLowResponse]);
            }
            else
            {
                throw new IOException("Invalid response message received from sensor");
            }
        }

        /// <summary>
        /// Initiates a zero point calibration.
        /// </summary>
        /// <exception cref="System.IO.IOException">Communication with sensor failed</exception>
        [Command]
        public void PerformZeroPointCalibration() => SendRequest(CreateRequest(Command.CalibrateZeroPoint));

        /// <summary>
        /// Initiate a span point calibration.
        /// </summary>
        /// <param name="span">span value, between 1000[ppm] and 5000[ppm]. The typical value is 2000[ppm].</param>
        /// <exception cref="System.ArgumentException">Thrown when span value is out of range</exception>
        /// <exception cref="System.IO.IOException">Communication with sensor failed</exception>
        [Command]
        public void PerformSpanPointCalibration(VolumeConcentration span)
        {
            if ((span.PartsPerMillion < 1000) || (span.PartsPerMillion > 5000))
            {
                throw new ArgumentException("Span value out of range (1000-5000[ppm])", nameof(span));
            }

            byte[] request = CreateRequest(Command.CalibrateSpanPoint);
            // set span in request, c. f. datasheet rev. 1.0, pg. 8 for details
            request[(int)MessageFormat.DataHighRequest] = (byte)(span.PartsPerMillion / 256);
            request[(int)MessageFormat.DataLowRequest] = (byte)(span.PartsPerMillion % 256);

            SendRequest(request);
        }

        /// <summary>
        /// Switches the autmatic baseline correction on and off.
        /// </summary>
        /// <param name="state">State of automatic correction</param>
        /// <exception cref="System.IO.IOException">Communication with sensor failed</exception>
        [Command]
        public void SetAutomaticBaselineCorrection(AbmState state)
        {
            byte[] request = CreateRequest(Command.AutoCalibrationSwitch);
            // set on/off state in request, c. f. datasheet rev. 1.0, pg. 8 for details
            request[(int)MessageFormat.DataHighRequest] = (byte)state;

            SendRequest(request);
        }

        /// <summary>
        /// Set the sensor detection range.
        /// </summary>
        /// <param name="detectionRange">Detection range of the sensor</param>
        /// <exception cref="System.IO.IOException">Communication with sensor failed</exception>
        [Property("DetectionRange")]
        public void SetSensorDetectionRange(DetectionRange detectionRange)
        {
            byte[] request = CreateRequest(Command.DetectionRangeSetting);
            // set detection range in request, c. f. datasheet rev. 1.0, pg. 8 for details
            request[(int)MessageFormat.DataHighRequest] = (byte)((int)detectionRange / 256);
            request[(int)MessageFormat.DataLowRequest] = (byte)((int)detectionRange % 256);

            SendRequest(request);
        }

        private void SendRequest(byte[] request)
        {
            request[(int)MessageFormat.Checksum] = Checksum(request);

            try
            {
                _serialPortStream.Write(request, 0, request.Length);
            }
            catch (Exception e)
            {
                throw new IOException("Sensor communication failed", e);
            }
        }

        private byte[] CreateRequest(Command command) => new byte[]
            {
                0xff, // start byte,
                0x01, // sensor number, always 0x1
                (byte)command,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00 // empty bytes
            };

        /// <summary>
        /// Calculate checksum for requests and responses.
        /// For details refer to datasheet rev. 1.0, pg. 8.
        /// </summary>
        /// <param name="packet">Packet the checksum is calculated for</param>
        /// <returns>Cheksum</returns>
        private byte Checksum(byte[] packet)
        {
            byte checksum = 0;
            for (int i = 1; i < 8; i++)
            {
                checksum += packet[i];
            }

            checksum = (byte)(0xff - checksum);
            checksum += 1;
            return checksum;
        }

        /// <inheritdoc cref="IDisposable" />
        public void Dispose()
        {
            if (_shouldDispose)
            {
                _serialPortStream?.Dispose();
                _serialPortStream = null!;
            }

            if (_serialPort?.IsOpen ?? false)
            {
                _serialPort.Close();
            }

            _serialPort?.Dispose();
            _serialPort = null;
        }

        private enum Command : byte
        {
            ReadCo2Concentration = 0x86,
            CalibrateZeroPoint = 0x87,
            CalibrateSpanPoint = 0x88,
            AutoCalibrationSwitch = 0x79,
            DetectionRangeSetting = 0x99
        }

        private enum MessageFormat
        {
            Start = 0x00,
            SensorNum = 0x01,
            Command = 0x02,
            DataHighRequest = 0x03,
            DataLowRequest = 0x04,
            DataHighResponse = 0x02,
            DataLowResponse = 0x03,
            Checksum = 0x08
        }
    }
}
