using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace WinYoutubeExtractor
{
    using System.IO;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using YoutubeExtractor;

    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private async void DownloadButton_Click(object sender, EventArgs e)
        {
            try
            {
                await Download(UrlTextBox.Text.Trim());
            }
            catch (AggregateException exception)
            {
                OutputTextBox.Text = exception.InnerException.ToString();
            }
        }

        private async Task Download(string url)
        {
            DownloadProgressBar.Value = 0;
            IEnumerable<VideoInfo> videoInfos = DownloadUrlResolver.GetDownloadUrls(url, false);

            var videos = videoInfos.Where(Filter).ToList();

            OutputTextBox.Text = string.Join(Environment.NewLine, videos.Select(FormatVideoInfo).ToArray());

            if (true)
            {
                var video = videos.First();
                if (video.RequiresDecryption)
                {
                    DownloadUrlResolver.DecryptDownloadUrl(video);
                }

                /*
                 * Create the video downloader.
                 * The first argument is the video to download.
                 * The second argument is the path to save the video file.
                 */
                var videoDownloader = new VideoDownloader(video,
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    RemoveIllegalPathCharacters(video.Title) + video.VideoExtension));

                // Register the ProgressChanged event and print the current progress
                videoDownloader.DownloadProgressChanged += VideoDownloaderOnDownloadProgressChanged;

                /*
                 * Execute the video downloader.
                 * For GUI applications note, that this method runs synchronously.
                 */
                await videoDownloader.ExecuteAsync();
            }

        }

        private void VideoDownloaderOnDownloadProgressChanged(object sender, ProgressEventArgs progressEventArgs)
        {
            DownloadProgressBar.Value = (int) progressEventArgs.ProgressPercentage;
        }

        private string FormatVideoInfo(VideoInfo video)
        {
            return video.ToString();
        }

        private bool Filter(VideoInfo video)
        {
            return 
                video.Resolution >= 720 && 
                video.VideoType == VideoType.Mp4 && 
                video.AdaptiveType == AdaptiveType.None;
        }

        private static string RemoveIllegalPathCharacters(string path)
        {
            string regexSearch = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
            var r = new Regex(string.Format("[{0}]", Regex.Escape(regexSearch)));
            return r.Replace(path, "");
        }
    }
}
