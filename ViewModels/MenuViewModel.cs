using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using GlobalPopup.Views;

namespace GlobalPopup.ViewModels
{
    public class MenuViewModel : BindableBase
    {
        private readonly IRegionManager _regionManager;
        private RegionManager _rm;

        public DelegateCommand NavigateCommand { get; private set; }

        public MenuViewModel(IRegionManager regionManager)
        {
            _regionManager = regionManager;
            _rm = regionManager as RegionManager;
            NavigateCommand = new DelegateCommand(Navigate);
        }

        private void Navigate()
        {
            var viewModel = new AnmeldFejlViewModel();
            var popUp = new AnmeldFejlPopupView() { DataContext = viewModel };
            if (AnmeldFejlPopupView.isOpen) return;
            popUp.Show();
        }
    }
}