using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#pragma warning disable CS1591
namespace Iot.Device.Arduino
{
    public enum OpCodeType
    {
        InlineNone = 0, // no inline args
        InlineVar = 1,  // local variable       (U2 (U1 if Short on))
        InlineI = 2,    // an signed integer    (I4 (I1 if Short on))
        InlineR = 3,    // a real number        (R8 (R4 if Short on))
        InlineBrTarget = 4,    // branch target        (I4 (I1 if Short on))
        InlineI8 = 5,
        InlineMethod = 6,   // method token (U4)
        InlineField = 7,   // field token  (U4)
        InlineType = 8,   // type token   (U4)
        InlineString = 9,   // string TOKEN (U4)
        InlineSig = 10,  // signature tok (U4)
        InlineRVA = 11,  // ldptr token  (U4)
        InlineTok = 12,  // a meta-data token of unknown type (U4)
        InlineSwitch = 13,  // count (U4), pcrel1 (U4) .... pcrelN (U4)
        InlinePhi = 14,  // count (U1), var1 (U2) ... varN (U2)

        // WATCH OUT we are close to the limit here, if you add
        // more enumerations you need to change ShortIline definition below

        // The extended enumeration also encodes the size in the IL stream
        ShortInline = 16, // if this bit is set, the format is the 'short' format
        PrimaryMask = (ShortInline - 1), // mask these off to get primary enumeration above
        ShortInlineVar = (ShortInline + InlineVar),
        ShortInlineI = (ShortInline + InlineI),
        ShortInlineR = (ShortInline + InlineR),
        ShortInlineBrTarget = (ShortInline + InlineBrTarget),
        InlineOpcode = (ShortInline + InlineNone),    // This is only used internally.  It means the 'opcode' is two byte instead of 1
    }

    public class OpCodeDefinitions
    {
        private const int Pop0 = 0;
        private const int Pop1 = 1;
        private const int Pop2 = 2;
        private const int Push1 = 1;
        private const int Push0 = 0;
        private const int PushI = 1;
        private const int PushRef = 1;
        private const int PopI = 1;
        private const int PushI8 = 2;
        private const int PushR4 = 1;
        private const int PushR8 = 2;
        private const int PopI8 = 2;
        private const int PopR4 = 1;
        private const int PopR8 = 2;
        private const int VarPush = -1;
        private const int VarPop = -1;
        private const int PopRef = 1;

        // I think we don't care about these
        private const int NEXT = 1;
        private const int ERROR = 2;
        private const int COND_BRANCH = 3;
        private const int CALL = 4;
        private const int RETURN = 5;
        private const int BRANCH = 6;
        private const int META = 7;

