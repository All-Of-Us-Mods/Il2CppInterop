using System.Runtime.InteropServices;

namespace Il2CppInterop.Runtime;

internal static partial class FusionInterop
{
    private const string LIBRARY_NAME = "fusion";

    [LibraryImport(LIBRARY_NAME, StringMarshalling = StringMarshalling.Utf8)]
    public static unsafe partial nint get_il2cpp_api(string name);

    public static string GetIl2CppApi(string name)
    {
        var ptr = get_il2cpp_api(name);
        if (ptr == nint.Zero) return name;

        return Marshal.PtrToStringUTF8(ptr) ?? name;
    }
}
