using System;
using System.Threading.Tasks;
using Windows.Storage;

namespace NetworkVideoPlayerFrontend
{
    static class Debug
    {
        private static StorageFile file;

        private static StorageFile GetFile()
        {
            if (file == null)
            {
                try
                {
                    Task<StorageFile> task = KnownFolders.VideosLibrary.GetFileAsync("NVP.txt").AsTask();
                    task.Wait();
                    file = task.Result;
                }
                catch
                {
                    Task<StorageFile> task = KnownFolders.VideosLibrary.CreateFileAsync("NVP.txt").AsTask();
                    task.Wait();
                    file = task.Result;
                }
            }

            return file;
        }

        public static void Clear()
        {
            FileIO.WriteTextAsync(GetFile(), string.Empty).AsTask().Wait();
        }

        public static void WriteLine(string line)
        {
            try
            {
                Task task = FileIO.AppendTextAsync(GetFile(), line + "\r\n").AsTask();

                System.Diagnostics.Debug.WriteLine(line);

                task.Wait();
            }
            catch { }
        }

        public static string GetText()
        {
            Task<string> task = FileIO.ReadTextAsync(GetFile()).AsTask();
            task.Wait();
            return task.Result;
        }
    }
}
