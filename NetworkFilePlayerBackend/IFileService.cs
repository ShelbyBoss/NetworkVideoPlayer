using System.ServiceModel;

namespace NetworkVideoPlayerBackend
{
    [ServiceContract]
    public interface IFileService
    {
        [OperationContract]
        string[] GetFiles(string path);

        [OperationContract]
        string[] GetFilesPage(string path,int pageSize, int pageIndex);

        [OperationContract]
        string[] GetDirectories(string path);

        [OperationContract]
        string[] GetDirectoriesPage(string path, int pageSize, int pageIndex);

        [OperationContract]
        bool IsFileProvided(string path);

        [OperationContract]
        bool IsProvidingFile(string path);

        [OperationContract]
        string StartProvideFile(string path);

        [OperationContract]
        string ProvideFile(string path);

        [OperationContract]
        string GetFileId(string path);

        [OperationContract]
        void UnprovideFile(string path);

        [OperationContract]
        void UnprovideFileForAll(string path);

        [OperationContract]
        (string files, string id, int count)[] GetProvidedFiles();

        [OperationContract]
        (string files, string id, int count)[] GetProvidedFilesPage(int pageSize, int pageIndex);

        [OperationContract]
        string GetBasePath();

        [OperationContract]
        string GetTime();
    }
}
