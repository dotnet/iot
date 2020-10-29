// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO.Ports;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

using Iot.Device.MettlerToledo.Exceptions;
using Iot.Device.MettlerToledo.Messages;
using Iot.Device.MettlerToledo.Readings;
using UnitsNet;

namespace Iot.Device.MettlerToledo
{
    /// <summary>
    /// <see cref="MettlerToledoDevice.WeightUpdated"/>
    /// </summary>
    /// <param name="sender">Object where the event occurred.</param>
    /// <param name="e">Event data.</param>
    public delegate void WeightUpdatedHandler(object sender, MettlerToledoWeightReading e);

    /// <summary>
    /// Provides an interface for interacting with MT-SACS devices, like scales.
    /// </summary>
    public class MettlerToledoDevice : IDisposable
    {
        // TODO: we need to test on a real scale to determine what the response for other weights are when they are set to Unit 1
        private static readonly (string mtAbbreviation, MettlerToledoWeightUnit mtUnit, UnitsNet.Units.MassUnit unitsNetUnit)[] _unitMapping =
        {
            ("g", MettlerToledoWeightUnit.Grams, UnitsNet.Units.MassUnit.Gram),
            ("kg", MettlerToledoWeightUnit.Kilograms, UnitsNet.Units.MassUnit.Kilogram),
            ("mg", MettlerToledoWeightUnit.Milligrams, UnitsNet.Units.MassUnit.Milligram)
        };

        private SerialPort _serialPort;
        private bool _shouldDisposeSerialPort = true;

