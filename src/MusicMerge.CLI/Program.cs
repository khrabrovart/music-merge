using CLIRedraw;
using MusicMerge.CLI.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace MusicMerge.CLI
{
    public class Program
    {
        static void Main(string[] args)
        {
            var menuItems = new[]
            {
                new MenuItem("Show differences", mi => ShowList()),
                new MenuItem("Set directory 1", mi => SetDirectory1()),
                new MenuItem("Set directory 2", mi => SetDirectory2()),
            };

            new Menu("MUSIC MERGE", menuItems).Show();
        }

        static void ShowList()
        {
            var dir1 = Settings.Default.MusicDirectory1;
            var dir2 = Settings.Default.MusicDirectory2;

            if (!CheckDirectory(dir1) || !CheckDirectory(dir2))
            {
                Console.WriteLine("Invalid directories.");
            }

            var files1 = GetMusicFileNames(dir1);
            var files2 = GetMusicFileNames(dir2);

            var musicFiles1 = files1.Except(files2).Select(f => new MusicFile(Path.Combine(dir1, f), MusicDirectory.Directory1));
            var musicFiles2 = files2.Except(files1).Select(f => new MusicFile(Path.Combine(dir2, f), MusicDirectory.Directory2));
            var allMusicFiles = musicFiles1.Concat(musicFiles2).ToArray();

            var musicMenuItems = new List<MenuItem>(allMusicFiles.Length);
            var menu = new Menu("LIST OF DIFFERENCES (Enter - open, Delete - delete, Insert - restore)", musicMenuItems);

            foreach (var mf in allMusicFiles)
            {
                var directoryToDelete = mf.Directory == MusicDirectory.Directory1 ? dir1 : dir2;
                var directoryToInsert = mf.Directory == MusicDirectory.Directory1 ? dir2 : dir1;

                var menuItem = new MenuItem($"{mf.Name}");
                menuItem.AddOrUpdateAction(ConsoleKey.Enter, mi => OpenInExplorer(mf));

                menuItem.AddOrUpdateAction(ConsoleKey.Delete, mi => 
                {
                    Delete(mf);
                    menu.Remove(menuItem);
                });

                menuItem.AddOrUpdateAction(ConsoleKey.Insert, mi => 
                {
                    Restore(mf);
                    menu.Remove(menuItem);
                });

                menu.Add(menuItem);
            }

            menu.Show();
        }

        static bool CheckDirectory(string directory)
        {
            return !string.IsNullOrWhiteSpace(directory) && Directory.Exists(directory);
        }

        static IEnumerable<string> GetMusicFileNames(string directory)
        {
            return Directory.GetFiles(directory, "*.mp3").Select(f => Path.GetFileName(f));
        }

        static void SetDirectory1()
        {
            var current = Settings.Default.MusicDirectory1;

            if (!string.IsNullOrEmpty(current))
            {
                Console.WriteLine($"Current directory: {current}");
            }

            Console.Write("Enter directory: ");
            var directory = Console.ReadLine();

            if (!CheckDirectory(directory))
            {
                return;
            }

            Settings.Default.MusicDirectory1 = directory;
            Settings.Default.Save();
        }

        static void SetDirectory2()
        {
            var current = Settings.Default.MusicDirectory2;

            if (!string.IsNullOrEmpty(current))
            {
                Console.WriteLine($"Current directory: {current}");
            }

            Console.Write("Enter directory: ");
            var directory = Console.ReadLine();

            if (!CheckDirectory(directory))
            {
                return;
            }

            Settings.Default.MusicDirectory2 = directory;
            Settings.Default.Save();
        }

        static void OpenInExplorer(MusicFile musicFile)
        {
            if (File.Exists(musicFile.FilePath))
            {
                Process.Start("explorer.exe", musicFile.FilePath);
            }
        }

        static void Delete(MusicFile musicFile)
        {
            if (File.Exists(musicFile.FilePath))
            {
                File.Delete(musicFile.FilePath);
            }
        }

        static void Restore(MusicFile musicFile)
        {
            if (File.Exists(musicFile.FilePath))
            {
                var destinationDirectory = musicFile.Directory == MusicDirectory.Directory1 
                    ? Settings.Default.MusicDirectory2 
                    : Settings.Default.MusicDirectory1;

                var copyTo = Path.Combine(destinationDirectory, Path.GetFileName(musicFile.FilePath));

                Console.WriteLine("Copying...");
                File.Copy(musicFile.FilePath, copyTo);
            }
        }
    }
}
