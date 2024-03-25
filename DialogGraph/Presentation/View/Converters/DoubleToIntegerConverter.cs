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
    /// Converter from <see cref="int"/> to <see cref="double"/> and vice versa.
    /// </summary>
    internal sealed class IntegerToDoubleConverter : IValueConverter
    {
        #region IValueConverter

        /// <inheritdoc />
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int integer)
                return (double)integer;

            throw new ArgumentException(
                $"{nameof(IntegerToDoubleConverter)} must take an int in parameter.",
                nameof(value));
        }

        /// <inheritdoc />
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {

            if (value is double d)
                return Math.Round(d);

            throw new ArgumentException(
                $"{nameof(IntegerToDoubleConverter)} back conversion must take a double in parameter.",
                nameof(value));
        }

        #endregion
    }
}