// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Data;
using System.Device.Gpio;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using BuildHat.Models;
using Iot.Device.BuildHat.Models;
using Iot.Device.BuildHat.Motors;
using Iot.Device.BuildHat.Sensors;
using UnitsNet;

namespace Iot.Device.BuildHat
{
    /// <summary>
    /// The main Brick class allowing low level access to motors and sensors
    /// </summary>
    public class Brick : IDisposable
    {
        private const string FirmwareVersion = "Firmware version: ";
        private const string FirmwareSignature = "Firmware signature: ";

        private SerialPort _port;
        private GpioController? _gpio;
        private int _reset;
        private bool _shouldDisposeSerial = false;
        private bool _shouldDispose = false;
        private bool _isDisposed;
        private LedMode _ledMode;
        private Thread? _runing;
        private CancellationTokenSource? _cancellationTokenSource;
        private ElectricPotential _vin;

        // To store the Sensor types
        private SensorType[] _sensorType = { SensorType.None, SensorType.None, SensorType.None, SensorType.None };
        // 4 ports, can be any of motors, sensors, active elements.
        private object[] _elements = new object[4];
        // Potentially 4 sensors

        /// <summary>
        /// Checks if the sensor is an active one.
        /// </summary>
        /// <param name="sensorType">The sensor type.</param>
        /// <returns>True if active</returns>
        public static bool IsActiveSensor(SensorType sensorType) => sensorType >= SensorType.ColourAndDistanceSensor;

        /// <summary>
        /// Checks if the sensor is a motor.
        /// </summary>
        /// <param name="sensorType">The sensor type.</param>
        /// <returns>True if it's a motor.</returns>
        public static bool IsMotor(SensorType sensorType) => sensorType switch
        {
            SensorType.MediumLinearMotor => true,
            SensorType.SpikeEssentialSmallAngularMotor => true,
            SensorType.SpikePrimeLargeMotor => true,
            SensorType.SpikePrimeMediumMotor => true,
            SensorType.SystemMediumMotor => true,
            SensorType.SystemTrainMotor => true,
            SensorType.SystemTurntableMotor => true,
            SensorType.TechnicLargeMotor => true,
            SensorType.TechnicLargeMotorId => true,
            SensorType.TechnicXLMotor => true,
            SensorType.TechnicXLMotorId => true,
            SensorType.TechnicMediumAngularMotor => true,
            SensorType.TechnicMotor => true,
            _ => false,
        };

        /// <summary>
        /// Creates a Brick with a serial port
        /// </summary>
        /// <param name="port">The serial port</param>
        /// <param name="controller">A GPIO Controller.</param>
        /// <param name="reset">The rest pin.</param>
        /// <param name="shouldDispose">True to dispose the GPIO Controller.</param>
        public Brick(SerialPort port, GpioController? controller = null, int reset = -1, bool shouldDispose = true)
        {
            SetupGpioAndReset(controller, reset, shouldDispose);

            _port = port ?? throw new ArgumentNullException(nameof(port));
            Initialize();
            BuildHatInformation = GetBuildHatInformation();
            StartRunning();
        }

        /// <summary>
        /// Creates a Brick with a serial port.
        /// </summary>
        /// <param name="port">The serial port name.</param>
        /// <param name="controller">A GPIO Controller.</param>
        /// <param name="reset">The rest pin.</param>
        /// <param name="shouldDispose">True to dispose the GPIO Controller.</param>
        public Brick(string port, GpioController? controller = null, int reset = -1, bool shouldDispose = true)
        {
            SetupGpioAndReset(controller, reset, shouldDispose);

            _port = new(port, 115200, Parity.None, 8, StopBits.One);
            _port.ReadTimeout = 5000;
            _port.WriteTimeout = 5000;
            _port.NewLine = "\r\n";
            _port.Open();
            _shouldDisposeSerial = true;
            Initialize();
            BuildHatInformation = GetBuildHatInformation();
            StartRunning();
        }

        /// <summary>
        /// Gets the Build Hat information.
        /// </summary>
        public BuildHatInformation BuildHatInformation { get; internal set; }

        /// <summary>
        /// Gets or sets the led mode.
        /// </summary>
        public LedMode LedMode
        {
            get => _ledMode;
            set
            {
                _ledMode = value;
                SetLedMode(_ledMode);
            }
        }

        /// <summary>
        /// Gets the input voltage.
        /// </summary>
        public ElectricPotential InputVoltage { get => _vin; }

        /// <summary>
        /// Gets the sensor type connected at a specific port.
        /// </summary>
        /// <param name="port">The port to check.</param>
        /// <returns>The sensor type connected.</returns>
        public SensorType GetSensorType(SensorPort port) => _sensorType[(byte)port];

