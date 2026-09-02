# Eulearn runtime and technical architecture options

Status: research input for issue #11. This report narrows the viable options; it does not select an architecture.

Scope: serviced Windows 11 on x64 and ARM64, offline-first, single-Teacher classroom whiteboard, under the product constraints resolved in issues #6-#10.

Method: primary sources only: first-party platform and framework documentation, standards, source repositories, and license texts.

## Constraint summary

The architecture must support:

- Pressure-aware pen input, touch navigation, palm rejection, mouse drawing, keyboard operation, and responsiveness no worse than Microsoft Whiteboard on the same device.
- Windows accessibility APIs, WCAG 2.2 AA, 200% scaling, high contrast, configurable shortcuts, and semantic accessibility for Math, Graph, geometry, and other Board objects.
- A semantic/vector infinite canvas spanning 1%-6400% zoom and remaining usable with 10,000 objects or 100,000 ink points.
- Offline Math notation, Cartesian/polar/parametric graphing, constrained geometry, structured teaching aids, and a scientific calculator.
- Embedded PDF-page import, PDF permission handling, one preview pipeline for tagged PDF and Windows printing, and accessible output.
- Real folders, external rename detection, atomic complete-file replacement, sync-provider coexistence, and Windows Recycle Bin deletion.
- A signed per-user offline installer, silent managed deployment, x64 and ARM64 support, and deferrable updates.
- No backend or network dependency for any core operation.

## Candidate families

1. Native Windows XAML: WinUI 3 / Windows App SDK with C#/.NET or C++/WinRT, using Win2D or Composition.
2. Native Windows WPF: .NET 10 with `InkCanvas`, optionally using Skia or Direct2D for the Board.
3. Native Win32/C++: Direct2D, DirectComposition, DirectWrite, and custom UI Automation.
4. Cross-platform native: Avalonia, Qt, or Flutter.
5. Hybrid packaged web: a native .NET, Rust/Tauri, or Electron shell hosting a Chromium canvas.
6. Installable web/PWA: Edge-installed web app using service workers and the File System Access API.

## Primary-source findings

### Ink and input

WinUI 3's inking controls are not yet stable. Microsoft's migration matrix says `InkCanvas` is experimental-only and `InkToolbar` is unavailable in the stable channel; Microsoft's August 2026 update describes inking support in Windows App SDK 2.4.1-experimental while 2.4.0 is stable:

