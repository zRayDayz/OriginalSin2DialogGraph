using System.Globalization;
using System.Windows.Data;

/* This file is part of the MIT C# repository on GitHub: https://github.com/KeRNeLith/GraphShape/
 * Sub-project: https://github.com/KeRNeLith/GraphShape/tree/master/samples/GraphShape.Sample
 * Original author: KeRNeLith (https://github.com/KeRNeLith)
 * License: MIT License
 */
namespace DialogGraph
{
    /// <summary>
    /// Converter that checks equality between value and parameter.
    /// </summary>
    internal sealed class EqualityToBooleanConverter : IValueConverter
    {
        /// <inheritdoc />
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Equals(value, parameter);
        }

        /// <inheritdoc />
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is true)
                return parameter;
            throw new NotSupportedException($"{nameof(EqualityToBooleanConverter)}: Can't bind back.");
        }
    }
}