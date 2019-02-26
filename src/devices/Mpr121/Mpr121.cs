// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Buffers.Binary;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Device.I2c;
using System.Threading;

namespace Iot.Device.Mpr121
{
    /// <summary>
    /// Supports MPR121 Proximity Capacitive Touch Sensor Controller.
    /// </summary>
    public class Mpr121 : IDisposable, INotifyPropertyChanged
    {
        private static readonly int CHANNELS_NUMBER = Enum.GetValues(typeof(Channels)).Length;

        private I2cDevice _device;
        private Timer _timer = null;

        private Dictionary<Channels, bool> _statuses;

        private int _periodRefresh;

        /// <summary>
        /// Notifies about a property has been changed.
        /// Refresh period can be changed by setting PeriodRefresh property.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Gets the channels statuses. The channel status is "true" if it's pressed. "False" otherwise.
        /// </summary>
        public IReadOnlyDictionary<Channels, bool> ChannelStatuses
        {
            get
            {
                return _statuses.ToImmutableDictionary();
            }

            private set
            {
                bool isStatusChanged = false;
                foreach (Channels channel in value.Keys)
                {
                    if (_statuses[channel] != value[channel])
                    {
                        _statuses[channel] = value[channel];
                        isStatusChanged = true;
                    }
                }

                if (isStatusChanged)
                {
                    OnPropertyChanged(nameof(ChannelStatuses));
                }
            }
        }

        /// <summary>
        /// Gets ot sets the period in milliseconds to refresh the channels statuses.
        /// </summary>
        /// <remark>
        /// Set value 0 to stop the automatically refreshing. Setting the value greater than 0 will start/update auto-refresh.
        /// </remark>
        public int PeriodRefresh
        {
            get { return _periodRefresh; }

            set
            {
                _periodRefresh = value;

                if (_periodRefresh > 0 && _timer == null)
                {
                    _timer = new Timer(RefreshChannelStatuses, this, TimeSpan.FromMilliseconds(_periodRefresh), TimeSpan.FromMilliseconds(_periodRefresh));
                }
                else if (_periodRefresh == 0 && _timer != null)
                {
                    _timer.Dispose();
                    _timer = null;
                }
                else if (_periodRefresh > 0 && _timer != null)
                {
                    _timer.Change(TimeSpan.FromMilliseconds(_periodRefresh), TimeSpan.FromMilliseconds(_periodRefresh));
                }
            }
        }

        /// <summary>
        /// Initialize a MPR121 controller with default configuration. Auto-refreshing of channel statuses is disabled.
        /// </summary>
        /// <param name="device">The i2c device.</param>
        public Mpr121(I2cDevice device) : this(device, 0, GetDefaultConfiguration())
        {
        }

        /// <summary>
        /// Initialize a MPR121 controller with default configuration.
        /// </summary>
        /// <param name="device">The i2c device.</param>
        /// <param name="periodRefresh">The period in milliseconds of refresing the channel statuses.</param>
        public Mpr121(I2cDevice device, int periodRefresh) : this(device, periodRefresh, GetDefaultConfiguration())
        {
        }

        /// <summary>
        /// Initialize a MPR121 controller with custom configuration. Auto-refreshing of channel statuses is disabled.
        /// </summary>
        /// <param name="device">The i2c device.</param>
        /// <param name="configuration">The controller configuration.</param>
        public Mpr121(I2cDevice device, Mpr121Configuration configuration) : this(device, 0, configuration)
        {
        }

        /// <summary>
        /// Initialize a MPR121 controller.
        /// </summary>
        /// <param name="device">The i2c device.</param>
        /// <param name="periodRefresh">The period in milliseconds of refresing the channel statuses.</param>
        /// <param name="configuration">The controller configuration.</param>
        public Mpr121(I2cDevice device, int periodRefresh, Mpr121Configuration configuration)
        {
            _device = device;
            _periodRefresh = periodRefresh;

            _statuses = new Dictionary<Channels, bool>();
            foreach (Channels channel in Enum.GetValues(typeof(Channels)))
            {
                _statuses.Add(channel, false);
            }

            InitializeController(configuration);

            if (PeriodRefresh > 0)
            {
                _timer = new Timer(RefreshChannelStatuses, this, TimeSpan.FromMilliseconds(PeriodRefresh), TimeSpan.FromMilliseconds(PeriodRefresh));
            }
        }

        public void Dispose()
        {
            if (_device != null)
            {
                _device.Dispose();
                _device = null;
            }

            if (_timer != null)
            {
                _timer.Dispose();
                _timer = null;
            }
        }

