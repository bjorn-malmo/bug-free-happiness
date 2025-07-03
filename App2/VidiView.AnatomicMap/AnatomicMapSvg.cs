using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Drawing;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

namespace VidiView.AnatomicMap;

/// <summary>
/// This class handles the SVG files that make up our anatomic maps.
/// It has support for extracting coded regions etc from the file
/// </summary>
public class AnatomicMapSvg : IAnatomicMap
{
    private const string BackgroundLayerName = "Background";
    private const string ForegroundLayerName = "Foreground";
    private const string DetaildLayerName = "Detail";
    private const string RegionLayerName = "SCT";

    private static readonly Regex _sctCode = new Regex(@"^_(?<code>[0-9]+)(_.+)?$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private readonly ILogger _logger;
    
    private XmlDocument? _svgDocument;
    private XmlNamespaceManager _ns = null!;
    private IReadOnlyList<AnatomicRegion>? _regions = null;
    private AnatomicRegion? _lastCheckedRegion = null;

    public AnatomicMapSvg(ILogger<AnatomicMapSvg> logger)
    {
        _logger = logger ?? NullLogger<AnatomicMapSvg>.Instance;
    }

    public void LoadResource(string resourceFilename)
    {
        string fullResourceName = $"{typeof(AnatomicMapSvg).Namespace}.Resources.{resourceFilename}";
        _logger.LogDebug("Load SVG resource {name}", fullResourceName);
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(fullResourceName)
            ?? throw new ArgumentException($"The specified resource does not exist: {fullResourceName}");

        _logger.LogDebug("Parse raw SVG as XML");
        var doc = new XmlDocument();
        doc.Load(stream);

        _svgDocument = doc;
        _ns = new XmlNamespaceManager(_svgDocument.NameTable);
        _ns.AddNamespace("svg", "http://www.w3.org/2000/svg");
        
        ViewBox = ParseViewBox();
        _regions = ExtractRegionsAndRemoveLayer();

        RemoveLayersNamedIgnore();
    }

    public string? DetailColorHex { get; set; } = "#ffffffff";

    public string? ForegroundColorHex { get; set; } = "#ffffffff";

    public string? BackgroundColorHex { get; set; } = "#44888888";

    public Rectangle ViewBox { get; private set; }

    /// <summary>
    /// Return the raw region paths from the SVG
    /// </summary>
    public IReadOnlyList<AnatomicRegion>? RegionPaths => _regions;

    public Stream GetSvgStream()
    {
        if (_svgDocument == null)
            throw new InvalidOperationException("Resource not loaded");

        // Update colors
        // Note! All anatomic maps use filled shapes (no outlines).
        // Therefore all colors are set as Fill color
        SetLayerColor(BackgroundLayerName, BackgroundColorHex, null);
        SetLayerColor(ForegroundLayerName, ForegroundColorHex, null);
        SetLayerColor(DetaildLayerName, DetailColorHex, null);

        var ms = new MemoryStream();
        _svgDocument.Save(ms);
        ms.Position = 0;
        return ms;
    }

    public IEnumerable<AnatomicRegion> GetRegionFromId(string id)
    {
        if (_regions == null)
            throw new InvalidOperationException("Resource not loaded");

        _logger.LogTrace("Get regions with id='{id}'", id);
        return _regions.Where(r => string.Equals(r.Id, id, StringComparison.OrdinalIgnoreCase));
    }

    public AnatomicRegion? GetRegionFromNormalizedPoint(double x, double y)
    {
        if (x < 0.0 || x > 1.0)
            throw new ArgumentOutOfRangeException(nameof(x), "X coordinate must be between 0.0 and 1.0");
        if (y < 0.0 || y > 1.0)
            throw new ArgumentOutOfRangeException(nameof(y), "Y coordinate must be between 0.0 and 1.0");
        if (_regions == null)
            throw new InvalidOperationException("Resource not loaded");

        // Convert to absolute coordinates based on the viewBox
        x = x * ViewBox.Width + ViewBox.X;
        y = y * ViewBox.Height + ViewBox.Y;

        _logger.LogTrace("Get region from point ({x}, {y})", x, y);

        // Since we normally check for adjacent points (using i.e. mouse),
        // check if this matches the last region first
        if (_lastCheckedRegion?.IsPointInRegion(x, y) == true)
            return _lastCheckedRegion;

        var result = _regions.FirstOrDefault((r) => r.IsPointInRegion(x, y));
        _logger.LogDebug("Point ({x}, {y}) is in region: {region}", x, y, result?.Id ?? "<None>");

        _lastCheckedRegion = result;
        return result;
    }

    /// <summary>
    /// Set color to child elements
    /// </summary>
    /// <param name="layerId"></param>
    /// <param name="fill"></param>
    /// <param name="stroke"></param>
    private void SetLayerColor(string layerId, string? fill, string? stroke)
    {
        var layer = GetLayerElement(layerId);
        if (layer == null)
            return;

        _logger.LogTrace("Set layer elements fill={fill}, stroke={stroke}", fill, stroke);

        SetColor(layer.SelectNodes("svg:path", _ns), fill, stroke);
    }

    private IReadOnlyList<AnatomicRegion> ExtractRegionsAndRemoveLayer()
    {
        _logger.LogTrace("Extract region codes from layer id='{name}'", RegionLayerName);

        IReadOnlyList<AnatomicRegion> result;
        var layer = GetLayerElement(RegionLayerName);
        if (layer != null)
        {
            var paths = GetRegionIdAndPaths(layer);
            _logger.LogDebug("Found {count} region codes", paths.Count);

            result = ParseAnatomicRegions(paths);

            _logger.LogTrace("Removing layer from SVG");
            layer.RemoveAll();
        }
        else
        {
            _logger.LogDebug("No layer with id='{name}' found", RegionLayerName);
            result = Array.Empty<AnatomicRegion>();
        }

        return result;
    }

    private IReadOnlyList<AnatomicRegion> ParseAnatomicRegions(List<(string Id, string Path)> paths)
    {
        var regions = new List<AnatomicRegion>(paths.Count);
        foreach (var r in paths)
        {
            regions.Add(new AnatomicRegion(r.Path, ParseCode(r.Id)));
        }
        return regions.AsReadOnly();
    }

    private List<(string Id, string Path)> GetRegionIdAndPaths(XmlElement layer)
    {
        var result = new List<(string Id, string Path)>();

        _logger.LogTrace("Locating <path> child elements");
        var nodes = layer.SelectNodes("svg:path", _ns);
        if (nodes != null)
        {
            foreach (var node in nodes)
            {
                if (node is XmlElement e)
                {
                    result.Add((e.GetAttribute("id"), e.GetAttribute("d")));
                }
            }
        }
        return result;
    }

    private void RemoveLayersNamedIgnore()
    {
        var xpath = $"//svg:svg/svg:g";
        var layers = _svgDocument!.SelectNodes(xpath, _ns);
        if (layers != null)
        {
            foreach (var node in layers)
            {
                if (node is XmlElement e
                    && e.GetAttribute("id").Contains("Ignore", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogTrace("Removing layer with id='{id}'", e.GetAttribute("id"));
                    e.RemoveAll();
                }
            }
        }
    }

    private XmlElement? GetLayerElement(string name)
    {
        var xpath = $"//svg:svg/svg:g[@id='{name}']";
        var layer = _svgDocument!.SelectSingleNode(xpath, _ns);
        return layer as XmlElement;
    }

    private void SetColor(XmlNodeList? nodes, string? fill, string? stroke)
    {
        fill = ExtractAlphaComponent(fill, out var fillOpacity);
        stroke = ExtractAlphaComponent(stroke, out var strokeOpacity);

        if (nodes != null)
        {
            foreach (var node in nodes)
            {
                if (node is XmlElement e)
                {
                    if (fill != null)
                    {
                        e.SetAttribute("fill", fill);
                        e.SetAttribute("fill-opacity", fillOpacity.ToString("0.00", CultureInfo.InvariantCulture));
                    }
                    if (stroke != null)
                    {
                        e.SetAttribute("stroke", stroke);
                        e.SetAttribute("stroke-opacity", strokeOpacity.ToString("0.00", CultureInfo.InvariantCulture));
                    }
                }
            }
        }
    }

    internal string? ExtractAlphaComponent(string? hexColor, out double opacity)
    {
        opacity = 1;
        if (hexColor != null && hexColor.Length == 9 && hexColor[0] == '#')
        {
            byte alpha = HexToByte(hexColor.Substring(1, 2));
            opacity = alpha / 255.0;
            return string.Concat("#", hexColor.AsSpan(3)); // Return the rest of the color without alpha
        }
        return hexColor;
    }

    private static byte HexToByte(string hex)
    {
        if (hex == null) throw new ArgumentNullException(nameof(hex));
        if (hex.Length != 2) throw new ArgumentException("Hex string must be exactly 2 characters.", nameof(hex));
        return Convert.ToByte(hex, 16);
    }

    /// <summary>
    /// Parse the viewBox attribute found in the root element
    /// </summary>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    /// <exception cref="FormatException"></exception>
    private Rectangle ParseViewBox()
    {
        var svg = _svgDocument!.SelectSingleNode("//svg:svg", _ns) as XmlElement 
            ?? throw new ArgumentException("SVG root element not found");
        
        var viewBoxAttr = svg.GetAttribute("viewBox");
        if (string.IsNullOrWhiteSpace(viewBoxAttr))
            throw new InvalidOperationException("SVG does not contain a viewBox attribute.");

        // viewBox format: "min-x min-y width height"
        var parts = viewBoxAttr.Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 4)
            throw new FormatException($"Invalid viewBox format: '{viewBoxAttr}'");

        if (!float.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var minX) ||
            !float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var minY) ||
            !float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out var width) ||
            !float.TryParse(parts[3], NumberStyles.Float, CultureInfo.InvariantCulture, out var height))
        {
            throw new FormatException($"Invalid viewBox values: '{viewBoxAttr}'");
        }

        // Rectangle only supports int, so round or truncate as appropriate
        return new Rectangle(
            (int)Math.Floor(minX),
            (int)Math.Floor(minY),
            (int)Math.Ceiling(width),
            (int)Math.Ceiling(height)
        );
    }

    /// <summary>
    /// Remove leading underscore and return the region code.
    /// CorelDRAW will add underscore and a number to enforce uniqueness. Remove this as well
    /// </summary>
    /// <param name="objectId"></param>
    /// <returns></returns>
    internal static string ParseCode(string objectId)
    {
        if (objectId != null)
        {
            var match = _sctCode.Match(objectId);
            if (match.Success)
            {
                objectId = match.Groups["code"].Value;
            }
        }
        return objectId!;
    }
}
