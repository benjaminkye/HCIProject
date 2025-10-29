using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using DigitalHelper.Models;
using DigitalHelper.Services;

namespace DigitalHelper.Views
{
    /// <summary>
    /// Interaction logic for Logins.xaml
    /// </summary>
    public partial class Logins : Page
    {
        private ObservableCollection<LoginItem> filteredLogins;
        private bool isPasswordVisible = false;
        private LoginItem? currentEditingLogin = null;
        private bool isAddingNew = false;

        public Logins()
        {
            InitializeComponent();
            
            filteredLogins = new ObservableCollection<LoginItem>();
            
            LoadLogins();
            
            LoginListBox.ItemsSource = filteredLogins;
            
            NewLoginButton.Click += NewLoginButton_Click;
            EditButton.Click += EditButton_Click;
            SearchBox.TextChanged += SearchBox_TextChanged;
            LoginListBox.SelectionChanged += LoginListBox_SelectionChanged;
            ShowPasswordButton.Click += ShowPasswordButton_Click;
            SaveButton.Click += SaveButton_Click;
            CancelButton.Click += CancelButton_Click;
            DeleteButton.Click += DeleteButton_Click;
            
            // Select first item by default if available
            if (filteredLogins.Count > 0)
            {
                LoginListBox.SelectedIndex = 0;
            }
            else
            {
                UpdateEmptyState();
            }
        }

        private void LoadLogins()
        {
            RefreshFilteredLogins();
        }

        private void SaveData()
        {
            VaultDataService.Instance.SaveData();
        }

        private void RefreshFilteredLogins()
        {
            string searchText = SearchBox.Text.ToLower();
            filteredLogins.Clear();
            
            var allLogins = VaultDataService.Instance.Data.Logins;
            var logins = string.IsNullOrWhiteSpace(searchText)
                ? allLogins
                : allLogins.Where(l => 
                    l.SiteName.ToLower().Contains(searchText) ||
                    l.Username.ToLower().Contains(searchText) ||
                    l.Url.ToLower().Contains(searchText));
            
            foreach (var login in logins)
            {
                filteredLogins.Add(login);
            }
            
            UpdateEmptyState();
        }

        private void UpdateEmptyState()
        {
            if (LoginListBox.SelectedItem == null || filteredLogins.Count == 0)
            {
                ViewPanel.Visibility = Visibility.Collapsed;
                EditPanel.Visibility = Visibility.Collapsed;
                EmptyStatePanel.Visibility = Visibility.Visible;
            }
            else
            {
                ViewPanel.Visibility = Visibility.Visible;
                EditPanel.Visibility = Visibility.Collapsed;
                EmptyStatePanel.Visibility = Visibility.Collapsed;
            }
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Update placeholder visibility
            SearchPlaceholder.Visibility = string.IsNullOrEmpty(SearchBox.Text) 
                ? Visibility.Visible 
                : Visibility.Collapsed;
            
            RefreshFilteredLogins();
        }

        private void LoginListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Don't change view if in edit mode
            if (EditPanel.Visibility == Visibility.Visible)
            {
                return;
            }

            if (LoginListBox.SelectedItem is LoginItem selectedLogin)
            {
                DisplayLoginDetails(selectedLogin);
                ViewPanel.Visibility = Visibility.Visible;
                EmptyStatePanel.Visibility = Visibility.Collapsed;
            }
            else
            {
                UpdateEmptyState();
            }
        }

        private void DisplayLoginDetails(LoginItem login)
        {
            ViewSiteNameTextBox.Text = login.SiteName;
            ViewUrlTextBox.Text = login.Url;
            ViewUsernameTextBox.Text = login.Username;
            ViewPasswordBox.Password = login.Password;
            ViewPasswordTextBox.Text = login.Password;
            
            // Reset password visibility
            isPasswordVisible = false;
            ViewPasswordBox.Visibility = Visibility.Visible;
            ViewPasswordTextBox.Visibility = Visibility.Collapsed;
            ShowPasswordButton.Content = "Show";
        }

        private void ShowPasswordButton_Click(object sender, RoutedEventArgs e)
        {
            isPasswordVisible = !isPasswordVisible;
            
            if (isPasswordVisible)
            {
                ViewPasswordBox.Visibility = Visibility.Collapsed;
                ViewPasswordTextBox.Visibility = Visibility.Visible;
                ShowPasswordButton.Content = "Hide";
            }
            else
            {
                ViewPasswordBox.Visibility = Visibility.Visible;
                ViewPasswordTextBox.Visibility = Visibility.Collapsed;
                ShowPasswordButton.Content = "Show";
            }
        }

