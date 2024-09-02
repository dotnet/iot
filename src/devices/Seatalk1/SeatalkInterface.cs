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
    /// <summary>
    /// Seatalk interface handler.
    /// This class is the main handler for talking over a seatalk network.
    /// Only when also working with NMEA-0183, the <see cref="SeatalkToNmeaConverter"/> should be used instead.
    /// </summary>
    public class SeatalkInterface : MarshalByRefObject, IDisposable
    {
        private const Parity DefaultParity = Parity.Even;
        private readonly SerialPort _port;
        private readonly Seatalk1Parser _parser;
        private readonly Thread _watchDog;
        private readonly CancellationTokenSource _cancellation;
        private readonly AutoPilotRemoteController _autopilotController;
        private readonly object _lock;
        private bool _disposed;

        /// <summary>
        /// This event is fired with every received message
        /// </summary>
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
            : this()
        {
            if (uart == null)
            {
                throw new ArgumentNullException(nameof(uart));
            }

            _port = new SerialPort(uart);
            _port.BaudRate = 4800;
            _port.Parity = DefaultParity; // Can be anything but none, but "Mark" or "Space" won't have the desired effect on linux, causing garbage to be sent
            _port.StopBits = StopBits.One;
            _port.DataBits = 8;
            _port.Open();

            _parser = new Seatalk1Parser(_port.BaseStream);
            _parser.NewMessageDecoded += OnNewMessage;
        }

        /// <summary>
        /// This constructor is for mocking only!
        /// </summary>
        protected SeatalkInterface()
        {
            _lock = new object();
            _port = null!;
            _parser = null!;
            _autopilotController = new AutoPilotRemoteController(this);
            _cancellation = new CancellationTokenSource();
            _watchDog = new Thread(WatchDogStarter);
            _disposed = false;
        }

        /// <summary>
        /// Provides access to the internal parser
        /// </summary>
        public Seatalk1Parser Parser => _parser;

        /// <summary>
        /// Starts listening on the port
        /// </summary>
        public void StartDecode()
        {
            _parser.StartDecode();
            _watchDog.Start();
        }

        private static int BitCount(int b)
        {
            int count = 0;
            while (b != 0)
            {
                count++;
                b &= (b - 1); // walking through all the bits which are set to one
            }

            return count;
        }

        /// <summary>
        /// This calculates the correct parity for each byte in the datagram.
        /// </summary>
        /// <param name="data">The datagram to send (first byte is the command byte, remainder are the arguments)</param>
        /// <returns>A list of bytes with parity information</returns>
        /// <remarks>
        /// We need to send the first byte (the command byte) with a parity of "mark" (=1), all the remaining bytes with a parity of "space" (=0),
        /// but since Linux doesn't seem to properly support Mark or Space parity settings (on the raspberry pi, that setting results in no parity bit to be sent)
        /// we cheat and count what the parity bit should be and use even or odd to achieve the desired result.
        /// </remarks>
        internal static List<(byte B, Parity P, int Index)> CalculateParityForEachByte(byte[] data)
        {
            bool isCommandByte = true;
            List<(byte, Parity, int)> ret = new();
            for (int i = 0; i < data.Length; i++)
            {
                byte b = data[i];
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

                ret.Add((data[i], parityToSend, i));
                isCommandByte = false;
            }

            return ret;
        }

        /// <summary>
        /// Periodic watchdog tasks
        /// </summary>
        private void WatchDogStarter()
        {
            while (!_cancellation.IsCancellationRequested)
            {
                WatchDog();
                Thread.Sleep(1000);
            }
        }

        /// <summary>
        /// Watchdog function. Called at regular intervals for some housekeeping. The
        /// default implementation checks for lost connections with the autopilot.
        /// </summary>
        protected virtual void WatchDog()
        {
            _autopilotController.UpdateStatus();
        }

        internal void OnNewMessage(SeatalkMessage obj)
        {
            MessageReceived?.Invoke(obj);
        }

        /// <summary>
        /// Synchronously send a datagram. This is a low-level function, prefer <see cref="SendMessage"/> instead.
        /// </summary>
        /// <param name="data">The datagram</param>
        /// <returns>
        /// True on success, false otherwise. May fail e.g. if the bus is busy all the time.
        /// Todo: This doesn't do any checks for collisions just yet.
        /// </returns>
        public virtual bool SendDatagram(byte[] data)
        {
            // First calculate the required parity setting for each byte.
            var dataWithParity = CalculateParityForEachByte(data);

            // Only start sending if the buffer is empty. This should hopefully avoid to many collisions
            while (!_parser.IsBufferEmpty)
            {
                // Sending a message takes 1 - 2ms, so wait a bit (knowing that this sleep isn't precise)
                Thread.Sleep(1);
            }

            // Only one thread at a time, please!
            try
            {
                lock (_lock)
                {
                    foreach (var e in dataWithParity)
                    {
                        // The many flushes here appear to be necessary to make sure the parity correctly applies to the next byte we send (and only to the next)
                        if (_port.Parity != e.P)
                        {
                            _port.BaseStream.Flush();
                            _port.Parity = e.P;
                            _port.BaseStream.Flush();
                        }

                        _port.Write(data, e.Index, 1);
                    }

                    _port.BaseStream.Flush();
                    _port.Parity = DefaultParity;
                }
            }
            catch (InvalidOperationException)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Synchronously send the given message
        /// </summary>
        /// <param name="message">The message to send</param>
        /// <remarks>Transmission of Seatalk messages can be very slow and include considerable delays. Also, there's no guarantee the
        /// message is successfully transmitted or received anywhere.</remarks>
        public virtual bool SendMessage(SeatalkMessage message)
        {
            byte[] bytes = message.CreateDatagram();
            return SendDatagram(bytes);
        }

        /// <summary>
        /// Synchronously send the given message
        /// </summary>
        /// <param name="message">The message to send</param>
        /// <remarks>Transmission of Seatalk messages can be very slow and include considerable delays. Also, there's no guarantee the
        /// message is successfully transmitted or received anywhere.</remarks>
        public Task<bool> SendMessageAsync(SeatalkMessage message)
        {
            return Task.Factory.StartNew(() => SendMessage(message));
        }

        /// <summary>
        /// Get an interface to the Autopilot remote controller.
        /// </summary>
        /// <returns>An interface to monitor and control an Autopilot connected via Seatalk1</returns>
        public AutoPilotRemoteController GetAutopilotRemoteController() => _autopilotController;

        /// <summary>
        /// Configures the display backlight of all devices connected to the bus, as far as this is supported
        /// (e.g. the ST2000 autopilot only supports on or off)
        /// </summary>
        /// <param name="intensity">The new backlight level</param>
        public void SetLampIntensity(DisplayBacklightLevel intensity)
        {
            var msg = new SetLampIntensity(intensity);
            SendMessage(msg);
        }

        /// <summary>
        /// Default dispose method
        /// </summary>
        /// <param name="disposing">Always true</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _cancellation.Cancel();
                _parser.StopDecode();
                _parser.Dispose();
                _port.Close();
                _port.Dispose();
                _watchDog.Join();
                _cancellation.Dispose();
                _disposed = true;
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
