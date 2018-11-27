using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Die Elementvorlage "Leere Seite" wird unter https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x407 dokumentiert.

namespace NetworkVideoPlayer
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private const string pathFilename = "path.txt";
        private static readonly Brush errorPathForeground = new SolidColorBrush(Colors.Red);

        private StorageFolder folder;
        private readonly Brush defaultPathForeground;

        private StorageFolder Folder
        {
            get { return folder; }
            set
            {
                if (value?.Path == folder?.Path) return;

                folder = value;
                tblPath.Text = folder?.Path ?? string.Empty;

                UpdateFiles();
                //SaveCurrentFolderPath();
            }
        }

        public MainPage()
        {
            this.InitializeComponent();

            defaultPathForeground = tblPath.Foreground;

            System.Net.CredentialCache.DefaultNetworkCredentials.Domain = "Server";
            System.Net.CredentialCache.DefaultNetworkCredentials.UserName = "Server";
            System.Net.CredentialCache.DefaultNetworkCredentials.Password = "product1";
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            try
            {
                StorageFile file = await ApplicationData.Current.LocalFolder.GetFileAsync(pathFilename);
                string path = await FileIO.ReadTextAsync(file);

                Folder = await StorageFolder.GetFolderFromPathAsync(@"\\Nas-server\2tb\Tmp");
            }
            catch
            {
                Folder = KnownFolders.VideosLibrary;
            }
        }

        private async void UpdateFiles()
        {
            if (Folder == null)
            {
                lbxFolderContent.ItemsSource = Enumerable.Empty<IStorageItem>();
                return;
            }

            var foldersTask = Folder.GetFoldersAsync();
            var filesTask = Folder.GetFilesAsync();
            IReadOnlyList<StorageFolder> folders = await foldersTask;
            IReadOnlyList<StorageFile> files = await filesTask;

            lbxFolderContent.ItemsSource = folders.Cast<IStorageItem>().Concat(files);
        }

        private async void SaveCurrentFolderPath()
        {
            StorageFile file = null;

            try
            {
                file = await ApplicationData.Current.LocalFolder.GetFileAsync(pathFilename);
            }
            catch (FileNotFoundException)
            {
                try
                {
                    file = await ApplicationData.Current.LocalFolder.CreateFileAsync(pathFilename);
                }
                catch
                {
                    return;
                }
            }
            catch
            {
                return;
            }

            try
            {
                await FileIO.WriteTextAsync(file, Folder?.Path ?? string.Empty);
            }
            catch { }
        }

        //private async void Button_Tapped(object sender, TappedRoutedEventArgs e)
        //{
        //    FileOpenPicker picker = new FileOpenPicker();
        //    picker.ViewMode = PickerViewMode.List;
        //    picker.SuggestedStartLocation = PickerLocationId.VideosLibrary;
        //    picker.FileTypeFilter.Add(".mp4");
        //    picker.FileTypeFilter.Add(".avi");
        //    picker.FileTypeFilter.Add(".mkv");

        //    StorageFile file = await picker.PickSingleFileAsync();

        //    if (file != null) Frame.Navigate(typeof(VideoPage), file);
        //}

        private async void BtnToParent_Click(object sender, RoutedEventArgs e)
        {
            //Folder = await Folder.GetParentAsync();

            FileOpenPicker picker = new FileOpenPicker();
            picker.ViewMode = PickerViewMode.List;
            picker.SuggestedStartLocation = PickerLocationId.ComputerFolder;
            picker.FileTypeFilter.Add(".mp4");
            picker.FileTypeFilter.Add(".avi");
            picker.FileTypeFilter.Add(".mkv");

            StorageFile file = await picker.PickSingleFileAsync();
        }

        private void StackPanel_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            IStorageItem item = (IStorageItem)((FrameworkElement)sender).DataContext;

            if (item is StorageFolder) Folder = (StorageFolder)item;
            else if (item is StorageFile) Frame.Navigate(typeof(VideoPage), item);
        }
    }
}
