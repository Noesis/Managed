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
            if (forType == null)
            {
                throw new ArgumentNullException("forType");
            }
            if (typeMetadata == null)
            {
                throw new ArgumentNullException("typeMetadata");
            }
            if (!typeof(DependencyObject).IsAssignableFrom(forType))
            {
                throw new ArgumentException(string.Format(
                    "OverrideMetadata type '{0}' must inherit from DependencyObject", forType.FullName));
            }
            if (typeMetadata.HasDefaultValue)
            {
                ValidateDefaultValue(Name, PropertyType, OwnerType, typeMetadata.DefaultValue);
            }

            DependencyPropertyMetadataOverrideResurrectionManager.Register(this, forType, typeMetadata);

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

            ValidateMetadata(name, propertyType, ownerType, ref propertyMetadata);

            // Create and register dependency property
            IntPtr dependencyPtr = Noesis_RegisterDependencyProperty(ownerTypePtr,
                name, nativeType, PropertyMetadata.getCPtr(propertyMetadata).Handle);

            if (DependencyPropertyResurrectionManager.TryResurrect(dependencyPtr, name, ownerType, 
                                                         out var existingDependencyProperty))
            {
                return existingDependencyProperty;
            }

            DependencyProperty dependencyProperty = new DependencyProperty(dependencyPtr, false);
            dependencyProperty.OriginalPropertyType = originalPropertyType;
            
            DependencyPropertyResurrectionManager.Register(dependencyProperty, name, propertyType, ownerType, propertyMetadata);

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

        private static void ValidateMetadata(string name, Type type, Type owner, ref PropertyMetadata metadata)
        {
            if (metadata == null)
            {
                metadata = new PropertyMetadata(GenerateDefaultValue(type));
            }
            else if (!metadata.HasDefaultValue || (type == typeof(string) && metadata.DefaultValue == null))
            {
                metadata.DefaultValue = GenerateDefaultValue(type);
            }
            else
            {
                ValidateDefaultValue(name, type, owner, metadata.DefaultValue);
            }
        }

        private static object GenerateDefaultValue(Type type)
        {
            if (type == typeof(string))
            {
                return string.Empty;
            }
            if (type.IsValueType)
            {
                return Activator.CreateInstance(type);
            }
            return null;
        }

        private static void ValidateDefaultValue(string name, Type type, Type owner, object value)
        {
            if (!IsValidType(value, type))
            {
                throw new ArgumentException(string.Format(
                    "Invalid default value type for dependency property '{0}.{1}'",
                    owner.FullName, name));
            }

            if (value is Expression)
            {
                throw new ArgumentException(string.Format(
                    "Dependency property '{0}.{1}' default value can't be an Expression",
                    owner.FullName, name));
            }

            DispatcherObject dispatcherObject = value as DispatcherObject;
            if (dispatcherObject != null && dispatcherObject.Dispatcher != null)
            {
                Freezable freezable = value as Freezable;
                if (freezable != null && freezable.CanFreeze)
                {
                    freezable.Freeze();
                    return;
                }

                throw new ArgumentException(string.Format(
                    "Dependency property '{0}.{1}' default value must be free threaded",
                    owner.FullName, name));
            }
        }

        private static bool IsValidType(object value, Type type)
        {
            if (value == null)
            {
                if (type.IsValueType && (!type.IsGenericType || !(type.GetGenericTypeDefinition() == NullableType)))
                {
                    return false;
                }
            }
            else if (!type.IsInstanceOfType(value) && (!type.IsEnum || !(value.GetType() == IntType)))
            {
                return false;
            }
            return true;
        }

        private static Type NullableType = typeof(Nullable<>);
        private static Type IntType = typeof(int);

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

            return validTypes;
        }

        #endregion

        #region Imports

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
