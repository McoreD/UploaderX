using System;
using System.IO;

namespace UploadersLib
{
    public class Watcher
    {
        static void Main(string[] args)
        {
            // Set the directory to watch
            string directory = "/path/to/watch/folder";

            // Create a new FileSystemWatcher
            FileSystemWatcher watcher = new FileSystemWatcher();
            watcher.Path = directory;

            // Set the filters to only watch for new files
            watcher.NotifyFilter = NotifyFilters.FileName;

            // Add event handlers
            watcher.Created += OnChanged;

            // Begin watching the directory
            watcher.EnableRaisingEvents = true;

            // Wait for the user to quit the program
            Console.WriteLine("Press 'q' to quit the program.");
            while (Console.Read() != 'q') ;
        }

        // This method is called when a new file is created in the watched directory
        static void OnChanged(object sender, FileSystemEventArgs e)
        {
            Console.WriteLine("A new file has been created: " + e.FullPath);
        }
    }
}


