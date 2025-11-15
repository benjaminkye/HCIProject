using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace DigitalHelper.Views
{
    public partial class Settings : Page
    {
        public Settings()
        {
            InitializeComponent();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

     
    }
}