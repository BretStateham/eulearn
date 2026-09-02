# Eulearn implementation roadmap

Status: implementation slicing for map issue [#3](https://github.com/BretStateham/eulearn/issues/3), resolving [#16](https://github.com/BretStateham/eulearn/issues/16).

This roadmap sequences already-decided product, interaction, architecture, persistence, and security contracts (#4–#15, #18). It adds **no new product decisions**. Where a source ticket deferred a detail, it stays deferred here; only ordering, prerequisites, and acceptance evidence are new.

Sponsor direction governing the order: **do not overengineer — get to a buildable whiteboard quickly.** Slices are vertical (each ends in something the Teacher can do), not component phases. Foundations appear only when the slice that needs them arrives.

## Locked architecture baseline

- .NET 10 Windows host (serviced Windows 11 x64/ARM64) + Evergreen WebView2, one React/TypeScript/Vite surface (#12).
- Framework-independent semantic Board core; hybrid DOM/SVG/Canvas renderer; delegated Ink for wet ink (#12).
- Native host owns file writes, autosave scheduling, recovery, atomic replacement, watching, dialogs; least-privileged workers own PDF/import/print (#12).
- Typed, versioned, capability-allowlisted host protocol; fixed local origin; restrictive CSP; everything bundled offline (#12, #15).
- `.eulearn` / `.eulearnt` deterministic ZIP-compatible semantic packages, content-addressed assets, hashes, typed versions, user-local recovery journal, edit lease, atomic compaction (#14).
- Minimal security baseline: inert imports, schema/path validation, typed native operations, secret redaction, temp cleanup, pinned lockfiles, signed releases (#15).

## Milestone-zero gates

Gates come from #18 and `docs/research/architecture-benchmark-results.md`. Each gate blocks **only its own subsystem slice**; a failure triggers the documented fallback from #12, not a full pivot. A full WPF/Win32 pivot is considered only if multiple core WebView subsystems fail.

| Gate | Evidence required | Gated slice | Fallback on failure |
|---|---|---|---|
| G1 Ink | Instrumented physical-ink comparison vs Microsoft Whiteboard on the same device; delegated ink working | S1 | Native WinRT wet-ink surface composited with WebView2 |
| G2 Capacity | Product-shaped spatial index, culling, batching, selection, memory, launch/open at 10,000 objects and 100,000 ink points at 1%, 100%, 6400% | S2 (re-run in S6) | Skia or Direct2D renderer adapter behind the renderer interface |
| G3 Accessibility | Narrator + Accessibility Insights over the real semantic Board model; WCAG 2.2 AA | S6 (smoke check in S2) | Native UI Automation overlay/peer bridge |
| G4 Tagged PDF | PDF/UA conformance incl. clipped cross-page objects and reading order; extended to Math/Graph/geometry in S5 | S4 (extended in S5) | Direct Skia structure-tree PDF adapter |
| G5 File safety | OneDrive + Google Drive atomic replacement, short-lived handles, transient-lock surfacing, conflict copies, external rename reflection | S3 | Persistence adapter change within the #14 protocol |
| G6 Deployment | Signed per-user offline installer, silent managed deployment, update deferral, uninstall preserves Board files | S7 | Packaging path change; product requirements unchanged |
| G7 ARM64 | Every native dependency built **and physically executed** on ARM64 | S4 (PDFium), completed in S7 | Replace the failing native dependency |
| G8 Math libraries | MathLive notation coverage for the #7 scope; JSXGraph constrained geometry and live measurement | S5 | Alternative library behind the same Eulearn semantic adapter |

The Recycle Bin portion of #18's file-safety evidence is void — see [Lifecycle reconciliation](#lifecycle-reconciliation).

---

## S0 — Repository and application skeleton

**Unlocks:** nothing for the Teacher yet; produces the first launchable Eulearn the sponsor can run.

**User-visible scope:** app launches offline to a branded empty window with the one-row labeled ribbon shell (#9), an empty Board surface, version/About, and a visibly distinct development build (#15).

**Technical prerequisites:** .NET 10 host + WebView2 with documented offline runtime path; React/TypeScript/Vite surface; repository layout separating host, web app, framework-independent Board core, and tests; typed asynchronous host capability protocol v0 generated from shared schemas (request IDs, progress, cancellation, typed errors); capability allowlist; fixed local origin and CSP; pinned NuGet/npm lockfiles with license review; local logging with configurable verbosity; single-command build/run; CI build for x64 and ARM64.

**Excluded:** persistence, ink, selection, Library, importers, math, signing, managed deployment, worker processes, any plugin API.

**Acceptance evidence:** host launches with networking disabled and loads no remote resource (zero CSP violations); protocol round-trip test covering success, typed error, progress, and cancellation; ARM64 cross-build succeeds; lockfiles committed; dev build distinguishable from release build.

---

## S1 — Minimal usable ink whiteboard on a real file

**Unlocks:** the Teacher opens Eulearn, writes with the pen at classroom quality, saves to a real file, and reopens it later. First dogfood build.

**User-visible scope:** pressure-aware pen and highlighter, stroke and area erasers, color/width choices; infinite Board with pan/zoom (one-finger pan, pinch zoom, wheel/Shift+wheel/Ctrl+wheel, Space+drag); latched tool selection with spring-loaded temporary tool via press-and-hold and pen barrel button; Undo/Redo for content; `Saving`/`Saved` status with `Ctrl+S` immediate flush; New Board with the `YYMMDD  HHmm - Untitled Board` default name; Open and Save through native dialogs; Show in File Explorer.

**Technical prerequisites:** retained semantic scene graph with spatial index, immutable snapshots and targeted deltas, viewport culling, batching, layered rendering with Canvas 2D for ink; delegated Ink wet-ink presentation with dry-ink commit into the scene; semantic command model shaped so commands are journalable later (#14); logical device-independent coordinates at 96 units/inch with Board Origin `(0,0)`; minimal `.eulearn` writer/reader (versioned manifest, normalized scene JSON, SHA-256 entry integrity, deterministic entry ordering) using atomic same-folder replacement; native file dialogs through the host protocol.

**Excluded:** Board Library, Templates, recovery journal and the one-second durability claim, object selection/manipulation, text, shapes, ink-to-shape, images, PDF, math, printing.

**Acceptance evidence:** **G1** passes or its fallback is adopted; write→reopen round trip reproduces the scene and validates all declared hashes; repeated saves of an unchanged Board are byte-deterministic; a partially written package is never observable; the whole slice works with networking disabled. Durability at this point is explicit save and clean close only — the one-second bound is claimed in S3.

---

## S2 — Objects, manipulation, text, and locking

**Unlocks:** preparation and live annotation — the Teacher types, arranges, groups, locks template-style material, and manipulates content with familiar Windows graphics conventions.

**User-visible scope:** lasso and marquee selection with persistent handles until blank-space click or Escape; move/resize/rotate with constrained, proportional, and center-based modifiers; z-order, group/ungroup, align/distribute, copy/paste, duplicate, delete; Lock/Unlock with lock badge, dashed outline, copy still permitted, prominent ribbon Unlock, immediately undoable; multiline text created in place (click-drag bounded box, click auto-width, double-click edit, Escape returns to prior tool) with font family, size, emphasis, alignment, and color; basic shapes, straight lines, arrows, connectors; optional ink-to-shape toggle where immediate Undo restores original ink; contextual ribbon region swapping on selection with responsive overflow, pinning, and reordering; Board Setup for page size and extent mode with preview that does not move content; independent Origin and Page Guides toggles; Previous View history; Escape cancels transient gestures.

**Technical prerequisites:** object, selection, and transform model in the framework-independent core; hit testing over the spatial index; SVG/DOM layer for semantic and vector text; command identity registry with user-defined shortcut binding, conflict detection, reserved OS/accessibility combinations, reset, and profile import/export (#8) — introduced here, before the command surface grows.

**Excluded:** Math, Graph, geometry, tables, number lines, coordinate planes, grid regions; images and PDF; a general Layers panel; handwriting or ink-to-text recognition.

**Acceptance evidence:** **G2** passes or its fallback is adopted; selection, manipulation, and text remain usable at the capacity target; display connect/disconnect/resize/rotate/DPI change preserves Board coordinates and viewport with no restart; Narrator smoke check exposes names, roles, and states for objects and locked groups (full conformance is S6); all non-drawing commands in this slice are keyboard operable and rebindable.

---

## S3 — Board Library, native format completion, and recovery

**Unlocks:** the full prepare–teach–recover–reuse loop across classes and terms: Templates, duplication, resume, and crash recovery.

**User-visible scope:** Board Library over multiple registered root folders showing real subfolder structure; browse current folder, recursive filename search, Boards/Templates/both filters, global Recent spanning roots and outside-Library items, one-click Resume Last Board; rename in Eulearn renames the file, external renames are reflected, same-folder conflicts take the first free ` (n)` suffix; Duplicate as independent copy; Create Board from Template and Create Template from Board; insert content from another Board or Template as one placed, immediately independent selected group; open a native file in place outside the Library with Add to Library; Show in File Explorer; external moves, renames, and disappearance update the Library; launch after abnormal termination shows a Recovered Board card (name, last durable save, recovery time) offering Open Recovered, Open Last Saved, Save Recovered As, Discard Recovery, and never auto-projects; save-failure state shows the high-contrast banner with cause, last successful save, Retry and Save As/Save Copy Elsewhere, plus the red `Not saved` badge, taskbar warning mark, and disableable one-time audible alert; closing with unsaved edits offers Save As, Keep Open, or explicit Discard.

**Technical prerequisites:** complete `.eulearn`/`.eulearnt` package contract (Deflate for text entries, stored pre-compressed assets, content-addressed deduplicated assets, per-entry size and SHA-256, package generation ID, `formatVersion`/`minimumReaderVersion`/required-feature manifest, unknown-field and unreferenced-entry preservation, safe read-only rendering for unsupported required features, warned downgraded Save As); append-only user-local recovery journal flushing every accepted semantic command within one second with ink coalescing and periodic scene checkpoints; compaction that writes, flushes, validates, and atomically replaces a temporary sibling, then deletes the journal; user-local edit lease with read-only second open and Open Independent Copy; stable file identity plus last-read content hash, external replacement moving the Board to Not Saved with no semantic merge; file watching; user/machine state store for window geometry, ribbon customization, shortcut profiles, recents, roots, and per-Board last export folder.

**Excluded:** Archive state or view, Delete/Trash/Recycle Bin commands (superseded — see below), cross-session version-history browser, any cloud account/backup/sync feature, bespoke cloud conflict UI, semantic merge.

**Acceptance evidence:** **G5** passes or a persistence adapter change is adopted; kill/power-interruption harness loses at most one second of accepted input; journal replay restores the recovered scene and reopening starts a fresh undo session; corrupt packages open a read-only recovery surface and never overwrite the damaged original; a package written by a newer feature set round-trips unknown data unchanged; provider conflict copies open as ordinary separate Boards.

---

## S4 — Import, PDF export, and printing

**Unlocks:** bring prepared material in and hand the taught Board out — the last piece of the #5 daily-use threshold.

**User-visible scope:** image import (PNG, JPEG/JPG, BMP, GIF first frame, WebP, SVG) via file dialog, clipboard, and drag/drop, preserving transparency, aspect ratio, and reliable physical-size metadata (else 96 px/inch); PDF import dialog with page thumbnails and page/range selection producing one embedded resolution-independent PDF Page object per page (selectable, movable, proportionally resizable, rotatable, croppable, lockable, copyable) with in-memory password prompt, honored copy/print permissions, and specific errors that leave the Board unchanged; atomic previewed import where the cursor shows arranged group bounds and one click places the group, optionally snapping pages to Page Guides in row-major order; page scopes All Occupied Pages, Current View, Selected Objects, and explicit range with exact clipping at page boundaries and no silent moving, scaling, or duplication; one shared preview pipeline feeding both Save PDF and direct Print, with page numbers, orientation, margins, blank/occupied pages, scaling, and zoom/navigation, enabling export only after a successful render; Save PDF as explicit Save As suggesting `<Board name>.pdf`, remembering the last export folder per Board, confirming overwrite, writing through an atomic temporary sibling; 100% default print scale with Fit to Printable Area and custom percentage, non-printable margins shown; pre-export accessibility panel listing missing descriptions and reading order with explicitly warned override; long operations show determinate progress and Cancel and leave no partial insertion or destination file.

**Technical prerequisites:** least-privileged worker processes for PDF parsing/rendering (PDFium, permission-aware), image/document decode, tagged PDF generation, and print preparation, where one worker failure fails one operation without harming the open Board; a dedicated semantic print document built from the Board model rather than the interactive canvas; inert handling of SVG, HTML fragments, and PDF content (no scripts, macros, event handlers, embedded document JavaScript, or launch actions); per-user temp storage with cleanup on success, cancellation, and startup; PDFium x64 and ARM64 qualification.

**Excluded:** editing or extracting original PDF elements; PowerPoint, Word, OneNote, and Microsoft Whiteboard import; view-only links; continuous PDF regeneration during autosave; an `Avoid splitting objects` option.

**Acceptance evidence:** **G4** passes for text, images, and page-clipped objects (Math/Graph/geometry coverage repeats in S5) or the Skia structure-tree adapter is adopted; **G7** partially satisfied by physical ARM64 execution of the PDF worker; direct Print output matches the previewed pages; killing a worker mid-operation fails only that operation; malformed input leaves Board and destination untouched; PDF passwords never appear in logs, journals, packages, or diagnostics.

---

## S5 — Structured Math, Graph, and geometry studio

**Unlocks:** teaching a full math period without another graphing, geometry, notation, or calculator application — the #7 completion boundary.

**User-visible scope:** right-side authoring studio, transient by default and pinnable for preparation, retargeting when another compatible object is selected; compact searchable Insert palette as an alternate entry point; Math objects with synchronized visual structured editor, grouped symbol/template sets covering the #7 notation scope, optional LaTeX-style source, and live typeset preview; Graph objects holding a named expression list with per-expression Cartesian/polar/parametric mode, domain, visibility, and style over shared viewport/axes/grid, organized into Expressions, View, Analyze, and Sliders, with trace, coordinates, roots, intersections, extrema, value tables, and numeric sliders redrawing live; geometry constructed and manipulated directly on the Board with snapping, constraints, and measurements that update as objects move, with exact values, labels, and options in the studio; number lines, tables with in-cell editing, coordinate planes, and grid regions as ordinary semantic Board objects; scientific calculator as another studio mode with Board-local expression history that can insert a result as editable Math, insert expression and result, or send an expression to a Graph; Accessibility section exposing generated, editable descriptions with flags for missing or ambiguous entries; draft edits preview live without mutating the committed object, Apply commits one Undo step and triggers autosave, Cancel/Escape restores, and a pinned studio requires Apply or discard before retargeting.

**Technical prerequisites:** Eulearn-owned semantic domain models for Math, Graph, geometry, number lines, tables, coordinate planes, and grid regions persisted in the package (never MathLive/JSXGraph-private structures); MathLive and JSXGraph bundled locally behind Eulearn adapters with coverage tests, using JSXGraph's MIT option; Eulearn-owned numeric evaluation/analysis service shared by Graph, calculator, and export; description generator shared with the pre-export accessibility panel.

**Excluded:** symbolic CAS, handwriting/ink-to-math, implicit relations, inequality regions, 3D graphing, differential-equation fields, symbolic explanations, theorem-aware dynamic geometry, proof tooling, construction scripting, virtual manipulatives.

**Acceptance evidence:** **G8** passes or an alternative library is adopted behind the same adapter; **G4** re-run covers Math, Graph, and geometry with semantic alternatives and deterministic reading order; structured objects round-trip through save/open without loss and contain no library-private structures; Apply produces exactly one undo step and one autosave.

---

## S6 — Accessibility, capacity, and quality hardening

**Unlocks:** dependable use by any Teacher, on any supported device, at classroom scale — closing the #8 release gates.

**User-visible scope:** every non-drawing action keyboard operable; complete command identity coverage with shortcut assignment, conflict detection, reserved combinations, reset, and profile import/export; visible focus, usable UI at 200% scaling, working high-contrast modes, no meaning conveyed by color alone; WCAG 2.2 AA for applicable UI and exported content; Narrator navigation of the semantic Board including selection, locked groups, and structured objects; abundant concise tooltips and contextual `?` quicklinks opening the exact bundled help topic; verified 1%–6400% zoom quality; verified 10,000-object / 100,000-ink-point usability on realistic Boards; display and DPI change behavior; log verbosity settings with secret redaction.

**Technical prerequisites:** accessibility fallback decision point (native UIA overlay/peer bridge if Chromium proves insufficient); performance instrumentation retained from S2; help authored once as static browser-viewable content suitable for local bundling and online hosting.

**Excluded:** enterprise hardening deferred in #15 (custom sandboxing, SBOM/provenance, response SLAs, penetration testing, compliance certification), telemetry exporters, app-managed encryption.

**Acceptance evidence:** **G3** passes or the native UIA bridge is adopted; **G2** re-run on product-shaped Boards; comparative responsiveness against Microsoft Whiteboard on the same device is recorded for launch, Board open, ink, pan/zoom, selection, and display change, with no classroom-disrupting stall or input loss and no absolute latency promise; WCAG 2.2 AA audit recorded with defects closed.

---

## S7 — Release packaging and servicing

**Unlocks:** installing and maintaining Eulearn on classroom and managed devices without disturbing teaching.

**User-visible scope:** signed MSIX per-user installation without administrator rights; offline installer including the WebView2 runtime path; silent deployment for managed-device tooling; signed, optional, deferrable updates that never install or restart during teaching; WebView2 compatibility verified before opening a Board; uninstall leaves Teacher Board files untouched; bundled offline help opened by default with an option to use the current online version; release builds visibly distinct from development builds.

**Technical prerequisites:** signing certificate and signing pipeline; managed-device test environment; physical ARM64 hardware; update distribution channel; final dependency license review.

**Excluded:** store distribution, forced auto-update, kill switches, provenance attestations, and the remaining #15 deferrals.

**Acceptance evidence:** **G6** passes (signed per-user offline install, silent deployment, deferral behavior, uninstall preservation); **G7** completes with every native dependency built and physically executed on ARM64; an update offered mid-session neither installs nor restarts.

---

## Release definitions

**R1 — first daily-usable release.** S0–S4 complete with gates G1, G2, G4, and G5 passed (or their fallbacks adopted), plus the R1 packaging subset pulled forward from S7: signed per-user installation and uninstall that preserves Board files. R1 is measured directly against the #5 daily-use threshold — the Teacher prepares in minutes from a blank Board, a duplicate, a Template, inserted Board content, or imported images/PDF pages; teaches a full period with pen, text, shapes, objects, and locking; survives network loss and app or device restart losing at most one second of accepted input; resumes the same Board in seconds; adapts material for another class without overwriting the source or Template; and produces a readable, printable, tagged PDF. R1 deliberately **excludes** the Math/Graph/geometry studio (S5), full WCAG 2.2 AA sign-off (S6), and managed deployment and update servicing (S7); it is a sponsor-classroom release, not general availability.

**R2 — general release.** S5, S6, and S7 complete with G3, G6, G7, and G8 passed. R2 satisfies the #7 completion boundary (a full class without any other whiteboard, graphing, geometry, notation, or calculator application) and the #8 accessibility, capacity, durability, file-safety, deployment, and help release gates.

## Lifecycle reconciliation

#14 supersedes the Archive/Delete portion of #6 and the related references in #8, with sponsor confirmation. This roadmap schedules that superseded behavior nowhere:

- No Archive state, Archive view, Delete command, Trash, or Recycle Bin operation is implemented in any slice.
- File Explorer and the Teacher's storage or sync-provider UI own moving, deleting, recovering, and permanently removing native files.
- Eulearn offers Show in File Explorer (S1, extended in S3) and observes external moves, renames, and disappearance to update the Library (S3).
- The user-defined real filesystem folder structure is the complete organization and removal model.
- Consequently the Recycle Bin item inside #18's file-safety evidence is void; G5 covers atomic replacement, short-lived handles, transient locks, conflict copies, and external rename reflection only.

## Requirements-to-slice traceability

| Requirement | Source | Slice |
|---|---|---|
| Whiteboard ink parity floor: pressure pen, highlighter, eraser modes, color/thickness | #4, #7 | S1 |
| Infinite Board, free pan/zoom, no slide navigation | #4, #7 | S1 |
| Optional ink-to-shape cleanup with Undo restore | #4, #7 | S2 |
| Pen/touch/mouse/keyboard input model, latched and spring-loaded tools, barrel-button select | #7, #9 | S1 (input), S2 (selection) |
| One-row labeled ribbon with in-place contextual commands, overflow, pin/reorder | #9 | S0 (shell), S2 (contextual) |
| Selection, constrained move/resize/rotate, order, group, align/distribute, duplicate | #7, #9 | S2 |
| Lock/Unlock semantics, badge, dashed outline, copy allowed, undoable | #7, #9 | S2 |
| Multiline formatted text created in place | #5, #7, #9 | S2 |
| Board extent modes, fixed Origin, Page Guides, Board Setup preview | #5, #7, #9 | S2 |
| Undo/redo scope, Previous View, Escape semantics | #8, #9 | S1 (content undo), S2 (view/escape) |
| Board Library over multiple real folder roots, search, filters, Recent, Resume | #6 | S3 |
| Filename-as-name, no naming gate, default names, ` (n)` conflict rule | #6 | S3 (default name in S1) |
| Duplicate, Template↔Board creation, insert content from another Board/Template | #6 | S3 |
| Open native file in place outside Library, Add to Library | #6, #10 | S3 |
| Continuous autosave with `Saving`/`Saved`/error status and `Ctrl+S` flush | #6, #8 | S1 (status), S3 (durable autosave) |
| One-second durability bound, journal, checkpoints, compaction | #8, #14 | S3 |
| Save-failure banner, badge, taskbar mark, audible alert, close options | #8 | S3 |
| Crash recovery card and four recovery actions, never auto-project | #8, #9 | S3 |
| `.eulearn`/`.eulearnt` deterministic packages, content-addressed assets, hashes | #14 | S1 (minimal), S3 (complete) |
| Version manifest, required features, read-only fallback, unknown-data preservation | #10, #14 | S3 |
| Edit lease, read-only second open, external replacement handling | #14 | S3 |
| Sync-provider coexistence: atomic replace, short handles, conflict copies | #8, #14 | S3 |
| Portable vs machine-local state separation | #14 | S3 |
| No Archive/Delete; filesystem tools own removal; Show in File Explorer | #14 (supersedes #6, #8) | S1/S3 (as exclusion) |
| Image import via file, clipboard, drag/drop with original data embedded | #7, #10 | S4 |
| PDF page import as self-contained objects, permissions, password handling | #7, #10 | S4 |
| Atomic previewed import, cursor-bounds placement, optional guide snapping | #10 | S4 |
| Page scopes, exact page clipping, occupied-page enumeration | #10 | S4 |
| Shared preview pipeline for Save PDF and direct Print, scaling and margins | #10 | S4 |
| Tagged PDF/UA output, semantic alternatives, reading order, accessibility panel | #8, #10 | S4, extended S5 |
| Export as Save As with atomic temp sibling; long-operation progress/cancel | #10 | S4 |
| Sharing via native files and PDFs only; view-only links deferred | #10 | S4 (as exclusion) |
| Math objects: visual editor, templates, optional LaTeX, live preview | #7, #13 | S5 |
| Graph objects: Cartesian/polar/parametric, expression list, view settings | #7, #13 | S5 |
| Graph analysis: trace, coordinates, roots, intersections, extrema, tables, sliders | #7, #13 | S5 |
| Precise geometry with snapping, constraints, live measurements on the Board | #7, #13 | S5 |
| Number lines, tables, coordinate planes, grid regions | #7, #13 | S5 |
| Scientific calculator with Board-local history and insert/send actions | #7, #13 | S5 |
| Studio draft/Apply/Cancel semantics, pinning, retarget rules, Insert palette | #13 | S5 |
| Editable generated accessibility descriptions for structured objects | #8, #13 | S5 |
| Full keyboard operability, command identity, user shortcut profiles | #8 | S2 (registry), S6 (complete) |
| WCAG 2.2 AA, Narrator/UIA fidelity, 200% scaling, high contrast, focus | #8 | S6 |
| Vector/semantic rendering, 1%–6400% zoom quality, raster preservation | #8 | S1/S2 (implementation), S6 (verification) |
| 10,000 objects / 100,000 ink points usability | #8 | S2 (gate), S6 (re-verify) |
| Comparative responsiveness vs Microsoft Whiteboard on the same device | #8, #18 | S1 (ink), S6 (full) |
| Display connect/disconnect/resize/rotate/DPI preserves coordinates and viewport | #8, #9 | S2 |
| Offline operation of every required capability | #7, #8, #12 | Every slice |
| Local logging with configurable verbosity, no remote telemetry | #8, #15 | S0 (baseline), S6 (settings) |
| Bundled offline help, online option, tooltips, contextual `?` links | #8 | S6 (authoring), S7 (bundling) |
| Signed per-user offline install, silent managed deployment, deferrable updates | #8, #12 | S7 (R1 subset earlier) |
| Uninstall leaves Board files untouched | #8 | S7 |
| Local bundling of all executable assets, fixed origin, CSP, no remote code | #12, #15 | S0 |
| Typed capability-allowlisted host protocol, no shell or arbitrary filesystem access | #12, #15 | S0 |
| Worker isolation for PDF/import/print, one failure fails one operation | #12, #15 | S4 |
| Untrusted-input validation of packages, PDFs, images, SVG, clipboard, messages | #15 | S1 (packages), S4 (imports) |
| Inert imported content, no script/macro/action execution | #15 | S4 |
| Secret and password redaction across logs, journals, packages, diagnostics | #15 | S4 (passwords), S6 (audit) |
| Pinned dependencies, committed lockfiles, license review | #15 | S0, revisited S7 |
| Signed releases, dev builds visibly distinct | #15 | S0 (distinct builds), S7 (signing) |
| Internal versioned extension seams; no third-party plugin API | #12 | S0 onward (as exclusion) |

## Not scheduled

Everything deferred by the source tickets remains unscheduled: handwriting recognition and ink-to-math, symbolic CAS, 3D and implicit graphing, dynamic-geometry theorem tooling, a full Layers UI, proprietary-format import, view-only links, cloud accounts/backup/sync, cross-session version history, remote telemetry exporters, enterprise security programs, school-market compliance certification, plugin APIs, and non-Windows hosts. The framework-independent Board core and typed host protocol preserve a future host option without paying for it now.
