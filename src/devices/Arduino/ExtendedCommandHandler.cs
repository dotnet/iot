// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Iot.Device.Common;
using Microsoft.Extensions.Logging;

namespace Iot.Device.Arduino
{
    /// <summary>
    /// Base class for specific command handlers for the Arduino firmware
    /// This class can be derived to support special features of the Arduino firmware
    /// for a specific board. See <see cref="DhtSensor"/> or <see cref="FrequencySensor"/> as examples.
    /// See https://github.com/firmata/ConfigurableFirmata for a list of possible extensions.
    /// </summary>
    public abstract class ExtendedCommandHandler : IDisposable
    {
        private FirmataDevice? _firmata;
        private ArduinoBoard? _board;

        /// <summary>
        /// Constructs an instance of this class.
        /// </summary>
        /// <param name="handlesMode">The pin mode that this handler uses. Can be null for software-only
        /// modules (such as the FirmataScheduler)</param>
        protected ExtendedCommandHandler(SupportedMode? handlesMode)
        {
            HandlesMode = handlesMode;
            Logger = this.GetCurrentClassLogger();
        }

        /// <summary>
        /// Constructs an instance of this class without a specific pin assignment.
        /// </summary>
        protected ExtendedCommandHandler()
            : this(null)
        {
        }

        /// <summary>
        /// The class-specific logger instance
        /// </summary>
        public ILogger Logger
        {
            get;
        }

        /// <summary>
        /// The pin mode this handler supports.
        /// </summary>
        public SupportedMode? HandlesMode
        {
            get;
        }

        /// <summary>
        /// Returns true if this command handler is registered.
        /// This might need to be checked in Dispose, to make sure an uninitialized component doesn't attempt
        /// to send a command.
        /// </summary>
        protected bool IsRegistered => _board != null;

        /// <summary>
        /// The reference to the arduino board
        /// </summary>
        public ArduinoBoard Board
        {
            get
            {
                if (_board == null)
                {
                    throw new InvalidOperationException("Command handler is not ready");
                }

                return _board;
            }
        }

        /// <summary>
        /// This is internally called when the command handler is registered
        /// </summary>
        internal void Registered(FirmataDevice firmata, ArduinoBoard board)
        {
            _firmata = firmata;
            _board = board;
            _firmata.OnSysexReply += OnSysexDataInternal;
        }

        /// <summary>
        /// This method is called when a connection to the hardware is
        /// established.
        /// </summary>
        protected internal virtual void OnConnected()
        {
        }

        /// <summary>
        /// Sends a command to the device, not expecting an answer.
        /// </summary>
        /// <param name="commandSequence">A command sequence. This
        /// should normally be a sysex command.</param>
        protected void SendCommand(FirmataCommandSequence commandSequence)
        {
            if (_firmata == null)
            {
                throw new InvalidOperationException("Command handler not registered");
            }

            _firmata.SendCommand(commandSequence);
        }

        /// <summary>
        /// Send a command to the device, expecting a reply.
        /// </summary>
        /// <param name="commandSequence">Command to send. This
        /// should normally be a sysex command.</param>
        /// <param name="timeout">Command timeout</param>
        /// <param name="error">An error code in case of a failure</param>
        /// <exception cref="TimeoutException">The timeout elapsed before a reply was received.</exception>
        /// <returns>The reply packet</returns>
        protected byte[] SendCommandAndWait(FirmataCommandSequence commandSequence, TimeSpan timeout, out CommandError error)
        {
            if (_firmata == null)
            {
                throw new InvalidOperationException("Command handler not registered");
            }

            return _firmata.SendCommandAndWait(commandSequence, timeout, IsMatchingAck, out error);
        }

        /// <summary>
        /// Send a command to the device, expecting a reply.
        /// </summary>
        /// <param name="commandSequences">Commands to send. This
        /// should normally be a sysex command.</param>
        /// <param name="timeout">Command timeout</param>
        /// <param name="error">An error code in case of a failure</param>
        /// <exception cref="TimeoutException">The timeout elapsed before a reply was received.</exception>
        /// <returns>True if all packets where send and properly acknowledged</returns>
        protected bool SendCommandsAndWait(IList<FirmataCommandSequence> commandSequences, TimeSpan timeout, out CommandError error)
        {
            if (_firmata == null)
            {
                throw new InvalidOperationException("Command handler not registered");
            }

            return _firmata.SendCommandsAndWait(commandSequences, timeout, IsMatchingAck, HasCommandError, out error);
        }

