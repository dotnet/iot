// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Text;

namespace System.Device.Gpio
{
    /// <summary>
    /// GPIO - Basic Scenarios
    /// 
    /// Most GPIO implementations share a core set of functionality to allow basic on/off control
    /// of pins. At the very least, we should support these.
    /// - Open a object that is a representation of a pin with the given pin number
    /// - Support closing a pin to release the resources owned by that pin object
    /// - Represent the mode of the pin that details how the pin handles reads and writes (e.g. Input, Output)
    /// - Allow a resistor to be added to a pin such that it can be set as pullup or pulldown (or no pull)
    /// - Support setting a PWM value on a pin
    /// </summary>
    #region GPIO - Basic Scenarios

    public partial class GPIOPin : IDisposable
    {
        int PinNumber { get { throw new NotImplementedException(); } }
        bool Read() { throw new NotImplementedException(); }
        void Write(bool value) { throw new NotImplementedException(); }
        void Dispose() { throw new NotImplementedException(); }
        GPIOPin(GPIOController controller, int pinNumber, GPIOPinMode pinMode = GPIOPinMode.Input) { }

        GPIOPinMode PinMode { get { throw new NotImplementedException(); } set { throw new NotImplementedException(); } }
        bool IsPinModeSupported(GPIOPinMode pinMode) { throw new NotImplementedException(); }
        int PWMValue { set { throw new NotImplementedException(); } }
    }

    public partial class GPIOController : IDisposable
    {
        GPIOPin this[int pinNumber] { get { throw new NotImplementedException(); } }
        GPIOPin OpenPin(int pinNumber) { throw new NotImplementedException(); }
        void ClosePin(int pinNumber) { throw new NotImplementedException(); }
        void ClosePin(GPIOPin pin) { throw new NotImplementedException(); }
        int PinCount() { throw new NotImplementedException(); }
        IEnumerable<GPIOPin> ConnectedPins { get { throw new NotImplementedException(); } }
        void Dispose() { throw new NotImplementedException(); }
    }

    public enum GPIOPinMode
    {
        Pull_Down,
        Pull_Up,
        PWM,
        Input,
        Output
    }

    #endregion

    /// <summary>
    /// GPIO - Advanced Scenarios
    /// - Analog Reads and Writes - Most GPIO works with digital pins, but sometimes analog pins are used. The difference in the 
    ///     Analog pins is that they have a range of potential values instead of just being on/off like the digital pins.
    /// - Listeners - Polling, interrupts, etc. There should be some way to listen for a change and respond accordingly
    /// - Edge Detection - Used with listeners/eventing as a way of definining the circumstances under which an event/callback will be raised
    /// - Allow setting a Debounce duration to ignore quickly occuring events during some timespan.
    /// </summary>
    #region GPIO - Advanced Scenarios

    public partial class GPIOPin
    {
        // Analog
        int AnalogRead() { throw new NotImplementedException(); }
        void AnalogWrite(int value) { throw new NotImplementedException(); }

        // Listeners
        // TODO

        // Edge Detection
        // TODO

        // Debounce
        public TimeSpan Debounce { get { throw new NotImplementedException(); }  set { throw new NotImplementedException(); } }
    }

    #endregion

    /// <summary>
    /// GPIO - Bonuses
    /// - Choose between BCM or BOARD pin numbering
    /// - Waiters - Instead of manually polling a Read, a Waiter will handle the polling until the desired Read value is reached
    /// - Bit shifting - Add helpers to allow easily working with more usable data types
    /// - Advanced PWM functions can be added to allow setting range, rpi mode, etc.
    /// </summary>
    #region GPIO - Other Scenarios

    // BCM vs BOARD
    public partial class GPIOController
    {
        GPIOController(GPIOScheme numbering = GPIOScheme.BOARD) { throw new NotImplementedException(); }
    }

    public enum GPIOScheme
    {
        BOARD,
        BCM
    }

    public partial class GPIOPin
    {
        // Waiters
        public bool ReadWait(TimeSpan timeout) { throw new NotImplementedException(); }
        public int AnalogReadWait(TimeSpan timeout) { throw new NotImplementedException(); }

        // Bit-Shifts and writer helpers
        // TODO

        // Advanced PWM
        public int PWMRange { get { throw new NotImplementedException(); } }
        public PWMMode PWMMode { get { throw new NotImplementedException(); } }
    }

    public enum PWMMode
    {
        MARK_SPACE,
        BALANCED
    }

    #endregion

    /// <summary>
    /// Stretch - Advanced Multi-Pin Connections
    /// 
    /// This section holds connection types where more than one pin is used to transmit data. There
    /// are a ton of these, but the most commonly supported are SPI and I2C, followed by UART/SerialPort
    /// </summary>
    #region Stretch - Advanced Multi-Pin Connections

    public enum GPIOPinMode
    {
        Pull_Down,
        Pull_Up,
        PWM,
        Input,
        Output,
        SPI,
        I2C,
        UART, // serialport
        Unknown
    }

    public partial class SpiConnection
    {
        // https://github.com/Petermarcu/Pi/tree/master/Pi.IO.SerialPeripheralInterface
    }

    public partial class I2cConnection
    {
        // https://github.com/Petermarcu/Pi/tree/master/Pi.IO.InterIntegratedCircuit
    }

    public class UARTConnection
    {
        // A Linux or platform-agnostic serial port library would likely have to be distinct from our 
        // existing bloated Windows implementation. That wouldn't necessarily be a bad thing, though, as
        // we could add basic functionality like read/write/open/close without the weight of the Windows
        // implementation weighing it down.
    }

    #endregion

    /// <summary>
    /// Out of Scope - More Advanced Connections
    /// 
    /// Though there are a bunch of useful connections types, we can't implement them all at once.
    /// This section lists some more cool types that we should keep in the back of our mind though
    /// and pursue after the above are complete.
    /// </summary>
    #region Out of Scope - More Advanced Connections

    public class USBConnection
    {
        // We could include discovery of USB and even allow hot-swapping potentially. In addition to
        // allowing easy communication over the port, of course.
    }

    public class BluetoothConnection
    {
        // Communicating with devices over bluetooth has been a highly requested feature for a while now. 
    }

    #endregion
}
