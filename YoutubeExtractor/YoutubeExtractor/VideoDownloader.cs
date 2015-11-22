using System;
using System.IO;
using System.Net;

namespace YoutubeExtractor
{
    using System.Threading.Tasks;

    /// <summary>
    /// Provides a method to download a video from YouTube.
    /// </summary>
    public class VideoDownloader : Downloader
    {
        private const int BufferSize = 4096;

        /// <summary>
        /// Initializes a new instance of the <see cref="VideoDownloader"/> class.
        /// </summary>
        /// <param name="video">The video to download.</param>
        /// <param name="savePath">The path to save the video.</param>
        /// <param name="bytesToDownload">An optional value to limit the number of bytes to download.</param>
        /// <exception cref="ArgumentNullException"><paramref name="video"/> or <paramref name="savePath"/> is <c>null</c>.</exception>
        public VideoDownloader(VideoInfo video, string savePath, int? bytesToDownload = null)
            : base(video, savePath, bytesToDownload)
        { }

        /// <summary>
        /// Occurs when the downlaod progress of the video file has changed.
        /// </summary>
        public event EventHandler<ProgressEventArgs> DownloadProgressChanged;

        /// <summary>
        /// Starts the video download.
        /// </summary>
        /// <exception cref="IOException">The video file could not be saved.</exception>
        /// <exception cref="WebException">An error occured while downloading the video.</exception>
        public override void Execute()
        {
            this.OnDownloadStarted(EventArgs.Empty);

            var request = (HttpWebRequest)WebRequest.Create(this.Video.DownloadUrl);

            if (this.BytesToDownload.HasValue)
            {
                request.AddRange(0, this.BytesToDownload.Value - 1);
            }

            // the following code is alternative, you may implement the function after your needs
            using (WebResponse response = request.GetResponse())
            {
                using (Stream source = response.GetResponseStream())
                {
                    using (FileStream target = File.Open(this.SavePath, FileMode.Create, FileAccess.Write))
                    {
                        var buffer = new byte[BufferSize];
                        bool cancel = false;
                        int bytes;
                        int copiedBytes = 0;

                        while (!cancel && (bytes = source.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            target.Write(buffer, 0, bytes);
                            copiedBytes += bytes;
                            cancel = OnDownloadProgressChanged(copiedBytes, response.ContentLength);
                        }
                    }
                }
            }

            this.OnDownloadFinished(EventArgs.Empty);
        }

        public override async Task ExecuteAsync()
        {
            this.OnDownloadStarted(EventArgs.Empty);
            var request = (HttpWebRequest)WebRequest.Create(this.Video.DownloadUrl);

            if (this.BytesToDownload.HasValue)
            {
                request.AddRange(0, this.BytesToDownload.Value - 1);
            }

            using (WebResponse response = await request.GetResponseAsync())
            {
                using (Stream source = response.GetResponseStream())
                {
                    using (FileStream target = File.Open(this.SavePath, FileMode.Create, FileAccess.Write))
                    {
                        var buffer = new byte[BufferSize];
                        bool cancel = false;
                        int bytes;
                        int copiedBytes = 0;
                        
                        while (!cancel && (bytes = await source.ReadAsync(buffer, 0, buffer.Length)) > 0)
                        {
                            await target.WriteAsync(buffer, 0, bytes);
                            copiedBytes += bytes;
                            cancel = OnDownloadProgressChanged(copiedBytes, response.ContentLength);
                        }
                    }
                }
            }
        }

        private bool OnDownloadProgressChanged(int position, long length)
        {
            bool cancel = false;

            var downloadProgressChanged = DownloadProgressChanged;
            if (downloadProgressChanged != null)
            {
                var eventArgs = new ProgressEventArgs((position * 1.0 / length) * 100);
                downloadProgressChanged(this, eventArgs);

                if (eventArgs.Cancel)
                {
                    cancel = true;
                }
            }

            return cancel;
        }
    }
}