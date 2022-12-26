﻿#region License Information (GPL v3)

/*
    ShareX - A program that allows you to take screenshots and share any file type
    Copyright (c) 2007-2022 ShareX Team

    This program is free software; you can redistribute it and/or
    modify it under the terms of the GNU General Public License
    as published by the Free Software Foundation; either version 2
    of the License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program; if not, write to the Free Software
    Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.

    Optionally you can also view the license at <http://www.gnu.org/licenses/>.
*/

#endregion License Information (GPL v3)

using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ShareX.HelpersLib
{
	public static class FileHelpers
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

    }
}

