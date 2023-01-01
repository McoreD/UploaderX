using ShareX;
using ShareX.UploadersLib;

namespace UploaderX;

public partial class App : Application
{
    public App()
	{
		InitializeComponent();

		MainPage = new AppShell();
	}
}

