using System;
using System.Globalization;

namespace Noesis
{
    /// <summary>
    /// Used by MultiBinding to convert and combine source values to target values and to
    /// convert and split target values to source values.
    /// </summary>
    public interface IMultiValueConverter
    {
        /// <summary>Called when moving values from sources to target.</summary>
        /// <param name="values">Array of values, as produced by source bindings. UnsetValue may
        /// be passed to indicate that the source binding has no value to provide for conversion.</param>
        /// <param name="targetType">Target type.</param>
        /// <param name="parameter">Converter parameter.</param>
        /// <param name="culture">Culture information.</param>
        /// <returns>Converted value. UnsetValue may be returned to indicate that the converter
        /// produced no value and that the fallback or default value should be used instead.
        /// Binding.DoNothing may be returned to indicate that the binding should not transfer the value
        /// or use the fallback or default value.</returns>
        object Convert(object[] values, Type targetType, object parameter, CultureInfo culture);

        /// <summary>Called when moving a value from target to sources.</summary>
        /// <param name="value">Value as produced by target.</param>
        /// <param name="targetTypes">Array of target types; array length indicates the number
        /// and types of values suggested for Convert to return.</param>
        /// <param name="parameter">Converter parameter.</param>
        /// <param name="culture">Culture information.</param>
        /// <returns>Array of converted back values. If there are more return values than source
        /// bindings, the excess portion of return values will be ignored. If there are more
        /// source bindings than return values, the remaining source bindings will not have any value
        /// set to them. Types of return values are not verified against targetTypes.
        /// Binding.DoNothing may be returned in position i to indicate that no value should be set
        /// on the source binding at index i.
        /// UnsetValue may be returned in position i to indicate that the converter is unable to
        /// provide a value to the source binding at index i, and no value will be set to it.
        /// ConvertBack may return null to indicate that the conversion could not be performed at all,
        /// or that the backward conversion direction is not supported by the converter.</returns>
        object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture);
    }
}
