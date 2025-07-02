namespace VidiView.AnatomicMap.Test;

[TestClass]
public sealed class AnatomicMapSvgTest
{
    [TestMethod]
    public void TestLoadResource()
    {
        var file = new AnatomicMapSvg(null!);
        file.LoadResource("Test.svg");

        Assert.AreEqual(32, file.RegionPaths.Count);
        Assert.AreEqual("M6732 21873l1177 0 0 -1119 -1177 0 0 1119z", file.RegionPaths["_245611006"]);
    }
    
}
