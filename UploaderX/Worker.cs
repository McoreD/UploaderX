using System;
using ShareX;
using ShareX.HelpersLib;
using ShareX.UploadersLib;

namespace UploaderX;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private string watchDir = "/Users/mike/Library/CloudStorage/OneDrive-MainRoads/Pictures/Screenshots";
    private string destDir = "/Users/mike/Library/CloudStorage/OneDrive-Personal/Pictures/Screenshots";

    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;
        Program.Settings = ApplicationConfig.Load("/Users/mike/Library/CloudStorage/OneDrive-MainRoads/Apps/ShareX/ApplicationConfig.json");
        Program.UploadersConfig = UploadersConfig.Load("/Users/mike/Library/CloudStorage/OneDrive-MainRoads/Apps/ShareX/UploadersConfig.json");
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
        _logger.LogInformation("A new file has been created: " + e.FullPath);

        try
        {
            string destPath = Path.Combine(Path.Combine(Path.Combine(destDir, DateTime.Now.ToString("yyyy")), DateTime.Now.ToString("yyyy-MM")), Path.GetFileName(e.FullPath));
            FileHelpers.CreateDirectoryFromFilePath(destPath);
            File.Move(e.FullPath, destPath);
            _logger.LogInformation($"Moved {e.FullPath} to {destPath}");

            WorkerTask wt = new WorkerTask(destPath);
            UploadResult result = wt.UploadFile();
            _logger.LogInformation(result.URL);
            Task<bool> task = ClipboardHelpers.CopyTextAsync(result.URL);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex.StackTrace);
        }
    }
}

