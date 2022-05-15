// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Device.Ports.SerialPort
{
    internal partial class LinuxSerialPort : SerialPort
    {
        private const string DefaultPortName = "/dev/tty0";

        public LinuxSerialPort()
        {
            _portName = DefaultPortName;
        }

        protected internal override void SetBaudRate(int value)
        {
            throw new NotImplementedException();
        }

        protected internal override void SetParity(Parity value)
        {
            throw new NotImplementedException();
        }

        protected internal override void SetDataBits(int value)
        {
            throw new NotImplementedException();
        }

        protected internal override void SetStopBits(StopBits value)
        {
            throw new NotImplementedException();
        }

        protected internal override void SetBreakState(bool value)
        {
            throw new NotImplementedException();
        }

        protected internal override int GetBytesToRead()
        {
            throw new NotImplementedException();
        }

        protected internal override int GetBytesToWrite()
        {
            throw new NotImplementedException();
        }

        protected internal override bool GetCDHolding()
        {
            throw new NotImplementedException();
        }

        protected internal override bool GetCtsHolding()
        {
            throw new NotImplementedException();
        }

        protected internal override void SetDiscardNull(bool value)
        {
            throw new NotImplementedException();
        }

        protected internal override bool GetDsrHolding()
        {
            throw new NotImplementedException();
        }

        protected internal override bool GetDtrEnable()
        {
            throw new NotImplementedException();
        }

        protected internal override void SetDtrEnable(bool value)
        {
            throw new NotImplementedException();
        }

        protected internal override void SetHandshake(Handshake value)
        {
            throw new NotImplementedException();
        }

        protected internal override void SetParityReplace(byte value)
        {
            throw new NotImplementedException();
        }

        protected internal override void SetReadTimeout(int value)
        {
            throw new NotImplementedException();
        }

        protected internal override bool GetRtsEnable()
        {
            throw new NotImplementedException();
        }

        protected internal override void SetRtsEnable(bool value, bool setField)
        {
            throw new NotImplementedException();
        }

        protected internal override void SetWriteBufferSize(int value)
        {
            throw new NotImplementedException();
        }

        protected internal override void SetWriteTimeout(int value)
        {
            throw new NotImplementedException();
        }

        protected internal override void OpenPort()
        {
            throw new NotImplementedException();
        }

        protected internal override void ClosePort(bool disposing)
        {
            throw new NotImplementedException();
        }

        public override void DiscardInBuffer()
        {
            throw new NotImplementedException();
        }

        public override void DiscardOutBuffer()
        {
            throw new NotImplementedException();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        protected internal override void InitializeBuffers(int readBufferSize, int writeBufferSize)
        {
            throw new NotImplementedException();
        }
    }
}
