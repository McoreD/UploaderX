using System;
using ShareX;
using ShareX.HelpersLib;
using ShareX.UploadersLib;

namespace UploaderX;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;

    private string watchDir;
    private string destDir;

    public Worker(ILogger<Worker> logger)
    {
        switch (Environment.OSVersion.Platform)
        {
            case PlatformID.Unix:
                watchDir = "/Users/mike/Library/CloudStorage/OneDrive-MainRoads/Pictures/Screenshots";
                destDir = "/Users/mike/Library/CloudStorage/OneDrive-Personal/Pictures/Screenshots";
                Program.Settings = ApplicationConfig.Load("/Users/mike/Library/CloudStorage/OneDrive-MainRoads/Apps/ShareX/ApplicationConfig.json");
                Program.UploadersConfig = UploadersConfig.Load("/Users/mike/Library/CloudStorage/OneDrive-MainRoads/Apps/ShareX/UploadersConfig.json");
                break;
            case PlatformID.Win32NT:
                watchDir = "C:\\Users\\mike\\OneDrive - Main Roads\\Pictures\\Screenshots";
                destDir = "C:\\Users\\mike\\OneDrive\\Pictures\\Screenshots";
                Program.Settings = ApplicationConfig.Load(@"C:\Users\mike\OneDrive - Main Roads\Apps\ShareX\ApplicationConfig.json");
                Program.UploadersConfig = UploadersConfig.Load(@"C:\\Users\\mike\\OneDrive - Main Roads\\Apps\\ShareX\\UploadersConfig.json");
                break;
        }

        _logger = logger;
        _logger.LogInformation($"Active S3 endpoint: {Program.UploadersConfig.AmazonS3Settings.Endpoint}");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        FileSystemWatcher watcher = new FileSystemWatcher();
        watcher.IncludeSubdirectories = true;
        watcher.Path = watchDir;

        watcher.NotifyFilter = NotifyFilters.FileName;
        watcher.Created += OnChanged;
        watcher.EnableRaisingEvents = true;
    }

    void OnChanged(object sender, FileSystemEventArgs e)
    {
        try
        {
            string destPath = Path.Combine(Path.Combine(Path.Combine(destDir, DateTime.Now.ToString("yyyy")), DateTime.Now.ToString("yyyy-MM")), Path.GetFileName(e.FullPath));
            FileHelpers.CreateDirectoryFromFilePath(destPath);
            if (!Path.GetFileName(e.FullPath).StartsWith("."))
            {
                File.Move(e.FullPath, destPath);

                WorkerTask wt = new WorkerTask(destPath);
                UploadResult result = wt.UploadFile();
                _logger.LogInformation(result.URL);
                ClipboardHelpers.CopyText(result.URL);
            }
        }
        catch(Exception ex)
        {
            _logger.LogError(ex.Message);
        }
    }
}

