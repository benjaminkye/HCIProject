﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DigitalHelper.Views
{
    /// <summary>
    /// Interaction logic for PasswordVault.xaml
    /// </summary>
    public partial class PasswordVault : Page
    {
        public PasswordVault()
        {
            InitializeComponent();
            
            // I added this because vaultFrame was just white on load but issue was unrelated, keeping just in case
            this.Loaded += PasswordVault_Loaded;
        }

        private void PasswordVault_Loaded(object sender, RoutedEventArgs e)
        {
            NavigateToLogins();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new MainMenu());
        }

        private void LoginsButton_Click(object sender, RoutedEventArgs e)
        {
            NavigateToLogins();
        }

        private void CardsButton_Click(object sender, RoutedEventArgs e)
        {
            NavigateToCards();
        }

        private void SecureNotesButton_Click(object sender, RoutedEventArgs e)
        {
            NavigateToSecureNotes();
        }

        private void NavigateToLogins()
        {
            VaultFrame.Navigate(new Logins());
            UpdateButtonHighlight(LoginsButton);
        }

        private void NavigateToCards()
        {
            VaultFrame.Navigate(new Cards());
            UpdateButtonHighlight(CardsButton);
        }

        private void NavigateToSecureNotes()
        {
            VaultFrame.Navigate(new SecureNotes());
            UpdateButtonHighlight(SecureNotesButton);
        }

        private void UpdateButtonHighlight(Button activeButton)
        {
            LoginsButton.Background = Brushes.Transparent;
            CardsButton.Background = Brushes.Transparent;
            SecureNotesButton.Background = Brushes.Transparent;

            activeButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF3E3E42"));
        }
    }
}
