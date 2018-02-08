using InfraStructure;
using Microsoft.Practices.Unity;
using Prism.Modularity;
using Prism.Regions;

namespace GlobalPopup
{
    public class GlobalPopupModule : IModule
    {
        protected IRegionManager _regionManager { get; private set; }
        protected IUnityContainer _container { get; private set; }

        public GlobalPopupModule(IUnityContainer container, IRegionManager regionManager)
        {
            _container = container;
            _regionManager = regionManager;
        }

        public void Initialize()
        {
            _regionManager.RegisterViewWithRegion(RegionNames.ContentRegion, typeof(Views.AnmeldFejlPopupView));
            _regionManager.RegisterViewWithRegion(RegionNames.MenuRegion, typeof(Views.MenuView));
        }
    }
}