// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iot.Device.Camera;

/// <summary>
/// Information on the camera listed by the tools
/// </summary>
/// <param name="Index">The progressive number assigned to the device from Libcamera</param>
/// <param name="Name">The name of the device</param>
/// <param name="MaxResolution">The maximum resolution supported from the device</param>
/// <param name="DevicePath">The native path assigned from the operating system to the device</param>
public record class CameraInfo(
    int Index,
    string Name,
    string MaxResolution,
    string DevicePath)
{
    private const string Line1 = "Available cameras";
    private const string Line2 = "-----------------";

    /// <summary>
    /// Parse the string obtained from the --list-cameras command line and
    /// extracts the main characteristics
    /// </summary>
    /// <param name="listOutputString">The string output returned from the execution with --list-cameras command line</param>
    /// <returns></returns>
    /// <exception cref="Exception">The input string has an unexpected strucutre and cannot be parsed</exception>
    public static async Task<IEnumerable<CameraInfo>> From(string listOutputString)
    {
        if (listOutputString == null || listOutputString.Length < Line1.Length + Line2.Length)
        {
            throw new Exception($"This text does not comply to the expected list of available cameras");
        }

        var result = new List<CameraInfo>();
        using StringReader sr = new(listOutputString);
        var line1 = await sr.ReadLineAsync();
        var line2 = await sr.ReadLineAsync();
        if (line1 != Line1 || line2 != Line2)
        {
            throw new Exception("The provided text does not start as expected");
        }

        string? line;
        while ((line = await sr.ReadLineAsync()) != null)
        {
            var firstChar = line[0];
            if (firstChar == ' ')
            {
                continue;
            }

            var colonIndex = line.IndexOf(':');
            if (colonIndex == -1)
            {
                continue;
            }

            if (!int.TryParse(line.Substring(0, colonIndex).Trim(), out int index))
            {
                continue;
            }

            var openBracketIndex = line.IndexOf('[');
            var closeBracketIndex = line.IndexOf(']');
            if (openBracketIndex == -1 || closeBracketIndex == -1)
            {
                continue;
            }

            var name = line.Substring(colonIndex + 1, openBracketIndex - (colonIndex + 1)).Trim();

            var resolution = line.Substring(openBracketIndex + 1, closeBracketIndex - (openBracketIndex + 1)).Trim();

            var openPar = line.IndexOf('(', closeBracketIndex);
            var closePar = line.IndexOf(')', closeBracketIndex);
            if (openPar == -1 || closePar == -1)
            {
                continue;
            }

            var devicePath = line.Substring(openPar + 1, closePar - (openPar + 1)).Trim();

            result.Add(new(index, name, resolution, devicePath));
        }

        return result;
    }
}
