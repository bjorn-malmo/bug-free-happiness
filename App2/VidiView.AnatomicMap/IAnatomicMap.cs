using System.Drawing;

namespace VidiView.AnatomicMap;

/// <summary>
/// This interface defines the methods and properties for handling anatomic maps in SVG format.
/// </summary>
public interface IAnatomicMap
{
    /// <summary>
    /// Load internal map resource
    /// </summary>
    /// <param name="resourceFilename"></param>
    /// <exception cref="ArgumentException">Thrown if the resource does not exist</exception>
    void LoadResource(string resourceFilename);

    /// <summary>
    /// If set, this will override the fill color on shapes in the Detail layer
    /// Specified in hex format, e.g. "#440000ff" for a semi-transparent blue background.
    /// </summary>
    string? DetailColorHex { get; set; }

    /// <summary>
    /// If set, this will override the fill color on shapes in the Foreground layer
    /// Specified in hex format, e.g. "#440000ff" for a semi-transparent blue background.
    /// </summary>
    string? ForegroundColorHex { get; set; }

    /// <summary>
    /// If set, this will override the fill color on shapes in the Background layer
    /// Specified in hex format, e.g. "#440000ff" for a semi-transparent blue background.
    /// </summary>
    string? BackgroundColorHex { get; set; }

    /// <summary>
    /// Return the viewbox (dimension) of the anatomic map.
    /// This is the size of the internal coordinate system used in the SVG file,
    /// and does not imply a resolution in pixels.
    /// </summary>
    Rectangle ViewBox { get; }

    /// <summary>
    /// Return the SVG as a raw XML document with the colors set according to the properties.
    /// </summary>
    /// <returns></returns>
    Stream GetSvgStream();

    /// <summary>
    /// Return all regions haveing the specific region ID
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    IEnumerable<AnatomicRegion> GetRegionFromId(string id);

    /// <summary>
    /// Checks whether the point (x, y) is within any of the defined regions in the anatomic map.
    /// </summary>
    /// <param name="x">Normalized x-coordinate (0.0 - 1.0)</param>
    /// <param name="y">Normalized y-coordinate (0.0 - 1.0)</param>
    /// <returns></returns>
    AnatomicRegion? GetRegionFromNormalizedPoint(double x, double y);
}
