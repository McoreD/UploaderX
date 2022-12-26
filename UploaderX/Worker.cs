namespace UploaderX;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;

    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Set the directory to watch
        string dir = "/Users/mike/Library/CloudStorage/OneDrive-Personal/Pictures/Screenshots";

        // Create a new FileSystemWatcher
        FileSystemWatcher watcher = new FileSystemWatcher();
        watcher.Path = dir;

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
    }
}

