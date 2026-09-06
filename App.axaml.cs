namespace SnakeGame;

// This class represents the application
// It is used to initialize the app and display the main window

public partial class App : Avalonia.Application
{
    // initialize the app (overload of Avalonia.Application.Initialize())
    public override void Initialize()
    {
        // load the app's XAML (App.axaml)
        Avalonia.Markup.Xaml.AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop)
            desktop.MainWindow = new MainWindow();

        base.OnFrameworkInitializationCompleted();
    }
}