- [What is supported when migrating from UWP to WinUI 3](https://learn.microsoft.com/en-us/windows/apps/windows-app-sdk/migrate-to-windows-app-sdk/what-is-supported)
- [What's new for Windows developers](https://learn.microsoft.com/en-us/windows/apps/whats-new/whats-new-for-developers)

The underlying WinRT ink stack remains available. `InkPresenter` handles ink input and `CoreWetStrokeUpdateSource` permits processing wet ink before rendering, including ruler/stencil-style constraints:

- [InkPresenter](https://learn.microsoft.com/en-us/uwp/api/windows.ui.input.inking.inkpresenter)
- [CoreWetStrokeUpdateSource](https://learn.microsoft.com/en-us/uwp/api/windows.ui.input.inking.core.corewetstrokeupdatesource)

WPF continues to ship a stable first-party `InkCanvas`:

- [WPF InkCanvas](https://learn.microsoft.com/en-us/dotnet/api/system.windows.controls.inkcanvas)

Chromium-based stacks can delegate predicted ink trails to Windows DirectComposition through the Ink API, while Pointer Events expose pressure and tilt:

- [Ink API](https://developer.mozilla.org/en-US/docs/Web/API/Ink_API)
- [DelegatedInkTrailPresenter](https://developer.mozilla.org/en-US/docs/Web/API/DelegatedInkTrailPresenter)
- [Microsoft Edge delegated ink explainer](https://github.com/MicrosoftEdge/MSEdgeExplainers/blob/main/DelegatedInkTrail/explainer.md)
- [PointerEvent](https://developer.mozilla.org/en-US/docs/Web/API/PointerEvent)

No primary source establishes comparative end-to-end latency for Eulearn's workload. Physical-device benchmarking remains mandatory.

### Accessibility

Windows requires programmatic access through UI Automation, keyboard access, and contrast support; custom controls require custom automation peers/providers:

- [Develop accessible Windows apps](https://learn.microsoft.com/en-us/windows/apps/develop/accessibility)

WPF and WinUI expose native automation-peer models. Avalonia documents its own automation-peer bridge:

- [Avalonia accessibility](https://github.com/AvaloniaUI/avalonia-docs/blob/main/docs/app-development/accessibility.md)

Electron uses Chromium's accessibility tree and can explicitly enable accessibility support:

- [Electron accessibility](https://www.electronjs.org/docs/latest/tutorial/accessibility)

Every candidate still needs custom semantic exposure for a Board canvas. Chromium-to-UIA availability is established, but fidelity for Math, Graph, locked groups, and canvas navigation must be tested with Narrator and Accessibility Insights.

### Math, graphing, and geometry ecosystem

The strongest offline-capable, permissively licensed components are web-native:

| Component | Primary capability | License and offline fit |
|---|---|---|
| [MathLive](https://github.com/arnog/mathlive) | Editable math field; LaTeX, MathML, ASCIIMath, MathJSON; virtual keyboard and accessibility support | MIT; locally bundleable |
| [JSXGraph](https://github.com/jsxgraph/jsxgraph) | Interactive geometry, function plotting, charts, multitouch; SVG/canvas rendering | Dual LGPL-3.0 or MIT; locally bundleable |
| [GeoGebra](https://www.geogebra.org/license) | Broad graphing, geometry, and CAS | Commercial use requires a special license |
| [Desmos API](https://www.desmos.com/api/v1.11/docs/index.html) | Mature graphing UX | Hosted script and API key conflict with the hard-offline contract |

No equivalent documented MIT-licensed .NET/C++ ecosystem covers editable typeset math, interactive graphing, and constrained geometry together. A native-only option must build these systems or host a web island.

### Canvas and rendering

Win2D provides GPU-accelerated Direct2D drawing for WinUI 3, but its documentation states that the WinUI 3 move remains a work in progress:

- [Win2D for WinUI 3](https://microsoft.github.io/Win2D/WinUI3/html/Introduction.htm)

Raw Direct2D/DirectComposition offers maximum control but requires substantially more rendering, interaction, and accessibility infrastructure.

Chromium supplies Canvas 2D, WebGL, WebGPU, SVG, and delegated ink. No primary benchmark establishes that any candidate meets Eulearn's 10,000-object, 100,000-ink-point, 6400%-zoom, or Whiteboard-comparative requirements. Those outcomes must be measured.

### PDF import, tagged output, and printing

Windows includes `Windows.Data.Pdf.PdfDocument`, including password-protected loading, but its documented API does not expose PDF copy/print permission flags:

- [Windows.Data.Pdf.PdfDocument](https://learn.microsoft.com/en-us/uwp/api/windows.data.pdf.pdfdocument)

PDFium exposes document permissions and has maintained x64 and ARM64 Windows binary distributions:

- [PDFium API and Windows binaries](https://github.com/bblanchon/pdfium-binaries)

For web stacks, PDF.js is Apache-2.0 and suitable for offline bundling:

- [PDF.js](https://github.com/mozilla/pdf.js)

Skia's PDF backend has a real structure tree with language, alternative text, and node binding:

- [SkPDFDocument structure APIs](https://github.com/google/skia/blob/main/include/docs/SkPDFDocument.h)
- [Skia PDF notes](https://skia.org/docs/user/sample/pdf/)

Chromium connects an accessibility tree to Skia's tagged-PDF structure and exposes tagged PDF generation through its print pipeline:

- [Chromium Skia metafile PDF test](https://github.com/chromium/chromium/blob/main/printing/metafile_skia_unittest.cc)
- [Chromium tagged PDF print delegate](https://github.com/chromium/chromium/blob/main/headless/lib/renderer/headless_print_render_frame_helper_delegate.cc)
- [Chromium DevTools tagged PDF test](https://github.com/chromium/chromium/blob/main/chrome/browser/devtools/protocol/devtools_printtopdf_browsertest.cc)

QuestPDF documents offline PDF/UA-1 and PDF/A support but becomes commercial above its community-license revenue threshold:

- [QuestPDF](https://github.com/QuestPDF/QuestPDF)

WinUI 3's `PrintManager` is supported on Windows 11 according to Microsoft's migration matrix. Chromium's shared print/PDF path naturally aligns with Eulearn's requirement that direct Print and Save PDF use one page model.

### Filesystem and Recycle Bin

Native .NET/Win32 can use Windows `ReplaceFile` and `IFileOperation`. Electron directly exposes OS trash:

- [Electron shell.trashItem](https://www.electronjs.org/docs/latest/api/shell)

A Tauri or custom WebView2 host can provide equivalent native operations.

The web File System standard defines permission-gated handles and removal but no Windows Recycle Bin operation, Windows `ReplaceFile` equivalent, or complete external-rename/file-watch contract:

- [WHATWG File System standard](https://fs.spec.whatwg.org/)

This is a product-contract failure for a pure PWA.

### Packaging, updating, and ARM64

MSIX requires signed packages and supports enterprise deployment and differential updates:

- [MSIX overview](https://learn.microsoft.com/en-us/windows/msix/overview)
- [Manage MSIX deployment](https://learn.microsoft.com/en-us/windows/msix/desktop/managing-your-msix-deployment-overview)

WebView2 Evergreen is included with Windows 11; Microsoft also provides a standalone offline installer and per-user installation behavior. Its independently serviced runtime creates a decision point against Eulearn's teaching-session update constraint:

- [Distribute WebView2 Runtime](https://learn.microsoft.com/en-us/microsoft-edge/webview2/concepts/distribution)

Tauri documents WiX/NSIS installers and an ARM64 Windows target:

- [Tauri Windows installers](https://v2.tauri.app/distribute/windows-installer/)

Electron documents Windows ARM64 builds and native-module rebuild considerations:

- [Electron Windows on Arm](https://www.electronjs.org/docs/latest/tutorial/windows-arm)

.NET 10 supports serviced Windows 11 versions on ARM64 and x64:

- [.NET 10 supported operating systems](https://github.com/dotnet/core/blob/main/release-notes/10.0/supported-os.md)

## Candidate assessment

| Family | Ink | Accessibility | Math ecosystem | Tagged PDF | Filesystem | Packaging | Assessment |
|---|---|---|---|---|---|---|---|
| WinUI 3 | Strong underlying APIs; controls pre-stable | Native UIA | Custom or web island | Skia/QuestPDF | Native | MSIX | Shortlist, timing risk |
| WPF/.NET 10 | Stable InkCanvas | Native UIA | Custom or web island | Skia/QuestPDF | Native | MSIX/per-user EXE | Shortlist |
| Win32/C++ | Maximum control | Manual UIA | Custom | Direct Skia PDF | Native | MSIX | Viable, highest cost |
| Avalonia/Qt/Flutter | Incomplete pen evidence | Bridged | Custom | Skia-family options | Native | Varies | Not favored without a cross-platform goal |
| Native shell + Chromium | Delegated Windows ink | Chromium to UIA; custom-canvas fidelity unproven | MathLive + JSXGraph | Chromium tagged print pipeline | Native shell | MSIX/NSIS | Shortlist |
| PWA | Delegated ink available | Chromium | MathLive + JSXGraph | Browser print | Fails Recycle Bin/atomic contract | Fails deployment contract | Ruled out |

## Evidence-based shortlist

### S1: Native Windows shell plus Chromium web canvas

Possible shells include .NET/WebView2, Rust/Tauri v2, or Electron.

Strengths:

- Documented offline MathLive and JSXGraph ecosystem.
- Windows delegated ink through Chromium.
- Chromium accessibility-tree-to-tagged-PDF path.
- Native host supplies Recycle Bin, atomic replacement, file watching, installer, and platform integration.

Risks:

- UI Automation fidelity for a semantic custom canvas is unproven.
- Capacity, memory, startup, and Whiteboard-comparative responsiveness are unproven.
- WebView2 Evergreen updates independently; Electron bundles Chromium but transfers the security-update burden to Eulearn.

### S2: Native .NET with optional web island

Use WPF now or WinUI 3 after inking stabilizes, with Win2D/Skia and an optional WebView2 island for Math/Graph.

Strengths:

- Strongest stable Windows ink and native UI Automation story through WPF.
- Direct Windows filesystem, printing, packaging, and accessibility integration.

Risks:

- Native math, graphing, and constrained geometry require substantial custom work.
- A web island reintroduces Chromium accessibility questions for the exact semantic objects that require strong accessibility.
- Tagged PDF requires a Skia binding or a licensing decision around QuestPDF.
- WinUI 3 inking remains experimental.

### S3: Native Win32/C++ with Direct2D/DirectComposition and Skia PDF

Strengths:

- Maximum rendering and latency control.
- Direct access to Skia's tagged-PDF structure.
- No unstable UI framework dependency.

Risks:

- Highest implementation and maintenance cost.
- Custom UI Automation implementation throughout.
- Math, graphing, and geometry systems must be built or embedded.

## Mandatory evidence before architecture choice

1. Compare pen-to-photon behavior for WinRT ink, Chromium delegated ink, and plain pointer/canvas ink against Microsoft Whiteboard on the same device.
2. Compare pan, zoom, selection, rendering, memory, launch, and Board-open behavior at 10,000 objects and 100,000 ink points at 1%, 100%, and 6400%.
3. Validate tagged PDF/UA output with representative Math, Graph, and page-clipped objects using veraPDF or equivalent.
4. Validate Narrator and Accessibility Insights behavior for Math, Graph, locked groups, selection, and canvas navigation.
5. Exercise atomic replacement and short-lived handles in OneDrive and Google Drive folders.
6. Verify Recycle Bin deletion and external rename reflection.
7. Build signed per-user offline installers, silent managed deployment, and deferrable-update behavior.
8. Build and run every native dependency on ARM64.
9. Track WinUI 3 inking stabilization or cost the lower-level custom ink fallback.
10. Verify MathLive's full notation coverage and JSXGraph's required constrained geometry/live-measurement behavior.

## Conclusion

A pure PWA cannot satisfy the validated filesystem and deployment contracts. Cross-platform native frameworks add risk without serving a stated cross-platform requirement.

The credible decision set is:

1. Native Windows shell plus Chromium web canvas.
2. Native .NET with an optional web island.
3. Native Win32/C++ with Direct2D/DirectComposition.

Documentation alone cannot choose among them. The comparative benchmark and accessibility/PDF/file-safety evidence above is a prerequisite to the sponsor's architecture decision.
