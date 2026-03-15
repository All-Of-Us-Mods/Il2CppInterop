using System;
using Il2CppInterop.Common;
using Il2CppInterop.Runtime.Startup;

namespace Il2CppInterop.Runtime.Injection;

public interface IDetour : IDisposable
{
    nint Target { get; }
    nint Detour { get; }
    nint OriginalTrampoline { get; }

    void Apply();
    T GenerateTrampoline<T>() where T : Delegate;
}

public interface IDetourProvider
{
    IDetour Create<TDelegate>(nint original, TDelegate target) where TDelegate : Delegate;

    IDetour Create<TDelegate>(nint original, TDelegate target, bool unityFunction) where TDelegate : Delegate
    {
        return Create(original, target);
    }
}

internal static class Detour
{
    public static IDetour Apply<T>(nint original, T target, out T trampoline) where T : Delegate
    {
        var unityFunction = Il2CppInteropUtils.TryGetUnityFunctionFlagForGeneratedMethod(target.Method, out var isUnityFunction) &&
                            isUnityFunction;

        var detour = Il2CppInteropRuntime.Instance.DetourProvider.Create(original, target, unityFunction);
        trampoline = detour.GenerateTrampoline<T>();
        detour.Apply();
        return detour;
    }
}
