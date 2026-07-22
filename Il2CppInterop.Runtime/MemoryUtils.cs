using System.Diagnostics;
using System.Linq;
using Il2CppInterop.Common.XrefScans;

namespace Il2CppInterop.Runtime;

internal class MemoryUtils
{
    public static nint FindSignatureInModule(ProcessModule module, SignatureDefinition sigDef)
    {
        var ptr = FindSignatureInBlock(
            module.BaseAddress,
            module.ModuleMemorySize,
            sigDef.pattern,
            sigDef.mask,
            sigDef.offset
        );
        if (ptr != 0 && sigDef.xref)
            ptr = XrefScannerLowLevel.JumpTargets(ptr).FirstOrDefault();
        return ptr;
    }

    public static nint FindSignatureInBlock(nint block, long blockSize, string pattern, string mask, long sigOffset = 0)
    {
        return FindSignatureInBlock(block, blockSize, pattern.ToCharArray(), mask.ToCharArray(), sigOffset);
    }

    public static unsafe nint FindSignatureInBlock(nint block, long blockSize, char[] pattern, char[] mask,
        long sigOffset = 0)
    {
        if (block == nint.Zero || mask.Length == 0 || pattern.Length < mask.Length || blockSize < mask.Length)
            return 0;

        // The inner loop reads mask.Length bytes, so stop at the final complete
        // candidate instead of reading past the mapped block boundary.
        for (long address = 0; address <= blockSize - mask.Length; address++)
        {
            var found = true;
            for (uint offset = 0; offset < mask.Length; offset++)
                if (*(byte*)(address + block + offset) != (byte)pattern[offset] && mask[offset] != '?')
                {
                    found = false;
                    break;
                }

            if (found)
                return (nint)(address + block + sigOffset);
        }

        return 0;
    }

    public struct SignatureDefinition
    {
        public string pattern;
        public string mask;
        public int offset;
        public bool xref;
    }
}
