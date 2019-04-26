using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Reflection;
using System.Globalization;
using System.Linq;

namespace Noesis
{
    ////////////////////////////////////////////////////////////////////////////////////////////////
    // Manages Noesis Extensibility
    ////////////////////////////////////////////////////////////////////////////////////////////////
    internal partial class Extend
    {
        #region Property accessors

        #region Normal properties

        public class PropertyAccessor
        {
            PropertyInfo _property;

            public PropertyAccessor(PropertyInfo p) { _property = p; }
            public PropertyInfo Property { get { return _property; } }
            public virtual bool IsNullable { get { return false; } }
        }

        public abstract class PropertyAccessorT<PropertyT>: PropertyAccessor
        {
            public PropertyAccessorT(PropertyInfo p): base(p) { }

            public abstract PropertyT Get(object instance);
            public virtual void Set(object instance, PropertyT value)
            {
                throw new NotImplementedException();
            }
        }

        public class PropertyAccessorCastRW<PropertyT, SourceT>: PropertyAccessorT<PropertyT>
        {
            PropertyAccessorT<SourceT> _prop;
            Func<SourceT, PropertyT> _castTo;
            Func<PropertyT, SourceT> _castFrom;

            public PropertyAccessorCastRW(PropertyInfo p, PropertyAccessorT<SourceT> prop,
                Func<SourceT, PropertyT> castTo, Func<PropertyT, SourceT> castFrom): base(p)
            {
                _prop = prop;
                _castTo = castTo;
                _castFrom = castFrom;
            }

            public override PropertyT Get(object instance)
            {
                return _castTo(_prop.Get(instance));
            }

            public override void Set(object instance, PropertyT value)
            {
                _prop.Set(instance, _castFrom(value));
            }
        }

        public class PropertyAccessorCastRO<PropertyT, SourceT>: PropertyAccessorT<PropertyT>
        {
            PropertyAccessorT<SourceT> _prop;
            Func<SourceT, PropertyT> _castTo;

            public PropertyAccessorCastRO(PropertyInfo p, PropertyAccessorT<SourceT> prop,
                Func<SourceT, PropertyT> castTo): base(p)
            {
                _prop = prop;
                _castTo = castTo;
            }

            public override PropertyT Get(object instance)
            {
                return _castTo(_prop.Get(instance));
            }
        }

        public class PropertyAccessorNullableCastRW<PropertyT, SourceT>: PropertyAccessorCastRW<PropertyT, SourceT>
        {
            public PropertyAccessorNullableCastRW(PropertyInfo p, PropertyAccessorT<SourceT> prop,
                Func<SourceT, PropertyT> castTo, Func<PropertyT, SourceT> castFrom): base(p, prop, castTo, castFrom) { }
            public override bool IsNullable { get { return true; } }
        }
        public class PropertyAccessorNullableCastRO<PropertyT, SourceT>: PropertyAccessorCastRO<PropertyT, SourceT>
        {
            public PropertyAccessorNullableCastRO(PropertyInfo p, PropertyAccessorT<SourceT> prop,
                Func<SourceT, PropertyT> castTo): base(p, prop, castTo) { }
            public override bool IsNullable { get { return true; } }
        }

        #region Accessors using PropertyInfo

        public class PropertyAccessorPropRW<PropertyT>: PropertyAccessorT<PropertyT>
        {
            Func<object, PropertyT> _getter;
            Action<object, PropertyT> _setter;

            public PropertyAccessorPropRW(PropertyInfo p): base(p)
            {
                _getter = (instance) => (PropertyT)p.GetValue(instance, null);
                _setter = (instance, value) => p.SetValue(instance, value, null);
            }

            public override PropertyT Get(object instance)
            {
                return _getter(instance);
            }

            public override void Set(object instance, PropertyT value)
            {
                _setter(instance, value);
            }
        }
        public class PropertyAccessorPropRO<PropertyT>: PropertyAccessorT<PropertyT>
        {
            Func<object, PropertyT> _getter;

            public PropertyAccessorPropRO(PropertyInfo p): base(p)
            {
                _getter = (instance) => (PropertyT)p.GetValue(instance, null);
            }

            public override PropertyT Get(object instance)
            {
                return _getter(instance);
            }
        }

        public class PropertyAccessorNullablePropRW<PropertyT>: PropertyAccessorPropRW<PropertyT>
        {
            public PropertyAccessorNullablePropRW(PropertyInfo p) : base(p) { }
            public override bool IsNullable { get { return true; } }
        }
        public class PropertyAccessorNullablePropRO<PropertyT>: PropertyAccessorPropRO<PropertyT>
        {
            public PropertyAccessorNullablePropRO(PropertyInfo p) : base(p) { }
            public override bool IsNullable { get { return true; } }
        }

