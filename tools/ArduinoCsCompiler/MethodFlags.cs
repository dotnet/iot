using System;

#pragma warning disable CS1591
namespace ArduinoCsCompiler
{
    [Flags]
    public enum MethodFlags
    {
        None = 0,
        Static = 1,
        Virtual = 2,
        SpecialMethod = 4, // Method will resolve to a built-in function on the arduino
        Void = 8, // The method returns void
        Ctor = 16, // The method is a ctor (which only implicitly returns "this"); the flag is not set for static ctors.
        Abstract = 32, // The method is abstract (or an interface stub)
        ExceptionClausesPresent = 64, // The method has at least one exception clause
    }
}
