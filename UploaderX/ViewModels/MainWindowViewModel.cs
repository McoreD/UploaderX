
using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Threading;
using MediaLib;
using ShareX;
using ShareX.HelpersLib;
using ShareX.UploadersLib;

namespace UploaderX.ViewModels;

public class MainWindowViewModel : ViewModelBase, INotifyPropertyChanged
{
    public string Greeting => "Welcome to Avalonia!";
    public string AppConfigPath { get; set; }
    public string UploadersConfigPath { get; set; }
    public string WatchDir { get; set; }
    public string DestDir { get; set; }
    public string DestSubDir { get; set; }

    public event PropertyChangedEventHandler PropertyChanged;

    private string _url = "http://";
    public string Url
    {
        get => _url;
        set
        {
            if (value == _url) return;
            _url = value;
            OnPropertyChanged();
        }
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    string AppDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "UploaderX");

    private FileSystemWatcher _watcher;
    public delegate void UrlReceivedEventHandler(string url);
    public event UrlReceivedEventHandler UrlReceived;

    public MainWindowViewModel()
    {
        DebugHelper.Init(Path.Combine(Path.Combine(AppDir, "Logs"), $"UploaderX-{DateTime.Now.ToString("yyyyMMdd")}-Log.txt"));

        string AppSettingsDir = Path.Combine(AppDir, "Settings");
        AppConfigPath = Path.Combine(AppSettingsDir, "ApplicationConfig.json");
        App.Settings = ApplicationConfig.Load(AppConfigPath);

        UploadersConfigPath = Path.Combine(AppSettingsDir, "UploadersConfig.json");
        App.UploadersConfig = UploadersConfig.Load(UploadersConfigPath);
        App.UploadersConfig.SupportDPAPIEncryption = false;

        WatchDir = Directory.Exists(App.Settings.CustomScreenshotsPath2) ? App.Settings.CustomScreenshotsPath2 : Path.Combine(AppDir, "Watch Folder");
        Helpers.CreateDirectoryFromDirectoryPath(WatchDir);
        DestDir = WatchDir;
        DestSubDir = Path.Combine(Path.Combine(DestDir, DateTime.Now.ToString("yyyy")), DateTime.Now.ToString("yyyy-MM"));

        _watcher = new FileSystemWatcher();
        _watcher.Path = WatchDir;

        _watcher.NotifyFilter = NotifyFilters.FileName;
        _watcher.Created += OnCreated;
        _watcher.EnableRaisingEvents = true;

        UrlReceived += MainWindowViewModel_UrlReceivedAsync;
    }

    private async void MainWindowViewModel_UrlReceivedAsync(string url)
    {
        Url = url;
        await Application.Current.Clipboard.SetTextAsync(url);
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
            string destPath = Path.Combine(DestSubDir, fileName);
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
                    FFmpegCLIManager ffmpeg = new FFmpegCLIManager(ffmpegPath);
                    string mp4Path = Path.ChangeExtension(destPath, "mp4");
                    string args = $"-i \"{destPath}\" -c:v libx264 -preset medium -crf 23 -pix_fmt yuv420p -movflags +faststart -y \"{mp4Path}\"";
                    if (ffmpeg.Run(args))
                    {
                        FileHelpers.DeleteFile(destPath);
                        destPath = mp4Path;
                    }
                }

                WorkerTask task = new WorkerTask(destPath);
                UploadResult result = task.UploadFile();
                DebugHelper.Logger.WriteLine(result.URL);

                Dispatcher.UIThread.Post(() =>
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
}