        #region Motors

        /// <summary>
        /// Gets the attached motor to a port.
        /// </summary>
        /// <param name="port">The port.</param>
        /// <returns>A motor.</returns>
        public IMotor GetMotor(SensorPort port)
        {
            if (!IsMotor(_sensorType[(byte)port]))
            {
                throw new Exception("No motor connected to this port");
            }

            return (IMotor)_elements[(byte)port];
        }

        /// <summary>
        /// Set the motor power in percent
        /// </summary>
        /// <param name="port">The Motor port.</param>
        /// <param name="powerPercent">The power from - 100 to 100</param>
        public void SetMotorPower(SensorPort port, int powerPercent)
        {
            if (!IsMotor(_sensorType[(byte)port]))
            {
                throw new ArgumentException("Not a motor connected");
            }

            powerPercent = powerPercent < -100 ? -100 : powerPercent > 100 ? 100 : powerPercent;
            Debug.WriteLine($"powerPercent: {powerPercent}");
            if (IsActiveSensor(_sensorType[(byte)port]))
            {
                _port.Write($"port {(byte)port} ; combi 0 1 0 2 0 3 0 ; select 0 ; pid {(byte)port} 0 0 s1 1 0 0.003 0.01 0 100; set {powerPercent}\r");
            }
            else
            {
                _port.Write($"port {(byte)port} ; pwm ; set {powerPercent}\r");
            }
        }

        /// <summary>
        /// Set the motor speed limit
        /// </summary>
        /// <param name="port">The Motor port.</param>
        /// <param name="powerLimit">The power limit between 0 and 1.</param>
        public void SetMotorLimits(SensorPort port, double powerLimit = 0.1)
        {
            if (!IsMotor(_sensorType[(byte)port]))
            {
                throw new ArgumentException("Not a motor connected");
            }

            powerLimit = powerLimit < 0 ? 0 : powerLimit > 1 ? 1 : powerLimit;
            Debug.WriteLine($"Bias: {powerLimit.ToString(CultureInfo.InvariantCulture)}");
            _port.Write($"port {(byte)port} ; plimit {powerLimit.ToString(CultureInfo.InvariantCulture)}\r");
        }

        /// <summary>
        /// Set the motor bias
        /// </summary>
        /// <param name="port">The Motor port.</param>
        /// <param name="bias">The bias between 0 and 1.</param>
        public void SetMotorBias(SensorPort port, double bias = 0.1)
        {
            if (!IsMotor(_sensorType[(byte)port]))
            {
                throw new ArgumentException("Not a motor connected");
            }

            bias = bias < 0 ? 0 : bias > 1 ? 1 : bias;
            Debug.WriteLine($"Bias: {bias.ToString(CultureInfo.InvariantCulture)}");
            _port.Write($"port {(byte)port} ; bias {bias.ToString(CultureInfo.InvariantCulture)}\r");
        }

        /// <summary>
        /// Run the specified motors for an amount of seconds.
        /// </summary>
        /// <param name="port">The Motor port</param>
        /// <param name="seconds">The amount of seconds.</param>
        /// <param name="speed">>The speed from - 100 to 100.</param>
        /// <param name="blocking">True to block the function and wait for the execution.</param>
        public void RunMotorForSeconds(SensorPort port, double seconds, int speed, bool blocking = false)
        {
            if (seconds < 0)
            {
                return;
            }

            speed = speed < -100 ? -100 : speed > 100 ? 100 : speed;
            _port.Write($"port {(byte)port} ; combi 0 1 0 2 0 3 0 ; select 0 ; pid {(byte)port} 0 0 s1 1 0 0.003 0.01 0 100; set pulse {speed} 0.0 {seconds.ToString(CultureInfo.InvariantCulture)} 0\r");
            if (blocking)
            {
                Thread.Sleep((int)(seconds * 1000));
            }
        }

