using System;
using ShareX.HelpersLib;
using System.Diagnostics;
using ShareX.UploadersLib;
using ShareX;
using System.IO;
using MediaLib;

namespace UploaderX
{
    public class Worker : IDisposable
    {
        private ApplicationConfig? _appConfig { get; set; }
        private UploadersConfig? _uploadersConfig { get; set; }

        public string AppConfigPath { get; set; }
        public string UploadersConfigPath { get; set; }
        public string WatchDir { get; set; }
        public string DestDir { get; set; }
        public string DestSubDir { get; set; }

        string _ffmpegDir;

        public TaskInfo Info { get; private set; }
        public Stream Data { get; private set; }

        private GenericUploader uploader;
        private TaskReferenceHelper taskReferenceHelper;

        private FileSystemWatcher _watcher;

        public delegate void FilesDroppedEventHandler(IEnumerable<string> filePaths);
        public event FilesDroppedEventHandler FilesDropped;

        public delegate void UrlCollectionReceivedEventHandler(IEnumerable<string> filePaths);
        public event UrlCollectionReceivedEventHandler UrlCollectionReceived;

        public delegate void UrlReceivedEventHandler(string url);
        public event UrlReceivedEventHandler UrlReceived;


        public Worker(string configDir)
        {
            DebugHelper.Init(Path.Combine(Path.Combine(configDir, "Logs"),
                $"UploaderX-{DateTime.Now.ToString("yyyyMMdd")}-Log.txt"));
            _ffmpegDir = Path.Combine(configDir, "Tools");
            string settingsDir = Path.Combine(configDir, "Settings");
            AppConfigPath = Path.Combine(settingsDir, "ApplicationConfig.json");
            UploadersConfigPath = Path.Combine(settingsDir, "UploadersConfig.json");

            _appConfig = ApplicationConfig.Load(AppConfigPath);
            _uploadersConfig = UploadersConfig.Load(UploadersConfigPath);
            _uploadersConfig.SupportDPAPIEncryption = false;

            WatchDir = Directory.Exists(_appConfig.CustomScreenshotsPath2)
                ? _appConfig.CustomScreenshotsPath2
                : Path.Combine(configDir, "Watch Folder");
            DestDir = WatchDir;
            DestSubDir = Path.Combine(Path.Combine(DestDir, DateTime.Now.ToString("yyyy")),
                DateTime.Now.ToString("yyyy-MM"));

            Helpers.CreateDirectoryFromDirectoryPath(WatchDir);
        }

        public void Watch()
        {
            _watcher = new FileSystemWatcher();
            _watcher.Path = WatchDir;

            _watcher.NotifyFilter = NotifyFilters.FileName;
            _watcher.Created += OnCreated;
            _watcher.EnableRaisingEvents = true;
        }

        private async void OnCreated(object sender, FileSystemEventArgs e)
        {
            try
            {
                string fileName = new NameParser(NameParserType.FileName).Parse("%y%mo%dT%h%mi%s_%ra{6}") +
                                  Path.GetExtension(e.FullPath);
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
                    }, 250, 5000, () => { File.Move(e.FullPath, destPath, overwrite: true); }, 1000);

                    UploadResult result = UploadFile(destPath);
                    DebugHelper.Logger.WriteLine(result.URL);
                }
            }
            catch (Exception ex)
            {
                DebugHelper.Logger.WriteLine(ex.Message);
            }
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

        public List<UploadResult> UploadFiles(IEnumerable<string> filePaths)
        {
            OnFilesDropped(filePaths);

            List<string> urls = new List<string>();
            List<UploadResult> results = new List<UploadResult>();
            foreach (string filePath in filePaths)
            {
                UploadResult r = UploadFile(filePath);
                if (r != null)
                    urls.Add(r.URL);
                results.Add(r);
            }
            
            OnUrlCollectionReceived(urls);
            return results;
        }

        public UploadResult UploadFile(string filePath)
        {
            if (Path.GetExtension(filePath).ToLower().Equals(".mov"))
            {
                string ffmpegPath = Path.Combine(_ffmpegDir, "ffmpeg");
                FFmpegCLIManager ffmpeg = new FFmpegCLIManager(ffmpegPath);
                if (File.Exists(ffmpegPath))
                {
                    string mp4Path = Path.ChangeExtension(filePath, "mp4");
                    string args =
                        $"-i \"{filePath}\" -c:v libx264 -preset medium -crf 23 -pix_fmt yuv420p -movflags +faststart -y \"{mp4Path}\"";
                    if (ffmpeg.Run(args))
                    {
                        FileHelpers.DeleteFile(filePath);
                        filePath = mp4Path;
                    }
                }
            }

            Info = new TaskInfo();
            Info.FilePath = filePath;

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

                if (result != null)
                    OnUrlReceived(result.URL);

                return result;
            }

            return null;
        }

        private void OnFilesDropped(IEnumerable<string> filePaths)
        {
            FilesDropped?.Invoke(filePaths);
        }

        private void OnUrlCollectionReceived(IEnumerable<string> urls)
        {
            UrlCollectionReceived?.Invoke(urls);
        }

        private void OnUrlReceived(string url)
        {
            UrlReceived?.Invoke(url);
        }

        public void Dispose()
        {
            if (Data != null)
            {
                Data.Dispose();
                Data = null;
            }
        }
    }
}