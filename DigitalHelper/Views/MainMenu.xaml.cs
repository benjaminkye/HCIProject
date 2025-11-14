using DigitalHelper.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DigitalHelper.Views
{
    /// <summary>
    /// Interaction logic for MainMenu.xaml
    /// </summary>
    public partial class MainMenu : Page
    {
        private ChatWindow chatWindow = new ChatWindow();
        public MainMenu()
        {
            InitializeComponent();
        }

        private void exitButton_Click(object sender, RoutedEventArgs e)
        {
            // In future may need to rework to ask for minimize if not using topbar for transition to mini helper
            Application.Current.Shutdown();
        }

        private void PasswordVaultButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new PasswordVault(this));
        }

        private void CustomHelpButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(chatWindow);
        }

        private void TestCapture_Click(object sender, RoutedEventArgs e)
        {
            var svc = new ScreenCaptureService();
            //var desktop
            string path = svc.SaveCapture1000();
            MessageBox.Show($"Saved 1kx1k capture to :\n{path}");
        }
        
        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/Views/Settings.xaml", UriKind.Relative));
        }
    }
}