        #endregion

        #region Accessors using dynamic code

#if !ENABLE_IL2CPP && !UNITY_IOS

        public class PropertyAccessorRW<PropertyT>: PropertyAccessorT<PropertyT>
        {
            Func<object, PropertyT> _getter;
            Action<object, PropertyT> _setter;

            public PropertyAccessorRW(PropertyInfo p)
                : base(p)
            {
                _getter = CreateGetter<PropertyT>(p.DeclaringType, p.GetGetMethod());
                _setter = CreateSetter<PropertyT>(p.DeclaringType, p.GetSetMethod());
            }

            public override PropertyT Get(object instance)
            {
                return _getter(instance);
            }

            public override void Set(object instance, PropertyT value)
            {
                _setter(instance, value);
            }
        }
        public class PropertyAccessorRO<PropertyT>: PropertyAccessorT<PropertyT>
        {
            Func<object, PropertyT> _getter;

            public PropertyAccessorRO(PropertyInfo p)
                : base(p)
            {
                _getter = CreateGetter<PropertyT>(p.DeclaringType, p.GetGetMethod());
            }

            public override PropertyT Get(object instance)
            {
                return _getter(instance);
            }
        }

        public class PropertyAccessorNullableRW<PropertyT>: PropertyAccessorRW<PropertyT>
        {
            public PropertyAccessorNullableRW(PropertyInfo p) : base(p) { }
            public override bool IsNullable { get { return true; } }
        }
        public class PropertyAccessorNullableRO<PropertyT>: PropertyAccessorRO<PropertyT>
        {
            public PropertyAccessorNullableRO(PropertyInfo p) : base(p) { }
            public override bool IsNullable { get { return true; } }
        }

        #region Getter/Setter delegate creation

        private static Func<object, ReturnT> CreateGetter<ReturnT>(Type type, MethodInfo method)
        {
            // Now supply the type arguments
            MethodInfo helper = CreateGetterHelperMethod.MakeGenericMethod(type,
                method.ReturnType);

            // Now call it. The null argument is because it's a static method.
            object[] methodArg = { method };
            object ret = helper.Invoke(null, methodArg);

            // Cast the result to the right kind of delegate and return it
            return (Func<object, ReturnT>)ret;
        }

        private static Func<object, ReturnT> CreateGetterHelper<InstanceT, ReturnT>(MethodInfo method)
        {
            // Convert the slow MethodInfo into a fast, strongly typed, open delegate
            Func<InstanceT, ReturnT> func = (Func<InstanceT, ReturnT>)method.CreateDelegate(typeof(Func<InstanceT, ReturnT>));

            // Now create a more weakly typed delegate which will call the strongly typed one
            Func<object, ReturnT> ret = (object target) => func((InstanceT)target);

            return ret;
        }

#if NETFX_CORE
        static MethodInfo CreateGetterHelperMethod = typeof(Extend).GetTypeInfo().GetDeclaredMethods("CreateGetterHelper")
            .Where(m => !m.IsPublic && m.IsStatic).FirstOrDefault();
#else
        static MethodInfo CreateGetterHelperMethod = typeof(Extend).GetMethod("CreateGetterHelper",
            BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.NonPublic);
#endif

        private static Action<object, ValueT> CreateSetter<ValueT>(Type type, MethodInfo method)
        {
            // Now supply the type arguments
            MethodInfo helper = CreateSetterHelperMethod.MakeGenericMethod(type,
                method.GetParameters()[0].ParameterType);

            // Now call it. The null argument is because it's a static method.
            object[] methodArg = { method };
            object ret = helper.Invoke(null, methodArg);

            // Cast the result to the right kind of delegate and return it
            return (Action<object, ValueT>)ret;
        }

        private static Action<object, ValueT> CreateSetterHelper<InstanceT, ValueT>(MethodInfo method)
        {
            // Convert the slow MethodInfo into a fast, strongly typed, open delegate
            Action<InstanceT, ValueT> func = (Action<InstanceT, ValueT>)method.CreateDelegate(typeof(Action<InstanceT, ValueT>));

            // Now create a more weakly typed delegate which will call the strongly typed one
            Action<object, ValueT> ret = (object target, ValueT value) => func((InstanceT)target, value);

            return ret;
        }

#if NETFX_CORE
        static MethodInfo CreateSetterHelperMethod = typeof(Extend).GetTypeInfo().GetDeclaredMethods("CreateSetterHelper")
            .Where(m => !m.IsPublic && m.IsStatic).FirstOrDefault();
#else
        static MethodInfo CreateSetterHelperMethod = typeof(Extend).GetMethod("CreateSetterHelper",
            BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.NonPublic);
#endif

