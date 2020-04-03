#if NETFX_CORE

namespace System.Runtime.InteropServices
{
    using System;

    [System.Runtime.InteropServices.ComVisible(true)]
    public struct HandleRef
    {
        // ! Do not add or rearrange fields as the EE depends on this layout.
        //------------------------------------------------------------------
        internal Object _wrapper;
        internal IntPtr _handle;
        //------------------------------------------------------------------

        public HandleRef(Object wrapper, IntPtr handle)
        {
            _wrapper = wrapper;
            _handle = handle;
        }

        public Object Wrapper
        {
            get { return _wrapper; }
        }

        public IntPtr Handle
        {
            get { return _handle; }
        }
    }
}

namespace System.ComponentModel
{
    // There is no [TypeConverter] attribute in WinRT
    [AttributeUsage(AttributeTargets.All)]
    public sealed class TypeConverterAttribute: Attribute
    {
        private string converter_type;

        public TypeConverterAttribute()
        {
            converter_type = "";
        }

        public TypeConverterAttribute(string typeName)
        {
            converter_type = typeName;
        }

        public TypeConverterAttribute(Type type)
        {
            converter_type = type.AssemblyQualifiedName;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is TypeConverterAttribute))
                return false;
            return ((TypeConverterAttribute) obj).ConverterTypeName == converter_type;
        }

        public override int GetHashCode()
        {
            return converter_type.GetHashCode ();
        }
        
        public string ConverterTypeName
        {
            get { return converter_type; }
        }
    }
}

namespace System.Security
{
    // There is no [SuppressUnmanagedCodeSecurity] attribute in WinRT
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Interface |
        AttributeTargets.Delegate, AllowMultiple = true, Inherited = false)]
    public sealed class SuppressUnmanagedCodeSecurityAttribute: Attribute {}
}

namespace System
{
    // SWIG uses (incorrectly) System.SystemException instead of System.Exception and there is no
    // SystemException class in WinRT
    public class SystemException : Exception
    {
        public SystemException(string message, Exception innerException) : base(message, innerException) { }
    }

    // Unity already defines this in WinRTLegacy assembly
    #if !UNITY_WSA_10_0 && !UNITY_WSA_8_1
    // SWIG uses System.ApplicationException and there is no such class in .NETCore
    public class ApplicationException : Exception
    {
        public ApplicationException() { }
        public ApplicationException(string message) : base(message) { }
        public ApplicationException(string message, Exception innerException) : base(message, innerException) { }
    }
    #endif
}

namespace Noesis
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    public class AssemblyLoadEventArgs : EventArgs
    {
        public AssemblyLoadEventArgs(Assembly loadedAssembly)
        {
            LoadedAssembly = loadedAssembly;
        }

        public Assembly LoadedAssembly { get; private set; }
    }

    public delegate void AssemblyLoadEventHandler(object sender, AssemblyLoadEventArgs args);

    sealed class AppDomain
    {
        public static AppDomain CurrentDomain { get; private set; }

        static AppDomain()
        {
            CurrentDomain = new AppDomain();
        }

        public Assembly[] GetAssemblies()
        {
            return GetAssemblyListAsync().Result.ToArray();
        }

        public event AssemblyLoadEventHandler AssemblyLoad;

        private async System.Threading.Tasks.Task<IEnumerable<Assembly>> GetAssemblyListAsync()
        {
            var folder = Windows.ApplicationModel.Package.Current.InstalledLocation;
            var assemblies = new List<Assembly>();

            foreach (var file in await folder.GetFilesAsync())
            {
                if (file.FileType == ".dll" || file.FileType == ".exe")
                {
                    try
                    {
                        // Loading UnityPlayer.dll causes an internal error in the .NET Runtime
                        var assemblyName = System.IO.Path.GetFileNameWithoutExtension(file.Name);
                        if (assemblyName != "UnityPlayer" && assemblyName != "BridgeInterface")
                        {
                            var assemblyRef = new AssemblyName() { Name = assemblyName };
                            assemblies.Add(Assembly.Load(assemblyRef));
                        }
                    }
                    catch { }
                }
            }

            // Add extra assemblies that are part of the system
            AddAssemblyFromType(typeof(ObservableCollection<bool>), assemblies);
            return assemblies;
        }

        private void AddAssemblyFromType(Type type, List<Assembly> assemblies)
        {
            Assembly assembly = type.GetTypeInfo().Assembly;
            if (assemblies.IndexOf(assembly) == -1)
            {
                assemblies.Add(assembly);
            }
        }
    }

    public static class ExtensionMethods
    {
        public static Type[] GetGenericArguments(this Type type)
        {
            return type.GetTypeInfo().GenericTypeArguments;
        }

        public static MethodInfo GetGetMethod(this PropertyInfo property)
        {
            return property.GetMethod;
        }

        public static MethodInfo GetSetMethod(this PropertyInfo property)
        {
            return property.SetMethod;
        }
    }
}

