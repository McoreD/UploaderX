using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualBasic.FileIO;

namespace ShareX.HelpersLib
{
    public class FileHelpers
    {
        public static readonly string[] ImageFileExtensions = new string[] { "jpg", "jpeg", "png", "gif", "bmp", "ico", "tif", "tiff" };
        public static readonly string[] TextFileExtensions = new string[] { "txt", "log", "nfo", "c", "cpp", "cc", "cxx", "h", "hpp", "hxx", "cs", "vb",
            "html", "htm", "xhtml", "xht", "xml", "css", "js", "php", "bat", "java", "lua", "py", "pl", "cfg", "ini", "dart", "go", "gohtml" };
        public static readonly string[] VideoFileExtensions = new string[] { "mp4", "webm", "mkv", "avi", "vob", "ogv", "ogg", "mov", "qt", "wmv", "m4p",
            "m4v", "mpg", "mp2", "mpeg", "mpe", "mpv", "m2v", "m4v", "flv", "f4v" };

        public static string GetFileNameExtension(string filePath, bool includeDot = false, bool checkSecondExtension = true)
        {
            string extension = "";

            if (!string.IsNullOrEmpty(filePath))
            {
                int pos = filePath.LastIndexOf('.');

                if (pos >= 0)
                {
                    extension = filePath.Substring(pos + 1);

                    if (checkSecondExtension)
                    {
                        filePath = filePath.Remove(pos);
                        string extension2 = GetFileNameExtension(filePath, false, false);

                        if (!string.IsNullOrEmpty(extension2))
                        {
                            foreach (string knownExtension in new string[] { "tar" })
                            {
                                if (extension2.Equals(knownExtension, StringComparison.OrdinalIgnoreCase))
                                {
                                    extension = extension2 + "." + extension;
                                    break;
                                }
                            }
                        }
                    }

                    if (includeDot)
                    {
                        extension = "." + extension;
                    }
                }
            }

            return extension;
        }

        public static bool CheckExtension(string filePath, IEnumerable<string> extensions)
        {
            string ext = GetFileNameExtension(filePath);

            if (!string.IsNullOrEmpty(ext))
            {
                return extensions.Any(x => ext.Equals(x, StringComparison.OrdinalIgnoreCase));
            }

            return false;
        }

        public static bool IsImageFile(string filePath)
        {
            return CheckExtension(filePath, ImageFileExtensions);
        }

        public static bool IsTextFile(string filePath)
        {
            return CheckExtension(filePath, TextFileExtensions);
        }

        public static bool IsVideoFile(string filePath)
        {
            return CheckExtension(filePath, VideoFileExtensions);
        }

        public static bool IsFileLocked(string filePath)
        {
            try
            {
                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    fs.Close();
                }
            }
            catch (IOException)
            {
                return true;
            }

            return false;
        }

        public static long GetFileSize(string filePath)
        {
            try
            {
                return new FileInfo(filePath).Length;
            }
            catch
            {
            }

            return -1;
        }

        public static bool DeleteFile(string filePath, bool sendToRecycleBin = false)
        {
            try
            {
                if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
                {
                    if (sendToRecycleBin)
                    {
                        FileSystem.DeleteFile(filePath, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
                    }
                    else
                    {
                        File.Delete(filePath);
                    }

                    return true;
                }
            }
            catch (Exception e)
            {
                DebugHelper.WriteException(e);
            }

            return false;
        }

        public static string GetPathRoot(string path)
        {
            int separator = path.IndexOf(":\\");

            if (separator > 0)
            {
                return path.Substring(0, separator + 2);
            }

            return "";
        }

        public static string SanitizeFileName(string fileName, string replaceWith = "")
        {
            char[] invalidChars = Path.GetInvalidFileNameChars();
            return SanitizeFileName(fileName, replaceWith, invalidChars);
        }

        private static string SanitizeFileName(string fileName, string replaceWith, char[] invalidChars)
        {
            fileName = fileName.Trim();

            foreach (char c in invalidChars)
            {
                fileName = fileName.Replace(c.ToString(), replaceWith);
            }

            return fileName;
        }

        public static string SanitizePath(string path, string replaceWith = "")
        {
            string root = GetPathRoot(path);

            if (!string.IsNullOrEmpty(root))
            {
                path = path.Substring(root.Length);
            }

            char[] invalidChars = Path.GetInvalidFileNameChars().Except(new char[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }).ToArray();
            path = SanitizeFileName(path, replaceWith, invalidChars);

            return root + path;
        }

        public static string GetVariableFolderPath(string path, bool supportCustomSpecialFolders = false)
        {
            if (!string.IsNullOrEmpty(path))
            {
                try
                {
                    if (supportCustomSpecialFolders)
                    {
                        foreach (KeyValuePair<string, string> specialFolder in HelpersOptions.ShareXSpecialFolders)
                        {
                            path = path.Replace(specialFolder.Value, $"%{specialFolder.Key}%", StringComparison.OrdinalIgnoreCase);
                        }
                    }

                    foreach (Environment.SpecialFolder specialFolder in Helpers.GetEnums<Environment.SpecialFolder>())
                    {
                        path = path.Replace(Environment.GetFolderPath(specialFolder), $"%{specialFolder}%", StringComparison.OrdinalIgnoreCase);
                    }
                }
                catch (Exception e)
                {
                    DebugHelper.WriteException(e);
                }
            }

            return path;
        }

        public static string ExpandFolderVariables(string path, bool supportCustomSpecialFolders = false)
        {
            if (!string.IsNullOrEmpty(path))
            {
                try
                {
                    if (supportCustomSpecialFolders)
                    {
                        foreach (KeyValuePair<string, string> specialFolder in HelpersOptions.ShareXSpecialFolders)
                        {
                            path = path.Replace($"%{specialFolder.Key}%", specialFolder.Value, StringComparison.OrdinalIgnoreCase);
                        }
                    }

                    foreach (Environment.SpecialFolder specialFolder in Helpers.GetEnums<Environment.SpecialFolder>())
                    {
                        path = path.Replace($"%{specialFolder}%", Environment.GetFolderPath(specialFolder), StringComparison.OrdinalIgnoreCase);
                    }

                    path = Environment.ExpandEnvironmentVariables(path);
                }
                catch (Exception e)
                {
                    DebugHelper.WriteException(e);
                }
            }

            return path;
        }

        public static void CreateDirectory(string directoryPath)
        {
            if (!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath))
            {
                try
                {
                    Directory.CreateDirectory(directoryPath);
                }
                catch (Exception e)
                {
                    DebugHelper.WriteException(e);
                }
            }
        }

        public static void CreateDirectoryFromFilePath(string filePath)
        {
            if (!string.IsNullOrEmpty(filePath))
            {
                string directoryPath = Path.GetDirectoryName(filePath);
                CreateDirectory(directoryPath);
            }
        }

        public static string BackupFileWeekly(string filePath, string destinationFolder)
        {
            if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
            {
                string fileName = Path.GetFileNameWithoutExtension(filePath);
                DateTime dateTime = DateTime.Now;
                string extension = Path.GetExtension(filePath);
                string newFileName = string.Format("{0}-{1:yyyy-MM}-W{2:00}{3}", fileName, dateTime, dateTime.WeekOfYear(), extension);
                string newFilePath = Path.Combine(destinationFolder, newFileName);

                if (!File.Exists(newFilePath))
                {
                    CreateDirectory(destinationFolder);
                    File.Copy(filePath, newFilePath, false);
                    return newFilePath;
                }
            }

            return null;
        }
    }
}
