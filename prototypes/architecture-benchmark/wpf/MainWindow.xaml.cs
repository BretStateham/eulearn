using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Ink;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Eulearn.ThrowawayArchitectureBenchmark;

public partial class MainWindow : Window
{
    private Point? _panStart;
    private InkCanvasEditingMode _editingModeBeforePan;
    private RenderTargetBitmap? _lastBenchmarkFrame;

    public MainWindow()
    {
        InitializeComponent();
        DataContext = this;
        InkSurface.DefaultDrawingAttributes = new DrawingAttributes
        {
            Color = Color.FromRgb(0, 88, 140),
            Width = 4,
            Height = 4,
            FitToCurve = true,
            IgnorePressure = false
        };
    }

    public ObservableCollection<BenchmarkResult> Results { get; } = [];

    public async Task<IReadOnlyList<BenchmarkResult>> RunAllBenchmarksAsync()
    {
        Results.Clear();
        foreach (var (label, scale) in Scenarios)
        {
            await RunScenarioAsync(label, scale);
        }

        StateText.Text = "Complete: all capacity scenarios rendered. Results remain visible below.";
        return Results.ToArray();
    }

    private static (string Label, double Scale)[] Scenarios =>
    [
        ("1%", 0.01),
        ("100%", 1),
        ("6400%", 64)
    ];

    private async Task<BenchmarkResult> RunScenarioAsync(string label, double scale)
    {
        StateText.Text = $"Running {label}: rendering 10,000 vectors and 100,000 ink points...";
        Scene.SetZoom(scale);
        ZoomValue.Text = label;
        await Dispatcher.InvokeAsync(() => { }, DispatcherPriority.Render);

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        var width = Math.Max(1, (int)Scene.ActualWidth);
        var height = Math.Max(1, (int)Scene.ActualHeight);
        var dpi = VisualTreeHelper.GetDpi(Scene);
        var bitmap = new RenderTargetBitmap(
            width,
            height,
            dpi.PixelsPerInchX,
            dpi.PixelsPerInchY,
            PixelFormats.Pbgra32);

        var stopwatch = Stopwatch.StartNew();
        bitmap.Render(Scene);
        stopwatch.Stop();
        _lastBenchmarkFrame = bitmap;

        using var process = Process.GetCurrentProcess();
        process.Refresh();
        var result = new BenchmarkResult(
            label,
            BenchmarkSceneControl.VectorObjectCount,
            BenchmarkSceneControl.InkPointCount,
            stopwatch.Elapsed.TotalMilliseconds,
            GC.GetTotalMemory(false) / 1024d / 1024d,
            process.PrivateMemorySize64 / 1024d / 1024d,
            "Rendered");

        Results.Add(result);
        ResultsGrid.ScrollIntoView(result);
        StateText.Text =
            $"{label} complete: {result.RenderMilliseconds:F2} ms, " +
            $"{result.ManagedMegabytes:F2} MB managed, {result.PrivateMegabytes:F2} MB private.";
        return result;
    }

    private async void RunOnePercent_Click(object sender, RoutedEventArgs e) =>
        await RunScenarioAsync("1%", 0.01);

    private async void RunOneHundredPercent_Click(object sender, RoutedEventArgs e) =>
        await RunScenarioAsync("100%", 1);

    private async void RunSixThousandFourHundredPercent_Click(object sender, RoutedEventArgs e) =>
        await RunScenarioAsync("6400%", 64);

    private async void RunAll_Click(object sender, RoutedEventArgs e) =>
        await RunAllBenchmarksAsync();

    private void ClearResults_Click(object sender, RoutedEventArgs e)
    {
        Results.Clear();
        _lastBenchmarkFrame = null;
        StateText.Text = "Results cleared. The rendered scene remains visible.";
    }

    private void SelectMath_Click(object sender, RoutedEventArgs e) => SelectObject("Math");

    private void SelectGraph_Click(object sender, RoutedEventArgs e) => SelectObject("Graph");

    private void SelectLockedGroup_Click(object sender, RoutedEventArgs e) => SelectObject("Locked group");

    private void SelectObject(string name)
    {
        Scene.SelectSemanticObject(name);
        SelectedValue.Text = name;
        StateText.Text = name == "Locked group"
            ? "Selected locked group. Selection is visible; editing remains intentionally unavailable."
            : $"Selected representative {name} semantic object.";
    }

    private void ClearInk_Click(object sender, RoutedEventArgs e)
    {
        InkSurface.Strokes.Clear();
        InputText.Text = "Captured InkCanvas strokes cleared.";
    }

    private void InkSurface_StrokeCollected(object sender, InkCanvasStrokeCollectedEventArgs e)
    {
        var points = e.Stroke.StylusPoints;
        var minimumPressure = points.Min(point => point.PressureFactor);
        var maximumPressure = points.Max(point => point.PressureFactor);
        InputText.Text =
            $"Stroke {InkSurface.Strokes.Count}: {points.Count:N0} points; " +
            $"pressure {minimumPressure:F3}-{maximumPressure:F3}. Visual ink remains on canvas.";
    }

    private void InkSurface_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        Scene.ZoomAt(e.GetPosition(Scene), e.Delta > 0 ? 1.2 : 1 / 1.2);
        ZoomValue.Text = $"{Scene.Zoom * 100:0.##}%";
        StateText.Text = $"Interactive zoom changed to {Scene.Zoom * 100:0.##}%.";
        e.Handled = true;
    }

    private void InkSurface_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        _panStart = e.GetPosition(Scene);
        _editingModeBeforePan = InkSurface.EditingMode;
        InkSurface.EditingMode = InkCanvasEditingMode.None;
        InkSurface.CaptureMouse();
        StateText.Text = "Panning scene...";
        e.Handled = true;
    }

    private void InkSurface_PreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (_panStart is null)
        {
            return;
        }

        _panStart = null;
        InkSurface.ReleaseMouseCapture();
        InkSurface.EditingMode = _editingModeBeforePan;
        StateText.Text = "Pan complete. Scene remains in its new visual position.";
        e.Handled = true;
    }

    private void InkSurface_PreviewMouseMove(object sender, MouseEventArgs e)
    {
        if (_panStart is { } previous && e.RightButton == MouseButtonState.Pressed)
        {
            var current = e.GetPosition(Scene);
            Scene.Pan(current - previous);
            _panStart = current;
            e.Handled = true;
            return;
        }

        if (e.StylusDevice is null)
        {
            var point = e.GetPosition(InkSurface);
            InputText.Text = $"Mouse at {point.X:F0}, {point.Y:F0}; pressure unavailable.";
        }
    }

    private void InkSurface_StylusMove(object sender, StylusEventArgs e)
    {
        var points = e.GetStylusPoints(InkSurface);
        if (points.Count == 0)
        {
            return;
        }

        var point = points[^1];
        InputText.Text =
            $"Stylus at {point.X:F0}, {point.Y:F0}; pressure {point.PressureFactor:F3}; " +
            $"{points.Count:N0} event point(s).";
    }
}
