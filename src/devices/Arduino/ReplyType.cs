using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#pragma warning disable CS1591
namespace Iot.Device.Arduino
{
    public enum ReplyType
    {
        None = 0,
        SysexCommand = 1,
        AsciiData = 2,
    }
}
