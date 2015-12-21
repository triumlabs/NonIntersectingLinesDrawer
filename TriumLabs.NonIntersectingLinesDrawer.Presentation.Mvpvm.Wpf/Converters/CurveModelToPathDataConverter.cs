using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
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

            string pathData;
            var mode = (parameter as string ?? "continuous").ToUpperInvariant();
            switch (mode)
            {
                case "CONTINUOUS":
                    var sb = new StringBuilder();
                    var modelPointFirst = modelCurve.Points.FirstOrDefault();
                    if (modelPointFirst != null)
                        sb.AppendFormat(CultureInfo.InvariantCulture, "M{0},{1} ", modelPointFirst.X, modelPointFirst.Y);

                    for (var idx = 1; idx < modelCurve.Points.Count - 1; idx++)
                    {
                        var modelPointPrev = modelCurve.Points[idx - 1];
                        var modelPointMid = modelCurve.Points[idx];
                        var modelPointNext = modelCurve.Points[idx + 1];
                        var segmentPrev = new Vector(
                            modelPointMid.X - modelPointPrev.X,
                            modelPointMid.Y - modelPointPrev.Y);
                        var segmentNext = new Vector(
                            modelPointNext.X - modelPointMid.X,
                            modelPointNext.Y - modelPointMid.Y);
                        var vectorMid = new Vector(modelPointMid.X, modelPointMid.Y);

                        var deltaPrev = 20.0 * segmentPrev / segmentPrev.Length;
                        var startVector = vectorMid - deltaPrev;
                        if (deltaPrev.Length * 2 < segmentPrev.Length)
                            sb.AppendFormat(CultureInfo.InvariantCulture, "L{0},{1} ", startVector.X, startVector.Y);

                        var deltaNext = 20.0 * segmentNext / segmentNext.Length;
                        var endVector = vectorMid + deltaNext;

                        sb.AppendFormat(CultureInfo.InvariantCulture, "Q{0},{1} {2},{3} ", vectorMid.X, vectorMid.Y, endVector.X, endVector.Y);
                    }

                    var modelPointLast = modelCurve.Points.LastOrDefault();
                    if (modelPointLast != null)
                        sb.AppendFormat(CultureInfo.InvariantCulture, "L{0},{1} ", modelPointLast.X, modelPointLast.Y);
                    
                    pathData = sb.ToString();
                    break;
                default:
                    pathData = String.Join(
                        " ",
                        modelCurve.Points
                        .Select((point, idx) => String.Format(CultureInfo.InvariantCulture, "{0}{1},{2}", idx == 0 ? "M" : "L", point.X, point.Y)));
                    break;
            }
            
            return Geometry.Parse(pathData);
        }
    }
}
