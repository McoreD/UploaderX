using ShareX;
using ShareX.UploadersLib;

namespace UploaderX;

public partial class App : Application
{
    internal static ApplicationConfig? Settings { get; set; }
    internal static UploadersConfig? UploadersConfig { get; set; }
    internal static TaskSettings DefaultTaskSettings { get; set; }

    public App()
	{
		InitializeComponent();

		MainPage = new AppShell();
	}
}

