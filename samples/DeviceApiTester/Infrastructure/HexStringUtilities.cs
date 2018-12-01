// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Text;

namespace DeviceApiTester.Infrastructure
{
    public static class HexStringUtilities
    {
        public static string FormatByteData(byte[] data, int perGroup = 4, int perLine = 16)
        {
            data = data ?? throw new ArgumentNullException(nameof(data));
            if (perGroup < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(perGroup));
            }
            if (perLine < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(perLine));
            }

            int dataLength = data.Length;
            int lineCount = dataLength / perLine;

            const string groupDelimeter = " ";

            var sb = new StringBuilder(
                dataLength * 2                                     // 2 characters per byte
                + dataLength / perGroup * groupDelimeter.Length    // gropu delimiter string
                + lineCount * Environment.NewLine.Length           // 1 new-line string between each line
                + perLine);                                        // some extra calculation padding

            int groupsPerLine = perLine / perGroup;

            int dataIndex = 0;
            for (int lineIndex = 0; lineIndex < lineCount && dataIndex < dataLength; ++lineIndex)
            {
                for (int groupIndex = 0; groupIndex < groupsPerLine && dataIndex < dataLength; ++groupIndex)
                {
                    for (int byteIndex = 0; byteIndex < perGroup && dataIndex < dataLength; ++byteIndex, ++dataIndex)
                    {
                        sb.AppendFormat("{0:X2}", data[dataIndex]);
                    }

                    sb.Append(groupDelimeter);
                }

                sb.AppendLine();
            }

            return sb.ToString();
        }

    }
}
