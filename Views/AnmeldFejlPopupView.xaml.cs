using GlobalPopup.ViewModels;
using System;
using System.Windows;

namespace GlobalPopup.Views
{
    /// <summary>
    /// Interaction logic for AnmeldFejlPopup.xaml
    /// </summary>
    public partial class AnmeldFejlPopupView : Window
    {
        public static bool isOpen { get; private set; }
        
        public AnmeldFejlPopupView()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            isOpen = true;
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            isOpen = false;
        }
    }
}