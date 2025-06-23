using System;
using System.Globalization;

namespace Noesis
{
    /// <summary>Provides a way to apply custom logic to a binding.</summary>
    public interface IValueConverter
    {
        /// <summary>Converts a value. </summary>
        /// <returns>A converted value. If the method returns null, the valid null value is used.</returns>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        object Convert(object value, Type targetType, object parameter, CultureInfo culture);

        /// <summary>Converts a value. </summary>
        /// <returns>A converted value. If the method returns null, the valid null value is used.</returns>
        /// <param name="value">The value that is produced by the binding target.</param>
        /// <param name="targetType">The type to convert to.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture);
    }

    internal class NativeValueConverter: BaseComponent, IValueConverter
    {
        internal NativeValueConverter(IntPtr cPtr, bool cMemoryOwn) : base(cPtr, cMemoryOwn) { }

        internal new static NativeValueConverter CreateProxy(IntPtr cPtr, bool cMemoryOwn)
        {
            return new NativeValueConverter(cPtr, cMemoryOwn);
        }

        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
