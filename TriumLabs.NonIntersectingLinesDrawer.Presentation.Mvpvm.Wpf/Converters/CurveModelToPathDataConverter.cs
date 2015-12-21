using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using TriumLabs.NonIntersectingLinesDrawer.Presentation.Mvpvm.ViewModels;
using TriumLabs.NonIntersectingLinesDrawer.BusinessServices;

namespace TriumLabs.NonIntersectingLinesDrawer.Presentation.Mvpvm.Wpf.Converters
{
    /// <summary>
    /// Converts a curve model into a path.
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
                // Converts a curve model into a continuous path
                case "CONTINUOUS":
                    var sb = new StringBuilder();

                    // Sets the path initial point to the curve's first point
                    var modelPointFirst = modelCurve.Points.FirstOrDefault();
                    if (modelPointFirst != null)
                        sb.AppendFormat(CultureInfo.InvariantCulture, "M{0},{1} ", modelPointFirst.X, modelPointFirst.Y);

                    // Replaces curve's inner points (edges) with small quadratic Bezier curve as
                    // - introduces a new inner point 20px far from edge on segment ahead if segment is shorter, no inner point is placed
                    // - defines a quadratic Bezier curve by defining the edge point as control point
                    // - defines a new inner point 20px far from edge on segment next as end point
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

                        var deltaPrev = segmentPrev.NormalizeTo(20.0);
                        var startVector = vectorMid - deltaPrev;
                        if (deltaPrev.Length * 2 < segmentPrev.Length)
                            sb.AppendFormat(CultureInfo.InvariantCulture, "L{0},{1} ", startVector.X, startVector.Y);

                        var deltaNext = segmentNext.NormalizeTo(20.0);
                        var endVector = vectorMid + deltaNext;

                        sb.AppendFormat(CultureInfo.InvariantCulture, "Q{0},{1} {2},{3} ", vectorMid.X, vectorMid.Y, endVector.X, endVector.Y);
                    }

                    // Sets the path last point to the curve's last point
                    var modelPointLast = modelCurve.Points.LastOrDefault();
                    if (modelPointLast != null)
                        sb.AppendFormat(CultureInfo.InvariantCulture, "L{0},{1} ", modelPointLast.X, modelPointLast.Y);
                    
                    pathData = sb.ToString();
                    break;

                // Converts a curve model into a simple path having edges
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
