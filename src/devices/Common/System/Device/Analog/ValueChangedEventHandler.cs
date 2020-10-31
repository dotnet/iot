using System;
using System.Collections.Generic;
using System.Text;

namespace System.Device.Analog
{
    /// <summary>
    /// Delegate that defines the structure for callbacks when the value of a measurement (i.e. analog input) changes.
    /// </summary>
    /// <param name="sender">The sender of the event.</param>
    /// <param name="pinValueChangedEventArgs">The pin value changed arguments from the event.</param>
    public delegate void ValueChangedEventHandler(object sender, ValueChangedEventArgs pinValueChangedEventArgs);
}
