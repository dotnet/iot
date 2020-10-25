// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Text;
using System.IO.Ports;
using UnitsNet;
using Iot.Device.MettlerToledo.Readings;
using Iot.Device.MettlerToledo.Exceptions;

namespace Iot.Device.MettlerToledo
{
    /// <summary>
    /// Provides an interface for interacting with MT-SACS devices, like scales.
    /// </summary>
    public class MettlerToledoDevice : IDisposable
    {
        private SerialPort _serialPort;
        private bool _shouldDisposeSerialPort = true;

        /// <summary>
        /// Initializes a new instance of the <see cref="MettlerToledoDevice"/> class.
        /// </summary>
        public MettlerToledoDevice(string port, int baudrate = 9600)
        {
            _serialPort = new SerialPort(port, baudrate, Parity.None);
            _serialPort.Encoding = Encoding.ASCII;
            _serialPort.NewLine = "\r\n";
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MettlerToledoDevice"/> class.
        /// </summary>
        /// <param name="serialPort">Serial port to use</param>
        /// <param name="shouldDispose">Should the serial port be disposed by this class?</param>
        public MettlerToledoDevice(SerialPort serialPort, bool shouldDispose = true)
        {
            _serialPort = serialPort;
            _serialPort.Encoding = Encoding.ASCII;
            _serialPort.NewLine = "\r\n";
            _shouldDisposeSerialPort = shouldDispose;
        }

        /// <summary>
        /// Retrieves the current stable weight from the scale.
        /// </summary>
        /// <returns>Current weight</returns>
        public MettlerToledoWeightReading GetStableWeight() => GetWeightReading(false);

        /// <summary>
        /// Retrieves the last internal weight value (stable or dynamic) before receipt of this command.
        /// </summary>
        /// <returns>Current weight reading</returns>
        public MettlerToledoWeightReading GetImmediateWeight() => GetWeightReading(true);

        /// <summary>
        /// Opens a connection to the MT-SACS scale.
        /// </summary>
        /// <exception cref="UnauthorizedAccessException"></exception>
        public void Open()
        {
            _serialPort.Open();
        }

        /// <summary>
        /// Closes the connection to the MT-SACS scale.
        /// </summary>
        public void Close()
        {
            if (_serialPort.IsOpen)
            {
                _serialPort.Close();
            }
        }

        /// <summary>
        /// Zeroes the balance/scale. If <paramref name="immediately"/> is set to true, will balance regardless of stability.
        /// </summary>
        /// <exception cref="CommandNotExecutableException"></exception>
        /// <exception cref="BalanceOutOfRangeException"></exception>
        /// <exception cref="UnknownResultException"></exception>
        /// <returns>Boolean indicating whether re-zero was performed under stable conditions</returns>
        public bool Zero(bool immediately = false)
        {
            var command = immediately ? "ZI" : "Z";
            var response = SendCommandWithResponse(command);
            switch (response[1])
            {
                case "I":
                    throw new CommandNotExecutableException(command);
                case "+":
                    throw new BalanceOutOfRangeException(true, false, command);
                case "-":
                    throw new BalanceOutOfRangeException(false, true, command);
                case "A": // non-force stable success
                    return true;
                case "S": // force stable success
                    return true;
                case "D": // force non-stable success
                    return false;
                default:
                    throw new UnknownResultException("Unknown result received");
            }
        }

        /// <summary>
        /// Resets the balance (scale) to the condition found after switching on, but without a zero setting being performed. This means:
        /// <list type="bullet">
        /// <item>All commands awaiting responses are cancelled</item>
        /// <item>Key control is set to the default setting</item>
        /// <item>Tare memory is reset to zero</item>
        /// <item>If balance (scale) is on standby, it is switched on</item>
        /// </list>
        /// </summary>
        /// <exception cref="UnknownResultException"></exception>
        public void Reset()
        {
            var response = SendCommandWithResponse("@");
            if (response[0] != "I4")
            {
                throw new UnknownResultException("Unknown result received");
            }
        }

        /// <summary>
        /// Writes text to the balance display
        /// </summary>
        /// <param name="text">Text to write to the display</param>
        /// <exception cref="UnknownResultException"></exception>
        /// <exception cref="CommandNotExecutableException"></exception>
        public void WriteToDisplay(string text)
        {
            if (text.Contains("\""))
            {
                throw new ArgumentException("Text to display cannot contain quotation marks.", nameof(text));
            }

            var command = $"D \"{text}\"";
            var response = SendCommandWithResponse(command);
            switch (response[1])
            {
                case "A": // success
                    break;
                case "I":
                    throw new CommandNotExecutableException(command);
                case "L":
                    throw new CommandNotExecutableException(command); // todo: maybe show a different error?
                default:
                    throw new UnknownResultException("Unknown result received");
            }
        }

        /// <summary>
        /// Clears any text that was written to the display.
        /// </summary>
        /// <exception cref="UnknownResultException" />
        /// <exception cref="CommandNotExecutableException" />
        public void ClearDisplay()
        {
            WriteToDisplay(string.Empty);
        }

        /// <summary>
        /// Changes the display to show the current weight.
        /// </summary>
        /// <exception cref="UnknownResultException" />
        /// <exception cref="CommandNotExecutableException" />
        public void ShowWeightOnDisplay()
        {
            var command = "DW";
            var response = SendCommandWithResponse(command);
            switch (response[1])
            {
                case "A": // success
                    break;
                case "I":
                    throw new CommandNotExecutableException(command);
                default:
                    throw new UnknownResultException("Unknown result received");
            }
        }

        /// <summary>
        /// Have the scale send an event every time the weight is changed.
        /// </summary>
        public void SubscribeToWeightEvents()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Sends a command over the serial port.
        /// </summary>
        /// <param name="command">Command to send</param>
        /// <returns>Result of the command.</returns>
        /// <exception cref="InvalidOperationException" />
        /// <exception cref="TimeoutException">Occurs if the serial port times out while sending or receiving.</exception>
        /// <exception cref="SyntaxErrorException" />
        /// <exception cref="TransmissionErrorException" />
        /// <exception cref="LogicalErrorException" />
        private string[] SendCommandWithResponse(string command)
        {
            _serialPort.WriteLine(command);
            var result = _serialPort.ReadLine();
            var split = result.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            switch (split[0])
            {
                case "ES":
                    throw new SyntaxErrorException(command);
                case "ET":
                    throw new TransmissionErrorException(command);
                case "EL":
                    throw new LogicalErrorException(command);
            }

            return split;
        }

        private MettlerToledoWeightReading GetWeightReading(bool forceImmediate = false)
        {
            var command = forceImmediate ? "SI" : "S"; // SI sends the current net weight value, irrespective of balance stability. S will wait for the scale to balance.
            var response = SendCommandWithResponse(command);
            bool stable = false;
            switch (response[1])
            {
                case "I":
                    throw new CommandNotExecutableException(command);
                case "+":
                    throw new BalanceOutOfRangeException(true, false, command);
                case "-":
                    throw new BalanceOutOfRangeException(false, true, command);
                case "S":
                    stable = true;
                    break;
                case "D":
                    stable = false;
                    break;
            }

            var value = double.Parse(response[2]);
            return new MettlerToledoWeightReading(stable, new Mass(value, UnitsNet.Units.MassUnit.Gram));
            // TODO: Weight unit is specified by response[3]
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Close();
        }
    }
}