        /// <summary>
        /// Run the motor to an absolute position.
        /// </summary>
        /// <param name="port">The port.</param>
        /// <param name="targetPosition">The target angle from -180 to +179.</param>
        /// <param name="way">The way to go to the position.</param>
        /// <param name="speed">The speed from - 100 to 100.</param>
        /// <param name="blocking">True to block the function and wait for the execution.</param>
        /// <exception cref="ArgumentException">Not a motor or not an active motor.</exception>
        public void RunMotorToAbsolutePosition(SensorPort port, int targetPosition, PositionWay way, int speed, bool blocking = false)
        {
            if (!IsMotor(_sensorType[(byte)port]))
            {
                throw new ArgumentException("Not a motor connected");
            }

            if (!IsActiveSensor(_sensorType[(byte)port]))
            {
                throw new ArgumentException("Not an active motor connected");
            }

            if ((targetPosition < -180) || (targetPosition > 179))
            {
                throw new ArgumentException("Target position can only be between -180 and +179");
            }

            if (speed == 0)
            {
                throw new ArgumentException("Speed can't be 0");
            }

            speed = speed < -100 ? -100 : speed > 100 ? 100 : speed;
            ActiveMotor motor = (ActiveMotor)_elements[(byte)port];
            // We need both the relative and absolute position
            int actualPosition = motor.Position;
            int actualAbsolutePosition = motor.AbsolutePosition;
            double actualPositionDouble = actualPosition / 360.0;
            int difference = (targetPosition - actualAbsolutePosition + 180) % 360 - 180;
            double newPosition;

            switch (way)
            {
                case PositionWay.Clockwise:
                    if (difference > 0)
                    {
                        newPosition = (actualPosition + difference) / 360.0;
                    }
                    else
                    {
                        newPosition = (actualPosition + 360 + difference) / 360.0;
                    }

                    break;
                case PositionWay.AntoClockwise:
                    if (difference < 0)
                    {
                        newPosition = (actualPosition + difference) / 360.0;
                    }
                    else
                    {
                        newPosition = (actualPosition + 360 + difference) / 360.0;
                    }

                    break;
                case PositionWay.Shortest:
                default:
                    if (Math.Abs(difference) > 180)
                    {
                        newPosition = (actualPosition + 360 + difference) / 360.0;
                    }
                    else
                    {
                        newPosition = (actualPosition + difference) / 360.0;
                    }

                    break;
            }

            double duration = Math.Abs(newPosition - actualPositionDouble) / (speed * 0.05 / motor.PowerLimit);
            // Ramp uses first param as initial position, second as target, third is how long, foruth is always 0
            _port.Write($"port {(byte)port} ; combi 0 1 0 2 0 3 0 ; select 0 ; pid {(byte)port} 0 1 s4 0.0027777778 0 5 0 .1 3 ; set ramp {actualPositionDouble.ToString(CultureInfo.InvariantCulture)} {newPosition.ToString(CultureInfo.InvariantCulture)} {duration.ToString(CultureInfo.InvariantCulture)} 0\r");
            if (blocking)
            {
                // This is precise enougth
                Thread.Sleep((int)(duration * 1000));
            }
        }

        /// <summary>
        /// Run the motor to an absolute position.
        /// </summary>
        /// <param name="port">The port.</param>
        /// <param name="targetPosition">The target angle from -180 to +180.</param>
        /// <param name="speed">The speed from - 100 to 100.</param>
        /// <param name="blocking">True to block the function and wait for the execution.</param>
        /// <exception cref="ArgumentException">Not a motor or not an active motor.</exception>
        public void RunMotorToPosition(SensorPort port, int targetPosition, int speed, bool blocking = false)
        {
            if (!IsMotor(_sensorType[(byte)port]))
            {
                throw new ArgumentException("Not a motor connected");
            }

            if (!IsActiveSensor(_sensorType[(byte)port]))
            {
                throw new ArgumentException("Not an active motor connected");
            }

            if (speed == 0)
            {
                throw new ArgumentException("Speed can't be 0");
            }

            speed = speed < -100 ? -100 : speed > 100 ? 100 : speed;
            ActiveMotor motor = (ActiveMotor)_elements[(byte)port];
            // We need both the relative
            int actualPosition = motor.Position;
            double actualPositionDouble = actualPosition / 360.0;
            double newPosition = (targetPosition - actualPositionDouble) / 360.0;
            double duration = Math.Abs(newPosition - actualPositionDouble) / (speed * 0.05 / motor.PowerLimit);
            // Ramp uses first param as initial position, second as target, third is how long, foruth is always 0
            _port.Write($"port {(byte)port} ; combi 0 1 0 2 0 3 0 ; select 0 ; pid {(byte)port} 0 1 s4 0.0027777778 0 5 0 .1 3 ; set ramp {actualPositionDouble.ToString(CultureInfo.InvariantCulture)} {newPosition.ToString(CultureInfo.InvariantCulture)} {duration.ToString(CultureInfo.InvariantCulture)} 0\r");
            if (blocking)
            {
                while (!((motor.Position / 360.0 < newPosition + 2.028) && (motor.Position / 360.0 > newPosition - 2.028)))
                {
                    Thread.Sleep(5);
                }
            }
        }

        #endregion

        #region sensors