        #endregion

#endif // !ENABLE_IL2CPP && !UNITY_IOS

        #endregion

        #endregion

        #region Indexer properties

        public abstract class IndexerAccessor { }

        public abstract class IndexerAccessorT<IndexT>: IndexerAccessor
        {
            public abstract object Get(object instance, IndexT index);
            public virtual void Set(object instance, IndexT index, object value)
            {
                throw new NotImplementedException();
            }
        }

        #region Indexer accessors using PropertyInfo

        public abstract class IndexerAccessorPropT<IndexT>: IndexerAccessorT<IndexT>
        {
            object[] _index = new object[1];
            protected object[] Index(IndexT index)
            {
                _index[0] = index;
                return _index;
            }
        }

        public class IndexerAccessorPropRW<IndexT>: IndexerAccessorPropT<IndexT>
        {
            Func<object, IndexT, object> _getter;
            Action<object, IndexT, object> _setter;

            public IndexerAccessorPropRW(PropertyInfo p)
            {
                _getter = (instance, index) => p.GetValue(instance, Index(index));
                _setter = (instance, index, value) => p.SetValue(instance, value, Index(index));
            }

            public override object Get(object instance, IndexT index)
            {
                return _getter(instance, index);
            }

            public override void Set(object instance, IndexT index, object value)
            {
                _setter(instance, index, value);
            }
        }

        public class IndexerAccessorPropRO<IndexT>: IndexerAccessorPropT<IndexT>
        {
            Func<object, IndexT, object> _getter;

            public IndexerAccessorPropRO(PropertyInfo p)
            {
                _getter = (instance, index) => p.GetValue(instance, Index(index));
            }

            public override object Get(object instance, IndexT index)
            {
                return _getter(instance, index);
            }
        }

        #endregion

        #region Indexer accessors using dynamic code

#if !ENABLE_IL2CPP && !UNITY_IOS

        public class IndexerAccessorRW<IndexT>: IndexerAccessorT<IndexT>
        {
            Func<object, IndexT, object> _getter;
            Action<object, IndexT, object> _setter;

            public IndexerAccessorRW(PropertyInfo p)
            {
                _getter = CreateIndexerGetter<IndexT>(p.DeclaringType, p.GetGetMethod());
                _setter = CreateIndexerSetter<IndexT>(p.DeclaringType, p.GetSetMethod());
            }

            public override object Get(object instance, IndexT index)
            {
                return _getter(instance, index);
            }

            public override void Set(object instance, IndexT index, object value)
            {
                _setter(instance, index, value);
            }
        }

        public class IndexerAccessorRO<IndexT>: IndexerAccessorT<IndexT>
        {
            Func<object, IndexT, object> _getter;

            public IndexerAccessorRO(PropertyInfo p)
            {
                _getter = CreateIndexerGetter<IndexT>(p.DeclaringType, p.GetGetMethod());
            }

            public override object Get(object instance, IndexT index)
            {
                return _getter(instance, index);
            }
        }

        #region Getter/Setter delegate creation

        private static Func<object, IndexT, object> CreateIndexerGetter<IndexT>(Type type, MethodInfo method)
        {
            // Now supply the type arguments
            MethodInfo helper = CreateIndexerGetterHelperMethod.MakeGenericMethod(type,
                method.GetParameters()[0].ParameterType, method.ReturnType);

            // Now call it. The null argument is because it's a static method.
            object[] methodArg = { method };
            object ret = helper.Invoke(null, methodArg);

            // Cast the result to the right kind of delegate and return it
            return (Func<object, IndexT, object>)ret;
        }

        private static Func<object, IndexT, object> CreateIndexerGetterHelper<InstanceT, IndexT, ReturnT>(MethodInfo method)
        {
            // Convert the slow MethodInfo into a fast, strongly typed, open delegate
            Func<InstanceT, IndexT, ReturnT> func = (Func<InstanceT, IndexT, ReturnT>)method.CreateDelegate(typeof(Func<InstanceT, IndexT, ReturnT>));

            // Now create a more weakly typed delegate which will call the strongly typed one
            Func<object, IndexT, object> ret = (object target, IndexT index)
                => func((InstanceT)target, index);

            return ret;
        }

#if NETFX_CORE
        static MethodInfo CreateIndexerGetterHelperMethod = typeof(Extend).GetTypeInfo().GetDeclaredMethods("CreateIndexerGetterHelper")
            .Where(m => !m.IsPublic && m.IsStatic).FirstOrDefault();
#else
        static MethodInfo CreateIndexerGetterHelperMethod = typeof(Extend).GetMethod("CreateIndexerGetterHelper",
            BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.NonPublic);
#endif

