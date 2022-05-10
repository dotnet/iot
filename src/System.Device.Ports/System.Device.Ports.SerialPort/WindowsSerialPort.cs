// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Device.Ports.SerialPort
{
    internal class WindowsSerialPort : SerialPort
    {
        private const string DefaultPortName = "COM1";

        public WindowsSerialPort()
        {
            _portName = DefaultPortName;
        }

        protected internal override void SetBaudRate(int baudRate)
        {
            throw new NotImplementedException();
        }

        protected internal override void SetParity(Parity parity)
        {
            throw new NotImplementedException();
        }

        protected internal override void SetDataBits(int dataBits)
        {
            throw new NotImplementedException();
        }

        protected internal override void SetStopBits(StopBits stopBits)
        {
            throw new NotImplementedException();
        }

        protected internal override void SetBreakState(bool breakState)
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

        protected internal override int GetCDHolding()
        {
            throw new NotImplementedException();
        }

        protected internal override int GetCtsHolding()
        {
            throw new NotImplementedException();
        }

        protected internal override void SetDiscardNull(bool value)
        {
            throw new NotImplementedException();
        }

        protected internal override int GetDsrHolding()
        {
            throw new NotImplementedException();
        }

        protected internal override void SetDtrEnable(bool value)
        {
            throw new NotImplementedException();
        }

        protected internal override void SetHandshake(Handshake handshake)
        {
            throw new NotImplementedException();
        }

        protected internal override byte SetParityReplace(byte parityReplace)
        {
            throw new NotImplementedException();
        }

        protected internal override void SetReadTimeout(int timeout)
        {
            throw new NotImplementedException();
        }

        protected internal override void SetRtsEnable(bool rtsEnable)
        {
            throw new NotImplementedException();
        }

        protected internal override void SetWriteBufferSize(int writeBufferSize)
        {
            throw new NotImplementedException();
        }

        protected internal override void SetWriteTimeout(int writeTimeout)
        {
            throw new NotImplementedException();
        }
    }
}
