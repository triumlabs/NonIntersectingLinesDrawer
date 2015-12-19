using System;
using System.Globalization;
using System.Linq;
using System.Windows.Media;
using TriumLabs.NonIntersectingLinesDrawer.Presentation.Mvpvm.ViewModels;

namespace TriumLabs.NonIntersectingLinesDrawer.Presentation.Mvpvm.Wpf.Converters
{
    /// <summary>
    /// Converts a curve model into a continuous path.
    /// </summary>
    public sealed class CurveModelToPathDataConverter : ConverterBase
    {
        /// <summary>
        /// Converts a value.
        /// </summary>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>A converted value. If the method returns null, the valid null value is used.</returns>
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var modelCurve = value as CurveModel;
            if (modelCurve == null) return null;

            var pathData = String.Join(
                " ",
                modelCurve.Points
                .Select((point, idx) => String.Concat(idx == 0 ? "M" : "L", point.X, ",", point.Y)));
            return Geometry.Parse(pathData);
        }
    }
}