        private static Action<object, IndexT, object> CreateIndexerSetter<IndexT>(Type type, MethodInfo method)
        {
            // Now supply the type arguments
            var parameters = method.GetParameters();
            MethodInfo helper = CreateIndexerSetterHelperMethod.MakeGenericMethod(type,
                parameters[0].ParameterType, parameters[1].ParameterType);

            // Now call it. The null argument is because it's a static method.
            object[] methodArg = { method };
            object ret = helper.Invoke(null, methodArg);

            // Cast the result to the right kind of delegate and return it
            return (Action<object, IndexT, object>)ret;
        }

        private static Action<object, IndexT, object> CreateIndexerSetterHelper<InstanceT, IndexT, ValueT>(MethodInfo method)
        {
            // Convert the slow MethodInfo into a fast, strongly typed, open delegate
            Action<InstanceT, IndexT, ValueT> func = (Action<InstanceT, IndexT, ValueT>)method.CreateDelegate(typeof(Action<InstanceT, IndexT, ValueT>));

            // Now create a more weakly typed delegate which will call the strongly typed one
            Action<object, IndexT, object> ret = (object target, IndexT index, object value)
                => func((InstanceT)target, index, ConvertValue<ValueT>(value));

            return ret;
        }

        private static ValueT ConvertValue<ValueT>(object value)
        {
            Type targetType = typeof(ValueT);
            return targetType.GetTypeInfo().IsValueType ? (ValueT)Convert.ChangeType(value, targetType) : (ValueT)value;
        }

#if NETFX_CORE
        static MethodInfo CreateIndexerSetterHelperMethod = typeof(Extend).GetTypeInfo().GetDeclaredMethods("CreateIndexerSetterHelper")
            .Where(m => !m.IsPublic && m.IsStatic).FirstOrDefault();
#else
        static MethodInfo CreateIndexerSetterHelperMethod = typeof(Extend).GetMethod("CreateIndexerSetterHelper",
            BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.NonPublic);
#endif


        #endregion

#endif

        #endregion

        #endregion

        #endregion

        #region Add property type table

        private delegate ExtendPropertyData AddPropertyDelegate(NativeTypePropsInfo info, PropertyInfo p, bool usePropertyInfo);
        private static Dictionary<Type, AddPropertyDelegate> _addPropertyFunctions = AddPropertyFunctions();

