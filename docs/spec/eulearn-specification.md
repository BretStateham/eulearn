# Eulearn implementation-ready specification

Status: destination artifact for map issue [#3](https://github.com/BretStateham/eulearn/issues/3), resolving [#17](https://github.com/BretStateham/eulearn/issues/17).

This document assembles the resolved decisions of [#4](https://github.com/BretStateham/eulearn/issues/4)–[#16](https://github.com/BretStateham/eulearn/issues/16) and [#18](https://github.com/BretStateham/eulearn/issues/18) into one coherent, implementation-ready specification. It **adds no new product decisions**. Where a source ticket deferred a detail, it remains deferred and is listed in [§16 Non-blocking open implementation choices](#16-non-blocking-open-implementation-choices) together with the contract that constrains it.

Supporting local evidence:

- `docs/spec/implementation-roadmap.md` — slice sequencing and acceptance evidence (#16).
- `docs/research/runtime-architecture-options.md` — architecture option research (#11).
- `docs/research/architecture-benchmark-results.md` — throwaway probe evidence (#18).
- `brand/README.md` — authoritative brand assets, palette, and typography.

### Reading this document

- **MUST** — required for the release in which the requirement's slice ships; failure is a release blocker.
- **SHOULD** — strongly expected; a deviation must be recorded with justification.
- **MAY** — permitted, at implementation discretion.
- Requirement IDs are stable and area-scoped (`AREA-nnn`). IDs are never reused or renumbered; superseded requirements are struck from this document only by a later decision issue.

### Superseded and reconciled source material

| Reconciliation | Authority |
|---|---|
| The Archive state, Archive view, Delete command, Trash, and Recycle Bin behavior of #6 are **void**. Eulearn ships no Archive/Delete/Trash/Recycle Bin UI or operation. | #14 (sponsor-confirmed), restated in #16 |
| Recycle Bin wording in #8 (local security/file safety), #11 (filesystem findings), #12 (host boundaries: "Recycle Bin operations"), and #18 (file-safety evidence item 5) is **void** for the same reason. G5 covers atomic replacement, short-lived handles, transient locks, conflict copies, and external rename reflection only. | #14, #16 |
| The "explicit production security standard" and "school-market compliance" questions left open by #8 are **closed** by the minimal baseline in #15. Enterprise hardening is deferred, not required. | #15 |
| No projection-specific surface, detection, or optimization exists anywhere in the product. | #9 |
| No built-in cloud account/backup/sync and no Eulearn-hosted, peer-to-peer, or local-network view-only links. | #7, #10 |
| No school-specific compliance target. | #8, #15 |

---

## 1. Product goal and non-goals

**Eulearn** is a single-Teacher, offline-first, pen-first digital whiteboard for live high-school mathematics instruction on Windows 11. Its goal is to be a **complete daily-use replacement** for Microsoft Whiteboard in the Teacher's prepare–teach–recover–reuse loop, adding native mathematics authoring so the Teacher never leaves the app to graph, construct geometry, typeset notation, or calculate (#4, #5, #7).

The name is *Euler* + *learn* (`brand/README.md`).

**PRD-001** Eulearn MUST reach the Microsoft Whiteboard inking parity floor: pressure-aware pen and highlighter, multiple colors and thicknesses, stroke and area erasers, ink-to-shape, infinite canvas, named persistent Boards (#4).

**PRD-002** Eulearn MUST differentiate on the four axes Whiteboard cannot serve: native math authoring, offline/local-first operation, no account requirement, and portable open native files plus tagged PDF output (#4, #7).

**PRD-003** Core preparation, teaching, saving, reopening, reuse, import, and export MUST NOT require account sign-in or network availability (#7, #8).

**PRD-004** The initial product is complete only when a Teacher can teach a full class without Microsoft Whiteboard or any other graphing, geometry, notation, or calculator application (#7).

### Non-goals (explicitly rejected for the initial product)

**PRD-010** Eulearn MUST NOT implement multi-user editing, Student editing/input, comments, reactions, live cursors, presence, synchronous remote presentation, or source-linked Board/Template content (#7).

**PRD-011** Eulearn MUST NOT implement any built-in cloud account, backup, or synchronization feature, nor a bespoke cloud-conflict UI (#7, #8).

**PRD-012** Eulearn MUST NOT implement Eulearn-hosted, peer-to-peer, or local-network view-only links in the initial product (#10).

**PRD-013** Eulearn MUST NOT implement a projection-specific surface, display detection, or projection optimization. Windows display management and ordinary mirroring/extension are the mechanism (#9).

**PRD-014** Eulearn MUST NOT implement an Archive state or view, a Delete command, a Trash, or a Recycle Bin operation (#14, supersedes #6).

**PRD-015** Eulearn MUST NOT pursue feature-for-feature Microsoft Whiteboard parity outside the classroom and math outcomes specified here (#7).

**PRD-016** Eulearn MUST NOT adopt a school-specific compliance target or an enterprise security program in the initial product (#8, #15).

**Deferred capability set** (not scheduled, not blocking): automatic handwriting-to-math, ink-to-text and general handwriting recognition; symbolic CAS (exact solving, factoring, simplification, symbolic differentiation/integration); implicit relations, inequality regions, 3D graphing, differential-equation fields, symbolic graph explanations; theorem-aware dynamic geometry, proof tooling, construction scripting; virtual manipulatives, balance scales, probability simulators, subject animations, sticky notes, reactions, comments, audio/video, decorative libraries, saved presentation Views, transitions, animations; a full Layers UI; animated image playback, TIFF/HEIC/RAW, linked external images; PowerPoint, Word, OneNote, and Microsoft Whiteboard import; cross-session version-history browser; remote telemetry exporters; third-party plugin APIs; non-Windows hosts (#7, #8, #10, #12, #14, #15).

---

## 2. Domain vocabulary (glossary)

These terms are normative. Implementation code, UI strings, schemas, and tests MUST use them consistently.

| Term | Definition |
|---|---|
| **Teacher** | The single operating user. The sole editing persona of the initial product (#3, #5). |
| **Board** | A Teacher-authored infinite or page-constrained canvas of semantic objects, persisted as one self-contained `.eulearn` file. The unit of preparation, teaching, resume, duplication, and export (#5, #6, #14). |
| **Board Template** | A reusable Board-shaped item persisted as `.eulearnt`. Same package schema and lifecycle as a Board; its type controls filtering and creation behavior, not storage location (#6, #14). |
| **Class Session** | One live teaching period. Each Class Session produces its own independent taught Board; several may start from one Board Template and diverge (#5). |
| **Board Library** | The Eulearn view over one or more registered real filesystem **Library roots**, showing their actual subfolder structure (#6). |
| **Library root** | A Teacher-registered real folder. Unregistering a root never moves or deletes files (#6). |
| **Board Origin** | The fixed logical point `(0,0)` of a Board, set at creation and immutable thereafter. Anchors Page Guides; independently show/hide (#5, #9, #14). |
| **Page Guides** | Non-selectable overlay tiles of the Board's output page size laid out from the Board Origin. They never intercept input and never export (#5, #9, #10). |
| **Extent mode** | One of: infinite in both directions (default), fixed page width with vertical infinity, or fixed page height with horizontal infinity. Preserved across copy and instantiation (#5, #7). |
| **Board Setup** | The command that changes page size, orientation, and extent mode with a preview, without moving existing content (#9, #10). |
| **Object** | Any semantic Board element: ink stroke, text, shape, line/arrow/connector, image, PDF Page, Math, Graph, geometry, number line, table, coordinate plane, grid region, or group (#7, #14). |
| **Group** | An object that references child object IDs and shares selection/transform (#7, #14). |
| **Lock / Unlock** | Per-object or per-group protection blocking move, resize, rotate, format, and delete while remaining selectable and copyable. The user-visible labels are `Lock` and `Unlock`, never `Protect` (#7, #9). |
| **Ribbon** | The single always-visible labeled command row holding Board identity, primary teaching tools, global commands, an in-place contextual/property region, Undo/Redo, and save state (#9). |
| **Studio** | The right-side authoring surface for structured objects and the calculator; transient by default, pinnable for preparation (#13). |
| **Insert palette** | A compact searchable alternate entry point that chooses a structured object type and opens the Studio; it never duplicates Studio editing (#13). |
| **PDF Page object** | One imported PDF page embedded as a single resolution-independent, non-ungroupable Board object (#10). |
| **Occupied page** | A Page Guide tile intersecting visible printable Board content; the unit of `All Occupied Pages` export enumeration (#10). |
| **Recovery journal** | The append-only, user-local, per-Board command log that delivers the one-second durability bound (#14). |
| **Compaction** | Writing a complete temporary sibling package, flushing, validating, and atomically replacing the native file (#14). |
| **Edit lease** | A lightweight user-local claim keyed by stable file identity that makes a second Eulearn open read-only (#14). |
| **Milestone-zero gate** | A mandatory acceptance gate (G1–G8) from #18 that blocks exactly one subsystem slice and triggers a documented subsystem fallback on failure (#12, #16). |

---

## 3. Personas and environment

**ENV-001** The Teacher persona is a high-school mathematics teacher; the product sponsor is the authoritative proxy for it. No other persona receives dedicated functionality; only extension constraints are considered (#3).

**ENV-002** The primary environment is a pen-enabled Windows device connected to a projector or classroom display, which **mirrors** what the Teacher sees. There is no separate Student View and no private Teacher-only notes surface (#3, #5, #9).

**ENV-003** Supported OS scope MUST be currently serviced Windows 11 releases on x64 and ARM64. No Windows 10 commitment is made and no reference hardware profile is fixed (#8).

**ENV-004** Students consume Board content via the classroom display or exported artifacts only. Student editing and remote collaboration are out of scope (#3, #7).

---

## 4. End-to-end workflows

The daily-use route is the **prepare–teach–recover–reuse** loop (#5).

**WF-001 Prepare.** The Teacher MUST be able to start a Board in minutes from any of: a blank Board, a duplicate of an existing Board, a Board Template, content inserted from another Board or Template, or imported images and PDF pages. No polished slide deck is required (#5, #6, #10).

**WF-002 Teach.** The Teacher MUST be able to write and draw with imperceptible pen lag, create and format multiline typed text, reveal or add prepared material, navigate freely without losing context, and manipulate objects with familiar Windows graphics conventions, while the classroom display mirrors the Board (#5, #7, #9).

**WF-003 Recover.** Core teaching MUST continue with networking disabled and without sign-in. Accidental navigation or tool changes MUST be immediately reversible. Opening or resuming a Board MUST take seconds; recovery from app or device restart MUST take roughly ten seconds without reconstructing recent work (#5, #8).

**WF-004 Preserve and reuse.** Each Class Session MUST produce an independent taught Board. Multiple classes starting from one Board Template MUST diverge without overwriting one another or the Template. The Teacher MUST be able to create a Board Template from an existing Board and generalize it (#5, #6).

**WF-005 Export and share.** The Teacher MUST be able to export the taught Board to a printable PDF (default US Letter, alternate page size selectable at Board creation), save it to any filesystem location, and distribute it through existing services. No LMS or Microsoft 365 integration is required (#5, #10).

**WF-006 Organize and remove.** Organization and removal MUST be performed with the real filesystem: File Explorer or the storage/sync-provider UI. Eulearn offers `Show in File Explorer` and observes external moves, renames, and disappearance (#14).

---

## 5. Board model, extent, Origin, and guides

**BRD-001** A new Board MUST default to infinite extent in both directions; fixed-page-width/vertical-infinite and fixed-page-height/horizontal-infinite modes MUST also be selectable (#5, #7).

**BRD-002** Extent mode and output page size MUST be preserved when a Board or Board Template is copied or instantiated (#5).

**BRD-003** Each Board MUST have a Board Origin fixed at creation and immutable thereafter (#5, #9).

**BRD-004** Board Origin and Page Guides MUST be independently toggleable from a ribbon View group with configurable shortcuts (#5, #7, #9).

**BRD-005** Page Guides MUST be non-selectable and MUST NOT intercept ink, pointer, or any other input (#9).

**BRD-006** Content MUST be allowed above or left of the Board Origin wherever the extent mode permits (#5).

**BRD-007** Output page size MUST be selectable at Board creation, defaulting to US Letter, with A4 and custom dimensions available (#5, #10).

**BRD-008** Board Setup MUST change page size, orientation, and extent mode with a preview and MUST NOT move existing content (#9, #10).

**BRD-009** Board geometry MUST use unbounded device-independent logical coordinates with Board Origin `(0,0)` and 96 logical units per inch (#14).

**BRD-010** Navigation MUST be free pan and zoom. Named Views, slide navigation, presentation timelines, and transitions MUST NOT exist (#7).

---

## 6. Board Library and lifecycle

### 6.1 Library and organization

**LIB-001** The Teacher MUST be able to register multiple Library root folders, whose real subfolder structures are shown directly in Eulearn (#6).

**LIB-002** Removing a Library root MUST only unregister it; files MUST never be moved or deleted (#6).

**LIB-003** Normal browsing MUST show the current folder. Search MUST cover that folder and its descendants recursively by filename (#6).

**LIB-004** Views MUST be filterable to Boards only, Templates only, or both, and a global Recent view MUST span every registered root plus recently opened outside-Library items (#6).

**LIB-005** A native file opened outside a Library root MUST be edited in place and appear in recent/outside-Library context. An explicit `Add to Library` action MUST be able to copy or move it into a chosen Library folder (#6, #10).

**LIB-006** Eulearn MUST offer `Show in File Explorer` and MUST observe external moves, renames, and disappearance to update the Library (#14).

**LIB-007** Eulearn MUST NOT provide an Archive state, an Archive view, a Delete command, a Trash, or a Recycle Bin operation. The user-defined real filesystem folder structure is the complete organization and removal model (#14, supersedes #6).

### 6.2 Identity and naming

**LIB-010** The filename without its extension MUST be the sole user-visible name. Renaming inside Eulearn MUST rename the file; external renames MUST be reflected in Eulearn (#6).

**LIB-011** New items MUST open immediately without a naming gate, using local-time default names `YYMMDD  HHmm - Untitled Board` and `YYMMDD  HHmm - Untitled Template` (#6).

**LIB-012** Any same-folder name conflict arising from creation, rename, duplication, move, or `Add to Library` MUST take the first available numeric suffix before the extension (` (1)`, ` (2)`, …). Identical names in different folders are valid (#6).

### 6.3 Opening, saving, duplication, and reuse

**LIB-020** Editing MUST continuously autosave locally and expose a visible `Saving`, `Saved`, or actionable error status. `Saved` MUST mean a durable commit, not an in-memory state (#6, #8).

**LIB-021** `Ctrl+S` MUST request an immediate durable flush and MUST NOT be required for ordinary safety (#6).

**LIB-022** `Duplicate` MUST replace traditional Save As semantics and MUST create an independent copy with no live relationship to its source (#6).

**LIB-023** Launch MUST open the Board Library with recent items prominent and a one-click `Resume Last Board` action (#6).

**LIB-024** `Create Board from Template` and `Create Template from Board` MUST each create an independent item of the other type and leave the source unchanged. An item's type MUST NOT be mutated in place (#6).

**LIB-025** While editing a Board, the Teacher MUST be able to insert content from another Board or Template. Eulearn MUST copy only the source objects, preserving relative geometry, appearance, and lock state (#6).

**LIB-026** Inserted content MUST arrive as one selected group whose top-left bounds are placed at the Teacher's chosen Board point, MUST NOT import the source Origin, extent mode, page size, guides, history, or identity, and MUST be independent immediately (#6).

---

## 7. Ink, drawing tools, and input

**INK-001** Eulearn MUST provide a pressure-aware pen and highlighter with a color and thickness range at least equal to the Microsoft Whiteboard baseline, plus stroke and area erasers (#4, #7).

**INK-002** Active pen, touch, mouse/trackpad, and keyboard MUST all be first-class input. Pen pressure MUST affect supported tools; mouse drawing MUST be fully supported without pressure (#7, #9).

**INK-003** Pen MUST write while touch pans and zooms by default, with palm rejection (#7).

**INK-004** With the Pen tool active, pen contact MUST always ink. Holding the pen barrel button MUST temporarily select; mouse right-click or a configurable keyboard modifier MUST provide the equivalent temporary selection (#9).

**INK-005** Ribbon tool clicks MUST latch the chosen tool until it is changed (#9).

**INK-006** Configurable press-and-hold shortcuts and pen buttons MUST support spring-loaded temporary tools that revert to the prior tool on release (#9).

**INK-007** Default navigation MUST be one-finger touch pan, two-finger pinch zoom, wheel vertical pan, `Shift`+wheel horizontal pan, `Ctrl`+wheel zoom, and `Space`+drag temporary pan. All keyboard mappings MUST be configurable (#9).

**INK-008** Optional ink-to-shape cleanup MUST be available for common lines, arrows, circles, rectangles, triangles, and polygons, toggled by a recognizable toolbar icon, with immediate `Undo` restoring the original ink (#7).

**INK-009** Automatic handwriting-to-math, ink-to-text, and general handwriting recognition MUST NOT ship in the initial product (#7).

**INK-010** A non-persistent laser-pointer style presentation aid MAY be provided as part of the parity floor; it MUST NOT introduce any projection-specific surface or detection (#4, #9).

---

## 8. Objects, text, selection, manipulation, and locking

**OBJ-001** Eulearn MUST provide lasso and marquee selection, undo/redo, grouping/ungrouping, z-order commands, copy/paste, duplicate, delete, and align/distribute (#7).

**OBJ-002** Move, resize, and rotate MUST follow familiar Windows graphics conventions and MUST support constrained (straight-line, fixed-angle), proportional, and center-based modifiers (#5, #7, #9).

**OBJ-003** Selection outlines and handles MUST remain visible until the Teacher clicks blank space or presses `Escape` (#9).

**OBJ-004** Basic shapes, straight lines, arrows, and connectors MUST be available (#7).

**OBJ-005** Typed text MUST be an ordinary Board object: multiline, editable during preparation and class, and selectable, movable, resizable, rotatable, duplicable, and deletable (#5, #7).

**OBJ-006** Text formatting MUST include font family, size, emphasis, alignment, and color (#5).

**OBJ-007** Text MUST be created in place: click-drag creates a bounded box, click creates an auto-width box, and double-click/tap edits existing text (#9).

**OBJ-008** `Escape` MUST finish text editing and return to the previously active tool; `Enter` MUST retain normal multiline behavior (#9).

**OBJ-010** Per-object and per-group `Lock`/`Unlock` MUST exist, including locks inherited from Templates or inserted Board content (#5, #7).

**OBJ-011** Locked objects MUST remain normally selectable but MUST block move, resize, rotate, formatting, and deletion. Copy MUST remain available (#9).

**OBJ-012** A locked selection MUST show a lock badge and a distinct dashed outline, and the ribbon MUST prominently offer `Unlock`. `Lock`/`Unlock` MUST be immediately undoable (#9).

**OBJ-013** The user-visible labels MUST be `Lock` and `Unlock`, never `Protect` (#9).

**OBJ-014** A general-purpose Layers panel MUST NOT ship in the initial product (#7).

**OBJ-020** Undo/redo MUST cover Board content and object-property changes and MUST NOT cover viewport pan/zoom or tool selection (#9).

**OBJ-021** A `Previous View` command MUST traverse viewport history (#9).

**OBJ-022** `Escape` MUST cancel an active gesture or transient mode without changing committed content (#9).

**OBJ-023** Undo/redo MUST cover the current open session, bounded only by a documented memory-safety limit; reopening a Board MUST begin a new undo session (#8).

---

## 9. Ribbon and selected-object interaction

**UI-001** Eulearn MUST use exactly one always-visible labeled ribbon row containing Board identity, primary teaching tools, global commands, a contextual/property region, Undo/Redo, and save state (#9).

**UI-002** Selecting an object MUST replace the ribbon's contextual/property region in place with commands for that selection, while preserving the primary tool strip and stable global controls (#9).

**UI-003** A floating selection toolbar attached to the object and a persistent right-side *selection* panel are rejected models and MUST NOT be implemented (#9). This does not restrict the structured-object Studio of §11.

**UI-004** When display width or 200% scaling cannot fit every command, Eulearn MUST preserve accessible target and label size, MUST keep frequently used teaching tools visible, and MUST move lower-frequency commands into clearly labeled responsive overflow. Horizontal scrolling MUST be a last resort (#9).

**UI-005** The Teacher MUST be able to pin and reorder ribbon commands (#9).

**UI-006** No persistent side panel or floating toolbar may reduce Board space during ordinary teaching; the Studio is transient by default (#9, #13).

**UI-007** Tooltips MUST be abundant and concise, and a visible contextual `?` quicklink MUST open the exact relevant help topic where more explanation is needed (#8).

**BRN-001** The application icon, wordmark, lockup, palette (Chalk Teal `#14C9B6`, Deep Teal `#1C6E8C`, Stroke Teal `#12B3A3`, Slate `#12303D`, Marker Orange `#FF9F43`/`#FF7F2A`, Grid line `#DCEDEA`) and the Quicksand type family from `brand/` MUST be the product's visual identity (`brand/README.md`).

**BRN-002** Brand fonts MUST be bundled locally under the SIL Open Font License and MUST NOT be loaded from any network origin (`brand/README.md`, #12, #15).

**BRN-003** Brand color MUST NOT be the sole carrier of meaning in any UI state (#8, ACC-005).

---

## 10. Display and interruption behavior

**DSP-001** Eulearn MUST implement no projection-specific surface, detection, or optimization; the Teacher relies on Windows display management and ordinary mirroring/extension (#9).

**DSP-002** Connecting, disconnecting, resizing, rotating, or changing DPI of a classroom display MUST preserve Board coordinates and the current viewport. Only application chrome may reflow, and no restart may be required (#8, #9).

---

## 11. Math, Graph, geometry, aids, and calculator (the Studio)

### 11.1 Studio model

**STU-001** Structured mathematical content MUST be authored in a right-side Studio that is transient by default and pinnable for preparation (#13).

**STU-002** Opening a structured-object tool, or explicitly editing a structured object, MUST open the Studio. `Apply` or `Escape` MUST close it and return to the previous tool by default (#13).

**STU-003** A pinned Studio MUST retarget when another compatible object is selected (#13).

**STU-004** A compact searchable Insert palette MUST exist as an alternate entry point that chooses Math, Graph, geometry, aids, or calculator and opens the same Studio. It MUST NOT duplicate editing functionality (#13).

**STU-005** A popover attached directly to the Board object and a full keyboard-first command palette are rejected primary models and MUST NOT replace the Studio (#13).

**STU-006** While the Studio is open, edits MUST update a transient live preview and MUST NOT mutate the committed object or autosave state (#13).

**STU-007** `Apply` MUST commit the full editing gesture as exactly one Undo step and MUST trigger autosave (#13).

**STU-008** `Cancel`/`Escape` MUST restore the prior object. A pinned Studio with an outstanding draft MUST require `Apply` or discard before retargeting (#13).

### 11.2 Math objects

**STU-010** Math objects MUST be editable and typeset, authored through a visual structured editor with searchable grouped symbol/template sets and an optional LaTeX-style source view, with live typeset preview (#7, #13).

**STU-011** Editing either the visual structure or the LaTeX source MUST update the same semantic draft so the Teacher can move freely between both (#13).

**STU-012** Notation coverage MUST include arithmetic and algebraic notation, fractions, powers, roots, functions, Greek symbols, inequalities, matrices, piecewise expressions, vectors, limits, sums, products, derivatives and partial derivatives, differentials, integrals (definite, indefinite, line, surface, and multiple where notation requires), and common geometry notation (#7).

**STU-013** Symbolic CAS behavior (exact solving, factoring, simplification, symbolic differentiation/integration) MUST NOT ship (#7, #13).

### 11.3 Graph objects

**STU-020** One Graph object MUST contain a named expression list; each expression selects Cartesian `y=f(x)`, polar `r=f(theta)`, or parametric `(x(t), y(t))` mode plus domain, visibility, and style (#7, #13).

**STU-021** Expressions MUST share Graph-level viewport, axes, and grid settings, with Teacher-controlled styling (#7, #13).

**STU-022** Studio Graph editing MUST be organized into Expressions, View, Analyze, and Sliders sections (#13).

**STU-023** Live exploration MUST provide point tracing, displayed coordinates, roots, intersections, extrema, generated value tables, and adjustable numeric parameter sliders with immediate redraw over the same semantic model and live Board preview (#7, #13).

**STU-024** Implicit relations, inequality regions, 3D graphing, differential-equation fields, and symbolic explanations MUST NOT ship (#7).

### 11.4 Geometry and teaching aids

**STU-030** Precise geometry MUST support lines, rays, segments, angles, circles, arcs, polygons, coordinate points, and dimensions/labels, with snapping, constrained construction, and measured lengths/angles that update as objects move (#7).

**STU-031** Points MUST be placed and geometry constructed and manipulated **directly on the Board** with live snapping and measurement; the Studio MUST reflect those spatial changes and MUST own construction type, exact numeric values, constraints, snapping, labels, and measurement options (#13).

**STU-032** Configurable number lines, value/input-output tables, coordinate-plane backgrounds, and graph-paper/grid regions MUST be provided through object-specific Studio forms with common presets (#7, #13).

**STU-033** After insertion, all aids MUST be ordinary semantic Board objects. Tables MUST support direct in-cell editing; number-line points/intervals and coordinate-plane bounds MUST support direct Board manipulation (#13).

**STU-034** A theorem-aware dynamic-geometry system, proof tooling, and construction scripting MUST NOT ship (#7).

### 11.5 Calculator

**STU-040** A compact scientific calculator MUST be another mode of the same transient/pinnable Studio, covering arithmetic, powers, roots, logarithms, trigonometry, and constants (#7, #13).

**STU-041** The calculator MUST provide local expression history and actions to insert a result as editable Math, insert both expression and result, or send an expression to a selected or new Graph (#7, #13).

**STU-042** Calculator history MUST be Board-local and MUST persist with the Board (#13, #14).

### 11.6 Structured-object accessibility

**STU-050** Math, Graph, geometry, and aid models MUST generate semantic default descriptions: spoken Math, graph expression/viewport summaries, and geometry relationship summaries (#8, #13).

**STU-051** An Accessibility section in the Studio MUST let the Teacher inspect and edit those descriptions (#13).

**STU-052** Missing or ambiguous descriptions MUST be flagged in the Studio and again in the pre-export accessibility panel (#10, #13).

---

## 12. Import

**IMP-001** Eulearn MUST import PNG, JPEG/JPG, BMP, GIF (first frame), WebP, and SVG through file import, clipboard, and drag/drop, embedding them as self-contained Board content and preserving transparency where supported (#7).

**IMP-002** Common images MUST preserve aspect ratio and physical-size metadata when reliable, otherwise assume 96 CSS pixels per inch. Initial displayed bounds MUST fit the visible viewport with margins while the original source data is embedded unchanged (#8, #10).

**IMP-003** PDF file import and drag/drop MUST open a dialog with PDF page thumbnails and page/range selection (#7, #10).

**IMP-004** Each selected PDF page MUST become one embedded, resolution-independent PDF Page object. Original page contents MUST NOT be ungrouped or converted into editable Eulearn text or elements (#10).

**IMP-005** A PDF Page object MUST be selectable, movable, proportionally resizable, rotatable, croppable, lockable, and copyable, and Board ink and other objects MUST be placeable above it (#10).

**IMP-006** Password-protected PDFs MUST prompt for a password in memory only when needed. Eulearn MUST honor PDF copy/print permissions, MUST NOT store the password in the Board or logs, and MUST explain unavailable operations (#10, #15).

**IMP-007** Malformed or unsupported PDFs MUST produce a specific error and leave the Board unchanged (#10).

**IMP-008** Every selected page or image MUST be validated and rendered in preview before insertion. Import MUST be atomic: if any selected item fails or the operation is cancelled, nothing is inserted and the current Board and selection remain unchanged (#10).

**IMP-009** After selection, the cursor MUST display the arranged group bounds, and one click MUST place that group's top-left at the chosen Board point (#10).

**IMP-010** Imported pages MUST initially remain selected as one temporary group for collective move/scale; each page MUST remain an independent object afterward (#10).

**IMP-011** An optional `Snap pages to Board Page Guides` MUST place the first imported page at the nearest guide's top-left and map later pages to consecutive page tiles in row-major order, each scaled uniformly to fit its tile while preserving aspect ratio. Preview MUST show the exact result and free placement MUST remain available (#10).

**IMP-012** Editing or extracting original PDF elements, text, or images MUST NOT ship. PowerPoint, Word, OneNote, Microsoft Whiteboard, and other proprietary-format import MUST NOT ship (#7, #10).

---

## 13. PDF export, printing, and sharing

### 13.1 Pagination

**OUT-001** `All Occupied Pages` MUST export every Page Guide tile intersecting visible printable Board content, enumerated row-major from the top-leftmost occupied tile (#5, #10).

**OUT-002** Empty outer tiles MUST be omitted; intentionally empty interior tiles between occupied pages MUST be preserved (#10).

**OUT-003** Additional export scopes MUST be `Current View`, `Selected Objects`, and an explicit page range (#10).

**OUT-004** Objects crossing Page Guides MUST be clipped exactly to each page tile and continue naturally on adjacent pages. Eulearn MUST NOT silently move, scale, or duplicate them, and MUST NOT offer an `Avoid splitting objects` option (#10).

### 13.2 Shared preview pipeline

**OUT-010** `Save PDF` and direct `Print` MUST use the same page model and renderer; direct Print MUST invoke the Windows print dialog only after preview, and the resulting page content MUST match PDF export (#10).

**OUT-011** Preview MUST be required and MUST show page numbers, page size/orientation, clipping at every boundary, occupied and blank pages, margins, selection scope, background, and scaling, supporting zoom and page navigation without changing Board content (#10).

**OUT-012** Export/Print MUST become available only after preview renders successfully (#10).

**OUT-013** Board Setup MUST store default page size, orientation, and Page Guide layout. Preview MAY temporarily override size, orientation, and margins; the Board MUST change only through an explicit `Apply to Board` action (#10).

**OUT-014** Page presets MUST include US Letter and A4 plus custom dimensions (#10).

**OUT-015** Print scaling MUST default to 100% physical scale matching Page Guides, with `Fit to Printable Area` and custom percentage options. Preview MUST show printer non-printable margins and the effective scale, and Eulearn MUST NOT silently shrink content (#10).

### 13.3 Output fidelity and accessibility

**OUT-020** Output MUST contain all visible Board content and MUST use vector/semantic rendering where possible, rendering from source geometry rather than canvas screenshots (#8, #10).

**OUT-021** Fonts or standards-safe subsets MUST be embedded, text MUST remain selectable where feasible, Math and structured objects MUST preserve accessible alternatives, and raster sources MUST render at sufficient source-limited resolution (#10).

**OUT-022** Selection outlines/handles, the Origin marker, Page Guide overlays, ribbon and UI chrome, and save indicators MUST be omitted from output. Locked content MUST export normally (#10).

**OUT-023** Unpainted Board areas MUST default to opaque white; PDF export MAY choose a transparent background where supported. Explicit Board grid/background objects MUST export; the editor's ambient grid MUST NOT (#10).

**OUT-024** PDF output MUST be tagged with document language, semantic text/Math alternatives, object descriptions, and deterministic reading order (#8, #10).

**OUT-025** A pre-export accessibility panel MUST expose missing descriptions and reading order for correction. Export with unresolved items MUST be allowed only after explicit warnings (#10).

### 13.4 Filesystem export and long operations

**OUT-030** `Save PDF` MUST use explicit Save As to any filesystem location, suggest `<Board name>.pdf`, remember the last export folder per Board, and confirm overwrite (#10).

**OUT-031** Export MUST write through an atomic temporary sibling file. Eulearn MUST NOT continuously regenerate a PDF during Board autosave (#10).

**OUT-032** Large import/export operations MUST run without blocking Board interaction, MUST show determinate page/item progress and `Cancel`, MUST produce no partial Board insertion or destination file on failure or cancellation, and MUST remove temporary files (#10, #15).

### 13.5 Sharing and compatibility

**OUT-040** Editable material MUST be shared as the self-contained native `.eulearn`/`.eulearnt` file through the filesystem, email, an LMS, OneDrive, Google Drive, or another existing service. No share package, Eulearn account, or upload may be required (#10).

**OUT-041** A received native file MUST open in place outside the Library and offer `Add to Library` (#6, #10).

**OUT-042** Native files MUST carry a format version and required-feature manifest. Older Eulearn versions MUST refuse destructive overwrite, explain unsupported features, and provide read-only rendering where possible. Newer versions MUST preserve unknown optional data through round-trip where possible (#10, #14).

**OUT-043** Tagged PDF MUST be the universal read/print exchange format (#10).

---

## 14. Native format and persistence protocol

### 14.1 Package structure

**FMT-001** Boards MUST use the `.eulearn` extension and Templates the `.eulearnt` extension. Both MUST use the same package schema and persistence semantics, with the manifest declaring item type (#14).

**FMT-002** Creating a Board from a Template MUST write a new independent identity and generation (#14).

**FMT-003** The package MUST be ZIP-compatible and self-contained, holding a versioned JSON manifest, normalized semantic scene data, embedded assets, and integrity metadata (#14).

**FMT-004** Textual JSON/SVG entries MUST use Deflate; already-compressed PNG/JPEG/WebP/PDF/font assets MUST be stored without recompression (#14).

**FMT-005** Entry ordering and package metadata MUST be normalized so output is deterministic and sync deltas are efficient where providers support them (#14).

### 14.2 Semantic scene

**FMT-010** Every object and group MUST carry a stable UUID, explicit domain type, logical bounds and affine transform, z-order key, lock/visibility state, accessibility metadata, and a type-specific semantic payload (#14).

**FMT-011** Groups MUST reference child IDs. The persisted model MUST NOT contain React state or MathLive/JSXGraph-private structures (#12, #14).

**FMT-012** Renderers MAY use floating point internally but MUST preserve serialized geometry without cumulative resampling (#14).

**FMT-013** Native ink, text, Math, Graphs, geometry, shapes, and guides MUST be stored as resolution-independent semantic/vector information and rerendered at each zoom (#8).

**FMT-014** Original raster image data MUST be preserved. Source pixels MAY become visible when enlarged, but Eulearn MUST introduce no additional destructive degradation (#8).

### 14.3 Embedded assets

**FMT-020** Original images, PDF data/pages, fonts/resources, and other assets MUST be stored once under content-hash paths with MIME type and provenance metadata (#14).

**FMT-021** Objects MUST reference asset hashes plus crop/render settings, and identical content MUST be deduplicated within the package (#14).

**FMT-022** Required font subsets/resources MUST be embedded when licensing permits; otherwise the object MUST record a standards-safe fallback (#14).

**FMT-023** External filesystem links MUST NOT be authoritative Board content (#14).

### 14.4 Versioning and integrity

**FMT-030** The manifest MUST declare semantic `formatVersion`, `minimumReaderVersion`, and required features (#14).

**FMT-031** Unsupported required features MUST force safe read-only rendering where possible. Breaking changes MUST migrate to a newly written package and retain the original until successful completion (#14).

**FMT-032** Readers MUST preserve unknown optional JSON fields and unreferenced entries byte-for-byte where practical (#14).

**FMT-033** If an edit would invalidate unknown object data, Eulearn MUST require an explicitly warned downgraded `Save As` copy stating what will be lost, and MUST NOT silently strip data (#14).

**FMT-034** The manifest MUST record size and SHA-256 for each required entry plus a package generation ID. Structure and required entries MUST be validated before editing; large assets MAY validate lazily before first use (#14, #15).

**FMT-035** Corruption MUST open a read-only recovery/report surface where possible and MUST NEVER cause overwrite of the damaged original (#14).

### 14.5 Portable versus machine-local state

**FMT-040** The package MUST store portable Board state: extent/page settings, Board Origin and guide visibility, last Board viewport, semantic object descriptions, calculator history, and all content state (#14).

**FMT-041** Window geometry, ribbon customization, shortcut profiles, recent files, registered Library roots, and machine-specific paths MUST stay outside the package in user/machine state (#14).

**FMT-042** The last export folder MUST be user-local metadata keyed by Board identity, not shared package data (#10, #14).

---

## 15. Durability, autosave, recovery, and concurrency

**REC-001** Under normal writable local-storage conditions, a crash or power interruption MUST lose at most one second of accepted input (#8, #14).

**REC-002** The active scene MUST be held in memory, and every accepted semantic command MUST be flushed within one second to an append-only recovery journal in user-local app data (#14).

**REC-003** Journal entries MUST be versioned commands with IDs, timestamps, affected stable object IDs, and compact payloads. High-frequency ink input MUST coalesce into committed stroke commands (#14).

**REC-004** Periodic scene checkpoints MUST bound replay time. The durable journal is for recovery; in-memory session command history implements Undo/Redo (#8, #14).

**REC-005** Compaction MUST write a complete temporary sibling package, flush it, validate it, and atomically replace the native file. ZIP entries MUST NEVER be edited in place and a partial package MUST NEVER be exposed (#14).

**REC-006** Journals MUST use ordinary Windows user-only ACLs, no Eulearn encryption, and opaque Board IDs rather than filenames, and MUST NEVER live beside or sync with the Board (#14, #15).

**REC-007** A journal MUST be deleted after verified compaction and clean close. Crash or save-failure journals MUST remain until recovery is applied or explicitly discarded (#14).

**REC-010** Autosave failures MUST preserve unsaved edits in memory and recovery storage where possible and MUST keep editing available (#8).

**REC-011** A save failure MUST show a high-contrast persistent banner with cause and last successful save time, a red `Not saved` badge beside the Board name, an app/taskbar warning mark, and a one-time audible alert that MAY be disabled. The banner MUST offer `Retry` and `Save As`/`Save Copy Elsewhere` (#8).

**REC-012** Closing a Board or Eulearn with unsaved edits MUST offer `Save As` first, `Keep Open`, or an explicitly destructive `Discard Unsaved Changes`. Ordinary close MUST NEVER silently discard (#8).

**REC-013** After abnormal termination, launch MUST show the Library with a prominent Recovered Board card containing Board name, last durable save time, and recovery timestamp (#8, #9).

**REC-014** Recovery actions MUST be `Open Recovered`, `Open Last Saved`, `Save Recovered As`, and `Discard Recovery`. Eulearn MUST NEVER auto-project recovered content or overwrite the native Board before the Teacher chooses (#9).

**REC-020** The first Eulearn editor MUST hold a lightweight user-local edit lease keyed by stable file identity and MUST NOT hold the Board file open (#14).

**REC-021** Later Eulearn opens MUST be read-only and MUST offer `Open Independent Copy`. Stale leases MUST be resolved through crash recovery (#14).

**REC-022** Eulearn MUST track stable file identity and the last-read content hash, and MUST follow a pure external rename when identity is preserved (#14).

**REC-023** If external content replaces or changes the open file, Eulearn MUST stop replacing that path and enter the standard prominent `Not Saved` state, offering `Save As`, `Keep Open`, or explicit discard on close. Eulearn MUST NOT attempt a semantic merge (#14).

**REC-030** Board files MAY live in folders managed by OneDrive, Google Drive, or similar tools. Eulearn MUST use short-lived handles and atomic complete-file/same-folder replacement where supported, MUST NEVER expose a partially written Board, and MUST surface temporary sharing/lock failures as save failures (#8, #14).

**REC-031** The storage provider owns external conflict detection and resolution. A provider-created conflict copy MUST be treated as an ordinary separate Board file (#8, #14).

**REC-032** Eulearn MUST NOT implement a cross-session version-history browser in the initial product (#8).

---

## 16. Quality: rendering, capacity, offline, logging

**QUA-001** Required zoom range MUST be 1% through 6400% without geometric or text-quality loss (#8).

**QUA-002** A Board with 10,000 objects or 100,000 ink points MUST remain functionally usable (#8).

**QUA-003** On the same supported Windows device and representative Board, launch, Board open, ink, pan/zoom, selection, and display-change behavior MUST be no less responsive than the current Microsoft Whiteboard baseline and MUST exhibit no classroom-disrupting stalls or input loss. Measured results MUST be recorded; no universal latency, frame-rate, startup, or display-recovery time is promised (#8, #18).

**QUA-004** PDF and print output MUST render from source geometry, never from canvas screenshots (#8).

**OFF-001** After installation, every required capability MUST work with networking disabled: Board and Template lifecycle operations, editing and recovery, content insertion, Math, Graph, geometry, calculator, ink recognition, image/PDF import, print/PDF export, settings, and teaching help (#8).

**OFF-002** Every executable UI resource, font, Math/Graph library, parser dependency, and runtime asset MUST be bundled locally (#12, #15).

**LOG-001** Eulearn MUST plan no remote telemetry transmission. The logging design MUST be standards-based and OpenTelemetry-compatible so future integrations remain possible, with no exporter enabled initially (#8).

**LOG-002** Full local logging MUST be implemented from the beginning with configurable verbosity. Debug and Trace MAY include complete Board content, paths, imported-content details, and user input; no safeguards beyond selecting the level are required, and logs MUST remain local unless the user manually shares them (#8, #15).

**LOG-003** Board files MUST NOT be encrypted by Eulearn. Protection relies on Windows account permissions, device/disk controls such as BitLocker, and the chosen storage provider. Eulearn MUST store no credentials or unnecessary personal metadata in Boards (#8).

---

## 17. Accessibility

**ACC-001** Applicable UI and exported content MUST meet WCAG 2.2 AA (#8).

**ACC-002** All non-drawing actions MUST be keyboard operable (#8).

**ACC-003** Controls MUST expose names, roles, states, and actions through Windows accessibility APIs (#8).

**ACC-004** Focus MUST be visible, the UI MUST remain usable at 200% scaling, and high-contrast modes MUST work (#8).

**ACC-005** Meaning MUST NEVER depend on color alone (#8).

**ACC-006** Structured Math, Graph, geometry, table, number-line, and other semantic objects MUST expose accessible labels or descriptions. Freehand drawing itself MAY remain pointer-based (#8).

**ACC-007** Every command exposed by a button, icon, or menu, plus tool properties such as color, width, pen style, zoom, and object ordering, MUST have a discoverable command identity and MUST be assignable one or more user-defined shortcuts (#8).

**ACC-008** Shortcut assignment MUST detect conflicts, reserve essential OS/accessibility combinations, support reset to defaults, and support import/export of shortcut profiles (#8).

**ACC-009** Narrator MUST be able to navigate the semantic Board model including selection, locked groups, and structured objects (#8, #16).

---

## 18. Security baseline (minimal, not enterprise)

The baseline is deliberately ordinary desktop hygiene. It closes the security question left open by #8; the deferrals below are **not** blockers (#15).

**SEC-001** Application code, libraries, fonts, and runtime assets MUST be bundled locally, and Board content MUST NEVER supply executable application code (#15).

**SEC-002** `.eulearn`, `.eulearnt`, PDF, image, SVG, clipboard, drag/drop, and host-message data MUST be treated as untrusted: declared schemas, types, sizes, package paths, and required hashes MUST be validated before use (#15).

**SEC-003** Imported SVG, HTML fragments, PDF, and native-file content MUST be inert. Eulearn MUST NOT execute scripts, macros, event handlers, embedded HTML, document JavaScript, or automatic launch/open actions (#15).

**SEC-004** The WebView MUST call only a fixed set of typed native operations. The host MUST validate operation type and MUST restrict filesystem work to the active Board, registered Library roots, explicit user selections, recovery storage, and export destinations (#12, #15).

**SEC-005** PDF passwords and any future credentials, tokens, keys, cookies, or signing material MUST NEVER be logged, journaled, persisted in Board files, or included in diagnostics (#15).

**SEC-006** Temporary conversion, preview, and export files MUST use ordinary per-user application/temp storage and MUST be cleaned after success, cancellation, or at a later startup cleanup. Final exports and package compaction MUST still use atomic sibling writes (#15).

**SEC-007** NuGet/npm/native dependency versions MUST be pinned with committed lockfiles or equivalent reproducible version declarations, and licenses MUST be reviewed before adoption (#15).

**SEC-008** Release packages and update packages MUST be signed and verified through the selected MSIX/WebView2 deployment path (#15).

**SEC-009** Development builds MUST be visibly distinct from signed release builds (#15).

**Deferred (non-blocking):** custom Windows sandbox/AppContainer engineering beyond natural WebView2 and out-of-process boundaries; elaborate worker CPU/memory/rate-limit frameworks; mandatory SBOM formats, provenance attestations, reproducible-build proofs, hardware-backed signing, severity-based release policy; formal vulnerability-response SLAs, coordinated disclosure, kill switches, dedicated security operations; penetration testing, school compliance certification, app-managed encryption, remote telemetry security, and defenses against an attacker controlling the Teacher's Windows account or device administrator (#15).

---

## 19. Architecture

### 19.1 Runtime and UI

**ARC-001** The host MUST be a .NET 10 Windows application targeting serviced Windows 11 x64 and ARM64 (#12).

**ARC-002** The embedded runtime MUST be Evergreen WebView2, with a documented/offline runtime installation path for disconnected deployments (#12).

**ARC-003** The UI MUST be React + TypeScript + Vite for Library, ribbon, Board chrome, Studio, previews, settings, and other product surfaces (#12).

**ARC-004** Board state, commands, geometry, serialization-facing domain objects, and renderer contracts MUST be framework-independent TypeScript. React MUST NOT own one component per Board object (#12).

**ARC-005** Native surfaces MUST be limited to OS-owned or trust-boundary functions: file/folder and Print dialogs, window/display lifecycle, installer/update, fatal recovery, and accessibility bridging where Chromium is insufficient (#12).

### 19.2 Board rendering and input

**ARC-010** The Board MUST be a retained semantic scene graph with a spatial index, immutable snapshots/targeted deltas, viewport culling, batching, and layered rendering (#12).

**ARC-011** DOM MUST be used for accessible/editable application UI and semantic overlays; SVG SHOULD be used where semantic/vector text, Math, and geometry are advantageous; Canvas 2D MUST handle high-volume ink and batched primitives (#12).

**ARC-012** Chromium's delegated Ink API MUST provide wet-ink presentation on Windows, with dry ink committed to the semantic scene (#12, #18).

**ARC-013** WebGPU MUST NOT be a baseline dependency. It MAY be introduced behind the renderer interface only if milestone-zero product-shaped profiling proves Canvas/SVG insufficient (#12).

### 19.3 Host, worker, and protocol boundaries

**ARC-020** The .NET host MUST own native Board-file reads/writes, autosave scheduling, durable recovery checkpoints, atomic same-folder replacement, file watching, and native dialogs. It MUST NOT own any Recycle Bin, Delete, or Archive operation (#12 as amended by #14).

**ARC-021** WebView2 renderer processes MUST host the TypeScript application (#12).

**ARC-022** Least-privileged worker processes MUST handle PDF parsing/rendering, image/document import decoding, tagged PDF generation, and print preparation. A worker failure MUST fail exactly one operation without terminating or corrupting the open Board (#12).

**ARC-023** The TypeScript app MUST call native capabilities through a versioned, typed, asynchronous message protocol generated from shared schemas, with request IDs, progress, cancellation, and explicit typed errors (#12).

**ARC-024** Native operations MUST be capability-allowlisted. No arbitrary shell execution, reflection, unrestricted filesystem API, or generic localhost server may be exposed to web content (#12, #15).

**ARC-025** The application MUST use a fixed local WebView2 origin and a restrictive Content Security Policy, and MUST NOT be able to load remote code or CDN resources. Explicit online help navigation and update infrastructure occur outside Board content (#12, #15).

### 19.4 Math, graphing, geometry, PDF, and print stacks

**ARC-030** MathLive and JSXGraph MUST be bundled locally, using JSXGraph's MIT licensing option, behind Eulearn-owned semantic adapters with coverage tests (#12).

**ARC-031** Board files MUST persist Eulearn domain objects only, so the libraries can be replaced without changing the native format's domain contract (#12, #14).

**ARC-032** The calculator and semantic accessibility descriptions MUST be Eulearn-owned services/models shared by the Studio and the export pipeline (#12, #13).

**ARC-033** Permission-aware PDF import/rendering MUST use PDFium in an isolated worker, with x64 and ARM64 dependency qualification (#12).

**ARC-034** Export MUST build a dedicated semantic print document from the Board model rather than printing the interactive canvas (#12, #18).

**ARC-035** Chromium/Skia tagged output MUST be used only after PDF/UA validation; on failure the export adapter MUST switch to a direct Skia structure-tree implementation (#12, #18).

**ARC-036** Direct Print MUST consume the same semantic page model and rendering pipeline as `Save PDF` (#10, #12).

### 19.5 Extension seams and cross-platform posture

**ARC-040** Internal versioned interfaces MUST exist for renderers, Math/Graph adapters, importers/exporters, persistence, print, and host capabilities (#12).

**ARC-041** No third-party .NET or JavaScript plugin API may ship initially. Signed/sandboxed extensions MAY be reconsidered only after the native format and security contracts stabilize (#12).

**ARC-042** The initial host is deliberately Windows-specific. The framework-independent TypeScript Board core and typed host protocol MUST preserve the option of a future Avalonia or other host without imposing current cross-platform cost (#12).

---

## 20. Deployment, updates, and help

**DEP-001** The primary package MUST be signed MSIX supporting per-user installation without administrator rights, offline installation, and silent managed deployment (#8, #12).

**DEP-002** Updates MUST be signed, optional, and deferrable, and MUST NEVER install or restart during teaching (#8, #12).

**DEP-003** Eulearn MUST verify WebView2 compatibility before opening a Board (#12).

**DEP-004** Uninstall MUST leave Teacher Board files untouched (#8).

**DEP-005** Help MUST be authored once as static browser-viewable content suitable for both local bundling and online hosting such as GitHub Pages (#8).

**DEP-006** Eulearn MUST ship an offline copy of help. Help actions MUST open the relevant local page by default, with an option to use the current online version (#8).

---

## 21. Release slices, gates, and fallbacks

Slice detail, prerequisites, exclusions, and per-slice acceptance evidence live in `docs/spec/implementation-roadmap.md` (#16). Slices are vertical: each ends in something the Teacher can do.

| Slice | Outcome | Gate |
|---|---|---|
| **S0** Repository and application skeleton | Launchable branded shell, typed protocol v0, CSP/local origin, pinned lockfiles, x64/ARM64 CI | — |
| **S1** Minimal usable ink whiteboard on a real file | Pen/highlighter/erasers, infinite pan/zoom, undo, minimal `.eulearn` write/reopen with atomic replacement. First dogfood build | **G1 Ink** |
| **S2** Objects, manipulation, text, locking | Selection, constrained transforms, grouping, Lock/Unlock, in-place text, shapes, ink-to-shape, contextual ribbon, Board Setup, Origin/Page Guides, command/shortcut registry | **G2 Capacity** |
| **S3** Board Library, native format completion, recovery | Multi-root Library, Templates, duplication, insert-from-Board, one-second journal durability, compaction, edit lease, external-change handling, recovery card, Not Saved states | **G5 File safety** |
| **S4** Import, PDF export, printing | Image/PDF import as self-contained objects, atomic previewed placement, page scopes and exact clipping, shared preview pipeline, pre-export accessibility panel, isolated workers | **G4 Tagged PDF**, **G7 ARM64 (PDFium)** |
| **S5** Structured Math, Graph, geometry Studio | Transient/pinnable Studio, Math/Graph/geometry/aids/calculator, draft-Apply semantics, editable generated descriptions | **G8 Math libraries**, G4 extension |
| **S6** Accessibility, capacity, quality hardening | Full keyboard operability, WCAG 2.2 AA, Narrator over the real Board model, zoom/capacity verification, help authoring | **G3 Accessibility**, G2 re-run |
| **S7** Release packaging and servicing | Signed MSIX per-user offline install, silent managed deployment, deferrable updates, uninstall preservation, bundled help | **G6 Deployment**, **G7 ARM64 complete** |

**REL-001** R1, the first daily-usable release, MUST be S0–S4 with G1, G2, G4, and G5 passed or their fallbacks adopted, plus a pulled-forward packaging subset (signed per-user install; uninstall preserves Board files) (#16).

**REL-002** R1 MUST be measured against the #5 daily-use threshold: prepare in minutes, teach a full period fluidly, survive network loss and app/device restart within the one-second bound, resume the same Board in seconds, adapt material for another class without overwriting the source or Template, and produce a readable printable tagged PDF (#5, #16).

**REL-003** R1 explicitly excludes the Math/Graph/geometry Studio (S5), full WCAG 2.2 AA sign-off (S6), and managed deployment and update servicing (S7). R1 is a sponsor-classroom release, not general availability (#16).

**REL-004** R2, general release, MUST add S5–S7 with G3, G6, G7, and G8 passed, satisfying the #7 completion boundary and the #8 release gates (#16).

### Milestone-zero gates and subsystem fallbacks

**REL-010** Each gate MUST block only its own subsystem slice and MUST trigger the documented fallback rather than a product-requirement relaxation (#12, #16, #18).

| Gate | Evidence required | Gated slice | Fallback |
|---|---|---|---|
| **G1 Ink** | Instrumented physical-ink comparison vs Microsoft Whiteboard on the same device; delegated ink working | S1 | Native WinRT wet-ink surface composited with WebView2 |
| **G2 Capacity** | Product-shaped spatial index, culling, batching, selection, memory, launch/open at 10,000 objects and 100,000 ink points at 1%, 100%, 6400% | S2 (re-run S6) | Skia or Direct2D renderer adapter behind the renderer interface |
| **G3 Accessibility** | Narrator + Accessibility Insights over the real semantic Board model; WCAG 2.2 AA | S6 (smoke check S2) | Native UI Automation overlay/peer bridge |
| **G4 Tagged PDF** | PDF/UA conformance including clipped cross-page objects and reading order; extended to Math/Graph/geometry in S5 | S4 (extended S5) | Direct Skia structure-tree PDF adapter |
| **G5 File safety** | OneDrive + Google Drive atomic replacement, short-lived handles, transient-lock surfacing, conflict copies, external rename reflection | S3 | Persistence adapter change within the #14 protocol |
| **G6 Deployment** | Signed per-user offline installer, silent managed deployment, update deferral, uninstall preserves Board files | S7 | Packaging path change; product requirements unchanged |
| **G7 ARM64** | Every native dependency built **and physically executed** on ARM64 | S4 (PDFium), completed S7 | Replace the failing native dependency |
| **G8 Math libraries** | MathLive notation coverage for the STU-012 scope; JSXGraph constrained geometry and live measurement | S5 | Alternative library behind the same Eulearn semantic adapter |

**REL-011** The Recycle Bin item inside #18's file-safety evidence is void. G5 MUST cover atomic replacement, short-lived handles, transient locks, conflict copies, and external rename reflection only (#14, #16).

**REL-012** A full WPF or Win32 architecture pivot MUST be considered only if multiple core WebView subsystems fail (#12, #16).

---

## 22. Risks

| # | Risk | Evidence | Mitigation |
|---|---|---|---|
| R1 | Chromium's automatic tagged PDF is not PDF/UA conformant — the probe treated the canvas as a single `Figure` and reported structure warnings | `docs/research/architecture-benchmark-results.md`, #18 | ARC-034 semantic print document; G4 gate; direct Skia structure-tree fallback (ARC-035) |
| R2 | Ink responsiveness evidence is qualitative ("very responsive" vs "slight lag"), not instrumented | #18 | G1 instrumented comparison; WinRT wet-ink fallback |
| R3 | Canvas/SVG may not sustain 10,000 objects / 100,000 ink points in product shape; probe scenes were flat and diagnostic | #18, QUA-002 | G2 product-shaped benchmark; Skia/Direct2D renderer adapter |
| R4 | No ARM64 hardware was available during probing; PDFium and other native dependencies are unproven there | #18 | G7 build **and physical execution**; replace failing dependency |
| R5 | Sync-provider coexistence (OneDrive/Google Drive) was never exercised; transient locks can surface as save failures | #18, REC-030 | G5 fixture testing; short-lived handles; atomic replacement; provider owns conflicts |
| R6 | MathLive/JSXGraph may not cover the full STU-012 notation and constrained-geometry scope | #11, #18 | G8 coverage tests behind Eulearn adapters; ARC-031 keeps the format library-agnostic |
| R7 | The one-second durability bound depends on journal flush behavior under real Windows I/O and antivirus interference | #14 | Kill/power-interruption harness in S3; checkpoint cadence tuning |
| R8 | Removing Archive/Delete puts all organization in File Explorer; Teachers may expect in-app removal | #14 (sponsor-confirmed) | `Show in File Explorer`, external-change observation, help documentation |
| R9 | Accessibility of a canvas-based Board through Chromium is unvalidated beyond proxy controls | #18, ACC-009 | G3 Narrator/Accessibility Insights gate; native UIA bridge fallback |
| R10 | Evergreen WebView2 servicing may change behavior between releases | #12 | DEP-003 compatibility verification before opening a Board; pinned app dependencies (SEC-007) |

---

## 23. Acceptance criteria for the initial product

**ACP-001** A Teacher MUST be able to complete the full prepare–teach–recover–reuse loop (WF-001–WF-006) on a supported Windows 11 device with networking disabled (#5, #7, #8).

**ACP-002** A crash or power interruption MUST lose at most one second of accepted input, verified by a kill/power-interruption harness; journal replay MUST restore the recovered scene and reopening MUST start a fresh undo session (#8, #14, #16).

**ACP-003** Repeated saves of an unchanged Board MUST be byte-deterministic, all declared hashes MUST validate on reopen, and a partially written package MUST never be observable (#14, #16).

**ACP-004** A corrupt package MUST open a read-only recovery surface and MUST NEVER overwrite the damaged original; a package written by a newer feature set MUST round-trip unknown data unchanged (#14).

**ACP-005** Selection, manipulation, and text MUST remain usable at 10,000 objects and 100,000 ink points across 1%, 100%, and 6400% zoom (#8, #18).

**ACP-006** Display connect/disconnect/resize/rotate/DPI change MUST preserve Board coordinates and viewport with no restart (#8, #9).

**ACP-007** Direct Print output MUST match the previewed pages exactly, and exported PDF MUST pass PDF/UA validation for text, images, page-clipped objects, and (from S5) Math, Graph, and geometry (#10, #16).

**ACP-008** Killing a worker mid-operation MUST fail only that operation; malformed input MUST leave the Board and the destination untouched; PDF passwords MUST never appear in logs, journals, packages, or diagnostics (#12, #15, #16).

**ACP-009** Every non-drawing command MUST be keyboard operable and rebindable, and a WCAG 2.2 AA audit MUST be recorded with defects closed (#8, #16).

**ACP-010** Comparative responsiveness against Microsoft Whiteboard on the same device MUST be recorded for launch, Board open, ink, pan/zoom, selection, and display change, with no classroom-disrupting stall or input loss (#8, #18).

**ACP-011** A signed per-user offline install MUST succeed without administrator rights, silent managed deployment MUST succeed, an update offered mid-session MUST neither install nor restart, and uninstall MUST leave Board files untouched (#8, #12, #16).

**ACP-012** Structured objects MUST round-trip through save/open without loss and MUST contain no library-private structures; `Apply` MUST produce exactly one undo step and one autosave (#13, #14, #16).

**ACP-013** The application MUST load no remote resource and MUST report zero CSP violations at runtime (#12, #15, #16).

**ACP-014** Every shipped build MUST be free of an Archive state or view, a Delete command, a Trash, and any Recycle Bin operation (#14, #16).

---

## 24. Traceability

Requirements-to-source-decision and requirements-to-slice in one table. Sources are decision issue numbers; local evidence is `docs/research/runtime-architecture-options.md` (#11), `docs/research/architecture-benchmark-results.md` (#18), `docs/spec/implementation-roadmap.md` (#16), and `brand/README.md`.

| Requirement IDs | Area | Source decision | Slice |
|---|---|---|---|
| PRD-001–PRD-004 | Product goal and parity floor | #4, #5, #7, #8 | All (measured R1/R2) |
| PRD-010–PRD-016 | Non-goals and rejections | #7, #8, #9, #10, #14, #15 | All (as exclusions) |
| ENV-001–ENV-004 | Personas and environment | #3, #5, #8, #9 | All |
| WF-001–WF-006 | End-to-end workflows | #5, #6, #10, #14 | S1–S4 (R1 threshold) |
| BRD-001–BRD-002 | Extent modes and preservation | #5, #7 | S2 |
| BRD-003–BRD-006 | Board Origin and Page Guides | #5, #7, #9 | S2 |
| BRD-007–BRD-008 | Page size and Board Setup | #5, #9, #10 | S2 |
| BRD-009 | Logical coordinate system | #14 | S1 |
| BRD-010 | Free pan/zoom, no named Views | #7 | S1 |
| LIB-001–LIB-006 | Library roots, browsing, filters, Explorer | #6, #10, #14 | S3 |
| LIB-007 | No Archive/Delete/Trash/Recycle Bin | #14 (supersedes #6) | S1/S3 as exclusion |
| LIB-010–LIB-012 | Filename identity, defaults, ` (n)` rule | #6 | S3 (default name S1) |
| LIB-020–LIB-021 | Autosave status and `Ctrl+S` flush | #6, #8 | S1 (status), S3 (durable) |
| LIB-022–LIB-026 | Duplicate, Template↔Board, insert content | #6 | S3 |
| INK-001–INK-003 | Ink parity floor and input classes | #4, #7, #9 | S1 |
| INK-004–INK-007 | Latched/spring-loaded tools, navigation | #9 | S1 (input), S2 (selection) |
| INK-008 | Ink-to-shape with Undo restore | #4, #7 | S2 |
| INK-009 | No handwriting recognition | #7 | Not scheduled |
| INK-010 | Optional laser-pointer aid | #4, #9 | S2 (MAY) |
| OBJ-001–OBJ-004 | Selection, transforms, shapes | #5, #7, #9 | S2 |
| OBJ-005–OBJ-008 | Text object and in-place editing | #5, #7, #9 | S2 |
| OBJ-010–OBJ-014 | Lock/Unlock semantics; no Layers UI | #5, #7, #9 | S2 |
| OBJ-020–OBJ-023 | Undo scope, Previous View, Escape | #8, #9 | S1 (content), S2 (view) |
| UI-001–UI-006 | One-row ribbon and contextual region | #9, #13 | S0 (shell), S2 (contextual) |
| UI-007 | Tooltips and contextual `?` links | #8 | S6 |
| BRN-001–BRN-003 | Brand identity, bundled fonts, color independence | `brand/README.md`, #8, #12, #15 | S0 |
| DSP-001–DSP-002 | No projection features; display change safety | #8, #9 | S2 |
| STU-001–STU-008 | Studio model and draft/Apply semantics | #13 | S5 |
| STU-010–STU-013 | Math objects and notation scope | #7, #13 | S5 |
| STU-020–STU-024 | Graph objects and live analysis | #7, #13 | S5 |
| STU-030–STU-034 | Geometry and teaching aids | #7, #13 | S5 |
| STU-040–STU-042 | Scientific calculator | #7, #13 | S5 |
| STU-050–STU-052 | Generated editable descriptions | #8, #10, #13 | S5 |
| IMP-001–IMP-002 | Image import and sizing | #7, #8, #10 | S4 |
| IMP-003–IMP-007 | PDF page import, permissions, errors | #7, #10, #15 | S4 |
| IMP-008–IMP-011 | Atomic previewed placement and snapping | #10 | S4 |
| IMP-012 | Deferred import formats and PDF editing | #7, #10 | Not scheduled |
| OUT-001–OUT-004 | Page scopes, occupied pages, exact clipping | #5, #10 | S4 |
| OUT-010–OUT-015 | Shared preview pipeline, presets, scaling | #10 | S4 |
| OUT-020–OUT-025 | Output fidelity, tagged PDF, accessibility panel | #8, #10 | S4, extended S5 |
| OUT-030–OUT-032 | Save As, atomic temp sibling, long operations | #10, #15 | S4 |
| OUT-040–OUT-043 | Native-file and PDF sharing, compatibility | #6, #10, #14 | S3 (files), S4 (PDF) |
| FMT-001–FMT-005 | Package types, structure, determinism | #14 | S1 (minimal), S3 (complete) |
| FMT-010–FMT-014 | Semantic scene and vector fidelity | #8, #12, #14 | S1/S2, verified S6 |
| FMT-020–FMT-023 | Content-addressed embedded assets | #14 | S3 (S4 for imported assets) |
| FMT-030–FMT-035 | Versioning, unknown data, integrity, corruption | #10, #14, #15 | S3 |
| FMT-040–FMT-042 | Portable vs machine-local state | #10, #14 | S3 |
| REC-001–REC-007 | One-second journal, checkpoints, compaction | #8, #14, #15 | S3 |
| REC-010–REC-014 | Save-failure and recovery experience | #8, #9 | S3 |
| REC-020–REC-023 | Edit lease and external-change handling | #14 | S3 |
| REC-030–REC-032 | Sync-provider coexistence; no version browser | #8, #14 | S3 |
| QUA-001–QUA-004 | Zoom range, capacity, comparative responsiveness | #8, #18 | S2 gate, S6 verify |
| OFF-001–OFF-002 | Offline operation and local bundling | #7, #8, #12, #15 | Every slice (S0 baseline) |
| LOG-001–LOG-003 | Logging, no telemetry, no app encryption | #8, #15 | S0 (baseline), S6 (settings) |
| ACC-001–ACC-009 | Accessibility contract | #8, #16 | S2 (registry/smoke), S6 (complete) |
| SEC-001–SEC-009 | Minimal security baseline | #15 (closes #8 open question) | S0, S1, S4, S6, S7 |
| ARC-001–ARC-005 | Runtime, WebView2, React/TS/Vite, native surfaces | #11, #12 | S0 |
| ARC-010–ARC-013 | Scene graph, DOM/SVG/Canvas, delegated ink | #12, #18 | S1, S2 |
| ARC-020–ARC-025 | Host/worker boundaries, typed protocol, CSP | #12, #14, #15 | S0 (protocol), S3 (host), S4 (workers) |
| ARC-030–ARC-036 | Math/graph libraries, PDFium, semantic print doc | #12, #18 | S4 (PDF/print), S5 (math) |
| ARC-040–ARC-042 | Extension seams, no plugins, cross-platform posture | #12 | S0 onward |
| DEP-001–DEP-004 | Packaging, updates, uninstall | #8, #12 | S7 (R1 subset earlier) |
| DEP-005–DEP-006 | Help authoring and offline bundling | #8 | S6 (authoring), S7 (bundling) |
| REL-001–REL-004 | Release definitions | #16 | R1, R2 |
| REL-010–REL-012 | Gate discipline, void Recycle Bin evidence, pivot rule | #12, #14, #16, #18 | All gates |
| ACP-001–ACP-014 | Acceptance criteria | #5–#18 | R1/R2 sign-off |

**Requirement count: 271 stable requirement IDs**, distributed as ACC 9, ACP 14, ARC 25, BRD 10, BRN 3, DEP 6, DSP 2, ENV 4, FMT 23, IMP 12, INK 10, LIB 17, LOG 3, OBJ 17, OFF 2, OUT 23, PRD 11, QUA 4, REC 19, REL 7, SEC 9, STU 28, UI 7, WF 6.

---

## 25. Non-blocking open implementation choices

Every item below is an implementation detail, not an unresolved product decision. Each is already constrained by a ratified contract, so none blocks implementation planning.

| Open choice | Constraining contract |
|---|---|
| Exact Board JSON schemas, command encoding, and object payload shapes | FMT-003, FMT-010–FMT-012, FMT-030–FMT-034 (#14) |
| Journal checkpoint cadence and compaction cadence | REC-002–REC-005 one-second bound and atomic replacement (#14) |
| ZIP implementation library and Windows file-identity API | FMT-003–FMT-005, REC-022 (#14) |
| Migration code for future `formatVersion` bumps | FMT-031–FMT-033 (#14) |
| Internal models and detailed editors for Math, Graph, geometry, table, number-line, coordinate-plane, and grid-region types | STU-010–STU-033, ARC-030–ARC-032, FMT-011 (#7, #12, #13) |
| Numerical-analysis algorithms for roots, intersections, extrema, and tables | STU-023, ARC-032 (#13) |
| Preset libraries and Studio field taxonomy | STU-032, STU-001–STU-004 (#13) |
| Final visual styling, iconography, and ribbon command taxonomy | UI-001–UI-005, BRN-001 (#9, `brand/README.md`) |
| Default keyboard shortcut bindings and constrained-manipulation modifier defaults | OBJ-002, ACC-007–ACC-008 (configurable by contract) (#8, #9) |
| Choice of PDF/UA validator tooling and the print-DOM vs direct-Skia decision point | OUT-024, ARC-034–ARC-035, gate G4 (#10, #12, #18) |
| Spatial index data structure and batching strategy | ARC-010, QUA-002, gate G2 (#12) |
| Accessibility bridging technique if Chromium proves insufficient | ARC-005, ACC-009, gate G3 fallback (#12) |
| Update distribution channel mechanics | DEP-001–DEP-003 (#8, #12) |
| Help information architecture and topic granularity | DEP-005–DEP-006, UI-007 (#8) |
| Log storage layout and verbosity switch UI | LOG-001–LOG-002 (#8, #15) |

Nothing in this list blocks implementation planning.
