// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Text;

namespace Iot.Device.Pn5180
{
    internal enum Command
    {
        /// <summary>Write one 32bit register value</summary>
        WRITE_REGISTER = 0x00,

        /// <summary>Sets one 32bit register value using a 32 bit OR mask</summary>
        WRITE_REGISTER_OR_MASK = 0x01,

        /// <summary>Sets one 32bit register value using a 32 bit AND mask</summary>
        WRITE_REGISTER_AND_MASK = 0x02,

        /// <summary>Processes an array of register addresses in random order and
        /// performs the defined action on these addresses.</summary>
        WRITE_REGISTER_MULTIPLE = 0x03,

        /// <summary>Reads one 32bit register value</summary>
        READ_REGISTER = 0x04,

        /// <summary>Reads from an array of max.18 register addresses in random
        /// order</summary>
        READ_REGISTER_MULTIPLE = 0x05,

        /// <summary>Processes an array of EEPROM addresses in random order and
        /// writes the value to these addresses</summary>
        WRITE_EEPROM = 0x06,

        /// <summary>Processes an array of EEPROM addresses from a start address
        /// and reads the values from these addresses</summary>
        READ_EEPROM = 0x07,

        /// <summary>This instruction is used to write data into the transmission buffer</summary>
        WRITE_TX_DATA = 0x08,

        /// <summary>This instruction is used to write data into the transmission buffer,
        /// the START_SEND bit is automatically set.</summary>
        SEND_DATA = 0x09,

        /// <summary>This instruction is used to read data from reception buffer, after
        /// successful reception.</summary>
        READ_DATA = 0x0A,

        /// <summary>This instruction is used to switch the mode. It is only possible to
        /// switch from NormalMode to standby, LPCD or Autocoll.</summary>
        SWITCH_MODE = 0x0B,

        /// <summary>This instruction is used to perform a MIFARE Classic
        /// Authentication on an activated card.</summary>
        MIFARE_AUTHENTICATE = 0x0C,

        /// <summary>This instruction is used to perform an inventory of ISO18000-3M3 tags.</summary>
        EPC_INVENTORY = 0x0D,

        /// <summary>This instruction is used to resume the inventory algorithm in case it is paused.</summary>
        EPC_RESUME_INVENTORY = 0x0E,

        /// <summary>This instruction is used to retrieve the size of the inventory result.</summary>
        EPC_RETRIEVE_INVENTORY_RESULT_SIZE = 0x0F,

        /// <summary>This instruction is used to retrieve the result of a preceding
        /// EPC_INVENTORY or EPC_RESUME_INVENTORY instruction.</summary>
        EPC_RETRIEVE_INVENTORY_RESULT = 0x10,

        /// <summary>This instruction is used to load the RF configuration from
        /// EEPROM into the configuration registers.</summary>
        LOAD_RF_CONFIG = 0x11,

        /// <summary>This instruction is used to update the RF configuration within
        /// EEPROM.</summary>
        UPDATE_RF_CONFIG = 0x12,

        /// <summary>This instruction is used to retrieve the number of registers for a
        /// selected RF configuration</summary>
        RETRIEVE_RF_CONFIG_SIZE = 0x13,

        /// <summary>This instruction is used to read out an RF configuration. The
        /// register address-value-pairs are available in the response</summary>
        RETRIEVE_RF_CONFIG = 0x14,

        /// <summary>This instruction switch on the RF Field</summary>
        RF_ON = 0x16,

        /// <summary>This instruction switch off the RF Field</summary>
        RF_OFF = 0x17,

        /// <summary>Enables the Digital test bus</summary>
        CONFIGURE_TESTBUS_DIGITAL = 0x18,

        /// <summary>Enables the Analog test bus</summary>
        CONFIGURE_TESTBUS_ANALOG = 0x19,
    }
}