        /// <summary>
        /// Select modes on a specific port. This is only possible on active sensors and motors.
        /// </summary>
        /// <param name="port">The port.</param>
        /// <param name="modes">The modes.</param>
        public void SelectModes(SensorPort port, int[] modes)
        {
            if ((modes == null) || (modes.Length == 0))
            {
                throw new ArgumentException("Modes can't be null or empty");
            }

            if (!IsActiveSensor(_sensorType[(byte)port]))
            {
                throw new Exception("Mode can be changed only on active sensors.");
            }

            var sensor = (ActiveSensor)_elements[(byte)port];
            // check all modes are present
            foreach (var mode in modes)
            {
                if (!sensor.ModeDetails.Select(m => m.Number == mode).Any())
                {
                    throw new Exception("Mode has to be suported in this sensor");
                }
            }

            StringBuilder command = new($"port {(byte)port} ; combi 0 ");
            for (int i = 0; i < modes.Length; i++)
            {
                command.Append($"{modes[i]} 0 ");
            }

            command.Append("; select 0");
            _port.Write(command.ToString());
        }

        /// <summary>
        /// Gets the sensor connected.
        /// </summary>
        /// <param name="port">The port.</param>
        /// <returns>A sensor.</returns>
        /// <exception cref="Exception">A motor or no sensor is connected</exception>
        public Sensor GetSensor(SensorPort port)
        {
            if (IsMotor(_sensorType[(byte)port]))
            {
                throw new Exception("A motor or no sensor is connected");
            }

            return (Sensor)_elements[(byte)port];
        }

        /// <summary>
        /// Gets the sensor connected.
        /// </summary>
        /// <param name="port">The port.</param>
        /// <returns>A sensor.</returns>
        /// <exception cref="Exception">A motor or no sensor is connected</exception>
        public ActiveSensor GetActiveSensor(SensorPort port)
        {
            if (!IsActiveSensor(_sensorType[(byte)port]))
            {
                throw new Exception("A motor or no sensor is connected");
            }

            return (ActiveSensor)_elements[(byte)port];
        }

        #endregion

        /// <inheritdoc/>
        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            // Stop the running thread
            _cancellationTokenSource?.Cancel();
            _runing?.Join();

            // Make sure there is no motor running
            for (int i = 0; i < _sensorType.Length; i++)
            {
                // stop all possible motors
                if (_sensorType[i] != SensorType.SpikeEssential3x3ColourLightMatrix)
                {
                    _port.Write($"port {i} ; pwm ; coast ; off \r");
                }
                else
                {
                    // Special case for the matrix, swith everything off
                    _port.Write($"port {i} ; write1 C2 00 00 00 00 00 00 00 00 00\r");
                }
            }

            // Deselect all ports
            _port.Write("port 0 ; select ; port 1 ; select ; port 2 ; select ; port 3 ; select ; echo 0\r");
            // Depending on the serial port, disposing too fast won't all the message to be send
            // Wiating to see of the bytes has been sent does not properly work for some serial port
            // So waiting seems to be the best option here.
            Thread.Sleep(100);

            // Need to set everything to float mode
            if (_shouldDisposeSerial)
            {
                _port?.Dispose();
            }

            if (_shouldDispose)
            {
                _gpio?.Dispose();
            }

            _isDisposed = true;
        }

        private void SetupGpioAndReset(GpioController? controller, int reset, bool shouldDispose)
        {
            _reset = reset;
            if (_reset >= 0)
            {
                _gpio = controller ?? new GpioController();
                _shouldDispose = shouldDispose || controller == null;
                Reset();
            }
        }

        private void CheckForFirmwareAndUpload()
        {
            const string FirmwareFile = "firmware.bin";
            const string FirmwareSignature = "signature.bin";
            const string DataPath = "data";
            // const string Prompt = "BHBL>";
            const string BootloaderSignature = "BuildHAT bootloader version";
            // Let's clear the port first
            _port.ReadExisting();
            _port.Write("version\r");
            Thread.Sleep(10);
            var prompt = _port.ReadExisting();

            // In this case, we'll need to uploadthe firmware
            if (prompt.Contains(BootloaderSignature))
            {
                _port.Write("\r");
                prompt = _port.ReadLine();
                prompt = _port.ReadExisting();

                // Chack if the 2 files are present
                var firmwareFile = Path.Combine(DataPath, FirmwareFile);
                var firmwareSignature = Path.Combine(DataPath, FirmwareSignature);
                if (!File.Exists(firmwareFile))
                {
                    throw new Exception("Firmware file does not exist");
                }

                if (!File.Exists(firmwareSignature))
                {
                    throw new Exception("Signature file does not exist");
                }

                // Load both files
                byte[] firmware = File.ReadAllBytes(firmwareFile);
                byte[] signature = File.ReadAllBytes(firmwareSignature);
                // Step 1: clear and get the prompt
                _port.Write("clear\r");
                prompt = _port.ReadLine();
                prompt = _port.ReadExisting();

                // Step 2: load the firmware
                _port.Write($"load {firmware.Length} {GetFirmwareCheckSum(firmware)}\r");
                prompt = _port.ReadExisting();
                // STX = 0x02, ETX = 0x03
                _port.Write("\x02");
                // Write the byte data of the firmware between the STX and ETX
                _port.Write(firmware, 0, firmware.Length);
                _port.Write("\x03\r");
                Thread.Sleep(10);

                // Step 3: load the signature
                prompt = _port.ReadExisting();
                Console.WriteLine(prompt);
                // STX = 0x02, ETX = 0x03
                _port.Write($"signature {signature.Length}\r");
                _port.Write("\x02");
                // Write the byte data of the signature between the STX and ETX
                _port.Write(signature, 0, signature.Length);
                _port.Write("\x03\r");
                Thread.Sleep(10);
                Console.WriteLine(_port.ReadExisting());

                // Step 4: reboot
                _port.Write("reboot\r");
                prompt = _port.ReadLine();
                prompt = _port.ReadExisting();
                // this time seems suffisant to have it booted
                Thread.Sleep(1500);
            }
        }

