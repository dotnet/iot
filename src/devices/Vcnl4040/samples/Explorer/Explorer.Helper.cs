// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Globalization;

internal partial class Explorer
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
            Console.WriteLine("\nINVALID INPUT OR CHOICE\n");
            return false;
        }

        return true;
    }

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

        Console.WriteLine("  (x) : cancel");
        Console.Write("=> ");

        string? input = Console.ReadLine();
        if (input == "x")
        {
            value = default;
            return false;
        }

        bool result = int.TryParse(input, out int choice);
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

    private static void PrintBarGraph(int value, int maxValue, int width, string addInfo = "")
    {
        Console.CursorLeft = 0;
        Console.Write("[");

        int progressBarWidth = (int)((double)Math.Min(value, maxValue) / maxValue * width);
        int spaces = width - progressBarWidth;

        for (int i = 0; i < progressBarWidth; i++)
        {
            Console.Write("#");
        }

        for (int i = 0; i < spaces; i++)
        {
            Console.Write(" ");
        }

        Console.Write($"] ({value}){(!string.IsNullOrEmpty(addInfo) ? " [" + addInfo + "]" : string.Empty)}".PadLeft(8, ' '));
        Console.CursorLeft = 0;
    }

    private enum YesNoChoice
    {
        No,
        Yes
    }
}
