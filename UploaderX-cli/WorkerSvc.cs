using System;
using System.IO;
using ShareX;
using ShareX.HelpersLib;
using ShareX.UploadersLib;
using TextCopy;

namespace UploaderX;

public class WorkerSvc : BackgroundService
{
    private readonly ILogger<WorkerSvc> _logger;
    readonly string _configDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "UploaderX");
    private Worker _worker;

    public WorkerSvc(ILogger<WorkerSvc> logger)
    {
        _logger = logger;
    }

    private void _worker_UrlReceived(string url)
    {
        ClipboardService.SetText(url);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        string AppSettingsDir = Path.Combine(_configDir, "Settings");
        _worker = new Worker(_configDir);
        _worker.UrlReceived += _worker_UrlReceived; ;
        _worker.Watch();

        _logger.LogInformation("Watch Dir: " + _worker.WatchDir);
        _logger.LogInformation("Destination Dir: " + _worker.DestDir);
    }
}