#else

namespace Noesis
{
    using System;
    using System.Reflection;
    using System.Runtime;
    using System.Collections.Generic;

    public static class ExtensionMethods
    {
        // In .NET 4.5 Type and TypeInfo are different classes. This extension method adapts the
        // version of Mono used in Unity to that API. That way we can mantain the same code without
        // using preprocessor
        public static Type GetTypeInfo(this Type type)
        {
            return type;
        }

        public static T GetCustomAttribute<T>(this MemberInfo member) where T: Attribute
        {
            return (T)Attribute.GetCustomAttribute(member, typeof(T));
        }

        public static Delegate CreateDelegate(this MethodInfo method, Type type)
        {
            return Delegate.CreateDelegate(type, method);
        }
    }

    public static class Marshal
    {
        public static void StructureToPtr(object structure, IntPtr ptr, bool fDeleteOld)
        {
            System.Runtime.InteropServices.Marshal.StructureToPtr(structure, ptr, fDeleteOld);
        }

        public static T PtrToStructure<T>(IntPtr ptr)
        {
            return (T)System.Runtime.InteropServices.Marshal.PtrToStructure(ptr, typeof(T));
        }

        public static IntPtr StringToHGlobalAnsi(string s)
        {
            return System.Runtime.InteropServices.Marshal.StringToHGlobalAnsi(s);
        }

        public static string PtrToStringAnsi(IntPtr ptr)
        {
            return System.Runtime.InteropServices.Marshal.PtrToStringAnsi(ptr);
        }

        public static IntPtr StringToHGlobalUni(string s)
        {
            return System.Runtime.InteropServices.Marshal.StringToHGlobalUni(s);
        }

        public static int SizeOf<T>()
        {
            return System.Runtime.InteropServices.Marshal.SizeOf(typeof(T));
        }

        public static IntPtr AllocHGlobal(int numBytes)
        {
            return System.Runtime.InteropServices.Marshal.AllocHGlobal(numBytes);
        }

        public static void FreeHGlobal(IntPtr hglobal)
        {
            System.Runtime.InteropServices.Marshal.FreeHGlobal(hglobal);
        }

        public static void Copy(IntPtr source, byte[] destination, int startIndex, int length)
        {
            System.Runtime.InteropServices.Marshal.Copy(source, destination, startIndex, length);
        }

        public static byte ReadByte(IntPtr ptr, int offset)
        {
            return System.Runtime.InteropServices.Marshal.ReadByte(ptr, offset);
        }

        public static IntPtr ReadIntPtr(IntPtr ptr, int ofs)
        {
            return System.Runtime.InteropServices.Marshal.ReadIntPtr(ptr, ofs);
        }

        public static void WriteInt32(IntPtr ptr, int ofs, int val)
        {
            System.Runtime.InteropServices.Marshal.WriteInt32(ptr, ofs, val);
        }

        public static void WriteInt64(IntPtr ptr, int ofs, long val)
        {
            System.Runtime.InteropServices.Marshal.WriteInt64(ptr, ofs, val);
        }

        public static void WriteIntPtr(IntPtr ptr, int ofs, IntPtr val)
        {
            System.Runtime.InteropServices.Marshal.WriteIntPtr(ptr, ofs, val);
        }

        public static Delegate GetDelegateForFunctionPointer(IntPtr ptr, Type t)
        {
            return System.Runtime.InteropServices.Marshal.GetDelegateForFunctionPointer(ptr, t);
        }

        public static int GetLastWin32Error()
        {
            return System.Runtime.InteropServices.Marshal.GetLastWin32Error();
        }
    }
}

#endif
