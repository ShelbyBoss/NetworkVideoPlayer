using System.ServiceModel;

namespace NetworkFilePlayerBackend
{
    [ServiceContract]
    public interface IFileService
    {
        [OperationContract]
        string[] GetFiles(string path);

        [OperationContract]
        string[] GetFilesPage(string path,int pageSize, int pageIndex);

        [OperationContract]
        FileProperties GetFileProperties(string path);

        [OperationContract]
        string[] GetDirectories(string path);

        [OperationContract]
        string[] GetDirectoriesPage(string path, int pageSize, int pageIndex);

        [OperationContract]
        DirectoryProperties GetDirectoryProperties(string path);

        [OperationContract]
        FileStates GetFileStates(string path);

        [OperationContract]
        string StartProvideFile(string path);

        [OperationContract]
        string ProvideFile(string path);

        [OperationContract]
        void StartUnprovideFile(string path);

        [OperationContract]
        void UnprovideFile(string path);

        [OperationContract]
        void UnprovideFileForAll(string path);

        [OperationContract]
        void StartUnprovideFileForAll(string path);

        [OperationContract]
        FileStates[] GetProvidedFiles();

        [OperationContract]
        FileStates[] GetProvidedFilesPage(int pageSize, int pageIndex);

        [OperationContract]
        string GetBasePath();

        [OperationContract]
        string GetTime();
    }
}
