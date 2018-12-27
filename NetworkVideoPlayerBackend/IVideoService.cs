using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace NetworkVideoPlayerBackend
{
    // HINWEIS: Mit dem Befehl "Umbenennen" im Menü "Umgestalten" können Sie den Schnittstellennamen "IVideoService" sowohl im Code als auch in der Konfigurationsdatei ändern.
    [ServiceContract]
    public interface IVideoService
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
