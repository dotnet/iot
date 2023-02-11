// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using UnitsNet;
using UnitsNet.Units;

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
            return WritePartitionMap(d.TargetRootPath);
        }

        /// <summary>
        /// Writes a partition.csv file that is adjusted to the flash size provided in options
        /// </summary>
        private bool WritePartitionMap(string targetPath)
        {
            // CONFIG_WL_SECTOR_SIZE is declared as 4096 in the ESP firmware headers
            // Aligning to a smaller size will cause a failure to mount the Fat partition.
            Information alignment = Information.FromBytes(4096);
            if (CommandLineOptions.FlashSize == null)
            {
                return true;
            }

            Information flashSize;
            if (!Information.TryParse(CommandLineOptions.FlashSize, CultureInfo.CurrentCulture, out flashSize))
            {
                Logger.LogError($"Error: Unable to parse {CommandLineOptions.FlashSize} as valid size");
                PrintSizeValues();
                return false;
            }

            if (flashSize < Information.FromKibibytes(4))
            {
                Logger.LogError($"Error: Flash size must be at least 4 kb");
                PrintSizeValues();
                return false;
            }

            Information firmwareSize = Information.FromMebibytes(1);
            if (CommandLineOptions.FirmwareSize != null && !Information.TryParse(CommandLineOptions.FirmwareSize, CultureInfo.CurrentCulture, out firmwareSize))
            {
                Logger.LogError($"Error: Unable to parse {CommandLineOptions.FirmwareSize} as valid size");
                PrintSizeValues();
                return false;
            }

            RoundUp(ref firmwareSize, alignment);

            Information programSize = Information.FromMebibytes(1);
            if (CommandLineOptions.FirmwareSize != null && !Information.TryParse(CommandLineOptions.ProgramSize, CultureInfo.CurrentCulture, out programSize))
            {
                Logger.LogError($"Error: Unable to parse {CommandLineOptions.ProgramSize} as valid size");
                PrintSizeValues();
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
            string offsetField = $"0x{AsBytes(startOffset):x},";
            string sizeField = $"0x{AsBytes(firmwareSize):x}";
            partitionMap.AppendLine($"factory,        app,       factory,  {offsetField} {sizeField}");

            startOffset = startOffset + firmwareSize;
            offsetField = $"0x{AsBytes(startOffset):x},";
            sizeField = $"0x{AsBytes(programSize):x}";
            partitionMap.AppendLine($"ilcode,         data,      ,         {offsetField} {sizeField}");
            startOffset = startOffset + programSize;
            offsetField = $"0x{AsBytes(startOffset):x},";
            Information fatSize = flashSize - startOffset;

            if (fatSize < Information.FromKibibytes(10))
            {
                // At this time, we do not support external flash cards as FAT partitions. Would be something useful to add, of course.
                Logger.LogError("There's not enough space left for the fat data partition. Check your space allocation");
                return false;
            }

            sizeField = $"0x{AsBytes(fatSize):x}";
            partitionMap.AppendLine($"ffat,           data,      fat,      {offsetField}{sizeField}");

            partitionMap.AppendLine();
            partitionMap.AppendLine("# Comments:");
            partitionMap.AppendLine("# nvs: Non-Volatile storage. Used to mimic EEPROM");
            partitionMap.AppendLine("# phy_init: Configuration section");
            partitionMap.AppendLine($"# factory: The firmware partition ({firmwareSize.Kilobytes} kb)");
            partitionMap.AppendLine($"# ilcode: The user code partition ({programSize.Kilobytes} kb)");
            partitionMap.AppendLine($"# ffat: The data partition ({fatSize.Kilobytes} kb)");

            using (var file = new StreamWriter(Path.Combine(targetPath, "partitions.csv")))
            {
                file.Write(partitionMap);
            }

            Logger.LogInformation($"Written partition table partitions.csv to {targetPath}");
            Logger.LogDebug(partitionMap.ToString());

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

        private long AsBytes(Information information)
        {
            return (long)information.Bytes;
        }

        private void PrintSizeValues()
        {
            Logger.LogInformation("Valid units for sizes: ");
            foreach (var u in Information.Info.UnitInfos)
            {
                decimal factor = Information.From(1, u.Value) / Information.FromBytes(1);
                Logger.LogInformation($"Use {Information.GetAbbreviation(u.Value)} for a multiplication by {factor:N0} bytes");
            }
        }
    }
}
