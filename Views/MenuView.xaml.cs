using System.Windows.Controls;

namespace GlobalPopup.Views
{
    /// <summary>
    /// Interaction logic for MenuView.xaml
    /// </summary>
    public partial class MenuView : UserControl
    {
        public MenuView(ViewModels.MenuViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}

