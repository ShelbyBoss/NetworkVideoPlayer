using System;
using System.Threading.Tasks;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

namespace NetworkVideoPlayerFrontend
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class VideoPage : Page
    {
        private static readonly TimeSpan removeUiWaitTime = TimeSpan.FromSeconds(3);

        private bool isUpdatingPosition, unloadMedia, loadMedia;
        private StorageItem item;
        private DateTime lastWaitStarted;
        private App app;

        public VideoPage()
        {
            this.InitializeComponent();

            app = (App)Application.Current;
        }

        private async void PlaybackSession_PlaybackStateChanged(MediaPlaybackSession sender, object args)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                sbiPlayPause.Symbol = sender.PlaybackState == MediaPlaybackState.Playing ? Symbol.Pause : Symbol.Play;
                rpb.IsActive = false;
            });
        }

        private async void PlaybackSession_PositionChangedAsync(MediaPlaybackSession sender, object args)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (isUpdatingPosition) return;

                isUpdatingPosition = true;
                sldPosition.Value = sender.Position.TotalSeconds;
                tblPosition.Text = ConvertToString(sender.Position);
                isUpdatingPosition = false;
            });
        }

        private async void PlaybackSession_NaturalDurationChanged(MediaPlaybackSession sender, object args)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                sldPosition.Maximum = sender.NaturalDuration.TotalSeconds;
                tblDuration.Text = ConvertToString(sender.NaturalDuration);
            });
        }

        private async void MediaPlayer_MediaEnded(MediaPlayer sender, object args)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => Frame.GoBack());
        }

        private async void MediaPlayer_MediaFailed(MediaPlayer sender, MediaPlayerFailedEventArgs args)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => Frame.GoBack());
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            rpb.Width = ActualWidth / 10;
            rpb.Height = ActualHeight / 10;
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            Debug.WriteLine("NaviTo");
            item = (StorageItem)e.Parameter;

            ApplicationView.GetForCurrentView().Title = item.Name;
        }

        protected async override void OnNavigatingFrom(NavigatingCancelEventArgs args)
        {
            Debug.WriteLine("NaviFrom");
            ExitFullscreenMode();

            await UnloadMedia();
        }

        public static string ConvertToString(TimeSpan span, bool includeMillis = false)
        {
            string text = string.Empty;
            int hours = (int)Math.Floor(span.TotalHours);

            if (hours >= 1) text += string.Format("{0,2}:", hours);

            text += string.Format("{0,2}:{1,2}", span.Minutes, span.Seconds);

            if (includeMillis) text += string.Format(":{0,2}", span.Milliseconds);

            return text.Replace(' ', '0');
        }

        private void Mpe_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            ToggleFullscreenMode();
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }

        private void BtnPlayPause_Click(object sender, RoutedEventArgs e)
        {
            TogglePlayState();
        }

        private void SldPosition_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (isUpdatingPosition) return;

            isUpdatingPosition = true;
            mpe.MediaPlayer.PlaybackSession.Position = TimeSpan.FromSeconds(e.NewValue);
            isUpdatingPosition = false;

            TryRemoveUI();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Loaded1");

            await LoadMedia();

            Window.Current.CoreWindow.KeyDown += CoreWindow_KeyDown;
            Window.Current.CoreWindow.PointerMoved += CoreWindow_PointerAction;
            Window.Current.CoreWindow.PointerPressed += CoreWindow_PointerAction;
            Window.Current.CoreWindow.PointerReleased += CoreWindow_PointerAction;

            Debug.WriteLine("Loaded2");
        }

        private async void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Unloaded1");
            await UnloadMedia();

            Window.Current.CoreWindow.KeyDown -= CoreWindow_KeyDown;
            Window.Current.CoreWindow.PointerMoved -= CoreWindow_PointerAction;
            Window.Current.CoreWindow.PointerPressed -= CoreWindow_PointerAction;
            Window.Current.CoreWindow.PointerReleased -= CoreWindow_PointerAction;
            Debug.WriteLine("Unloaded2");
        }

        private void CoreWindow_PointerAction(CoreWindow sender, PointerEventArgs e)
        {
            ViewUI();
        }

        private void TogglePlayState()
        {
            if (mpe.MediaPlayer.PlaybackSession.PlaybackState == MediaPlaybackState.Playing) mpe.MediaPlayer.Pause();
            else mpe.MediaPlayer.Play();
        }

        private void ToggleFullscreenMode()
        {
            ApplicationView view = ApplicationView.GetForCurrentView();

            if (view.IsFullScreenMode)
            {
                view.ExitFullScreenMode();
                mpe.IsFullWindow = false;
            }
            else
            {
                view.TryEnterFullScreenMode();
                TryRemoveUI();
            }
        }

        private void ExitFullscreenMode()
        {
            ApplicationView.GetForCurrentView().ExitFullScreenMode();
            mpe.IsFullWindow = false;
        }

        private async void TryRemoveUI()
        {
            ApplicationView view = ApplicationView.GetForCurrentView();
            if (!view.IsFullScreenMode) return;

            lastWaitStarted = DateTime.Now;

            await Task.Delay(removeUiWaitTime);

            if (!view.IsFullScreenMode || lastWaitStarted + removeUiWaitTime > DateTime.Now) return;

            mpe.IsFullWindow = true;
        }

        private void ViewUI()
        {
            mpe.IsFullWindow = false;

            TryRemoveUI();
        }

        private void CoreWindow_KeyDown(CoreWindow sender, KeyEventArgs args)
        {
            switch (args.VirtualKey)
            {
                case VirtualKey.F:
                    ToggleFullscreenMode();
                    break;

                case VirtualKey.Escape:
                    ExitFullscreenMode();
                    break;

                case VirtualKey.Space:
                    TogglePlayState();
                    break;
            }
        }

        private async Task LoadMedia()
        {
            try
            {
                Debug.WriteLine("LoadMedia: " + loadMedia);

                if (loadMedia) return;

                loadMedia = true;
                unloadMedia = false;

                string id = await app.ProvideFileAsync(item.Path);
                System.Diagnostics.Debug.WriteLine(id);

                if (unloadMedia) return;

                Uri uri = new Uri(App.ServerAddress + id);

                if (mpe.MediaPlayer == null) mpe.SetMediaPlayer(new MediaPlayer());

                mpe.MediaPlayer.MediaEnded += MediaPlayer_MediaEnded;
                mpe.MediaPlayer.MediaFailed += MediaPlayer_MediaFailed;

                mpe.Source = MediaSource.CreateFromUri(uri);

                if (mpe.MediaPlayer?.PlaybackSession != null)
                {
                    mpe.MediaPlayer.PlaybackSession.PlaybackStateChanged += PlaybackSession_PlaybackStateChanged;
                    mpe.MediaPlayer.PlaybackSession.PositionChanged += PlaybackSession_PositionChangedAsync;
                    mpe.MediaPlayer.PlaybackSession.NaturalDurationChanged += PlaybackSession_NaturalDurationChanged;
                }

                mpe.MediaPlayer.Play();
            }
            catch (Exception exc)
            {
                Debug.WriteLine("LoedMediaFail: \r\n" + exc);
                await new MessageDialog(exc.Message).ShowAsync();
                Frame.GoBack();
            }
        }

        private async Task UnloadMedia()
        {
            Debug.WriteLine("UnloadMedia: " + unloadMedia);
            if (unloadMedia) return;

            unloadMedia = true;
            loadMedia = false;

            if (mpe.MediaPlayer != null) mpe.MediaPlayer.MediaEnded -= MediaPlayer_MediaEnded;

            if (mpe.MediaPlayer?.PlaybackSession != null)
            {
                mpe.MediaPlayer.PlaybackSession.PlaybackStateChanged -= PlaybackSession_PlaybackStateChanged;
                mpe.MediaPlayer.PlaybackSession.PositionChanged -= PlaybackSession_PositionChangedAsync;
                mpe.MediaPlayer.PlaybackSession.NaturalDurationChanged -= PlaybackSession_NaturalDurationChanged;
            }

            mpe.Source = null;

            await app.UnprovideFileAsync(item.Path);
        }
    }
}
