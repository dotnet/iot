// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Iot.Device.Seatalk1.Messages;

namespace Iot.Device.Seatalk1
{
    public class SeatalkInterface : IDisposable
    {
        private readonly SerialPort _port;
        private readonly Seatalk1Parser _parser;
        private readonly Thread _watchDog;
        private readonly CancellationTokenSource _cancellation;
        private readonly AutoPilotRemoteController _autopilotController;

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

            _autopilotController = new AutoPilotRemoteController(this);

            _cancellation = new CancellationTokenSource();
            _watchDog = new Thread(WatchDog);
            _watchDog.Start();
        }

        public Seatalk1Parser Parser => _parser;

        /// <summary>
        /// Periodic watchdog tasks
        /// </summary>
        private void WatchDog()
        {
            while (!_cancellation.IsCancellationRequested)
            {
                Thread.Sleep(1000);
                _autopilotController.UpdateStatus();
            }
        }

        private void OnNewMessage(SeatalkMessage obj)
        {
            MessageReceived?.Invoke(obj);
        }

        private int BitCount(int b)
        {
            int count = 0;
            while (b != 0)
            {
                count++;
                b &= (b - 1); // walking through all the bits which are set to one
            }

            return count;
        }

        public void SendDatagram(byte[] data)
        {
            // Send byte-by-byte
            bool isCommandByte = true;
            for (int i = 0; i < data.Length; i++)
            {
                byte b = data[i];
                // We need to send the first byte (the command byte) with a parity of "mark", all the remaining bytes with a parity of "space",
                // but since Linux doesn't seem to properly support Mark or Space (on the raspberry pi, that setting results in no parity bit to be sent)
                // we cheat here and count what the parity bit should be and use even or odd to achieve the desired result.
                bool isEven = BitCount(b) % 2 == 0;
                Parity parityToSend;
                if (isEven)
                {
                    if (isCommandByte)
                    {
                        // command byte so far is even, we need the parity bit to be 1, so use odd parity
                        // (because the parity setting "odd" means that the data bits including the parity bit count to odd)
                        parityToSend = Parity.Odd;
                    }
                    else
                    {
                        parityToSend = Parity.Even;
                    }
                }
                else
                {
                    if (isCommandByte)
                    {
                        parityToSend = Parity.Even;
                    }
                    else
                    {
                        parityToSend = Parity.Odd;
                    }
                }

                // parityToSend = parityToSend == Parity.Odd ? Parity.Even : Parity.Odd;
                _port.Parity = parityToSend;
                _port.Write(data, i, 1);
                while (_port.BytesToWrite != 0)
                {
                    Thread.Yield();
                }

                isCommandByte = false;
            }
        }

        /// <summary>
        /// Get an interface to the Autopilot remote controller.
        /// </summary>
        /// <returns>An interface to monitor and control an Autopilot connected via Seatalk1</returns>
        public AutoPilotRemoteController GetAutopilotRemoteController() => _autopilotController;

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _cancellation.Cancel();
                _parser.StopDecode();
                _parser.Dispose();
                _port.Close();
                _port.Dispose();
                _watchDog.Join();
                _cancellation.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
