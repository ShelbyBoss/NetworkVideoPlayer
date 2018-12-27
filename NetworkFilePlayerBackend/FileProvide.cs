using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NetworkVideoPlayerBackend
{
    public class FileProvide
    {
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
                return provideFiles.Values.Where(p => p.UserCount > 0).ToArray();
            }
        }

        private readonly object ioLockObj;
        private readonly List<object> users;

        public string SrcPath { get; private set; }

        public bool IsProvidingFile { get; private set; }

        public bool IsFileProvidedFinish { get; private set; }

        public string ID { get; private set; }

        public int UserCount { get { return users.Count; } }

        private FileProvide(string path)
        {
            ioLockObj = new object();
            users = new List<object>();

            SrcPath = path;
        }

        public void Provide(object user)
        {
            lock (ioLockObj)
            {
                if (!users.Contains(user)) users.Add(user);

                if (IsFileProvidedFinish) return;

                if (string.IsNullOrWhiteSpace(ID)) ID = GetID(Path.GetExtension(SrcPath));

                File.Copy(SrcPath, ID);
            }
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

        public static void UnprovideForAll(object user)
        {
            lock (provideFiles)
            {
                foreach (FileProvide file in provideFiles.Values) file.Unprovide(user);
            }
        }

        public void Unprovide(object user)
        {
            lock (ioLockObj)
            {
                users.Remove(user);

                if (users.Count == 0) File.Delete(ID);
            }
        }

        public void UnprovideForAll()
        {
            lock (ioLockObj)
            {
                users.Clear();

                File.Delete(ID);
            }
        }
    }
}