using System.Windows;
using System.Windows.Threading;

namespace Eulearn.ThrowawayArchitectureBenchmark;

public partial class App : Application
{
    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var benchmarkOnly = e.Args.Any(argument =>
            string.Equals(argument, "--benchmark", StringComparison.OrdinalIgnoreCase));
        var window = new MainWindow();

        if (!benchmarkOnly)
        {
            MainWindow = window;
            window.Show();
            return;
        }

        ShutdownMode = ShutdownMode.OnExplicitShutdown;
        window.ShowActivated = false;
        window.ShowInTaskbar = false;
        window.WindowStyle = WindowStyle.None;
        window.Left = -10000;
        window.Top = -10000;
        window.Show();

        await Dispatcher.InvokeAsync(
            () => { },
            DispatcherPriority.ApplicationIdle);

        try
        {
            Console.WriteLine("Eulearn throwaway WPF architecture capacity benchmark");
            Console.WriteLine($"Runtime: {Environment.Version}; OS: {Environment.OSVersion}; Architecture: {System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture}");
            Console.WriteLine("Scale\tObjects\tInkPoints\tRenderMs\tManagedMB\tPrivateMB\tResult");

            foreach (var result in await window.RunAllBenchmarksAsync())
            {
                Console.WriteLine(
                    $"{result.Scale}\t{result.VectorObjects:N0}\t{result.InkPoints:N0}\t" +
                    $"{result.RenderMilliseconds:F2}\t{result.ManagedMegabytes:F2}\t" +
                    $"{result.PrivateMegabytes:F2}\t{result.Result}");
            }
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception);
            Environment.ExitCode = 1;
        }
        finally
        {
            window.Close();
            Shutdown();
        }
    }
}