        private static Dictionary<Type, AddPropertyDelegate> AddPropertyFunctions()
        {
            Dictionary<Type, AddPropertyDelegate> addPropFunctions = new Dictionary<Type, AddPropertyDelegate>();

            addPropFunctions[typeof(bool)] = (info, p, usePropertyInfo) =>
            {
                AddPropertyAccessor<bool>(info, p, usePropertyInfo);
                return CreatePropertyData(p, NativePropertyType.Bool);
            };

            addPropFunctions[typeof(float)] = (info, p, usePropertyInfo) =>
            {
                AddPropertyAccessor<float>(info, p, usePropertyInfo);
                return CreatePropertyData(p, NativePropertyType.Float);
            };

            addPropFunctions[typeof(double)] = (info, p, usePropertyInfo) =>
            {
                AddPropertyAccessor<double>(info, p, usePropertyInfo);
                return CreatePropertyData(p, NativePropertyType.Double);
            };

            addPropFunctions[typeof(decimal)] = (info, p, usePropertyInfo) =>
            {
                AddPropertyAccessor<double, decimal>(info, p, (v) => (double)v, (v) => (decimal)v, usePropertyInfo);
                return CreatePropertyData(p, NativePropertyType.Double);
            };

            addPropFunctions[typeof(int)] = (info, p, usePropertyInfo) =>
            {
                AddPropertyAccessor<int>(info, p, usePropertyInfo);
                return CreatePropertyData(p, NativePropertyType.Int);
            };

            addPropFunctions[typeof(long)] = (info, p, usePropertyInfo) =>
            {
                AddPropertyAccessor<int, long>(info, p, (v) => (int)v, (v) => (long)v, usePropertyInfo);
                return CreatePropertyData(p, NativePropertyType.Int);
            };

            addPropFunctions[typeof(uint)] = (info, p, usePropertyInfo) =>
            {
                AddPropertyAccessor<uint>(info, p, usePropertyInfo);
                return CreatePropertyData(p, NativePropertyType.UInt);
            };

            addPropFunctions[typeof(ulong)] = (info, p, usePropertyInfo) =>
            {
                AddPropertyAccessor<uint, ulong>(info, p, (v) => (uint)v, (v) => (ulong)v, usePropertyInfo);
                return CreatePropertyData(p, NativePropertyType.UInt);
            };

            addPropFunctions[typeof(char)] = (info, p, usePropertyInfo) =>
            {
                AddPropertyAccessor<uint, char>(info, p, (v) => (uint)v, (v) => (char)v, usePropertyInfo);
                return CreatePropertyData(p, NativePropertyType.UInt);
            };

            addPropFunctions[typeof(short)] = (info, p, usePropertyInfo) =>
            {
                AddPropertyAccessor<short>(info, p, usePropertyInfo);
                return CreatePropertyData(p, NativePropertyType.Short);
            };

            addPropFunctions[typeof(sbyte)] = (info, p, usePropertyInfo) =>
            {
                AddPropertyAccessor<short, sbyte>(info, p, (v) => (short)v, (v) => (sbyte)v, usePropertyInfo);
                return CreatePropertyData(p, NativePropertyType.Short);
            };

            addPropFunctions[typeof(ushort)] = (info, p, usePropertyInfo) =>
            {
                AddPropertyAccessor<ushort>(info, p, usePropertyInfo);
                return CreatePropertyData(p, NativePropertyType.UShort);
            };

            addPropFunctions[typeof(byte)] = (info, p, usePropertyInfo) =>
            {
                AddPropertyAccessor<ushort, byte>(info, p, (v) => (ushort)v, (v) => (byte)v, usePropertyInfo);
                return CreatePropertyData(p, NativePropertyType.UShort);
            };

            addPropFunctions[typeof(Noesis.Color)] = (info, p, usePropertyInfo) =>
            {
                AddPropertyAccessor<Noesis.Color>(info, p, usePropertyInfo);
                return CreatePropertyData(p, NativePropertyType.Color);
            };

            addPropFunctions[typeof(Noesis.Point)] = (info, p, usePropertyInfo) =>
            {
                AddPropertyAccessor<Noesis.Point>(info, p, usePropertyInfo);
                return CreatePropertyData(p, NativePropertyType.Point);
            };

            addPropFunctions[typeof(Noesis.Rect)] = (info, p, usePropertyInfo) =>
            {
                AddPropertyAccessor<Noesis.Rect>(info, p, usePropertyInfo);
                return CreatePropertyData(p, NativePropertyType.Rect);
            };

            addPropFunctions[typeof(Noesis.Size)] = (info, p, usePropertyInfo) =>
            {
                AddPropertyAccessor<Noesis.Size>(info, p, usePropertyInfo);
                return CreatePropertyData(p, NativePropertyType.Size);
            };

            addPropFunctions[typeof(Noesis.Thickness)] = (info, p, usePropertyInfo) =>
            {
                AddPropertyAccessor<Noesis.Thickness>(info, p, usePropertyInfo);
                return CreatePropertyData(p, NativePropertyType.Thickness);
            };

            addPropFunctions[typeof(Noesis.CornerRadius)] = (info, p, usePropertyInfo) =>
            {
                AddPropertyAccessor<Noesis.CornerRadius>(info, p, usePropertyInfo);
                return CreatePropertyData(p, NativePropertyType.CornerRadius);
            };

            addPropFunctions[typeof(System.TimeSpan)] = (info, p, usePropertyInfo) =>
            {
                AddPropertyAccessor<Noesis.TimeSpanStruct, System.TimeSpan>(info, p,
                    (v) => (Noesis.TimeSpanStruct)v, (v) => (System.TimeSpan)v, usePropertyInfo);
                return CreatePropertyData(p, NativePropertyType.TimeSpan);
            };

            addPropFunctions[typeof(Noesis.Duration)] = (info, p, usePropertyInfo) =>
            {
                AddPropertyAccessor<Noesis.Duration>(info, p, usePropertyInfo);
                return CreatePropertyData(p, NativePropertyType.Duration);
            };

            addPropFunctions[typeof(Noesis.KeyTime)] = (info, p, usePropertyInfo) =>
            {
                AddPropertyAccessor<Noesis.KeyTime>(info, p, usePropertyInfo);
                return CreatePropertyData(p, NativePropertyType.KeyTime);
            };

            addPropFunctions[typeof(bool?)] = (info, p, usePropertyInfo) =>
            {
                AddPropertyAccessorNullable<bool?>(info, p, usePropertyInfo);
                return CreatePropertyData(p, NativePropertyType.NullableBool);
            };

            addPropFunctions[typeof(float?)] = (info, p, usePropertyInfo) =>
            {
                AddPropertyAccessorNullable<float?>(info, p, usePropertyInfo);
                return CreatePropertyData(p, NativePropertyType.NullableFloat);
            };

            addPropFunctions[typeof(double?)] = (info, p, usePropertyInfo) =>
            {
                AddPropertyAccessorNullable<double?>(info, p, usePropertyInfo);
                return CreatePropertyData(p, NativePropertyType.NullableDouble);
            };

            addPropFunctions[typeof(decimal?)] = (info, p, usePropertyInfo) =>
            {
                AddPropertyAccessorNullable<double?, decimal?>(info, p, (v) => (double?)v, (v) => (decimal?)v, usePropertyInfo);
                return CreatePropertyData(p, NativePropertyType.NullableDouble);
            };

            addPropFunctions[typeof(int?)] = (info, p, usePropertyInfo) =>
            {
                AddPropertyAccessorNullable<int?>(info, p, usePropertyInfo);
                return CreatePropertyData(p, NativePropertyType.NullableInt);
            };

            addPropFunctions[typeof(long?)] = (info, p, usePropertyInfo) =>
            {
                AddPropertyAccessorNullable<int?, long?>(info, p, (v) => (int?)v, (v) => (long?)v, usePropertyInfo);
                return CreatePropertyData(p, NativePropertyType.NullableInt);
            };

            addPropFunctions[typeof(uint?)] = (info, p, usePropertyInfo) =>
            {
                AddPropertyAccessorNullable<uint?>(info, p, usePropertyInfo);
                return CreatePropertyData(p, NativePropertyType.NullableUInt);
            };

            addPropFunctions[typeof(ulong?)] = (info, p, usePropertyInfo) =>
            {
                AddPropertyAccessorNullable<uint?, ulong?>(info, p, (v) => (uint?)v, (v) => (ulong?)v, usePropertyInfo);
                return CreatePropertyData(p, NativePropertyType.NullableUInt);
            };

            addPropFunctions[typeof(char?)] = (info, p, usePropertyInfo) =>
            {
                AddPropertyAccessorNullable<uint?, char?>(info, p, (v) => (uint?)v, (v) => (char?)v, usePropertyInfo);
                return CreatePropertyData(p, NativePropertyType.NullableUInt);
            };

            addPropFunctions[typeof(short?)] = (info, p, usePropertyInfo) =>
            {
                AddPropertyAccessorNullable<short?>(info, p, usePropertyInfo);
                return CreatePropertyData(p, NativePropertyType.NullableShort);
            };

            addPropFunctions[typeof(sbyte?)] = (info, p, usePropertyInfo) =>
            {
                AddPropertyAccessorNullable<short?, sbyte?>(info, p, (v) => (short?)v, (v) => (sbyte?)v, usePropertyInfo);
                return CreatePropertyData(p, NativePropertyType.NullableShort);
            };

            addPropFunctions[typeof(ushort?)] = (info, p, usePropertyInfo) =>
            {
                AddPropertyAccessorNullable<ushort?>(info, p, usePropertyInfo);
                return CreatePropertyData(p, NativePropertyType.NullableUShort);
            };

            addPropFunctions[typeof(byte?)] = (info, p, usePropertyInfo) =>
            {
                AddPropertyAccessorNullable<ushort?, byte?>(info, p, (v) => (ushort?)v, (v) => (byte?)v, usePropertyInfo);
                return CreatePropertyData(p, NativePropertyType.NullableUShort);
            };

            addPropFunctions[typeof(Noesis.Color?)] = (info, p, usePropertyInfo) =>
            {
                AddPropertyAccessorNullable<Noesis.Color?>(info, p, usePropertyInfo);
                return CreatePropertyData(p, NativePropertyType.NullableColor);
            };

            addPropFunctions[typeof(Noesis.Point?)] = (info, p, usePropertyInfo) =>
            {
                AddPropertyAccessorNullable<Noesis.Point?>(info, p, usePropertyInfo);
                return CreatePropertyData(p, NativePropertyType.NullablePoint);
            };

            addPropFunctions[typeof(Noesis.Rect?)] = (info, p, usePropertyInfo) =>
            {
                AddPropertyAccessorNullable<Noesis.Rect?>(info, p, usePropertyInfo);
                return CreatePropertyData(p, NativePropertyType.NullableRect);
            };

            addPropFunctions[typeof(Noesis.Size?)] = (info, p, usePropertyInfo) =>
            {
                AddPropertyAccessorNullable<Noesis.Size?>(info, p, usePropertyInfo);
                return CreatePropertyData(p, NativePropertyType.NullableSize);
            };

            addPropFunctions[typeof(Noesis.Thickness?)] = (info, p, usePropertyInfo) =>
            {
                AddPropertyAccessorNullable<Noesis.Thickness?>(info, p, usePropertyInfo);
                return CreatePropertyData(p, NativePropertyType.NullableThickness);
            };

            addPropFunctions[typeof(Noesis.CornerRadius?)] = (info, p, usePropertyInfo) =>
            {
                AddPropertyAccessorNullable<Noesis.CornerRadius?>(info, p, usePropertyInfo);
                return CreatePropertyData(p, NativePropertyType.NullableCornerRadius);
            };

            addPropFunctions[typeof(System.TimeSpan?)] = (info, p, usePropertyInfo) =>
            {
                AddPropertyAccessorNullable<Noesis.TimeSpanStruct?, System.TimeSpan?>(info, p,
                    (v) => (Noesis.TimeSpanStruct?)v, (v) => (System.TimeSpan?)v, usePropertyInfo);
                return CreatePropertyData(p, NativePropertyType.NullableTimeSpan);
            };

            addPropFunctions[typeof(Noesis.Duration?)] = (info, p, usePropertyInfo) =>
            {
                AddPropertyAccessorNullable<Noesis.Duration?>(info, p, usePropertyInfo);
                return CreatePropertyData(p, NativePropertyType.NullableDuration);
            };

            addPropFunctions[typeof(Noesis.KeyTime?)] = (info, p, usePropertyInfo) =>
            {
                AddPropertyAccessorNullable<Noesis.KeyTime?>(info, p, usePropertyInfo);
                return CreatePropertyData(p, NativePropertyType.NullableKeyTime);
            };

            addPropFunctions[typeof(string)] = (info, p, usePropertyInfo) =>
            {
                AddPropertyAccessor<string>(info, p, usePropertyInfo);
                return CreatePropertyData(p, NativePropertyType.String);
            };

            addPropFunctions[typeof(System.Uri)] = (info, p, usePropertyInfo) =>
            {
                AddPropertyAccessor<System.Uri>(info, p, usePropertyInfo);
                return CreatePropertyData(p, NativePropertyType.Uri);
            };

            return addPropFunctions;
        }

