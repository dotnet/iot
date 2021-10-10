#pragma warning disable CS1591
namespace Iot.Device.Arduino
{
    internal enum ExecutorCommand : byte
    {
        None = 0,
        DeclareMethod = 1,
        LoadIl = 3,
        StartTask = 4,
        ResetExecutor = 5,
        KillTask = 6,
        MethodSignature = 7,
        ClassDeclaration = 8,
        ClassDeclarationEnd = 9, // Last part of class declaration
        ConstantData = 10,
        Interfaces = 11,
        CopyToFlash = 12,

        WriteFlashHeader = 13,
        CheckFlashVersion = 14,
        EraseFlash = 15,

        SetConstantMemorySize = 16,
        SpecialTokenList = 17,
        ExceptionClauses = 18,

        ArrayOperations = 19,

        Nack = 0x7e,
        Ack = 0x7f,
    }
}
