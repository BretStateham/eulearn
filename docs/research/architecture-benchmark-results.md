# Eulearn architecture benchmark probe results

Status: throwaway evidence for issue #18, used to unblock the architecture decision. These probes are not production benchmarks.

## Host

- Windows 11 Pro 10.0.26200, x64
- Intel Core i7-1370P, 14 cores
- NVIDIA RTX A500 Laptop GPU and Intel Iris Xe Graphics
- .NET SDK 10.0.303 / runtime 10.0.11
- Microsoft Edge/Chromium 152

No ARM64 hardware, instrumented high-speed camera, signing certificate, managed-device deployment environment, OneDrive/Google Drive test fixture, or Microsoft Whiteboard automation harness was available.

## Probes

- `prototypes/architecture-benchmark/web/canvas-benchmark.html`
  - Chromium Canvas 2D scene with 10,000 vector objects and 100,000 polyline points.
  - Zoom scenarios at 1%, 100%, and 6400%.
  - Pointer pressure/tilt display and Chromium delegated-ink API detection.
  - Semantic accessibility proxy controls for Math, Graph, and a locked group.
- `prototypes/architecture-benchmark/wpf/`
  - .NET 10 WPF application using `InkCanvas`.
  - The same nominal object/point counts and zoom scenarios.
  - Semantic WPF controls with UI Automation names/help.
  - x64 execution and ARM64 cross-publish.

The rendering implementations are intentionally small and structurally different. Their timing numbers are diagnostic and must not be treated as an apples-to-apples framework ranking.

## Capacity results

### Chromium Canvas 2D

Three headless Edge runs at a 1440x900 viewport produced:

| Run | Zoom | Median draw | P95 draw |
|---|---:|---:|---:|
| 1 | 1% | 4.1 ms | 10.8 ms |
| 1 | 100% | 3.3 ms | 6.3 ms |
| 1 | 6400% | 3.6 ms | 9.0 ms |
| 2 | 1% | 7.4 ms | 12.6 ms |
| 2 | 100% | 7.3 ms | 10.2 ms |
| 2 | 6400% | 9.2 ms | 10.5 ms |
| 3 | 1% | 9.3 ms | 17.1 ms |
| 3 | 100% | 6.9 ms | 9.6 ms |
| 3 | 6400% | 8.2 ms | 12.2 ms |

The scene rendered at every required scale. The Ink API was available. Headless JavaScript draw duration does not measure interactive composition or physical pen-to-photon latency.

### WPF

The noninteractive .NET 10 probe used one synchronous `RenderTargetBitmap.Render` of the visible scene:

| Zoom | Objects | Ink points | Render | Managed | Private |
|---|---:|---:|---:|---:|---:|
| 1% | 10,000 | 100,000 | 104.18 ms | 2.62 MB | 166.14 MB |
| 100% | 10,000 | 100,000 | 131.84 ms | 3.02 MB | 165.21 MB |
| 6400% | 10,000 | 100,000 | 101.57 ms | 3.15 MB | 168.55 MB |

The scene rendered at every required scale. `RenderTargetBitmap` is a CPU-side capture, not WPF's interactive compositor, so these values cannot be compared directly with the Chromium frame durations. They do demonstrate that a naive WPF retained-object/capture path needs aggressive culling, batching, or a separate renderer.

## Physical stylus observation

The sponsor drew directly in both probes on the same Windows device:

- Chromium probe: "very responsive."
- WPF probe: "had a slight lag."

This is qualitative rather than instrumented, but it is the only same-device physical-input evidence gathered. It favors retaining Chromium delegated ink in the architecture shortlist and does not justify a strict numerical latency claim.

## Accessibility

The WPF probe exposed named controls through Windows UI Automation, including:

- `Math semantic object`
- `Graph semantic object`
- `Locked group semantic object`
- `Pressure-sensitive benchmark ink canvas`

Chromium's full accessibility tree exposed:

- Button: `Math object: quadratic formula`
- Button: `Graph object: Cartesian y equals x squared, viewport negative ten to ten`
- Disabled button: `Locked group: lesson heading and directions`

Both stacks can expose semantic proxies. Neither probe validates the complete Board navigation model, reading order, structured Math speech, focus behavior at 200% scaling, or WCAG 2.2 AA.

## Tagged PDF

Chromium generated a tagged Letter PDF through its print pipeline. `pdfinfo -struct-text` found a document structure, heading, forms, and a figure, confirming that tags were emitted.

The same inspection also reported structure warnings/errors, including unexpected structure-element types and role attributes. The canvas itself appeared as one `Figure`; the hidden semantic proxy controls did not automatically become adequate descriptions for Board content.

Conclusion: Chromium provides the tagged-PDF mechanism, but Eulearn cannot rely on automatic browser output alone. The architecture needs a deliberate print DOM or direct Skia/PDF structure builder, plus PDF/UA validation in milestone zero.

## ARM64

The WPF probe restored and cross-published successfully for `win-arm64` on the x64 host. It was not executed on ARM64 hardware. The web probe has no native module, but the eventual native shell, PDFium/Skia bindings, installer, and any native dependencies still require ARM64 build and execution evidence.

## Findings for the architecture decision

1. Chromium can represent the required synthetic scene, exposes delegated ink, felt responsive under physical stylus use, and exposes semantic controls through its accessibility tree.
2. WPF supplies stable native InkCanvas and UI Automation, but this probe felt slightly laggier and its naive large-scene render path is unsuitable without a specialized renderer.
3. Chromium's automatic tagged PDF is not sufficient by itself; the selected architecture must plan an explicit accessible print representation and conformance testing.
4. A native host remains mandatory for filesystem, Recycle Bin, atomic replace, deployment, update, and Windows integration requirements.
5. Native Win32/Direct2D remains theoretically strongest for rendering control but was not prototyped; its manual accessibility and math/graph implementation cost remains the principal reason not to select it without evidence of a Chromium blocker.

## Milestone-zero gates

The architecture decision may proceed with these mandatory implementation gates:

1. Instrumented Microsoft Whiteboard comparison for physical ink and interactive scene behavior.
2. Product-shaped spatial index, culling, batching, selection, and memory benchmark rather than the flat diagnostic scenes.
3. Narrator and Accessibility Insights test of the actual semantic Board model.
4. Tagged PDF/UA conformance for Math, Graph, geometry, clipped cross-page objects, and reading order.
5. OneDrive and Google Drive atomic-save, temporary-lock, conflict-copy, external-rename, and Recycle Bin tests.
6. Signed per-user offline installer, silent managed deployment, update-deferral test, and uninstall preservation.
7. ARM64 build and physical execution of every native dependency.
8. MathLive and JSXGraph coverage validation if selected.

These are acceptance gates, not unresolved product decisions. Failure of a gate must trigger the documented architecture fallback rather than silently relaxing a product requirement.
