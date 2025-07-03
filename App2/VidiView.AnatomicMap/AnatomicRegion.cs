using System.Drawing;
using System.Globalization;
using System.Text.RegularExpressions;

namespace VidiView.AnatomicMap;

/// <summary>
/// This class represents an anatomic region defined by a path in SVG format.
/// Only line and move commands are supported (no curves)
/// </summary>
public class AnatomicRegion
{
    private readonly IReadOnlyList<(double X, double Y)> _points;

    public AnatomicRegion(string path, string id)
    {
        _points = ExtractPointsFromPath(path);
        BoundingRectangle = GetBoundingRectangle(_points);
        Id = id;
        PathData = path;
    }

    /// <summary>
    /// The region ID
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Returns the bounding rectangle
    /// </summary>
    public Rectangle BoundingRectangle { get; }

    /// <summary>
    /// The original path string used to create this region.
    /// </summary>
    public string PathData { get; }

    public override string ToString()
    {
        return Id;
    }

    /// <summary>
    /// Returns true if the point is contained within this region
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public bool IsPointInRegion(double x, double y)
    {
        // First check if the point is within the bounding rectangle
        // for a quick comparison
        if (x < BoundingRectangle.Left || x > BoundingRectangle.Right 
            || y < BoundingRectangle.Top || y > BoundingRectangle.Bottom)
        {
            return false;
        }

        return IsPointInPolygon(_points, x, y);
    }

    internal static bool IsPointInPolygon(IReadOnlyList<(double X, double Y)> polygon, double x, double y)
    {
        int n = polygon.Count;
        bool inside = false;

        for (int i = 0, j = n - 1; i < n; j = i++)
        {
            var pi = polygon[i];
            var pj = polygon[j];

            if (((pi.Y > y) != (pj.Y > y)) &&
                (x < (pj.X - pi.X) * (y - pi.Y) / (pj.Y - pi.Y + double.Epsilon) + pi.X))
            {
                inside = !inside;
            }
        }

        return inside;
    }

    internal static List<(double X, double Y)> ExtractPointsFromPath(string path)
    {
        var points = new List<(double X, double Y)>();
        var regex = new Regex(@"([MmLlHhVvCcSsQqTtAaZz])|(-?\d*\.?\d+(?:[eE][+-]?\d+)?)");
        var matches = regex.Matches(path);

        double currentX = 0, currentY = 0;
        int i = 0;
        char lastCommand = ' ';

        while (i < matches.Count)
        {
            var match = matches[i];
            if (match.Groups[1].Success)
            {
                lastCommand = match.Value[0];
                i++;
                if (lastCommand == 'z' || lastCommand == 'Z')
                {
                    break;
                }
            }
            else
            {
                if (lastCommand == 'M' || lastCommand == 'L')
                {
                    // Absolute coordinates
                    double x = double.Parse(matches[i++].Value, CultureInfo.InvariantCulture);
                    double y = double.Parse(matches[i++].Value, CultureInfo.InvariantCulture);
                    currentX = x;
                    currentY = y;
                    points.Add((currentX, currentY));
                }
                else if (lastCommand == 'm' || lastCommand == 'l')
                {
                    // Relative coordinates
                    double dx = double.Parse(matches[i++].Value, CultureInfo.InvariantCulture);
                    double dy = double.Parse(matches[i++].Value, CultureInfo.InvariantCulture);
                    currentX += dx;
                    currentY += dy;
                    points.Add((currentX, currentY));
                }
                else if (lastCommand == 'H')
                {
                    // Absolute horizontal line
                    double x = double.Parse(matches[i++].Value, CultureInfo.InvariantCulture);
                    currentX = x;
                    points.Add((currentX, currentY));
                }
                else if (lastCommand == 'h')
                {
                    // Relative horizontal line
                    double dx = double.Parse(matches[i++].Value, CultureInfo.InvariantCulture);
                    currentX += dx;
                    points.Add((currentX, currentY));
                }
                else if (lastCommand == 'V')
                {
                    // Absolute vertical line
                    double y = double.Parse(matches[i++].Value, CultureInfo.InvariantCulture);
                    currentY = y;
                    points.Add((currentX, currentY));
                }
                else if (lastCommand == 'v')
                {
                    // Relative vertical line
                    double dy = double.Parse(matches[i++].Value, CultureInfo.InvariantCulture);
                    currentY += dy;
                    points.Add((currentX, currentY));
                }
                else
                {
                    i++;

                    throw new NotSupportedException($"The path command '{lastCommand}' is not supported for hit testing.");
                }
            }
        }

        return points;
    }

    private static Rectangle GetBoundingRectangle(IReadOnlyList<(double X, double Y)> points)
    {
        if (points.Count < 3)
            throw new InvalidOperationException("No points defined in the region.");

        double minX = double.PositiveInfinity;
        double minY = double.PositiveInfinity;
        double maxX = double.NegativeInfinity;
        double maxY = double.NegativeInfinity;

        foreach (var (x, y) in points)
        {
            if (x < minX) minX = x;
            if (y < minY) minY = y;
            if (x > maxX) maxX = x;
            if (y > maxY) maxY = y;
        }
        return Rectangle.FromLTRB(
            (int)Math.Floor(minX),
            (int)Math.Floor(minY),
            (int)Math.Ceiling(maxX),
            (int)Math.Ceiling(maxY)
        );

    }
}
