using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using Il2CppInterop.Common;
using Il2CppInterop.Common.Attributes;
using Il2CppInterop.Runtime.InteropTypes;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppInterop.Runtime.Runtime;
using Microsoft.Extensions.Logging;

namespace Il2CppInterop.Runtime;

[SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores")]
[SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Local")]
[SuppressMessage("ReSharper", "InconsistentNaming")]
public static unsafe class IL2CPP
{
    private static readonly Dictionary<string, nint> ourImagesMap = new();

    private static readonly nint s_nativeHandle;

    private static nint Resolve(string originalName)
    {
        string mapped = FusionInterop.get_il2cpp_api(originalName);
        if (!NativeLibrary.TryGetExport(s_nativeHandle, mapped, out var addr))
            throw new DllNotFoundException("Failed to resolve il2cpp export '" + originalName + "' (mapped '" + mapped +
                                           "')");
        return addr;
    }

    static IL2CPP()
    {
        s_nativeHandle = NativeLibrary.Load("libil2cpp.so", typeof(IL2CPP).Assembly, null);
        s_il2cpp_init = Resolve("il2cpp_init");
        s_il2cpp_init_utf16 = Resolve("il2cpp_init_utf16");
        s_il2cpp_shutdown = Resolve("il2cpp_shutdown");
        s_il2cpp_set_config_dir = Resolve("il2cpp_set_config_dir");
        s_il2cpp_set_data_dir = Resolve("il2cpp_set_data_dir");
        s_il2cpp_set_temp_dir = Resolve("il2cpp_set_temp_dir");
        s_il2cpp_set_commandline_arguments = Resolve("il2cpp_set_commandline_arguments");
        s_il2cpp_set_commandline_arguments_utf16 = Resolve("il2cpp_set_commandline_arguments_utf16");
        s_il2cpp_set_config_utf16 = Resolve("il2cpp_set_config_utf16");
        s_il2cpp_set_config = Resolve("il2cpp_set_config");
        s_il2cpp_set_memory_callbacks = Resolve("il2cpp_set_memory_callbacks");
        s_il2cpp_get_corlib = Resolve("il2cpp_get_corlib");
        s_il2cpp_add_internal_call = Resolve("il2cpp_add_internal_call");
        s_il2cpp_resolve_icall = Resolve("il2cpp_resolve_icall");
        s_il2cpp_alloc = Resolve("il2cpp_alloc");
        s_il2cpp_free = Resolve("il2cpp_free");
        s_il2cpp_array_class_get = Resolve("il2cpp_array_class_get");
        s_il2cpp_array_length = Resolve("il2cpp_array_length");
        s_il2cpp_array_get_byte_length = Resolve("il2cpp_array_get_byte_length");
        s_il2cpp_array_new = Resolve("il2cpp_array_new");
        s_il2cpp_array_new_specific = Resolve("il2cpp_array_new_specific");
        s_il2cpp_array_new_full = Resolve("il2cpp_array_new_full");
        s_il2cpp_bounded_array_class_get = Resolve("il2cpp_bounded_array_class_get");
        s_il2cpp_array_element_size = Resolve("il2cpp_array_element_size");
        s_il2cpp_assembly_get_image = Resolve("il2cpp_assembly_get_image");
        s_il2cpp_class_enum_basetype = Resolve("il2cpp_class_enum_basetype");
        s_il2cpp_class_is_generic = Resolve("il2cpp_class_is_generic");
        s_il2cpp_class_is_inflated = Resolve("il2cpp_class_is_inflated");
        s_il2cpp_class_is_assignable_from = Resolve("il2cpp_class_is_assignable_from");
        s_il2cpp_class_is_subclass_of = Resolve("il2cpp_class_is_subclass_of");
        s_il2cpp_class_has_parent = Resolve("il2cpp_class_has_parent");
        s_il2cpp_class_from_il2cpp_type = Resolve("il2cpp_class_from_il2cpp_type");
        s_il2cpp_class_from_name = Resolve("il2cpp_class_from_name");
        s_il2cpp_class_from_system_type = Resolve("il2cpp_class_from_system_type");
        s_il2cpp_class_get_element_class = Resolve("il2cpp_class_get_element_class");
        s_il2cpp_class_get_events = Resolve("il2cpp_class_get_events");
        s_il2cpp_class_get_fields = Resolve("il2cpp_class_get_fields");
        s_il2cpp_class_get_nested_types = Resolve("il2cpp_class_get_nested_types");
        s_il2cpp_class_get_interfaces = Resolve("il2cpp_class_get_interfaces");
        s_il2cpp_class_get_properties = Resolve("il2cpp_class_get_properties");
        s_il2cpp_class_get_property_from_name = Resolve("il2cpp_class_get_property_from_name");
        s_il2cpp_class_get_field_from_name = Resolve("il2cpp_class_get_field_from_name");
        s_il2cpp_class_get_methods = Resolve("il2cpp_class_get_methods");
        s_il2cpp_class_get_method_from_name = Resolve("il2cpp_class_get_method_from_name");
        s_il2cpp_class_get_name = Resolve("il2cpp_class_get_name");
        s_il2cpp_class_get_namespace = Resolve("il2cpp_class_get_namespace");
        s_il2cpp_class_get_parent = Resolve("il2cpp_class_get_parent");
        s_il2cpp_class_get_declaring_type = Resolve("il2cpp_class_get_declaring_type");
        s_il2cpp_class_instance_size = Resolve("il2cpp_class_instance_size");
        s_il2cpp_class_num_fields = Resolve("il2cpp_class_num_fields");
        s_il2cpp_class_is_valuetype = Resolve("il2cpp_class_is_valuetype");
        s_il2cpp_class_value_size = Resolve("il2cpp_class_value_size");
        s_il2cpp_class_is_blittable = Resolve("il2cpp_class_is_blittable");
        s_il2cpp_class_get_flags = Resolve("il2cpp_class_get_flags");
        s_il2cpp_class_is_abstract = Resolve("il2cpp_class_is_abstract");
        s_il2cpp_class_is_interface = Resolve("il2cpp_class_is_interface");
        s_il2cpp_class_array_element_size = Resolve("il2cpp_class_array_element_size");
        s_il2cpp_class_from_type = Resolve("il2cpp_class_from_type");
        s_il2cpp_class_get_type = Resolve("il2cpp_class_get_type");
        s_il2cpp_class_get_type_token = Resolve("il2cpp_class_get_type_token");
        s_il2cpp_class_has_attribute = Resolve("il2cpp_class_has_attribute");
        s_il2cpp_class_has_references = Resolve("il2cpp_class_has_references");
        s_il2cpp_class_is_enum = Resolve("il2cpp_class_is_enum");
        s_il2cpp_class_get_image = Resolve("il2cpp_class_get_image");
        s_il2cpp_class_get_assemblyname = Resolve("il2cpp_class_get_assemblyname");
        s_il2cpp_class_get_rank = Resolve("il2cpp_class_get_rank");
        s_il2cpp_class_get_bitmap_size = Resolve("il2cpp_class_get_bitmap_size");
        s_il2cpp_class_get_bitmap = Resolve("il2cpp_class_get_bitmap");
        s_il2cpp_stats_dump_to_file = Resolve("il2cpp_stats_dump_to_file");
        s_il2cpp_domain_get = Resolve("il2cpp_domain_get");
        s_il2cpp_domain_assembly_open = Resolve("il2cpp_domain_assembly_open");
        s_il2cpp_domain_get_assemblies = Resolve("il2cpp_domain_get_assemblies");
        s_il2cpp_exception_from_name_msg = Resolve("il2cpp_exception_from_name_msg");
        s_il2cpp_get_exception_argument_null = Resolve("il2cpp_get_exception_argument_null");
        s_il2cpp_format_exception = Resolve("il2cpp_format_exception");
        s_il2cpp_format_stack_trace = Resolve("il2cpp_format_stack_trace");
        s_il2cpp_unhandled_exception = Resolve("il2cpp_unhandled_exception");
        s_il2cpp_field_get_flags = Resolve("il2cpp_field_get_flags");
        s_il2cpp_field_get_name = Resolve("il2cpp_field_get_name");
        s_il2cpp_field_get_parent = Resolve("il2cpp_field_get_parent");
        s_il2cpp_field_get_offset = Resolve("il2cpp_field_get_offset");
        s_il2cpp_field_get_type = Resolve("il2cpp_field_get_type");
        s_il2cpp_field_get_value = Resolve("il2cpp_field_get_value");
        s_il2cpp_field_get_value_object = Resolve("il2cpp_field_get_value_object");
        s_il2cpp_field_has_attribute = Resolve("il2cpp_field_has_attribute");
        s_il2cpp_field_set_value = Resolve("il2cpp_field_set_value");
        s_il2cpp_field_static_get_value = Resolve("il2cpp_field_static_get_value");
        s_il2cpp_field_static_set_value = Resolve("il2cpp_field_static_set_value");
        s_il2cpp_field_set_value_object = Resolve("il2cpp_field_set_value_object");
        s_il2cpp_gc_collect = Resolve("il2cpp_gc_collect");
        s_il2cpp_gc_collect_a_little = Resolve("il2cpp_gc_collect_a_little");
        s_il2cpp_gc_disable = Resolve("il2cpp_gc_disable");
        s_il2cpp_gc_enable = Resolve("il2cpp_gc_enable");
        s_il2cpp_gc_is_disabled = Resolve("il2cpp_gc_is_disabled");
        s_il2cpp_gc_get_used_size = Resolve("il2cpp_gc_get_used_size");
        s_il2cpp_gc_get_heap_size = Resolve("il2cpp_gc_get_heap_size");
        s_il2cpp_gc_wbarrier_set_field = Resolve("il2cpp_gc_wbarrier_set_field");
        s_il2cpp_gchandle_new = Resolve("il2cpp_gchandle_new");
        s_il2cpp_gchandle_new_weakref = Resolve("il2cpp_gchandle_new_weakref");
        s_il2cpp_gchandle_get_target = Resolve("il2cpp_gchandle_get_target");
        s_il2cpp_gchandle_free = Resolve("il2cpp_gchandle_free");
        s_il2cpp_unity_liveness_calculation_begin = Resolve("il2cpp_unity_liveness_calculation_begin");
        s_il2cpp_unity_liveness_calculation_end = Resolve("il2cpp_unity_liveness_calculation_end");
        s_il2cpp_unity_liveness_calculation_from_root = Resolve("il2cpp_unity_liveness_calculation_from_root");
        s_il2cpp_unity_liveness_calculation_from_statics = Resolve("il2cpp_unity_liveness_calculation_from_statics");
        s_il2cpp_method_get_return_type = Resolve("il2cpp_method_get_return_type");
        s_il2cpp_method_get_declaring_type = Resolve("il2cpp_method_get_declaring_type");
        s_il2cpp_method_get_name = Resolve("il2cpp_method_get_name");
        s__il2cpp_method_get_from_reflection = Resolve("il2cpp_method_get_from_reflection");
        s_il2cpp_method_get_object = Resolve("il2cpp_method_get_object");
        s_il2cpp_method_is_generic = Resolve("il2cpp_method_is_generic");
        s_il2cpp_method_is_inflated = Resolve("il2cpp_method_is_inflated");
        s_il2cpp_method_is_instance = Resolve("il2cpp_method_is_instance");
        s_il2cpp_method_get_param_count = Resolve("il2cpp_method_get_param_count");
        s_il2cpp_method_get_param = Resolve("il2cpp_method_get_param");
        s_il2cpp_method_get_class = Resolve("il2cpp_method_get_class");
        s_il2cpp_method_has_attribute = Resolve("il2cpp_method_has_attribute");
        s_il2cpp_method_get_flags = Resolve("il2cpp_method_get_flags");
        s_il2cpp_method_get_token = Resolve("il2cpp_method_get_token");
        s_il2cpp_method_get_param_name = Resolve("il2cpp_method_get_param_name");
        s_il2cpp_profiler_install = Resolve("il2cpp_profiler_install");
        s_il2cpp_profiler_install_enter_leave = Resolve("il2cpp_profiler_install_enter_leave");
        s_il2cpp_profiler_install_allocation = Resolve("il2cpp_profiler_install_allocation");
        s_il2cpp_profiler_install_gc = Resolve("il2cpp_profiler_install_gc");
        s_il2cpp_profiler_install_fileio = Resolve("il2cpp_profiler_install_fileio");
        s_il2cpp_profiler_install_thread = Resolve("il2cpp_profiler_install_thread");
        s_il2cpp_property_get_flags = Resolve("il2cpp_property_get_flags");
        s_il2cpp_property_get_get_method = Resolve("il2cpp_property_get_get_method");
        s_il2cpp_property_get_set_method = Resolve("il2cpp_property_get_set_method");
        s_il2cpp_property_get_name = Resolve("il2cpp_property_get_name");
        s_il2cpp_property_get_parent = Resolve("il2cpp_property_get_parent");
        s_il2cpp_object_get_class = Resolve("il2cpp_object_get_class");
        s_il2cpp_object_get_size = Resolve("il2cpp_object_get_size");
        s_il2cpp_object_get_virtual_method = Resolve("il2cpp_object_get_virtual_method");
        s_il2cpp_object_new = Resolve("il2cpp_object_new");
        s_il2cpp_object_unbox = Resolve("il2cpp_object_unbox");
        s_il2cpp_value_box = Resolve("il2cpp_value_box");
        s_il2cpp_monitor_enter = Resolve("il2cpp_monitor_enter");
        s_il2cpp_monitor_try_enter = Resolve("il2cpp_monitor_try_enter");
        s_il2cpp_monitor_exit = Resolve("il2cpp_monitor_exit");
        s_il2cpp_monitor_pulse = Resolve("il2cpp_monitor_pulse");
        s_il2cpp_monitor_pulse_all = Resolve("il2cpp_monitor_pulse_all");
        s_il2cpp_monitor_wait = Resolve("il2cpp_monitor_wait");
        s_il2cpp_monitor_try_wait = Resolve("il2cpp_monitor_try_wait");
        s_il2cpp_runtime_invoke = Resolve("il2cpp_runtime_invoke");
        s_il2cpp_runtime_invoke_convert_args = Resolve("il2cpp_runtime_invoke_convert_args");
        s_il2cpp_runtime_class_init = Resolve("il2cpp_runtime_class_init");
        s_il2cpp_runtime_object_init = Resolve("il2cpp_runtime_object_init");
        s_il2cpp_runtime_object_init_exception = Resolve("il2cpp_runtime_object_init_exception");
        s_il2cpp_string_length = Resolve("il2cpp_string_length");
        s_il2cpp_string_chars = Resolve("il2cpp_string_chars");
        s_il2cpp_string_new = Resolve("il2cpp_string_new");
        s_il2cpp_string_new_len = Resolve("il2cpp_string_new_len");
        s_il2cpp_string_new_utf16 = Resolve("il2cpp_string_new_utf16");
        s_il2cpp_string_new_wrapper = Resolve("il2cpp_string_new_wrapper");
        s_il2cpp_string_intern = Resolve("il2cpp_string_intern");
        s_il2cpp_string_is_interned = Resolve("il2cpp_string_is_interned");
        s_il2cpp_thread_current = Resolve("il2cpp_thread_current");
        s_il2cpp_thread_attach = Resolve("il2cpp_thread_attach");
        s_il2cpp_thread_detach = Resolve("il2cpp_thread_detach");
        s_il2cpp_thread_get_all_attached_threads = Resolve("il2cpp_thread_get_all_attached_threads");
        s_il2cpp_is_vm_thread = Resolve("il2cpp_is_vm_thread");
        s_il2cpp_current_thread_walk_frame_stack = Resolve("il2cpp_current_thread_walk_frame_stack");
        s_il2cpp_thread_walk_frame_stack = Resolve("il2cpp_thread_walk_frame_stack");
        s_il2cpp_current_thread_get_top_frame = Resolve("il2cpp_current_thread_get_top_frame");
        s_il2cpp_thread_get_top_frame = Resolve("il2cpp_thread_get_top_frame");
        s_il2cpp_current_thread_get_frame_at = Resolve("il2cpp_current_thread_get_frame_at");
        s_il2cpp_thread_get_frame_at = Resolve("il2cpp_thread_get_frame_at");
        s_il2cpp_current_thread_get_stack_depth = Resolve("il2cpp_current_thread_get_stack_depth");
        s_il2cpp_thread_get_stack_depth = Resolve("il2cpp_thread_get_stack_depth");
        s_il2cpp_type_get_object = Resolve("il2cpp_type_get_object");
        s_il2cpp_type_get_type = Resolve("il2cpp_type_get_type");
        s_il2cpp_type_get_class_or_element_class = Resolve("il2cpp_type_get_class_or_element_class");
        s_il2cpp_type_get_name = Resolve("il2cpp_type_get_name");
        s_il2cpp_type_is_byref = Resolve("il2cpp_type_is_byref");
        s_il2cpp_type_get_attrs = Resolve("il2cpp_type_get_attrs");
        s_il2cpp_type_equals = Resolve("il2cpp_type_equals");
        s_il2cpp_type_get_assembly_qualified_name = Resolve("il2cpp_type_get_assembly_qualified_name");
        s_il2cpp_image_get_assembly = Resolve("il2cpp_image_get_assembly");
        s_il2cpp_image_get_name = Resolve("il2cpp_image_get_name");
        s_il2cpp_image_get_filename = Resolve("il2cpp_image_get_filename");
        s_il2cpp_image_get_entry_point = Resolve("il2cpp_image_get_entry_point");
        s_il2cpp_image_get_class_count = Resolve("il2cpp_image_get_class_count");
        s_il2cpp_image_get_class = Resolve("il2cpp_image_get_class");
        s_il2cpp_capture_memory_snapshot = Resolve("il2cpp_capture_memory_snapshot");
        s_il2cpp_free_captured_memory_snapshot = Resolve("il2cpp_free_captured_memory_snapshot");
        s_il2cpp_set_find_plugin_callback = Resolve("il2cpp_set_find_plugin_callback");
        s_il2cpp_register_log_callback = Resolve("il2cpp_register_log_callback");
        s_il2cpp_debugger_set_agent_options = Resolve("il2cpp_debugger_set_agent_options");
        s_il2cpp_is_debugger_attached = Resolve("il2cpp_is_debugger_attached");
        s_il2cpp_unity_install_unitytls_interface = Resolve("il2cpp_unity_install_unitytls_interface");
        s_il2cpp_custom_attrs_from_class = Resolve("il2cpp_custom_attrs_from_class");
        s_il2cpp_custom_attrs_from_method = Resolve("il2cpp_custom_attrs_from_method");
        s_il2cpp_custom_attrs_get_attr = Resolve("il2cpp_custom_attrs_get_attr");
        s_il2cpp_custom_attrs_has_attr = Resolve("il2cpp_custom_attrs_has_attr");
        s_il2cpp_custom_attrs_construct = Resolve("il2cpp_custom_attrs_construct");
        s_il2cpp_custom_attrs_free = Resolve("il2cpp_custom_attrs_free");

        var domain = il2cpp_domain_get();
        if (domain == nint.Zero)
        {
            Logger.Instance.LogError("No il2cpp domain found; sad!");
            return;
        }

        uint assembliesCount = 0;
        var assemblies = il2cpp_domain_get_assemblies(domain, ref assembliesCount);
        for (var i = 0; i < assembliesCount; i++)
        {
            var image = il2cpp_assembly_get_image(assemblies[i]);
            var name = il2cpp_image_get_name_(image)!;
            ourImagesMap[name] = image;
        }
    }

    internal static nint GetIl2CppImage(string name)
    {
        return ourImagesMap.TryGetValue(name, out var image) ? image : nint.Zero;
    }

    internal static nint[] GetIl2CppImages()
    {
        return ourImagesMap.Values.ToArray();
    }

    public static nint GetIl2CppClass(string assemblyName, string namespaze, string className)
    {
        if (!ourImagesMap.TryGetValue(assemblyName, out var image))
        {
            Logger.Instance.LogError("Assembly {AssemblyName} is not registered in il2cpp", assemblyName);
            return nint.Zero;
        }

        var clazz = il2cpp_class_from_name(image, namespaze, className);
        return clazz;
    }

    public static nint GetIl2CppField(nint clazz, string fieldName)
    {
        if (clazz == nint.Zero) return nint.Zero;

        var field = il2cpp_class_get_field_from_name(clazz, fieldName);
        if (field == nint.Zero)
            Logger.Instance.LogError(
                "Field {FieldName} was not found on class {ClassName}", fieldName, il2cpp_class_get_name_(clazz));
        return field;
    }

    public static nint GetIl2CppMethodByToken(nint clazz, int token)
    {
        if (clazz == nint.Zero)
            return NativeStructUtils.GetMethodInfoForMissingMethod(token.ToString());

        var iter = nint.Zero;
        nint method;
        while ((method = il2cpp_class_get_methods(clazz, ref iter)) != nint.Zero)
            if (il2cpp_method_get_token(method) == token)
                return method;

        var className = il2cpp_class_get_name_(clazz);
        Logger.Instance.LogTrace("Unable to find method {ClassName}::{Token}", className, token);

        return NativeStructUtils.GetMethodInfoForMissingMethod(className + "::" + token);
    }

    public static nint GetIl2CppMethod(nint clazz, bool isGeneric, string methodName, string returnTypeName,
        params string[] argTypes)
    {
        if (clazz == nint.Zero)
            return NativeStructUtils.GetMethodInfoForMissingMethod(methodName + "(" + string.Join(", ", argTypes) +
                                                                   ")");

        returnTypeName = Regex.Replace(returnTypeName, "\\`\\d+", "").Replace('/', '.').Replace('+', '.');
        for (var index = 0; index < argTypes.Length; index++)
        {
            var argType = argTypes[index];
            argTypes[index] = Regex.Replace(argType, "\\`\\d+", "").Replace('/', '.').Replace('+', '.');
        }

        var methodsSeen = 0;
        var lastMethod = nint.Zero;
        var iter = nint.Zero;
        nint method;
        while ((method = il2cpp_class_get_methods(clazz, ref iter)) != nint.Zero)
        {
            if (il2cpp_method_get_name_(method) != methodName)
                continue;

            if (il2cpp_method_get_param_count(method) != argTypes.Length)
                continue;

            if (il2cpp_method_is_generic(method) != isGeneric)
                continue;

            var returnType = il2cpp_method_get_return_type(method);
            var returnTypeNameActual = il2cpp_type_get_name_(returnType);
            if (returnTypeNameActual != returnTypeName)
                continue;

            methodsSeen++;
            lastMethod = method;

            var badType = false;
            for (var i = 0; i < argTypes.Length; i++)
            {
                var paramType = il2cpp_method_get_param(method, (uint)i);
                var typeName = il2cpp_type_get_name_(paramType);
                if (typeName != argTypes[i])
                {
                    badType = true;
                    break;
                }
            }

            if (badType) continue;

            return method;
        }

        var className = il2cpp_class_get_name_(clazz);

        if (methodsSeen == 1)
        {
            Logger.Instance.LogTrace(
                "Method {ClassName}::{MethodName} was stubbed with a random matching method of the same name",
                className, methodName);
            Logger.Instance.LogTrace(
                "Stubby return type/target: {LastMethod} / {ReturnTypeName}",
                il2cpp_type_get_name_(il2cpp_method_get_return_type(lastMethod)), returnTypeName);
            Logger.Instance.LogTrace("Stubby parameter types/targets follow:");
            for (var i = 0; i < argTypes.Length; i++)
            {
                var paramType = il2cpp_method_get_param(lastMethod, (uint)i);
                var typeName = il2cpp_type_get_name_(paramType);
                Logger.Instance.LogTrace("    {TypeName} / {ArgType}", typeName, argTypes[i]);
            }

            return lastMethod;
        }

        Logger.Instance.LogTrace("Unable to find method {ClassName}::{MethodName}; signature follows", className,
            methodName);
        Logger.Instance.LogTrace("    return {ReturnTypeName}", returnTypeName);
        foreach (var argType in argTypes)
            Logger.Instance.LogTrace("    {ArgType}", argType);
        Logger.Instance.LogTrace("Available methods of this name follow:");
        iter = nint.Zero;
        while ((method = il2cpp_class_get_methods(clazz, ref iter)) != nint.Zero)
        {
            if (il2cpp_method_get_name_(method) != methodName)
                continue;

            var nParams = il2cpp_method_get_param_count(method);
            Logger.Instance.LogTrace("Method starts");
            Logger.Instance.LogTrace(
                "     return {MethodTypeName}", il2cpp_type_get_name_(il2cpp_method_get_return_type(method)));
            for (var i = 0; i < nParams; i++)
            {
                var paramType = il2cpp_method_get_param(method, (uint)i);
                var typeName = il2cpp_type_get_name_(paramType);
                Logger.Instance.LogTrace("    {TypeName}", typeName);
            }

            return method;
        }

        return NativeStructUtils.GetMethodInfoForMissingMethod(className + "::" + methodName + "(" +
                                                               string.Join(", ", argTypes) + ")");
    }

    public static string? Il2CppStringToManaged(nint il2CppString)
    {
        if (il2CppString == nint.Zero) return null;

        var length = il2cpp_string_length(il2CppString);
        var chars = il2cpp_string_chars(il2CppString);

        return new string(chars, 0, length);
    }

    public static nint ManagedStringToIl2Cpp(string? str)
    {
        if (str == null) return nint.Zero;

        fixed (char* chars = str)
        {
            return il2cpp_string_new_utf16(chars, str.Length);
        }
    }

    public static nint Il2CppObjectBaseToPtr(Il2CppObjectBase obj)
    {
        return obj?.Pointer ?? nint.Zero;
    }

    public static nint Il2CppObjectBaseToPtrNotNull(Il2CppObjectBase obj)
    {
        return obj?.Pointer ?? throw new NullReferenceException();
    }

    public static nint GetIl2CppNestedType(nint enclosingType, string nestedTypeName)
    {
        if (enclosingType == nint.Zero) return nint.Zero;

        var iter = nint.Zero;
        nint nestedTypePtr;
        if (il2cpp_class_is_inflated(enclosingType))
        {
            Logger.Instance.LogTrace("Original class was inflated, falling back to reflection");

            return RuntimeReflectionHelper.GetNestedTypeViaReflection(enclosingType, nestedTypeName);
        }

        while ((nestedTypePtr = il2cpp_class_get_nested_types(enclosingType, ref iter)) != nint.Zero)
            if (il2cpp_class_get_name_(nestedTypePtr) == nestedTypeName)
                return nestedTypePtr;

        Logger.Instance.LogError(
            "Nested type {NestedTypeName} on {EnclosingTypeName} not found!", nestedTypeName,
            il2cpp_class_get_name_(enclosingType));

        return nint.Zero;
    }

    public static void ThrowIfNull(object arg)
    {
        if (arg == null)
            throw new NullReferenceException();
    }

    public static T ResolveICall<T>(string signature) where T : Delegate
    {
        var icallPtr = il2cpp_resolve_icall(signature);
        if (icallPtr == nint.Zero)
        {
            Logger.Instance.LogTrace("ICall {Signature} not resolved", signature);
            return GenerateDelegateForMissingICall<T>(signature);
        }

        return Marshal.GetDelegateForFunctionPointer<T>(icallPtr);
    }

    private static T GenerateDelegateForMissingICall<T>(string signature) where T : Delegate
    {
        var invoke = typeof(T).GetMethod("Invoke")!;

        var trampoline = new DynamicMethod("(missing icall delegate) " + typeof(T).FullName,
            invoke.ReturnType, invoke.GetParameters().Select(it => it.ParameterType).ToArray(), typeof(IL2CPP), true);
        var bodyBuilder = trampoline.GetILGenerator();

        bodyBuilder.Emit(OpCodes.Ldstr, $"ICall with signature {signature} was not resolved");
        bodyBuilder.Emit(OpCodes.Newobj, typeof(Exception).GetConstructor(new[] { typeof(string) })!);
        bodyBuilder.Emit(OpCodes.Throw);

        return (T)trampoline.CreateDelegate(typeof(T));
    }

    public static T? PointerToValueGeneric<T>(nint objectPointer, bool isFieldPointer, bool valueTypeWouldBeBoxed)
    {
        if (isFieldPointer)
        {
            if (il2cpp_class_is_valuetype(Il2CppClassPointerStore<T>.NativeClassPtr))
                objectPointer = il2cpp_value_box(Il2CppClassPointerStore<T>.NativeClassPtr, objectPointer);
            else
                objectPointer = *(nint*)objectPointer;
        }

        if (!valueTypeWouldBeBoxed && il2cpp_class_is_valuetype(Il2CppClassPointerStore<T>.NativeClassPtr))
            objectPointer = il2cpp_value_box(Il2CppClassPointerStore<T>.NativeClassPtr, objectPointer);

        if (typeof(T) == typeof(string))
            return (T)(object)Il2CppStringToManaged(objectPointer);

        if (objectPointer == nint.Zero)
            return default;

        if (typeof(T).IsValueType)
            return Il2CppObjectBase.UnboxUnsafe<T>(objectPointer);

        return Il2CppObjectPool.Get<T>(objectPointer);
    }

    public static string RenderTypeName<T>(bool addRefMarker = false)
    {
        return RenderTypeName(typeof(T), addRefMarker);
    }

    public static string RenderTypeName(Type t, bool addRefMarker = false)
    {
        if (addRefMarker) return RenderTypeName(t) + "&";
        if (t.IsArray) return RenderTypeName(t.GetElementType()) + "[]";
        if (t.IsByRef) return RenderTypeName(t.GetElementType()) + "&";
        if (t.IsPointer) return RenderTypeName(t.GetElementType()) + "*";
        if (t.IsGenericParameter) return t.Name;

        if (t.IsGenericType)
        {
            if (t.TypeHasIl2CppArrayBase())
                return RenderTypeName(t.GetGenericArguments()[0]) + "[]";

            var builder = new StringBuilder();
            builder.Append(t.GetGenericTypeDefinition().FullNameObfuscated().TrimIl2CppPrefix());
            builder.Append('<');
            var genericArguments = t.GetGenericArguments();
            for (var i = 0; i < genericArguments.Length; i++)
            {
                if (i != 0) builder.Append(',');
                builder.Append(RenderTypeName(genericArguments[i]));
            }

            builder.Append('>');
            return builder.ToString();
        }

        if (t == typeof(Il2CppStringArray))
            return "System.String[]";

        return t.FullNameObfuscated().TrimIl2CppPrefix();
    }

    private static string FullNameObfuscated(this Type t)
    {
        var obfuscatedNameAnnotations = t.GetCustomAttribute<ObfuscatedNameAttribute>();
        if (obfuscatedNameAnnotations == null) return t.FullName;
        return obfuscatedNameAnnotations.ObfuscatedName;
    }

    private static string TrimIl2CppPrefix(this string s)
    {
        return s.StartsWith("Il2Cpp") ? s.Substring("Il2Cpp".Length) : s;
    }

    private static bool TypeHasIl2CppArrayBase(this Type type)
    {
        if (type == null) return false;
        if (type.IsConstructedGenericType) type = type.GetGenericTypeDefinition();
        if (type == typeof(Il2CppArrayBase<>)) return true;
        return TypeHasIl2CppArrayBase(type.BaseType);
    }

    // this is called if there's no actual il2cpp_gc_wbarrier_set_field()
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void FieldWriteWbarrierStub(nint obj, nint targetAddress, nint value)
    {
        // ignore obj
        *(nint*)targetAddress = value;
    }

    // IL2CPP Functions (resolved via FusionInterop.get_il2cpp_api)
    private static nint s_il2cpp_init;
    private static nint s_il2cpp_init_utf16;
    private static nint s_il2cpp_shutdown;
    private static nint s_il2cpp_set_config_dir;
    private static nint s_il2cpp_set_data_dir;
    private static nint s_il2cpp_set_temp_dir;
    private static nint s_il2cpp_set_commandline_arguments;
    private static nint s_il2cpp_set_commandline_arguments_utf16;
    private static nint s_il2cpp_set_config_utf16;
    private static nint s_il2cpp_set_config;
    private static nint s_il2cpp_set_memory_callbacks;
    private static nint s_il2cpp_get_corlib;
    private static nint s_il2cpp_add_internal_call;
    private static nint s_il2cpp_resolve_icall;
    private static nint s_il2cpp_alloc;
    private static nint s_il2cpp_free;
    private static nint s_il2cpp_array_class_get;
    private static nint s_il2cpp_array_length;
    private static nint s_il2cpp_array_get_byte_length;
    private static nint s_il2cpp_array_new;
    private static nint s_il2cpp_array_new_specific;
    private static nint s_il2cpp_array_new_full;
    private static nint s_il2cpp_bounded_array_class_get;
    private static nint s_il2cpp_array_element_size;
    private static nint s_il2cpp_assembly_get_image;
    private static nint s_il2cpp_class_enum_basetype;
    private static nint s_il2cpp_class_is_generic;
    private static nint s_il2cpp_class_is_inflated;
    private static nint s_il2cpp_class_is_assignable_from;
    private static nint s_il2cpp_class_is_subclass_of;
    private static nint s_il2cpp_class_has_parent;
    private static nint s_il2cpp_class_from_il2cpp_type;
    private static nint s_il2cpp_class_from_name;
    private static nint s_il2cpp_class_from_system_type;
    private static nint s_il2cpp_class_get_element_class;
    private static nint s_il2cpp_class_get_events;
    private static nint s_il2cpp_class_get_fields;
    private static nint s_il2cpp_class_get_nested_types;
    private static nint s_il2cpp_class_get_interfaces;
    private static nint s_il2cpp_class_get_properties;
    private static nint s_il2cpp_class_get_property_from_name;
    private static nint s_il2cpp_class_get_field_from_name;
    private static nint s_il2cpp_class_get_methods;
    private static nint s_il2cpp_class_get_method_from_name;
    private static nint s_il2cpp_class_get_name;
    private static nint s_il2cpp_class_get_namespace;
    private static nint s_il2cpp_class_get_parent;
    private static nint s_il2cpp_class_get_declaring_type;
    private static nint s_il2cpp_class_instance_size;
    private static nint s_il2cpp_class_num_fields;
    private static nint s_il2cpp_class_is_valuetype;
    private static nint s_il2cpp_class_value_size;
    private static nint s_il2cpp_class_is_blittable;
    private static nint s_il2cpp_class_get_flags;
    private static nint s_il2cpp_class_is_abstract;
    private static nint s_il2cpp_class_is_interface;
    private static nint s_il2cpp_class_array_element_size;
    private static nint s_il2cpp_class_from_type;
    private static nint s_il2cpp_class_get_type;
    private static nint s_il2cpp_class_get_type_token;
    private static nint s_il2cpp_class_has_attribute;
    private static nint s_il2cpp_class_has_references;
    private static nint s_il2cpp_class_is_enum;
    private static nint s_il2cpp_class_get_image;
    private static nint s_il2cpp_class_get_assemblyname;
    private static nint s_il2cpp_class_get_rank;
    private static nint s_il2cpp_class_get_bitmap_size;
    private static nint s_il2cpp_class_get_bitmap;
    private static nint s_il2cpp_stats_dump_to_file;
    private static nint s_il2cpp_domain_get;
    private static nint s_il2cpp_domain_assembly_open;
    private static nint s_il2cpp_domain_get_assemblies;
    private static nint s_il2cpp_exception_from_name_msg;
    private static nint s_il2cpp_get_exception_argument_null;
    private static nint s_il2cpp_format_exception;
    private static nint s_il2cpp_format_stack_trace;
    private static nint s_il2cpp_unhandled_exception;
    private static nint s_il2cpp_field_get_flags;
    private static nint s_il2cpp_field_get_name;
    private static nint s_il2cpp_field_get_parent;
    private static nint s_il2cpp_field_get_offset;
    private static nint s_il2cpp_field_get_type;
    private static nint s_il2cpp_field_get_value;
    private static nint s_il2cpp_field_get_value_object;
    private static nint s_il2cpp_field_has_attribute;
    private static nint s_il2cpp_field_set_value;
    private static nint s_il2cpp_field_static_get_value;
    private static nint s_il2cpp_field_static_set_value;
    private static nint s_il2cpp_field_set_value_object;
    private static nint s_il2cpp_gc_collect;
    private static nint s_il2cpp_gc_collect_a_little;
    private static nint s_il2cpp_gc_disable;
    private static nint s_il2cpp_gc_enable;
    private static nint s_il2cpp_gc_is_disabled;
    private static nint s_il2cpp_gc_get_used_size;
    private static nint s_il2cpp_gc_get_heap_size;
    private static nint s_il2cpp_gc_wbarrier_set_field;
    private static nint s_il2cpp_gchandle_new;
    private static nint s_il2cpp_gchandle_new_weakref;
    private static nint s_il2cpp_gchandle_get_target;
    private static nint s_il2cpp_gchandle_free;
    private static nint s_il2cpp_unity_liveness_calculation_begin;
    private static nint s_il2cpp_unity_liveness_calculation_end;
    private static nint s_il2cpp_unity_liveness_calculation_from_root;
    private static nint s_il2cpp_unity_liveness_calculation_from_statics;
    private static nint s_il2cpp_method_get_return_type;
    private static nint s_il2cpp_method_get_declaring_type;
    private static nint s_il2cpp_method_get_name;
    private static nint s__il2cpp_method_get_from_reflection;
    private static nint s_il2cpp_method_get_object;
    private static nint s_il2cpp_method_is_generic;
    private static nint s_il2cpp_method_is_inflated;
    private static nint s_il2cpp_method_is_instance;
    private static nint s_il2cpp_method_get_param_count;
    private static nint s_il2cpp_method_get_param;
    private static nint s_il2cpp_method_get_class;
    private static nint s_il2cpp_method_has_attribute;
    private static nint s_il2cpp_method_get_flags;
    private static nint s_il2cpp_method_get_token;
    private static nint s_il2cpp_method_get_param_name;
    private static nint s_il2cpp_profiler_install;
    private static nint s_il2cpp_profiler_install_enter_leave;
    private static nint s_il2cpp_profiler_install_allocation;
    private static nint s_il2cpp_profiler_install_gc;
    private static nint s_il2cpp_profiler_install_fileio;
    private static nint s_il2cpp_profiler_install_thread;
    private static nint s_il2cpp_property_get_flags;
    private static nint s_il2cpp_property_get_get_method;
    private static nint s_il2cpp_property_get_set_method;
    private static nint s_il2cpp_property_get_name;
    private static nint s_il2cpp_property_get_parent;
    private static nint s_il2cpp_object_get_class;
    private static nint s_il2cpp_object_get_size;
    private static nint s_il2cpp_object_get_virtual_method;
    private static nint s_il2cpp_object_new;
    private static nint s_il2cpp_object_unbox;
    private static nint s_il2cpp_value_box;
    private static nint s_il2cpp_monitor_enter;
    private static nint s_il2cpp_monitor_try_enter;
    private static nint s_il2cpp_monitor_exit;
    private static nint s_il2cpp_monitor_pulse;
    private static nint s_il2cpp_monitor_pulse_all;
    private static nint s_il2cpp_monitor_wait;
    private static nint s_il2cpp_monitor_try_wait;
    private static nint s_il2cpp_runtime_invoke;
    private static nint s_il2cpp_runtime_invoke_convert_args;
    private static nint s_il2cpp_runtime_class_init;
    private static nint s_il2cpp_runtime_object_init;
    private static nint s_il2cpp_runtime_object_init_exception;
    private static nint s_il2cpp_string_length;
    private static nint s_il2cpp_string_chars;
    private static nint s_il2cpp_string_new;
    private static nint s_il2cpp_string_new_len;
    private static nint s_il2cpp_string_new_utf16;
    private static nint s_il2cpp_string_new_wrapper;
    private static nint s_il2cpp_string_intern;
    private static nint s_il2cpp_string_is_interned;
    private static nint s_il2cpp_thread_current;
    private static nint s_il2cpp_thread_attach;
    private static nint s_il2cpp_thread_detach;
    private static nint s_il2cpp_thread_get_all_attached_threads;
    private static nint s_il2cpp_is_vm_thread;
    private static nint s_il2cpp_current_thread_walk_frame_stack;
    private static nint s_il2cpp_thread_walk_frame_stack;
    private static nint s_il2cpp_current_thread_get_top_frame;
    private static nint s_il2cpp_thread_get_top_frame;
    private static nint s_il2cpp_current_thread_get_frame_at;
    private static nint s_il2cpp_thread_get_frame_at;
    private static nint s_il2cpp_current_thread_get_stack_depth;
    private static nint s_il2cpp_thread_get_stack_depth;
    private static nint s_il2cpp_type_get_object;
    private static nint s_il2cpp_type_get_type;
    private static nint s_il2cpp_type_get_class_or_element_class;
    private static nint s_il2cpp_type_get_name;
    private static nint s_il2cpp_type_is_byref;
    private static nint s_il2cpp_type_get_attrs;
    private static nint s_il2cpp_type_equals;
    private static nint s_il2cpp_type_get_assembly_qualified_name;
    private static nint s_il2cpp_image_get_assembly;
    private static nint s_il2cpp_image_get_name;
    private static nint s_il2cpp_image_get_filename;
    private static nint s_il2cpp_image_get_entry_point;
    private static nint s_il2cpp_image_get_class_count;
    private static nint s_il2cpp_image_get_class;
    private static nint s_il2cpp_capture_memory_snapshot;
    private static nint s_il2cpp_free_captured_memory_snapshot;
    private static nint s_il2cpp_set_find_plugin_callback;
    private static nint s_il2cpp_register_log_callback;
    private static nint s_il2cpp_debugger_set_agent_options;
    private static nint s_il2cpp_is_debugger_attached;
    private static nint s_il2cpp_unity_install_unitytls_interface;
    private static nint s_il2cpp_custom_attrs_from_class;
    private static nint s_il2cpp_custom_attrs_from_method;
    private static nint s_il2cpp_custom_attrs_get_attr;
    private static nint s_il2cpp_custom_attrs_has_attr;
    private static nint s_il2cpp_custom_attrs_construct;
    private static nint s_il2cpp_custom_attrs_free;

    public static void il2cpp_init(nint domain_name)
    {
        ((delegate* unmanaged[Cdecl]<nint, void>)s_il2cpp_init)(domain_name);
    }

    public static void il2cpp_init_utf16(nint domain_name)
    {
        ((delegate* unmanaged[Cdecl]<nint, void>)s_il2cpp_init_utf16)(domain_name);
    }

    public static void il2cpp_shutdown()
    {
        ((delegate* unmanaged[Cdecl]<void>)s_il2cpp_shutdown)();
    }

    public static void il2cpp_set_config_dir(nint config_path)
    {
        ((delegate* unmanaged[Cdecl]<nint, void>)s_il2cpp_set_config_dir)(config_path);
    }

    public static void il2cpp_set_data_dir(nint data_path)
    {
        ((delegate* unmanaged[Cdecl]<nint, void>)s_il2cpp_set_data_dir)(data_path);
    }

    public static void il2cpp_set_temp_dir(nint temp_path)
    {
        ((delegate* unmanaged[Cdecl]<nint, void>)s_il2cpp_set_temp_dir)(temp_path);
    }

    public static void il2cpp_set_commandline_arguments(int argc, nint argv, nint basedir)
    {
        ((delegate* unmanaged[Cdecl]<int, nint, nint, void>)s_il2cpp_set_commandline_arguments)(argc, argv,
            basedir);
    }

    public static void il2cpp_set_commandline_arguments_utf16(int argc, nint argv, nint basedir)
    {
        ((delegate* unmanaged[Cdecl]<int, nint, nint, void>)s_il2cpp_set_commandline_arguments_utf16)(argc, argv,
            basedir);
    }

    public static void il2cpp_set_config_utf16(nint executablePath)
    {
        ((delegate* unmanaged[Cdecl]<nint, void>)s_il2cpp_set_config_utf16)(executablePath);
    }

    public static void il2cpp_set_config(nint executablePath)
    {
        ((delegate* unmanaged[Cdecl]<nint, void>)s_il2cpp_set_config)(executablePath);
    }

    public static void il2cpp_set_memory_callbacks(nint callbacks)
    {
        ((delegate* unmanaged[Cdecl]<nint, void>)s_il2cpp_set_memory_callbacks)(callbacks);
    }

    public static nint il2cpp_get_corlib()
    {
        return ((delegate* unmanaged[Cdecl]<nint>)s_il2cpp_get_corlib)();
    }

    public static void il2cpp_add_internal_call(nint name, nint method)
    {
        ((delegate* unmanaged[Cdecl]<nint, nint, void>)s_il2cpp_add_internal_call)(name, method);
    }

    public static nint il2cpp_resolve_icall(string name)
    {
        nint __m_name = Marshal.StringToHGlobalAnsi(name);
        try
        {
            return ((delegate* unmanaged[Cdecl]<byte*, nint>)s_il2cpp_resolve_icall)((byte*)__m_name);
        }
        finally
        {
            Marshal.FreeHGlobal(__m_name);
        }
    }

    public static nint il2cpp_alloc(uint size)
    {
        return ((delegate* unmanaged[Cdecl]<uint, nint>)s_il2cpp_alloc)(size);
    }

    public static void il2cpp_free(nint ptr)
    {
        ((delegate* unmanaged[Cdecl]<nint, void>)s_il2cpp_free)(ptr);
    }

    public static nint il2cpp_array_class_get(nint element_class, uint rank)
    {
        return ((delegate* unmanaged[Cdecl]<nint, uint, nint>)s_il2cpp_array_class_get)(element_class, rank);
    }

    public static uint il2cpp_array_length(nint array)
    {
        return ((delegate* unmanaged[Cdecl]<nint, uint>)s_il2cpp_array_length)(array);
    }

    public static uint il2cpp_array_get_byte_length(nint array)
    {
        return ((delegate* unmanaged[Cdecl]<nint, uint>)s_il2cpp_array_get_byte_length)(array);
    }

    public static nint il2cpp_array_new(nint elementTypeInfo, ulong length)
    {
        return ((delegate* unmanaged[Cdecl]<nint, ulong, nint>)s_il2cpp_array_new)(elementTypeInfo, length);
    }

    public static nint il2cpp_array_new_specific(nint arrayTypeInfo, ulong length)
    {
        return ((delegate* unmanaged[Cdecl]<nint, ulong, nint>)s_il2cpp_array_new_specific)(arrayTypeInfo, length);
    }

    public static nint il2cpp_array_new_full(nint array_class, ref ulong lengths, ref ulong lower_bounds)
    {
        return ((delegate* unmanaged[Cdecl]<nint, ref ulong, ref ulong, nint>)s_il2cpp_array_new_full)(array_class,
            ref lengths, ref lower_bounds);
    }

    public static nint il2cpp_bounded_array_class_get(nint element_class, uint rank, bool bounded)
    {
        return ((delegate* unmanaged[Cdecl]<nint, uint, byte, nint>)s_il2cpp_bounded_array_class_get)(element_class,
            rank, (byte)(bounded ? 1 : 0));
    }

    public static int il2cpp_array_element_size(nint array_class)
    {
        return ((delegate* unmanaged[Cdecl]<nint, int>)s_il2cpp_array_element_size)(array_class);
    }

    public static nint il2cpp_assembly_get_image(nint assembly)
    {
        return ((delegate* unmanaged[Cdecl]<nint, nint>)s_il2cpp_assembly_get_image)(assembly);
    }

    public static nint il2cpp_class_enum_basetype(nint klass)
    {
        return ((delegate* unmanaged[Cdecl]<nint, nint>)s_il2cpp_class_enum_basetype)(klass);
    }

    public static bool il2cpp_class_is_generic(nint klass)
    {
        return (((delegate* unmanaged[Cdecl]<nint, byte>)s_il2cpp_class_is_generic)(klass) != 0);
    }

    public static bool il2cpp_class_is_inflated(nint klass)
    {
        return (((delegate* unmanaged[Cdecl]<nint, byte>)s_il2cpp_class_is_inflated)(klass) != 0);
    }

    public static bool il2cpp_class_is_assignable_from(nint klass, nint oklass)
    {
        return (((delegate* unmanaged[Cdecl]<nint, nint, byte>)s_il2cpp_class_is_assignable_from)(klass, oklass) !=
                0);
    }

    public static bool il2cpp_class_is_subclass_of(nint klass, nint klassc, bool check_interfaces)
    {
        return (((delegate* unmanaged[Cdecl]<nint, nint, byte, byte>)s_il2cpp_class_is_subclass_of)(klass, klassc,
            (byte)(check_interfaces ? 1 : 0)) != 0);
    }

    public static bool il2cpp_class_has_parent(nint klass, nint klassc)
    {
        return (((delegate* unmanaged[Cdecl]<nint, nint, byte>)s_il2cpp_class_has_parent)(klass, klassc) != 0);
    }

    public static nint il2cpp_class_from_il2cpp_type(nint type)
    {
        return ((delegate* unmanaged[Cdecl]<nint, nint>)s_il2cpp_class_from_il2cpp_type)(type);
    }

    public static nint il2cpp_class_from_name(nint image, string namespaze, string name)
    {
        nint __m_namespaze = Marshal.StringToCoTaskMemUTF8(namespaze);
        try
        {
            nint __m_name = Marshal.StringToCoTaskMemUTF8(name);
            try
            {
                return ((delegate* unmanaged[Cdecl]<nint, byte*, byte*, nint>)s_il2cpp_class_from_name)(image,
                    (byte*)__m_namespaze, (byte*)__m_name);
            }
            finally
            {
                Marshal.FreeCoTaskMem(__m_name);
            }
        }
        finally
        {
            Marshal.FreeCoTaskMem(__m_namespaze);
        }
    }

    public static nint il2cpp_class_from_system_type(nint type)
    {
        return ((delegate* unmanaged[Cdecl]<nint, nint>)s_il2cpp_class_from_system_type)(type);
    }

    public static nint il2cpp_class_get_element_class(nint klass)
    {
        return ((delegate* unmanaged[Cdecl]<nint, nint>)s_il2cpp_class_get_element_class)(klass);
    }

    public static nint il2cpp_class_get_events(nint klass, ref nint iter)
    {
        return ((delegate* unmanaged[Cdecl]<nint, ref nint, nint>)s_il2cpp_class_get_events)(klass, ref iter);
    }

    public static nint il2cpp_class_get_fields(nint klass, ref nint iter)
    {
        return ((delegate* unmanaged[Cdecl]<nint, ref nint, nint>)s_il2cpp_class_get_fields)(klass, ref iter);
    }

    public static nint il2cpp_class_get_nested_types(nint klass, ref nint iter)
    {
        return ((delegate* unmanaged[Cdecl]<nint, ref nint, nint>)s_il2cpp_class_get_nested_types)(klass,
            ref iter);
    }

    public static nint il2cpp_class_get_interfaces(nint klass, ref nint iter)
    {
        return ((delegate* unmanaged[Cdecl]<nint, ref nint, nint>)s_il2cpp_class_get_interfaces)(klass, ref iter);
    }

    public static nint il2cpp_class_get_properties(nint klass, ref nint iter)
    {
        return ((delegate* unmanaged[Cdecl]<nint, ref nint, nint>)s_il2cpp_class_get_properties)(klass, ref iter);
    }

    public static nint il2cpp_class_get_property_from_name(nint klass, nint name)
    {
        return ((delegate* unmanaged[Cdecl]<nint, nint, nint>)s_il2cpp_class_get_property_from_name)(klass, name);
    }

    public static nint il2cpp_class_get_field_from_name(nint klass, string name)
    {
        nint __m_name = Marshal.StringToCoTaskMemUTF8(name);
        try
        {
            return ((delegate* unmanaged[Cdecl]<nint, byte*, nint>)s_il2cpp_class_get_field_from_name)(klass,
                (byte*)__m_name);
        }
        finally
        {
            Marshal.FreeCoTaskMem(__m_name);
        }
    }

    public static nint il2cpp_class_get_methods(nint klass, ref nint iter)
    {
        return ((delegate* unmanaged[Cdecl]<nint, ref nint, nint>)s_il2cpp_class_get_methods)(klass, ref iter);
    }

    public static nint il2cpp_class_get_method_from_name(nint klass, string name, int argsCount)
    {
        nint __m_name = Marshal.StringToCoTaskMemUTF8(name);
        try
        {
            return ((delegate* unmanaged[Cdecl]<nint, byte*, int, nint>)s_il2cpp_class_get_method_from_name)(klass,
                (byte*)__m_name, argsCount);
        }
        finally
        {
            Marshal.FreeCoTaskMem(__m_name);
        }
    }

    public static nint il2cpp_class_get_name(nint klass)
    {
        return ((delegate* unmanaged[Cdecl]<nint, nint>)s_il2cpp_class_get_name)(klass);
    }

    public static string? il2cpp_class_get_name_(nint klass)
        => Marshal.PtrToStringUTF8(il2cpp_class_get_name(klass));

    public static nint il2cpp_class_get_namespace(nint klass)
    {
        return ((delegate* unmanaged[Cdecl]<nint, nint>)s_il2cpp_class_get_namespace)(klass);
    }

    public static string? il2cpp_class_get_namespace_(nint klass)
        => Marshal.PtrToStringUTF8(il2cpp_class_get_namespace(klass));

    public static nint il2cpp_class_get_parent(nint klass)
    {
        return ((delegate* unmanaged[Cdecl]<nint, nint>)s_il2cpp_class_get_parent)(klass);
    }

    public static nint il2cpp_class_get_declaring_type(nint klass)
    {
        return ((delegate* unmanaged[Cdecl]<nint, nint>)s_il2cpp_class_get_declaring_type)(klass);
    }

    public static int il2cpp_class_instance_size(nint klass)
    {
        return ((delegate* unmanaged[Cdecl]<nint, int>)s_il2cpp_class_instance_size)(klass);
    }

    public static uint il2cpp_class_num_fields(nint enumKlass)
    {
        return ((delegate* unmanaged[Cdecl]<nint, uint>)s_il2cpp_class_num_fields)(enumKlass);
    }

    public static bool il2cpp_class_is_valuetype(nint klass)
    {
        return (((delegate* unmanaged[Cdecl]<nint, byte>)s_il2cpp_class_is_valuetype)(klass) != 0);
    }

    public static int il2cpp_class_value_size(nint klass, ref uint align)
    {
        return ((delegate* unmanaged[Cdecl]<nint, ref uint, int>)s_il2cpp_class_value_size)(klass, ref align);
    }

    public static bool il2cpp_class_is_blittable(nint klass)
    {
        return (((delegate* unmanaged[Cdecl]<nint, byte>)s_il2cpp_class_is_blittable)(klass) != 0);
    }

    public static int il2cpp_class_get_flags(nint klass)
    {
        return ((delegate* unmanaged[Cdecl]<nint, int>)s_il2cpp_class_get_flags)(klass);
    }

    public static bool il2cpp_class_is_abstract(nint klass)
    {
        return (((delegate* unmanaged[Cdecl]<nint, byte>)s_il2cpp_class_is_abstract)(klass) != 0);
    }

    public static bool il2cpp_class_is_interface(nint klass)
    {
        return (((delegate* unmanaged[Cdecl]<nint, byte>)s_il2cpp_class_is_interface)(klass) != 0);
    }

    public static int il2cpp_class_array_element_size(nint klass)
    {
        return ((delegate* unmanaged[Cdecl]<nint, int>)s_il2cpp_class_array_element_size)(klass);
    }

    public static nint il2cpp_class_from_type(nint type)
    {
        return ((delegate* unmanaged[Cdecl]<nint, nint>)s_il2cpp_class_from_type)(type);
    }

    public static nint il2cpp_class_get_type(nint klass)
    {
        return ((delegate* unmanaged[Cdecl]<nint, nint>)s_il2cpp_class_get_type)(klass);
    }

    public static uint il2cpp_class_get_type_token(nint klass)
    {
        return ((delegate* unmanaged[Cdecl]<nint, uint>)s_il2cpp_class_get_type_token)(klass);
    }

    public static bool il2cpp_class_has_attribute(nint klass, nint attr_class)
    {
        return (((delegate* unmanaged[Cdecl]<nint, nint, byte>)s_il2cpp_class_has_attribute)(klass, attr_class) !=
                0);
    }

    public static bool il2cpp_class_has_references(nint klass)
    {
        return (((delegate* unmanaged[Cdecl]<nint, byte>)s_il2cpp_class_has_references)(klass) != 0);
    }

    public static bool il2cpp_class_is_enum(nint klass)
    {
        return (((delegate* unmanaged[Cdecl]<nint, byte>)s_il2cpp_class_is_enum)(klass) != 0);
    }

    public static nint il2cpp_class_get_image(nint klass)
    {
        return ((delegate* unmanaged[Cdecl]<nint, nint>)s_il2cpp_class_get_image)(klass);
    }

    public static nint il2cpp_class_get_assemblyname(nint klass)
    {
        return ((delegate* unmanaged[Cdecl]<nint, nint>)s_il2cpp_class_get_assemblyname)(klass);
    }

    public static string? il2cpp_class_get_assemblyname_(nint klass)
        => Marshal.PtrToStringUTF8(il2cpp_class_get_assemblyname(klass));

    public static int il2cpp_class_get_rank(nint klass)
    {
        return ((delegate* unmanaged[Cdecl]<nint, int>)s_il2cpp_class_get_rank)(klass);
    }

    public static uint il2cpp_class_get_bitmap_size(nint klass)
    {
        return ((delegate* unmanaged[Cdecl]<nint, uint>)s_il2cpp_class_get_bitmap_size)(klass);
    }

    public static void il2cpp_class_get_bitmap(nint klass, ref uint bitmap)
    {
        ((delegate* unmanaged[Cdecl]<nint, ref uint, void>)s_il2cpp_class_get_bitmap)(klass, ref bitmap);
    }

    public static bool il2cpp_stats_dump_to_file(nint path)
    {
        return (((delegate* unmanaged[Cdecl]<nint, byte>)s_il2cpp_stats_dump_to_file)(path) != 0);
    }

    public static nint il2cpp_domain_get()
    {
        return ((delegate* unmanaged[Cdecl]<nint>)s_il2cpp_domain_get)();
    }

    public static nint il2cpp_domain_assembly_open(nint domain, nint name)
    {
        return ((delegate* unmanaged[Cdecl]<nint, nint, nint>)s_il2cpp_domain_assembly_open)(domain, name);
    }

    public static nint* il2cpp_domain_get_assemblies(nint domain, ref uint size)
    {
        return ((delegate* unmanaged[Cdecl]<nint, ref uint, nint*>)s_il2cpp_domain_get_assemblies)(domain,
            ref size);
    }

    public static nint il2cpp_exception_from_name_msg(nint image, nint name_space, nint name, nint msg)
    {
        return ((delegate* unmanaged[Cdecl]<nint, nint, nint, nint, nint>)s_il2cpp_exception_from_name_msg)(
            image, name_space, name, msg);
    }

    public static nint il2cpp_get_exception_argument_null(nint arg)
    {
        return ((delegate* unmanaged[Cdecl]<nint, nint>)s_il2cpp_get_exception_argument_null)(arg);
    }

    public static void il2cpp_format_exception(nint ex, void* message, int message_size)
    {
        ((delegate* unmanaged[Cdecl]<nint, void*, int, void>)s_il2cpp_format_exception)(ex, message, message_size);
    }

    public static void il2cpp_format_stack_trace(nint ex, void* output, int output_size)
    {
        ((delegate* unmanaged[Cdecl]<nint, void*, int, void>)s_il2cpp_format_stack_trace)(ex, output, output_size);
    }

    public static void il2cpp_unhandled_exception(nint ex)
    {
        ((delegate* unmanaged[Cdecl]<nint, void>)s_il2cpp_unhandled_exception)(ex);
    }

    public static int il2cpp_field_get_flags(nint field)
    {
        return ((delegate* unmanaged[Cdecl]<nint, int>)s_il2cpp_field_get_flags)(field);
    }

    public static nint il2cpp_field_get_name(nint field)
    {
        return ((delegate* unmanaged[Cdecl]<nint, nint>)s_il2cpp_field_get_name)(field);
    }

    public static string? il2cpp_field_get_name_(nint field)
        => Marshal.PtrToStringUTF8(il2cpp_field_get_name(field));

    public static nint il2cpp_field_get_parent(nint field)
    {
        return ((delegate* unmanaged[Cdecl]<nint, nint>)s_il2cpp_field_get_parent)(field);
    }

    public static uint il2cpp_field_get_offset(nint field)
    {
        return ((delegate* unmanaged[Cdecl]<nint, uint>)s_il2cpp_field_get_offset)(field);
    }

    public static nint il2cpp_field_get_type(nint field)
    {
        return ((delegate* unmanaged[Cdecl]<nint, nint>)s_il2cpp_field_get_type)(field);
    }

    public static void il2cpp_field_get_value(nint obj, nint field, void* value)
    {
        ((delegate* unmanaged[Cdecl]<nint, nint, void*, void>)s_il2cpp_field_get_value)(obj, field, value);
    }

    public static nint il2cpp_field_get_value_object(nint field, nint obj)
    {
        return ((delegate* unmanaged[Cdecl]<nint, nint, nint>)s_il2cpp_field_get_value_object)(field, obj);
    }

    public static bool il2cpp_field_has_attribute(nint field, nint attr_class)
    {
        return (((delegate* unmanaged[Cdecl]<nint, nint, byte>)s_il2cpp_field_has_attribute)(field, attr_class) !=
                0);
    }

    public static void il2cpp_field_set_value(nint obj, nint field, void* value)
    {
        ((delegate* unmanaged[Cdecl]<nint, nint, void*, void>)s_il2cpp_field_set_value)(obj, field, value);
    }

    public static void il2cpp_field_static_get_value(nint field, void* value)
    {
        ((delegate* unmanaged[Cdecl]<nint, void*, void>)s_il2cpp_field_static_get_value)(field, value);
    }

    public static void il2cpp_field_static_set_value(nint field, void* value)
    {
        ((delegate* unmanaged[Cdecl]<nint, void*, void>)s_il2cpp_field_static_set_value)(field, value);
    }

    public static void il2cpp_field_set_value_object(nint instance, nint field, nint value)
    {
        ((delegate* unmanaged[Cdecl]<nint, nint, nint, void>)s_il2cpp_field_set_value_object)(instance, field,
            value);
    }

    public static void il2cpp_gc_collect(int maxGenerations)
    {
        ((delegate* unmanaged[Cdecl]<int, void>)s_il2cpp_gc_collect)(maxGenerations);
    }

    public static int il2cpp_gc_collect_a_little()
    {
        return ((delegate* unmanaged[Cdecl]<int>)s_il2cpp_gc_collect_a_little)();
    }

    public static void il2cpp_gc_disable()
    {
        ((delegate* unmanaged[Cdecl]<void>)s_il2cpp_gc_disable)();
    }

    public static void il2cpp_gc_enable()
    {
        ((delegate* unmanaged[Cdecl]<void>)s_il2cpp_gc_enable)();
    }

    public static bool il2cpp_gc_is_disabled()
    {
        return (((delegate* unmanaged[Cdecl]<byte>)s_il2cpp_gc_is_disabled)() != 0);
    }

    public static long il2cpp_gc_get_used_size()
    {
        return ((delegate* unmanaged[Cdecl]<long>)s_il2cpp_gc_get_used_size)();
    }

    public static long il2cpp_gc_get_heap_size()
    {
        return ((delegate* unmanaged[Cdecl]<long>)s_il2cpp_gc_get_heap_size)();
    }

    public static void il2cpp_gc_wbarrier_set_field(nint obj, nint targetAddress, nint gcObj)
    {
        ((delegate* unmanaged[Cdecl]<nint, nint, nint, void>)s_il2cpp_gc_wbarrier_set_field)(obj, targetAddress,
            gcObj);
    }

    public static nint il2cpp_gchandle_new(nint obj, bool pinned)
    {
        return ((delegate* unmanaged[Cdecl]<nint, byte, nint>)s_il2cpp_gchandle_new)(obj, (byte)(pinned ? 1 : 0));
    }

    public static nint il2cpp_gchandle_new_weakref(nint obj, bool track_resurrection)
    {
        return ((delegate* unmanaged[Cdecl]<nint, byte, nint>)s_il2cpp_gchandle_new_weakref)(obj,
            (byte)(track_resurrection ? 1 : 0));
    }

    public static nint il2cpp_gchandle_get_target(nint gchandle)
    {
        return ((delegate* unmanaged[Cdecl]<nint, nint>)s_il2cpp_gchandle_get_target)(gchandle);
    }

    public static void il2cpp_gchandle_free(nint gchandle)
    {
        ((delegate* unmanaged[Cdecl]<nint, void>)s_il2cpp_gchandle_free)(gchandle);
    }

    public static nint il2cpp_unity_liveness_calculation_begin(nint filter, int max_object_count, nint callback,
        nint userdata, nint onWorldStarted, nint onWorldStopped)
    {
        return ((delegate* unmanaged[Cdecl]<nint, int, nint, nint, nint, nint, nint>)
            s_il2cpp_unity_liveness_calculation_begin)(filter, max_object_count, callback, userdata, onWorldStarted,
            onWorldStopped);
    }

    public static void il2cpp_unity_liveness_calculation_end(nint state)
    {
        ((delegate* unmanaged[Cdecl]<nint, void>)s_il2cpp_unity_liveness_calculation_end)(state);
    }

    public static void il2cpp_unity_liveness_calculation_from_root(nint root, nint state)
    {
        ((delegate* unmanaged[Cdecl]<nint, nint, void>)s_il2cpp_unity_liveness_calculation_from_root)(root, state);
    }

    public static void il2cpp_unity_liveness_calculation_from_statics(nint state)
    {
        ((delegate* unmanaged[Cdecl]<nint, void>)s_il2cpp_unity_liveness_calculation_from_statics)(state);
    }

    public static nint il2cpp_method_get_return_type(nint method)
    {
        return ((delegate* unmanaged[Cdecl]<nint, nint>)s_il2cpp_method_get_return_type)(method);
    }

    public static nint il2cpp_method_get_declaring_type(nint method)
    {
        return ((delegate* unmanaged[Cdecl]<nint, nint>)s_il2cpp_method_get_declaring_type)(method);
    }

    public static nint il2cpp_method_get_name(nint method)
    {
        return ((delegate* unmanaged[Cdecl]<nint, nint>)s_il2cpp_method_get_name)(method);
    }

    public static string? il2cpp_method_get_name_(nint method)
        => Marshal.PtrToStringUTF8(il2cpp_method_get_name(method));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static nint il2cpp_method_get_from_reflection(nint method)
    {
        if (UnityVersionHandler.HasGetMethodFromReflection) return _il2cpp_method_get_from_reflection(method);
        Il2CppReflectionMethod* reflectionMethod = (Il2CppReflectionMethod*)method;
        return (nint)reflectionMethod->method;
    }

    private static nint _il2cpp_method_get_from_reflection(nint method)
    {
        return ((delegate* unmanaged[Cdecl]<nint, nint>)s__il2cpp_method_get_from_reflection)(method);
    }

    public static nint il2cpp_method_get_object(nint method, nint refclass)
    {
        return ((delegate* unmanaged[Cdecl]<nint, nint, nint>)s_il2cpp_method_get_object)(method, refclass);
    }

    public static bool il2cpp_method_is_generic(nint method)
    {
        return (((delegate* unmanaged[Cdecl]<nint, byte>)s_il2cpp_method_is_generic)(method) != 0);
    }

    public static bool il2cpp_method_is_inflated(nint method)
    {
        return (((delegate* unmanaged[Cdecl]<nint, byte>)s_il2cpp_method_is_inflated)(method) != 0);
    }

    public static bool il2cpp_method_is_instance(nint method)
    {
        return (((delegate* unmanaged[Cdecl]<nint, byte>)s_il2cpp_method_is_instance)(method) != 0);
    }

    public static uint il2cpp_method_get_param_count(nint method)
    {
        return ((delegate* unmanaged[Cdecl]<nint, uint>)s_il2cpp_method_get_param_count)(method);
    }

    public static nint il2cpp_method_get_param(nint method, uint index)
    {
        return ((delegate* unmanaged[Cdecl]<nint, uint, nint>)s_il2cpp_method_get_param)(method, index);
    }

    public static nint il2cpp_method_get_class(nint method)
    {
        return ((delegate* unmanaged[Cdecl]<nint, nint>)s_il2cpp_method_get_class)(method);
    }

    public static bool il2cpp_method_has_attribute(nint method, nint attr_class)
    {
        return (((delegate* unmanaged[Cdecl]<nint, nint, byte>)s_il2cpp_method_has_attribute)(method, attr_class) !=
                0);
    }

    public static uint il2cpp_method_get_flags(nint method, ref uint iflags)
    {
        return ((delegate* unmanaged[Cdecl]<nint, ref uint, uint>)s_il2cpp_method_get_flags)(method, ref iflags);
    }

    public static uint il2cpp_method_get_token(nint method)
    {
        return ((delegate* unmanaged[Cdecl]<nint, uint>)s_il2cpp_method_get_token)(method);
    }

    public static nint il2cpp_method_get_param_name(nint method, uint index)
    {
        return ((delegate* unmanaged[Cdecl]<nint, uint, nint>)s_il2cpp_method_get_param_name)(method, index);
    }

    public static string? il2cpp_method_get_param_name_(nint method, uint index)
        => Marshal.PtrToStringUTF8(il2cpp_method_get_param_name(method, index));

    public static void il2cpp_profiler_install(nint prof, nint shutdown_callback)
    {
        ((delegate* unmanaged[Cdecl]<nint, nint, void>)s_il2cpp_profiler_install)(prof, shutdown_callback);
    }

    public static void il2cpp_profiler_install_enter_leave(nint enter, nint fleave)
    {
        ((delegate* unmanaged[Cdecl]<nint, nint, void>)s_il2cpp_profiler_install_enter_leave)(enter, fleave);
    }

    public static void il2cpp_profiler_install_allocation(nint callback)
    {
        ((delegate* unmanaged[Cdecl]<nint, void>)s_il2cpp_profiler_install_allocation)(callback);
    }

    public static void il2cpp_profiler_install_gc(nint callback, nint heap_resize_callback)
    {
        ((delegate* unmanaged[Cdecl]<nint, nint, void>)s_il2cpp_profiler_install_gc)(callback,
            heap_resize_callback);
    }

    public static void il2cpp_profiler_install_fileio(nint callback)
    {
        ((delegate* unmanaged[Cdecl]<nint, void>)s_il2cpp_profiler_install_fileio)(callback);
    }

    public static void il2cpp_profiler_install_thread(nint start, nint end)
    {
        ((delegate* unmanaged[Cdecl]<nint, nint, void>)s_il2cpp_profiler_install_thread)(start, end);
    }

    public static uint il2cpp_property_get_flags(nint prop)
    {
        return ((delegate* unmanaged[Cdecl]<nint, uint>)s_il2cpp_property_get_flags)(prop);
    }

    public static nint il2cpp_property_get_get_method(nint prop)
    {
        return ((delegate* unmanaged[Cdecl]<nint, nint>)s_il2cpp_property_get_get_method)(prop);
    }

    public static nint il2cpp_property_get_set_method(nint prop)
    {
        return ((delegate* unmanaged[Cdecl]<nint, nint>)s_il2cpp_property_get_set_method)(prop);
    }

    public static nint il2cpp_property_get_name(nint prop)
    {
        return ((delegate* unmanaged[Cdecl]<nint, nint>)s_il2cpp_property_get_name)(prop);
    }

    public static string? il2cpp_property_get_name_(nint prop)
        => Marshal.PtrToStringUTF8(il2cpp_property_get_name(prop));

    public static nint il2cpp_property_get_parent(nint prop)
    {
        return ((delegate* unmanaged[Cdecl]<nint, nint>)s_il2cpp_property_get_parent)(prop);
    }

    public static nint il2cpp_object_get_class(nint obj)
    {
        return ((delegate* unmanaged[Cdecl]<nint, nint>)s_il2cpp_object_get_class)(obj);
    }

    public static uint il2cpp_object_get_size(nint obj)
    {
        return ((delegate* unmanaged[Cdecl]<nint, uint>)s_il2cpp_object_get_size)(obj);
    }

    public static nint il2cpp_object_get_virtual_method(nint obj, nint method)
    {
        return ((delegate* unmanaged[Cdecl]<nint, nint, nint>)s_il2cpp_object_get_virtual_method)(obj, method);
    }

    public static nint il2cpp_object_new(nint klass)
    {
        return ((delegate* unmanaged[Cdecl]<nint, nint>)s_il2cpp_object_new)(klass);
    }

    public static nint il2cpp_object_unbox(nint obj)
    {
        return ((delegate* unmanaged[Cdecl]<nint, nint>)s_il2cpp_object_unbox)(obj);
    }

    public static nint il2cpp_value_box(nint klass, nint data)
    {
        return ((delegate* unmanaged[Cdecl]<nint, nint, nint>)s_il2cpp_value_box)(klass, data);
    }

    public static void il2cpp_monitor_enter(nint obj)
    {
        ((delegate* unmanaged[Cdecl]<nint, void>)s_il2cpp_monitor_enter)(obj);
    }

    public static bool il2cpp_monitor_try_enter(nint obj, uint timeout)
    {
        return (((delegate* unmanaged[Cdecl]<nint, uint, byte>)s_il2cpp_monitor_try_enter)(obj, timeout) != 0);
    }

    public static void il2cpp_monitor_exit(nint obj)
    {
        ((delegate* unmanaged[Cdecl]<nint, void>)s_il2cpp_monitor_exit)(obj);
    }

    public static void il2cpp_monitor_pulse(nint obj)
    {
        ((delegate* unmanaged[Cdecl]<nint, void>)s_il2cpp_monitor_pulse)(obj);
    }

    public static void il2cpp_monitor_pulse_all(nint obj)
    {
        ((delegate* unmanaged[Cdecl]<nint, void>)s_il2cpp_monitor_pulse_all)(obj);
    }

    public static void il2cpp_monitor_wait(nint obj)
    {
        ((delegate* unmanaged[Cdecl]<nint, void>)s_il2cpp_monitor_wait)(obj);
    }

    public static bool il2cpp_monitor_try_wait(nint obj, uint timeout)
    {
        return (((delegate* unmanaged[Cdecl]<nint, uint, byte>)s_il2cpp_monitor_try_wait)(obj, timeout) != 0);
    }

    public static nint il2cpp_runtime_invoke(nint method, nint obj, void** param, ref nint exc)
    {
        return ((delegate* unmanaged[Cdecl]<nint, nint, void**, ref nint, nint>)s_il2cpp_runtime_invoke)(method,
            obj, param, ref exc);
    }

    // param can be of Il2CppObject*
    public static nint il2cpp_runtime_invoke_convert_args(nint method, nint obj, void** param, int paramCount,
        ref nint exc)
    {
        return ((delegate* unmanaged[Cdecl]<nint, nint, void**, int, ref nint, nint>)
            s_il2cpp_runtime_invoke_convert_args)(method, obj, param, paramCount, ref exc);
    }

    public static void il2cpp_runtime_class_init(nint klass)
    {
        ((delegate* unmanaged[Cdecl]<nint, void>)s_il2cpp_runtime_class_init)(klass);
    }

    public static void il2cpp_runtime_object_init(nint obj)
    {
        ((delegate* unmanaged[Cdecl]<nint, void>)s_il2cpp_runtime_object_init)(obj);
    }

    public static void il2cpp_runtime_object_init_exception(nint obj, ref nint exc)
    {
        ((delegate* unmanaged[Cdecl]<nint, ref nint, void>)s_il2cpp_runtime_object_init_exception)(obj, ref exc);
    }

    public static int il2cpp_string_length(nint str)
    {
        return ((delegate* unmanaged[Cdecl]<nint, int>)s_il2cpp_string_length)(str);
    }

    public static char* il2cpp_string_chars(nint str)
    {
        return ((delegate* unmanaged[Cdecl]<nint, char*>)s_il2cpp_string_chars)(str);
    }

    public static nint il2cpp_string_new(string str)
    {
        nint __m_str = Marshal.StringToHGlobalAnsi(str);
        try
        {
            return ((delegate* unmanaged[Cdecl]<byte*, nint>)s_il2cpp_string_new)((byte*)__m_str);
        }
        finally
        {
            Marshal.FreeHGlobal(__m_str);
        }
    }

    public static nint il2cpp_string_new_len(string str, uint length)
    {
        nint __m_str = Marshal.StringToHGlobalAnsi(str);
        try
        {
            return ((delegate* unmanaged[Cdecl]<byte*, uint, nint>)s_il2cpp_string_new_len)((byte*)__m_str, length);
        }
        finally
        {
            Marshal.FreeHGlobal(__m_str);
        }
    }

    public static nint il2cpp_string_new_utf16(char* text, int len)
    {
        return ((delegate* unmanaged[Cdecl]<char*, int, nint>)s_il2cpp_string_new_utf16)(text, len);
    }

    public static nint il2cpp_string_new_wrapper(string str)
    {
        nint __m_str = Marshal.StringToHGlobalAnsi(str);
        try
        {
            return ((delegate* unmanaged[Cdecl]<byte*, nint>)s_il2cpp_string_new_wrapper)((byte*)__m_str);
        }
        finally
        {
            Marshal.FreeHGlobal(__m_str);
        }
    }

    public static nint il2cpp_string_intern(string str)
    {
        nint __m_str = Marshal.StringToHGlobalAnsi(str);
        try
        {
            return ((delegate* unmanaged[Cdecl]<byte*, nint>)s_il2cpp_string_intern)((byte*)__m_str);
        }
        finally
        {
            Marshal.FreeHGlobal(__m_str);
        }
    }

    public static nint il2cpp_string_is_interned(string str)
    {
        nint __m_str = Marshal.StringToHGlobalAnsi(str);
        try
        {
            return ((delegate* unmanaged[Cdecl]<byte*, nint>)s_il2cpp_string_is_interned)((byte*)__m_str);
        }
        finally
        {
            Marshal.FreeHGlobal(__m_str);
        }
    }

    public static nint il2cpp_thread_current()
    {
        return ((delegate* unmanaged[Cdecl]<nint>)s_il2cpp_thread_current)();
    }

    public static nint il2cpp_thread_attach(nint domain)
    {
        return ((delegate* unmanaged[Cdecl]<nint, nint>)s_il2cpp_thread_attach)(domain);
    }

    public static void il2cpp_thread_detach(nint thread)
    {
        ((delegate* unmanaged[Cdecl]<nint, void>)s_il2cpp_thread_detach)(thread);
    }

    public static void** il2cpp_thread_get_all_attached_threads(ref uint size)
    {
        return ((delegate* unmanaged[Cdecl]<ref uint, void**>)s_il2cpp_thread_get_all_attached_threads)(ref size);
    }

    public static bool il2cpp_is_vm_thread(nint thread)
    {
        return (((delegate* unmanaged[Cdecl]<nint, byte>)s_il2cpp_is_vm_thread)(thread) != 0);
    }

    public static void il2cpp_current_thread_walk_frame_stack(nint func, nint user_data)
    {
        ((delegate* unmanaged[Cdecl]<nint, nint, void>)s_il2cpp_current_thread_walk_frame_stack)(func, user_data);
    }

    public static void il2cpp_thread_walk_frame_stack(nint thread, nint func, nint user_data)
    {
        ((delegate* unmanaged[Cdecl]<nint, nint, nint, void>)s_il2cpp_thread_walk_frame_stack)(thread, func,
            user_data);
    }

    public static bool il2cpp_current_thread_get_top_frame(nint frame)
    {
        return (((delegate* unmanaged[Cdecl]<nint, byte>)s_il2cpp_current_thread_get_top_frame)(frame) != 0);
    }

    public static bool il2cpp_thread_get_top_frame(nint thread, nint frame)
    {
        return (((delegate* unmanaged[Cdecl]<nint, nint, byte>)s_il2cpp_thread_get_top_frame)(thread, frame) != 0);
    }

    public static bool il2cpp_current_thread_get_frame_at(int offset, nint frame)
    {
        return (((delegate* unmanaged[Cdecl]<int, nint, byte>)s_il2cpp_current_thread_get_frame_at)(offset, frame) !=
                0);
    }

    public static bool il2cpp_thread_get_frame_at(nint thread, int offset, nint frame)
    {
        return (((delegate* unmanaged[Cdecl]<nint, int, nint, byte>)s_il2cpp_thread_get_frame_at)(thread, offset,
            frame) != 0);
    }

    public static int il2cpp_current_thread_get_stack_depth()
    {
        return ((delegate* unmanaged[Cdecl]<int>)s_il2cpp_current_thread_get_stack_depth)();
    }

    public static int il2cpp_thread_get_stack_depth(nint thread)
    {
        return ((delegate* unmanaged[Cdecl]<nint, int>)s_il2cpp_thread_get_stack_depth)(thread);
    }

    public static nint il2cpp_type_get_object(nint type)
    {
        return ((delegate* unmanaged[Cdecl]<nint, nint>)s_il2cpp_type_get_object)(type);
    }

    public static int il2cpp_type_get_type(nint type)
    {
        return ((delegate* unmanaged[Cdecl]<nint, int>)s_il2cpp_type_get_type)(type);
    }

    public static nint il2cpp_type_get_class_or_element_class(nint type)
    {
        return ((delegate* unmanaged[Cdecl]<nint, nint>)s_il2cpp_type_get_class_or_element_class)(type);
    }

    public static nint il2cpp_type_get_name(nint type)
    {
        return ((delegate* unmanaged[Cdecl]<nint, nint>)s_il2cpp_type_get_name)(type);
    }

    public static string? il2cpp_type_get_name_(nint type)
        => Marshal.PtrToStringUTF8(il2cpp_type_get_name(type));

    public static bool il2cpp_type_is_byref(nint type)
    {
        return (((delegate* unmanaged[Cdecl]<nint, byte>)s_il2cpp_type_is_byref)(type) != 0);
    }

    public static uint il2cpp_type_get_attrs(nint type)
    {
        return ((delegate* unmanaged[Cdecl]<nint, uint>)s_il2cpp_type_get_attrs)(type);
    }

    public static bool il2cpp_type_equals(nint type, nint otherType)
    {
        return (((delegate* unmanaged[Cdecl]<nint, nint, byte>)s_il2cpp_type_equals)(type, otherType) != 0);
    }

    public static nint il2cpp_type_get_assembly_qualified_name(nint type)
    {
        return ((delegate* unmanaged[Cdecl]<nint, nint>)s_il2cpp_type_get_assembly_qualified_name)(type);
    }

    public static nint il2cpp_image_get_assembly(nint image)
    {
        return ((delegate* unmanaged[Cdecl]<nint, nint>)s_il2cpp_image_get_assembly)(image);
    }

    public static nint il2cpp_image_get_name(nint image)
    {
        return ((delegate* unmanaged[Cdecl]<nint, nint>)s_il2cpp_image_get_name)(image);
    }

    public static string? il2cpp_image_get_name_(nint image)
        => Marshal.PtrToStringUTF8(il2cpp_image_get_name(image));

    public static nint il2cpp_image_get_filename(nint image)
    {
        return ((delegate* unmanaged[Cdecl]<nint, nint>)s_il2cpp_image_get_filename)(image);
    }

    public static string? il2cpp_image_get_filename_(nint image)
        => Marshal.PtrToStringUTF8(il2cpp_image_get_filename(image));

    public static nint il2cpp_image_get_entry_point(nint image)
    {
        return ((delegate* unmanaged[Cdecl]<nint, nint>)s_il2cpp_image_get_entry_point)(image);
    }

    public static uint il2cpp_image_get_class_count(nint image)
    {
        return ((delegate* unmanaged[Cdecl]<nint, uint>)s_il2cpp_image_get_class_count)(image);
    }

    public static nint il2cpp_image_get_class(nint image, uint index)
    {
        return ((delegate* unmanaged[Cdecl]<nint, uint, nint>)s_il2cpp_image_get_class)(image, index);
    }

    public static nint il2cpp_capture_memory_snapshot()
    {
        return ((delegate* unmanaged[Cdecl]<nint>)s_il2cpp_capture_memory_snapshot)();
    }

    public static void il2cpp_free_captured_memory_snapshot(nint snapshot)
    {
        ((delegate* unmanaged[Cdecl]<nint, void>)s_il2cpp_free_captured_memory_snapshot)(snapshot);
    }

    public static void il2cpp_set_find_plugin_callback(nint method)
    {
        ((delegate* unmanaged[Cdecl]<nint, void>)s_il2cpp_set_find_plugin_callback)(method);
    }

    public static void il2cpp_register_log_callback(nint method)
    {
        ((delegate* unmanaged[Cdecl]<nint, void>)s_il2cpp_register_log_callback)(method);
    }

    public static void il2cpp_debugger_set_agent_options(nint options)
    {
        ((delegate* unmanaged[Cdecl]<nint, void>)s_il2cpp_debugger_set_agent_options)(options);
    }

    public static bool il2cpp_is_debugger_attached()
    {
        return (((delegate* unmanaged[Cdecl]<byte>)s_il2cpp_is_debugger_attached)() != 0);
    }

    public static void il2cpp_unity_install_unitytls_interface(void* unitytlsInterfaceStruct)
    {
        ((delegate* unmanaged[Cdecl]<void*, void>)s_il2cpp_unity_install_unitytls_interface)(unitytlsInterfaceStruct);
    }

    public static nint il2cpp_custom_attrs_from_class(nint klass)
    {
        return ((delegate* unmanaged[Cdecl]<nint, nint>)s_il2cpp_custom_attrs_from_class)(klass);
    }

    public static nint il2cpp_custom_attrs_from_method(nint method)
    {
        return ((delegate* unmanaged[Cdecl]<nint, nint>)s_il2cpp_custom_attrs_from_method)(method);
    }

    public static nint il2cpp_custom_attrs_get_attr(nint ainfo, nint attr_klass)
    {
        return ((delegate* unmanaged[Cdecl]<nint, nint, nint>)s_il2cpp_custom_attrs_get_attr)(ainfo, attr_klass);
    }

    public static bool il2cpp_custom_attrs_has_attr(nint ainfo, nint attr_klass)
    {
        return (((delegate* unmanaged[Cdecl]<nint, nint, byte>)s_il2cpp_custom_attrs_has_attr)(ainfo, attr_klass) !=
                0);
    }

    public static nint il2cpp_custom_attrs_construct(nint cinfo)
    {
        return ((delegate* unmanaged[Cdecl]<nint, nint>)s_il2cpp_custom_attrs_construct)(cinfo);
    }

    public static void il2cpp_custom_attrs_free(nint ainfo)
    {
        ((delegate* unmanaged[Cdecl]<nint, void>)s_il2cpp_custom_attrs_free)(ainfo);
    }
}
