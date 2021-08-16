using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Iot.Device.Common;
using Microsoft.Extensions.Logging;

#pragma warning disable CS1591

namespace Iot.Device.Arduino
{
    public abstract class ExtendedCommandHandler : IDisposable
    {
        private FirmataDevice? _firmata;
        private ArduinoBoard? _board;

        protected ExtendedCommandHandler(SupportedMode? handlesMode)
        {
            HandlesMode = handlesMode;
            Logger = this.GetCurrentClassLogger();
        }

        protected ExtendedCommandHandler()
            : this(null)
        {
        }

        public ILogger Logger
        {
            get;
        }

        public SupportedMode? HandlesMode
        {
            get;
        }

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

        internal void Registered(FirmataDevice firmata, ArduinoBoard board)
        {
            _firmata = firmata;
            _board = board;
            _firmata.OnSysexReply += OnSysexData;
        }

        protected internal virtual void OnConnected()
        {
        }

        protected void SendCommand(FirmataCommandSequence commandSequence)
        {
            if (_firmata == null)
            {
                throw new InvalidOperationException("Command handler not registered");
            }

            _firmata.SendCommand(commandSequence);
        }

        protected byte[] SendCommandAndWait(FirmataCommandSequence commandSequence, TimeSpan timeout)
        {
            if (_firmata == null)
            {
                throw new InvalidOperationException("Command handler not registered");
            }

            return _firmata.SendCommandAndWait(commandSequence, timeout);
        }

        protected byte[] SendCommandAndWait(FirmataCommandSequence commandSequence)
        {
            return SendCommandAndWait(commandSequence, FirmataDevice.DefaultReplyTimeout);
        }

        protected virtual void OnSysexData(ReplyType type, byte[] data)
        {
        }

        private void OnSysexDataInternal(ReplyType type, byte[] data)
        {
            if (_firmata == null)
            {
                return;
            }

            OnSysexData(type, data);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_firmata != null)
            {
                _firmata.OnSysexReply -= OnSysexDataInternal;
            }

            _firmata = null;
            _board = null;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
