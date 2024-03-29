﻿using System.Diagnostics;
using ShareX.HelpersLib;

namespace UploaderX;

public partial class MainPage : ContentPage
{
    readonly string _configDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "UploaderX");

    private Worker _worker;
    private WebView _browser;

    public MainPage()
    {
        InitializeComponent();
        txtAppPath.Text = Process.GetCurrentProcess().MainModule.FileName;

        _worker = new Worker(_configDir);
        _worker.UrlReceived += _worker_UrlReceived;
        _worker.Watch();
     
        txtAppConfigPath.Text = _worker.AppConfigPath;
        txtUploaderConfigPath.Text = _worker.UploadersConfigPath;
        txtWatchDir.Text = _worker.WatchDir;
        txtScreenshotsDir.Text = _worker.DestDir;

        _browser = this.FindByName<WebView>("wvUrl");
    }

    private void _worker_UrlReceived(string url)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            Clipboard.Default.SetTextAsync(url);
            lblUrl.Text = url;
            _browser.Source = new UrlWebViewSource() { Url = url };
        });
    }

    async void btnGo_ClickedAsync(System.Object sender, System.EventArgs e)
    {
        Uri uri = new Uri(lblUrl.Text);
        await Browser.Default.OpenAsync(uri, BrowserLaunchMode.SystemPreferred);
    }

    async void btnBrowseWatchDir_Clicked(System.Object sender, System.EventArgs e)
    {
        Helpers.OpenFolder(txtWatchDir.Text);
    }

    void btnBrowseScreenshotsDir_Clicked(System.Object sender, System.EventArgs e)
    {
        Helpers.OpenFolder(txtScreenshotsDir.Text);
    }
}


