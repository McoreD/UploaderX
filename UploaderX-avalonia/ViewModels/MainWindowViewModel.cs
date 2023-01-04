
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
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

    public delegate void FileDroppedEventHandler(string[] filePath);
    public event FileDroppedEventHandler FileDropped;

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

    private ObservableCollection<FileObject> fileObjectCollection = new ObservableCollection<FileObject>();
    public ObservableCollection<FileObject> FileObjectCollection
    {
        get { return fileObjectCollection; }
        set
        {
            if (value != this.fileObjectCollection)
                fileObjectCollection = value;
            this.SetPropertyChanged("FileObjectCollection");
        }
    }

    public MainWindowViewModel()
    {
        DebugHelper.Init(Path.Combine(Path.Combine(Program.ConfigDir, "Logs"), $"UploaderX-{DateTime.Now.ToString("yyyyMMdd")}-Log.txt"));

        string AppSettingsDir = Path.Combine(Program.ConfigDir, "Settings");

        Program.MyWorker.UrlReceived += MainWindowViewModel_UrlReceivedAsync;
        Program.MyWorker.Watch();

        AppConfigPath = Program.MyWorker.AppConfigPath;
        UploadersConfigPath = Program.MyWorker.UploadersConfigPath;
        WatchDir = Program.MyWorker.WatchDir;
        DestSubDir = Program.MyWorker.DestSubDir;

       // fileObjectCollection.CollectionChanged += FileObjectCollection_CollectionChanged;
    }

    private void FileObjectCollection_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        Debug.WriteLine(fileObjectCollection[0].ToString());
    }

    private async void MainWindowViewModel_UrlReceivedAsync(string url)
    {
        Url = url;
        await Application.Current.Clipboard.SetTextAsync(url);
    }

}

