using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace NetworkFilePlayerBackend
{
    public class FileProvide
    {
        public static string BasePath = AppDomain.CurrentDomain.BaseDirectory;

        private static Dictionary<string, FileProvide> provideFiles = new Dictionary<string, FileProvide>();

        public static FileProvide GetInstance(string path)
        {
            lock (provideFiles)
            {
                if (provideFiles.TryGetValue(path, out FileProvide file)) return file;

                file = new FileProvide(path);
                provideFiles.Add(path, file);

                return file;
            }
        }

        public static FileProvide[] GetProvidedFiles()
        {
            lock (provideFiles)
            {
                return provideFiles.Values.Where(p => p.IsFileProvided).ToArray();
            }
        }

        private readonly object ioLockObj;

        public string SrcPath { get; private set; }

        public bool IsProvidingFile { get; private set; }

        public bool IsFileProvided { get; private set; }

        public string ID { get; private set; }

        public int UserCount { get; private set; }

        public FileStates States { get { return new FileStates(IsProvidingFile, IsFileProvided, UserCount, ID, SrcPath); } }

        private FileProvide(string path)
        {
            ioLockObj = new object();
            UserCount = 0;

            SrcPath = path;
            ID = GetID(Path.GetExtension(SrcPath));
        }

        public void ProvideOne()
        {
            lock (ioLockObj)
            {
                UserCount++;

                if (UserCount > 1) return;

                IsProvidingFile = true;

                File.Copy(SrcPath, GetIdPath());

                IsFileProvided = true;
                IsProvidingFile = false;
            }
        }

        private string GetIdPath()
        {
            return Path.Combine(BasePath, ID);
        }

        private static string GetID(string extension)
        {
            lock (provideFiles)
            {
                string id;

                do
                {
                    id = Path.GetRandomFileName() + extension;
                }
                while (provideFiles.Values.Any(p => p.ID == id));

                return id;
            }
        }

        public static void UnprovideAllFiles()
        {
            lock (provideFiles)
            {
                foreach (FileProvide file in provideFiles.Values) file.Unprovide();
            }
        }

        public void UnprovideOne()
        {
            lock (ioLockObj)
            {
                if (UserCount == 0) return;

                UserCount--;

                if (UserCount == 0)
                {
                    IsFileProvided = false;
                    File.Delete(GetIdPath());
                }
            }
        }

        public void Unprovide()
        {
            lock (ioLockObj)
            {
                if (UserCount == 0) return;

                UserCount = 0;
                IsFileProvided = IsProvidingFile = false;
                File.Delete(GetIdPath());
            }
        }

        ~FileProvide()
        {
            //FileService.Log("~FileProvide: " + SrcPath);

            try
            {
                Unprovide();
            }
            catch { }
        }
    }
}