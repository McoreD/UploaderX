#region License Information (GPL v3)

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

// using ShareX.HelpersLib.Properties;

namespace ShareX.HelpersLib
{
    public class CodeMenuEntryFilename : CodeMenuEntry
    {
        protected override string Prefix { get; } = "%";

        public static readonly CodeMenuEntryFilename t = new CodeMenuEntryFilename("t", "Title of window", "Window");
        public static readonly CodeMenuEntryFilename pn = new CodeMenuEntryFilename("pn", "Process_name_of_active_window", "Window");
        public static readonly CodeMenuEntryFilename y = new CodeMenuEntryFilename("y", "Current year", "Date and Time");
        public static readonly CodeMenuEntryFilename yy = new CodeMenuEntryFilename("yy", "Current year (2 digits)", "Date and Time");
        public static readonly CodeMenuEntryFilename mo = new CodeMenuEntryFilename("mo", "Current month", "Date and Time");
        public static readonly CodeMenuEntryFilename mon = new CodeMenuEntryFilename("mon", "Current month name (Local language)", "Date and Time");
        public static readonly CodeMenuEntryFilename mon2 = new CodeMenuEntryFilename("mon2", "Current month name (English)", "Date and Time");
        public static readonly CodeMenuEntryFilename w = new CodeMenuEntryFilename("w", "Current week name (Local language)", "Date and Time");
        public static readonly CodeMenuEntryFilename w2 = new CodeMenuEntryFilename("w2", "Current week name (English)", "Date and Time");
        public static readonly CodeMenuEntryFilename wy = new CodeMenuEntryFilename("wy", "Week of year", "Date and Time");
        public static readonly CodeMenuEntryFilename d = new CodeMenuEntryFilename("d", "Current day", "Date and Time");
        public static readonly CodeMenuEntryFilename h = new CodeMenuEntryFilename("h", "Current hour", "Date and Time");
        public static readonly CodeMenuEntryFilename mi = new CodeMenuEntryFilename("mi", "Current minute", "Date and Time");
        public static readonly CodeMenuEntryFilename s = new CodeMenuEntryFilename("s", "Current second", "Date and Time");
        public static readonly CodeMenuEntryFilename ms = new CodeMenuEntryFilename("ms", "Current millisecond", "Date and Time");
        public static readonly CodeMenuEntryFilename pm = new CodeMenuEntryFilename("pm", "Gets AM/PM", "Date and Time");
        public static readonly CodeMenuEntryFilename unix = new CodeMenuEntryFilename("unix", "Unix timestamp", "Date and Time");
        public static readonly CodeMenuEntryFilename i = new CodeMenuEntryFilename("i", "Auto increment number", "Incremental");
        public static readonly CodeMenuEntryFilename ia = new CodeMenuEntryFilename("ia", "Auto increment alphanumeric", "Incremental");
        public static readonly CodeMenuEntryFilename iAa = new CodeMenuEntryFilename("iAa", "Auto increment alphanumeric all", "Incremental");
        public static readonly CodeMenuEntryFilename ib = new CodeMenuEntryFilename("ib", "Auto increment base alphanumeric", "Incremental");
        public static readonly CodeMenuEntryFilename ix = new CodeMenuEntryFilename("ix", "Auto increment hexadecimal", "Incremental");
        public static readonly CodeMenuEntryFilename rn = new CodeMenuEntryFilename("rn", "Random number 0 to 9", "Random");
        public static readonly CodeMenuEntryFilename ra = new CodeMenuEntryFilename("ra", "Random alphanumeric char", "Random");
        public static readonly CodeMenuEntryFilename rna = new CodeMenuEntryFilename("rna", "Random non-ambiguous alphanumeric char repeat using N", "Random");
        public static readonly CodeMenuEntryFilename rx = new CodeMenuEntryFilename("rx", "Random hexadecimal", "Random");
        public static readonly CodeMenuEntryFilename guid = new CodeMenuEntryFilename("guid", "Random guid", "Random");
        public static readonly CodeMenuEntryFilename radjective = new CodeMenuEntryFilename("radjective", "Random adjective", "Random");
        public static readonly CodeMenuEntryFilename ranimal = new CodeMenuEntryFilename("ranimal", "Random animal", "Random");
        public static readonly CodeMenuEntryFilename remoji = new CodeMenuEntryFilename("remoji", "Random emoji repeat using N", "Random");
        public static readonly CodeMenuEntryFilename rf = new CodeMenuEntryFilename("rf", "Random line from file", "Random");
        public static readonly CodeMenuEntryFilename width = new CodeMenuEntryFilename("width", "Image width", "Image");
        public static readonly CodeMenuEntryFilename height = new CodeMenuEntryFilename("height", "Image height", "Image");
        public static readonly CodeMenuEntryFilename un = new CodeMenuEntryFilename("un", "User name", "Computer");
        public static readonly CodeMenuEntryFilename uln = new CodeMenuEntryFilename("uln", "User login name", "Computer");
        public static readonly CodeMenuEntryFilename cn = new CodeMenuEntryFilename("cn", "Computer name", "Computer");
        public static readonly CodeMenuEntryFilename n = new CodeMenuEntryFilename("n", "New line");

        public CodeMenuEntryFilename(string value, string description, string category = null) : base(value, description, category)
        {
        }
    }
}