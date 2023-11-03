// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Globalization;

internal partial class ExplorerApp
{
    private static bool PromptMultipleChoice(string prompt, List<string> choices, out int choice)
    {
        Console.WriteLine(prompt);
        int choiceCount = 0;
        foreach (string choiceStr in choices)
        {
            Console.WriteLine($"({choiceCount}) {choiceStr}");
            choiceCount++;
        }

        Console.Write("=> ");

        string? input = Console.ReadLine();
        bool result = int.TryParse(input, out choice);

        if (!result || choice < 0 || choice >= choiceCount)
        {
            Console.WriteLine("Invalid input or choice");
            return false;
        }

        return true;
    }

    private static bool PromptEnum<T>(string prompt, out T value)
        where T : struct
    {
        Console.WriteLine(prompt);
        foreach (var x in Enum.GetValues(typeof(T)))
        {
            Console.WriteLine("  " + x.ToString());
        }

        Console.Write("=> ");

        string? input = Console.ReadLine();
        if (Enum.TryParse<T>(input, true, out T parsedValue) && Enum.IsDefined(typeof(T), parsedValue))
        {
            value = parsedValue;
            return true;
        }
        else
        {
            Console.WriteLine($"Invalid input ({input})");
            value = default(T);
            return false;
        }
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
            Console.WriteLine("Invalid input");
            return false;
        }

        if (value < min || value > max)
        {
            Console.WriteLine($"Input out of range ({min}-{max})");
            return false;
        }

        return true;
    }

    private static void PrintBarGraph(int value, int maxValue, int width)
    {
        if (value > maxValue)
        {
            value = maxValue;
        }

        Console.CursorLeft = 0;
        Console.Write("[");

        int progressBarWidth = (int)((double)value / maxValue * width);
        int spaces = width - progressBarWidth;

        for (int i = 0; i < progressBarWidth; i++)
        {
            Console.Write("#");
        }

        for (int i = 0; i < spaces; i++)
        {
            Console.Write(" ");
        }

        Console.Write($"] ({value})");
        Console.CursorLeft = 0;
    }
}
