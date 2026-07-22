using Disarm;
using Microsoft.Extensions.Logging;

namespace Il2CppInterop.Common.XrefScans;

internal static class XrefScanUtilFinder
{
    public static IntPtr FindLastRcxReadAddressBeforeCallTo(IntPtr codeStart, IntPtr callTarget)
    {
        var decoder = XrefScanner.DecoderForAddress(codeStart);
        var lastRcxRead = IntPtr.Zero;

        foreach (Arm64Instruction instruction in decoder)
        {
            if (instruction.MnemonicCategory.HasFlag(Arm64MnemonicCategory.Return))
                return IntPtr.Zero;

            if (HasGroup(instruction.Mnemonic, "AArch64_GRP_JUMP"))
                continue;

            if (HasGroup(instruction.Mnemonic, "AArch64_GRP_CALL"))
            {
                var target = ExtractTargetAddress(instruction);
                if ((IntPtr)target == callTarget)
                    return lastRcxRead;
            }

            if (instruction.MnemonicCategory.HasFlag(Arm64MnemonicCategory.Move))
            {
                // seemingly unneeded?
                if (instruction.Op0Kind == Arm64OperandKind.Register && instruction.Op1Kind == Arm64OperandKind.ImmediatePcRelative)
                {
                    var target = (long)instruction.Address + instruction.Op1Imm;
                    lastRcxRead = (IntPtr)target;
                }
            }
        }

        return IntPtr.Zero;
    }

    public static IntPtr FindByteWriteTargetRightAfterCallTo(IntPtr codeStart, IntPtr callTarget)
    {
        var decoder = XrefScanner.DecoderForAddress(codeStart);
        var seenCall = false;

        foreach (Arm64Instruction instruction in decoder)
        {
            if (instruction.MnemonicCategory.HasFlag(Arm64MnemonicCategory.Return))
                return IntPtr.Zero;

            if (HasGroup(instruction.Mnemonic, "AArch64_GRP_JUMP"))
                continue;

            if (HasGroup(instruction.Mnemonic, "AArch64_GRP_CALL"))
            {
                var target = ExtractTargetAddress(instruction);
                if ((IntPtr)target == callTarget)
                {
                    seenCall = true;
                    continue;
                }
            }

            if (instruction.MnemonicCategory.HasFlag(Arm64MnemonicCategory.Move) && seenCall)
            {
                // seemingly unneeded?
                if (instruction.Op0Kind == Arm64OperandKind.Register && instruction.Op1Kind == Arm64OperandKind.ImmediatePcRelative)
                {
                    var target = (long)instruction.Address + instruction.Op1Imm;
                    return (IntPtr)target;
                }
            }
        }

        return IntPtr.Zero;
    }

    public static ulong ExtractTargetAddress(in Arm64Instruction instruction)
    {
        // Only direct branches have a PC-relative immediate that can be resolved
        // without knowing the current register state. BR/BLR use a register operand;
        // treating their zero-valued Imm field as a relative address makes the
        // instruction incorrectly point to itself.
        return instruction switch
        {
            { Op0Kind: Arm64OperandKind.ImmediatePcRelative } =>
                (ulong)((long)instruction.Address + instruction.Op0Imm),
            { Op1Kind: Arm64OperandKind.ImmediatePcRelative } =>
                (ulong)((long)instruction.Address + instruction.Op1Imm),
            { Op2Kind: Arm64OperandKind.ImmediatePcRelative } =>
                (ulong)((long)instruction.Address + instruction.Op2Imm),
            { Op3Kind: Arm64OperandKind.ImmediatePcRelative } =>
                (ulong)((long)instruction.Address + instruction.Op3Imm),
            _ => 0,
        };
    }

    // group markings stolen from capstone
    public static bool HasGroup(Arm64Mnemonic mnemonic, string group)
    {
        switch (mnemonic)
        {
            case Arm64Mnemonic.B:
                {
                    if (group == "AArch64_GRP_JUMP")
                        return true;
                    if (group == "AArch64_GRP_BRANCH_RELATIVE")
                        return true;
                    return false;
                }
            case Arm64Mnemonic.BC:
                {
                    if (group == "AArch64_GRP_JUMP")
                        return true;
                    if (group == "AArch64_GRP_BRANCH_RELATIVE")
                        return true;
                    return false;
                }
            case Arm64Mnemonic.BL:
                {
                    if (group == "AArch64_GRP_CALL")
                        return true;
                    if (group == "AArch64_GRP_BRANCH_RELATIVE")
                        return true;
                    return false;
                }
            case Arm64Mnemonic.BLR:
                {
                    if (group == "AArch64_GRP_CALL")
                        return true;
                    return false;
                }
            case Arm64Mnemonic.BR:
                {
                    if (group == "AArch64_GRP_JUMP")
                        return true;
                    return false;
                }
            case Arm64Mnemonic.CBNZ:
                {
                    if (group == "AArch64_GRP_JUMP")
                        return true;
                    if (group == "AArch64_GRP_BRANCH_RELATIVE")
                        return true;
                    return false;
                }
            case Arm64Mnemonic.CBZ:
                {
                    if (group == "AArch64_GRP_JUMP")
                        return true;
                    if (group == "AArch64_GRP_BRANCH_RELATIVE")
                        return true;
                    return false;
                }
            case Arm64Mnemonic.TBNZ:
                {
                    if (group == "AArch64_GRP_JUMP")
                        return true;
                    if (group == "AArch64_GRP_BRANCH_RELATIVE")
                        return true;
                    return false;
                }
            case Arm64Mnemonic.TBZ:
                {
                    if (group == "AArch64_GRP_JUMP")
                        return true;
                    if (group == "AArch64_GRP_BRANCH_RELATIVE")
                        return true;
                    return false;
                }
            default: return false;
        }
    }
}
