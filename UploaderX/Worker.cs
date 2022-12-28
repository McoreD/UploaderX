using ShareX.HelpersLib;

namespace UploaderX;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private string watchDir = "/Users/mike/Library/CloudStorage/OneDrive-MainRoads/Pictures/Screenshots";
    private string destDir = "/Users/mike/Library/CloudStorage/OneDrive-Personal/Pictures/Screenshots";

    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Create a new FileSystemWatcher
        FileSystemWatcher watcher = new FileSystemWatcher();
        watcher.IncludeSubdirectories = true;
        watcher.Path = watchDir;

        // Set the filters to only watch for new files
        watcher.NotifyFilter = NotifyFilters.FileName;

        // Add event handlers
        watcher.Created += OnChanged;

        // Begin watching the directory
        watcher.EnableRaisingEvents = true;

        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            await Task.Delay(1000, stoppingToken);
        }
    }

    // This method is called when a new file is created in the watched directory
    void OnChanged(object sender, FileSystemEventArgs e)
    {
        _logger.LogInformation("A new file has been created: " + e.FullPath);

        try
        {
            // Move to yyyy-MM folder
            string destPath = Path.Combine(Path.Combine(Path.Combine(destDir, DateTime.Now.ToString("yyyy")), DateTime.Now.ToString("yyyy-MM")), Path.GetFileName(e.FullPath));
            FileHelpers.CreateDirectoryFromFilePath(destPath);
            File.Move(e.FullPath, destPath);
            _logger.LogInformation($"Moved {e.FullPath} to {destPath}");
        }
        catch(Exception ex)
        {
            _logger.LogError(ex.Message);
        }
    }
}

