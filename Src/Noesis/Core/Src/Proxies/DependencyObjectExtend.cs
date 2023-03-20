using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace Noesis
{
    public delegate void EnumDependencyPropertiesDelegate(DependencyProperty dp, object value);

    public partial class DependencyObject
    {
        public object GetValue(DependencyProperty dp)
        {
            if (dp == null)
            {
                throw new Exception("Can't get value, DependencyProperty is null");
            }

            Type dpType = dp.PropertyType;

            GetDelegate getDelegate;
            if (dpType.GetTypeInfo().IsEnum)
            {
                _getFunctions.TryGetValue(typeof(ulong), out getDelegate);
                ulong value = (ulong)getDelegate(swigCPtr.Handle, DependencyProperty.getCPtr(dp).Handle);
                return Enum.ToObject(dpType, value);
            }
            else if (_getFunctions.TryGetValue(dpType, out getDelegate))
            {
                return getDelegate(swigCPtr.Handle, DependencyProperty.getCPtr(dp).Handle);
            }
            else
            {
                IntPtr ptr = Noesis_DependencyGet_BaseComponent(swigCPtr.Handle, DependencyProperty.getCPtr(dp).Handle);
                return Noesis.Extend.GetProxy(ptr, false);
            }
        }


        public void SetValue(DependencyProperty dp, object value)
        {
            if (dp == null)
            {
                throw new Exception("Can't set value, DependencyProperty is null");
            }

            Type dpType = dp.PropertyType;

            SetDelegate setDelegate;
            if (dpType.GetTypeInfo().IsEnum)
            {
                _setFunctions.TryGetValue(typeof(ulong), out setDelegate);
                setDelegate(swigCPtr.Handle, DependencyProperty.getCPtr(dp).Handle, Convert.ToUInt64(value));
            }
            else if (_setFunctions.TryGetValue(dpType, out setDelegate))
            {
                setDelegate(swigCPtr.Handle, DependencyProperty.getCPtr(dp).Handle, value);
            }
            else
            {
                Noesis_DependencySet_BaseComponent(swigCPtr.Handle, DependencyProperty.getCPtr(dp).Handle,
                    Noesis.Extend.GetInstanceHandle(value).Handle);
            }
        }

        #region Getter and Setter map

        private delegate object GetDelegate(IntPtr cPtr, IntPtr dp);
        private static Dictionary<Type, GetDelegate> _getFunctions = CreateGetFunctions();

        private delegate void SetDelegate(IntPtr cPtr, IntPtr dp, object value);
        private static Dictionary<Type, SetDelegate> _setFunctions = CreateSetFunctions();

        private static Dictionary<Type, GetDelegate> CreateGetFunctions()
        {
            Dictionary<Type, GetDelegate> getFunctions = new Dictionary<Type, GetDelegate>(50);

            getFunctions[typeof(bool)] = (cPtr, dp) =>
            {
                CheckProperty(cPtr, dp, "get");
                bool isNull;
                return Noesis_DependencyGet_Bool(cPtr, dp, false, out isNull);
            };
            getFunctions[typeof(float)] = (cPtr, dp) =>
            {
                CheckProperty(cPtr, dp, "get");
                bool isNull;
                return Noesis_DependencyGet_Float(cPtr, dp, false, out isNull);
            };
            getFunctions[typeof(double)] = (cPtr, dp) =>
            {
                CheckProperty(cPtr, dp, "get");
                bool isNull;
                return Noesis_DependencyGet_Double(cPtr, dp, false, out isNull);
            };
            getFunctions[typeof(decimal)] = (cPtr, dp) =>
            {
                CheckProperty(cPtr, dp, "get");
                bool isNull;
                return (decimal)Noesis_DependencyGet_Double(cPtr, dp, false, out isNull);
            };
            getFunctions[typeof(int)] = (cPtr, dp) =>
            {
                CheckProperty(cPtr, dp, "get");
                bool isNull;
                return Noesis_DependencyGet_Int(cPtr, dp, false, out isNull);
            };
            getFunctions[typeof(long)] = (cPtr, dp) =>
            {
                CheckProperty(cPtr, dp, "get");
                bool isNull;
                return Noesis_DependencyGet_Int64(cPtr, dp, false, out isNull);
            };
            getFunctions[typeof(uint)] = (cPtr, dp) =>
            {
                CheckProperty(cPtr, dp, "get");
                bool isNull;
                return Noesis_DependencyGet_UInt(cPtr, dp, false, out isNull);
            };
            getFunctions[typeof(ulong)] = (cPtr, dp) =>
            {
                CheckProperty(cPtr, dp, "get");
                bool isNull;
                return Noesis_DependencyGet_UInt64(cPtr, dp, false, out isNull);
            };
            getFunctions[typeof(char)] = (cPtr, dp) =>
            {
                CheckProperty(cPtr, dp, "get");
                bool isNull;
                return (char)Noesis_DependencyGet_UInt(cPtr, dp, false, out isNull);
            };
            getFunctions[typeof(short)] = (cPtr, dp) =>
            {
                CheckProperty(cPtr, dp, "get");
                bool isNull;
                return Noesis_DependencyGet_Short(cPtr, dp, false, out isNull);
            };
            getFunctions[typeof(sbyte)] = (cPtr, dp) =>
            {
                CheckProperty(cPtr, dp, "get");
                bool isNull;
                return (sbyte)Noesis_DependencyGet_Short(cPtr, dp, false, out isNull);
            };
            getFunctions[typeof(ushort)] = (cPtr, dp) =>
            {
                CheckProperty(cPtr, dp, "get");
                bool isNull;
                return Noesis_DependencyGet_UShort(cPtr, dp, false, out isNull);
            };
            getFunctions[typeof(byte)] = (cPtr, dp) =>
            {
                CheckProperty(cPtr, dp, "get");
                bool isNull;
                return (byte)Noesis_DependencyGet_UShort(cPtr, dp, false, out isNull);
            };
            getFunctions[typeof(string)] = (cPtr, dp) =>
            {
                CheckProperty(cPtr, dp, "get");
                IntPtr ptr = Noesis_DependencyGet_String(cPtr, dp);
                return Noesis.Extend.StringFromNativeUtf8(ptr);
            };
            getFunctions[typeof(System.Uri)] = (cPtr, dp) =>
            {
                CheckProperty(cPtr, dp, "get");
                IntPtr ptr = Noesis_DependencyGet_Uri(cPtr, dp);
                string uri = Noesis.Extend.StringFromNativeUtf8(ptr);
                return new Uri(uri, UriKind.RelativeOrAbsolute);
            };
            getFunctions[typeof(Noesis.Color)] = (cPtr, dp) =>
            {
                CheckProperty(cPtr, dp, "get");
                bool isNull;
                IntPtr ptr = Noesis_DependencyGet_Color(cPtr, dp, false, out isNull);
                return Marshal.PtrToStructure<Noesis.Color>(ptr);
            };
            getFunctions[typeof(Noesis.Point)] = (cPtr, dp) =>
            {
                CheckProperty(cPtr, dp, "get");
                bool isNull;
                IntPtr ptr = Noesis_DependencyGet_Point(cPtr, dp, false, out isNull);
                return Marshal.PtrToStructure<Noesis.Point>(ptr);
            };
            getFunctions[typeof(Noesis.Rect)] = (cPtr, dp) =>
            {
                CheckProperty(cPtr, dp, "get");
                bool isNull;
                IntPtr ptr = Noesis_DependencyGet_Rect(cPtr, dp, false, out isNull);
                return Marshal.PtrToStructure<Noesis.Rect>(ptr);
            };
            getFunctions[typeof(Noesis.Int32Rect)] = (cPtr, dp) =>
            {
                CheckProperty(cPtr, dp, "get");
                bool isNull;
                IntPtr ptr = Noesis_DependencyGet_Int32Rect(cPtr, dp, false, out isNull);
                return Marshal.PtrToStructure<Noesis.Int32Rect>(ptr);
            };
            getFunctions[typeof(Noesis.Size)] = (cPtr, dp) =>
            {
                CheckProperty(cPtr, dp, "get");
                bool isNull;
                IntPtr ptr = Noesis_DependencyGet_Size(cPtr, dp, false, out isNull);
                return Marshal.PtrToStructure<Noesis.Size>(ptr);
            };
            getFunctions[typeof(Noesis.Thickness)] = (cPtr, dp) =>
            {
                CheckProperty(cPtr, dp, "get");
                bool isNull;
                IntPtr ptr = Noesis_DependencyGet_Thickness(cPtr, dp, false, out isNull);
                return Marshal.PtrToStructure<Noesis.Thickness>(ptr);
            };
            getFunctions[typeof(Noesis.CornerRadius)] = (cPtr, dp) =>
            {
                CheckProperty(cPtr, dp, "get");
                bool isNull;
                IntPtr ptr = Noesis_DependencyGet_CornerRadius(cPtr, dp, false, out isNull);
                return Marshal.PtrToStructure<Noesis.CornerRadius>(ptr);
            };
            getFunctions[typeof(TimeSpan)] = (cPtr, dp) =>
            {
                CheckProperty(cPtr, dp, "get");
                bool isNull;
                IntPtr ptr = Noesis_DependencyGet_TimeSpan(cPtr, dp, false, out isNull);
                return (TimeSpan)Marshal.PtrToStructure<Noesis.TimeSpanStruct>(ptr);
            };
            getFunctions[typeof(Noesis.Duration)] = (cPtr, dp) =>
            {
                CheckProperty(cPtr, dp, "get");
                bool isNull;
                IntPtr ptr = Noesis_DependencyGet_Duration(cPtr, dp, false, out isNull);
                return Marshal.PtrToStructure<Noesis.Duration>(ptr);
            };
            getFunctions[typeof(Noesis.KeyTime)] = (cPtr, dp) =>
            {
                CheckProperty(cPtr, dp, "get");
                bool isNull;
                IntPtr ptr = Noesis_DependencyGet_KeyTime(cPtr, dp, false, out isNull);
                return Marshal.PtrToStructure<Noesis.KeyTime>(ptr);
            };

            getFunctions[typeof(bool?)] = (cPtr, dp) =>
            {
                CheckProperty(cPtr, dp, "get");
                bool isNull;
                bool val = Noesis_DependencyGet_Bool(cPtr, dp, true, out isNull);
                return isNull ? null : (bool?)val;
            };
            getFunctions[typeof(float?)] = (cPtr, dp) =>
            {
                CheckProperty(cPtr, dp, "get");
                bool isNull;
                float val = Noesis_DependencyGet_Float(cPtr, dp, true, out isNull);
                return isNull ? null : (float?)val;
            };
            getFunctions[typeof(double?)] = (cPtr, dp) =>
            {
                CheckProperty(cPtr, dp, "get");
                bool isNull;
                double val = Noesis_DependencyGet_Double(cPtr, dp, true, out isNull);
                return isNull ? null : (double?)val;
            };
            getFunctions[typeof(decimal?)] = (cPtr, dp) =>
            {
                CheckProperty(cPtr, dp, "get");
                bool isNull;
                double val = Noesis_DependencyGet_Double(cPtr, dp, true, out isNull);
                return isNull ? null : (decimal?)(decimal)val;
            };
            getFunctions[typeof(int?)] = (cPtr, dp) =>
            {
                CheckProperty(cPtr, dp, "get");
                bool isNull;
                int val = Noesis_DependencyGet_Int(cPtr, dp, true, out isNull);
                return isNull ? null : (int?)val;
            };
            getFunctions[typeof(long?)] = (cPtr, dp) =>
            {
                CheckProperty(cPtr, dp, "get");
                bool isNull;
                long val = Noesis_DependencyGet_Int64(cPtr, dp, true, out isNull);
                return isNull ? null : (long?)val;
            };
            getFunctions[typeof(uint?)] = (cPtr, dp) =>
            {
                CheckProperty(cPtr, dp, "get");
                bool isNull;
                uint val = Noesis_DependencyGet_UInt(cPtr, dp, true, out isNull);
                return isNull ? null : (uint?)val;
            };
            getFunctions[typeof(ulong?)] = (cPtr, dp) =>
            {
                CheckProperty(cPtr, dp, "get");
                bool isNull;
                ulong val = Noesis_DependencyGet_UInt64(cPtr, dp, true, out isNull);
                return isNull ? null : (ulong?)val;
            };
            getFunctions[typeof(char?)] = (cPtr, dp) =>
            {
                CheckProperty(cPtr, dp, "get");
                bool isNull;
                uint val = Noesis_DependencyGet_UInt(cPtr, dp, true, out isNull);
                return isNull ? null : (char?)(char)val;
            };
            getFunctions[typeof(short?)] = (cPtr, dp) =>
            {
                CheckProperty(cPtr, dp, "get");
                bool isNull;
                short val = Noesis_DependencyGet_Short(cPtr, dp, true, out isNull);
                return isNull ? null : (short?)val;
            };
            getFunctions[typeof(sbyte?)] = (cPtr, dp) =>
            {
                CheckProperty(cPtr, dp, "get");
                bool isNull;
                short val = Noesis_DependencyGet_Short(cPtr, dp, true, out isNull);
                return isNull ? null : (sbyte?)(sbyte)val;
            };
            getFunctions[typeof(ushort?)] = (cPtr, dp) =>
            {
                CheckProperty(cPtr, dp, "get");
                bool isNull;
                ushort val = Noesis_DependencyGet_UShort(cPtr, dp, true, out isNull);
                return isNull ? null : (ushort?)val;
            };
            getFunctions[typeof(byte?)] = (cPtr, dp) =>
            {
                CheckProperty(cPtr, dp, "get");
                bool isNull;
                ushort val = Noesis_DependencyGet_UShort(cPtr, dp, true, out isNull);
                return isNull ? null : (byte?)(byte)val;
            };
            getFunctions[typeof(Noesis.Color?)] = (cPtr, dp) =>
            {
                CheckProperty(cPtr, dp, "get");
                bool isNull;
                IntPtr ptr = Noesis_DependencyGet_Color(cPtr, dp, true, out isNull);
                return isNull ? null : (Noesis.Color?)Marshal.PtrToStructure<Noesis.Color>(ptr);
            };
            getFunctions[typeof(Noesis.Point?)] = (cPtr, dp) =>
            {
                CheckProperty(cPtr, dp, "get");
                bool isNull;
                IntPtr ptr = Noesis_DependencyGet_Point(cPtr, dp, true, out isNull);
                return isNull ? null : (Noesis.Point?)Marshal.PtrToStructure<Noesis.Point>(ptr);
            };
            getFunctions[typeof(Noesis.Rect?)] = (cPtr, dp) =>
            {
                CheckProperty(cPtr, dp, "get");
                bool isNull;
                IntPtr ptr = Noesis_DependencyGet_Rect(cPtr, dp, true, out isNull);
                return isNull ? null : (Noesis.Rect?)Marshal.PtrToStructure<Noesis.Rect>(ptr);
            };
            getFunctions[typeof(Noesis.Int32Rect?)] = (cPtr, dp) =>
            {
                CheckProperty(cPtr, dp, "get");
                bool isNull;
                IntPtr ptr = Noesis_DependencyGet_Int32Rect(cPtr, dp, true, out isNull);
                return isNull ? null : (Noesis.Int32Rect?)Marshal.PtrToStructure<Noesis.Int32Rect>(ptr);
            };
            getFunctions[typeof(Noesis.Size?)] = (cPtr, dp) =>
            {
                CheckProperty(cPtr, dp, "get");
                bool isNull;
                IntPtr ptr = Noesis_DependencyGet_Size(cPtr, dp, true, out isNull);
                return isNull ? null : (Noesis.Size?)Marshal.PtrToStructure<Noesis.Size>(ptr);
            };
            getFunctions[typeof(Noesis.Thickness?)] = (cPtr, dp) =>
            {
                CheckProperty(cPtr, dp, "get");
                bool isNull;
                IntPtr ptr = Noesis_DependencyGet_Thickness(cPtr, dp, true, out isNull);
                return isNull ? null : (Noesis.Thickness?)Marshal.PtrToStructure<Noesis.Thickness>(ptr);
            };
            getFunctions[typeof(Noesis.CornerRadius?)] = (cPtr, dp) =>
            {
                CheckProperty(cPtr, dp, "get");
                bool isNull;
                IntPtr ptr = Noesis_DependencyGet_CornerRadius(cPtr, dp, true, out isNull);
                return isNull ? null : (Noesis.CornerRadius?)Marshal.PtrToStructure<Noesis.CornerRadius>(ptr);
            };
            getFunctions[typeof(TimeSpan?)] = (cPtr, dp) =>
            {
                CheckProperty(cPtr, dp, "get");
                bool isNull;
                IntPtr ptr = Noesis_DependencyGet_TimeSpan(cPtr, dp, true, out isNull);
                return isNull ? null : (TimeSpan?)(TimeSpan)Marshal.PtrToStructure<Noesis.TimeSpanStruct>(ptr);
            };
            getFunctions[typeof(Noesis.Duration?)] = (cPtr, dp) =>
            {
                CheckProperty(cPtr, dp, "get");
                bool isNull;
                IntPtr ptr = Noesis_DependencyGet_Duration(cPtr, dp, true, out isNull);
                return isNull ? null : (Noesis.Duration?)Marshal.PtrToStructure<Noesis.Duration>(ptr);
            };
            getFunctions[typeof(Noesis.KeyTime?)] = (cPtr, dp) =>
            {
                CheckProperty(cPtr, dp, "get");
                bool isNull;
                IntPtr ptr = Noesis_DependencyGet_KeyTime(cPtr, dp, true, out isNull);
                return isNull ? null : (Noesis.KeyTime?)Marshal.PtrToStructure<Noesis.KeyTime>(ptr);
            };
            getFunctions[typeof(Type)] = (cPtr, dp) =>
            {
                CheckProperty(cPtr, dp, "get");
                IntPtr ptr = Noesis_DependencyGet_Type(cPtr, dp);
                if (ptr != IntPtr.Zero)
                {
                    Noesis.Extend.NativeTypeInfo info = Noesis.Extend.GetNativeTypeInfo(ptr);
                    return info.Type;
                }
                return null;
            };

            return getFunctions;
        }

        private static Dictionary<Type, SetDelegate> CreateSetFunctions()
        {
            Dictionary<Type, SetDelegate> setFunctions = new Dictionary<Type, SetDelegate>(50);

            setFunctions[typeof(bool)] = (cPtr, dp, value) =>
            {
                CheckProperty(cPtr, dp, "set");
                Noesis_DependencySet_Bool(cPtr, dp, (bool)value, false, false);
            };
            setFunctions[typeof(float)] = (cPtr, dp, value) =>
            {
                CheckProperty(cPtr, dp, "set");
                Noesis_DependencySet_Float(cPtr, dp, (float)value, false, false);
            };
            setFunctions[typeof(double)] = (cPtr, dp, value) =>
            {
                CheckProperty(cPtr, dp, "set");
                Noesis_DependencySet_Double(cPtr, dp, (double)value, false, false);
            };
            setFunctions[typeof(decimal)] = (cPtr, dp, value) =>
            {
                CheckProperty(cPtr, dp, "set");
                Noesis_DependencySet_Double(cPtr, dp, (double)(decimal)value, false, false);
            };
            setFunctions[typeof(int)] = (cPtr, dp, value) =>
            {
                CheckProperty(cPtr, dp, "set");
                Noesis_DependencySet_Int(cPtr, dp, (int)value, false, false);
            };
            setFunctions[typeof(long)] = (cPtr, dp, value) =>
            {
                CheckProperty(cPtr, dp, "set");
                Noesis_DependencySet_Int64(cPtr, dp, (long)value, false, false);
            };
            setFunctions[typeof(uint)] = (cPtr, dp, value) =>
            {
                CheckProperty(cPtr, dp, "set");
                Noesis_DependencySet_UInt(cPtr, dp, (uint)value, false, false);
            };
            setFunctions[typeof(ulong)] = (cPtr, dp, value) =>
            {
                CheckProperty(cPtr, dp, "set");
                Noesis_DependencySet_UInt64(cPtr, dp, (ulong)value, false, false);
            };
            setFunctions[typeof(char)] = (cPtr, dp, value) =>
            {
                CheckProperty(cPtr, dp, "set");
                Noesis_DependencySet_UInt(cPtr, dp, (uint)(char)value, false, false);
            };
            setFunctions[typeof(short)] = (cPtr, dp, value) =>
            {
                CheckProperty(cPtr, dp, "set");
                Noesis_DependencySet_Short(cPtr, dp, (short)value, false, false);
            };
            setFunctions[typeof(sbyte)] = (cPtr, dp, value) =>
            {
                CheckProperty(cPtr, dp, "set");
                Noesis_DependencySet_Short(cPtr, dp, (short)(sbyte)value, false, false);
            };
            setFunctions[typeof(ushort)] = (cPtr, dp, value) =>
            {
                CheckProperty(cPtr, dp, "set");
                Noesis_DependencySet_UShort(cPtr, dp, (ushort)value, false, false);
            };
            setFunctions[typeof(byte)] = (cPtr, dp, value) =>
            {
                CheckProperty(cPtr, dp, "set");
                Noesis_DependencySet_UShort(cPtr, dp, (ushort)(byte)value, false, false);
            };
            setFunctions[typeof(string)] = (cPtr, dp, value) =>
            {
                CheckProperty(cPtr, dp, "set");
                Noesis_DependencySet_String(cPtr, dp, value == null ? string.Empty : value.ToString());
            };
            setFunctions[typeof(System.Uri)] = (cPtr, dp, value) =>
            {
                CheckProperty(cPtr, dp, "set");
                Noesis_DependencySet_Uri(cPtr, dp, value == null ? string.Empty : ((Uri)value).OriginalString);
            };
            setFunctions[typeof(Noesis.Color)] = (cPtr, dp, value) =>
            {
                CheckProperty(cPtr, dp, "set");
                Noesis.Color value_ = (Noesis.Color)value;
                Noesis_DependencySet_Color(cPtr, dp, ref value_, false, false);
            };
            setFunctions[typeof(Noesis.Point)] = (cPtr, dp, value) =>
            {
                CheckProperty(cPtr, dp, "set");
                Noesis.Point value_ = (Noesis.Point)value;
                Noesis_DependencySet_Point(cPtr, dp, ref value_, false, false);
            };
            setFunctions[typeof(Noesis.Rect)] = (cPtr, dp, value) =>
            {
                CheckProperty(cPtr, dp, "set");
                Noesis.Rect value_ = (Noesis.Rect)value;
                Noesis_DependencySet_Rect(cPtr, dp, ref value_, false, false);
            };
            setFunctions[typeof(Noesis.Int32Rect)] = (cPtr, dp, value) =>
            {
                CheckProperty(cPtr, dp, "set");
                Noesis.Int32Rect value_ = (Noesis.Int32Rect)value;
                Noesis_DependencySet_Int32Rect(cPtr, dp, ref value_, false, false);
            };
            setFunctions[typeof(Noesis.Size)] = (cPtr, dp, value) =>
            {
                CheckProperty(cPtr, dp, "set");
                Noesis.Size value_ = (Noesis.Size)value;
                Noesis_DependencySet_Size(cPtr, dp, ref value_, false, false);
            };
            setFunctions[typeof(Noesis.Thickness)] = (cPtr, dp, value) =>
            {
                CheckProperty(cPtr, dp, "set");
                Noesis.Thickness value_ = (Noesis.Thickness)value;
                Noesis_DependencySet_Thickness(cPtr, dp, ref value_, false, false);
            };
            setFunctions[typeof(Noesis.CornerRadius)] = (cPtr, dp, value) =>
            {
                CheckProperty(cPtr, dp, "set");
                Noesis.CornerRadius value_ = (Noesis.CornerRadius)value;
                Noesis_DependencySet_CornerRadius(cPtr, dp, ref value_, false, false);
            };
            setFunctions[typeof(TimeSpan)] = (cPtr, dp, value) =>
            {
                CheckProperty(cPtr, dp, "set");
                Noesis.TimeSpanStruct value_ = (Noesis.TimeSpanStruct)((TimeSpan)value);
                Noesis_DependencySet_TimeSpan(cPtr, dp, ref value_, false, false);
            };
            setFunctions[typeof(Noesis.Duration)] = (cPtr, dp, value) =>
            {
                CheckProperty(cPtr, dp, "set");
                Noesis.Duration value_ = (Noesis.Duration)value;
                Noesis_DependencySet_Duration(cPtr, dp, ref value_, false, false);
            };
            setFunctions[typeof(Noesis.KeyTime)] = (cPtr, dp, value) =>
            {
                CheckProperty(cPtr, dp, "set");
                Noesis.KeyTime value_ = (Noesis.KeyTime)value;
                Noesis_DependencySet_KeyTime(cPtr, dp, ref value_, false, false);
            };
            setFunctions[typeof(bool?)] = (cPtr, dp, value) =>
            {
                CheckProperty(cPtr, dp, "set");
                if (value == null)
                {
                    Noesis_DependencySet_Bool(cPtr, dp, default(bool), true, true);
                }
                else
                {
                    Noesis_DependencySet_Bool(cPtr, dp, (bool)value, true, false);
                }
            };
            setFunctions[typeof(float?)] = (cPtr, dp, value) =>
            {
                CheckProperty(cPtr, dp, "set");
                if (value == null)
                {
                    Noesis_DependencySet_Float(cPtr, dp, default(float), true, true);
                }
                else
                {
                    Noesis_DependencySet_Float(cPtr, dp, (float)value, true, false);
                }
            };
            setFunctions[typeof(double?)] = (cPtr, dp, value) =>
            {
                CheckProperty(cPtr, dp, "set");
                if (value == null)
                {
                    Noesis_DependencySet_Double(cPtr, dp, default(double), true, true);
                }
                else
                {
                    Noesis_DependencySet_Double(cPtr, dp, (double)value, true, false);
                }
            };
            setFunctions[typeof(decimal?)] = (cPtr, dp, value) =>
            {
                CheckProperty(cPtr, dp, "set");
                if (value == null)
                {
                    Noesis_DependencySet_Double(cPtr, dp, default(double), true, true);
                }
                else
                {
                    Noesis_DependencySet_Double(cPtr, dp, (double)(decimal)value, true, false);
                }
            };
            setFunctions[typeof(int?)] = (cPtr, dp, value) =>
            {
                CheckProperty(cPtr, dp, "set");
                if (value == null)
                {
                    Noesis_DependencySet_Int(cPtr, dp, default(int), true, true);
                }
                else
                {
                    Noesis_DependencySet_Int(cPtr, dp, (int)value, true, false);
                }
            };
            setFunctions[typeof(long?)] = (cPtr, dp, value) =>
            {
                CheckProperty(cPtr, dp, "set");
                if (value == null)
                {
                    Noesis_DependencySet_Int64(cPtr, dp, default(long), true, true);
                }
                else
                {
                    Noesis_DependencySet_Int64(cPtr, dp, (long)value, true, false);
                }
            };
            setFunctions[typeof(uint?)] = (cPtr, dp, value) =>
            {
                CheckProperty(cPtr, dp, "set");
                if (value == null)
                {
                    Noesis_DependencySet_UInt(cPtr, dp, default(uint), true, true);
                }
                else
                {
                    Noesis_DependencySet_UInt(cPtr, dp, (uint)value, true, false);
                }
            };
            setFunctions[typeof(ulong?)] = (cPtr, dp, value) =>
            {
                CheckProperty(cPtr, dp, "set");
                if (value == null)
                {
                    Noesis_DependencySet_UInt64(cPtr, dp, default(ulong), true, true);
                }
                else
                {
                    Noesis_DependencySet_UInt64(cPtr, dp, (ulong)value, true, false);
                }
            };
            setFunctions[typeof(char?)] = (cPtr, dp, value) =>
            {
                CheckProperty(cPtr, dp, "set");
                if (value == null)
                {
                    Noesis_DependencySet_UInt(cPtr, dp, default(uint), true, true);
                }
                else
                {
                    Noesis_DependencySet_UInt(cPtr, dp, (uint)(char)value, true, false);
                }
            };
            setFunctions[typeof(short?)] = (cPtr, dp, value) =>
            {
                CheckProperty(cPtr, dp, "set");
                if (value == null)
                {
                    Noesis_DependencySet_Short(cPtr, dp, default(short), true, true);
                }
                else
                {
                    Noesis_DependencySet_Short(cPtr, dp, (short)value, true, false);
                }
            };
            setFunctions[typeof(sbyte?)] = (cPtr, dp, value) =>
            {
                CheckProperty(cPtr, dp, "set");
                if (value == null)
                {
                    Noesis_DependencySet_Short(cPtr, dp, default(short), true, true);
                }
                else
                {
                    Noesis_DependencySet_Short(cPtr, dp, (short)(sbyte)value, true, false);
                }
            };
            setFunctions[typeof(ushort?)] = (cPtr, dp, value) =>
            {
                CheckProperty(cPtr, dp, "set");
                if (value == null)
                {
                    Noesis_DependencySet_UShort(cPtr, dp, default(ushort), true, true);
                }
                else
                {
                    Noesis_DependencySet_UShort(cPtr, dp, (ushort)value, true, false);
                }
            };
            setFunctions[typeof(byte?)] = (cPtr, dp, value) =>
            {
                CheckProperty(cPtr, dp, "set");
                if (value == null)
                {
                    Noesis_DependencySet_UShort(cPtr, dp, default(ushort), true, true);
                }
                else
                {
                    Noesis_DependencySet_UShort(cPtr, dp, (ushort)(byte)value, true, false);
                }
            };
            setFunctions[typeof(Noesis.Color?)] = (cPtr, dp, value) =>
            {
                CheckProperty(cPtr, dp, "set");
                if (value == null)
                {
                    Noesis.Color value_ = default(Noesis.Color);
                    Noesis_DependencySet_Color(cPtr, dp, ref value_, true, true);
                }
                else
                {
                    Noesis.Color value_ = (Noesis.Color)value;
                    Noesis_DependencySet_Color(cPtr, dp, ref value_, true, false);
                }
            };
            setFunctions[typeof(Noesis.Point?)] = (cPtr, dp, value) =>
            {
                CheckProperty(cPtr, dp, "set");
                if (value == null)
                {
                    Noesis.Point value_ = default(Noesis.Point);
                    Noesis_DependencySet_Point(cPtr, dp, ref value_, true, true);
                }
                else
                {
                    Noesis.Point value_ = (Noesis.Point)value;
                    Noesis_DependencySet_Point(cPtr, dp, ref value_, true, false);
                }
            };
            setFunctions[typeof(Noesis.Rect?)] = (cPtr, dp, value) =>
            {
                CheckProperty(cPtr, dp, "set");
                if (value == null)
                {
                    Noesis.Rect value_ = default(Noesis.Rect);
                    Noesis_DependencySet_Rect(cPtr, dp, ref value_, true, true);
                }
                else
                {
                    Noesis.Rect value_ = (Noesis.Rect)value;
                    Noesis_DependencySet_Rect(cPtr, dp, ref value_, true, false);
                }
            };
            setFunctions[typeof(Noesis.Int32Rect?)] = (cPtr, dp, value) =>
            {
                CheckProperty(cPtr, dp, "set");
                if (value == null)
                {
                    Noesis.Int32Rect value_ = default(Noesis.Int32Rect);
                    Noesis_DependencySet_Int32Rect(cPtr, dp, ref value_, true, true);
                }
                else
                {
                    Noesis.Int32Rect value_ = (Noesis.Int32Rect)value;
                    Noesis_DependencySet_Int32Rect(cPtr, dp, ref value_, true, false);
                }
            };
            setFunctions[typeof(Noesis.Size?)] = (cPtr, dp, value) =>
            {
                CheckProperty(cPtr, dp, "set");
                if (value == null)
                {
                    Noesis.Size value_ = default(Noesis.Size);
                    Noesis_DependencySet_Size(cPtr, dp, ref value_, true, true);
                }
                else
                {
                    Noesis.Size value_ = (Noesis.Size)value;
                    Noesis_DependencySet_Size(cPtr, dp, ref value_, true, false);
                }
            };
            setFunctions[typeof(Noesis.Thickness?)] = (cPtr, dp, value) =>
            {
                CheckProperty(cPtr, dp, "set");
                if (value == null)
                {
                    Noesis.Thickness value_ = default(Noesis.Thickness);
                    Noesis_DependencySet_Thickness(cPtr, dp, ref value_, true, true);
                }
                else
                {
                    Noesis.Thickness value_ = (Noesis.Thickness)value;
                    Noesis_DependencySet_Thickness(cPtr, dp, ref value_, true, false);
                }
            };
            setFunctions[typeof(Noesis.CornerRadius?)] = (cPtr, dp, value) =>
            {
                CheckProperty(cPtr, dp, "set");
                if (value == null)
                {
                    Noesis.CornerRadius value_ = default(Noesis.CornerRadius);
                    Noesis_DependencySet_CornerRadius(cPtr, dp, ref value_, true, true);
                }
                else
                {
                    Noesis.CornerRadius value_ = (Noesis.CornerRadius)value;
                    Noesis_DependencySet_CornerRadius(cPtr, dp, ref value_, true, false);
                }
            };
            setFunctions[typeof(TimeSpan?)] = (cPtr, dp, value) =>
            {
                CheckProperty(cPtr, dp, "set");
                if (value == null)
                {
                    Noesis.TimeSpanStruct value_ = default(Noesis.TimeSpanStruct);
                    Noesis_DependencySet_TimeSpan(cPtr, dp, ref value_, true, true);
                }
                else
                {
                    Noesis.TimeSpanStruct value_ = (Noesis.TimeSpanStruct)((TimeSpan)value);
                    Noesis_DependencySet_TimeSpan(cPtr, dp, ref value_, true, false);
                }
            };
            setFunctions[typeof(Noesis.Duration?)] = (cPtr, dp, value) =>
            {
                CheckProperty(cPtr, dp, "set");
                if (value == null)
                {
                    Noesis.Duration value_ = default(Noesis.Duration);
                    Noesis_DependencySet_Duration(cPtr, dp, ref value_, true, true);
                }
                else
                {
                    Noesis.Duration value_ = (Noesis.Duration)value;
                    Noesis_DependencySet_Duration(cPtr, dp, ref value_, true, false);
                }
            };
            setFunctions[typeof(Noesis.KeyTime?)] = (cPtr, dp, value) =>
            {
                CheckProperty(cPtr, dp, "set");
                if (value == null)
                {
                    Noesis.KeyTime value_ = default(Noesis.KeyTime);
                    Noesis_DependencySet_KeyTime(cPtr, dp, ref value_, true, true);
                }
                else
                {
                    Noesis.KeyTime value_ = (Noesis.KeyTime)value;
                    Noesis_DependencySet_KeyTime(cPtr, dp, ref value_, true, false);
                }
            };
            setFunctions[typeof(Type)] = (cPtr, dp, value) =>
            {
                CheckProperty(cPtr, dp, "set");
                System.Type value_ = (System.Type)value;
                IntPtr ptr = value != null ? Noesis.Extend.GetNativeType(value_) : IntPtr.Zero;
                Noesis_DependencySet_Type(cPtr, dp, ptr);
            };

            return setFunctions;
        }

        private static void CheckProperty(IntPtr dependencyObject, IntPtr dependencyProperty, string msg)
        {
            if (dependencyObject == IntPtr.Zero)
            {
                throw new Exception("Can't " + msg + " value, DependencyObject is null");
            }

            if (dependencyProperty == IntPtr.Zero)
            {
                throw new Exception("Can't " + msg + " value, DependencyProperty is null");
            }
        }

        #endregion

        #region Enum properties
        public void EnumProperties(EnumDependencyPropertiesDelegate callback)
        {
            EnumPropsInfo info = new EnumPropsInfo { Callback = callback };
            int id = info.GetHashCode();
            _enumPropsInfo[id] = info;
            Noesis_Dependency_EnumProps(swigCPtr.Handle, id, _enumProps);
            _enumPropsInfo.Remove(id);
        }

        private delegate void NoesisEnumPropertiesCallback(int id, IntPtr dpPtr, IntPtr valPtr);
        private static NoesisEnumPropertiesCallback _enumProps = OnEnumProperties;
        [MonoPInvokeCallback(typeof(NoesisEnumPropertiesCallback))]
        private static void OnEnumProperties(int id, IntPtr dpPtr, IntPtr valPtr)
        {
            try
            {
                EnumPropsInfo info = _enumPropsInfo[id];
                DependencyProperty dp = (DependencyProperty)Noesis.Extend.GetProxy(dpPtr, false);
                object val = Noesis.Extend.GetProxy(valPtr, true);
                info.Callback(dp, val);
            }
            catch (Exception e)
            {
                Error.UnhandledException(e);
            }
        }

        private struct EnumPropsInfo
        {
            public EnumDependencyPropertiesDelegate Callback;
        }

        private static Dictionary<int, EnumPropsInfo> _enumPropsInfo = new Dictionary<int, EnumPropsInfo>();

        #endregion

        #region Destroyed event
        public delegate void DestroyedHandler(IntPtr d);
        public event DestroyedHandler Destroyed
        {
            add
            {
                long ptr = swigCPtr.Handle.ToInt64();
                if (!_Destroyed.ContainsKey(ptr))
                {
                    _Destroyed.Add(ptr, null);

                    Noesis_Dependency_Destroyed_Bind(_raiseDestroyed, swigCPtr);
                }

                _Destroyed[ptr] += value;
            }
            remove
            {
                long ptr = swigCPtr.Handle.ToInt64();
                if (_Destroyed.ContainsKey(ptr))
                {

                    _Destroyed[ptr] -= value;

                    if (_Destroyed[ptr] == null)
                    {
                        Noesis_Dependency_Destroyed_Unbind(_raiseDestroyed, swigCPtr);

                        _Destroyed.Remove(ptr);
                    }
                }
            }
        }

        internal delegate void RaiseDestroyedCallback(IntPtr cPtr);
        private static RaiseDestroyedCallback _raiseDestroyed = RaiseDestroyed;

        [MonoPInvokeCallback(typeof(RaiseDestroyedCallback))]
        private static void RaiseDestroyed(IntPtr cPtr)
        {
            try
            {
                if (Noesis.Extend.Initialized)
                {
                    long ptr = cPtr.ToInt64();
                    DestroyedHandler handler = null;
                    if (_Destroyed.TryGetValue(ptr, out handler))
                    {
                        handler?.Invoke(cPtr);
                        _Destroyed.Remove(ptr);
                    }
                }
            }
            catch (Exception exception)
            {
                Noesis.Error.UnhandledException(exception);
            }
        }

        internal static Dictionary<long, DestroyedHandler> _Destroyed =
            new Dictionary<long, DestroyedHandler>();
        #endregion

        #region Imports

        ////////////////////////////////////////////////////////////////////////////////////////////////
        [DllImport(Library.Name)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool Noesis_DependencyGet_Bool(IntPtr dependencyObject, IntPtr dependencyProperty,
            bool isNullable, [MarshalAs(UnmanagedType.U1)]out bool isNull);

        [DllImport(Library.Name)]
        private static extern float Noesis_DependencyGet_Float(IntPtr dependencyObject, IntPtr dependencyProperty,
            bool isNullable, [MarshalAs(UnmanagedType.U1)]out bool isNull);

        [DllImport(Library.Name)]
        private static extern double Noesis_DependencyGet_Double(IntPtr dependencyObject, IntPtr dependencyProperty,
            bool isNullable, [MarshalAs(UnmanagedType.U1)]out bool isNull);

        [DllImport(Library.Name)]
        private static extern long Noesis_DependencyGet_Int64(IntPtr dependencyObject, IntPtr dependencyProperty,
            bool isNullable, [MarshalAs(UnmanagedType.U1)] out bool isNull);

        [DllImport(Library.Name)]
        private static extern ulong Noesis_DependencyGet_UInt64(IntPtr dependencyObject, IntPtr dependencyProperty,
            bool isNullable, [MarshalAs(UnmanagedType.U1)] out bool isNull);

        [DllImport(Library.Name)]
        private static extern int Noesis_DependencyGet_Int(IntPtr dependencyObject, IntPtr dependencyProperty,
            bool isNullable, [MarshalAs(UnmanagedType.U1)]out bool isNull);

        [DllImport(Library.Name)]
        private static extern uint Noesis_DependencyGet_UInt(IntPtr dependencyObject, IntPtr dependencyProperty,
            bool isNullable, [MarshalAs(UnmanagedType.U1)]out bool isNull);

        [DllImport(Library.Name)]
        private static extern short Noesis_DependencyGet_Short(IntPtr dependencyObject, IntPtr dependencyProperty,
            bool isNullable, [MarshalAs(UnmanagedType.U1)]out bool isNull);

        [DllImport(Library.Name)]
        private static extern ushort Noesis_DependencyGet_UShort(IntPtr dependencyObject, IntPtr dependencyProperty,
            bool isNullable, [MarshalAs(UnmanagedType.U1)]out bool isNull);

        [DllImport(Library.Name)]
        private static extern IntPtr Noesis_DependencyGet_String(IntPtr dependencyObject, IntPtr dependencyProperty);

        [DllImport(Library.Name)]
        private static extern IntPtr Noesis_DependencyGet_Uri(IntPtr dependencyObject, IntPtr dependencyProperty);

        [DllImport(Library.Name)]
        private static extern IntPtr Noesis_DependencyGet_Color(IntPtr dependencyObject, IntPtr dependencyProperty,
            bool isNullable, [MarshalAs(UnmanagedType.U1)]out bool isNull);

        [DllImport(Library.Name)]
        private static extern IntPtr Noesis_DependencyGet_Point(IntPtr dependencyObject, IntPtr dependencyProperty,
            bool isNullable, [MarshalAs(UnmanagedType.U1)]out bool isNull);

        [DllImport(Library.Name)]
        private static extern IntPtr Noesis_DependencyGet_Rect(IntPtr dependencyObject, IntPtr dependencyProperty,
            bool isNullable, [MarshalAs(UnmanagedType.U1)]out bool isNull);

        [DllImport(Library.Name)]
        private static extern IntPtr Noesis_DependencyGet_Int32Rect(IntPtr dependencyObject, IntPtr dependencyProperty,
            bool isNullable, [MarshalAs(UnmanagedType.U1)] out bool isNull);

        [DllImport(Library.Name)]
        private static extern IntPtr Noesis_DependencyGet_Size(IntPtr dependencyObject, IntPtr dependencyProperty,
            bool isNullable, [MarshalAs(UnmanagedType.U1)]out bool isNull);

        [DllImport(Library.Name)]
        private static extern IntPtr Noesis_DependencyGet_Thickness(IntPtr dependencyObject, IntPtr dependencyProperty,
            bool isNullable, [MarshalAs(UnmanagedType.U1)]out bool isNull);

        [DllImport(Library.Name)]
        private static extern IntPtr Noesis_DependencyGet_CornerRadius(IntPtr dependencyObject, IntPtr dependencyProperty,
            bool isNullable, [MarshalAs(UnmanagedType.U1)]out bool isNull);

        [DllImport(Library.Name)]
        private static extern IntPtr Noesis_DependencyGet_TimeSpan(IntPtr dependencyObject, IntPtr dependencyProperty,
            bool isNullable, [MarshalAs(UnmanagedType.U1)]out bool isNull);

        [DllImport(Library.Name)]
        private static extern IntPtr Noesis_DependencyGet_Duration(IntPtr dependencyObject, IntPtr dependencyProperty,
            bool isNullable, [MarshalAs(UnmanagedType.U1)]out bool isNull);

        [DllImport(Library.Name)]
        private static extern IntPtr Noesis_DependencyGet_KeyTime(IntPtr dependencyObject, IntPtr dependencyProperty,
            bool isNullable, [MarshalAs(UnmanagedType.U1)]out bool isNull);

        [DllImport(Library.Name)]
        private static extern IntPtr Noesis_DependencyGet_Type(IntPtr dependencyObject, IntPtr dependencyProperty);

        [DllImport(Library.Name)]
        private static extern IntPtr Noesis_DependencyGet_BaseComponent(IntPtr dependencyObject, IntPtr dependencyProperty);

        ////////////////////////////////////////////////////////////////////////////////////////////////
        [DllImport(Library.Name)]
        private static extern void Noesis_DependencySet_Bool(IntPtr dependencyObject, IntPtr dependencyProperty,
            bool val, bool isNullable, bool isNull);

        [DllImport(Library.Name)]
        private static extern void Noesis_DependencySet_Float(IntPtr dependencyObject, IntPtr dependencyProperty,
            float val, bool isNullable, bool isNull);

        [DllImport(Library.Name)]
        private static extern void Noesis_DependencySet_Double(IntPtr dependencyObject, IntPtr dependencyProperty,
            double val, bool isNullable, bool isNull);

        [DllImport(Library.Name)]
        private static extern void Noesis_DependencySet_Int64(IntPtr dependencyObject, IntPtr dependencyProperty,
            long val, bool isNullable, bool isNull);

        [DllImport(Library.Name)]
        private static extern void Noesis_DependencySet_UInt64(IntPtr dependencyObject, IntPtr dependencyProperty,
            ulong val, bool isNullable, bool isNull);

        [DllImport(Library.Name)]
        private static extern void Noesis_DependencySet_Int(IntPtr dependencyObject, IntPtr dependencyProperty,
            int val, bool isNullable, bool isNull);

        [DllImport(Library.Name)]
        private static extern void Noesis_DependencySet_UInt(IntPtr dependencyObject, IntPtr dependencyProperty,
            uint val, bool isNullable, bool isNull);

        [DllImport(Library.Name)]
        private static extern void Noesis_DependencySet_Short(IntPtr dependencyObject, IntPtr dependencyProperty,
            short val, bool isNullable, bool isNull);

        [DllImport(Library.Name)]
        private static extern void Noesis_DependencySet_UShort(IntPtr dependencyObject, IntPtr dependencyProperty,
            ushort val, bool isNullable, bool isNull);

        [DllImport(Library.Name)]
        private static extern void Noesis_DependencySet_String(IntPtr dependencyObject, IntPtr dependencyProperty,
            [MarshalAs(UnmanagedType.LPWStr)]string val);

        [DllImport(Library.Name)]
        private static extern void Noesis_DependencySet_Uri(IntPtr dependencyObject, IntPtr dependencyProperty,
            [MarshalAs(UnmanagedType.LPWStr)]string val);

        [DllImport(Library.Name)]
        private static extern void Noesis_DependencySet_Color(IntPtr dependencyObject, IntPtr dependencyProperty,
            ref Noesis.Color val, bool isNullable, bool isNull);

        [DllImport(Library.Name)]
        private static extern void Noesis_DependencySet_Point(IntPtr dependencyObject, IntPtr dependencyProperty,
            ref Noesis.Point val, bool isNullable, bool isNull);

        [DllImport(Library.Name)]
        private static extern void Noesis_DependencySet_Rect(IntPtr dependencyObject, IntPtr dependencyProperty,
            ref Noesis.Rect val, bool isNullable, bool isNull);

        [DllImport(Library.Name)]
        private static extern void Noesis_DependencySet_Int32Rect(IntPtr dependencyObject, IntPtr dependencyProperty,
            ref Noesis.Int32Rect val, bool isNullable, bool isNull);

        [DllImport(Library.Name)]
        private static extern void Noesis_DependencySet_Size(IntPtr dependencyObject, IntPtr dependencyProperty,
            ref Noesis.Size val, bool isNullable, bool isNull);

        [DllImport(Library.Name)]
        private static extern void Noesis_DependencySet_Thickness(IntPtr dependencyObject, IntPtr dependencyProperty,
            ref Noesis.Thickness val, bool isNullable, bool isNull);

        [DllImport(Library.Name)]
        private static extern void Noesis_DependencySet_CornerRadius(IntPtr dependencyObject, IntPtr dependencyProperty,
            ref Noesis.CornerRadius val, bool isNullable, bool isNull);

        [DllImport(Library.Name)]
        private static extern void Noesis_DependencySet_TimeSpan(IntPtr dependencyObject, IntPtr dependencyProperty,
            ref Noesis.TimeSpanStruct val, bool isNullable, bool isNull);

        [DllImport(Library.Name)]
        private static extern void Noesis_DependencySet_Duration(IntPtr dependencyObject, IntPtr dependencyProperty,
            ref Noesis.Duration val, bool isNullable, bool isNull);

        [DllImport(Library.Name)]
        private static extern void Noesis_DependencySet_KeyTime(IntPtr dependencyObject, IntPtr dependencyProperty,
            ref Noesis.KeyTime val, bool isNullable, bool isNull);

        [DllImport(Library.Name)]
        private static extern void Noesis_DependencySet_Type(IntPtr dependencyObject, IntPtr dependencyProperty,
            IntPtr val);

        [DllImport(Library.Name)]
        private static extern void Noesis_DependencySet_BaseComponent(IntPtr dependencyObject, IntPtr dependencyProperty,
            IntPtr val);

        ////////////////////////////////////////////////////////////////////////////////////////////////
        [DllImport(Library.Name)]
        private static extern void Noesis_Dependency_EnumProps(IntPtr dependencyObject, int id,
            NoesisEnumPropertiesCallback callback);

        ////////////////////////////////////////////////////////////////////////////////////////////////
        [DllImport(Library.Name)]
        private static extern void Noesis_Dependency_Destroyed_Bind(RaiseDestroyedCallback callback, HandleRef instance);

        [DllImport(Library.Name)]
        private static extern void Noesis_Dependency_Destroyed_Unbind(RaiseDestroyedCallback callback, HandleRef instance);

        #endregion
    }

}
