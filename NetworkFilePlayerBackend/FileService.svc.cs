using System;
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

        public string GetTime()
        {
            return AppDomain.CurrentDomain.BaseDirectory;
            return DateTime.Now.ToLongTimeString();
        }

        public (string files, int count)[] GetProvidedFiles()
        {
            try
            {
                return FileProvide.GetProvidedFiles().Select(p => (p.SrcPath, p.UserCount)).ToArray();
            }
            catch (Exception e)
            {
                Log(e.ToString());
                throw;
            }
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
