using System.Windows;

namespace YoutubeDownloader
{
    using Prism;

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            Bootstrapper bs = new DefaultStructureMapBootstrapper();
            bs.Run();
        }
    }
}
