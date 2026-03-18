using System.Reflection;
using MonoMod;
using MonoMod.Utils;

namespace Il2CppInterop.HarmonySupport;

public static class BridgeInterop
{
    public static MethodInfo GetReturnBufferMethodInfo { get; } =
        typeof(BridgeInterop).GetMethod(nameof(GetReturnBuffer), BindingFlags.Public | BindingFlags.Static)!;

    private static string s_libraryFname;
    private static nint s_libraryHandle;
    private static nint s_getReturnBuffer;
    private static nint s_setReturnBuffer;
    private static nint s_returnBufferBridge;

    public static string LibraryPath => s_libraryFname;
    public static nint ReturnBufferBridgeFn => s_returnBufferBridge;
    public static nint SetReturnBufferFn => s_setReturnBuffer;

    public static void Initialize()
    {
        if (s_libraryHandle != nint.Zero) return;

        using (var embedded = Assembly.GetExecutingAssembly().GetManifestResourceStream("bridge_helper_arm64_linux.so"))
        {
            // copy to temp file
            Helpers.Assert(embedded is not null);
            Switches.TryGetSwitchValue(Switches.HelperDropPath,  out var dropPath);

            var dropDir = dropPath is string dp ? Path.GetFullPath(dp) : Path.GetTempPath();
            _ = Directory.CreateDirectory(dropDir);

            var tempFile = Path.Combine(dropDir, "bridge_helper_arm64_linux.so");
            using var output = File.Create(tempFile);
            embedded.CopyTo(output);
            s_libraryFname = tempFile;
        }

        s_libraryHandle = DynDll.OpenLibrary(s_libraryFname);
        try
        {
            s_getReturnBuffer = s_libraryHandle.GetExport(nameof(GetReturnBuffer));
            s_setReturnBuffer =  s_libraryHandle.GetExport(nameof(SetReturnBuffer));
            s_returnBufferBridge = s_libraryHandle.GetExport("ReturnBufferBridge");

            Helpers.Assert(s_getReturnBuffer != nint.Zero);
        }
        catch
        {
            DynDll.CloseLibrary(s_libraryHandle);
            throw;
        }
    }

    public static unsafe nint GetReturnBuffer()
    {
        return ((delegate* unmanaged[Cdecl]<nint>)s_getReturnBuffer)();
    }

    public static unsafe void SetReturnBuffer(nint value)
    {
        ((delegate* unmanaged[Cdecl]<nint, void>)s_setReturnBuffer)(value);
    }
}
