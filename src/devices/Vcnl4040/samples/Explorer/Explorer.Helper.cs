// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

internal partial class Explorer
{
    private static bool PromptEnum<T>(string prompt, out T value)
        where T : struct
    {
        Array enumValues = Enum.GetValues(typeof(T));

        Console.WriteLine(prompt);
        int n = 0;
        foreach (var x in enumValues)
        {
            Console.WriteLine($"  ({n}) {x}");
            n++;
        }

        Console.Write("=> ");

        string? input = Console.ReadLine();
        bool result = int.TryParse(input, out int choice);
        if (!result)
        {
            value = default;
            return false;
        }

        if (choice < 0 || choice >= n)
        {
            Console.WriteLine($"\nINVALID CHOICE ({choice})\n");
            value = default;
            return false;
        }

        object? choosenEnumValue = enumValues.GetValue(choice);
        if (choosenEnumValue != null)
        {
            value = (T)choosenEnumValue!;
            return true;
        }

        Console.WriteLine($"\nINVALID CHOICE ({choice})\n");
        value = default;
        return false;
    }

    private static bool PromptIntegerValue(string prompt, out int value, bool fromHex = false, int min = int.MinValue, int max = int.MaxValue)
    {
        Console.Write(prompt + ": ");
        string? input = Console.ReadLine();
        bool result = false;
        if (!fromHex)
        {
            result = int.TryParse(input, out value);
        }
        else
        {
            result = int.TryParse(input, NumberStyles.HexNumber, null, out value);
        }

        if (!result)
        {
            Console.WriteLine("\nINVALID INPUT\n");
            return false;
        }

        if (value < min || value > max)
        {
            Console.WriteLine($"\nINPUT OUT OF RANGE ({min}-{max})\n");
            return false;
        }

        return true;
    }

    private static void PrintBarGraph(double value, double maxValue, string addInfo = "")
    {
        Console.CursorLeft = 0;

        string addInfoPart = $" ({value,8:0.000}){(!string.IsNullOrEmpty(addInfo) ? " [" + addInfo + "]" : string.Empty)}";

        StringBuilder line = new();
        int barWidth = Console.WindowWidth - addInfoPart.Length - 2;
        int progressBarWidth = (int)((double)Math.Min(value, maxValue) / maxValue * barWidth);
        int spaces = barWidth - progressBarWidth;

        line.Append('[');

        for (int i = 0; i < progressBarWidth; i++)
        {
            line.Append('#');
        }

        for (int i = 0; i < spaces; i++)
        {
            line.Append(' ');
        }

        line.Append(']');
        line.Append(addInfoPart);

        Console.Write(line.ToString());
        Console.Write(new string(' ', Console.WindowWidth - line.Length));
        Console.CursorLeft = 0;
    }

    private enum YesNoChoice
    {
        No,
        Yes
    }

    private enum YesNoCancelChoice
    {
        No,
        Yes,
        Cancel
    }

    private enum OnOffCancelChoice
    {
        Off,
        On,
        Cancel
    }
}
