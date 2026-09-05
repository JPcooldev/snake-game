namespace SnakeGame;

// sealed class = cannot be inherited
sealed class Program
{
    // main entry point
    // STA = Single Threaded Apartment. This is mainly for Windows compatibility.
    [System.STAThread]
    public static void Main(string[] args) =>
        Avalonia.ClassicDesktopStyleApplicationLifetimeExtensions.StartWithClassicDesktopLifetime(
            BuildAvaloniaApp(), args
        );

    // build the Avalonia app
    // - UsePlatformDetect() detects the platform and uses the appropriate Avalonia theme.
    // - WithInterFont() uses the Inter font for text.
    // - LogToTrace() logs to the console for debugging.
    // - Configure<App>() configures the app to use the App class.
    public static Avalonia.AppBuilder BuildAvaloniaApp()
        => Avalonia.LoggingExtensions.LogToTrace(
            Avalonia.AppBuilderExtension.WithInterFont(
                Avalonia.AppBuilderDesktopExtensions.UsePlatformDetect(
                    Avalonia.AppBuilder.Configure<App>()
                )
            )
        );
}
