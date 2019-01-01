using NetworkVideoPlayerFrontend.VideoServiceReference;
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Storage;
using Windows.System;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace NetworkVideoPlayerFrontend
{
    public sealed partial class MainPage : Page
    {
        private const string pathFilename = "path.txt";
        private static readonly Brush errorPathForeground = new SolidColorBrush(Colors.Red);

        private static string directoryPath;

        private readonly Brush defaultTbxPathForeground, defaultTblPathForeground;
        private FileServiceClient service;
        private Exception updateFilesDirectoriesExeption;

        private string DirectoryPath
        {
            get { return directoryPath; }
            set
            {
                if (value == directoryPath) return;

                directoryPath = value;
                tblPath.Text = tbxPath.Text = directoryPath;

                UpdateStorageItems();
            }
        }

        public MainPage()
        {
            this.InitializeComponent();

            service = ((App)Application.Current).Service;

            defaultTbxPathForeground = tbxPath.Foreground;
            defaultTblPathForeground = tblPath.Foreground;

            System.Diagnostics.Debug.WriteLine("MainPageCtor");
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            ApplicationView.GetForCurrentView().Title = string.Empty;

            try
            {
                System.Diagnostics.Debug.WriteLine(await service.GetTimeAsync());
            }
            catch (Exception exc)
            {
                MessageDialog dialog = new MessageDialog(exc.ToString());
                await dialog.ShowAsync();
            }

            try
            {
                if (string.IsNullOrWhiteSpace(directoryPath))
                {
                    StorageFile file = await ApplicationData.Current.LocalFolder.GetFileAsync(pathFilename);
                    DirectoryPath = await FileIO.ReadTextAsync(file);
                }
                else
                {
                    tblPath.Text = tbxPath.Text = directoryPath;
                    UpdateStorageItems();
                }
            }
            catch
            {
                DirectoryPath = @"E:\";
            }
        }

        private async void UpdateStorageItems()
        {
            try
            {
                rpb.IsActive = true;

                var dirsTask = service.GetDirectoriesAsync(DirectoryPath);
                var filesTask = service.GetFilesAsync(DirectoryPath);
                StorageItem[] folders = (await dirsTask).Select(p => StorageItem.GetDirectory(p)).ToArray();
                StorageItem[] files = (await filesTask).Select(p => StorageItem.GetFile(p)).ToArray();

                lbxFolderContent.ItemsSource = folders.Concat(files);

                tbxPath.Foreground = defaultTbxPathForeground;
                tblPath.Foreground = defaultTblPathForeground;
                btnError.Visibility = Visibility.Collapsed;
            }
            catch (Exception e)
            {
                updateFilesDirectoriesExeption = e;

                lbxFolderContent.ItemsSource = Enumerable.Empty<StorageItem>();

                tbxPath.Foreground = tblPath.Foreground = errorPathForeground;
                btnError.Visibility = Visibility.Visible;
            }

            rpb.IsActive = false;
        }

        private void BtnToParent_Click(object sender, RoutedEventArgs e)
        {
            string path = DirectoryPath.TrimEnd('\\');
            int index = path.LastIndexOf('\\');

            if (index > 0) DirectoryPath = path.Remove(index + 1);
        }

        private void StackPanel_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            StorageItem item = (StorageItem)((FrameworkElement)sender).DataContext;

            if (item.IsDirectory) DirectoryPath = item.Path;
            else if (item.IsFile) Frame.Navigate(typeof(VideoPage), item);
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            rpb.Width = ActualWidth / 10;
            rpb.Height = ActualHeight / 10;
        }

        private void TblPath_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            tbxPath.Visibility = Visibility.Visible;
            tblPath.Visibility = Visibility.Collapsed;
        }

        private void TbxPath_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Escape)
            {
                tblPath.Visibility = Visibility.Visible;
                tbxPath.Visibility = Visibility.Collapsed;
            }
        }

        private void TbxPath_TextChanged(object sender, TextChangedEventArgs e)
        {
            DirectoryPath = tbxPath.Text;
        }

        private void LbxFolderContent_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lbxFolderContent.SelectedItem == null) return;

            StorageItem item = (StorageItem)lbxFolderContent.SelectedItem;

            if (item.IsDirectory) DirectoryPath = item.Path;
            else if (item.IsFile) Frame.Navigate(typeof(VideoPage), item);
        }

        private async void BtnError_Click(object sender, RoutedEventArgs e)
        {
            MessageDialog dialog = new MessageDialog(updateFilesDirectoriesExeption.ToString());
            await dialog.ShowAsync();
        }

        private async void Grid_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            string text = Debug.GetText();

            await new MessageDialog("Debug:\r\n" + text).ShowAsync();

            var files = await service.GetProvidedFilesAsync();
            text = string.Join("\r\n", files.Select(f => f.Item1 + ": " + f.Item2));

            await new MessageDialog("ProvidedFiles:\r\n" + text).ShowAsync();
        }

        private async void BtnSavePath_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                StorageFile file = await ApplicationData.Current.LocalFolder.GetFileAsync(pathFilename);
                await FileIO.WriteTextAsync(file, DirectoryPath);
            }
            catch (FileNotFoundException) { }
            catch (Exception exc)
            {
                MessageDialog dialog = new MessageDialog(exc.ToString());
                await dialog.ShowAsync();
                return;
            }

            try
            {
                StorageFile file = await ApplicationData.Current.LocalFolder.CreateFileAsync(pathFilename);
                await FileIO.WriteTextAsync(file, DirectoryPath);
            }
            catch (Exception exc)
            {
                MessageDialog dialog = new MessageDialog(exc.ToString());
                await dialog.ShowAsync();
            }
        }
    }
}
