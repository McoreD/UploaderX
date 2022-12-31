using System;
using System.IO;
using ShareX;
using ShareX.HelpersLib;
using ShareX.UploadersLib;
using Microsoft.Maui.ApplicationModel.DataTransfer;
using TextCopy;

namespace UploaderX;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private FileSystemWatcher _watcher;

    private string _watchDir;
    private string _destDir;

    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;
        string AppDir = AppDomain.CurrentDomain.BaseDirectory;
        Program.Settings = ApplicationConfig.Load(Path.Combine(AppDir, "ApplicationConfig.json"));
        Program.UploadersConfig = UploadersConfig.Load(Path.Combine(AppDir, "UploadersConfig.json"));
        Program.UploadersConfig.SupportDPAPIEncryption = false;

        _watchDir = Directory.Exists(Program.Settings.CustomScreenshotsPath2) ? Program.Settings.CustomScreenshotsPath2 : Path.Combine(AppDir, "Watch Folder");
        Helpers.CreateDirectoryFromDirectoryPath(_watchDir);

        _destDir = _watchDir;

        _logger.LogInformation("Watch Dir: " + _watchDir);
        _logger.LogInformation("Destination Dir: " + _destDir);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _watcher = new FileSystemWatcher();
        _watcher.Path = _watchDir;

        _watcher.NotifyFilter = NotifyFilters.FileName;
        _watcher.Created += OnChanged;
        _watcher.EnableRaisingEvents = true;
    }

    async void OnChanged(object sender, FileSystemEventArgs e)
    {
        try
        {
            string fileName = new NameParser(NameParserType.FileName).Parse("%y%mo%d_%ra{10}") + Path.GetExtension(e.FullPath);
            string destPath = Path.Combine(Path.Combine(Path.Combine(_destDir, DateTime.Now.ToString("yyyy")), DateTime.Now.ToString("yyyy-MM")), fileName);
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

                WorkerTask wt = new WorkerTask(destPath);
                UploadResult result = wt.UploadFile();
                _logger.LogInformation(result.URL);
                ClipboardService.SetText(result.URL);
                // TODO: Clipboard.Default.SetTextAsync(result.URL);
            }
        }
        catch(Exception ex)
        {
            _logger.LogError(ex.Message);
        }
    }
}

