using System.Runtime.InteropServices;

namespace Il2CppInterop.Runtime;

internal static partial class FusionInterop
{
    private const string LIBRARY_NAME = "fusion";

    [LibraryImport(LIBRARY_NAME, StringMarshalling = StringMarshalling.Utf8)]
    public static unsafe partial string get_il2cpp_api(string name);
}