        #endregion

        private static ExtendPropertyData AddProperty(NativeTypePropsInfo info, PropertyInfo p, bool usePropertyInfo)
        {
            AddPropertyDelegate addPropFunction;
            if (_addPropertyFunctions.TryGetValue(p.PropertyType, out addPropFunction))
            {
                return addPropFunction(info, p, usePropertyInfo);
            }

            IntPtr propertyType = EnsureNativeType(p.PropertyType);

            if (p.PropertyType.GetTypeInfo().IsEnum)
            {
                AddPropertyAccessor<int, object>(info, p,
                    (v) => (int)Convert.ToInt64(v),
                    (v) => Enum.ToObject(p.PropertyType, v),
                    true);

                return CreatePropertyData(p, NativePropertyType.Enum, propertyType);
            }
            else
            {
                if (p.PropertyType == typeof(System.Type))
                {
                    AddPropertyAccessor<object, object>(info, p,
                        (v) => Noesis.Extend.GetResourceKeyType((Type)v),
                        (v) => v != null ? ((ResourceKeyType)v).Type : null,
                        true);
                }
                else
                {
                    AddPropertyAccessor<object>(info, p, true);
                }

                return CreatePropertyData(p, NativePropertyType.BaseComponent, propertyType);
            }
        }

        private static void AddPropertyAccessor<PropertyT>(NativeTypePropsInfo info, PropertyInfo p, bool usePropertyInfo)
        {
#if !ENABLE_IL2CPP && !UNITY_IOS
            if (!usePropertyInfo)
            {
                AddPropertyAccessor(info, p,
                    () => new PropertyAccessorRW<PropertyT>(p),
                    () => new PropertyAccessorRO<PropertyT>(p));
            }
            else
#endif
            {
                AddPropertyAccessor(info, p,
                    () => new PropertyAccessorPropRW<PropertyT>(p),
                    () => new PropertyAccessorPropRO<PropertyT>(p));
            }
        }

