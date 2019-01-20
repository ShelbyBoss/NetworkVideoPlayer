using System;
using System.Runtime.Serialization;

namespace NetworkFilePlayerBackend
{
    [DataContract]
    public abstract class FileSystemItem
    {
        [DataMember]
        public bool Exists { get; private set; }

        [DataMember]
        public string Name { get; private set; }

        [DataMember]
        public string FullPath { get; private set; }

        [DataMember]
        public DateTime LastWriteTime { get; private set; }

        [DataMember]
        public DateTime CreateTime { get; private set; }

        [DataMember]
        public FileSystemAttributes Attributes { get; private set; }

        [DataMember]
        public string Parent { get; private set; }

        [DataMember]
        public string ParentFullPath { get; private set; }

        protected FileSystemItem(bool exists, string name, string fullPath, DateTime lastWriteTime,
            DateTime createTime, FileSystemAttributes attributes, string parent, string parentFullPath)
        {
            Exists = exists;
            Name = name;
            FullPath = fullPath;
            LastWriteTime = lastWriteTime;
            CreateTime = createTime;
            Attributes = attributes;
            Parent = parent;
            ParentFullPath = parentFullPath;
        }
    }
}