        /// <summary>
        /// Refresh the channel statuses.
        /// </summary>
        public void RefreshChannelStatuses()
        {
            Span<byte> buffer = stackalloc byte[2];
            _device.Read(buffer);

            short rawStatus = BinaryPrimitives.ReadInt16LittleEndian(buffer);

            Dictionary<Channels, bool> statuses = new Dictionary<Channels, bool>();
            for (var i = 0; i < CHANNELS_NUMBER; i++)
            {
                statuses[(Channels)i] = ((1 << i) & rawStatus) > 0;
            }

            ChannelStatuses = statuses.ToImmutableDictionary();
        }

        private static Mpr121Configuration GetDefaultConfiguration()
        {
            return new Mpr121Configuration()
            {
                MaxHalfDeltaRising = 0x01,
                NoiseHalfDeltaRising = 0x01,
                NoiseCountLimitRising = 0x00,
                FilterDelayCountLimitRising = 0x00,
                MaxHalfDeltaFalling = 0x01,
                NoiseHalfDeltaFalling = 0x01,
                NoiseCountLimitFalling = 0xFF,
                FilterDelayCountLimitFalling = 0x01,
                ElectrodeTouchThreshold = 0x0F,
                ElectrodeReleaseThreshold = 0x0A,
                ChargeDischargeTimeConfiguration = 0x04,
                ElectrodeConfiguration = 0x0C
            };
        }

        private void InitializeController(Mpr121Configuration configuration)
        {
            SetRegister(Registers.MHDR, configuration.MaxHalfDeltaRising);
            SetRegister(Registers.NHDR, configuration.NoiseHalfDeltaRising);
            SetRegister(Registers.NCLR, configuration.NoiseCountLimitRising);
            SetRegister(Registers.FDLR, configuration.FilterDelayCountLimitRising);
            SetRegister(Registers.MHDF, configuration.MaxHalfDeltaFalling);
            SetRegister(Registers.NHDF, configuration.NoiseHalfDeltaFalling);
            SetRegister(Registers.NCLF, configuration.NoiseCountLimitFalling);
            SetRegister(Registers.FDLF, configuration.FilterDelayCountLimitFalling);
            SetRegister(Registers.E0TTH, configuration.ElectrodeTouchThreshold);
            SetRegister(Registers.E0RTH, configuration.ElectrodeReleaseThreshold);
            SetRegister(Registers.E1TTH, configuration.ElectrodeTouchThreshold);
            SetRegister(Registers.E1RTH, configuration.ElectrodeReleaseThreshold);
            SetRegister(Registers.E2TTH, configuration.ElectrodeTouchThreshold);
            SetRegister(Registers.E2RTH, configuration.ElectrodeReleaseThreshold);
            SetRegister(Registers.E3TTH, configuration.ElectrodeTouchThreshold);
            SetRegister(Registers.E3RTH, configuration.ElectrodeReleaseThreshold);
            SetRegister(Registers.E4TTH, configuration.ElectrodeTouchThreshold);
            SetRegister(Registers.E4RTH, configuration.ElectrodeReleaseThreshold);
            SetRegister(Registers.E5TTH, configuration.ElectrodeTouchThreshold);
            SetRegister(Registers.E5RTH, configuration.ElectrodeReleaseThreshold);
            SetRegister(Registers.E6TTH, configuration.ElectrodeTouchThreshold);
            SetRegister(Registers.E6RTH, configuration.ElectrodeReleaseThreshold);
            SetRegister(Registers.E7TTH, configuration.ElectrodeTouchThreshold);
            SetRegister(Registers.E7RTH, configuration.ElectrodeReleaseThreshold);
            SetRegister(Registers.E8TTH, configuration.ElectrodeTouchThreshold);
            SetRegister(Registers.E8RTH, configuration.ElectrodeReleaseThreshold);
            SetRegister(Registers.E9TTH, configuration.ElectrodeTouchThreshold);
            SetRegister(Registers.E9RTH, configuration.ElectrodeReleaseThreshold);
            SetRegister(Registers.E10TTH, configuration.ElectrodeTouchThreshold);
            SetRegister(Registers.E10RTH, configuration.ElectrodeReleaseThreshold);
            SetRegister(Registers.E11TTH, configuration.ElectrodeTouchThreshold);
            SetRegister(Registers.E11RTH, configuration.ElectrodeReleaseThreshold);
            SetRegister(Registers.CDTC, configuration.ChargeDischargeTimeConfiguration);
            SetRegister(Registers.ELECONF, configuration.ElectrodeConfiguration);
        }

        private void SetRegister(Registers register, byte value)
        {
            Span<byte> data = stackalloc byte[] { (byte)register, value };
            _device.Write(data);
        }

        /// <summary>
        /// The callback function for timer to refresh channels statuses.
        /// </summary>
        private void RefreshChannelStatuses(object state)
        {
            RefreshChannelStatuses();
        }

        private void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
