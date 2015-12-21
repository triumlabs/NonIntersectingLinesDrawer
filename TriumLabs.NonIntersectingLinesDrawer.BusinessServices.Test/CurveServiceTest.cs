using System.Linq;
using System.Windows;
using NUnit.Framework;
using TriumLabs.NonIntersectingLinesDrawer.BusinessServices.Model;

namespace TriumLabs.NonIntersectingLinesDrawer.BusinessServices.Test
{
    [TestFixture]
    public class CurveServiceTest
    {
        private ICurveService TestService;

        [SetUp]
        public void SetUp()
        {
            TestService = new CurveService();
        }

        [Test]
        public void Test1SegmentCurveWhenThereIsNoOtherCurves()
        {
            // Arrange
            var vectorA = new Vector(20,50);
            var vectorB = new Vector(50, 100);

            // Act
            var curveActual = TestService.FindNonIntersectingCurve(vectorA, vectorB, new Curve[0]);

            // Assert
            Assert.IsNotNull(curveActual, "Curve is null");
            Assert.IsNotNull(curveActual.Segments, "Curve.Segments is null");
            Assert.AreEqual(1, curveActual.Segments.Count(), "Curve.Segments.Count is incorrect");
            Assert.AreEqual(vectorA, curveActual.Segments.ElementAt(0).StartVector, "Curve.Segments[0].StartVector is not correct");
            Assert.AreEqual(vectorB, curveActual.Segments.ElementAt(0).EndVector, "Curve.Segments[0].EndVector is not correct");
        }

        [Test]
        public void Test1SegmentCurveWhenThereAreOtherCurves()
        {
            // Arrange
            var vectorA = new Vector(20, 50);
            var vectorB = new Vector(50, 100);
            var curves = new[]
                {
                    new Curve(new[] { new LineSegment { StartVector = new Vector(20, 20), EndVector = new Vector(50, 20), }}),
                    new Curve(new[] { new LineSegment { StartVector = new Vector(20, 150), EndVector = new Vector(150, 120), }}),
                };

            // Act
            var curveActual = TestService.FindNonIntersectingCurve(vectorA, vectorB, curves);

            // Assert
            Assert.IsNotNull(curveActual, "Curve is null");
            Assert.IsNotNull(curveActual.Segments, "Curve.Segments is null");
            Assert.AreEqual(1, curveActual.Segments.Count(), "Curve.Segments.Count is incorrect");
            Assert.AreEqual(vectorA, curveActual.Segments.ElementAt(0).StartVector, "Curve.Segments[0].StartVector is not correct");
            Assert.AreEqual(vectorB, curveActual.Segments.ElementAt(0).EndVector, "Curve.Segments[0].EndVector is not correct");
        }

        [Test]
        public void TestTop3SegmentsCurveWhenThereIsOneOtherCurve()
        {
            // Arrange
            var vectorA = new Vector(25, 60);
            var vectorB = new Vector(75, 75);
            var curves = new[]
                {
                    new Curve(new[] { new LineSegment { StartVector = new Vector(50, 50), EndVector = new Vector(50, 100), }}),
                };

            // Act
            var curveActual = TestService.FindNonIntersectingCurve(vectorA, vectorB, curves);

            // Assert
            Assert.IsNotNull(curveActual, "Curve is null");
            Assert.IsNotNull(curveActual.Segments, "Curve.Segments is null");
            Assert.AreEqual(3, curveActual.Segments.Count(), "Curve.Segments.Count is incorrect");
            Assert.AreEqual(vectorA, curveActual.Segments.ElementAt(0).StartVector, "Curve.Segments[0].StartVector is not correct");
            Assert.AreEqual(vectorB, curveActual.Segments.ElementAt(2).EndVector, "Curve.Segments[2].EndVector is not correct");
        }

        [Test]
        public void TestBottom3SegmentsCurveWhenThereIsOneOtherCurve()
        {
            // Arrange
            var vectorA = new Vector(25, 80);
            var vectorB = new Vector(75, 75);
            var curves = new[]
                {
                    new Curve(new[] { new LineSegment { StartVector = new Vector(50, 50), EndVector = new Vector(50, 100), }}),
                };

            // Act
            var curveActual = TestService.FindNonIntersectingCurve(vectorA, vectorB, curves);

            // Assert
            Assert.IsNotNull(curveActual, "Curve is null");
            Assert.IsNotNull(curveActual.Segments, "Curve.Segments is null");
            Assert.AreEqual(3, curveActual.Segments.Count(), "Curve.Segments.Count is incorrect");
            Assert.AreEqual(vectorA, curveActual.Segments.ElementAt(0).StartVector, "Curve.Segments[0].StartVector is not correct");
            Assert.AreEqual(vectorB, curveActual.Segments.ElementAt(2).EndVector, "Curve.Segments[2].EndVector is not correct");
        }

        [Test]
        public void TestTop3SegmentsCurveWhenThereAreTwoOtherCurves()
        {
            // Arrange
            var vectorA = new Vector(25, 60);
            var vectorB = new Vector(125, 75);
            var curves = new[]
                {
                    new Curve(new[] { new LineSegment { StartVector = new Vector(50, 50), EndVector = new Vector(50, 100), }}),
                    new Curve(new[] { new LineSegment { StartVector = new Vector(100, 50), EndVector = new Vector(100, 100), }}),
                };

            // Act
            var curveActual = TestService.FindNonIntersectingCurve(vectorA, vectorB, curves);

            // Assert
            Assert.IsNotNull(curveActual, "Curve is null");
            Assert.IsNotNull(curveActual.Segments, "Curve.Segments is null");
            Assert.AreEqual(3, curveActual.Segments.Count(), "Curve.Segments.Count is incorrect");
            Assert.AreEqual(vectorA, curveActual.Segments.ElementAt(0).StartVector, "Curve.Segments[0].StartVector is not correct");
            Assert.AreEqual(vectorB, curveActual.Segments.ElementAt(2).EndVector, "Curve.Segments[1].EndVector is not correct");
        }
    }
}
