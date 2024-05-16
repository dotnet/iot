// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Device.Ports.SerialPort.Linux;
using System.Diagnostics.CodeAnalysis;
using System.IO;

using static System.Net.WebRequestMethods;

namespace System.Device.Ports.SerialPort
{
    internal partial class LinuxSerialPort : SerialPort
    {
        private const string DefaultPortName = "/dev/ttyUSB0";
        private TermiosIo? _tio;
        private FileStream? _file = null;

        public LinuxSerialPort()
        {
            PortName = DefaultPortName;
        }

        [MemberNotNull(nameof(_file))]
        [MemberNotNull(nameof(_tio))]
        private void Validate()
        {
            if (_file == null || _tio == null)
            {
                throw new InvalidOperationException($"The port {PortName} is closed");
            }
        }

        protected override void Dispose(bool disposing)
        {
            _file?.Dispose();
            _file = null;
            base.Dispose(disposing);
        }

        protected internal override void OpenPort()
        {
            _file = new(PortName, FileMode.Open, FileAccess.ReadWrite);
            _tio = new(_file.SafeFileHandle.DangerousGetHandle().ToInt32());
            _tio.CanonicalMode = false;
            _tio.Read();   // read the current values of the port after opening it

            throw new NotImplementedException();
        }

        protected internal override void ClosePort(bool disposing)
        {
            _file?.Close();
        }

        protected internal override void SetBaudRate(int value)
        {
            Validate();

            if (value <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }

            var speed = _tio.GetStandardSpeed(value);
            if (speed == 0)
            {
                speed = (uint)value;
            }

            try
            {
                _tio.CfSetISpeed(speed);
                _tio.CfSetOSpeed(speed);
                return;
            }
            catch (Exception)
            {
                // when the speed cannot be sent using the standard API
                // we can just go on and try setting it via ioctl
            }

            // This will write the termios structure to the driver using ioctl
            _tio.CustomBaudRate = speed;
        }

        protected internal override void SetParity(Parity value)
        {
            Validate();

            _tio.Parity = value;
            _tio.Write();
        }

        protected internal override void SetDataBits(int value)
        {
            Validate();

            _tio.DataBits = value;
            _tio.Write();
        }

        protected internal override void SetStopBits(StopBits value)
        {
            Validate();

            switch (value)
            {
                case StopBits.One:
                    _tio.StopBits = 1;
                    break;

                case StopBits.Two:
                    _tio.StopBits = 2;
                    break;

                case StopBits.None:
                case StopBits.OnePointFive:
                default:
                    throw new NotSupportedException($"Stop bits {value} is not supported on this platform");
            }

            _tio.Write();
        }

        protected internal override void SetBreakState(bool value)
        {
            Validate();

            // there is no "stop the break" in Linux
            if (value)
            {
                // 0 means default which is 250ms
                // https://github.com/torvalds/linux/blob/62bad54b26db8bc98e28749cd76b2d890edb4258/drivers/tty/tty_io.c#L2744
                _tio.TcSendBreak(0);
            }
        }

        protected internal override int GetBytesToRead()
        {
            Validate();
            return (int)_tio.AvailableBytes;
        }

        protected internal override int GetBytesToWrite()
        {
            Validate();
            return (int)_tio.OutputBufferBytes;
        }

        protected internal override bool GetCDHolding()
        {
            Validate();
            return _tio.ModemStatus.SignalCD;
        }

        protected internal override bool GetCtsHolding()
        {
            Validate();
            return _tio.ModemStatus.SignalCTS;
        }

        protected internal override bool GetDsrHolding()
        {
            Validate();
            return _tio.ModemStatus.SignalDSR;
        }

        protected internal override bool GetDtrEnable()
        {
            Validate();
            return _tio.ModemStatus.SignalDTR;
        }

        protected internal override bool GetRtsEnable()
        {
            Validate();
            return _tio.ModemStatus.SignalRTS;
        }

        protected internal override void SetDtrEnable(bool value)
        {
            Validate();
            _tio.ModemStatus.SignalDTR = value;
            _tio.ModemStatus.Write();
        }

        protected internal override void SetRtsEnable(bool value, bool setField)
        {
            Validate();
            _tio.ModemStatus.SignalRTS = value;
            _tio.ModemStatus.Write();
        }

        protected internal override void SetHandshake(Handshake value)
        {
            Validate();
            _tio.Handshake = value;
            _tio.Write();
        }

        protected internal override void SetDiscardNull(bool value)
        {
            throw new PlatformNotSupportedException(
                $"This platform do not allow discarding nulls");
        }

        protected internal override void SetParityReplace(byte value)
        {
            throw new PlatformNotSupportedException(
                $"This platform do not allow changing the character marking parity errors");
        }

        // TODO: get?
        protected internal override void SetReadTimeout(int value)
        {
            Validate();
            if (value == 0)
            {
                _tio.VTime = 0;
                _tio.VMin = 0;
                _tio.Write();
                return;
            }

            if (value == SerialPort.InfiniteTimeout)
            {
                _tio.VTime = 0;     // blocking read
                _tio.VMin = 255;    // number of bytes to read before returning
                _tio.Write();
                return;
            }

            _tio.VTime = (byte)(value * 10);
            _tio.VMin = 0;
            _tio.Write();
        }

        protected internal override void SetWriteTimeout(int value)
        {
            // TODO: async + cancel?
            throw new NotImplementedException();
        }

        public override void DiscardInBuffer()
        {
            Validate();
            _tio.TcFlush(LinuxConstants.TCIFLUSH);
        }

        public override void DiscardOutBuffer()
        {
            Validate();
            _tio.TcFlush(LinuxConstants.TCOFLUSH);
        }

        protected internal override void InitializeBuffers(int readBufferSize, int writeBufferSize)
        {
            // In Linux the serial port buffers are currently fixed to 4096 bytes (non-canonical mode)
            // https://elixir.bootlin.com/linux/latest/source/include/linux/tty.h#L247
            // https://elixir.bootlin.com/linux/latest/source/drivers/tty/n_tty.c#L1583
            // #define N_TTY_BUF_SIZE 4096
            //
            // This will not throw because the Platform-independent method calls this
            // throw new PlatformNotSupportedException($"Read and write buffers cannot be changed");
        }

        /// <summary>
        /// Sends the data which is pending in the out buffer
        /// </summary>
        public override void Flush()
        {
            Validate();
            _tio.TcDrain();
        }
    }
}
