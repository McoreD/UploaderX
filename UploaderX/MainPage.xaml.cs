using System.Diagnostics;
using HelpersLib;
using Microsoft.Extensions.Logging;
using ShareX.HelpersLib;
using ShareX.UploadersLib;

namespace UploaderX;

public partial class MainPage : ContentPage
{
    string AppDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "UploaderX");

    private string _watchDir;
    private string _destDir;
    private string _destSubDir;

    private FileSystemWatcher _watcher;

    public delegate void UrlReceivedEventHandler(string url);
    public event UrlReceivedEventHandler UrlReceived;

    public MainPage()
    {
        InitializeComponent();

        string AppSettingsDir = Path.Combine(AppDir, "Settings");

        txtAppConfigPath.Text = Path.Combine(AppSettingsDir, "ApplicationConfig.json");
        App.Settings = ApplicationConfig.Load(txtAppConfigPath.Text);

        txtUploaderConfigPath.Text = Path.Combine(AppSettingsDir, "UploadersConfig.json");
        App.UploadersConfig = UploadersConfig.Load(txtUploaderConfigPath.Text);
        App.UploadersConfig.SupportDPAPIEncryption = false;

        DebugHelper.Init(Path.Combine(Path.Combine(AppDir, "Logs"), $"UploaderX-{DateTime.Now.ToString("yyyyMMdd")}-Log.txt"));

        _watchDir = Directory.Exists(App.Settings.CustomScreenshotsPath2) ? App.Settings.CustomScreenshotsPath2 : Path.Combine(AppDir, "Watch Folder");
        Helpers.CreateDirectoryFromDirectoryPath(_watchDir);
        txtWatchDir.Text = _watchDir;
        _destDir = _watchDir;
        _destSubDir = Path.Combine(Path.Combine(_destDir, DateTime.Now.ToString("yyyy")), DateTime.Now.ToString("yyyy-MM"));
        txtScreenshotsDir.Text = _destSubDir;
        
        DebugHelper.Logger.WriteLine("Watch Dir: " + _watchDir);
        DebugHelper.Logger.WriteLine("Destination Dir: " + _destSubDir);

        _watcher = new FileSystemWatcher();
        _watcher.Path = _watchDir;

        _watcher.NotifyFilter = NotifyFilters.FileName;
        _watcher.Created += OnCreated;
        _watcher.EnableRaisingEvents = true;

        this.UrlReceived += MainPage_UrlReceived;

    }

    private void MainPage_UrlReceived(string url)
    {
        Clipboard.Default.SetTextAsync(url);
        lblUrl.Text = url;
        wvUrl.Source = new UrlWebViewSource() { Url = url };
    }

    private void OnUrlReceived(string url)
    {
        UrlReceived?.Invoke(url);
    }

    async void OnCreated(object sender, FileSystemEventArgs e)
    {
        try
        {
            string fileName = new NameParser(NameParserType.FileName).Parse("%y%mo%d_%ra{10}") + Path.GetExtension(e.FullPath);
            string destPath = Path.Combine(_destSubDir, fileName);
            FileHelpers.CreateDirectoryFromFilePath(destPath);
            if (!Path.GetFileName(e.FullPath).StartsWith("."))
            {
                int successCount = 0;
                long previousSize = -1;

                await Helpers.WaitWhileAsync(() =>
                {
                    if (!FileHelpers.IsFileLocked(e.FullPath))
                    {
                        long currentSize = FileHelpers.GetFileSize(e.FullPath);

                        if (currentSize > 0 && currentSize == previousSize)
                        {
                            successCount++;
                        }

                        previousSize = currentSize;
                        return successCount < 4;
                    }

                    previousSize = -1;
                    return true;
                }, 250, 5000, () =>
                {
                    File.Move(e.FullPath, destPath, overwrite: true);
                }, 1000);

                if (Path.GetExtension(destPath).ToLower().Equals(".mov"))
                {
                    string ffmpegPath = Path.Combine(AppDir, "ffmpeg");
                    ShareX.MediaLib.FFmpegCLIManager ffmpeg = new ShareX.MediaLib.FFmpegCLIManager(ffmpegPath);
                    string mp4Path = Path.ChangeExtension(destPath, "mp4");
                    string args = $"-i \"{destPath}\" -c:v libx264 -preset medium -crf 23 -pix_fmt yuv420p -movflags +faststart -y \"{mp4Path}\"";
                    if (ffmpeg.Run(args))
                    {
                        destPath = mp4Path;
                    }
                }           

                WorkerTask task = new WorkerTask(destPath);
                UploadResult result = task.UploadFile();
                DebugHelper.Logger.WriteLine(result.URL);
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    OnUrlReceived(result.URL);
                });
            }
        }
        catch (Exception ex)
        {
            DebugHelper.Logger.WriteLine(ex.Message);
        }

    }

    async void btnGo_ClickedAsync(System.Object sender, System.EventArgs e)
    {
        Uri uri = new Uri(lblUrl.Text);
        await Browser.Default.OpenAsync(uri, BrowserLaunchMode.SystemPreferred);
    }
}