        private static void AddPropertyAccessorNullable<PropertyT>(NativeTypePropsInfo info, PropertyInfo p, bool usePropertyInfo)
        {
#if !ENABLE_IL2CPP && !UNITY_IOS
            if (!usePropertyInfo)
            {
                AddPropertyAccessor(info, p,
                    () => new PropertyAccessorNullableRW<PropertyT>(p),
                    () => new PropertyAccessorNullableRO<PropertyT>(p));
            }
            else
#endif
            {
                AddPropertyAccessor(info, p,
                    () => new PropertyAccessorNullablePropRW<PropertyT>(p),
                    () => new PropertyAccessorNullablePropRO<PropertyT>(p));
            }
        }

        private static void AddPropertyAccessor<PropertyT, SourceT>(NativeTypePropsInfo info, PropertyInfo p,
            Func<SourceT, PropertyT> castTo, Func<PropertyT, SourceT> castFrom, bool usePropertyInfo)
        {
#if !ENABLE_IL2CPP && !UNITY_IOS
            if (!usePropertyInfo)
            {
                AddPropertyAccessor(info, p,
                    () => new PropertyAccessorCastRW<PropertyT, SourceT>(p, new PropertyAccessorRW<SourceT>(p), castTo, castFrom),
                    () => new PropertyAccessorCastRO<PropertyT, SourceT>(p, new PropertyAccessorRO<SourceT>(p), castTo));
            }
            else
#endif
            {
                AddPropertyAccessor(info, p,
                    () => new PropertyAccessorCastRW<PropertyT, SourceT>(p, new PropertyAccessorPropRW<SourceT>(p), castTo, castFrom),
                    () => new PropertyAccessorCastRO<PropertyT, SourceT>(p, new PropertyAccessorPropRO<SourceT>(p), castTo));
            }
        }

