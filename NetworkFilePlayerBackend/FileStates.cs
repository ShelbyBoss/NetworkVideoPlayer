using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace NetworkFilePlayerBackend
{
    [DataContract]
    public struct FileStates
    {
        [DataMember]
        public bool IsProvidingFile { get; private set; }

        [DataMember]
        public bool IsFileProvided { get; private set; }

        [DataMember]
        public int UserCount { get; private set; }

        [DataMember]
        public string ID { get; private set; }

        [DataMember]
        public string Path { get; private set; }

        public FileStates(bool isProvidingFile, bool isFileProvided, int userCount, string iD, string path) : this()
        {
            IsProvidingFile = isProvidingFile;
            IsFileProvided = isFileProvided;
            UserCount = userCount;
            ID = iD;
            Path = path;
        }
    }
}