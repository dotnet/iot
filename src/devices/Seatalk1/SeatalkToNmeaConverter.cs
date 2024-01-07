// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Iot.Device.Common;
using Iot.Device.Nmea0183;
using Iot.Device.Nmea0183.Sentences;
using Iot.Device.Seatalk1.Messages;
using Microsoft.Extensions.Logging;

namespace Iot.Device.Seatalk1
{
    public class SeatalkToNmeaConverter : NmeaSinkAndSource
    {
        private SeatalkInterface _seatalkInterface;
        private bool _isDisposed;
        private ILogger _logger;

        public SeatalkToNmeaConverter(string interfaceName, string portName)
            : base(interfaceName)
        {
            _logger = this.GetCurrentClassLogger();
            _seatalkInterface = new SeatalkInterface(portName);
            _seatalkInterface.MessageReceived += SeatalkMessageReceived;
            _isDisposed = false;
        }

        private void SeatalkMessageReceived(SeatalkMessage stalk)
        {
            var nmeaMsg = new SeatalkNmeaMessage(stalk.CreateDatagram(), DateTimeOffset.UtcNow);
            _logger.LogDebug($"Received Seatalk message: {stalk}");
            DispatchSentenceEvents(nmeaMsg);
        }

        public override void StartDecode()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(nameof(SeatalkToNmeaConverter));
            }

            _seatalkInterface.StartDecode();
        }

        public override void SendSentence(NmeaSinkAndSource source, NmeaSentence sentence)
        {
            if (sentence.Valid && sentence.SentenceId == SeatalkNmeaMessage.Id)
            {
                // Since we get all commands we send out back on this interface, we must make sure we're not bouncing those again
                if (sentence is SeatalkNmeaMessage msg && source != this)
                {
                    var data = msg.Datagram;
                    _seatalkInterface.SendDatagram(data);
                }
            }
        }

        public override void StopDecode()
        {
            _seatalkInterface.Dispose();
            _isDisposed = true;
        }
    }
}
