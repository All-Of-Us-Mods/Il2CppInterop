using System.Reflection;
using MonoMod;
using MonoMod.Utils;

namespace Il2CppInterop.HarmonySupport;

public static class BridgeInterop
{
    public static MethodInfo GetReturnBufferMethodInfo { get; } =
        typeof(BridgeInterop).GetMethod(nameof(GetReturnBuffer), BindingFlags.Public | BindingFlags.Static)!;

    private static readonly object s_initializeLock = new();
    private static string s_libraryFname;
    private static nint s_libraryHandle;
    private static nint s_getReturnBuffer;
    private static nint s_setReturnBuffer;
    private static nint s_returnBufferBridge;

    public static string LibraryPath => s_libraryFname;
    public static nint ReturnBufferBridgeFn => s_returnBufferBridge;
    public static nint SetReturnBufferFn => s_setReturnBuffer;
    public static nint GetReturnBufferFn => s_getReturnBuffer;

    public static void Initialize()
    {
        lock (s_initializeLock)
        {
            if (s_libraryHandle != nint.Zero) return;

            string libraryFname;
            nint libraryHandle = nint.Zero;
            try
            {
                using (var embedded = Assembly.GetExecutingAssembly()
                           .GetManifestResourceStream("bridge_helper_arm64_linux.so"))
                {
                    if (embedded is null)
                        throw new InvalidOperationException("The ARM64 return-buffer bridge resource is missing");

                    Switches.TryGetSwitchValue(Switches.HelperDropPath, out var dropPath);

                    var dropDir = dropPath is string dp ? Path.GetFullPath(dp) : Path.GetTempPath();
                    _ = Directory.CreateDirectory(dropDir);

                    // Multiple Android processes can start concurrently. A process-specific
                    // file avoids one process truncating the helper while another loads it.
                    libraryFname = Path.Combine(dropDir,
                        $"bridge_helper_arm64_linux_{Environment.ProcessId}.so");
                    using var output = File.Create(libraryFname);
                    embedded.CopyTo(output);
                }

                libraryHandle = DynDll.OpenLibrary(libraryFname);
                var getReturnBuffer = libraryHandle.GetExport(nameof(GetReturnBuffer));
                var setReturnBuffer = libraryHandle.GetExport(nameof(SetReturnBuffer));
                var returnBufferBridge = libraryHandle.GetExport("ReturnBufferBridge");

                if (getReturnBuffer == nint.Zero || setReturnBuffer == nint.Zero ||
                    returnBufferBridge == nint.Zero)
                    throw new EntryPointNotFoundException("The ARM64 return-buffer bridge is missing required exports");

                // Publish initialized state only after every operation succeeds.
                s_libraryFname = libraryFname;
                s_getReturnBuffer = getReturnBuffer;
                s_setReturnBuffer = setReturnBuffer;
                s_returnBufferBridge = returnBufferBridge;
                s_libraryHandle = libraryHandle;
            }
            catch
            {
                if (libraryHandle != nint.Zero)
                    DynDll.CloseLibrary(libraryHandle);

                throw;
            }
        }
    }

    public static unsafe nint GetReturnBuffer()
    {
        if (s_getReturnBuffer == nint.Zero)
            throw new InvalidOperationException("BridgeInterop.Initialize must succeed before reading a return buffer");

        return ((delegate* unmanaged[Cdecl]<nint>)s_getReturnBuffer)();
    }

    public static unsafe void SetReturnBuffer(nint value)
    {
        if (s_setReturnBuffer == nint.Zero)
            throw new InvalidOperationException("BridgeInterop.Initialize must succeed before writing a return buffer");

        ((delegate* unmanaged[Cdecl]<nint, void>)s_setReturnBuffer)(value);
    }
}
