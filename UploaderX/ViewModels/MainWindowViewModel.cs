
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
    public event PropertyChangedEventHandler PropertyChanged;
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public string AppConfigPath { get; set; }
    public string UploadersConfigPath { get; set; }
    public string WatchDir { get; set; }
    public string DestSubDir { get; set; }

    private string _url = "https://";
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

    readonly string _configDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "UploaderX");

    private Worker _worker;

    public MainWindowViewModel()
    {
        DebugHelper.Init(Path.Combine(Path.Combine(_configDir, "Logs"), $"UploaderX-{DateTime.Now.ToString("yyyyMMdd")}-Log.txt"));

        string AppSettingsDir = Path.Combine(_configDir, "Settings");

        _worker = new Worker(_configDir);
        _worker.UrlReceived += MainWindowViewModel_UrlReceivedAsync;
        _worker.Watch();

        AppConfigPath = _worker.AppConfigPath;
        UploadersConfigPath = _worker.UploadersConfigPath;
        WatchDir = _worker.WatchDir;
        DestSubDir = _worker.DestSubDir;
    }

    private async void MainWindowViewModel_UrlReceivedAsync(string url)
    {
        Url = url;
        await Application.Current.Clipboard.SetTextAsync(url);
    }

}

