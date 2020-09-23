// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
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
    public sealed class Mhz19b : IDisposable
    {
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

        private const int MessageSize = 9;

        private SerialPort _serialPort = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="Mhz19b"/> class.
        /// </summary>
        /// <param name="uartDevice">Path to the UART device / serial port, e.g. /dev/serial0</param>
        /// <exception cref="System.ArgumentException">uartDevice is null or empty</exception>
        public Mhz19b(string uartDevice)
        {
            if (string.IsNullOrEmpty(uartDevice))
            {
                throw new ArgumentException(nameof(uartDevice));
            }

            // create serial port using the setting acc. to datasheet, pg. 7, sec. general settings
            _serialPort = new SerialPort(uartDevice, 9600, Parity.None, 8, StopBits.One);
            _serialPort.Encoding = Encoding.ASCII;
            _serialPort.ReadTimeout = 1000;
            _serialPort.WriteTimeout = 1000;
        }

        /// <summary>
        /// Gets the current CO2 concentration from the sensor.
        /// The validity is true if the current concentration was successfully read.
        /// If the serial communication timed out or the checksum was invalid the validity is false.
        /// If the validity is false the ratio is set to 0.
        /// </summary>
        /// <returns>CO2 concentration in ppm and validity</returns>
        /// <exception cref="System.IO.IOException">Communication with sensor failed</exception>
        public VolumeConcentration GetCo2Reading()
        {
            try
            {
                var request = CreateRequest(Command.ReadCo2Concentration);
                request[(int)MessageFormat.Checksum] = Checksum(request);

                _serialPort.Open();
                _serialPort.Write(request, 0, request.Length);

                // wait until the response has been completely received
                int timeout = 100;
                while (timeout > 0 && _serialPort.BytesToRead < MessageSize)
                {
                    Thread.Sleep(1);
                    timeout--;
                }

                if (timeout == 0)
                {
                    throw new IOException("Timeout");
                }

                // read and process response
                byte[] response = new byte[MessageSize];
                if ((_serialPort.Read(response, 0, response.Length) == response.Length) && (response[(int)MessageFormat.Checksum] == Checksum(response)))
                {
                    return VolumeConcentration.FromPartsPerMillion((int)response[(int)MessageFormat.DataHighResponse] * 256 + (int)response[(int)MessageFormat.DataLowResponse]);
                }
                else
                {
                    throw new IOException("Invalid response message received from sensor");
                }
            }
            catch (Exception e)
            {
                throw new IOException("Sensor communication failed", e);
            }
            finally
            {
                _serialPort.Close();
            }
        }

        /// <summary>
        /// Initiates a zero point calibration.
        /// The sensor doesn't respond anything, so this is fire and forget.
        /// </summary>
        /// <exception cref="System.IO.IOException">Communication with sensor failed</exception>
        public void PerformZeroPointCalibration() => SendRequest(CreateRequest(Command.CalibrateZeroPoint));

        /// <summary>
        /// Initiate a span point calibration.
        /// The sensor doesn't respond anything, so this is fire and forget.
        /// </summary>
        /// <param name="span">span value, between 1000[ppm] and 5000[ppm]. The typical value is 2000[ppm].</param>
        /// <exception cref="System.ArgumentException">Thrown when span value is out of range</exception>
        /// <exception cref="System.IO.IOException">Communication with sensor failed</exception>
        public void PerformSpanPointCalibration(VolumeConcentration span)
        {
            if ((span.PartsPerMillion < 1000) || (span.PartsPerMillion > 5000))
            {
                throw new ArgumentException("Span value out of range (1000-5000[ppm])", nameof(span));
            }

            var request = CreateRequest(Command.CalibrateSpanPoint);
            // set span in request, c. f. datasheet rev. 1.0, pg. 8 for details
            request[(int)MessageFormat.DataHighRequest] = (byte)(span.PartsPerMillion / 256);
            request[(int)MessageFormat.DataLowRequest] = (byte)(span.PartsPerMillion % 256);

            SendRequest(request);
        }

        /// <summary>
        /// Switches the autmatic baseline correction on and off.
        /// The sensor doesn't respond anything, so this is fire and forget.
        /// </summary>
        /// <param name="state">State of automatic correction</param>
        /// <exception cref="System.IO.IOException">Communication with sensor failed</exception>
        public void SetAutomaticBaselineCorrection(AbmState state)
        {
            var request = CreateRequest(Command.AutoCalibrationSwitch);
            // set on/off state in request, c. f. datasheet rev. 1.0, pg. 8 for details
            request[(int)MessageFormat.DataHighRequest] = (byte)state;

            SendRequest(request);
        }

        /// <summary>
        /// Set the sensor detection range.
        /// The sensor doesn't respond anything, so this is fire and forget
        /// </summary>
        /// <param name="detectionRange">Detection range of the sensor</param>
        /// <exception cref="System.IO.IOException">Communication with sensor failed</exception>
        public void SetSensorDetectionRange(DetectionRange detectionRange)
        {
            var request = CreateRequest(Command.DetectionRangeSetting);
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
                _serialPort.Open();
                _serialPort.Write(request, 0, request.Length);
            }
            catch (Exception e)
            {
                throw new IOException("Sensor communication failed", e);
            }
            finally
            {
                _serialPort.Close();
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
            if (_serialPort == null)
            {
                return;
            }

            _serialPort.Dispose();
            _serialPort = null;
        }
    }
}