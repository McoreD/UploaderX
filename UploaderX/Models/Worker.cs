using System;
using ShareX.HelpersLib;
using System.Diagnostics;
using ShareX.UploadersLib;
using ShareX;
using System.IO;
using Avalonia.Threading;
using MediaLib;

namespace UploaderX
{
    public class Worker : IDisposable
    {
        private string _configDir { get; set; }
        private ApplicationConfig? _appConfig { get; set; }
        private UploadersConfig? _uploadersConfig { get; set; }

        public string AppConfigPath { get; set; }
        public string UploadersConfigPath { get; set; }
        public string WatchDir { get; set; }
        public string DestDir { get; set; }
        public string DestSubDir { get; set; }

        public TaskInfo Info { get; private set; }
        public Stream Data { get; private set; }

        private GenericUploader uploader;
        private TaskReferenceHelper taskReferenceHelper;

        private FileSystemWatcher _watcher;
        public delegate void UrlReceivedEventHandler(string url);
        public event UrlReceivedEventHandler UrlReceived;


        public Worker(string configDir)
        {
            string settingsDir = Path.Combine(configDir, "Settings");
            AppConfigPath = Path.Combine(settingsDir, "ApplicationConfig.json");
            UploadersConfigPath = Path.Combine(settingsDir, "UploadersConfig.json");

            _appConfig = ApplicationConfig.Load(AppConfigPath);
            _uploadersConfig = UploadersConfig.Load(UploadersConfigPath);
            _uploadersConfig.SupportDPAPIEncryption = false;

            WatchDir = Directory.Exists(_appConfig.CustomScreenshotsPath2) ? _appConfig.CustomScreenshotsPath2 : Path.Combine(_configDir, "Watch Folder");
            DestDir = WatchDir;
            DestSubDir = Path.Combine(Path.Combine(DestDir, DateTime.Now.ToString("yyyy")), DateTime.Now.ToString("yyyy-MM"));

            Helpers.CreateDirectoryFromDirectoryPath(WatchDir);
        }

        private void Init(string filePath)
        {
            Info = new TaskInfo();
            Info.FilePath = filePath;
        }

        private bool LoadFileStream()
        {
            try
            {
                Data = new FileStream(Info.FilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                return false;
            }

            return true;
        }

        private UploadResult UploadFile()
        {
            LoadFileStream();
            return UploadFile(Data, Info.FileName);
        }

        private UploadResult UploadFile(Stream stream, string fileName)
        {
            IGenericUploaderService service;
            if (!string.IsNullOrEmpty(_uploadersConfig.AmazonS3Settings.SecretAccessKey))
            {
                service = UploaderFactory.FileUploaderServices[FileDestination.AmazonS3];
            }
            else
            {
                service = UploaderFactory.ImageUploaderServices[ImageDestination.Imgur];
            }
            return UploadData(service, stream, fileName);
        }

        private UploadResult UploadData(IGenericUploaderService service, Stream stream, string fileName)
        {

            uploader = service.CreateUploader(_uploadersConfig, taskReferenceHelper);

            if (uploader != null)
            {
                uploader.Errors.DefaultTitle = service.ServiceName + " " + "error";
                uploader.BufferSize = (int)Math.Pow(2, _appConfig.BufferSizePower) * 1024;

                fileName = URLHelpers.RemoveBidiControlCharacters(fileName);
                fileName = URLHelpers.ReplaceReservedCharacters(fileName, "_");

                Info.UploadDuration = Stopwatch.StartNew();

                UploadResult result = uploader.Upload(stream, fileName);

                Info.UploadDuration.Stop();

                Console.WriteLine(uploader.Errors.ToString());

                return result;
            }

            return null;
        }

        public void Dispose()
        {
            if (Data != null)
            {
                Data.Dispose();
                Data = null;
            }
        }

        private void OnUrlReceived(string url)
        {
            UrlReceived?.Invoke(url);
        }

        internal void Watch()
        {
            _watcher = new FileSystemWatcher();
            _watcher.Path = WatchDir;

            _watcher.NotifyFilter = NotifyFilters.FileName;
            _watcher.Created += OnCreated;
            _watcher.EnableRaisingEvents = true;
        }

        async void OnCreated(object sender, FileSystemEventArgs e)
        {
            try
            {
                string fileName = new NameParser(NameParserType.FileName).Parse("%y%mo%d_%ra{10}") + Path.GetExtension(e.FullPath);
                string destPath = Path.Combine(DestSubDir, fileName);
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

                    if (Path.GetExtension(destPath).ToLower().Equals(".mov"))
                    {
                        string ffmpegDir = Path.Combine(_configDir, "Tools");
                        string ffmpegPath = Path.Combine(ffmpegDir, "ffmpeg");
                        FFmpegCLIManager ffmpeg = new FFmpegCLIManager(ffmpegPath);
                        string mp4Path = Path.ChangeExtension(destPath, "mp4");
                        string args = $"-i \"{destPath}\" -c:v libx264 -preset medium -crf 23 -pix_fmt yuv420p -movflags +faststart -y \"{mp4Path}\"";
                        if (ffmpeg.Run(args))
                        {
                            FileHelpers.DeleteFile(destPath);
                            destPath = mp4Path;
                        }
                    }

                    Init(destPath);
                    UploadResult result = UploadFile();
                    DebugHelper.Logger.WriteLine(result.URL);

                    Dispatcher.UIThread.Post(() =>
                    {
                        OnUrlReceived(result.URL);
                    });
                }
            }
            catch (Exception ex)
            {
                DebugHelper.Logger.WriteLine(ex.Message);
            }

        }
    }
}

