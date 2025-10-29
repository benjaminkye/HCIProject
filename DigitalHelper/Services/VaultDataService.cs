using System;
using System.IO;
using System.Text.Json;
using DigitalHelper.Models;

namespace DigitalHelper.Services
{
    public class VaultDataService
    {
        private static VaultDataService? _instance;
        private static readonly object _lock = new object();
        
        private VaultData _vaultData;
        private readonly string _dataFilePath;

        private VaultDataService()
        {
            // Set the file path to be in the same directory as the executable
            string exeDirectory = AppDomain.CurrentDomain.BaseDirectory;
            _dataFilePath = Path.Combine(exeDirectory, "vault_data.json");
            
            _vaultData = new VaultData();
            LoadData();
        }

        public static VaultDataService Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new VaultDataService();
                        }
                    }
                }
                return _instance;
            }
        }

        public VaultData Data => _vaultData;

        private void LoadData()
        {
            try
            {
                if (File.Exists(_dataFilePath))
                {
                    string json = File.ReadAllText(_dataFilePath);
                    var data = JsonSerializer.Deserialize<VaultData>(json);
                    
                    if (data != null)
                    {
                        _vaultData = data;
                    }
                }
                else
                {
                    // Create dummy data for initial testing
                    CreateDummyData();
                    SaveData();
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error loading vault data: {ex.Message}", "Error", 
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        public void SaveData()
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(_vaultData, options);
                File.WriteAllText(_dataFilePath, json);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error saving vault data: {ex.Message}", "Error", 
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        private void CreateDummyData()
        {
            // Dummy Logins
            _vaultData.Logins.Add(new LoginItem
            {
                SiteName = "Google",
                Url = "https://accounts.google.com",
                Username = "user@gmail.com",
                Password = "password123"
            });
            _vaultData.Logins.Add(new LoginItem
            {
                SiteName = "Facebook",
                Url = "https://www.facebook.com",
                Username = "john.doe@email.com",
                Password = "fb_pass456"
            });
            _vaultData.Logins.Add(new LoginItem
            {
                SiteName = "GitHub",
                Url = "https://github.com/login",
                Username = "developer_user",
                Password = "github2024!"
            });
            _vaultData.Logins.Add(new LoginItem
            {
                SiteName = "Amazon",
                Url = "https://www.amazon.com",
                Username = "shopper@example.com",
                Password = "shop_secure789"
            });
            _vaultData.Logins.Add(new LoginItem
            {
                SiteName = "Netflix",
                Url = "https://www.netflix.com/login",
                Username = "movie_lover",
                Password = "netflixPass321"
            });

            // Dummy Cards
            _vaultData.Cards.Add(new CardItem
            {
                CardName = "Personal Visa",
                CardHolderName = "John Doe",
                CardNumber = "4532123456789012",
                ExpiryDate = "12/26",
                CVV = "123",
                CardType = "Visa",
                BillingAddress = "123 Main St, Springfield, IL 62701"
            });
            _vaultData.Cards.Add(new CardItem
            {
                CardName = "Work AmEx",
                CardHolderName = "John Doe",
                CardNumber = "378282246310005",
                ExpiryDate = "08/27",
                CVV = "4567",
                CardType = "American Express",
                BillingAddress = "456 Business Ave, Suite 200, Chicago, IL 60601"
            });
            _vaultData.Cards.Add(new CardItem
            {
                CardName = "Personal MasterCard",
                CardHolderName = "Jane Doe",
                CardNumber = "5425233430109903",
                ExpiryDate = "03/25",
                CVV = "789",
                CardType = "MasterCard",
                BillingAddress = "789 Oak Lane, Austin, TX 78701"
            });
            _vaultData.Cards.Add(new CardItem
            {
                CardName = "Discover Rewards",
                CardHolderName = "John Doe",
                CardNumber = "6011111111111117",
                ExpiryDate = "06/28",
                CVV = "321",
                CardType = "Discover",
                BillingAddress = ""
            });

            // Dummy Secure Notes
            _vaultData.SecureNotes.Add(new SecureNoteItem
            {
                Title = "WiFi Passwords",
                Content = "Home WiFi: SecureNetwork2024\nGuest WiFi: GuestAccess123\nOffice WiFi: WorkSecure456",
                CreatedDate = DateTime.Now.AddDays(-30),
                ModifiedDate = DateTime.Now.AddDays(-5)
            });
            _vaultData.SecureNotes.Add(new SecureNoteItem
            {
                Title = "Software License Keys",
                Content = "Windows 10 Pro: XXXXX-XXXXX-XXXXX-XXXXX-XXXXX\nOffice 365: YYYYY-YYYYY-YYYYY-YYYYY-YYYYY\nAdobe CC: ZZZZZ-ZZZZZ-ZZZZZ-ZZZZZ-ZZZZZ",
                CreatedDate = DateTime.Now.AddDays(-60),
                ModifiedDate = DateTime.Now.AddDays(-15)
            });
            _vaultData.SecureNotes.Add(new SecureNoteItem
            {
                Title = "Bank Account Info",
                Content = "Checking Account: 1234567890\nRouting Number: 987654321\nSavings Account: 0987654321",
                CreatedDate = DateTime.Now.AddDays(-90),
                ModifiedDate = DateTime.Now.AddDays(-1)
            });
            _vaultData.SecureNotes.Add(new SecureNoteItem
            {
                Title = "Emergency Contacts",
                Content = "Mom: (555) 123-4567\nDad: (555) 234-5678\nSister: (555) 345-6789\nBest Friend: (555) 456-7890",
                CreatedDate = DateTime.Now.AddDays(-120),
                ModifiedDate = DateTime.Now.AddDays(-20)
            });
            _vaultData.SecureNotes.Add(new SecureNoteItem
            {
                Title = "Server Credentials",
                Content = "Production Server:\nIP: 192.168.1.100\nUsername: admin\nPassword: prod_secure_pass\nSSH Key: ~/.ssh/prod_key",
                CreatedDate = DateTime.Now.AddDays(-45),
                ModifiedDate = DateTime.Now.AddHours(-2)
            });
        }
    }
}

