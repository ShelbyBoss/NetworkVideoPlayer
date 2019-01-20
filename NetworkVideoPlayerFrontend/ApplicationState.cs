using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;

namespace NetworkVideoPlayerFrontend
{
    public class ApplicationState
    {
        public string DirectoryPath { get; set; }

        public string VideoPath { get; set; }

        public TimeSpan Position { get; set; }

        public async Task Save(StorageFile file)
        {
            string[] data = new string[] { DirectoryPath, VideoPath, Position.Ticks.ToString() };

            await FileIO.WriteLinesAsync(file, data);
        }

        public async static Task<ApplicationState> Load(StorageFile file)
        {
            string[] data = (await FileIO.ReadLinesAsync(file)).ToArray();

            return new ApplicationState()
            {
                DirectoryPath = data[0],
                VideoPath = data[1],
                Position = new TimeSpan(int.Parse(data[2]))
            };
        }

        public string GetVideoName()
        {
            return VideoPath.Split('\\').LastOrDefault();
        }

        public string GetPositionAsString()
        {
            return ConvertToString(Position);
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
    }
}
