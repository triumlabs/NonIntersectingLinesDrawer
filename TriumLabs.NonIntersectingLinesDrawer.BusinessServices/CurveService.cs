using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using TriumLabs.Core.Collections;
using TriumLabs.NonIntersectingLinesDrawer.BusinessServices.Model;

namespace TriumLabs.NonIntersectingLinesDrawer.BusinessServices
{
    /// <summary>
    /// Represents the Curve service.
    /// </summary>
    public class CurveService : ICurveService
    {
        /// <summary>
        /// Finds a curve between the given two points which does not intersect any other provided curves.
        /// </summary>
        /// <param name="vectorA">The base vector of point A.</param>
        /// <param name="vectorB">The base vector of point B.</param>
        /// <param name="curves">The list of curves which the new curve should not intersect.</param>
        /// <returns>The non-intersecting curve if it exists; otherwise null.</returns>
        public Curve FindNonIntersectingCurve(Vector vectorA, Vector vectorB, IEnumerable<Curve> curves)
        {
            var lineSegmentAB = new LineSegment { StartVector = vectorA, EndVector = vectorB };

            var curveSegments = curves
                .SelectMany(curve => curve.Segments)
                .ToArray();
            var isIntersecting = curveSegments.Any(lineSegmentCD => DetectLineSegmentsIntersection(lineSegmentAB, lineSegmentCD));
            if (!isIntersecting)
                return new Curve(new[] { lineSegmentAB });

            var rotationAngle = Math.PI / 3.0;
            var alternateVectors = curves
                .SelectMany(curve =>
                {
                    return curve.Segments.Take(1).Concat(curve.Segments.Last())
                        .SelectMany((segment, idx) =>
                        {
                            var segmentVector = segment.EndVector - segment.StartVector;
                            var deltaVector = new Vector(
                                20.0 * segmentVector.X / segmentVector.Length,
                                20.0 * segmentVector.Y / segmentVector.Length);
                            // Rotate a vector, see https://en.wikipedia.org/wiki/Rotation_(mathematics)
                            var deltaVectorA = new Vector(
                                deltaVector.X * Math.Cos(rotationAngle) - deltaVector.Y * Math.Sin(rotationAngle),
                                deltaVector.X * Math.Sin(rotationAngle) + deltaVector.Y * Math.Cos(rotationAngle));
                            var deltaVectorB = new Vector(
                                deltaVector.X * Math.Cos(-rotationAngle) - deltaVector.Y * Math.Sin(-rotationAngle),
                                deltaVector.X * Math.Sin(-rotationAngle) + deltaVector.Y * Math.Cos(-rotationAngle));

                            if (idx == 0)
                                return new[] 
                                    {
                                        segment.StartVector - deltaVectorA,
                                        segment.StartVector - deltaVectorB,
                                    };
                            else
                                return new[] 
                                    {
                                        segment.EndVector + deltaVectorA,
                                        segment.EndVector + deltaVectorB,
                                    };
                        })
                        .Concat(curve.Segments
                            .SelectMany(segmentPrev => curve.Segments
                                .Skip(1)
                                .Select(segmentNext => new
                                {
                                    SegmentVectorPrev = (segmentPrev.EndVector - segmentPrev.StartVector),
                                    SegmentVectorNext = (segmentNext.EndVector - segmentNext.StartVector),
                                    MidSegmentVector = segmentPrev.EndVector,
                                })
                                .SelectMany(tuple =>
                                {
                                    var midVector = 
                                        (tuple.SegmentVectorPrev / tuple.SegmentVectorPrev.Length) - 
                                        (tuple.SegmentVectorNext / tuple.SegmentVectorNext.Length);
                                    var midDeltaVector = new Vector(
                                        20.0 * midVector.X / midVector.Length,
                                        20.0 * midVector.Y / midVector.Length);

                                    return new[] 
                                        { 
                                            tuple.MidSegmentVector + midDeltaVector,
                                            tuple.MidSegmentVector - midDeltaVector,
                                        };
                                })));
                })
                .Concat(vectorA, vectorB);

            //var alternateVectors = curves
            //    .SelectMany(curve =>
            //        {
            //            var segmentCount = curve.Segments.Count();
            //            return curve.Segments
            //                .SelectMany((segment, idx) =>
            //                    {
            //                        var vectorSegment = segment.EndVector - segment.StartVector;
            //                        var vectorDelta = new Vector(
            //                            20.0 * vectorSegment.X / vectorSegment.Length,
            //                            20.0 * vectorSegment.Y / vectorSegment.Length);

            //                        if (idx == 0 || idx == segmentCount - 1)
            //                        {
            //                            var lVector = new List<Vector>(3);

            //                            var vectorDeltaA = new Vector(
            //                                vectorDelta.X * Math.Cos(rotationAngle) - vectorDelta.Y * Math.Sin(rotationAngle),
            //                                vectorDelta.X * Math.Sin(rotationAngle) + vectorDelta.Y * Math.Cos(rotationAngle));
            //                            var vectorDeltaB = new Vector(
            //                                vectorDelta.X * Math.Cos(-rotationAngle) - vectorDelta.Y * Math.Sin(-rotationAngle),
            //                                vectorDelta.X * Math.Sin(-rotationAngle) + vectorDelta.Y * Math.Cos(-rotationAngle));

            //                            if (idx == 0)
            //                            {
            //                                lVector.Add(segment.StartVector - vectorDeltaA);
            //                                lVector.Add(segment.StartVector - vectorDeltaB);
            //                            }
            //                            else lVector.Add(segment.StartVector - vectorDelta);
                                        
            //                            if (idx == segmentCount - 1)
            //                            {
            //                                lVector.Add(segment.EndVector + vectorDeltaA);
            //                                lVector.Add(segment.EndVector + vectorDeltaB);
            //                            }
            //                            else lVector.Add(segment.EndVector + vectorDelta);

            //                            return (IEnumerable<Vector>)lVector;
            //                        }
            //                        else
            //                        {
            //                            return new[] 
            //                                {
            //                                    segment.StartVector - vectorDelta,
            //                                    segment.EndVector + vectorDelta,
            //                                };
            //                        }
            //                    });
            //        })
            //    .Concat(vectorA, vectorB);
            
            //var alternateVectors = curveSegments
            //    .SelectMany(segment =>
            //        {
            //            var vectorSegment = segment.EndVector - segment.StartVector;
            //            var vectorDelta = new Vector(
            //                20.0 * vectorSegment.X / vectorSegment.Length,
            //                20.0 * vectorSegment.Y / vectorSegment.Length);
            //            return new [] 
            //                {
            //                    segment.StartVector - vectorDelta,
            //                    segment.EndVector + vectorDelta,
            //                };
            //        })
            //    .Concat(vectorA, vectorB)
            //    .ToArray();

            //var alternateVectors = curves
            //    .SelectMany(curve =>
            //        curve.Segments
            //            .Take(1)
            //            .Select(segment => segment.StartVector)
            //        .Concat(curve.Segments.Select(segment => segment.EndVector)))
            //    .SelectMany(vector => new[] 
            //        {
            //            new Vector(vector.X - 20, vector.Y),
            //            new Vector(vector.X, vector.Y - 20),
            //            new Vector(vector.X + 20, vector.Y),
            //            new Vector(vector.X, vector.Y + 20),
            //        })
            //    .Concat(new [] { vectorA, vectorB })
            //    .ToArray();

            var lLineSegments = alternateVectors
                .SelectMany((vectorStart, idxStart) => alternateVectors
                    .Skip(idxStart + 1)
                    .Select(vectorEnd => new LineSegment { StartVector = vectorStart, EndVector = vectorEnd }))
                .Where(lineSegmentAlternate => curveSegments.All(lineSegment => !DetectLineSegmentsIntersection(lineSegmentAlternate, lineSegment)))
                .SelectMany(lineSegmentAlternate => new[] 
                    {
                        lineSegmentAlternate,
                        new LineSegment { StartVector = lineSegmentAlternate.EndVector, EndVector = lineSegmentAlternate.StartVector },
                    })
                .ToList();
            
            var lCurve = new List<Curve>();
            var lCurveFound = new List<Curve>();

            while (lLineSegments.Count > 0)
            {
                var startVectors = lCurve.Count > 0 ?
                    lCurve
                        .Select(curve => curve.Segments.Last().EndVector)
                        .ToArray() :
                    new[] { vectorA };
                var segments = lLineSegments
                    .Where(segment => startVectors.Contains(segment.StartVector))
                    .ToArray();
                if (segments.Length == 0) break;

                segments.ForEach(segment => lLineSegments.Remove(segment));
                segments
                    .Select(segment => lLineSegments
                        .FirstOrDefault(segmentBackward =>
                            segmentBackward.StartVector == segment.EndVector &&
                            segmentBackward.EndVector == segment.StartVector))
                    .Where(segmentBackward => segmentBackward != null)
                    .ForEach(segmentBackward => lLineSegments.Remove(segmentBackward));

                var curvesExtended = lCurve.Count > 0 ?
                    segments
                        .SelectMany(segment => lCurve
                            .Where(curve => curve.Segments.Last().EndVector == segment.StartVector)
                            .Select(curve => new Curve(curve.Segments.Concat(segment))))
                        .ToArray() :
                    segments
                        .Select(segment => new Curve(new[] { segment }))
                        .ToArray();

                lCurveFound.AddRange(curvesExtended.Where(curve => curve.Segments.Last().EndVector == vectorB));

                lCurve.Clear();
                lCurve.AddRange(curvesExtended.Where(curve => curve.Segments.Last().EndVector != vectorB));
            }

            return lCurveFound
                .OrderBy(curve => curve.Length)
                .FirstOrDefault();
        }

