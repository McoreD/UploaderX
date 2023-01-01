﻿using System;
using ShareX.HelpersLib;
using System.Diagnostics;
using ShareX.UploadersLib;
using ShareX;
using System.IO;

namespace UploaderX
{
    public class Worker : IDisposable
    {
        public TaskInfo Info { get; private set; }
        public Stream Data { get; private set; }

        private GenericUploader uploader;
        private TaskReferenceHelper taskReferenceHelper;

        public Worker(string filePath)
        {
            Info = new TaskInfo();
            Info.FilePath = filePath;
            Info.TaskSettings = TaskSettings.GetDefaultTaskSettings();
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

        public UploadResult UploadFile()
        {
            LoadFileStream();
            return UploadFile(Data, Info.FileName);
        }

        public UploadResult UploadFile(Stream stream, string fileName)
        {
            IGenericUploaderService service;
            if (Directory.Exists(App.Settings.CustomScreenshotsPath2))
            {
                service = UploaderFactory.FileUploaderServices[FileDestination.AmazonS3];
            }
            else
            {
                service = UploaderFactory.ImageUploaderServices[ImageDestination.Imgur];
            }
            return UploadData(service, stream, fileName);
        }

        public UploadResult UploadData(IGenericUploaderService service, Stream stream, string fileName)
        {

            uploader = service.CreateUploader(App.UploadersConfig, taskReferenceHelper);

            if (uploader != null)
            {
                uploader.Errors.DefaultTitle = service.ServiceName + " " + "error";
                uploader.BufferSize = (int)Math.Pow(2, App.Settings.BufferSizePower) * 1024;

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
    }
}
