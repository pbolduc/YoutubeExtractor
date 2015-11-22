namespace YoutubeDownloader.Modules.DownloadModule
{
    using Prism.Modularity;
    using Prism.Regions;
    using Views;

    public class DownloadModule : IModule
    {
        private readonly IRegionManager _regionManager;

        public DownloadModule(IRegionManager regionManager)
        {
            _regionManager = regionManager;
        }

        public void Initialize()
        {
            _regionManager.RegisterViewWithRegion("MainRegion", typeof(DownloadView));
        }
    }
}
