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
            {
                // Curve can be represented as a straight line
                return new Curve(new[] { lineSegmentAB });
            }

            // Curve cannot be represented as straight line as it would intersect at least one other curve
            // Curve need to be splitted into more, connected line segments

            var rotationAngle = Math.PI / 3.0; // Angle of 60 degrees

            // Defines points 20px far from exisiting curves' edges as
            // - 2 points rotated +/- 60 degree from the line for each start and end point of a curve
            // - 2 points on the mid line for each inner points of a curve
            var alternateVectors = curves
                .SelectMany(curve => curve.Segments.Take(1).Concat(curve.Segments.Last())
                    .SelectMany((segment, idx) =>
                    {
                        var segmentVector = segment.EndVector - segment.StartVector;
                        var deltaVector = segmentVector.NormalizeTo(20.0);
                        var deltaVectorA = deltaVector.RotateBy(rotationAngle);
                        var deltaVectorB = deltaVector.RotateBy(-rotationAngle);

                        // Gets the 20px, rotated points for first point of the curve
                        if (idx == 0)
                            return new[] 
                                {
                                    segment.StartVector - deltaVectorA,
                                    segment.StartVector - deltaVectorB,
                                };
                        // Gets the 20px, rotated points for last point of the curve
                        else
                            return new[] 
                                {
                                    segment.EndVector + deltaVectorA,
                                    segment.EndVector + deltaVectorB,
                                };
                    })
                    // Gets the 2 points 20px far on mid lane for inner points of the curve
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
                                var midVector = tuple.SegmentVectorPrev.NormalizeTo() - tuple.SegmentVectorNext.NormalizeTo();
                                var midDeltaVector = midVector.NormalizeTo(20);

                                return new[] 
                                    { 
                                        tuple.MidSegmentVector + midDeltaVector,
                                        tuple.MidSegmentVector - midDeltaVector,
                                    };
                            }))))
                .Concat(vectorA, vectorB);

            // Creates a bi-directed graph by connecting all alternate points which do not intersect an existing curve
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

            // Finds curves (path) between the given points A and B (breadth-first search)
            // TODO: replace this to Dijkstra
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

                // Removes segments and their backward directions
                segments.ForEach(segment => lLineSegments.Remove(segment));
                segments
                    .Select(segment => lLineSegments
                        .FirstOrDefault(segmentBackward =>
                            segmentBackward.StartVector == segment.EndVector &&
                            segmentBackward.EndVector == segment.StartVector))
                    .Where(segmentBackward => segmentBackward != null)
                    .ForEach(segmentBackward => lLineSegments.Remove(segmentBackward));

                // Extends appropriate curves with the last segment found (tracking path)
                var curvesExtended = lCurve.Count > 0 ?
                    segments
                        .SelectMany(segment => lCurve
                            .Where(curve => curve.Segments.Last().EndVector == segment.StartVector)
                            .Select(curve => new Curve(curve.Segments.Concat(segment))))
                        .ToArray() :
                    segments
                        .Select(segment => new Curve(new[] { segment }))
                        .ToArray();

                // Stores curves reached end point
                lCurveFound.AddRange(curvesExtended.Where(curve => curve.Segments.Last().EndVector == vectorB));

                // Defines start points for the next search iteration
                lCurve.Clear();
                lCurve.AddRange(curvesExtended.Where(curve => curve.Segments.Last().EndVector != vectorB));
            }

            // Returns the shortest curve/path between points A and B
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