        private static void AddPropertyAccessorNullable<PropertyT, SourceT>(NativeTypePropsInfo info, PropertyInfo p,
            Func<SourceT, PropertyT> castTo, Func<PropertyT, SourceT> castFrom, bool usePropertyInfo)
        {
#if !ENABLE_IL2CPP && !UNITY_IOS
            if (!usePropertyInfo)
            {
                AddPropertyAccessor(info, p,
                    () => new PropertyAccessorNullableCastRW<PropertyT, SourceT>(p, new PropertyAccessorRW<SourceT>(p), castTo, castFrom),
                    () => new PropertyAccessorNullableCastRO<PropertyT, SourceT>(p, new PropertyAccessorRO<SourceT>(p), castTo));
            }
            else
#endif
            {
                AddPropertyAccessor(info, p,
                    () => new PropertyAccessorNullableCastRW<PropertyT, SourceT>(p, new PropertyAccessorPropRW<SourceT>(p), castTo, castFrom),
                    () => new PropertyAccessorNullableCastRO<PropertyT, SourceT>(p, new PropertyAccessorPropRO<SourceT>(p), castTo));
            }
        }

        private static void AddPropertyAccessor(NativeTypePropsInfo info, PropertyInfo p,
            Func<PropertyAccessor> creatorRW, Func<PropertyAccessor> creatorRO)
        {
            if (p.GetSetMethod() != null)
            {
                info.Properties.Add(creatorRW());
            }
            else
            {
                info.Properties.Add(creatorRO());
            }
        }

        private static IndexerAccessor CreateIndexerAccessor<IndexT>(PropertyInfo p)
        {
            if (p.GetSetMethod() != null)
            {
#if !ENABLE_IL2CPP && !UNITY_IOS
                if (!p.DeclaringType.GetTypeInfo().IsValueType)
                {
                    return new IndexerAccessorRW<IndexT>(p);
                }
                else
#endif
                {
                    return new IndexerAccessorPropRW<IndexT>(p);
                }
            }
            else
            {
#if !ENABLE_IL2CPP && !UNITY_IOS
                if (!p.DeclaringType.GetTypeInfo().IsValueType)
                {
                    return new IndexerAccessorRO<IndexT>(p);
                }
                else
#endif
                {
                    return new IndexerAccessorPropRO<IndexT>(p);
                }
            }
        }

        private static ExtendPropertyData CreatePropertyData(PropertyInfo p, NativePropertyType extendType)
        {
            ExtendPropertyData propData = new ExtendPropertyData();
            propData.name = Marshal.StringToHGlobalAnsi(p.Name).ToInt64();
            propData.extendType = (int)extendType;
            propData.readOnly = p.GetSetMethod() == null ? 1 : 0;

            var typeConverter = p.GetCustomAttribute<System.ComponentModel.TypeConverterAttribute>();
            if (typeConverter != null)
            {
                propData.typeConverter = Marshal.StringToHGlobalAnsi(typeConverter.ConverterTypeName).ToInt64();
            }

            return propData;
        }

        private static ExtendPropertyData CreatePropertyData(PropertyInfo p,
            NativePropertyType extendType, IntPtr propertyType)
        {
            ExtendPropertyData propData = CreatePropertyData(p, extendType);
            propData.type = propertyType.ToInt64();

            return propData;
        }
    }
}