        /// <summary>
        /// Detects whether the given two line segments intersect each other.
        /// </summary>
        /// <param name="lineSegmentA">The first line segment to detect intersection.</param>
        /// <param name="lineSegmentB">The second line segment to detect intersection.</param>
        /// <returns><c>true</c> if the given two line segments intersect; otherwise <c>false</c>.</returns>
        /// <remarks>The explanation of detection of line segments intersection can be found at http://stackoverflow.com/questions/563198/how-do-you-detect-where-two-line-segments-intersect. </remarks>
        public bool DetectLineSegmentsIntersection(LineSegment lineSegmentA, LineSegment lineSegmentB)
        {
            // Any point of a line segment can be expressed with the sum of two vectors:
            // - one vector pointing to its start point (P and Q) and
            // - scalar product of other vector pointing from start point to end point (R and S)
            // So, 
            // - LineSegmentA = P + t * R
            // - LineSegmentB = Q + u * S
            // Therefore the 2 line segments intersect if we can find t and u such that: 
            // P + t * R = Q + u * S, where
            // t = (Q - P) x S / (R x S)
            // u = (Q - P) X R / (R X S)

            const double Epsilon = 0.00001;
            var R = lineSegmentA.EndVector - lineSegmentA.StartVector;
            var S = lineSegmentB.EndVector - lineSegmentB.StartVector;

            var RxS = Vector.CrossProduct(R, S);
            var QminusP = lineSegmentB.StartVector - lineSegmentA.StartVector;
            var QminusPxR = Vector.CrossProduct(QminusP, R);

            var isZeroRxS = Math.Abs(RxS) <= Epsilon;
            if (isZeroRxS)
            {
                var isZeroQminusPxR = Math.Abs(QminusPxR) <= Epsilon;
                if (!isZeroQminusPxR) 
                    // Two line segments are parallel and non-intersecting
                    return false;

                // Two line segments are collinear
                var RdividedRmultiplyR = R / (R * R);
                var t0 = QminusP * RdividedRmultiplyR;
                var t1 = t0 + S * RdividedRmultiplyR;

                // Overlaps if t0 and t1 intersects range [0,1]; otherwise disjoint
                var areOverlapping = !(Math.Max(t0, t1) < 0.0 || Math.Min(t0, t1) > 1.0);
                return areOverlapping;
            }

            var QminusPxS = Vector.CrossProduct(QminusP, S);
            var t = QminusPxS / RxS;
            var u = QminusPxR / RxS;

            // |t * R| <= |R| + KeepDistance or |u * S| <= |Q| + KeepDistance than two lines intersect
            const double KeepDistance = 5.0;
            var intersect =
                (t < 0.0 ? ((t * R).Length <= KeepDistance) : ((t * R).Length <= R.Length + KeepDistance)) &&
                (u < 0.0 ? ((u * S).Length <= KeepDistance) : ((u * S).Length <= S.Length + KeepDistance));
            return intersect;

            //if (u > 1.0 || u < 0.0 || t > 1.0 || t < 0.0)
            //    // Two line segments are not parallel, but do not intersect
            //    return false;
            
            //return true;
        }
    }
}
