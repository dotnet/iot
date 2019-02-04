// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Iot.Device.BrickPi3.Extensions
{
	/// <summary>
	/// Extensions to get next or previous enum
	/// </summary>
	internal static class EnumExtensions
	{
	    public static T Next<T>(this T src) where T : struct
	    {
	        if (!typeof(T).IsEnum) throw new ArgumentException($"Argumnent {typeof(T).FullName} is not an Enum");
	
	        T[] Arr = (T[])Enum.GetValues(src.GetType());
	        int j = Array.IndexOf<T>(Arr, src) + 1;
	        return (Arr.Length==j) ? Arr[0] : Arr[j];            
	    }
	    
	    public static T Previous<T>(this T src) where T : struct
	    {
	        if (!typeof(T).IsEnum) throw new ArgumentException($"Argumnent {typeof(T).FullName} is not an Enum");
	
	        T[] Arr = (T[])Enum.GetValues(src.GetType());
	        int j = Array.IndexOf<T>(Arr, src) -1;
	        return (j < 0) ? Arr[Arr.Length-1] : Arr[j];            
	    }
	}
}

