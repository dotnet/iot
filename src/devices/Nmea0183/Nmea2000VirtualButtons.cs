// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Iot.Device.Nmea0183.Sentences;
using Microsoft.Extensions.Logging;

#pragma warning disable CS1591
namespace Iot.Device.Nmea0183
{
    /// <summary>
    /// This class provides a set of 4 virtual buttons that can be used to trigger events
    /// from a NMEA2000-capable modern plotter. It tries to emulate a Yacht-Devices YDCC-04,
    /// therefore you can download the corresponding UI from the Yacht-Devices website and use it with this class,
    /// see https://www.yachtd.com/products/ds/?czone. Choose "Circuit control", and enter a virtual
    /// serial number (you can use the default of 260001). After importing the zip file, you get
    /// another page in the "Data" menu of your plotter with 4 buttons named according to your input.
    /// If the page shows a configuration error, run the "Init" command, then reboot the plotter.
    /// </summary>
    public class Nmea2000VirtualButtons : NmeaSinkAndSource
    {
        private readonly ushort _buttonOffset;
        private readonly Dictionary<int, SwitchStatus> _switches;
        private readonly CancellationTokenSource _cancellationTokenSource;

        private Thread? _updateThread;

        /// <summary>
        /// Source to monitor or null to use all matching input
        /// </summary>
        private string? Nmea2000Source
        {
            get;
        }

        public byte DipSwitch { get; }

        public event Action<int, SwitchStatus>? SwitchStatusChanged;

        /// <summary>
        /// Create an instance of this class
        /// </summary>
        /// <param name="interfaceName">Name of this interface (for filtering purposes)</param>
        /// <param name="nmea2000Source">Name of the interface providing the NMEA2000 communication, can
        /// be null if filtering is provided externally</param>
        /// <param name="dipSwitch">Dip switch of the module. Try the value 2</param>
        /// <param name="buttonOffset">Offset the controller uses when sending state change commands</param>
        /// <param name="initialStatus0">Initial status of first switch</param>
        /// <param name="initialStatus1">Initial status of second switch</param>
        /// <param name="initialStatus2">Initial status of third switch</param>
        /// <param name="initialStatus3">Initial status of fourth switch</param>
        public Nmea2000VirtualButtons(string interfaceName, string? nmea2000Source, byte dipSwitch,
            ushort buttonOffset = 5,
            SwitchStatus initialStatus0 = SwitchStatus.Off,
            SwitchStatus initialStatus1 = SwitchStatus.Off,
            SwitchStatus initialStatus2 = SwitchStatus.Off,
            SwitchStatus initialStatus3 = SwitchStatus.Off)
            : base(interfaceName)
        {
            _switches = new Dictionary<int, SwitchStatus>();
            _switches[0] = initialStatus0;
            _switches[1] = initialStatus1;
            _switches[2] = initialStatus2;
            _switches[3] = initialStatus3;
            _buttonOffset = buttonOffset;
            Nmea2000Source = nmea2000Source;
            DipSwitch = dipSwitch;

            _cancellationTokenSource = new CancellationTokenSource();
        }

        public SwitchStatus GetSwitchStatus(int channel)
        {
            if (_switches.TryGetValue(channel, out var status))
            {
                return status;
            }

            throw new ArgumentOutOfRangeException(nameof(channel), $"No such channel: {channel}");
        }

        /// <summary>
        /// Externally change the staus of the given switch.
        /// This will trigger a status change event, but only if the state actually changed8 (to prevent an event loop)
        /// </summary>
        /// <param name="channel">Channel to update (0-3)</param>
        /// <param name="newStatus">The new status of the switch (on or off)</param>
        /// <exception cref="ArgumentOutOfRangeException">Invalid channel number</exception>
        public void SetSwitchStatus(int channel, SwitchStatus newStatus)
        {
            if (_switches.TryGetValue(channel, out var status))
            {
                if (status != newStatus)
                {
                    _switches[channel] = newStatus;
                    SwitchStatusChanged?.Invoke(channel, newStatus);
                }
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(channel), $"No such channel: {channel}");
            }
        }

        /// <summary>
        /// Make the given serial number known to the plotter. It seems this command
        /// must be sent exactly once when the plotter is on. Repeating it causes a configuration error.
        /// There's some confusion about the dipswitch value, as it may need to be set to anything _but_ the
        /// value otherwise used in this class.
        /// </summary>
        /// <param name="serialNumber">Serial number to make known. Try 260001.</param>
        /// <param name="dipswitch">Dipswitch value, see above</param>
        public void Init(uint serialNumber, byte dipswitch)
        {
            CzoneModuleAnnounce announce = new CzoneModuleAnnounce(serialNumber, dipswitch);
            DispatchSentenceEvents(announce);
        }

        public override void SendSentence(NmeaSinkAndSource source, NmeaSentence sentence)
        {
            // We received a sentence, check if we can handle it
            if (Nmea2000Source == null || source.InterfaceName == Nmea2000Source)
            {
                if (sentence is CzoneCircuitControl cc)
                {
                    cc.ButtonOffset = _buttonOffset;
                    if (cc.NewStatus != SwitchStatus.NoAction && cc.Channel < 4)
                    {
                        Logger.LogInformation($"Switch {cc.Channel} changes to {cc.NewStatus}");
                        _switches[cc.Channel] = cc.NewStatus;
                        SwitchStatusChanged?.Invoke(cc.Channel, cc.NewStatus);
                    }
                }
            }
        }

        public override void StartDecode()
        {
            _updateThread = new Thread(Updater);
            _updateThread.Start();
        }

        public override void StopDecode()
        {
            _cancellationTokenSource.Cancel();
            _updateThread?.Join();
            _updateThread = null;
        }

        private void Updater()
        {
            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                var switchStatus = new BinarySwitchStatus(DipSwitch, _switches);
                DispatchSentenceEvents(switchStatus);
                _cancellationTokenSource.Token.WaitHandle.WaitOne(TimeSpan.FromMilliseconds(200));

                var switchStatus2 = new CzoneChannelState(DipSwitch, _switches);
                DispatchSentenceEvents(switchStatus2);
                _cancellationTokenSource.Token.WaitHandle.WaitOne(TimeSpan.FromMilliseconds(200));

                var switchStatus3 = new CzoneCircuitStatus(DipSwitch, _switches);
                DispatchSentenceEvents(switchStatus3);
                _cancellationTokenSource.Token.WaitHandle.WaitOne(TimeSpan.FromMilliseconds(800));
            }
        }
    }
}
