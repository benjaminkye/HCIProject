using System;
using System.Collections.Generic;
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
using DigitalHelper.Services;

namespace DigitalHelper.Views
{
    /// <summary>
    /// Interaction logic for MainMenu.xaml
    /// </summary>
    public partial class MainMenu : Page
    {
        public MainMenu()
        {
            InitializeComponent();
        }

        private void exitButton_Click(object sender, RoutedEventArgs e)
        {
            //keep NavigationService?.Navigate(new ExitConfirm());
            NavigationService?.Navigate(new ExitConfirm());
        }

        private void PasswordVaultButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new PasswordVault());
        }

        private void TestCapture_Click(object sender, RoutedEventArgs e)
        {
            var svc = new ScreenCaptureService();
            //var desktop
            string path = svc.SaveCapture1000();
            MessageBox.Show($"Saved 1kx1k capture to :\n{path}");
        }

        private void CustomHelpButton_Click(object sender, RoutedEventArgs e)
        {
            var svc = new ScreenCaptureService();
            var desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            var folder = System.IO.Path.Combine(desktop, "HCI");
            string path = svc.SaveCapture1000(
                folderPath: folder,
                fileBaseName: "ui_image",
                scale: true);
            MessageBox.Show($"Saved 1kx1k capture to :\n{path}");
        }
        
        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/Views/Settings.xaml", UriKind.Relative));
        }
    }
}
