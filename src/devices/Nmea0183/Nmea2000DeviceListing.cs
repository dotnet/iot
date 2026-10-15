// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Iot.Device.Nmea0183.Sentences;

#pragma warning disable CS1591
namespace Iot.Device.Nmea0183
{
    public class Nmea2000DeviceListing : NmeaSinkAndSource
    {
        private enum ComponentState
        {
            WaitingForConnection,
            AddressClaimSent,
            ProductInformationRequestSent,
            WaitingForData,
        }

        private DateTimeOffset _lastUpdate;
        private ConcurrentDictionary<uint, IsoAddressClaim> _devices;
        private ConcurrentDictionary<uint, ProductInformation> _deviceProductInformation;
        private ComponentState _state;

        public Nmea2000DeviceListing(string interfaceName)
            : base(interfaceName)
        {
            _state = ComponentState.WaitingForConnection;
            _lastUpdate = DateTimeOffset.UnixEpoch;
            _devices = new ConcurrentDictionary<uint, IsoAddressClaim>();
            _deviceProductInformation = new ConcurrentDictionary<uint, ProductInformation>();
            UpdateInterval = TimeSpan.FromMinutes(10);
        }

        public TimeSpan UpdateInterval
        {
            get;
            set;
        }

        public override void StartDecode()
        {
        }

        public override void SendSentence(NmeaSinkAndSource source, NmeaSentence sentence)
        {
            // Received a sentence (from the component's point of view)
            DateTimeOffset now = DateTimeOffset.UtcNow;
            if (now - _lastUpdate > UpdateInterval)
            {
                DispatchSentenceEvents(new IsoRequest(IsoAddressClaim.HexId));
                _lastUpdate = now;
                _state = ComponentState.AddressClaimSent;
            }

            if (sentence is IsoAddressClaim claim)
            {
                _devices[claim.MessageSource] = claim;
                if (_state != ComponentState.ProductInformationRequestSent)
                {
                    DispatchSentenceEvents(new IsoRequest(ProductInformation.HexId));
                    _state = ComponentState.ProductInformationRequestSent;
                }
            }

            if (sentence is ProductInformation productInformation)
            {
                _deviceProductInformation[productInformation.MessageSource] = productInformation;
                _state = ComponentState.WaitingForData;
            }
        }

        public List<Nmea2000DeviceInformation> GetDeviceList()
        {
            var ret = _devices.Select(x =>
                {
                    _deviceProductInformation.TryGetValue(x.Key, out ProductInformation? productInformation);
                    return new Nmea2000DeviceInformation(x.Value, productInformation);
                })
                .OrderBy(y => y.BusAddress)
                .ToList();
            return ret;
        }

        public override void StopDecode()
        {
        }
    }
}
