// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Text;
using System.Device.Gpio;
using System.Device.I2c;
using System.Device.Spi;
using System.IO.Ports;
using UnitsNet;

namespace Iot.Device.MettlerToledo
{
    /// <summary>
    /// Provides an interface for interacting with MT-SACS devices, like scales.
    /// </summary>
    public class MettlerToledoDevice : IDisposable
    {
        private SerialPort _serialPort;

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
        public void Open()
        {
            _serialPort.Open();
        }

        /// <summary>
        /// Closes the connection to the MT-SACS scale.
        /// </summary>
        public void Close()
        {
            _serialPort.Close();
        }

        /// <summary>
        /// Zeroes the balance/scale. If <paramref name="immediately"/> is set to true, will balance regardless of stability.
        /// </summary>
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
                    throw new Exception("Unknown result received");
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
        public void Reset()
        {
            var response = SendCommandWithResponse("@");
            if (response[0] != "I4")
            {
                throw new Exception("Unknown result received");
            }
        }

        /// <summary>
        /// Writes text to the balance display
        /// </summary>
        /// <param name="text">Text to write to the display</param>
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
                    throw new Exception("Unknown result received");
            }
        }

        /// <summary>
        /// Clears any text that was written to the display
        /// </summary>
        public void ClearDisplay()
        {
            WriteToDisplay(string.Empty);
        }

        /// <summary>
        /// Changes the display to show the current weight
        /// </summary>
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
                    throw new Exception("Unknown result received");
            }
        }

        /// <summary>
        /// Have the scale send an event every time the weight is changed
        /// </summary>
        public void SubscribeToWeightEvents()
        {
            throw new NotImplementedException();
        }

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

    /// <summary>
    /// Weight reading returned by Mettler Toledo scales
    /// </summary>
    public class MettlerToledoWeightReading
    {
        /// <summary>
        /// Scale returned this weight value is stable
        /// </summary>
        public bool Stable { get; }

        /// <summary>
        /// Weight returned by the scale
        /// </summary>
        public Mass Weight { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MettlerToledoWeightReading"/> class.
        /// </summary>
        /// <param name="stable">Whether this value is stable</param>
        /// <param name="weight">Weight returned by scale</param>
        public MettlerToledoWeightReading(bool stable, Mass weight) => (Stable, Weight) = (stable, weight);
    }

    /// <summary>
    /// A generic Mettler Toledo exception
    /// </summary>
    public abstract class MettlerToledoException : System.Exception
    {
        /// <summary>
        /// The command that triggered this exception
        /// </summary>
        public string Command { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MettlerToledoException"/> class.
        /// </summary>
        /// <param name="command">Command that triggered this exception</param>
        public MettlerToledoException(string command) => Command = command;
    }

    /// <summary>
    /// Occurs when a scale returns a Syntax Error.
    /// </summary>
    public class SyntaxErrorException : MettlerToledoException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SyntaxErrorException"/> class.
        /// </summary>
        /// <param name="command">Command that triggered this exception</param>
        public SyntaxErrorException(string command)
            : base(command)
        {
        }
    }

    /// <summary>
    /// Occurs when a scale returns a Transmission Error.
    /// </summary>
    public class TransmissionErrorException : MettlerToledoException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TransmissionErrorException"/> class.
        /// </summary>
        /// <param name="command">Command that triggered this exception</param>
        public TransmissionErrorException(string command)
            : base(command)
        {
        }
    }

    /// <summary>
    /// Occurs when a scale returns a Logical Error.
    /// </summary>
    public class LogicalErrorException : MettlerToledoException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LogicalErrorException"/> class.
        /// </summary>
        /// <param name="command">Command that triggered this exception</param>
        public LogicalErrorException(string command)
            : base(command)
        {
        }
    }

    /// <summary>
    /// Occurs when a scale returns a Command Not Executable error. This is typically because the scale is currently running a different command or a timeout occurred.
    /// </summary>
    public class CommandNotExecutableException : MettlerToledoException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommandNotExecutableException"/> class.
        /// </summary>
        /// <param name="command">Command that triggered this exception</param>
        public CommandNotExecutableException(string command)
            : base(command)
        {
        }
    }

    /// <summary>
    /// Occurs when a scale returns a Balance Overload or Balance Underload error.
    /// </summary>
    public class BalanceOutOfRangeException : MettlerToledoException
    {
        /// <summary>
        /// Scale returned it is overloaded.
        /// </summary>
        public bool IsOverloaded { get; }

        /// <summary>
        /// Scale returned it is underloaded.
        /// </summary>
        public bool IsUnderloaded { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BalanceOutOfRangeException"/> class.
        /// </summary>
        /// <param name="_overloaded">Scale returned overloaded</param>
        /// <param name="_underloaded">Scale returned underloaded</param>
        /// <param name="command">Command that triggered this exception</param>
        public BalanceOutOfRangeException(bool _overloaded, bool _underloaded, string command)
            : base(command)
        {
            IsOverloaded = _overloaded;
            IsUnderloaded = _underloaded;
        }
    }
}
