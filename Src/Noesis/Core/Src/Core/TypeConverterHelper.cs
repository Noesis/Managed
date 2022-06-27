using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.InteropServices;

namespace Noesis
{
    public class BrushConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string)
            {
                return Brush.Parse((string)value);
            }

            return base.ConvertFrom(context, culture, value);
        }
    }

    public class GeometryConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string)
            {
                return Geometry.Parse((string)value);
            }

            return base.ConvertFrom(context, culture, value);
        }
    }

    public class TransformConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string)
            {
                return Transform.Parse((string)value);
            }

            return base.ConvertFrom(context, culture, value);
        }
    }

    public class MouseActionConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string)
            {
                return MouseBinding.ParseMouseAction((string)value);
            }

            return base.ConvertFrom(context, culture, value);
        }
    }

    public class KeyConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string)
            {
                return KeyBinding.ParseKey((string)value);
            }

            return base.ConvertFrom(context, culture, value);
        }
    }

    public class ModifierKeysConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string)
            {
                return KeyBinding.ParseModifierKeys((string)value);
            }

            return base.ConvertFrom(context, culture, value);
        }
    }

    public class KeyGestureConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string)
            {
                return KeyBinding.ParseKeyGesture((string)value);
            }

            return base.ConvertFrom(context, culture, value);
        }
    }

    public class MouseGestureConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string)
            {
                return MouseBinding.ParseMouseGesture((string)value);
            }

            return base.ConvertFrom(context, culture, value);
        }
    }
}

