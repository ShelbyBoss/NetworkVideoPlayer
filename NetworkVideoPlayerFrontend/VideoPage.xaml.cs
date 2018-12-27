using NetworkVideoPlayerFrontend.VideoServiceReference;
using System;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.UI.Core;
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
        private bool isUpdatingPosition;
        private StorageItem item;
        private FileServiceClient service;

        public VideoPage()
        {
            this.InitializeComponent();

            service = ((App)Application.Current).Service;
        }

        private async void PlaybackSession_PlaybackStateChanged(MediaPlaybackSession sender, object args)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                () => sbiPlayPause.Symbol = sender.PlaybackState == MediaPlaybackState.Playing ? Symbol.Pause : Symbol.Play);
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

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            item = (StorageItem)e.Parameter;
            tblName.Text = item.Name;

            string id = await service.ProvideFileAsync(item.Path);
            System.Diagnostics.Debug.WriteLine(id);

            Uri uri = new Uri(App.ServerAddress + id);

            mpe.Source = MediaSource.CreateFromUri(uri);

            mpe.MediaPlayer.MediaEnded += MediaPlayer_MediaEnded;

            mpe.MediaPlayer.PlaybackSession.PlaybackStateChanged += PlaybackSession_PlaybackStateChanged;
            mpe.MediaPlayer.PlaybackSession.PositionChanged += PlaybackSession_PositionChangedAsync;
            mpe.MediaPlayer.PlaybackSession.NaturalDurationChanged += PlaybackSession_NaturalDurationChanged;

            mpe.MediaPlayer.Play();
        }

        protected async override void OnNavigatingFrom(NavigatingCancelEventArgs args)
        {
            mpe.MediaPlayer.MediaEnded -= MediaPlayer_MediaEnded;

            mpe.MediaPlayer.PlaybackSession.PlaybackStateChanged -= PlaybackSession_PlaybackStateChanged;
            mpe.MediaPlayer.PlaybackSession.PositionChanged -= PlaybackSession_PositionChangedAsync;
            mpe.MediaPlayer.PlaybackSession.NaturalDurationChanged -= PlaybackSession_NaturalDurationChanged;

            mpe.Source = null;

            await service.UnprovideFileAsync(item.Path);
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

        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }

        private void BtnPlayPause_Click(object sender, RoutedEventArgs e)
        {
            if (mpe.MediaPlayer.PlaybackSession.PlaybackState == MediaPlaybackState.Playing) mpe.MediaPlayer.Pause();
            else mpe.MediaPlayer.Play();
        }

        private void SldPosition_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (isUpdatingPosition) return;

            isUpdatingPosition = true;
            mpe.MediaPlayer.PlaybackSession.Position = TimeSpan.FromSeconds(e.NewValue);
            isUpdatingPosition = false;
        }
    }
}
