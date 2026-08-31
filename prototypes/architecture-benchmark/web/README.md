# Chromium canvas benchmark prototype

Throwaway diagnostic probe for Eulearn issue #18.

Open `canvas-benchmark.html` in current Microsoft Edge. Use **Run all** to record Canvas 2D draw timings for 10,000 vector objects and 100,000 polyline points at 1%, 100%, and 6400%. Draw with pen, mouse, or touch to inspect pointer type, pressure, and tilt.

`?autorun=1` runs the synthetic draw benchmark automatically.

The probe also exposes hidden semantic proxy controls for a Math object, Graph object, and locked group so Accessibility Insights or Narrator can inspect the Chromium-to-UIA path.

## Limits

- JavaScript draw duration is not physical pen-to-photon latency.
- Headless results do not represent interactive GPU composition.
- This probe does not validate tagged PDF/UA, native file operations, Recycle Bin behavior, installer behavior, ARM64 execution, or cloud-sync coexistence.
- Results are useful only when recorded with browser version, hardware, display, and comparison conditions.
