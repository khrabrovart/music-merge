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
        public static void Main(string[] args)
        {
            ShowMainMenu();
        }

        private static void ShowMainMenu()
        {
            var mainMenu = new Menu("MUSIC MERGE")
            {
                TitleForegroundColor = ConsoleColor.Yellow
            };

            mainMenu.AddItem("Show differences", new ExplicitMenuAction(ShowListOfDifferences));
            mainMenu.AddItem("Set directory 1", new ExplicitMenuAction(SetDirectory1));
            mainMenu.AddItem("Set directory 2", new ExplicitMenuAction(SetDirectory2));
            mainMenu.AddItem("Exit", new ImplicitMenuAction(mainMenu.Close));

            mainMenu.Show();
        }

        private static void ShowListOfDifferences()
        {
            var dir1 = Settings.Default.MusicDirectory1;
            var dir2 = Settings.Default.MusicDirectory2;

            if (!CheckDirectory(dir1) || !CheckDirectory(dir2))
            {
                Console.WriteLine("Invalid directories.");
                Console.ReadKey();
                return;
            }

            var files1 = GetMusicFileNames(dir1);
            var files2 = GetMusicFileNames(dir2);

            var musicFiles1 = files1.Except(files2).Select(f => new MusicFile(Path.Combine(dir1, f), MusicDirectory.Directory1));
            var musicFiles2 = files2.Except(files1).Select(f => new MusicFile(Path.Combine(dir2, f), MusicDirectory.Directory2));
            var differences = musicFiles1.Concat(musicFiles2).ToArray();

            if (!differences.Any())
            {
                Console.WriteLine("No differences found!\nPress any key to continue...");
                Console.ReadKey();
            }

            var musicMenuItems = new List<MenuItem>(differences.Length);
            var listMenu = new Menu("LIST OF DIFFERENCES (Enter - play, Delete - delete, R - restore, Esc - main menu)", musicMenuItems)
            {
                TitleForegroundColor = ConsoleColor.Yellow
            };

            foreach (var musicFile in differences)
            {
                var directoryToDelete = musicFile.Directory == MusicDirectory.Directory1 ? dir1 : dir2;
                var directoryToInsert = musicFile.Directory == MusicDirectory.Directory1 ? dir2 : dir1;

                var menuItem = new MenuItem($"{musicFile.Name}");
                menuItem.AddOrUpdateAction(ConsoleKey.Enter, new ImplicitMenuAction(() => Play(musicFile)));

                menuItem.AddOrUpdateAction(ConsoleKey.Delete, new ImplicitMenuAction(() => 
                {
                    Delete(musicFile);
                    listMenu.RemoveItem(menuItem);
                }));

                menuItem.AddOrUpdateAction(ConsoleKey.R, new ImplicitMenuAction(() => 
                {
                    Restore(musicFile);
                    listMenu.RemoveItem(menuItem);
                }));

                menuItem.AddOrUpdateAction(ConsoleKey.Escape, new ImplicitMenuAction(listMenu.Close));

                listMenu.AddItem(menuItem);
            }

            listMenu.Show();
        }

        private static bool CheckDirectory(string directory) => !string.IsNullOrWhiteSpace(directory) && Directory.Exists(directory);

        private static IEnumerable<string> GetMusicFileNames(string directory) => Directory.GetFiles(directory, "*.mp3").Select(f => Path.GetFileName(f));

        private static void SetDirectory1()
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

        private static void SetDirectory2()
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

        private static void Play(MusicFile musicFile)
        {
            if (File.Exists(musicFile.FilePath))
            {
                Process.Start("explorer.exe", musicFile.FilePath);
            }
        }

        private static void Delete(MusicFile musicFile)
        {
            if (File.Exists(musicFile.FilePath))
            {
                File.Delete(musicFile.FilePath);
            }
        }

        private static void Restore(MusicFile musicFile)
        {
            if (File.Exists(musicFile.FilePath))
            {
                var destinationDirectory = musicFile.Directory == MusicDirectory.Directory1 
                    ? Settings.Default.MusicDirectory2 
                    : Settings.Default.MusicDirectory1;

                var copyTo = Path.Combine(destinationDirectory, Path.GetFileName(musicFile.FilePath));

                File.Copy(musicFile.FilePath, copyTo);
            }
        }
    }
}