        private uint GetFirmwareCheckSum(byte[] firmware)
        {
            uint check = 1;
            for (int i = 0; i < firmware.Length; i++)
            {
                if ((check & 0x80000000) != 0)
                {
                    check = (check << 1) ^ 0x1d872b41;
                }
                else
                {
                    check = check << 1;
                }

                check = (check ^ firmware[i]) & 0xFFFFFFFF;
            }

            return check;
        }

        private void Initialize()
        {
            // Let's clear the port first
            _port.ReadExisting();
            // Check if there is a firmware, if not, upload one
            CheckForFirmwareAndUpload();
            _port.ReadExisting();
            // Clear the output and find the version
            string line;
            int inc = 0;
        retryVersion:
            _port.Write("version\r");
            while (_port.BytesToRead <= 0)
            {
                Thread.Sleep(10);
            }

            line = _port.ReadExisting();
            if (!line.Contains(FirmwareVersion))
            {
                inc++;
                if (inc < 10)
                {
                    // We are waiting a bit, the configuration may not be fully over
                    Thread.Sleep(100);
                    goto retryVersion;
                }

                throw new IOException("Can't read the version");
            }

            _port.ReadExisting();
            // No echo and read the voltage
            SetEcho(false);
            inc = 0;
        retryVoltage:
            SetLedMode(LedMode.VoltageDependant);
            _port.ReadLine();
            var rawV = GetRawVoltage().Split(' ');
            try
            {
                _vin = ElectricPotential.FromVolts(double.Parse(rawV[0], CultureInfo.InvariantCulture));
            }
            catch (Exception ex)
            {
                inc++;
                _port.ReadExisting();
                if (inc < 10)
                {
                    goto retryVoltage;
                }

                throw new IOException("Can't read the voltage", ex);
            }

        }

