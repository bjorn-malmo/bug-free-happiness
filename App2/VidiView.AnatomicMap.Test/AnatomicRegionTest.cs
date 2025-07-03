using Microsoft.VisualStudio.TestTools.UnitTesting;
using VidiView.AnatomicMap;

namespace VidiView.AnatomicMap.Test;

[TestClass]
public sealed class AnatomicRegionTest
{


    [TestMethod]
    public void IsPointInRegion_ReturnsTrue_ForPointInsidePolygon()
    {
        // Square: (0,0), (0,10), (10,10), (10,0)
        var path = "M0 0L0 10L10 10L10 0Z";
        var region = new AnatomicRegion(path, null!);

        Assert.IsTrue(region.IsPointInRegion(5, 5));
    }

    [TestMethod]
    public void IsPointInRegion_ReturnsFalse_ForPointOutsidePolygon()
    {
        var path = "M0 0L0 10L10 10L10 0Z";
        var region = new AnatomicRegion(path, null!);

        Assert.IsFalse(region.IsPointInRegion(15, 5));
    }

    [TestMethod]
    public void IsPointInRegion_ReturnsTrue_ForPointOnEdge()
    {
        var path = "M0 0L0 10L10 10L10 0Z";
        var region = new AnatomicRegion(path, null!);

        Assert.IsTrue(region.IsPointInRegion(0, 5));
    }

    [TestMethod]
    public void ExtractPointsFromPath_ParsesAbsoluteAndRelativeCommands()
    {
        var path = "M1 1l2 0l0 2l-2 0z";
        var points = AnatomicRegion.ExtractPointsFromPath(path);

        Assert.AreEqual(4, points.Count);
        Assert.AreEqual((1, 1), points[0]);
        Assert.AreEqual((3, 1), points[1]);
        Assert.AreEqual((3, 3), points[2]);
        Assert.AreEqual((1, 3), points[3]);
    }

    [TestMethod]
    [ExpectedException(typeof(NotSupportedException))]
    public void ExtractPointsFromPath_CurvesThrows()
    {
        var path = "M293 2305c-2,1 -4,2 -6,3 -10,5 -20,10 -36,4l3 -6c13,6 21,1 30,-3 2,-1 4,-2 6,-3l3 6z";
        var points = AnatomicRegion.ExtractPointsFromPath(path);
    }

    [TestMethod]
    public void IsPointInRegion_ComplexPolygon()
    {
        // Star-shaped polygon (self-intersecting, 5 points)
        // Coordinates: (50,0), (61,35), (98,35), (68,57), (79,91), (50,70), (21,91), (32,57), (2,35), (39,35)
        var path = "M50 0L61 35L98 35L68 57L79 91L50 70L21 91L32 57L2 35L39 35Z";
        var region = new AnatomicRegion(path, null!);

        // Point clearly inside the star
        Assert.IsTrue(region.IsPointInRegion(50, 50));

        // Point outside the star
        Assert.IsFalse(region.IsPointInRegion(50, 95));

        // Point in the a concave (bottom "dent") of the star (should be outside)
        Assert.IsFalse(region.IsPointInRegion(50, 71));

        // Point in in the star's "arms" (should be inside)
        Assert.IsFalse(region.IsPointInRegion(72, 90));
    }

    [TestMethod]
    public void BoundingRect_ComplexPolygon()
    {
        // Star-shaped polygon (self-intersecting, 5 points)
        // Coordinates: (50,0), (61,35), (98,35), (68,57), (79,91), (50,70), (21,91), (32,57), (2,35), (39,35)
        var path = "M50 0L61 35L98 35L68 57L79 91L50 70L21 91L32 57L2 35L39 35Z";
        var region = new AnatomicRegion(path, null!);

        // The bounding rectangle should cover the entire star shape
        var boundingRect = region.BoundingRectangle;
        Assert.AreEqual(2, boundingRect.X);
        Assert.AreEqual(0, boundingRect.Y);
        Assert.AreEqual(96, boundingRect.Width);
        Assert.AreEqual(91, boundingRect.Height);
    }

    [TestMethod]
    public void ExtractPointsFromPath_RelativeLines()
    {
        // Square: (0,0), (0,10), (10,10), (10,0)
        var path = "M0 0l0 10l10 0l0 -10z";
        var points = AnatomicRegion.ExtractPointsFromPath(path);

        CollectionAssert.AreEqual(
            new List<(double X, double Y)>
            {
                (0, 0),
                (0, 10),
                (10, 10),
                (10, 0)
            }, 
            points);
    }

    [TestMethod]
    public void ExtractPointsFromPath_AbsoluteLines()
    {
        // Square: (0,0), (0,10), (10,10), (10,0)
        var path = "M0 0L0 10L10 10L10 0Z";
        var points = AnatomicRegion.ExtractPointsFromPath(path);

        CollectionAssert.AreEqual(
            new List<(double X, double Y)>
            {
                (0, 0),
                (0, 10),
                (10, 10),
                (10, 0)
            },
            points);
    }

    [TestMethod]
    public void ExtractPointsFromPath_ParsesHorizontalAndVerticalCommands()
    {
        // Absolute H and V
        var path = "M0 0H10V10H0V0Z";
        var points = AnatomicRegion.ExtractPointsFromPath(path);

        CollectionAssert.AreEqual(
            new List<(double X, double Y)>
            {
                (0, 0),   // M0 0
                (10, 0),  // H10
                (10, 10), // V10
                (0, 10),  // H0
                (0, 0)    // V0
            },
            points
        );

        // Relative h and v
        path = "M1 1h4v3h-2v-1z";
        points = AnatomicRegion.ExtractPointsFromPath(path);

        CollectionAssert.AreEqual(
            new List<(double X, double Y)>
            {
                (1, 1),   // M1 1
                (5, 1),   // h4
                (5, 4),   // v3
                (3, 4),   // h-2
                (3, 3)    // v-1
            },
            points
        );
    }

}