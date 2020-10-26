using System;
using System.Collections.Generic;
using System.Text;

namespace Iot.Device.MettlerToledo.Messages
{
    internal static class Errors
    {
        /// <summary>
        /// The balance has not recognized the received command.
        /// </summary>
        internal const string SYNTAX_ERROR = "ES";

        /// <summary>
        /// The balance has received a "faulty" command, e.g. owing to a parity error or interface break.
        /// </summary>
        internal const string TRANSMISSION_ERROR = "ET";

        /// <summary>
        /// The balance can not execute the received command.
        /// </summary>
        internal const string LOGICAL_ERROR = "EL";

    }

    internal static class Commands
    {
        internal const string SEND_STABLE_WEIGHT_VALUE = "S";
        internal const string SEND_IMMEDIATE_WEIGHT_VALUE = "SI";
        internal const string SEND_IMMEDIATE_WEIGHT_VALUE_AND_REPEAT = "SIR";
        internal const string ZERO_BALANCE = "Z";
        internal const string ZERO_BALANCE_IMMEDIATELY = "ZI";
        internal const string RESET_BALANCE = "@";
        internal const string GET_SERIAL_NUMBER = "I4";

        /// <summary>
        /// Command to write to the balance display.
        /// </summary>
        /// <example>
        /// <para>
        /// Write a string to the display:
        /// <code>
        /// SerialPort.WriteLine($"<see cref="WRITE_TO_BALANCE_DISPLAY"/> \"{text}\"");
        /// </code>
        /// </para>
        /// <para>
        /// Clear the display:
        /// <code>
        /// SerialPort.WriteLine($"{<see cref="WRITE_TO_BALANCE_DISPLAY"/> \"\");
        /// </code>
        /// </para>
        /// </example>
        internal const string WRITE_TO_BALANCE_DISPLAY = "D";
        internal const string DISPLAY_WEIGHT = "DW";
        internal const string KEY_CONTROL = "K";
        // TODO: write example
        internal const string SEND_WEIGHT_ON_VAL_CHANGE = "SR";
        internal const string SET_TARE = "T";
        internal const string GET_TARE = "TA";
        internal const string CLEAR_TARE = "TAC";
        internal const string TARE_IMMEDIATELY = "TI";

        internal const string GET_BALANCE_TYPE = "I11";

        internal const string SET_UNIT_1_UNIT = "M21 0";
        internal const string SET_WEIGHT_DISPLAY_UNIT = "M21 1";
        internal const string SET_WEIGHT_INFO_UNIT = "M21 2";

        internal const string GET_SET_VALUE_RELEASE = "M29";

        internal const string EXECUTE_RESET = "M38";

        internal const string POWER_OFF = "PWR 0";

        // Also returns I4 A "text" after
        internal const string POWER_ON = "PWR 1";

        // TODO: add Auto-Zero
    }

    internal static class Responses
    {
        internal const string COMMAND_EXECUTED = "A";
        internal const string PARAMETERS_MISSING = "L";
        internal const string COMMAND_NOT_EXECUTABLE = "I";

        internal static class Range
        {
            internal const string UPPER_RANGE_EXCEEDED = "+";
            internal const string LOWER_RANGE_EXCEEDED = "-";
        }

        internal static class Success
        {
            internal const string STABLE_SUCCESS = "S";
            internal const string DYNAMIC_SUCCESS = "D";
        }
    }

    /// <summary>
    /// Unit of weight as expressed by the Mettler Toledo balance.
    /// </summary>
    public enum MettlerToledoWeightUnit
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        Grams = 0,
        Kilograms = 1,
        Tonnes = 2,
        Milligrams = 3,
        Micrograms = 4,
        Carat = 5,

        Ounces = 8,
        TroyOunces = 9,
        Grain = 10,
        PennyWeight = 11,
        Momme = 12,

        TaelHongKong = 14,
        TaelSingapore = 15,
        TaelTaiwan = 16,
        Tical = 17,
        Tola = 18,
        Baht = 19,

        NoUnit = 25
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}
