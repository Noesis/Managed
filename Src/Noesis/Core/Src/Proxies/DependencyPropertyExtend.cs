using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace Noesis
{

    public partial class DependencyProperty
    {
        public static DependencyProperty Register(string name, Type propertyType,
            Type ownerType)
        {
            return RegisterCommon(name, propertyType, ownerType, null);
        }
        public static DependencyProperty Register(string name, Type propertyType,
            Type ownerType, PropertyMetadata typeMetadata)
        {
            return RegisterCommon(name, propertyType, ownerType, typeMetadata);
        }

        public static DependencyProperty RegisterAttached(string name, Type propertyType,
            Type ownerType)
        {
            return RegisterCommon(name, propertyType, ownerType, null);
        }

        public static DependencyProperty RegisterAttached(string name, Type propertyType,
            Type ownerType, PropertyMetadata defaultMetadata)
        {
            return RegisterCommon(name, propertyType, ownerType, defaultMetadata);
        }

        public void OverrideMetadata(Type forType, PropertyMetadata typeMetadata)
        {
            IntPtr forTypePtr = Noesis.Extend.EnsureNativeType(forType, false);

            Noesis_OverrideMetadata(forTypePtr, swigCPtr.Handle,
                PropertyMetadata.getCPtr(typeMetadata).Handle);
        }

        internal static bool RegisterCalled { get; set; }

        #region Register implementation

        private static DependencyProperty RegisterCommon(string name, Type propertyType,
            Type ownerType, PropertyMetadata propertyMetadata)
        {
            ValidateParams(name, propertyType, ownerType);

            // Force native type registration, but skip DP registration because we are inside
            // static constructor and DP are already being registered
            IntPtr ownerTypePtr = Noesis.Extend.EnsureNativeType(ownerType, false);

            // Check property type is supported and get the registered native type
            Type originalPropertyType = propertyType;
            IntPtr nativeType = ValidatePropertyType(ref propertyType);

            // Create and register dependency property
            IntPtr dependencyPtr = Noesis_RegisterDependencyProperty(ownerTypePtr,
                name, nativeType, PropertyMetadata.getCPtr(propertyMetadata).Handle);

            DependencyProperty dependencyProperty = new DependencyProperty(dependencyPtr, false);
            dependencyProperty.OriginalPropertyType = originalPropertyType;

            RegisterCalled = true;
            return dependencyProperty;
        }

        private static void ValidateParams(string name, Type propertyType,
            Type ownerType)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            if (name.Length == 0)
            {
                throw new ArgumentException("Property name can't be empty");
            }

            if (ownerType == null)
            {
                throw new ArgumentNullException("ownerType");
            }

            if (propertyType == null)
            {
                throw new ArgumentNullException("propertyType");
            }
        }

        private Type OriginalPropertyType { get; set; }

        private static IntPtr ValidatePropertyType(ref Type propertyType)
        {
            Type validType;
            if (_validTypes.TryGetValue(propertyType, out validType))
            {
                propertyType = validType;
            }

            return Noesis.Extend.EnsureNativeType(propertyType);
        }

        private static Dictionary<Type, Type> _validTypes = CreateValidTypes();

        private static Dictionary<Type, Type> CreateValidTypes()
        {
            Dictionary<Type, Type> validTypes = new Dictionary<Type, Type>(13);

            validTypes[typeof(decimal)] = typeof(double);
            validTypes[typeof(long)] = typeof(int);
            validTypes[typeof(ulong)] = typeof(uint);
            validTypes[typeof(char)] = typeof(uint);
            validTypes[typeof(sbyte)] = typeof(short);
            validTypes[typeof(byte)] = typeof(ushort);

            validTypes[typeof(decimal?)] = typeof(double?);
            validTypes[typeof(long?)] = typeof(int?);
            validTypes[typeof(ulong?)] = typeof(uint?);
            validTypes[typeof(char?)] = typeof(uint?);
            validTypes[typeof(sbyte?)] = typeof(short?);
            validTypes[typeof(byte?)] = typeof(ushort?);

            validTypes[typeof(Type)] = typeof(ResourceKeyType);

            return validTypes;
        }

        #endregion

        #region Imports

        static DependencyProperty()
        {
            Noesis.GUI.Init();
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        [DllImport(Library.Name)]
        private static extern IntPtr Noesis_RegisterDependencyProperty(IntPtr classType,
            [MarshalAs(UnmanagedType.LPStr)]string propertyName,
            IntPtr propertyType, IntPtr propertyMetadata);

        ////////////////////////////////////////////////////////////////////////////////////////////////
        [DllImport(Library.Name)]
        private static extern void Noesis_OverrideMetadata(IntPtr classType,
            IntPtr dependencyProperty, IntPtr propertyMetadata);

        #endregion
    }

}