        internal static SingleOpCode[] OpcodeDef =
        {
            new SingleOpCode(OpCode.CEE_NOP, "nop", Pop0, Push0, OpCodeType.InlineNone, 0, 1, 0xFF, 0x00, NEXT),
            new SingleOpCode(OpCode.CEE_BREAK, "break", Pop0, Push0, OpCodeType.InlineNone, 0, 1, 0xFF, 0x01, ERROR),
            new SingleOpCode(OpCode.CEE_LDARG_0, "ldarg.0", Pop0, Push1, OpCodeType.InlineNone, 0, 1, 0xFF, 0x02, NEXT),
            new SingleOpCode(OpCode.CEE_LDARG_1, "ldarg.1", Pop0, Push1, OpCodeType.InlineNone, 1, 1, 0xFF, 0x03, NEXT),
            new SingleOpCode(OpCode.CEE_LDARG_2, "ldarg.2", Pop0, Push1, OpCodeType.InlineNone, 2, 1, 0xFF, 0x04, NEXT),
            new SingleOpCode(OpCode.CEE_LDARG_3, "ldarg.3", Pop0, Push1, OpCodeType.InlineNone, 3, 1, 0xFF, 0x05, NEXT),
            new SingleOpCode(OpCode.CEE_LDLOC_0, "ldloc.0", Pop0, Push1, OpCodeType.InlineNone, 0, 1, 0xFF, 0x06, NEXT),
            new SingleOpCode(OpCode.CEE_LDLOC_1, "ldloc.1", Pop0, Push1, OpCodeType.InlineNone, 1, 1, 0xFF, 0x07, NEXT),
            new SingleOpCode(OpCode.CEE_LDLOC_2, "ldloc.2", Pop0, Push1, OpCodeType.InlineNone, 2, 1, 0xFF, 0x08, NEXT),
            new SingleOpCode(OpCode.CEE_LDLOC_3, "ldloc.3", Pop0, Push1, OpCodeType.InlineNone, 3, 1, 0xFF, 0x09, NEXT),
            new SingleOpCode(OpCode.CEE_STLOC_0, "stloc.0", Pop1, Push0, OpCodeType.InlineNone, 0, 1, 0xFF, 0x0A, NEXT),
            new SingleOpCode(OpCode.CEE_STLOC_1, "stloc.1", Pop1, Push0, OpCodeType.InlineNone, 1, 1, 0xFF, 0x0B, NEXT),
            new SingleOpCode(OpCode.CEE_STLOC_2, "stloc.2", Pop1, Push0, OpCodeType.InlineNone, 2, 1, 0xFF, 0x0C, NEXT),
            new SingleOpCode(OpCode.CEE_STLOC_3, "stloc.3", Pop1, Push0, OpCodeType.InlineNone, 3, 1, 0xFF, 0x0D, NEXT),
            new SingleOpCode(OpCode.CEE_LDARG_S, "ldarg.s", Pop0, Push1, OpCodeType.ShortInlineVar, 0, 1, 0xFF, 0x0E, NEXT),
            new SingleOpCode(OpCode.CEE_LDARGA_S, "ldarga.s", Pop0, PushI, OpCodeType.ShortInlineVar, 0, 1, 0xFF, 0x0F, NEXT),
            new SingleOpCode(OpCode.CEE_STARG_S, "starg.s", Pop1, Push0, OpCodeType.ShortInlineVar, 0, 1, 0xFF, 0x10, NEXT),
            new SingleOpCode(OpCode.CEE_LDLOC_S, "ldloc.s", Pop0, Push1, OpCodeType.ShortInlineVar, 0, 1, 0xFF, 0x11, NEXT),
            new SingleOpCode(OpCode.CEE_LDLOCA_S, "ldloca.s", Pop0, PushI, OpCodeType.ShortInlineVar, 0, 1, 0xFF, 0x12, NEXT),
            new SingleOpCode(OpCode.CEE_STLOC_S, "stloc.s", Pop1, Push0, OpCodeType.ShortInlineVar, 0, 1, 0xFF, 0x13, NEXT),
            new SingleOpCode(OpCode.CEE_LDNULL, "ldnull", Pop0, PushRef, OpCodeType.InlineNone, 0, 1, 0xFF, 0x14, NEXT),
            new SingleOpCode(OpCode.CEE_LDC_I4_M1, "ldc.i4.m1", Pop0, PushI, OpCodeType.InlineNone, -1, 1, 0xFF, 0x15, NEXT),
            new SingleOpCode(OpCode.CEE_LDC_I4_0, "ldc.i4.0", Pop0, PushI, OpCodeType.InlineNone, 0, 1, 0xFF, 0x16, NEXT),
            new SingleOpCode(OpCode.CEE_LDC_I4_1, "ldc.i4.1", Pop0, PushI, OpCodeType.InlineNone, 1, 1, 0xFF, 0x17, NEXT),
            new SingleOpCode(OpCode.CEE_LDC_I4_2, "ldc.i4.2", Pop0, PushI, OpCodeType.InlineNone, 2, 1, 0xFF, 0x18, NEXT),
            new SingleOpCode(OpCode.CEE_LDC_I4_3, "ldc.i4.3", Pop0, PushI, OpCodeType.InlineNone, 3, 1, 0xFF, 0x19, NEXT),
            new SingleOpCode(OpCode.CEE_LDC_I4_4, "ldc.i4.4", Pop0, PushI, OpCodeType.InlineNone, 4, 1, 0xFF, 0x1A, NEXT),
            new SingleOpCode(OpCode.CEE_LDC_I4_5, "ldc.i4.5", Pop0, PushI, OpCodeType.InlineNone, 5, 1, 0xFF, 0x1B, NEXT),
            new SingleOpCode(OpCode.CEE_LDC_I4_6, "ldc.i4.6", Pop0, PushI, OpCodeType.InlineNone, 6, 1, 0xFF, 0x1C, NEXT),
            new SingleOpCode(OpCode.CEE_LDC_I4_7, "ldc.i4.7", Pop0, PushI, OpCodeType.InlineNone, 7, 1, 0xFF, 0x1D, NEXT),
            new SingleOpCode(OpCode.CEE_LDC_I4_8, "ldc.i4.8", Pop0, PushI, OpCodeType.InlineNone, 8, 1, 0xFF, 0x1E, NEXT),
            new SingleOpCode(OpCode.CEE_LDC_I4_S, "ldc.i4.s", Pop0, PushI, OpCodeType.ShortInlineI, 0, 1, 0xFF, 0x1F, NEXT),
            new SingleOpCode(OpCode.CEE_LDC_I4, "ldc.i4", Pop0, PushI, OpCodeType.InlineI, 0, 1, 0xFF, 0x20, NEXT),
            new SingleOpCode(OpCode.CEE_LDC_I8, "ldc.i8", Pop0, PushI8, OpCodeType.InlineI8, 0, 1, 0xFF, 0x21, NEXT),
            new SingleOpCode(OpCode.CEE_LDC_R4, "ldc.r4", Pop0, PushR4, OpCodeType.ShortInlineR, 0, 1, 0xFF, 0x22, NEXT),
            new SingleOpCode(OpCode.CEE_LDC_R8, "ldc.r8", Pop0, PushR8, OpCodeType.InlineR, 0, 1, 0xFF, 0x23, NEXT),
            new SingleOpCode(OpCode.CEE_UNUSED99, "unused99", Pop0, Push0, OpCodeType.InlineNone, 0, 1, 0xFF, 0x24, NEXT),
            new SingleOpCode(OpCode.CEE_DUP, "dup", Pop1, Push1 + Push1, OpCodeType.InlineNone, 0, 1, 0xFF, 0x25, NEXT),
            new SingleOpCode(OpCode.CEE_POP, "pop", Pop1, Push0, OpCodeType.InlineNone, 0, 1, 0xFF, 0x26, NEXT),
            new SingleOpCode(OpCode.CEE_JMP, "jmp", Pop0, Push0, OpCodeType.InlineMethod, 0, 1, 0xFF, 0x27, CALL),
            new SingleOpCode(OpCode.CEE_CALL, "call", VarPop, VarPush, OpCodeType.InlineMethod, 0, 1, 0xFF, 0x28, CALL),
            new SingleOpCode(OpCode.CEE_CALLI, "calli", VarPop, VarPush, OpCodeType.InlineSig, 0, 1, 0xFF, 0x29, CALL),
            new SingleOpCode(OpCode.CEE_RET, "ret", VarPop, Push0, OpCodeType.InlineNone, 0, 1, 0xFF, 0x2A, RETURN),
            new SingleOpCode(OpCode.CEE_BR_S, "br.s", Pop0, Push0, OpCodeType.ShortInlineBrTarget, 0, 1, 0xFF, 0x2B, BRANCH),
            new SingleOpCode(OpCode.CEE_BRFALSE_S, "brfalse.s", PopI, Push0, OpCodeType.ShortInlineBrTarget, 0, 1, 0xFF, 0x2C, COND_BRANCH),
            new SingleOpCode(OpCode.CEE_BRTRUE_S, "brtrue.s", PopI, Push0, OpCodeType.ShortInlineBrTarget, 0, 1, 0xFF, 0x2D, COND_BRANCH),
            new SingleOpCode(OpCode.CEE_BEQ_S, "beq.s", Pop1 + Pop1, Push0, OpCodeType.ShortInlineBrTarget, 0, 1, 0xFF, 0x2E, COND_BRANCH),
            new SingleOpCode(OpCode.CEE_BGE_S, "bge.s", Pop1 + Pop1, Push0, OpCodeType.ShortInlineBrTarget, 0, 1, 0xFF, 0x2F, COND_BRANCH),
            new SingleOpCode(OpCode.CEE_BGT_S, "bgt.s", Pop1 + Pop1, Push0, OpCodeType.ShortInlineBrTarget, 0, 1, 0xFF, 0x30, COND_BRANCH),
            new SingleOpCode(OpCode.CEE_BLE_S, "ble.s", Pop1 + Pop1, Push0, OpCodeType.ShortInlineBrTarget, 0, 1, 0xFF, 0x31, COND_BRANCH),
            new SingleOpCode(OpCode.CEE_BLT_S, "blt.s", Pop1 + Pop1, Push0, OpCodeType.ShortInlineBrTarget, 0, 1, 0xFF, 0x32, COND_BRANCH),
            new SingleOpCode(OpCode.CEE_BNE_UN_S, "bne.un.s", Pop1 + Pop1, Push0, OpCodeType.ShortInlineBrTarget, 0, 1, 0xFF, 0x33, COND_BRANCH),
            new SingleOpCode(OpCode.CEE_BGE_UN_S, "bge.un.s", Pop1 + Pop1, Push0, OpCodeType.ShortInlineBrTarget, 0, 1, 0xFF, 0x34, COND_BRANCH),
            new SingleOpCode(OpCode.CEE_BGT_UN_S, "bgt.un.s", Pop1 + Pop1, Push0, OpCodeType.ShortInlineBrTarget, 0, 1, 0xFF, 0x35, COND_BRANCH),
            new SingleOpCode(OpCode.CEE_BLE_UN_S, "ble.un.s", Pop1 + Pop1, Push0, OpCodeType.ShortInlineBrTarget, 0, 1, 0xFF, 0x36, COND_BRANCH),
            new SingleOpCode(OpCode.CEE_BLT_UN_S, "blt.un.s", Pop1 + Pop1, Push0, OpCodeType.ShortInlineBrTarget, 0, 1, 0xFF, 0x37, COND_BRANCH),
            new SingleOpCode(OpCode.CEE_BR, "br", Pop0, Push0, OpCodeType.InlineBrTarget, 0, 1, 0xFF, 0x38, BRANCH),
            new SingleOpCode(OpCode.CEE_BRFALSE, "brfalse", PopI, Push0, OpCodeType.InlineBrTarget, 0, 1, 0xFF, 0x39, COND_BRANCH),
            new SingleOpCode(OpCode.CEE_BRTRUE, "brtrue", PopI, Push0, OpCodeType.InlineBrTarget, 0, 1, 0xFF, 0x3A, COND_BRANCH),
            new SingleOpCode(OpCode.CEE_BEQ, "beq", Pop1 + Pop1, Push0, OpCodeType.InlineBrTarget, 0, 1, 0xFF, 0x3B, COND_BRANCH),
            new SingleOpCode(OpCode.CEE_BGE, "bge", Pop1 + Pop1, Push0, OpCodeType.InlineBrTarget, 0, 1, 0xFF, 0x3C, COND_BRANCH),
            new SingleOpCode(OpCode.CEE_BGT, "bgt", Pop1 + Pop1, Push0, OpCodeType.InlineBrTarget, 0, 1, 0xFF, 0x3D, COND_BRANCH),
            new SingleOpCode(OpCode.CEE_BLE, "ble", Pop1 + Pop1, Push0, OpCodeType.InlineBrTarget, 0, 1, 0xFF, 0x3E, COND_BRANCH),
            new SingleOpCode(OpCode.CEE_BLT, "blt", Pop1 + Pop1, Push0, OpCodeType.InlineBrTarget, 0, 1, 0xFF, 0x3F, COND_BRANCH),
            new SingleOpCode(OpCode.CEE_BNE_UN, "bne.un", Pop1 + Pop1, Push0, OpCodeType.InlineBrTarget, 0, 1, 0xFF, 0x40, COND_BRANCH),
            new SingleOpCode(OpCode.CEE_BGE_UN, "bge.un", Pop1 + Pop1, Push0, OpCodeType.InlineBrTarget, 0, 1, 0xFF, 0x41, COND_BRANCH),
            new SingleOpCode(OpCode.CEE_BGT_UN, "bgt.un", Pop1 + Pop1, Push0, OpCodeType.InlineBrTarget, 0, 1, 0xFF, 0x42, COND_BRANCH),
            new SingleOpCode(OpCode.CEE_BLE_UN, "ble.un", Pop1 + Pop1, Push0, OpCodeType.InlineBrTarget, 0, 1, 0xFF, 0x43, COND_BRANCH),
            new SingleOpCode(OpCode.CEE_BLT_UN, "blt.un", Pop1 + Pop1, Push0, OpCodeType.InlineBrTarget, 0, 1, 0xFF, 0x44, COND_BRANCH),
            new SingleOpCode(OpCode.CEE_SWITCH, "switch", PopI, Push0, OpCodeType.InlineSwitch, 0, 1, 0xFF, 0x45, COND_BRANCH),
            new SingleOpCode(OpCode.CEE_LDIND_I1, "ldind.i1", PopI, PushI, OpCodeType.InlineNone, 0, 1, 0xFF, 0x46, NEXT),
            new SingleOpCode(OpCode.CEE_LDIND_U1, "ldind.u1", PopI, PushI, OpCodeType.InlineNone, 0, 1, 0xFF, 0x47, NEXT),
            new SingleOpCode(OpCode.CEE_LDIND_I2, "ldind.i2", PopI, PushI, OpCodeType.InlineNone, 0, 1, 0xFF, 0x48, NEXT),
            new SingleOpCode(OpCode.CEE_LDIND_U2, "ldind.u2", PopI, PushI, OpCodeType.InlineNone, 0, 1, 0xFF, 0x49, NEXT),
            new SingleOpCode(OpCode.CEE_LDIND_I4, "ldind.i4", PopI, PushI, OpCodeType.InlineNone, 0, 1, 0xFF, 0x4A, NEXT),
            new SingleOpCode(OpCode.CEE_LDIND_U4, "ldind.u4", PopI, PushI, OpCodeType.InlineNone, 0, 1, 0xFF, 0x4B, NEXT),
            new SingleOpCode(OpCode.CEE_LDIND_I8, "ldind.i8", PopI, PushI8, OpCodeType.InlineNone, 0, 1, 0xFF, 0x4C, NEXT),
            new SingleOpCode(OpCode.CEE_LDIND_I, "ldind.i", PopI, PushI, OpCodeType.InlineNone, 0, 1, 0xFF, 0x4D, NEXT),
            new SingleOpCode(OpCode.CEE_LDIND_R4, "ldind.r4", PopI, PushR4, OpCodeType.InlineNone, 0, 1, 0xFF, 0x4E, NEXT),
            new SingleOpCode(OpCode.CEE_LDIND_R8, "ldind.r8", PopI, PushR8, OpCodeType.InlineNone, 0, 1, 0xFF, 0x4F, NEXT),
            new SingleOpCode(OpCode.CEE_LDIND_REF, "ldind.ref", PopI, PushRef, OpCodeType.InlineNone, 0, 1, 0xFF, 0x50, NEXT),
            new SingleOpCode(OpCode.CEE_STIND_REF, "stind.ref", PopI + PopI, Push0, OpCodeType.InlineNone, 0, 1, 0xFF, 0x51, NEXT),
            new SingleOpCode(OpCode.CEE_STIND_I1, "stind.i1", PopI + PopI, Push0, OpCodeType.InlineNone, 0, 1, 0xFF, 0x52, NEXT),
            new SingleOpCode(OpCode.CEE_STIND_I2, "stind.i2", PopI + PopI, Push0, OpCodeType.InlineNone, 0, 1, 0xFF, 0x53, NEXT),
            new SingleOpCode(OpCode.CEE_STIND_I4, "stind.i4", PopI + PopI, Push0, OpCodeType.InlineNone, 0, 1, 0xFF, 0x54, NEXT),
            new SingleOpCode(OpCode.CEE_STIND_I8, "stind.i8", PopI + PopI8, Push0, OpCodeType.InlineNone, 0, 1, 0xFF, 0x55, NEXT),
            new SingleOpCode(OpCode.CEE_STIND_R4, "stind.r4", PopI + PopR4, Push0, OpCodeType.InlineNone, 0, 1, 0xFF, 0x56, NEXT),
            new SingleOpCode(OpCode.CEE_STIND_R8, "stind.r8", PopI + PopR8, Push0, OpCodeType.InlineNone, 0, 1, 0xFF, 0x57, NEXT),
            new SingleOpCode(OpCode.CEE_ADD, "add", Pop1 + Pop1, Push1, OpCodeType.InlineNone, 0, 1, 0xFF, 0x58, NEXT),
            new SingleOpCode(OpCode.CEE_SUB, "sub", Pop1 + Pop1, Push1, OpCodeType.InlineNone, 0, 1, 0xFF, 0x59, NEXT),
            new SingleOpCode(OpCode.CEE_MUL, "mul", Pop1 + Pop1, Push1, OpCodeType.InlineNone, 0, 1, 0xFF, 0x5A, NEXT),
            new SingleOpCode(OpCode.CEE_DIV, "div", Pop1 + Pop1, Push1, OpCodeType.InlineNone, 0, 1, 0xFF, 0x5B, NEXT),
            new SingleOpCode(OpCode.CEE_DIV_UN, "div.un", Pop1 + Pop1, Push1, OpCodeType.InlineNone, 0, 1, 0xFF, 0x5C, NEXT),
            new SingleOpCode(OpCode.CEE_REM, "rem", Pop1 + Pop1, Push1, OpCodeType.InlineNone, 0, 1, 0xFF, 0x5D, NEXT),
            new SingleOpCode(OpCode.CEE_REM_UN, "rem.un", Pop1 + Pop1, Push1, OpCodeType.InlineNone, 0, 1, 0xFF, 0x5E, NEXT),
            new SingleOpCode(OpCode.CEE_AND, "and", Pop1 + Pop1, Push1, OpCodeType.InlineNone, 0, 1, 0xFF, 0x5F, NEXT),
            new SingleOpCode(OpCode.CEE_OR, "or", Pop1 + Pop1, Push1, OpCodeType.InlineNone, 0, 1, 0xFF, 0x60, NEXT),
            new SingleOpCode(OpCode.CEE_XOR, "xor", Pop1 + Pop1, Push1, OpCodeType.InlineNone, 0, 1, 0xFF, 0x61, NEXT),
            new SingleOpCode(OpCode.CEE_SHL, "shl", Pop1 + Pop1, Push1, OpCodeType.InlineNone, 0, 1, 0xFF, 0x62, NEXT),
            new SingleOpCode(OpCode.CEE_SHR, "shr", Pop1 + Pop1, Push1, OpCodeType.InlineNone, 0, 1, 0xFF, 0x63, NEXT),
            new SingleOpCode(OpCode.CEE_SHR_UN, "shr.un", Pop1 + Pop1, Push1, OpCodeType.InlineNone, 0, 1, 0xFF, 0x64, NEXT),
            new SingleOpCode(OpCode.CEE_NEG, "neg", Pop1, Push1, OpCodeType.InlineNone, 0, 1, 0xFF, 0x65, NEXT),
            new SingleOpCode(OpCode.CEE_NOT, "not", Pop1, Push1, OpCodeType.InlineNone, 0, 1, 0xFF, 0x66, NEXT),
            new SingleOpCode(OpCode.CEE_CONV_I1, "conv.i1", Pop1, PushI, OpCodeType.InlineNone, 0, 1, 0xFF, 0x67, NEXT),
            new SingleOpCode(OpCode.CEE_CONV_I2, "conv.i2", Pop1, PushI, OpCodeType.InlineNone, 0, 1, 0xFF, 0x68, NEXT),
            new SingleOpCode(OpCode.CEE_CONV_I4, "conv.i4", Pop1, PushI, OpCodeType.InlineNone, 0, 1, 0xFF, 0x69, NEXT),
            new SingleOpCode(OpCode.CEE_CONV_I8, "conv.i8", Pop1, PushI8, OpCodeType.InlineNone, 0, 1, 0xFF, 0x6A, NEXT),
            new SingleOpCode(OpCode.CEE_CONV_R4, "conv.r4", Pop1, PushR4, OpCodeType.InlineNone, 0, 1, 0xFF, 0x6B, NEXT),
            new SingleOpCode(OpCode.CEE_CONV_R8, "conv.r8", Pop1, PushR8, OpCodeType.InlineNone, 0, 1, 0xFF, 0x6C, NEXT),
            new SingleOpCode(OpCode.CEE_CONV_U4, "conv.u4", Pop1, PushI, OpCodeType.InlineNone, 0, 1, 0xFF, 0x6D, NEXT),
            new SingleOpCode(OpCode.CEE_CONV_U8, "conv.u8", Pop1, PushI8, OpCodeType.InlineNone, 0, 1, 0xFF, 0x6E, NEXT),
            new SingleOpCode(OpCode.CEE_CALLVIRT, "callvirt", VarPop, VarPush, OpCodeType.InlineMethod, 0, 1, 0xFF, 0x6F, CALL),
            new SingleOpCode(OpCode.CEE_CPOBJ, "cpobj", PopI + PopI, Push0, OpCodeType.InlineType, 0, 1, 0xFF, 0x70, NEXT),
            new SingleOpCode(OpCode.CEE_LDOBJ, "ldobj", PopI, Push1, OpCodeType.InlineType, 0, 1, 0xFF, 0x71, NEXT),
            new SingleOpCode(OpCode.CEE_LDSTR, "ldstr", Pop0, PushRef, OpCodeType.InlineString, 0, 1, 0xFF, 0x72, NEXT),
            new SingleOpCode(OpCode.CEE_NEWOBJ, "newobj", VarPop, PushRef, OpCodeType.InlineMethod, 0, 1, 0xFF, 0x73, CALL),
            new SingleOpCode(OpCode.CEE_CASTCLASS, "castclass", PopRef, PushRef, OpCodeType.InlineType, 0, 1, 0xFF, 0x74, NEXT),
            new SingleOpCode(OpCode.CEE_ISINST, "isinst", PopRef, PushI, OpCodeType.InlineType, 0, 1, 0xFF, 0x75, NEXT),
            new SingleOpCode(OpCode.CEE_CONV_R_UN, "conv.r.un", Pop1, PushR8, OpCodeType.InlineNone, 0, 1, 0xFF, 0x76, NEXT),
            new SingleOpCode(OpCode.CEE_UNUSED58, "unused58", Pop0, Push0, OpCodeType.InlineNone, 0, 1, 0xFF, 0x77, NEXT),
            new SingleOpCode(OpCode.CEE_UNUSED1, "unused1", Pop0, Push0, OpCodeType.InlineNone, 0, 1, 0xFF, 0x78, NEXT),
            new SingleOpCode(OpCode.CEE_UNBOX, "unbox", PopRef, PushI, OpCodeType.InlineType, 0, 1, 0xFF, 0x79, NEXT),
            new SingleOpCode(OpCode.CEE_THROW, "throw", PopRef, Push0, OpCodeType.InlineNone, 0, 1, 0xFF, 0x7A, ERROR),
            new SingleOpCode(OpCode.CEE_LDFLD, "ldfld", PopRef, Push1, OpCodeType.InlineField, 0, 1, 0xFF, 0x7B, NEXT),
            new SingleOpCode(OpCode.CEE_LDFLDA, "ldflda", PopRef, PushI, OpCodeType.InlineField, 0, 1, 0xFF, 0x7C, NEXT),
            new SingleOpCode(OpCode.CEE_STFLD, "stfld", PopRef + Pop1, Push0, OpCodeType.InlineField, 0, 1, 0xFF, 0x7D, NEXT),
            new SingleOpCode(OpCode.CEE_LDSFLD, "ldsfld", Pop0, Push1, OpCodeType.InlineField, 0, 1, 0xFF, 0x7E, NEXT),
            new SingleOpCode(OpCode.CEE_LDSFLDA, "ldsflda", Pop0, PushI, OpCodeType.InlineField, 0, 1, 0xFF, 0x7F, NEXT),
            new SingleOpCode(OpCode.CEE_STSFLD, "stsfld", Pop1, Push0, OpCodeType.InlineField, 0, 1, 0xFF, 0x80, NEXT),
            new SingleOpCode(OpCode.CEE_STOBJ, "stobj", PopI + Pop1, Push0, OpCodeType.InlineType, 0, 1, 0xFF, 0x81, NEXT),
            new SingleOpCode(OpCode.CEE_CONV_OVF_I1_UN, "conv.ovf.i1.un", Pop1, PushI, OpCodeType.InlineNone, 0, 1, 0xFF, 0x82, NEXT),
            new SingleOpCode(OpCode.CEE_CONV_OVF_I2_UN, "conv.ovf.i2.un", Pop1, PushI, OpCodeType.InlineNone, 0, 1, 0xFF, 0x83, NEXT),
            new SingleOpCode(OpCode.CEE_CONV_OVF_I4_UN, "conv.ovf.i4.un", Pop1, PushI, OpCodeType.InlineNone, 0, 1, 0xFF, 0x84, NEXT),
            new SingleOpCode(OpCode.CEE_CONV_OVF_I8_UN, "conv.ovf.i8.un", Pop1, PushI8, OpCodeType.InlineNone, 0, 1, 0xFF, 0x85, NEXT),
            new SingleOpCode(OpCode.CEE_CONV_OVF_U1_UN, "conv.ovf.u1.un", Pop1, PushI, OpCodeType.InlineNone, 0, 1, 0xFF, 0x86, NEXT),
            new SingleOpCode(OpCode.CEE_CONV_OVF_U2_UN, "conv.ovf.u2.un", Pop1, PushI, OpCodeType.InlineNone, 0, 1, 0xFF, 0x87, NEXT),
            new SingleOpCode(OpCode.CEE_CONV_OVF_U4_UN, "conv.ovf.u4.un", Pop1, PushI, OpCodeType.InlineNone, 0, 1, 0xFF, 0x88, NEXT),
            new SingleOpCode(OpCode.CEE_CONV_OVF_U8_UN, "conv.ovf.u8.un", Pop1, PushI8, OpCodeType.InlineNone, 0, 1, 0xFF, 0x89, NEXT),
            new SingleOpCode(OpCode.CEE_CONV_OVF_I_UN, "conv.ovf.i.un", Pop1, PushI, OpCodeType.InlineNone, 0, 1, 0xFF, 0x8A, NEXT),
            new SingleOpCode(OpCode.CEE_CONV_OVF_U_UN, "conv.ovf.u.un", Pop1, PushI, OpCodeType.InlineNone, 0, 1, 0xFF, 0x8B, NEXT),
            new SingleOpCode(OpCode.CEE_BOX, "box", Pop1, PushRef, OpCodeType.InlineType, 0, 1, 0xFF, 0x8C, NEXT),
            new SingleOpCode(OpCode.CEE_NEWARR, "newarr", PopI, PushRef, OpCodeType.InlineType, 0, 1, 0xFF, 0x8D, NEXT),
            new SingleOpCode(OpCode.CEE_LDLEN, "ldlen", PopRef, PushI, OpCodeType.InlineNone, 0, 1, 0xFF, 0x8E, NEXT),
            new SingleOpCode(OpCode.CEE_LDELEMA, "ldelema", PopRef + PopI, PushI, OpCodeType.InlineType, 0, 1, 0xFF, 0x8F, NEXT),
            new SingleOpCode(OpCode.CEE_LDELEM_I1, "ldelem.i1", PopRef + PopI, PushI, OpCodeType.InlineNone, 0, 1, 0xFF, 0x90, NEXT),
            new SingleOpCode(OpCode.CEE_LDELEM_U1, "ldelem.u1", PopRef + PopI, PushI, OpCodeType.InlineNone, 0, 1, 0xFF, 0x91, NEXT),
            new SingleOpCode(OpCode.CEE_LDELEM_I2, "ldelem.i2", PopRef + PopI, PushI, OpCodeType.InlineNone, 0, 1, 0xFF, 0x92, NEXT),
            new SingleOpCode(OpCode.CEE_LDELEM_U2, "ldelem.u2", PopRef + PopI, PushI, OpCodeType.InlineNone, 0, 1, 0xFF, 0x93, NEXT),
            new SingleOpCode(OpCode.CEE_LDELEM_I4, "ldelem.i4", PopRef + PopI, PushI, OpCodeType.InlineNone, 0, 1, 0xFF, 0x94, NEXT),
            new SingleOpCode(OpCode.CEE_LDELEM_U4, "ldelem.u4", PopRef + PopI, PushI, OpCodeType.InlineNone, 0, 1, 0xFF, 0x95, NEXT),
            new SingleOpCode(OpCode.CEE_LDELEM_I8, "ldelem.i8", PopRef + PopI, PushI8, OpCodeType.InlineNone, 0, 1, 0xFF, 0x96, NEXT),
            new SingleOpCode(OpCode.CEE_LDELEM_I, "ldelem.i", PopRef + PopI, PushI, OpCodeType.InlineNone, 0, 1, 0xFF, 0x97, NEXT),
            new SingleOpCode(OpCode.CEE_LDELEM_R4, "ldelem.r4", PopRef + PopI, PushR4, OpCodeType.InlineNone, 0, 1, 0xFF, 0x98, NEXT),
            new SingleOpCode(OpCode.CEE_LDELEM_R8, "ldelem.r8", PopRef + PopI, PushR8, OpCodeType.InlineNone, 0, 1, 0xFF, 0x99, NEXT),
            new SingleOpCode(OpCode.CEE_LDELEM_REF, "ldelem.ref", PopRef + PopI, PushRef, OpCodeType.InlineNone, 0, 1, 0xFF, 0x9A, NEXT),
            new SingleOpCode(OpCode.CEE_STELEM_I, "stelem.i", PopRef + PopI + PopI, Push0, OpCodeType.InlineNone, 0, 1, 0xFF, 0x9B, NEXT),
            new SingleOpCode(OpCode.CEE_STELEM_I1, "stelem.i1", PopRef + PopI + PopI, Push0, OpCodeType.InlineNone, 0, 1, 0xFF, 0x9C, NEXT),
            new SingleOpCode(OpCode.CEE_STELEM_I2, "stelem.i2", PopRef + PopI + PopI, Push0, OpCodeType.InlineNone, 0, 1, 0xFF, 0x9D, NEXT),
            new SingleOpCode(OpCode.CEE_STELEM_I4, "stelem.i4", PopRef + PopI + PopI, Push0, OpCodeType.InlineNone, 0, 1, 0xFF, 0x9E, NEXT),
            new SingleOpCode(OpCode.CEE_STELEM_I8, "stelem.i8", PopRef + PopI + PopI8, Push0, OpCodeType.InlineNone, 0, 1, 0xFF, 0x9F, NEXT),
            new SingleOpCode(OpCode.CEE_STELEM_R4, "stelem.r4", PopRef + PopI + PopR4, Push0, OpCodeType.InlineNone, 0, 1, 0xFF, 0xA0, NEXT),
            new SingleOpCode(OpCode.CEE_STELEM_R8, "stelem.r8", PopRef + PopI + PopR8, Push0, OpCodeType.InlineNone, 0, 1, 0xFF, 0xA1, NEXT),
            new SingleOpCode(OpCode.CEE_STELEM_REF, "stelem.ref", PopRef + PopI + PopRef, Push0, OpCodeType.InlineNone, 0, 1, 0xFF, 0xA2, NEXT),
            new SingleOpCode(OpCode.CEE_LDELEM, "ldelem", PopRef + PopI, Push1, OpCodeType.InlineType, 0, 1, 0xFF, 0xA3, NEXT),
            new SingleOpCode(OpCode.CEE_STELEM, "stelem", PopRef + PopI + Pop1, Push0, OpCodeType.InlineType, 0, 1, 0xFF, 0xA4, NEXT),
            new SingleOpCode(OpCode.CEE_UNBOX_ANY, "unbox.any", PopRef, Push1, OpCodeType.InlineType, 0, 1, 0xFF, 0xA5, NEXT),
            new SingleOpCode(OpCode.CEE_UNUSED5, "unused5", Pop0, Push0, OpCodeType.InlineNone, 0, 1, 0xFF, 0xA6, NEXT),
            new SingleOpCode(OpCode.CEE_UNUSED6, "unused6", Pop0, Push0, OpCodeType.InlineNone, 0, 1, 0xFF, 0xA7, NEXT),
            new SingleOpCode(OpCode.CEE_UNUSED7, "unused7", Pop0, Push0, OpCodeType.InlineNone, 0, 1, 0xFF, 0xA8, NEXT),
            new SingleOpCode(OpCode.CEE_UNUSED8, "unused8", Pop0, Push0, OpCodeType.InlineNone, 0, 1, 0xFF, 0xA9, NEXT),
            new SingleOpCode(OpCode.CEE_UNUSED9, "unused9", Pop0, Push0, OpCodeType.InlineNone, 0, 1, 0xFF, 0xAA, NEXT),
            new SingleOpCode(OpCode.CEE_UNUSED10, "unused10", Pop0, Push0, OpCodeType.InlineNone, 0, 1, 0xFF, 0xAB, NEXT),
            new SingleOpCode(OpCode.CEE_UNUSED11, "unused11", Pop0, Push0, OpCodeType.InlineNone, 0, 1, 0xFF, 0xAC, NEXT),
            new SingleOpCode(OpCode.CEE_UNUSED12, "unused12", Pop0, Push0, OpCodeType.InlineNone, 0, 1, 0xFF, 0xAD, NEXT),
            new SingleOpCode(OpCode.CEE_UNUSED13, "unused13", Pop0, Push0, OpCodeType.InlineNone, 0, 1, 0xFF, 0xAE, NEXT),
            new SingleOpCode(OpCode.CEE_UNUSED14, "unused14", Pop0, Push0, OpCodeType.InlineNone, 0, 1, 0xFF, 0xAF, NEXT),
            new SingleOpCode(OpCode.CEE_UNUSED15, "unused15", Pop0, Push0, OpCodeType.InlineNone, 0, 1, 0xFF, 0xB0, NEXT),
            new SingleOpCode(OpCode.CEE_UNUSED16, "unused16", Pop0, Push0, OpCodeType.InlineNone, 0, 1, 0xFF, 0xB1, NEXT),
            new SingleOpCode(OpCode.CEE_UNUSED17, "unused17", Pop0, Push0, OpCodeType.InlineNone, 0, 1, 0xFF, 0xB2, NEXT),
            new SingleOpCode(OpCode.CEE_CONV_OVF_I1, "conv.ovf.i1", Pop1, PushI, OpCodeType.InlineNone, 0, 1, 0xFF, 0xB3, NEXT),
            new SingleOpCode(OpCode.CEE_CONV_OVF_U1, "conv.ovf.u1", Pop1, PushI, OpCodeType.InlineNone, 0, 1, 0xFF, 0xB4, NEXT),
            new SingleOpCode(OpCode.CEE_CONV_OVF_I2, "conv.ovf.i2", Pop1, PushI, OpCodeType.InlineNone, 0, 1, 0xFF, 0xB5, NEXT),
            new SingleOpCode(OpCode.CEE_CONV_OVF_U2, "conv.ovf.u2", Pop1, PushI, OpCodeType.InlineNone, 0, 1, 0xFF, 0xB6, NEXT),
            new SingleOpCode(OpCode.CEE_CONV_OVF_I4, "conv.ovf.i4", Pop1, PushI, OpCodeType.InlineNone, 0, 1, 0xFF, 0xB7, NEXT),
            new SingleOpCode(OpCode.CEE_CONV_OVF_U4, "conv.ovf.u4", Pop1, PushI, OpCodeType.InlineNone, 0, 1, 0xFF, 0xB8, NEXT),
            new SingleOpCode(OpCode.CEE_CONV_OVF_I8, "conv.ovf.i8", Pop1, PushI8, OpCodeType.InlineNone, 0, 1, 0xFF, 0xB9, NEXT),
            new SingleOpCode(OpCode.CEE_CONV_OVF_U8, "conv.ovf.u8", Pop1, PushI8, OpCodeType.InlineNone, 0, 1, 0xFF, 0xBA, NEXT),
            new SingleOpCode(OpCode.CEE_UNUSED50, "unused50", Pop0, Push0, OpCodeType.InlineNone, 0, 1, 0xFF, 0xBB, NEXT),
            new SingleOpCode(OpCode.CEE_UNUSED18, "unused18", Pop0, Push0, OpCodeType.InlineNone, 0, 1, 0xFF, 0xBC, NEXT),
            new SingleOpCode(OpCode.CEE_UNUSED19, "unused19", Pop0, Push0, OpCodeType.InlineNone, 0, 1, 0xFF, 0xBD, NEXT),
            new SingleOpCode(OpCode.CEE_UNUSED20, "unused20", Pop0, Push0, OpCodeType.InlineNone, 0, 1, 0xFF, 0xBE, NEXT),
            new SingleOpCode(OpCode.CEE_UNUSED21, "unused21", Pop0, Push0, OpCodeType.InlineNone, 0, 1, 0xFF, 0xBF, NEXT),
            new SingleOpCode(OpCode.CEE_UNUSED22, "unused22", Pop0, Push0, OpCodeType.InlineNone, 0, 1, 0xFF, 0xC0, NEXT),
            new SingleOpCode(OpCode.CEE_UNUSED23, "unused23", Pop0, Push0, OpCodeType.InlineNone, 0, 1, 0xFF, 0xC1, NEXT),
            new SingleOpCode(OpCode.CEE_REFANYVAL, "refanyval", Pop1, PushI, OpCodeType.InlineType, 0, 1, 0xFF, 0xC2, NEXT),
            new SingleOpCode(OpCode.CEE_CKFINITE, "ckfinite", Pop1, PushR8, OpCodeType.InlineNone, 0, 1, 0xFF, 0xC3, NEXT),
            new SingleOpCode(OpCode.CEE_UNUSED24, "unused24", Pop0, Push0, OpCodeType.InlineNone, 0, 1, 0xFF, 0xC4, NEXT),
            new SingleOpCode(OpCode.CEE_UNUSED25, "unused25", Pop0, Push0, OpCodeType.InlineNone, 0, 1, 0xFF, 0xC5, NEXT),
            new SingleOpCode(OpCode.CEE_MKREFANY, "mkrefany", PopI, Push1, OpCodeType.InlineType, 0, 1, 0xFF, 0xC6, NEXT),
            new SingleOpCode(OpCode.CEE_UNUSED59, "unused59", Pop0, Push0, OpCodeType.InlineNone, 0, 1, 0xFF, 0xC7, NEXT),
            new SingleOpCode(OpCode.CEE_UNUSED60, "unused60", Pop0, Push0, OpCodeType.InlineNone, 0, 1, 0xFF, 0xC8, NEXT),
            new SingleOpCode(OpCode.CEE_UNUSED61, "unused61", Pop0, Push0, OpCodeType.InlineNone, 0, 1, 0xFF, 0xC9, NEXT),
            new SingleOpCode(OpCode.CEE_UNUSED62, "unused62", Pop0, Push0, OpCodeType.InlineNone, 0, 1, 0xFF, 0xCA, NEXT),
            new SingleOpCode(OpCode.CEE_UNUSED63, "unused63", Pop0, Push0, OpCodeType.InlineNone, 0, 1, 0xFF, 0xCB, NEXT),
            new SingleOpCode(OpCode.CEE_UNUSED64, "unused64", Pop0, Push0, OpCodeType.InlineNone, 0, 1, 0xFF, 0xCC, NEXT),
            new SingleOpCode(OpCode.CEE_UNUSED65, "unused65", Pop0, Push0, OpCodeType.InlineNone, 0, 1, 0xFF, 0xCD, NEXT),
            new SingleOpCode(OpCode.CEE_UNUSED66, "unused66", Pop0, Push0, OpCodeType.InlineNone, 0, 1, 0xFF, 0xCE, NEXT),
            new SingleOpCode(OpCode.CEE_UNUSED67, "unused67", Pop0, Push0, OpCodeType.InlineNone, 0, 1, 0xFF, 0xCF, NEXT),
            new SingleOpCode(OpCode.CEE_LDTOKEN, "ldtoken", Pop0, PushI, OpCodeType.InlineTok, 0, 1, 0xFF, 0xD0, NEXT),
            new SingleOpCode(OpCode.CEE_CONV_U2, "conv.u2", Pop1, PushI, OpCodeType.InlineNone, 0, 1, 0xFF, 0xD1, NEXT),
            new SingleOpCode(OpCode.CEE_CONV_U1, "conv.u1", Pop1, PushI, OpCodeType.InlineNone, 0, 1, 0xFF, 0xD2, NEXT),
            new SingleOpCode(OpCode.CEE_CONV_I, "conv.i", Pop1, PushI, OpCodeType.InlineNone, 0, 1, 0xFF, 0xD3, NEXT),
            new SingleOpCode(OpCode.CEE_CONV_OVF_I, "conv.ovf.i", Pop1, PushI, OpCodeType.InlineNone, 0, 1, 0xFF, 0xD4, NEXT),
            new SingleOpCode(OpCode.CEE_CONV_OVF_U, "conv.ovf.u", Pop1, PushI, OpCodeType.InlineNone, 0, 1, 0xFF, 0xD5, NEXT),
            new SingleOpCode(OpCode.CEE_ADD_OVF, "add.ovf", Pop1 + Pop1, Push1, OpCodeType.InlineNone, 0, 1, 0xFF, 0xD6, NEXT),
            new SingleOpCode(OpCode.CEE_ADD_OVF_UN, "add.ovf.un", Pop1 + Pop1, Push1, OpCodeType.InlineNone, 0, 1, 0xFF, 0xD7, NEXT),
            new SingleOpCode(OpCode.CEE_MUL_OVF, "mul.ovf", Pop1 + Pop1, Push1, OpCodeType.InlineNone, 0, 1, 0xFF, 0xD8, NEXT),
            new SingleOpCode(OpCode.CEE_MUL_OVF_UN, "mul.ovf.un", Pop1 + Pop1, Push1, OpCodeType.InlineNone, 0, 1, 0xFF, 0xD9, NEXT),
            new SingleOpCode(OpCode.CEE_SUB_OVF, "sub.ovf", Pop1 + Pop1, Push1, OpCodeType.InlineNone, 0, 1, 0xFF, 0xDA, NEXT),
            new SingleOpCode(OpCode.CEE_SUB_OVF_UN, "sub.ovf.un", Pop1 + Pop1, Push1, OpCodeType.InlineNone, 0, 1, 0xFF, 0xDB, NEXT),
            new SingleOpCode(OpCode.CEE_ENDFINALLY, "endfinally", Pop0, Push0, OpCodeType.InlineNone, 0, 1, 0xFF, 0xDC, RETURN),
            new SingleOpCode(OpCode.CEE_LEAVE, "leave", Pop0, Push0, OpCodeType.InlineBrTarget, 0, 1, 0xFF, 0xDD, BRANCH),
            new SingleOpCode(OpCode.CEE_LEAVE_S, "leave.s", Pop0, Push0, OpCodeType.ShortInlineBrTarget, 0, 1, 0xFF, 0xDE, BRANCH),
            new SingleOpCode(OpCode.CEE_STIND_I, "stind.i", PopI + PopI, Push0, OpCodeType.InlineNone, 0, 1, 0xFF, 0xDF, NEXT),
            new SingleOpCode(OpCode.CEE_CONV_U, "conv.u", Pop1, PushI, OpCodeType.InlineNone, 0, 1, 0xFF, 0xE0, NEXT),
            new SingleOpCode(OpCode.CEE_UNUSED26, "unused26", Pop0, Push0, OpCodeType.InlineNone, 0, 1, 0xFF, 0xE1, NEXT),
            new SingleOpCode(OpCode.CEE_UNUSED27, "unused27", Pop0, Push0, OpCodeType.InlineNone, 0, 1, 0xFF, 0xE2, NEXT),
            new SingleOpCode(OpCode.CEE_UNUSED28, "unused28", Pop0, Push0, OpCodeType.InlineNone, 0, 1, 0xFF, 0xE3, NEXT),
            new SingleOpCode(OpCode.CEE_UNUSED29, "unused29", Pop0, Push0, OpCodeType.InlineNone, 0, 1, 0xFF, 0xE4, NEXT),
            new SingleOpCode(OpCode.CEE_UNUSED30, "unused30", Pop0, Push0, OpCodeType.InlineNone, 0, 1, 0xFF, 0xE5, NEXT),
            new SingleOpCode(OpCode.CEE_UNUSED31, "unused31", Pop0, Push0, OpCodeType.InlineNone, 0, 1, 0xFF, 0xE6, NEXT),
            new SingleOpCode(OpCode.CEE_UNUSED32, "unused32", Pop0, Push0, OpCodeType.InlineNone, 0, 1, 0xFF, 0xE7, NEXT),
            new SingleOpCode(OpCode.CEE_UNUSED33, "unused33", Pop0, Push0, OpCodeType.InlineNone, 0, 1, 0xFF, 0xE8, NEXT),
            new SingleOpCode(OpCode.CEE_UNUSED34, "unused34", Pop0, Push0, OpCodeType.InlineNone, 0, 1, 0xFF, 0xE9, NEXT),
            new SingleOpCode(OpCode.CEE_UNUSED35, "unused35", Pop0, Push0, OpCodeType.InlineNone, 0, 1, 0xFF, 0xEA, NEXT),
            new SingleOpCode(OpCode.CEE_UNUSED36, "unused36", Pop0, Push0, OpCodeType.InlineNone, 0, 1, 0xFF, 0xEB, NEXT),
            new SingleOpCode(OpCode.CEE_UNUSED37, "unused37", Pop0, Push0, OpCodeType.InlineNone, 0, 1, 0xFF, 0xEC, NEXT),
            new SingleOpCode(OpCode.CEE_UNUSED38, "unused38", Pop0, Push0, OpCodeType.InlineNone, 0, 1, 0xFF, 0xED, NEXT),
            new SingleOpCode(OpCode.CEE_UNUSED39, "unused39", Pop0, Push0, OpCodeType.InlineNone, 0, 1, 0xFF, 0xEE, NEXT),
            new SingleOpCode(OpCode.CEE_UNUSED40, "unused40", Pop0, Push0, OpCodeType.InlineNone, 0, 1, 0xFF, 0xEF, NEXT),
            new SingleOpCode(OpCode.CEE_UNUSED41, "unused41", Pop0, Push0, OpCodeType.InlineNone, 0, 1, 0xFF, 0xF0, NEXT),
            new SingleOpCode(OpCode.CEE_UNUSED42, "unused42", Pop0, Push0, OpCodeType.InlineNone, 0, 1, 0xFF, 0xF1, NEXT),
            new SingleOpCode(OpCode.CEE_UNUSED43, "unused43", Pop0, Push0, OpCodeType.InlineNone, 0, 1, 0xFF, 0xF2, NEXT),
            new SingleOpCode(OpCode.CEE_UNUSED44, "unused44", Pop0, Push0, OpCodeType.InlineNone, 0, 1, 0xFF, 0xF3, NEXT),
            new SingleOpCode(OpCode.CEE_UNUSED45, "unused45", Pop0, Push0, OpCodeType.InlineNone, 0, 1, 0xFF, 0xF4, NEXT),
            new SingleOpCode(OpCode.CEE_UNUSED46, "unused46", Pop0, Push0, OpCodeType.InlineNone, 0, 1, 0xFF, 0xF5, NEXT),
            new SingleOpCode(OpCode.CEE_UNUSED47, "unused47", Pop0, Push0, OpCodeType.InlineNone, 0, 1, 0xFF, 0xF6, NEXT),
            new SingleOpCode(OpCode.CEE_UNUSED48, "unused48", Pop0, Push0, OpCodeType.InlineNone, 0, 1, 0xFF, 0xF7, NEXT),
            new SingleOpCode(OpCode.CEE_PREFIX7, "prefix7", Pop0, Push0, OpCodeType.InlineNone, 0, 1, 0xFF, 0xF8, META),
            new SingleOpCode(OpCode.CEE_PREFIX6, "prefix6", Pop0, Push0, OpCodeType.InlineNone, 0, 1, 0xFF, 0xF9, META),
            new SingleOpCode(OpCode.CEE_PREFIX5, "prefix5", Pop0, Push0, OpCodeType.InlineNone, 0, 1, 0xFF, 0xFA, META),
            new SingleOpCode(OpCode.CEE_PREFIX4, "prefix4", Pop0, Push0, OpCodeType.InlineNone, 0, 1, 0xFF, 0xFB, META),
            new SingleOpCode(OpCode.CEE_PREFIX3, "prefix3", Pop0, Push0, OpCodeType.InlineNone, 0, 1, 0xFF, 0xFC, META),
            new SingleOpCode(OpCode.CEE_PREFIX2, "prefix2", Pop0, Push0, OpCodeType.InlineNone, 0, 1, 0xFF, 0xFD, META),
            new SingleOpCode(OpCode.CEE_PREFIX1, "prefix1", Pop0, Push0, OpCodeType.InlineNone, 0, 1, 0xFF, 0xFE, META),
            new SingleOpCode(OpCode.CEE_PREFIXREF, "prefixref", Pop0, Push0, OpCodeType.InlineNone, 0, 1, 0xFF, 0xFF, META),
            new SingleOpCode(OpCode.CEE_ARGLIST, "arglist", Pop0, PushI, OpCodeType.InlineNone, 0, 2, 0xFE, 0x00, NEXT),
            new SingleOpCode(OpCode.CEE_CEQ, "ceq", Pop1 + Pop1, PushI, OpCodeType.InlineNone, 0, 2, 0xFE, 0x01, NEXT),
            new SingleOpCode(OpCode.CEE_CGT, "cgt", Pop1 + Pop1, PushI, OpCodeType.InlineNone, 0, 2, 0xFE, 0x02, NEXT),
            new SingleOpCode(OpCode.CEE_CGT_UN, "cgt.un", Pop1 + Pop1, PushI, OpCodeType.InlineNone, 0, 2, 0xFE, 0x03, NEXT),
            new SingleOpCode(OpCode.CEE_CLT, "clt", Pop1 + Pop1, PushI, OpCodeType.InlineNone, 0, 2, 0xFE, 0x04, NEXT),
            new SingleOpCode(OpCode.CEE_CLT_UN, "clt.un", Pop1 + Pop1, PushI, OpCodeType.InlineNone, 0, 2, 0xFE, 0x05, NEXT),
            new SingleOpCode(OpCode.CEE_LDFTN, "ldftn", Pop0, PushI, OpCodeType.InlineMethod, 0, 2, 0xFE, 0x06, NEXT),
            new SingleOpCode(OpCode.CEE_LDVIRTFTN, "ldvirtftn", PopRef, PushI, OpCodeType.InlineMethod, 0, 2, 0xFE, 0x07, NEXT),
            new SingleOpCode(OpCode.CEE_UNUSED56, "unused56", Pop0, Push0, OpCodeType.InlineNone, 0, 2, 0xFE, 0x08, NEXT),
            new SingleOpCode(OpCode.CEE_LDARG, "ldarg", Pop0, Push1, OpCodeType.InlineVar, 0, 2, 0xFE, 0x09, NEXT),
            new SingleOpCode(OpCode.CEE_LDARGA, "ldarga", Pop0, PushI, OpCodeType.InlineVar, 0, 2, 0xFE, 0x0A, NEXT),
            new SingleOpCode(OpCode.CEE_STARG, "starg", Pop1, Push0, OpCodeType.InlineVar, 0, 2, 0xFE, 0x0B, NEXT),
            new SingleOpCode(OpCode.CEE_LDLOC, "ldloc", Pop0, Push1, OpCodeType.InlineVar, 0, 2, 0xFE, 0x0C, NEXT),
            new SingleOpCode(OpCode.CEE_LDLOCA, "ldloca", Pop0, PushI, OpCodeType.InlineVar, 0, 2, 0xFE, 0x0D, NEXT),
            new SingleOpCode(OpCode.CEE_STLOC, "stloc", Pop1, Push0, OpCodeType.InlineVar, 0, 2, 0xFE, 0x0E, NEXT),
            new SingleOpCode(OpCode.CEE_LOCALLOC, "localloc", PopI, PushI, OpCodeType.InlineNone, 0, 2, 0xFE, 0x0F, NEXT),
            new SingleOpCode(OpCode.CEE_UNUSED57, "unused57", Pop0, Push0, OpCodeType.InlineNone, 0, 2, 0xFE, 0x10, NEXT),
            new SingleOpCode(OpCode.CEE_ENDFILTER, "endfilter", PopI, Push0, OpCodeType.InlineNone, 0, 2, 0xFE, 0x11, RETURN),
            new SingleOpCode(OpCode.CEE_UNALIGNED_, "unaligned.", Pop0, Push0, OpCodeType.ShortInlineI, 0, 2, 0xFE, 0x12, META),
            new SingleOpCode(OpCode.CEE_VOLATILE_, "volatile.", Pop0, Push0, OpCodeType.InlineNone, 0, 2, 0xFE, 0x13, META),
            new SingleOpCode(OpCode.CEE_TAIL_, "tail.", Pop0, Push0, OpCodeType.InlineNone, 0, 2, 0xFE, 0x14, META),
            new SingleOpCode(OpCode.CEE_INITOBJ, "initobj", PopI, Push0, OpCodeType.InlineType, 0, 2, 0xFE, 0x15, NEXT),
            new SingleOpCode(OpCode.CEE_CONSTRAINED_, "constrained.", Pop0, Push0, OpCodeType.InlineType, 0, 2, 0xFE, 0x16, META),
            new SingleOpCode(OpCode.CEE_CPBLK, "cpblk", PopI + PopI + PopI, Push0, OpCodeType.InlineNone, 0, 2, 0xFE, 0x17, NEXT),
            new SingleOpCode(OpCode.CEE_INITBLK, "initblk", PopI + PopI + PopI, Push0, OpCodeType.InlineNone, 0, 2, 0xFE, 0x18, NEXT),
            new SingleOpCode(OpCode.CEE_NO_, "no.", Pop0, Push0, OpCodeType.ShortInlineI, 0, 2, 0xFE, 0x19, NEXT),
            new SingleOpCode(OpCode.CEE_RETHROW, "rethrow", Pop0, Push0, OpCodeType.InlineNone, 0, 2, 0xFE, 0x1A, ERROR),
            new SingleOpCode(OpCode.CEE_UNUSED, "unused", Pop0, Push0, OpCodeType.InlineNone, 0, 2, 0xFE, 0x1B, NEXT),
            new SingleOpCode(OpCode.CEE_SIZEOF, "sizeof", Pop0, PushI, OpCodeType.InlineType, 0, 2, 0xFE, 0x1C, NEXT),
            new SingleOpCode(OpCode.CEE_REFANYTYPE, "refanytype", Pop1, PushI, OpCodeType.InlineNone, 0, 2, 0xFE, 0x1D, NEXT),
            new SingleOpCode(OpCode.CEE_READONLY_, "readonly.", Pop0, Push0, OpCodeType.InlineNone, 0, 2, 0xFE, 0x1E, META),
            new SingleOpCode(OpCode.CEE_UNUSED53, "unused53", Pop0, Push0, OpCodeType.InlineNone, 0, 2, 0xFE, 0x1F, NEXT),
            new SingleOpCode(OpCode.CEE_UNUSED54, "unused54", Pop0, Push0, OpCodeType.InlineNone, 0, 2, 0xFE, 0x20, NEXT),
            new SingleOpCode(OpCode.CEE_UNUSED55, "unused55", Pop0, Push0, OpCodeType.InlineNone, 0, 2, 0xFE, 0x21, NEXT),
            new SingleOpCode(OpCode.CEE_UNUSED70, "unused70", Pop0, Push0, OpCodeType.InlineNone, 0, 2, 0xFE, 0x22, NEXT),
            new SingleOpCode(OpCode.CEE_ILLEGAL, "illegal", Pop0, Push0, OpCodeType.InlineNone, 0, 2, 0x00, 0x00, META),
            new SingleOpCode(OpCode.CEE_ENDMAC, "endmac", Pop0, Push0, OpCodeType.InlineNone, 0, 2, 0x00, 0x00, META),
            new SingleOpCode(OpCode.CEE_MONO_ICALL, "mono_icall", VarPop, VarPush, OpCodeType.InlineI, 0, 2, 0xF0, 0x00, NEXT),
            new SingleOpCode(OpCode.CEE_MONO_OBJADDR, "mono_objaddr", Pop1, PushI, OpCodeType.InlineNone, 0, 2, 0xF0, 0x01, NEXT),
            new SingleOpCode(OpCode.CEE_MONO_LDPTR, "mono_ldptr", Pop0, PushI, OpCodeType.InlineI, 0, 2, 0xF0, 0x02, NEXT),
            new SingleOpCode(OpCode.CEE_MONO_VTADDR, "mono_vtaddr", Pop1, PushI, OpCodeType.InlineNone, 0, 2, 0xF0, 0x03, NEXT),
            new SingleOpCode(OpCode.CEE_MONO_NEWOBJ, "mono_newobj", Pop0, PushRef, OpCodeType.InlineType, 0, 2, 0xF0, 0x04, NEXT),
            new SingleOpCode(OpCode.CEE_MONO_RETOBJ, "mono_retobj", PopI, Push0, OpCodeType.InlineType, 0, 2, 0xF0, 0x05, RETURN),
            new SingleOpCode(OpCode.CEE_MONO_LDNATIVEOBJ, "mono_ldnativeobj", PopI, Push1, OpCodeType.InlineType, 0, 2, 0xF0, 0x06, RETURN),
            new SingleOpCode(OpCode.CEE_MONO_CISINST, "mono_cisinst", PopRef, Push1, OpCodeType.InlineType, 0, 2, 0xF0, 0x07, NEXT),
            new SingleOpCode(OpCode.CEE_MONO_CCASTCLASS, "mono_ccastclass", PopRef, Push1, OpCodeType.InlineType, 0, 2, 0xF0, 0x08, NEXT),
            new SingleOpCode(OpCode.CEE_MONO_SAVE_LMF, "mono_save_lmf", Pop0, Push0, OpCodeType.InlineNone, 0, 2, 0xF0, 0x09, NEXT),
            new SingleOpCode(OpCode.CEE_MONO_RESTORE_LMF, "mono_restore_lmf", Pop0, Push0, OpCodeType.InlineNone, 0, 2, 0xF0, 0x0A, NEXT),
            new SingleOpCode(OpCode.CEE_MONO_CLASSCONST, "mono_classconst", Pop0, PushI, OpCodeType.InlineI, 0, 2, 0xF0, 0x0B, NEXT),
            new SingleOpCode(OpCode.CEE_MONO_NOT_TAKEN, "mono_not_taken", Pop0, Push0, OpCodeType.InlineNone, 0, 2, 0xF0, 0x0C, NEXT),
            new SingleOpCode(OpCode.CEE_MONO_TLS, "mono_tls", Pop0, PushI, OpCodeType.InlineI, 0, 2, 0xF0, 0x0D, NEXT),
            new SingleOpCode(OpCode.CEE_MONO_ICALL_ADDR, "mono_icall_addr", Pop0, PushI, OpCodeType.InlineI, 0, 2, 0xF0, 0x0E, NEXT),
            new SingleOpCode(OpCode.CEE_MONO_DYN_CALL, "mono_dyn_call", Pop0, PushI, OpCodeType.InlineNone, 0, 2, 0xF0, 0x0F, NEXT),
            new SingleOpCode(OpCode.CEE_MONO_MEMORY_BARRIER, "mono_memory_barrier", Pop0, Push0, OpCodeType.InlineI, 0, 2, 0xF0, 0x10, NEXT),
            new SingleOpCode(OpCode.CEE_UNUSED71, "unused71", Pop0, Push0, OpCodeType.InlineNone, 0, 2, 0xF0, 0x11, NEXT),
            new SingleOpCode(OpCode.CEE_UNUSED72, "unused72", Pop0, Push0, OpCodeType.InlineNone, 0, 2, 0xF0, 0x12, NEXT),
            new SingleOpCode(OpCode.CEE_MONO_JIT_ICALL_ADDR, "mono_jit_icall_addr", Pop0, PushI, OpCodeType.InlineI, 0, 2, 0xF0, 0x13, NEXT),
            new SingleOpCode(OpCode.CEE_MONO_LDPTR_INT_REQ_FLAG, "mono_ldptr_int_req_flag", Pop0, PushI, OpCodeType.InlineNone, 0, 2, 0xF0, 0x14, NEXT),
            new SingleOpCode(OpCode.CEE_MONO_LDPTR_CARD_TABLE, "mono_ldptr_card_table", Pop0, PushI, OpCodeType.InlineNone, 0, 2, 0xF0, 0x15, NEXT),
            new SingleOpCode(OpCode.CEE_MONO_LDPTR_NURSERY_START, "mono_ldptr_nursery_start", Pop0, PushI, OpCodeType.InlineNone, 0, 2, 0xF0, 0x16, NEXT),
            new SingleOpCode(OpCode.CEE_MONO_LDPTR_NURSERY_BITS, "mono_ldptr_nursery_bits", Pop0, PushI, OpCodeType.InlineNone, 0, 2, 0xF0, 0x17, NEXT),
            new SingleOpCode(OpCode.CEE_MONO_CALLI_EXTRA_ARG, "mono_calli_extra_arg", VarPop, VarPush, OpCodeType.InlineSig, 0, 2, 0xF0, 0x18, CALL),
            new SingleOpCode(OpCode.CEE_MONO_LDDOMAIN, "mono_lddomain", Pop0, PushI, OpCodeType.InlineNone, 0, 2, 0xF0, 0x19, NEXT),
            new SingleOpCode(OpCode.CEE_MONO_ATOMIC_STORE_I4, "mono_atomic_store_i4", PopI + PopI, Push0, OpCodeType.InlineI, 0, 2, 0xF0, 0x1A, NEXT),
            new SingleOpCode(OpCode.CEE_MONO_SAVE_LAST_ERROR, "mono_save_last_error", Pop0, Push0, OpCodeType.InlineNone, 0, 2, 0xF0, 0x1B, NEXT),
            new SingleOpCode(OpCode.CEE_MONO_GET_RGCTX_ARG, "mono_get_rgctx_arg", Pop0, PushI, OpCodeType.InlineNone, 0, 2, 0xF0, 0x1C, NEXT),
            new SingleOpCode(OpCode.CEE_MONO_LDPTR_PROFILER_ALLOCATION_COUNT, "mono_ldptr_profiler_allocation_count", Pop0, PushI, OpCodeType.InlineNone, 0, 2, 0xF0, 0x1D, NEXT),
            new SingleOpCode(OpCode.CEE_MONO_LD_DELEGATE_METHOD_PTR, "mono_ld_delegate_method_ptr", Pop1, PushI, OpCodeType.InlineNone, 0, 2, 0xF0, 0x1E, NEXT),
            new SingleOpCode(OpCode.CEE_MONO_RETHROW, "mono_rethrow", PopRef, Push0, OpCodeType.InlineNone, 0, 2, 0xF0, 0x1F, ERROR),
            new SingleOpCode(OpCode.CEE_MONO_GET_SP, "mono_get_sp", Pop0, PushI, OpCodeType.InlineNone, 0, 2, 0xF0, 0x20, NEXT)
        };

        internal struct SingleOpCode
        {
            public SingleOpCode(OpCode opCode, string name, int push, int pop, OpCodeType type, int implicitiarg, int resultcount, byte code1, byte code2, int prefetch)
            {
                OpCode = opCode;
                Name = name;
                Type = type;
            }

            public OpCode OpCode { get; }
            public string Name { get; }
            public OpCodeType Type { get; }
        }
    }
}
