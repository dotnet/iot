// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Iot.Device.Mpu9250
{
    [Flags]
    internal enum UserControls
    {
        None = 0b0000_0000,
        // 1 – Reset all gyro digital signal path, accel digital signal path, and temp digital signal path. 
        // This bit also clears all the sensor registers.  SIG_COND_RST is a pulse of one clk8M wide. 
        SIG_COND_RST = 0b0000_0001,
        // 1 – Reset I2C Master module. Reset is asynchronous.  This bit auto clears after one clock cycle. 
        // NOTE:  This bit should only be set when the I2C master has hung.  If this bit is set during an active I2C master transaction, 
        // the I2C slave will hang, which will require the host to reset the slave. 
        I2C_MST_RST = 0b0000_0010,
        // 1 – Reset FIFO module. Reset is asynchronous.  This bit auto clears after one clock cycle
        FIFO_RST = 0b0000_0100,
        // 1 – Reset I2C Slave module and put the serial interface in SPI mode only.  This bit auto clears after one clock cycle. 
        I2C_IF_DIS = 0b0001_0000,
        // 1 – Enable the I2C Master I/F module; pins ES_DA and ES_SCL are isolated from pins SDA/SDI and SCL/ SCLK.
        // 0 – Disable I2C Master I/F module; pins ES_DA and ES_SCL are logically driven by pins SDA/SDI and SCL/ SCLK.
        // NOTE:  DMP will run when enabled, even if all internal sensors are disabled, except when the sample rate is set to 8Khz
        I2C_MST_EN = 0b0010_0000,
        // 1 – Enable FIFO operation mode.
        // 0 – Disable FIFO access from serial interface.
        // To disable FIFO writes by dma, use FIFO_EN register.
        // To disable possible FIFO writes from DMP, disable the DMP. 
        FIFO_EN = 0b0100_0000,
    }
}
