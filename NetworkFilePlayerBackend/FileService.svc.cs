using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NetworkVideoPlayerBackend
{
    public class FileService : IFileService
    {
        public FileService()
        {
            Log("FileServiceCtor: " + DateTime.Now.ToLongTimeString());
        }

        public string[] GetFiles(string path)
        {
            try
            {
                return Directory.GetFiles(path);
            }
            catch (Exception e)
            {
                Log(e.ToString());
                throw;
            }
        }

        public string[] GetFilesPage(string path, int pageSize, int pageIndex)
        {
            try
            {
                return Directory.GetFiles(path).Skip(pageSize * pageIndex).Take(pageSize).ToArray();
            }
            catch (Exception e)
            {
                Log(e.ToString());
                throw;
            }
        }

        public string[] GetDirectories(string path)
        {
            try
            {
                return Directory.GetDirectories(path);
            }
            catch (Exception e)
            {
                Log(e.ToString());
                throw;
            }
        }

        public string[] GetDirectoriesPage(string path, int pageSize, int pageIndex)
        {
            try
            {
                return Directory.GetDirectories(path).Skip(pageSize * pageIndex).Take(pageSize).ToArray();
            }
            catch (Exception e)
            {
                Log(e.ToString());
                throw;
            }
        }

        public bool IsFileProvided(string path)
        {
            try
            {
                return FileProvide.GetInstance(path).IsFileProvided;
            }
            catch (Exception e)
            {
                Log(e.ToString());
                throw;
            }
        }

        public bool IsProvidingFile(string path)
        {
            try
            {
                return FileProvide.GetInstance(path).IsProvidingFile;
            }
            catch (Exception e)
            {
                Log(e.ToString());
                throw;
            }
        }

        public string StartProvideFile(string path)
        {
            try
            {
                FileProvide file = FileProvide.GetInstance(path);
                file.StartProvideOne();

                return file.ID;
            }
            catch (Exception e)
            {
                Log(e.ToString());
                throw;
            }
        }

        public string ProvideFile(string path)
        {
            try
            {
                FileProvide file = FileProvide.GetInstance(path);
                file.ProvideOne();

                return file.ID;
            }
            catch (Exception e)
            {
                Log(e.ToString());
                throw;
            }
        }

        public string GetFileId(string path)
        {
            try
            {
                return FileProvide.GetInstance(path).ID;
            }
            catch (Exception e)
            {
                Log(e.ToString());
                throw;
            }
        }

        public void UnprovideFile(string path)
        {
            try
            {
                FileProvide file = FileProvide.GetInstance(path);
                file.UnprovideOne();
            }
            catch (Exception e)
            {
                Log(e.ToString());
                throw;
            }
        }

        public void UnprovideFileForAll(string path)
        {
            try
            {
                FileProvide file = FileProvide.GetInstance(path);
                file.Unprovide();
            }
            catch (Exception e)
            {
                Log(e.ToString());
                throw;
            }
        }

        public (string files, string id, int count)[] GetProvidedFiles()
        {
            try
            {
                return FileProvide.GetProvidedFiles().Select(p => (p.SrcPath, p.ID, p.UserCount)).ToArray();
            }
            catch (Exception e)
            {
                Log(e.ToString());
                throw;
            }
        }

        public (string files, string id, int count)[] GetProvidedFilesPage(int pageSize, int pageIndex)
        {
            try
            {
                IEnumerable<FileProvide> files = FileProvide.GetProvidedFiles().Skip(pageSize * pageIndex).Take(pageSize);
                return files.Select(p => (p.SrcPath, p.ID, p.UserCount)).ToArray();
            }
            catch (Exception e)
            {
                Log(e.ToString());
                throw;
            }
        }

        public string GetBasePath()
        {
            return FileProvide.BasePath;
        }

        public string GetTime()
        {
            return DateTime.Now.ToLongTimeString();
        }

        public static void Log(string text)
        {
            try
            {
                File.AppendAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "service.log"), text + "\r\n");
            }
            catch { }
        }
    }
}
