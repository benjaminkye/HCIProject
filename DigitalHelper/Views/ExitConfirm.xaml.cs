using System;
using System.Windows;
using System.Windows.Controls;
namespace DigitalHelper.Views
{
    public partial class ExitConfirm : Page
    {
        public ExitConfirm()
        {
            InitializeComponent();
        }
        private void YesExitButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
        private void NoBackButton_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService != null)
            {
                if (NavigationService.CanGoBack) NavigationService.GoBack();
                else NavigationService.Navigate(new MainMenu());
            }
        }
    }
}