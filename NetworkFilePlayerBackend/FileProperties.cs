using System.IO;
using System.Runtime.Serialization;

namespace NetworkFilePlayerBackend
{
    [DataContract]
    public class FileProperties : FileSystemItem
    {
        [DataMember]
        public string Extension { get; private set; }

        [DataMember]
        public long Length { get; private set; }

        internal FileProperties(FileInfo file) :
            base(file.Exists, file.Name, file.FullName, file.LastWriteTime, file.CreationTime,
                GetFileSystemAttributes(file.Attributes), file.DirectoryName, file.Directory.FullName)
        {
            Extension = file.Extension;
            Length = file.Length;
        }

        public FileProperties(string path):this(new FileInfo(path))
        {
        }

        private static FileSystemAttributes GetFileSystemAttributes(FileAttributes attributes)
        {
            return (FileSystemAttributes)(int)attributes;
        }
    }
}