        private void NewLoginButton_Click(object sender, RoutedEventArgs e)
        {
            isAddingNew = true;
            currentEditingLogin = null;
            
            EditSiteNameTextBox.Text = "";
            EditUrlTextBox.Text = "";
            EditUsernameTextBox.Text = "";
            EditPasswordTextBox.Text = "";
            
            EditTitleTextBlock.Text = "Add New Login";
            DeleteButton.Visibility = Visibility.Collapsed;
            
            ViewPanel.Visibility = Visibility.Collapsed;
            EmptyStatePanel.Visibility = Visibility.Collapsed;
            EditPanel.Visibility = Visibility.Visible;
            
            EditSiteNameTextBox.Focus();
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (LoginListBox.SelectedItem is LoginItem selectedLogin)
            {
                isAddingNew = false;
                currentEditingLogin = selectedLogin;
                
                EditSiteNameTextBox.Text = selectedLogin.SiteName;
                EditUrlTextBox.Text = selectedLogin.Url;
                EditUsernameTextBox.Text = selectedLogin.Username;
                EditPasswordTextBox.Text = selectedLogin.Password;
                
                EditTitleTextBlock.Text = "Edit Login";
                DeleteButton.Visibility = Visibility.Visible;
                
                ViewPanel.Visibility = Visibility.Collapsed;
                EditPanel.Visibility = Visibility.Visible;
                
                EditSiteNameTextBox.Focus();
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(EditSiteNameTextBox.Text))
            {
                MessageBox.Show("Please enter a site name.", "Validation Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                EditSiteNameTextBox.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(EditUsernameTextBox.Text))
            {
                MessageBox.Show("Please enter a username or email.", "Validation Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                EditUsernameTextBox.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(EditPasswordTextBox.Text))
            {
                MessageBox.Show("Please enter a password.", "Validation Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                EditPasswordTextBox.Focus();
                return;
            }

            LoginItem? itemToSelect = null;
            
            if (isAddingNew)
            {
                // Create new login
                var newLogin = new LoginItem
                {
                    SiteName = EditSiteNameTextBox.Text.Trim(),
                    Url = EditUrlTextBox.Text.Trim(),
                    Username = EditUsernameTextBox.Text.Trim(),
                    Password = EditPasswordTextBox.Text
                };
                
                VaultDataService.Instance.Data.Logins.Add(newLogin);
                RefreshFilteredLogins();
                SaveData();
                
                itemToSelect = newLogin;
            }
            else if (currentEditingLogin != null)
            {
                // Update existing login
                currentEditingLogin.SiteName = EditSiteNameTextBox.Text.Trim();
                currentEditingLogin.Url = EditUrlTextBox.Text.Trim();
                currentEditingLogin.Username = EditUsernameTextBox.Text.Trim();
                currentEditingLogin.Password = EditPasswordTextBox.Text;
                
                RefreshFilteredLogins();
                SaveData();
                
                itemToSelect = currentEditingLogin;
            }
            
            // Switching back to view mode
            ViewPanel.Visibility = Visibility.Visible;
            EditPanel.Visibility = Visibility.Collapsed;
            
            // Reselect prev item
            if (itemToSelect != null)
            {
                LoginListBox.SelectedItem = itemToSelect;
                DisplayLoginDetails(itemToSelect);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            if (isAddingNew)
            {
                if (filteredLogins.Count > 0 && LoginListBox.SelectedItem == null)
                {
                    UpdateEmptyState();
                }
                else if (LoginListBox.SelectedItem is LoginItem login)
                {
                    DisplayLoginDetails(login);
                    ViewPanel.Visibility = Visibility.Visible;
                    EditPanel.Visibility = Visibility.Collapsed;
                }
                else
                {
                    UpdateEmptyState();
                }
            }
            else
            {
                ViewPanel.Visibility = Visibility.Visible;
                EditPanel.Visibility = Visibility.Collapsed;
                
                if (LoginListBox.SelectedItem is LoginItem login)
                {
                    DisplayLoginDetails(login);
                }
            }
            
            currentEditingLogin = null;
            isAddingNew = false;
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentEditingLogin != null)
            {
                var result = MessageBox.Show(
                    "Are you sure you want to delete this login?",
                    "Confirm Delete",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    VaultDataService.Instance.Data.Logins.Remove(currentEditingLogin);
                    RefreshFilteredLogins();
                    SaveData();
                    
                    LoginListBox.SelectedItem = null;
                    currentEditingLogin = null;
                    isAddingNew = false;
                    
                    UpdateEmptyState();
                }
            }
        }
    }
}
