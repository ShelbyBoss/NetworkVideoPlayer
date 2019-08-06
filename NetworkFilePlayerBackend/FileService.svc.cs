using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace NetworkFilePlayerBackend
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

        public FileProperties GetFileProperties(string path)
        {
            try
            {
                return new FileProperties(path);
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

        public DirectoryProperties GetDirectoryProperties(string path)
        {
            try
            {
                return new DirectoryProperties(path);
            }
            catch (Exception e)
            {
                Log(e.ToString());
                throw;
            }
        }

        public FileStates GetFileStates(string path)
        {
            try
            {
                return FileProvide.GetInstance(path).States;
            }
            catch (Exception e)
            {
                Log(e.ToString());
                throw;
            }
        }

        public byte[] GetMD5FileHash(string path)
        {
            using (MD5 md5 = MD5.Create())
            {
                using (FileStream stream = File.OpenRead(path))
                {
                    return md5.ComputeHash(stream);
                }
            }
        }

        public string StartProvideFile(string path)
        {
            try
            {
                FileProvide file = FileProvide.GetInstance(path);
                Task.Run(new Action(file.ProvideOne));

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

        public void StartUnprovideFile(string path)
        {
            try
            {
                FileProvide file = FileProvide.GetInstance(path);
                Task.Run(new Action(file.UnprovideOne));
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

        public void StartUnprovideFileForAll(string path)
        {
            try
            {
                FileProvide file = FileProvide.GetInstance(path);
                Task.Run(new Action(file.Unprovide));
            }
            catch (Exception e)
            {
                Log(e.ToString());
                throw;
            }
        }

        public FileStates[] GetProvidedFiles()
        {
            try
            {
                return FileProvide.GetProvidedFiles().Select(p => p.States).ToArray();
            }
            catch (Exception e)
            {
                Log(e.ToString());
                throw;
            }
        }

        public FileStates[] GetProvidedFilesPage(int pageSize, int pageIndex)
        {
            try
            {
                IEnumerable<FileProvide> files = FileProvide.GetProvidedFiles().Skip(pageSize * pageIndex).Take(pageSize);
                return files.Select(p => p.States).ToArray();
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
