using System;
using System.IO;

namespace NetworkVideoPlayerBackend
{
    public class FileService : IFileService
    {
        public string[] GetFiles(string path)
        {
            return Directory.GetFiles(path);
        }

        public string[] GetDirectories(string path)
        {
            return Directory.GetDirectories(path);
        }

        public string ProvideFile(string path)
        {
            FileProvide file = FileProvide.GetInstance(path);
            file.Provide(this);

            return file.ID;
        }

        public void UnprovideFile(string path)
        {
            FileProvide file = FileProvide.GetInstance(path);
            file.Unprovide(this);
        }

        public string GetTime()
        {
            return DateTime.Now.ToLongTimeString();
        }

        ~FileService()
        {
            FileProvide.UnprovideForAll(this);
        }
    }
}
