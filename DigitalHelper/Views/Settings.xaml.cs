using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Windows.Media;

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

        private void FontSizeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox comboBox && comboBox.SelectedItem is ComboBoxItem selected)
            {
                string size = selected.Content.ToString();
                ApplyFontSize(size);
            }
        }

        private void ApplyFontSize(string size)
        {
            double zoom = size switch
            {
                "Small"       => 0.85,
                "Large"       => 1.25,
                "Extra Large" => 1.50,
                _             => 1.00
            };

            var mainWindow = Application.Current.MainWindow;

            if (mainWindow != null)
            {
                if (mainWindow.Content is Frame frame)
                {
                    frame.LayoutTransform = new ScaleTransform(zoom, zoom);
                }
                else if (mainWindow.Content is FrameworkElement element)
                {
                    element.LayoutTransform = new ScaleTransform(zoom, zoom);
                }
            }
        }
    }
}
