namespace SnakeGame;

// This class represents the program
// It is used to start the app and build the Avalonia app

// sealed class = cannot be inherited
sealed class Program
{
    // main entry point
    // STA = Single Threaded Apartment. This is mainly for Windows compatibility.
    [System.STAThread]
    public static void Main(string[] args) =>
        // start the app with the classic desktop lifetime
        Avalonia.ClassicDesktopStyleApplicationLifetimeExtensions.StartWithClassicDesktopLifetime(
            BuildAvaloniaApp(), args
        );

    // build the Avalonia app
    public static Avalonia.AppBuilder BuildAvaloniaApp()
        => Avalonia.LoggingExtensions.LogToTrace(
            // use the Inter font for text
            Avalonia.AppBuilderExtension.WithInterFont(
                // detect the platform and use the appropriate Avalonia theme
                Avalonia.AppBuilderDesktopExtensions.UsePlatformDetect(
                    // Configure<App>() configures the app to use the App class (App.axaml / App.axaml.cs)
                    Avalonia.AppBuilder.Configure<App>()
                )
            )
        );
}
