using System.IO;
using System.Runtime.Serialization;

namespace NetworkFilePlayerBackend
{
    [DataContract]
    public class DirectoryProperties : FileSystemItem
    {
        public DirectoryProperties(DirectoryInfo dir) : base(dir.Exists, dir.Name, dir.FullName, dir.LastWriteTime,
            dir.CreationTime, GetFileSystemAttributes(dir.Attributes), dir.Parent.Name, dir.FullName)
        {
        }

        public DirectoryProperties(string path) : this(new DirectoryInfo(path))
        {
        }

        private static FileSystemAttributes GetFileSystemAttributes(FileAttributes attributes)
        {
            return (FileSystemAttributes)(int)attributes;
        }
    }
}