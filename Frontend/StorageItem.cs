using System.Linq;

namespace NetworkVideoPlayer
{
    struct StorageItem
    {
        public bool IsFile { get; private set; }

        public bool IsDirectory { get; private set; }

        public string Name { get; private set; }

        public string Path { get; private set; }

        public static StorageItem GetFile(string path)
        {
            return new StorageItem()
            {
                IsFile = true,
                IsDirectory = false,
                Name = path.Split('\\').Last(p => p.Length > 0),
                Path = path
            };
        }

        public static StorageItem GetDirectory(string path)
        {
            return new StorageItem()
            {
                IsFile = false,
                IsDirectory = true,
                Name = path.Split('\\').Last(p => p.Length > 0),
                Path = path
            };
        }
    }
}
