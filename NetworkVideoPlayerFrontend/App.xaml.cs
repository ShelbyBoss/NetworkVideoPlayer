using NetworkVideoPlayerFrontend.VideoServiceReference;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace NetworkVideoPlayerFrontend
{
    /// <summary>
    /// Stellt das anwendungsspezifische Verhalten bereit, um die Standardanwendungsklasse zu ergänzen.
    /// </summary>
    sealed partial class App : Application
    {
        private static TimeSpan waitTimeSpan = TimeSpan.FromSeconds(1);
        public static string ServerAddress = "http://nas-server/filewcfservice/";
        private const string serviceName = "FileService.svc";

        private List<string> providedFiles;

        public FileServiceClient Service { get; private set; }

        /// <summary>
        /// Initialisiert das Singletonanwendungsobjekt. Dies ist die erste Zeile von erstelltem Code
        /// und daher das logische Äquivalent von main() bzw. WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Resuming += OnResuming;
            this.Suspending += OnSuspending;

            providedFiles = new List<string>();
        }

        private async void OnResuming(object sender, object e)
        {
            Debug.WriteLine("App_OnResume1");

            Service = CreateService();

            try
            {
                await Service.OpenAsync();
            }
            catch (Exception exc)
            {
                Debug.WriteLine("App_OnResumeFail1: " + exc);
            }

            try
            {
                foreach (string file in providedFiles)
                {
                    await Service.ProvideFileAsync(file);
                }
            }
            catch (Exception exc)
            {
                Debug.WriteLine("App_OnResumeFail2: " + exc);
            }

            Debug.WriteLine("App_OnResume2");
        }

        /// <summary>
        /// Wird aufgerufen, wenn die Anwendung durch den Endbenutzer normal gestartet wird. Weitere Einstiegspunkte
        /// werden z. B. verwendet, wenn die Anwendung gestartet wird, um eine bestimmte Datei zu öffnen.
        /// </summary>
        /// <param name="e">Details über Startanforderung und -prozess.</param>
        protected async override void OnLaunched(LaunchActivatedEventArgs e)
        {
            Service = CreateService();

            await Service.OpenAsync();

            Debug.Clear();

            Frame rootFrame = Window.Current.Content as Frame;

            // App-Initialisierung nicht wiederholen, wenn das Fenster bereits Inhalte enthält.
            // Nur sicherstellen, dass das Fenster aktiv ist.
            if (rootFrame == null)
            {
                // Frame erstellen, der als Navigationskontext fungiert und zum Parameter der ersten Seite navigieren
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Zustand von zuvor angehaltener Anwendung laden
                }

                // Den Frame im aktuellen Fenster platzieren
                Window.Current.Content = rootFrame;
            }

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    rootFrame.Navigate(typeof(MainPage), e.Arguments);
                }
                // Sicherstellen, dass das aktuelle Fenster aktiv ist
                Window.Current.Activate();
            }
        }

        /// <summary>
        /// Wird aufgerufen, wenn die Navigation auf eine bestimmte Seite fehlschlägt
        /// </summary>
        /// <param name="sender">Der Rahmen, bei dem die Navigation fehlgeschlagen ist</param>
        /// <param name="e">Details über den Navigationsfehler</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Wird aufgerufen, wenn die Ausführung der Anwendung angehalten wird.  Der Anwendungszustand wird gespeichert,
        /// ohne zu wissen, ob die Anwendung beendet oder fortgesetzt wird und die Speicherinhalte dabei
        /// unbeschädigt bleiben.
        /// </summary>
        /// <param name="sender">Die Quelle der Anhalteanforderung.</param>
        /// <param name="e">Details zur Anhalteanforderung.</param>
        private async void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();

            foreach (string file in providedFiles)
            {
                await Service.UnprovideFileAsync(file);
            }

            await Service?.CloseAsync();

            deferral.Complete();
        }

        private FileServiceClient CreateService()
        {
            //service = new FileServiceClient();
            //ServerAddress = service.Endpoint.Address.Uri.AbsoluteUri.Remove(22);

            BasicHttpBinding binding = new BasicHttpBinding();
            Uri endpointUri = new Uri(ServerAddress + serviceName);
            EndpointAddress endpoint = new EndpointAddress(endpointUri);

            return new FileServiceClient(binding, endpoint);
        }

        public async Task<string> ProvideFileAsync(string path)
        {
            //switch (Service.State)
            //{
            //    case CommunicationState.Closed:
            //        await Service.OpenAsync();
            //        break;

            //    case CommunicationState.Closing:
            //        while (Service.State == CommunicationState.Closing) await Task.Delay(100);
            //        await Service.OpenAsync();
            //        break;

            //    case CommunicationState.Created:
            //        await Service.OpenAsync();
            //        break;

            //    case CommunicationState.Faulted:
            //        await Service.CloseAsync();
            //        await Service.OpenAsync();
            //        break;

            //    case CommunicationState.Opening:
            //        while (Service.State == CommunicationState.Opening) await Task.Delay(100);
            //        break;
            //}

            providedFiles.Add(path);

            string id = await Service.StartProvideFileAsync(path);

            while (!await Service.IsFileProvidedAsync(path))
            {
                await Task.Delay(waitTimeSpan);
            }

            return id;
        }

        public async Task UnprovideFileAsync(string path)
        {
            providedFiles.Remove(path);
            await Service.UnprovideFileAsync(path);
        }
    }
}
