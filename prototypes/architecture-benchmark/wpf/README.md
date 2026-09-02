# Throwaway WPF architecture benchmark

Diagnostic-only WPF/.NET 10 probe for Eulearn architecture issue #18. It has no external packages and is intentionally not reusable production code.

The window contains:

- a pressure-aware WPF `InkCanvas` for pen/stylus or mouse drawing;
- wheel zoom and right-button drag pan;
- 10,000 vector objects and 100,000 synthetic polyline points rendered at 1%, 100%, and 6400%;
- visibly persistent timing, managed-memory, private-memory, result, zoom, input, and selection state;
- selectable Math, Graph, and locked-group examples with UI Automation names/help text; and
- individual scenario buttons plus **Run all capacity scenarios**.

## Build and run

From the repository root on Windows x64 with the .NET 10 SDK:

```powershell
dotnet build .\prototypes\architecture-benchmark\wpf\Eulearn.Throwaway.WpfArchitectureBenchmark.csproj -c Release
dotnet run --project .\prototypes\architecture-benchmark\wpf\Eulearn.Throwaway.WpfArchitectureBenchmark.csproj -c Release
```

Run all capacity scenarios noninteractively and print tab-separated results:

```powershell
dotnet run --project .\prototypes\architecture-benchmark\wpf\Eulearn.Throwaway.WpfArchitectureBenchmark.csproj -c Release -- --benchmark
```

The render time is one synchronous `RenderTargetBitmap.Render` of the visible scene after layout. Memory columns are process snapshots after each render, not isolated allocations. Record hardware, runtime, display, and comparison conditions with any results.

## Limitations

- Automated render timings do not measure physical pen-to-photon latency.
- The prototype does not validate tagged PDF/UA or any tagged PDF output.
- It does not validate ARM64 execution or native ARM64 dependencies.
- It does not exercise cloud sync, OneDrive/Google Drive atomic replacement, external renames, or Recycle Bin behavior.
- It does not exercise deployment, signing, installers, managed silent install, or update deferral.
- It does not compare against Microsoft Whiteboard, Chromium delegated ink, or plain pointer/canvas ink.
- `RenderTargetBitmap` is a diagnostic CPU-side capture and is not a complete measure of WPF composition or interactive frame pacing.
- The Math and Graph examples are labels/simple drawings, not MathLive/JSXGraph feature-coverage tests.
