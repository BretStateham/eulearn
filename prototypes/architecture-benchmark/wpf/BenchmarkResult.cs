namespace Eulearn.ThrowawayArchitectureBenchmark;

public sealed record BenchmarkResult(
    string Scale,
    int VectorObjects,
    int InkPoints,
    double RenderMilliseconds,
    double ManagedMegabytes,
    double PrivateMegabytes,
    string Result);
