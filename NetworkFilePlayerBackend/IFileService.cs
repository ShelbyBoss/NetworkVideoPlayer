using System.ServiceModel;

namespace NetworkVideoPlayerBackend
{
    // HINWEIS: Mit dem Befehl "Umbenennen" im Menü "Umgestalten" können Sie den Schnittstellennamen "IFileService" sowohl im Code als auch in der Konfigurationsdatei ändern.
    [ServiceContract]
    public interface IFileService
    {
        [OperationContract]
        string[] GetFiles(string path);

        [OperationContract]
        string[] GetDirectories(string path);

        [OperationContract]
        string ProvideFile(string path);

        [OperationContract]
        void UnprovideFile(string path);

        [OperationContract]
        string GetTime();
    }
}
