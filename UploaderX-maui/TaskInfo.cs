using System;
using System.Diagnostics;
using ShareX;
using ShareX.HelpersLib;
using ShareX.UploadersLib;

namespace UploaderX
{
	public class TaskInfo
	{
        private string filePath;

        public string FilePath
        {
            get
            {
                return filePath;
            }
            set
            {
                filePath = value;

                if (string.IsNullOrEmpty(filePath))
                {
                    FileName = "";
                }
                else
                {
                    FileName = Path.GetFileName(filePath);
                }
            }
        }

        public string FileName { get; set; }

        public TaskSettings TaskSettings { get; set; }

        public Stopwatch UploadDuration { get; set; }

        public UploadResult Result { get; set; }
    }
}

