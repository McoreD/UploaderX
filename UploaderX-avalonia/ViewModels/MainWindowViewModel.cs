
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
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

    private ObservableCollection<string> urlsCollection = new ObservableCollection<string>();
    public ObservableCollection<string> UrlsCollection
    {
        get { return urlsCollection; }
        set
        {
            if (value != this.urlsCollection)
                urlsCollection = value;
            this.SetPropertyChanged("FileObjectCollection");
        }
    }

    public MainWindowViewModel()
    {
        DebugHelper.Init(Path.Combine(Path.Combine(Program.ConfigDir, "Logs"), $"UploaderX-{DateTime.Now.ToString("yyyyMMdd")}-Log.txt"));

        string AppSettingsDir = Path.Combine(Program.ConfigDir, "Settings");

        Program.MyWorker.FilesDropped += MyWorker_FilesDropped;
        Program.MyWorker.UrlReceived += MyWorker_UrlReceivedAsync;
        Program.MyWorker.UrlCollectionReceived += MyWorker_UrlCollectionReceivedAsync;
        Program.MyWorker.Watch();

        AppConfigPath = Program.MyWorker.AppConfigPath;
        UploadersConfigPath = Program.MyWorker.UploadersConfigPath;
        WatchDir = Program.MyWorker.WatchDir;
        DestSubDir = Program.MyWorker.DestDir;

    }

    private void MyWorker_FilesDropped(IEnumerable<string> filePaths)
    {
        UrlsCollection.Clear();
    }

    private async void MyWorker_UrlCollectionReceivedAsync(IEnumerable<string> urls)
    {
        string urlsString = string.Join(Environment.NewLine, urlsCollection);
        await Application.Current.Clipboard.SetTextAsync(urlsString);
    }
    
    private async void MyWorker_UrlReceivedAsync(string url)
    {
        Url = url;
        urlsCollection.Add(url);
        await Application.Current.Clipboard.SetTextAsync(url);
    }

}

