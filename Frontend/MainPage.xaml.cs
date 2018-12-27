using NetworkVideoPlayer.VideoServiceReference;
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
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

        private readonly Brush defaultPathForeground;
        private VideoServiceClient service;
        private string directoryPath;

        private string DirectoryPath
        {
            get { return directoryPath; }
            set
            {
                if (value == directoryPath) return;

                directoryPath = value;
                tblPath.Text = directoryPath;

                UpdateStorageItems();
            }
        }

        public MainPage()
        {
            this.InitializeComponent();

            defaultPathForeground = tblPath.Foreground;
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            service = e.Parameter as VideoServiceClient;

            await service.OpenAsync();
            try
            {
                System.Diagnostics.Debug.WriteLine(await service.GetTimeAsync());
            }
            catch (Exception exc)
            {
                MessageDialog dialog = new MessageDialog(exc.Message);
                await dialog.ShowAsync();
            }

            try
            {
                StorageFile file = await ApplicationData.Current.LocalFolder.GetFileAsync(pathFilename);
                DirectoryPath = await FileIO.ReadTextAsync(file);
            }
            catch
            {
                DirectoryPath = @"D:\Clemens\Videos\Filme & Serien";
            }
        }

        private async void UpdateStorageItems()
        {
            try
            {
                var dirsTask = service.GetDirectoriesAsync(DirectoryPath);
                var filesTask = service.GetFilesAsync(DirectoryPath);
                StorageItem[] folders = (await dirsTask).Select(p => StorageItem.GetDirectory(p)).ToArray();
                StorageItem[] files = (await filesTask).Select(p => StorageItem.GetFile(p)).ToArray();

                lbxFolderContent.ItemsSource = folders.Concat(files);
            }
            catch (Exception e)
            {
                lbxFolderContent.ItemsSource = Enumerable.Empty<StorageItem>();

                MessageDialog dialog = new MessageDialog(e.Message);
                await dialog.ShowAsync();
            }
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
                await FileIO.WriteTextAsync(file, DirectoryPath);
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

        private void BtnToParent_Click(object sender, RoutedEventArgs e)
        {
            int index = DirectoryPath.LastIndexOf('\\');

            if (index > 0) DirectoryPath = DirectoryPath.Remove(index + 1);
        }

        private void StackPanel_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            StorageItem item = (StorageItem)((FrameworkElement)sender).DataContext;

            if (item.IsDirectory) DirectoryPath = item.Path;
            else if (item.IsFile) Frame.Navigate(typeof(VideoPage), item);
        }
    }
}
