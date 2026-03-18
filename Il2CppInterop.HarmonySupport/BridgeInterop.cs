using System.Reflection;
using MonoMod.Utils;

namespace Il2CppInterop.HarmonySupport;

public static class BridgeInterop
{
    public static MethodInfo GetReturnBufferMethodInfo { get; } =
        typeof(BridgeInterop).GetMethod(nameof(GetReturnBuffer), BindingFlags.Public | BindingFlags.Static)!;

    private static nint s_getReturnBuffer;
    private static nint s_setReturnBuffer;
    private static nint s_returnBufferBridge;

    public static void Initialize(string libraryPath)
    {
        // TODO: call initialize somewhere
        if (s_getReturnBuffer != nint.Zero) return;

        var handle = DynDll.OpenLibrary(libraryPath);
        try
        {
            s_getReturnBuffer = handle.GetExport(nameof(GetReturnBuffer));
            s_setReturnBuffer =  handle.GetExport(nameof(SetReturnBuffer));
            s_returnBufferBridge = handle.GetExport("ReturnBufferBridge");

            Helpers.Assert(s_getReturnBuffer != nint.Zero);
        }
        catch
        {
            DynDll.CloseLibrary(handle);
            throw;
        }
    }

    public static nint GetPointerToReturnBufferBridge() => s_returnBufferBridge;

    public static unsafe nint GetReturnBuffer()
    {
        return ((delegate* unmanaged[Cdecl]<nint>)s_getReturnBuffer)();
    }

    public static unsafe void SetReturnBuffer(nint value)
    {
        ((delegate* unmanaged[Cdecl]<nint, void>)s_setReturnBuffer)(value);
    }
}
