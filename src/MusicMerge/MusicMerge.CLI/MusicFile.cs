using MusicMerge.CLI.Enums;
using System.IO;

namespace MusicMerge.CLI
{
    public class MusicFile
    {
        public MusicFile(string filePath, MusicDirectory directory)
        {
            FilePath = filePath;
            Name = Path.GetFileNameWithoutExtension(filePath);
            Directory = directory;
        }

        public string FilePath { get; }
        public string Name { get; }
        public MusicDirectory Directory { get; }
    }
}
