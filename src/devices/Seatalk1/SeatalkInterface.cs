// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Iot.Device.Seatalk1.Messages;

namespace Iot.Device.Seatalk1
{
    public class SeatalkInterface : IDisposable
    {
        private readonly SerialPort _port;
        private readonly Seatalk1Parser _parser;

        public event Action<SeatalkMessage>? MessageReceived;

        /// <summary>
        /// Creates a new instance of the SeatalkInterface, that provides high-level functions on a seatalk bus
        /// </summary>
        /// <param name="uart">The Uart name that connects to the bus, e.g. /dev/ttyAMA2 or COM5</param>
        /// <exception cref="ArgumentNullException">The uart string is null</exception>
        /// <exception cref="IOException">The port could not be opened</exception>
        /// <exception cref="UnauthorizedAccessException">The port could not be accessed. Maybe it is already open by another application?</exception>
        /// <remarks>
        /// Unlike other classes that communicate over a serial port, this object requires to own the serial port itself, as it needs to
        /// set it up in a special way and change the communication parameters during operation.
        /// </remarks>
        public SeatalkInterface(string uart)
        {
            if (uart == null)
            {
                throw new ArgumentNullException(nameof(uart));
            }

            _port = new SerialPort(uart);
            _port.BaudRate = 4800;
            _port.Parity = Parity.Even; // Can be anything but none, but "Mark" or "Space" won't have the desired effect on linux, causing garbage to be sent
            _port.StopBits = StopBits.One;
            _port.DataBits = 8;
            _port.Open();

            _parser = new Seatalk1Parser(_port.BaseStream);
            _parser.NewMessageDecoded += OnNewMessage;
            _parser.StartDecode();
        }

        public Seatalk1Parser Parser => _parser;

        private void OnNewMessage(SeatalkMessage obj)
        {
            MessageReceived?.Invoke(obj);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _parser.StopDecode();
                _parser.Dispose();
                _port.Close();
                _port.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
