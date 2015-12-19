using System;
using System.Windows;
using TriumLabs.NonIntersectingLinesDrawer.Presentation.Mvpvm.ViewModels;

namespace TriumLabs.NonIntersectingLinesDrawer.Presentation.Mvpvm.Wpf.Converters
{
    /// <summary>
    /// Converts the XY-coordinates of a point model into a point.
    /// </summary>
    public sealed class PointModelToPointConverter : ConverterBase
    {
        /// <summary>
        /// Converts a value.
        /// </summary>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>A converted value. If the method returns null, the valid null value is used.</returns>
        public override object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var modelPoint = value as PointModel;
            if (modelPoint == null) return null;

            return new Point(modelPoint.X, modelPoint.Y);
        }
    }
}
