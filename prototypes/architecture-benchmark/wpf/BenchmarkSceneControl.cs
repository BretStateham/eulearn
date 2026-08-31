using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Eulearn.ThrowawayArchitectureBenchmark;

public sealed class BenchmarkSceneControl : FrameworkElement
{
    public const int VectorObjectCount = 10_000;
    public const int InkPointCount = 100_000;

    private const double WorldWidth = 4_000;
    private const double WorldHeight = 3_000;

    private static readonly Pen VectorPen = CreatePen(Color.FromRgb(75, 92, 108), 0.65);
    private static readonly Pen InkPen = CreatePen(Color.FromArgb(175, 26, 70, 116), 1.35);
    private static readonly Pen SelectionPen = CreatePen(Color.FromRgb(255, 183, 3), 5);
    private static readonly Brush[] ObjectBrushes =
    [
        CreateBrush(Color.FromRgb(222, 235, 247)),
        CreateBrush(Color.FromRgb(226, 240, 217)),
        CreateBrush(Color.FromRgb(252, 228, 214)),
        CreateBrush(Color.FromRgb(232, 222, 248))
    ];

    private readonly SceneObject[] _objects;
    private readonly StreamGeometry[] _inkPaths;
    private readonly Dictionary<string, Rect> _semanticObjects = new(StringComparer.Ordinal)
    {
        ["Math"] = new Rect(570, 470, 260, 150),
        ["Graph"] = new Rect(1_870, 1_330, 330, 230),
        ["Locked group"] = new Rect(3_050, 2_210, 360, 210)
    };

    private double _zoom = 1;
    private Vector _pan;
    private string _selectedObject = "Math";

    public BenchmarkSceneControl()
    {
        Focusable = true;
        ClipToBounds = true;
        _objects = CreateObjects();
        _inkPaths = CreateInkPaths();
    }

    public double Zoom => _zoom;

    public string SelectedObject => _selectedObject;

    public void SetZoom(double zoom)
    {
        _zoom = Math.Clamp(zoom, 0.01, 64);
        CenterSelection();
        InvalidateVisual();
    }

    public void ZoomAt(Point viewportPoint, double factor)
    {
        var oldZoom = _zoom;
        var newZoom = Math.Clamp(oldZoom * factor, 0.01, 64);
        if (Math.Abs(newZoom - oldZoom) < double.Epsilon)
        {
            return;
        }

        var worldPoint = ViewportToWorld(viewportPoint);
        _zoom = newZoom;
        _pan = new Vector(
            viewportPoint.X - worldPoint.X * newZoom,
            viewportPoint.Y - worldPoint.Y * newZoom);
        InvalidateVisual();
    }

    public void Pan(Vector delta)
    {
        _pan += delta;
        InvalidateVisual();
    }

    public void SelectSemanticObject(string name)
    {
        if (!_semanticObjects.ContainsKey(name))
        {
            throw new ArgumentOutOfRangeException(nameof(name));
        }

        _selectedObject = name;
        CenterSelection();
        InvalidateVisual();
    }

    protected override void OnRender(DrawingContext drawingContext)
    {
        base.OnRender(drawingContext);
        drawingContext.DrawRectangle(Brushes.White, null, new Rect(RenderSize));

        drawingContext.PushTransform(new MatrixTransform(
            _zoom,
            0,
            0,
            _zoom,
            _pan.X,
            _pan.Y));

        drawingContext.DrawRectangle(
            CreateBrush(Color.FromRgb(250, 252, 254)),
            CreatePen(Color.FromRgb(125, 135, 145), 2),
            new Rect(0, 0, WorldWidth, WorldHeight));

        foreach (var sceneObject in _objects)
        {
            drawingContext.DrawRoundedRectangle(
                ObjectBrushes[sceneObject.BrushIndex],
                VectorPen,
                sceneObject.Bounds,
                3,
                3);
        }

        foreach (var inkPath in _inkPaths)
        {
            drawingContext.DrawGeometry(null, InkPen, inkPath);
        }

        DrawSemanticObjects(drawingContext);
        drawingContext.Pop();

        var overlay = new FormattedText(
            $"{_zoom * 100:0.##}%  |  pan {_pan.X:0}, {_pan.Y:0}",
            System.Globalization.CultureInfo.InvariantCulture,
            FlowDirection.LeftToRight,
            new Typeface("Segoe UI"),
            13,
            Brushes.White,
            VisualTreeHelper.GetDpi(this).PixelsPerDip);
        drawingContext.DrawRoundedRectangle(
            CreateBrush(Color.FromArgb(220, 30, 41, 54)),
            null,
            new Rect(10, 10, overlay.Width + 20, overlay.Height + 12),
            4,
            4);
        drawingContext.DrawText(overlay, new Point(20, 16));
    }

