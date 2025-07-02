using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Reflection;
using System.Xml;

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

    private readonly ILogger _logger;
    private XmlDocument? _svgDocument;
    private XmlNamespaceManager _ns = null!;

    private Dictionary<string, string> _regionPaths = [];

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
        
        ExtractRegions();
    }

    /// <summary>
    /// Detail color in #argb
    /// </summary>
    public string DetailColorHex { get; set; } = "#ffffffff";

    /// <summary>
    /// Foreground color in #argb
    /// </summary>
    public string ForegroundColorHex { get; set; } = "#ffffffff";

    /// <summary>
    /// Background color in #argb
    /// </summary>
    public string BackgroundColorHex { get; set; } = "#44888888";

    /// <summary>
    /// Return the SVG as a raw XML document
    /// </summary>
    /// <returns></returns>
    public Stream GetSvgStream()
    {
        if (_svgDocument == null)
            throw new InvalidOperationException("Resource not loaded");

        // Update colors
        SetChildElementColor(GetLayer(BackgroundLayerName), BackgroundColorHex, null);
        //SetChildElementColor(GetLayer(ForegroundLayerName), ForegroundColorHex, null);
        //SetChildElementColor(GetLayer(DetaildLayerName), DetailColorHex, null);

        var ms = new MemoryStream();
        _svgDocument.Save(ms);
        ms.Position = 0;
        return ms;
    }

    private void ExtractRegions()
    {
        _logger.LogTrace("Extract region codes from layer id='{name}'", RegionLayerName);

        var layer = GetLayer(RegionLayerName);
        if (layer != null)
        {
            _regionPaths = GetRegionCodePath(layer);
            _logger.LogDebug("Found {count} region codes", _regionPaths.Count);

            _logger.LogTrace("Removing layer from SVG");
            layer.RemoveAll();
        }
        else
        {
            _logger.LogDebug("No layer with id='{name}' found", RegionLayerName);
        }
    }

    /// <summary>
    /// Return the raw region paths from the SVG
    /// </summary>
    public IReadOnlyDictionary<string, string> RegionPaths => _regionPaths.AsReadOnly();

    private Dictionary<string, string> GetRegionCodePath(XmlElement layer)
    {
        var result = new Dictionary<string, string>();

        _logger.LogTrace("Locating <path> child elements");
        var nodes = layer.SelectNodes("svg:path", _ns);
        if (nodes != null)
        {
            foreach (var node in nodes)
            {
                if (node is XmlElement e)
                {
                    result.Add(e.GetAttribute("id"), e.GetAttribute("d"));
                }
            }
        }
        return result;
    }

    private XmlElement? GetLayer(string name)
    {
        var xpath = $"//svg:svg/svg:g[@id='{name}']";
        var layer = _svgDocument!.SelectSingleNode(xpath, _ns);
        return layer as XmlElement;
    }

    private void SetChildElementColor(XmlElement? layer, string? fillColor, string? strokeColor)
    {
        if (layer == null)
            return;

        _logger.LogTrace("Set <path> child element fill={fillColor}, strokeColor={strokeColor}", fillColor, strokeColor);

        foreach (var node in layer.SelectNodes("svg:path", _ns))
        {
            if (node is XmlElement e)
            {
                if (fillColor != null)
                    e.SetAttribute("fill", fillColor);
                if (strokeColor != null)
                    e.SetAttribute("stroke", strokeColor);
            }
        }
    }
}