        private void StartRunning()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _runing = new Thread(() => Running(_cancellationTokenSource.Token));
            _runing.Start();
        }

        private void Running(CancellationToken token)
        {
            string line;
            // Force an update
            _port.Write("list\r");
            while (!token.IsCancellationRequested)
            {
                while (_port.BytesToRead > 0)
                {
                    line = _port.ReadLine();
                    ProcessOutput(line);
                }
            }
        }

        private void ProcessOutput(string line)
        {
            const string ConnectedActiveId = ": connected to active ID ";
            const string ConnectedPassiveId = ": connected to passive ID ";
            const string Disonnected = ": disconnected";
            const string TimeoutDisconnecting = ": timeout during data phase: disconnecting";
            const string NotConnected = ": no device detected";
            // The flow is the following when something is disconnected or connected
            // P0: timeout during data phase: disconnecting
            // P0: disconnected
            // P0: connecting to active device
            // set baud rate to 115200
            // Then there is a full dump of the element and its properties
            if (string.IsNullOrEmpty(line))
            {
                return;
            }

            if ((line.Contains(ConnectedActiveId)) || (line.Contains(ConnectedPassiveId)))
            {
                // P0: connected to active ID 4B
                var port = Convert.ToInt32(line[1] - '0');
                if ((port >= 0) && (port < 4))
                {
                    var sensorType = Convert.ToInt32(line.Substring(line.LastIndexOf(" ") + 1), 16);
                    _sensorType[port] = (SensorType)sensorType;
                    if (IsMotor(_sensorType[port]))
                    {
                        if (IsActiveSensor(_sensorType[port]))
                        {
                            _elements[port] = new ActiveMotor(this, (SensorPort)port, _sensorType[port]);
                            ((ActiveMotor)_elements[port]).IsConnected = true;
                            PopulateModelDetails(port);
                            // Forces 1 read of the data
                            _port.Write($"port {port} ; combi 0 1 0 2 0 3 0 ; selonce 0\r");
                        }
                        else
                        {
                            _elements[port] = new PassiveMotor(this, (SensorPort)port, _sensorType[port]);
                            ((PassiveMotor)_elements[port]).IsConnected = true;
                        }
                    }
                    else
                    {
                        if (IsActiveSensor(_sensorType[port]))
                        {
                            // TODO: adjust the creation based on the type of sensor
                            // TODO: forces one read of the mode
                            _elements[port] = new ActiveSensor(this, (SensorPort)port, _sensorType[port]);
                            ((ActiveSensor)_elements[port]).IsConnected = true;
                            PopulateModelDetails(port);
                        }
                        else
                        {
                            _elements[port] = new Sensor(this, (SensorPort)port, _sensorType[port]);
                            ((Sensor)_elements[port]).IsConnected = true;
                        }
                    }
                }
            }
            else if (line.Contains(Disonnected) || line.Contains(TimeoutDisconnecting) || line.Contains(NotConnected))
            {
                // P0: disconnected
                var port = Convert.ToInt32(line[1] - '0');
                if ((port >= 0) && (port < 4))
                {
                    if (IsMotor(_sensorType[port]))
                    {
                        if (IsActiveSensor(_sensorType[port]))
                        {
                            ((ActiveMotor)_elements[port]).IsConnected = false;
                        }
                        else
                        {
                            ((PassiveMotor)_elements[port]).IsConnected = false;
                        }
                    }

                    _sensorType[port] = SensorType.None;
                }
            }
            else if ((line[0] == 'P') && (((line[2] == 'C')) || ((line[2] == 'M'))))
            {
                // Will look like this for motores
                // P0C0: +18 +5489 +12
                var port = Convert.ToInt32(line[1] - '0');
                if (IsMotor(_sensorType[port]))
                {
                    if (IsActiveSensor(_sensorType[port]))
                    {
                        try
                        {
                            // As we always select the same modes 1, 2 and 3, we do then read
                            // speed position in degeres and absolute position in degrees
                            var elements = line.Split(' ');
                            var active = ((ActiveMotor)_elements[port]);
                            if ((active != null) && (elements.Length == 4))
                            {
                                active.Speed = Convert.ToInt32(elements[1]);
                                active.Position = Convert.ToInt32(elements[2]);
                                active.AbsolutePosition = Convert.ToInt32(elements[3]);
                            }

                        }
                        catch (Exception)
                        {
                            // Just skip, it can be malformed
                        }
                    }
                }
            }
        }

        private void PopulateModelDetails(int port)
        {
            const string TypeActive = "type ";
            const string Nmodes = "  nmodes =";
            const string Baud = "  baud   =";
            const string Hwver = "  hwver  =";
            const string Swver = "  swver  =";
            const string FormatCount = "    format ";
            const string Signal = " SI = ";
            // type 4B
            //   nmodes =5
            //   nview  =3
            //   baud   =115200
            //   hwver  =00000004
            //   swver  =10000000
            //   M0 POWER SI = PCT
            //     format count=1 type=0 chars=4 dp=0
            //     RAW: 00000000 00000064    PCT: 00000000 00000064    SI: 00000000 00000064
            //   M1 SPEED SI = PCT
            //     format count=1 type=0 chars=4 dp=0
            //     RAW: 00000000 00000064    PCT: 00000000 00000064    SI: 00000000 00000064
            //   M2 POS SI = DEG
            //     format count=1 type=2 chars=11 dp=0
            //     RAW: 00000000 00000168    PCT: 00000000 00000064    SI: 00000000 00000168
            //   M3 APOS SI = DEG
            //     format count=1 type=1 chars=3 dp=0
            //     RAW: 00000000 000000B3    PCT: 00000000 000000C8    SI: 00000000 000000B3
            //   M4 CALIB SI = CAL
            //     format count=2 type=1 chars=5 dp=0
            //     RAW: 00000000 00000E10    PCT: 00000000 00000064    SI: 00000000 00000E10
            //   M5 STATS SI = MIN
            //     format count=14 type=1 chars=5 dp=0
            //     RAW: 00000000 0000FFFF    PCT: 00000000 00000064    SI: 00000000 0000FFFF
            //   C0: M1+M2+M3
            //      speed PID: 00000BB8 00000064 00002328 00000438
            //   position PID: 00002EE0 000003E8 00013880 00000000
            // P0: established serial communication with active ID 4B
            // We will have a predictable way to read all this.
            // expect: type 4B
            string line = _port.ReadLine();
            if (!line.Contains(TypeActive))
            {
                _port.ReadExisting();
                return;
            }

            ModeDetail[]? details = null;
            var sensor = ((ActiveSensor)_elements[port]);

            try
            {
                // expect: nmodes =5
                line = _port.ReadLine();
                int num = int.Parse(line.Substring(Nmodes.Length));
                details = new ModeDetail[num + 1];
                // expect: nview  =3
                line = _port.ReadLine();
                // expect: baud   =115200
                line = _port.ReadLine();
                num = int.Parse(line.Substring(Baud.Length));
                sensor.BaudRate = num;
                // expect: hwver  =00000004
                line = _port.ReadLine();
                uint unum = Convert.ToUInt32(line.Substring(Hwver.Length), 16);
                sensor.HardwareVersion = unum;
                // expect: swver  =10000000
                line = _port.ReadLine();
                unum = Convert.ToUInt32(line.Substring(Swver.Length), 16);
                sensor.SoftwareVersion = unum;
                string[] data;
                for (int i = 0; i < details.Length; i++)
                {
                    ModeDetail mode = new();
                    mode.Number = i;
                    // expect:   M0 POWER SI = PCT
                    // or:  M5 COL O SI = IDX
                    // Mi mode_name SI = unit
                    line = _port.ReadLine();
                    // 5 is the number of characters before the name
                    mode.Name = line.Substring(5, line.IndexOf(Signal) - 5);
                    mode.Unit = line.Substring(line.IndexOf(Signal) + Signal.Length);
                    // expect: format count=1 type=0 chars=4 dp=0
                    line = _port.ReadLine().Substring(FormatCount.Length);
                    data = line.TrimStart(' ').Split(' ');
                    // Now data is a collectin of something=number
                    string[] modeData;
                    for (int modeType = 0; modeType < data.Length; modeType++)
                    {
                        modeData = data[modeType].Split('=');
                        switch (modeData[0])
                        {
                            case "count":
                                mode.NumberOfData = Convert.ToInt32(modeData[1]);
                                break;
                            case "type":
                                switch (modeData[1])
                                {
                                    case "0":
                                        mode.DataType = typeof(byte);
                                        break;
                                    case "1":
                                        mode.DataType = typeof(short);
                                        break;
                                    case "2":
                                        mode.DataType = typeof(int);
                                        break;
                                    case "3":
                                        mode.DataType = typeof(float);
                                        break;
                                    default:
                                        break;
                                }

                                break;
                            case "chars":
                                mode.NumberOfCharsToDisplay = Convert.ToInt32(modeData[1]);
                                break;
                            case "dp":
                                mode.DecimalPrecision = Convert.ToInt32(modeData[1]);
                                break;
                            default:
                                break;
                        }
                    }

                    // expect: RAW: 00000000 00000064    PCT: 00000000 00000064    SI: 00000000 00000064
                    line = _port.ReadLine();
                    MinimumMaximumValues[] values = new MinimumMaximumValues[3];
                    values[0] = new MinimumMaximumValues();
                    values[0].TypeValues = TypeValues.Raw;
                    values[0].MinimumValue = Convert.ToInt32(line.Substring(9, 8), 16);
                    values[0].MaximumValue = Convert.ToInt32(line.Substring(18, 8), 16);
                    values[1] = new MinimumMaximumValues();
                    values[1].TypeValues = TypeValues.Percent;
                    values[1].MinimumValue = Convert.ToInt32(line.Substring(35, 8), 16);
                    values[1].MaximumValue = Convert.ToInt32(line.Substring(44, 8), 16);
                    values[2] = new MinimumMaximumValues();
                    values[2].TypeValues = TypeValues.Signal;
                    values[2].MinimumValue = Convert.ToInt32(line.Substring(60, 8), 16);
                    values[2].MaximumValue = Convert.ToInt32(line.Substring(69, 8), 16);
                    mode.MinimumMaximumValues = values;

                    details[i] = mode;
                }

                sensor.ModeDetailsInternal.AddRange(details);
                // Now we should or not expect: C0: M1+M2+M3
                bool hasCombi;
                int combiNum = 0;
                do
                {
                    line = _port.ReadLine();
                    hasCombi = line.Contains($"C{combiNum}: ");
                    if (hasCombi)
                    {
                        line = line.Substring(line.IndexOf(':') + 2);
                        data = line.Split('+');
                        num = Convert.ToInt32(data.Length);
                        CombiModes combi = new();
                        combi.Number = combiNum++;
                        combi.Modes = new int[num];
                        for (int i = 0; i < num; i++)
                        {
                            // M1, M12, etc
                            combi.Modes[i] = Convert.ToInt32(data[i].Substring(1));
                        }

                        sensor.CombiModesInternal.Add(combi);
                    }
                }
                while (hasCombi);
                // If no combi, then it's speed PID
                line = line.Substring(line.LastIndexOf(':') + 2);
                data = line.Split(' ');
                RecommendedPid recoSpeed = new();
                recoSpeed.Pid1 = Convert.ToInt32(data[0], 16);
                recoSpeed.Pid2 = Convert.ToInt32(data[1], 16);
                recoSpeed.Pid3 = Convert.ToInt32(data[2], 16);
                recoSpeed.Pid4 = Convert.ToInt32(data[3], 16);
                sensor.SpeedPid = recoSpeed;
                line = _port.ReadLine();
                line = line.Substring(line.LastIndexOf(':') + 2);
                data = line.Split(' ');
                RecommendedPid recoPos = new();
                recoPos.Pid1 = Convert.ToInt32(data[0], 16);
                recoPos.Pid2 = Convert.ToInt32(data[1], 16);
                recoPos.Pid3 = Convert.ToInt32(data[2], 16);
                recoPos.Pid4 = Convert.ToInt32(data[3], 16);
                sensor.PositionPid = recoPos;
            }
            catch (Exception)
            {
                // We are catching anything bad that can happen and just continue
                // Something may be malformed
                if ((details != null) && (sensor.ModeDetailsInternal.Count == 0))
                {
                    sensor.ModeDetailsInternal.AddRange(details);
                }
            }
        }

        private BuildHatInformation GetBuildHatInformation()
        {
            const byte MaximumRetry = 3;
            byte retry = 0;

        retryVersion:
            var versionRaw = GetRawVersion();
            if (!versionRaw.Contains(FirmwareVersion))
            {
                if (retry++ <= MaximumRetry)
                {
                    goto retryVersion;
                }

                throw new IOException("BuildHat firmware version error");
            }

            retry = 0;
        retrySignature:
            var signatureRaw = GetRawSignature();
            if (!signatureRaw.Contains(FirmwareSignature))
            {
                if (retry++ <= MaximumRetry)
                {
                    goto retrySignature;
                }

                throw new IOException("BuildHat firmware signture error");
            }

            // Format is: Firmware version: 1636109636 2021-11-05T10:53:56+00:00
            var versionToClean = versionRaw.Substring(versionRaw.IndexOf(FirmwareVersion) + FirmwareVersion.Length);
            string[] version;
            if (versionToClean.Contains('\r'))
            {
                version = versionToClean.Substring(0, versionToClean.IndexOf('\r')).Split(' ');
            }
            else
            {
                version = versionToClean.Split(' ');
            }

            // Format is: Firmware signature: 1E B0 13 3B 10 0D 83 3E 67 FA 05 90 15 B1 AA 5D 6D 56 A5 56 21 E0 4C D9 99 07 E9 EE 0C 5D 4E 46 F3 CB E9 B4 8B F4 58 8F 7C B2 C7 5B E4 AC 2D A7 3B 14 1A 26 58 DA 38 03 5C 53 6F 04 6E 7C CB 24
            var signature = signatureRaw.Substring(signatureRaw.IndexOf(FirmwareSignature) + FirmwareSignature.Length).Split(' ');

            // Processing the signature
            byte[] resultSignature = signature
              .Select(value => Convert.ToByte(value, 16))
              .ToArray();

            // Second result of the version is the date
            DateTimeOffset dt = Convert.ToDateTime(version[1]);

            return new BuildHatInformation(version[0], resultSignature, dt);
        }

        private void Reset()
        {
            if (_reset >= 0)
            {
                _gpio!.OpenPin(_reset, PinMode.Output);
                _gpio.Write(_reset, PinValue.Low);
                // Timing according to the documentation
                Thread.Sleep(10);
                _gpio.Write(_reset, PinValue.High);
                Thread.Sleep(10);
                _gpio.ClosePin(_reset);
            }
        }

        #region primitives

        // The protocole returns plain text ending lines with \r\n
        // It always sent back 1 or more lines containging the echo
        // Then the result of the function
        // To send a new line, only \r is required
        private string GetRawVersion()
        {
            _port.Write("version\r");
            _port.ReadLine();
            return _port.ReadExisting();
        }

        private string GetRawSignature()
        {
            _port.Write("signature\r");
            _port.ReadLine();
            return _port.ReadLine();
        }

        private string GetRawVoltage()
        {
            _port.Write("vin\r");
            _port.ReadLine();
            return _port.ReadLine();
        }

        private void SetLedMode(LedMode mode)
        {
            _port.Write($"ledmode {(int)mode}\r");
        }

        private void SetEcho(bool on)
        {
            _port.Write($"echo {(on ? "1" : "0")}\r");
            _port.ReadLine();
        }

        #endregion
    }
}
