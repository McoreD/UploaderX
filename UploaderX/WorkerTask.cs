﻿using System;
using ShareX.HelpersLib;
using System.Diagnostics;
using ShareX.UploadersLib;

namespace UploaderX
{
	public class WorkerTask : IDisposable
	{
        public TaskInfo Info { get; private set; }
        public Stream Data { get; private set; }

        private GenericUploader uploader;
        private TaskReferenceHelper taskReferenceHelper;

        public WorkerTask()
		{
		}

        private bool LoadFileStream()
        {
            try
            {
                Data = new FileStream(Info.FilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            }
            catch (Exception e)
            {
                return false;
            }

            return true;
        }

        public UploadResult UploadFile(Stream stream, string fileName)
        {
            FileUploaderService service = UploaderFactory.FileUploaderServices[FileDestination.AmazonS3];

            return UploadData(service, stream, fileName);
        }

        public UploadResult UploadData(IGenericUploaderService service, Stream stream, string fileName)
        {

            uploader = service.CreateUploader(Program.UploadersConfig, taskReferenceHelper);

            if (uploader != null)
            {
                uploader.Errors.DefaultTitle = service.ServiceName + " " + "error";
                uploader.BufferSize = (int)Math.Pow(2, Program.Settings.BufferSizePower) * 1024;

                if (Info.TaskSettings.AfterUploadJob.HasFlag(AfterUploadTasks.CopyURLToClipboard) && Info.TaskSettings.AdvancedSettings.EarlyCopyURL)
                {
                    uploader.EarlyURLCopyRequested += url =>
                    {
                        ClipboardHelpers.CopyText(url);
                        EarlyURLCopied = true;
                    };
                }

                fileName = URLHelpers.RemoveBidiControlCharacters(fileName);

                if (Info.TaskSettings.UploadSettings.FileUploadReplaceProblematicCharacters)
                {
                    fileName = URLHelpers.ReplaceReservedCharacters(fileName, "_");
                }

                Info.UploadDuration = Stopwatch.StartNew();

                UploadResult result = uploader.Upload(stream, fileName);

                Info.UploadDuration.Stop();

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

