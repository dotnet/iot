using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#pragma warning disable CS1591

namespace Iot.Device.Arduino
{
    public abstract class ExtendedCommandHandler
    {
        private FirmataDevice? _firmata;

        protected ExtendedCommandHandler(SupportedMode? handlesMode)
        {
            HandlesMode = handlesMode;
        }

        protected ExtendedCommandHandler()
            : this(null)
        {
        }

        public SupportedMode? HandlesMode
        {
            get;
        }

        internal void Registered(FirmataDevice firmata)
        {
            _firmata = firmata;
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

        protected virtual bool OnSysexData(ReplyType type, byte[] data)
        {
            return false;
        }
    }
}