        /// <summary>
        /// Called when the scale reports a new weight, outside of the response to a command. Typically this is due to a call to <see cref="SubscribeToWeightAtIntervals()"/> or <see cref="SubscribeToWeightChangeEvents()"/>
        /// </summary>
        public event WeightUpdatedHandler WeightUpdated;

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
            _serialPort.DiscardInBuffer();
            _serialPort.DiscardOutBuffer();
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
            var command = immediately ? Commands.ZERO_BALANCE_IMMEDIATELY : Commands.ZERO_BALANCE;
            var response = SendCommandWithResponse(command);
            switch (response[1])
            {
                case Responses.COMMAND_NOT_EXECUTABLE:
                    throw new CommandNotExecutableException(command);
                case Responses.Range.UPPER_RANGE_EXCEEDED:
                    throw new BalanceOutOfRangeException(true, false, command);
                case Responses.Range.LOWER_RANGE_EXCEEDED:
                    throw new BalanceOutOfRangeException(false, true, command);
                case Responses.COMMAND_EXECUTED: // non-force stable success
                    return true;
                case Responses.Success.STABLE_SUCCESS: // force stable success
                    return true;
                case Responses.Success.DYNAMIC_SUCCESS: // force non-stable success
                    return false;
                default:
                    throw new UnknownResultException(command);
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
            var response = SendCommandWithResponse(Commands.RESET_BALANCE);
            // Scale will always print the serial number when powered on.
            if (response[0] != Commands.GET_SERIAL_NUMBER)
            {
                throw new UnknownResultException(Commands.RESET_BALANCE);
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

            var command = $"{Commands.WRITE_TO_BALANCE_DISPLAY} \"{text}\"";
            var response = SendCommandWithResponse(command);
            switch (response[1])
            {
                case Responses.COMMAND_EXECUTED:
                    break;
                case Responses.COMMAND_NOT_EXECUTABLE:
                    throw new CommandNotExecutableException(command);
                case Responses.PARAMETERS_MISSING:
                    throw new CommandNotExecutableException(command); // todo: maybe show a different error?
                default:
                    throw new UnknownResultException(command);
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
            var command = Commands.DISPLAY_WEIGHT;
            var response = SendCommandWithResponse(command);
            switch (response[1])
            {
                case Responses.COMMAND_EXECUTED:
                    break;
                case Responses.COMMAND_NOT_EXECUTABLE:
                    throw new CommandNotExecutableException(command);
                default:
                    throw new UnknownResultException(command);
            }
        }

        /// <summary>
        /// Have the scale send an event every time the weight is changed. <see cref="WeightUpdated"/> will be called every time this occurs.
        /// </summary>
        public void SubscribeToWeightChangeEvents()
        {
            _serialPort.WriteLine(Commands.SEND_WEIGHT_ON_VAL_CHANGE);
        }

        private void SubscribeToWeightChangeEvents(Mass minChange)
        {
            // TODO: see page 26 of manual (https://www.mt.com/mt_ext_files/Editorial/Generic/7/MT-SICS_for_Excellence_Balances_BA_Editorial-Generic_1116311007471_files/Excellence-SICS-BA-e-11780711B.pdf)
            throw new NotImplementedException();
        }

        /// <summary>
        /// Have the scale send its current weight at intervals, regardless of stability. <see cref="WeightUpdated"/> will be called every time this occurs.
        /// </summary>
        public void SubscribeToWeightAtIntervals()
        {
            _serialPort.WriteLine(Commands.SEND_IMMEDIATE_WEIGHT_VALUE_AND_REPEAT);
        }

        /// <summary>
        /// Sends a command over the serial port.
        /// </summary>
        /// <param name="command">Command to send</param>
        /// <param name="responseCode">Response code we want to receive.</param>
        /// <returns>Result of the command.</returns>
        /// <exception cref="InvalidOperationException" />
        /// <exception cref="TimeoutException">Occurs if the serial port times out while sending or receiving.</exception>
        /// <exception cref="SyntaxErrorException" />
        /// <exception cref="TransmissionErrorException" />
        /// <exception cref="LogicalErrorException" />
        private string[] SendCommandWithResponse(string command, string responseCode)
        {
            _serialPort.WriteLine(command);
            var result = ReadResponse(command, responseCode);
            return result;
        }

        private string[] SendCommandWithResponse(string command)
        {
            return SendCommandWithResponse(command, command.Split(' ')[0]);
        }

        private string[] ReadResponse(string command, string responseCode, TimeSpan? timeout = null)
        {
            var start = DateTime.Now;
            while (timeout == null || DateTime.Now - start < timeout)
            {
                // TODO: this timeout isn't accurate when timeout < _serialPort.ReadTimeout, because time may have elapsed already
                var line = timeout == null ? _serialPort.ReadLine() : ReadSerialPortWithTimeout(start - DateTime.Now + timeout.Value);
                var split = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                switch (split[0])
                {
                    case Errors.SYNTAX_ERROR:
                        throw new SyntaxErrorException(command);
                    case Errors.TRANSMISSION_ERROR:
                        throw new TransmissionErrorException(command);
                    case Errors.LOGICAL_ERROR:
                        throw new LogicalErrorException(command);
                }

                if (split[0] == responseCode)
                {
                    return split;
                }
                else
                {
                    // if we're listening for weight change events, we will receive "S" responses periodically
                    if (split[0] == Responses.WEIGHT_UPDATE)
                    {
                        WeightUpdated?.Invoke(this, ParseWeightReading(split));
                    }
                    else
                    {
                        throw new InvalidDataReceivedException(command, line);
                    }
                }
            }

            throw new TimeoutException();
        }

        private string ReadSerialPortWithTimeout(TimeSpan timeout)
        {
            if (timeout.TotalMilliseconds > _serialPort.ReadTimeout)
            {
                timeout = TimeSpan.FromMilliseconds(_serialPort.ReadTimeout);
            }

            var task = Task.Run(() => _serialPort.ReadLine());
            if (task.Wait(timeout))
            {
                return task.Result;
            }

            throw new TimeoutException();
        }

        private MettlerToledoWeightReading GetWeightReading(bool forceImmediate = false)
        {
            var command = forceImmediate ? Commands.SEND_IMMEDIATE_WEIGHT_VALUE : Commands.SEND_STABLE_WEIGHT_VALUE;
            var response = SendCommandWithResponse(command, Responses.WEIGHT_UPDATE);

            // Check if we have an error
            switch (response[1])
            {
                case Responses.COMMAND_NOT_EXECUTABLE:
                    throw new CommandNotExecutableException(command);
                case Responses.Range.UPPER_RANGE_EXCEEDED:
                    throw new BalanceOutOfRangeException(true, false, command);
                case Responses.Range.LOWER_RANGE_EXCEEDED:
                    throw new BalanceOutOfRangeException(false, true, command);
            }

            return ParseWeightReading(response);
        }

        private MettlerToledoWeightReading ParseWeightReading(string[] response)
        {
            var value = double.Parse(response[2]);
            var unit = _unitMapping.First(x => x.mtAbbreviation == response[3]).unitsNetUnit;

            bool stable = false;
            if (response[1] == Responses.Success.STABLE_SUCCESS)
            {
                stable = true;
            }
            else if (response[1] != Responses.Success.DYNAMIC_SUCCESS)
            {
                throw new ArgumentException("Did not contain a valid stable value.", nameof(response));
            }

            return new MettlerToledoWeightReading(stable, new Mass(value, unit));
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Close();
            if (_shouldDisposeSerialPort)
            {
                _serialPort.Dispose();
            }
        }
    }
}
