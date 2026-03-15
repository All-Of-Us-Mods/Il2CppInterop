using System.Reflection;
using System.Reflection.Emit;

namespace Il2CppInterop.Common;

public static class Il2CppInteropUtils
{
    private const string MethodInfoPointerPrefix = "NativeMethodInfoPtr_";
    private const string UnityFunctionFieldPrefix = "UnityFunction_";
    private const string UnityFunctionFieldName = "UnityFunction";

    private static FieldInfo? GetFieldInfoFromMethod(MethodBase method, string prefix)
    {
        var body = method.GetMethodBody();
        if (body == null) throw new ArgumentException("Target method may not be abstract");
        var methodModule = method.DeclaringType.Assembly.Modules.Single();
        foreach (var (opCode, opArg) in MiniIlParser.Decode(body.GetILAsByteArray()))
        {
            if (opCode != OpCodes.Ldsfld) continue;

            var fieldInfo = methodModule.ResolveField((int)opArg, method.DeclaringType.GenericTypeArguments, method.GetGenericArguments());
            if (fieldInfo?.FieldType != typeof(IntPtr)) continue;

            if (fieldInfo.Name.StartsWith(prefix)) return fieldInfo;

            // Resolve generic method info pointer fields
            if (method.IsGenericMethod && fieldInfo.DeclaringType.Name.StartsWith("MethodInfoStoreGeneric_") && fieldInfo.Name == "Pointer") return fieldInfo;
        }

        return null;
    }

    public static FieldInfo GetIl2CppMethodInfoPointerFieldForGeneratedMethod(MethodBase method)
    {
        return GetFieldInfoFromMethod(method, MethodInfoPointerPrefix);
    }

    public static FieldInfo GetIl2CppFieldInfoPointerFieldForGeneratedFieldAccessor(MethodBase method)
    {
        return GetFieldInfoFromMethod(method, "NativeFieldInfoPtr_");
    }

    public static bool TryGetUnityFunctionFlagForGeneratedMethod(MethodBase method, out bool unityFunction)
    {
        unityFunction = false;

        FieldInfo? methodInfoField;
        try
        {
            methodInfoField = GetFieldInfoFromMethod(method, MethodInfoPointerPrefix);
        }
        catch (ArgumentException)
        {
            return false;
        }

        if (methodInfoField == null)
        {
            return false;
        }

        var declaringType = methodInfoField.DeclaringType;
        if (declaringType == null)
        {
            return false;
        }

        if (TryReadStaticBoolField(declaringType, UnityFunctionFieldName, out unityFunction))
        {
            return true;
        }

        if (!methodInfoField.Name.StartsWith(MethodInfoPointerPrefix, StringComparison.Ordinal))
        {
            return false;
        }

        var methodSuffix = methodInfoField.Name.Substring(MethodInfoPointerPrefix.Length);
        return TryReadStaticBoolField(declaringType, UnityFunctionFieldPrefix + methodSuffix, out unityFunction);
    }

    private static bool TryReadStaticBoolField(Type type, string fieldName, out bool value)
    {
        value = false;
        const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;

        var field = type.GetField(fieldName, flags);
        if (field?.FieldType != typeof(bool))
        {
            return false;
        }

        value = (bool)field.GetValue(null)!;
        return true;
    }
}
