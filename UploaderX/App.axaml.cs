using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using ShareX;
using ShareX.UploadersLib;
using UploaderX.ViewModels;
using UploaderX.Views;

namespace UploaderX;

public partial class App : Application
{
    internal static ApplicationConfig? Settings { get; set; }
    internal static TaskSettings? DefaultTaskSettings { get; set; }
    internal static UploadersConfig? UploadersConfig { get; set; }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(),
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