        /// <summary>
        /// Send a command to the device, expecting a reply.
        /// </summary>
        /// <param name="commandSequence">Command to send. This
        /// should normally be a sysex command.</param>
        /// <param name="timeout">Command timeout</param>
        /// <exception cref="TimeoutException">The timeout elapsed before a reply was received.</exception>
        /// <returns>The reply packet</returns>
        protected byte[] SendCommandAndWait(FirmataCommandSequence commandSequence, TimeSpan timeout)
        {
            return SendCommandAndWait(commandSequence, timeout, out _);
        }

        /// <summary>
        /// Send a command to the device, expecting a reply. This uses a default timeout.
        /// </summary>
        /// <param name="commandSequence">Command to send. This
        /// should normally be a sysex command.</param>
        /// <exception cref="TimeoutException">The timeout elapsed before a reply was received.</exception>
        /// <returns>The reply packet</returns>
        protected byte[] SendCommandAndWait(FirmataCommandSequence commandSequence)
        {
            return SendCommandAndWait(commandSequence, FirmataDevice.DefaultReplyTimeout);
        }

        /// <summary>
        /// This is called when a sysex command is received from the board.
        /// This can include the reply to a command sent by a <see cref="SendCommandAndWait(FirmataCommandSequence)"/> before, in which case
        /// the reply should be ignored, as it is returned as result of the call itself. Therefore it is advised to use this function only
        /// to listen for data sent by the device automatically (e.g event messages or recurring status reports)
        /// </summary>
        /// <param name="type">Type of data received from the hardware. This should normally be <see cref="ReplyType.SysexCommand"/>,
        /// unless the hardware sends unencoded Ascii messages</param>
        /// <param name="data">The binary representation of the received data</param>
        /// <remarks>The implementation needs to check the type and source of the data. The messages are not filtered by requester!</remarks>
        protected virtual void OnSysexData(ReplyType type, byte[] data)
        {
        }

        /// <summary>
        /// This method is called to check whether the reply is a valid ACK/NOACK for the given command sequence.
        /// Can be used to avoid accepting something as command reply that is completely unrelated (such as an asynchronous callback).
        /// In different words, this should return false if the given reply is not something that is an answer to a synchronous command.
        /// </summary>
        /// <param name="sequence">The sequence that was sent</param>
        /// <param name="reply">The reply</param>
        /// <returns>True if this reply matches the sequence. True is the default, for backwards compatibility</returns>
        protected virtual bool IsMatchingAck(FirmataCommandSequence sequence, byte[] reply) => true;

        /// <summary>
        /// Callback function that returns whether the given reply indicates an error
        /// </summary>
        /// <param name="sequence">The original sequence</param>
        /// <param name="reply">The reply. <see cref="IsMatchingAck"/> is already tested to be true for this reply</param>
        /// <returns>A command error code, in case this reply indicates a no-acknowledge</returns>
        protected virtual CommandError HasCommandError(FirmataCommandSequence sequence, byte[] reply) => CommandError.None;

        private void OnSysexDataInternal(ReplyType type, byte[] data)
        {
            if (_firmata == null)
            {
                return;
            }

            OnSysexData(type, data);
        }

        /// <summary>
        /// Disposes this instance
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (_firmata != null)
            {
                _firmata.OnSysexReply -= OnSysexDataInternal;
            }

            _firmata = null;
            _board = null;
        }

        /// <summary>
        /// Called by the infrastructure when the parser reports an error or information message.
        /// The default implementation does nothing.
        /// </summary>
        /// <param name="message">The message text</param>
        /// <param name="exception">The exception observed (may be null)</param>
        protected internal virtual void OnErrorMessage(string message, Exception? exception)
        {
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
