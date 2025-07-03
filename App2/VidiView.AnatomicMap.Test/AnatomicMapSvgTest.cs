namespace VidiView.AnatomicMap.Test;

[TestClass]
public sealed class AnatomicMapSvgTest
{
    [TestMethod]
    public void TestLoadResourceAndVerifyRegion()
    {
        var file = new AnatomicMapSvg(null!);
        file.LoadResource("Test.svg");

        Assert.AreEqual(32, file.RegionPaths.Count);
//        Assert.AreEqual("M6732 21873l1177 0 0 -1119 -1177 0 0 1119z", file.RegionPaths["_245611006"]);
    }

    [TestMethod]
    public void TestGetRegionIdFromPoint()
    {
        var file = new AnatomicMapSvg(null!);
        file.LoadResource("Test.svg");

        Assert.AreEqual("245575001", file.GetRegionFromNormalizedPoint(0.48, 0.05)?.Id);
    }

    [TestMethod]
    public void TestGetRegionFromId()
    {
        var file = new AnatomicMapSvg(null!);
        file.LoadResource("Test.svg");

        var region = file.GetRegionFromId("245575001");

        Assert.AreEqual(1, region.Count());
        Assert.AreEqual("245575001", region.First().Id);
    }

    [TestMethod]
    public void TestExtractAlphaComponent_WithAlpha()
    {
        var svg = new AnatomicMapSvg(null!);
        var result = svg.ExtractAlphaComponent("#80FF0000", out var opacity);

        Assert.AreEqual("#FF0000", result);
        Assert.AreEqual(128.0 / 255.0, opacity, 1e-6);
    }

    [TestMethod]
    public void TestExtractAlphaComponent_WithoutAlpha()
    {
        var svg = new AnatomicMapSvg(null!);
        var result = svg.ExtractAlphaComponent("#FF0000", out var opacity);

        Assert.AreEqual("#FF0000", result);
        Assert.AreEqual(1.0, opacity, 1e-6);
    }

    [TestMethod]
    [DataRow("_12131415", "12131415")]
    [DataRow("_12131415_1", "12131415")]
    [DataRow("_12131415_5011", "12131415")]
    [DataRow("12131415", "12131415")]
    [DataRow("AXC131415", "AXC131415")]
    public void TestRegionCodeParser(string id, string expected)
    {
        Assert.AreEqual(expected, AnatomicMapSvg.ParseCode(id));
    }
}