    private void CenterSelection()
    {
        var center = new Point(
            _semanticObjects[_selectedObject].X + _semanticObjects[_selectedObject].Width / 2,
            _semanticObjects[_selectedObject].Y + _semanticObjects[_selectedObject].Height / 2);
        var width = ActualWidth > 0 ? ActualWidth : 1_000;
        var height = ActualHeight > 0 ? ActualHeight : 700;
        _pan = new Vector(width / 2 - center.X * _zoom, height / 2 - center.Y * _zoom);
    }

    private void DrawSemanticObjects(DrawingContext drawingContext)
    {
        foreach (var (name, bounds) in _semanticObjects)
        {
            var isSelected = name == _selectedObject;
            var fill = name switch
            {
                "Math" => CreateBrush(Color.FromRgb(214, 236, 255)),
                "Graph" => CreateBrush(Color.FromRgb(218, 247, 226)),
                _ => CreateBrush(Color.FromRgb(232, 232, 232))
            };

            drawingContext.DrawRoundedRectangle(
                fill,
                isSelected ? SelectionPen : CreatePen(Color.FromRgb(35, 56, 77), 2),
                bounds,
                12,
                12);

            var text = new FormattedText(
                name == "Locked group" ? "Locked group [locked]" : name,
                System.Globalization.CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight,
                new Typeface(new FontFamily("Segoe UI"), FontStyles.Normal, FontWeights.SemiBold, FontStretches.Normal),
                28,
                Brushes.Black,
                1);
            drawingContext.DrawText(text, new Point(bounds.X + 18, bounds.Y + 18));

            if (name == "Math")
            {
                DrawText(drawingContext, "x^2 + y^2 = r^2", bounds.X + 18, bounds.Y + 70, 24);
            }
            else if (name == "Graph")
            {
                var graphPen = CreatePen(Color.FromRgb(20, 120, 65), 4);
                drawingContext.DrawLine(graphPen, new Point(bounds.X + 35, bounds.Bottom - 35), new Point(bounds.Right - 25, bounds.Bottom - 35));
                drawingContext.DrawLine(graphPen, new Point(bounds.X + 35, bounds.Bottom - 35), new Point(bounds.X + 35, bounds.Y + 60));
                var curve = new StreamGeometry();
                using (var curveContext = curve.Open())
                {
                    curveContext.BeginFigure(new Point(bounds.X + 40, bounds.Bottom - 45), false, false);
                    curveContext.BezierTo(
                        new Point(bounds.X + 110, bounds.Y + 50),
                        new Point(bounds.X + 220, bounds.Bottom - 30),
                        new Point(bounds.Right - 35, bounds.Y + 70),
                        true,
                        false);
                }

                drawingContext.DrawGeometry(null, graphPen, curve);
            }
        }
    }

    private Point ViewportToWorld(Point point) =>
        new((point.X - _pan.X) / _zoom, (point.Y - _pan.Y) / _zoom);

    private static SceneObject[] CreateObjects()
    {
        var objects = new SceneObject[VectorObjectCount];
        var index = 0;
        for (var row = 0; row < 100; row++)
        {
            for (var column = 0; column < 100; column++)
            {
                objects[index] = new SceneObject(
                    new Rect(20 + column * 39.5, 18 + row * 29.5, 24, 16),
                    (row + column) % ObjectBrushes.Length);
                index++;
            }
        }

        return objects;
    }

    private static StreamGeometry[] CreateInkPaths()
    {
        const int paths = 100;
        const int pointsPerPath = InkPointCount / paths;
        var pathsGeometry = new StreamGeometry[paths];

        for (var pathIndex = 0; pathIndex < paths; pathIndex++)
        {
            var geometry = new StreamGeometry();
            using (var context = geometry.Open())
            {
                for (var pointIndex = 0; pointIndex < pointsPerPath; pointIndex++)
                {
                    var x = 35 + pointIndex * (3_900d / (pointsPerPath - 1));
                    var y = 35 + pathIndex * 29.2 +
                            Math.Sin(pointIndex * 0.045 + pathIndex * 0.25) * 7;
                    var point = new Point(x, y);
                    if (pointIndex == 0)
                    {
                        context.BeginFigure(point, false, false);
                    }
                    else
                    {
                        context.LineTo(point, true, false);
                    }
                }
            }

            geometry.Freeze();
            pathsGeometry[pathIndex] = geometry;
        }

        return pathsGeometry;
    }

    private static void DrawText(DrawingContext context, string text, double x, double y, double size)
    {
        var formattedText = new FormattedText(
            text,
            System.Globalization.CultureInfo.InvariantCulture,
            FlowDirection.LeftToRight,
            new Typeface("Cambria Math"),
            size,
            Brushes.Black,
            1);
        context.DrawText(formattedText, new Point(x, y));
    }

    private static Pen CreatePen(Color color, double thickness)
    {
        var pen = new Pen(CreateBrush(color), thickness);
        pen.Freeze();
        return pen;
    }

    private static SolidColorBrush CreateBrush(Color color)
    {
        var brush = new SolidColorBrush(color);
        brush.Freeze();
        return brush;
    }

    private readonly record struct SceneObject(Rect Bounds, int BrushIndex);
}
