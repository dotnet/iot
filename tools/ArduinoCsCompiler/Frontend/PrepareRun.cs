using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using UnitsNet;

namespace ArduinoCsCompiler
{
    internal class PrepareRun : Run<PrepareOptions>
    {
        public PrepareRun(PrepareOptions options)
        : base(options)
        {
        }

        public override bool RunCommand()
        {
            WriteRuntimeCoreData d = new WriteRuntimeCoreData(CommandLineOptions.TargetPath);
            d.Write();
            Logger.LogInformation($"Runtime data written to {d.TargetPath}");
            return WritePartitionMap();
        }

        /// <summary>
        /// Writes a partition.csv file that is adjusted to the flash size provided in options
        /// </summary>
        private bool WritePartitionMap()
        {
            Information alignment = Information.FromKilobytes(1);
            if (CommandLineOptions.FlashSize == null)
            {
                return true;
            }

            Information flashSize;
            if (!Information.TryParse(CommandLineOptions.FlashSize, CultureInfo.CurrentCulture, out flashSize))
            {
                Logger.LogError($"Error: Unable to parse {CommandLineOptions.FlashSize} as valid size");
                return false;
            }

            if (flashSize < Information.FromKilobytes(4))
            {
                Logger.LogError($"Error: Flash size must be at least 4 kb");
                return false;
            }

            Information firmwareSize = Information.FromMegabytes(1);
            if (CommandLineOptions.FirmwareSize != null && !Information.TryParse(CommandLineOptions.FirmwareSize, CultureInfo.CurrentCulture, out firmwareSize))
            {
                Logger.LogError($"Error: Unable to parse {CommandLineOptions.FirmwareSize} as valid size");
                return false;
            }

            RoundUp(ref firmwareSize, alignment);

            Information programSize = Information.FromMegabytes(1);
            if (CommandLineOptions.FirmwareSize != null && !Information.TryParse(CommandLineOptions.ProgramSize, CultureInfo.CurrentCulture, out programSize))
            {
                Logger.LogError($"Error: Unable to parse {CommandLineOptions.ProgramSize} as valid size");
                return false;
            }

            RoundUp(ref programSize, alignment);

            StringBuilder partitionMap = new StringBuilder();
            partitionMap.AppendLine($"# ESP32 partition map. Total flash size reserved {flashSize}");
            partitionMap.AppendLine($"# Name        | Type     | SubType | Offset  | Size    | Flags");
            // This is the firmware partition offset. When using the default bootloader, this is constant
            Information startOffset = Information.FromBytes(0x10000);
            partitionMap.AppendLine($"nvs,            data,      nvs,      0x9000,   0x6000,");
            partitionMap.AppendLine($"phy_init,       data,      phy,      0xf000,   0x1000,");
            string offsetField = $"0x{startOffset.Bytes:x,-11},";
            string sizeField = $"0x{firmwareSize.Bytes:x}";
            partitionMap.AppendLine($"factory,        app,       factory,  {offsetField}{sizeField}");

            startOffset = startOffset + firmwareSize;
            offsetField = $"0x{startOffset.Bytes:x,-11},";
            sizeField = $"0x{programSize.Bytes:x}";
            partitionMap.AppendLine($"ilcode,         data,      ,         {offsetField}{sizeField}");
            startOffset = startOffset + programSize;
            offsetField = $"0x{startOffset.Bytes:x,-11},";
            Information fatSize = flashSize - startOffset;
            sizeField = $"0x{fatSize.Bytes:x}";
            partitionMap.AppendLine($"ffat,           data,      fat,      {offsetField}{sizeField}");

            partitionMap.AppendLine("# Comments:");
            partitionMap.AppendLine("# nvs: Non-Volatile storage. Used to mimic EEPROM");
            partitionMap.AppendLine("# phy_init: Configuration section");
            partitionMap.AppendLine($"# factory: The firmware partition ({firmwareSize.Kilobytes} kb)");
            partitionMap.AppendLine($"# ilcode: The user code partition ({programSize.Kilobytes} kb)");
            partitionMap.AppendLine($"# ffat: The data partition ({fatSize.Kilobytes} kb)");
            return true;
        }

        private void RoundUp(ref Information offset, Information align)
        {
            long offsetBytes = (long)offset.Bytes;
            long alignBytes = (long)align.Bytes;
            long evenBy = offsetBytes % alignBytes;
            if (evenBy == 0)
            {
                return;
            }

            offsetBytes += (alignBytes - evenBy);

            offset = Information.FromBytes(offsetBytes);
        }
    }
}
