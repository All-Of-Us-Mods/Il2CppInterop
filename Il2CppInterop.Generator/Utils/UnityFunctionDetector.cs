using AsmResolver.DotNet;

namespace Il2CppInterop.Generator.Utils;

public static class UnityFunctionDetector
{
    private static readonly HashSet<string> s_unityMessages = new(StringComparer.Ordinal)
    {
        "Awake",
        "Start",
        "Update",
        "FixedUpdate",
        "LateUpdate",
        "OnEnable",
        "OnDisable",
        "OnDestroy",
        "OnTriggerEnter",
        "OnTriggerExit",
        "OnTriggerStay",
        "OnCollisionEnter",
        "OnCollisionExit",
        "OnCollisionStay",
        "OnApplicationQuit",
        "OnApplicationPause",
        "OnApplicationFocus",
        "OnAnimatorMove",
        "OnAnimatorIK",
    };

    private static readonly HashSet<string> s_unityBehaviourBaseTypes = new(StringComparer.Ordinal)
    {
        "MonoBehaviour",
        "Behaviour",
        "Component",
        "NetworkBehaviour",
    };

    public static bool IsUnityAssembly(TypeDefinition type)
    {
        var assemblyName = type.DeclaringModule?.Assembly?.Name?.ToString();
        if (assemblyName == null) return false;
        return assemblyName.StartsWith("UnityEngine", StringComparison.Ordinal) ||
               assemblyName.StartsWith("Unity.", StringComparison.Ordinal);
    }

    public static bool DetectUnityFunction(MethodDefinition method)
    {
        if (!s_unityMessages.Contains(method.Name ?? string.Empty))
            return false;

        var type = method.DeclaringType;
        while (type != null)
        {
            if (s_unityBehaviourBaseTypes.Contains(type.Name ?? string.Empty) && IsUnityAssembly(type))
                return true;
            type = type.BaseType?.Resolve();
        }

        return false;
    }
}
