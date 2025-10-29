using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using DigitalHelper.Models;
using DigitalHelper.Services;

namespace DigitalHelper.Views
{
    public partial class Cards : Page
    {
        private ObservableCollection<CardItem> filteredCards;
        private bool isCardNumberVisible = false;
        private bool isCVVVisible = false;
        private CardItem? currentEditingCard = null;
        private bool isAddingNew = false;

        public Cards()
        {
            InitializeComponent();
            
            filteredCards = new ObservableCollection<CardItem>();
            
            LoadCards();
            
            CardListBox.ItemsSource = filteredCards;
            
            NewCardButton.Click += NewCardButton_Click;
            EditButton.Click += EditButton_Click;
            SearchBox.TextChanged += SearchBox_TextChanged;
            CardListBox.SelectionChanged += CardListBox_SelectionChanged;
            ShowCardNumberButton.Click += ShowCardNumberButton_Click;
            ShowCVVButton.Click += ShowCVVButton_Click;
            SaveButton.Click += SaveButton_Click;
            CancelButton.Click += CancelButton_Click;
            DeleteButton.Click += DeleteButton_Click;
            
            if (filteredCards.Count > 0)
            {
                CardListBox.SelectedIndex = 0;
            }
            else
            {
                UpdateEmptyState();
            }
        }

        private void LoadCards()
        {
            RefreshFilteredCards();
        }

        private void SaveData()
        {
            VaultDataService.Instance.SaveData();
        }

        private void RefreshFilteredCards()
        {
            string searchText = SearchBox.Text.ToLower();
            filteredCards.Clear();
            
            var allCards = VaultDataService.Instance.Data.Cards;
            var cards = string.IsNullOrWhiteSpace(searchText)
                ? allCards
                : allCards.Where(c => 
                    c.CardName.ToLower().Contains(searchText) ||
                    c.CardHolderName.ToLower().Contains(searchText) ||
                    c.CardType.ToLower().Contains(searchText) ||
                    c.CardNumber.Contains(searchText));
            
            foreach (var card in cards)
            {
                filteredCards.Add(card);
            }
            
            UpdateEmptyState();
        }

        private void UpdateEmptyState()
        {
            if (CardListBox.SelectedItem == null || filteredCards.Count == 0)
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
            SearchPlaceholder.Visibility = string.IsNullOrEmpty(SearchBox.Text) 
                ? Visibility.Visible 
                : Visibility.Collapsed;
            
            RefreshFilteredCards();
        }

        private void CardListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (EditPanel.Visibility == Visibility.Visible)
            {
                return;
            }

            if (CardListBox.SelectedItem is CardItem selectedCard)
            {
                DisplayCardDetails(selectedCard);
                ViewPanel.Visibility = Visibility.Visible;
                EmptyStatePanel.Visibility = Visibility.Collapsed;
            }
            else
            {
                UpdateEmptyState();
            }
        }

        private void DisplayCardDetails(CardItem card)
        {
            ViewCardNameTextBox.Text = card.CardName;
            ViewCardHolderTextBox.Text = card.CardHolderName;
            ViewCardNumberTextBox.Text = card.LastFourDigits;
            ViewExpiryDateTextBox.Text = card.ExpiryDate;
            ViewCVVTextBox.Text = "•••";
            ViewCardTypeTextBox.Text = card.CardType;
            ViewBillingAddressTextBox.Text = card.BillingAddress;
            
            isCardNumberVisible = false;
            isCVVVisible = false;
            ShowCardNumberButton.Content = "Show";
            ShowCVVButton.Content = "Show";
        }

        private void ShowCardNumberButton_Click(object sender, RoutedEventArgs e)
        {
            if (CardListBox.SelectedItem is CardItem selectedCard)
            {
                isCardNumberVisible = !isCardNumberVisible;
                
                if (isCardNumberVisible)
                {
                    ViewCardNumberTextBox.Text = FormatCardNumber(selectedCard.CardNumber);
                    ShowCardNumberButton.Content = "Hide";
                }
                else
                {
                    ViewCardNumberTextBox.Text = selectedCard.LastFourDigits;
                    ShowCardNumberButton.Content = "Show";
                }
            }
        }

        private void ShowCVVButton_Click(object sender, RoutedEventArgs e)
        {
            if (CardListBox.SelectedItem is CardItem selectedCard)
            {
                isCVVVisible = !isCVVVisible;
                
                if (isCVVVisible)
                {
                    ViewCVVTextBox.Text = selectedCard.CVV;
                    ShowCVVButton.Content = "Hide";
                }
                else
                {
                    ViewCVVTextBox.Text = "•••";
                    ShowCVVButton.Content = "Show";
                }
            }
        }

        private string FormatCardNumber(string cardNumber)
        {
            cardNumber = cardNumber.Replace(" ", "");
            
            string formatted = "";
            for (int i = 0; i < cardNumber.Length; i++)
            {
                if (i > 0 && i % 4 == 0)
                    formatted += " ";
                formatted += cardNumber[i];
            }
            
            return formatted;
        }

        private void NewCardButton_Click(object sender, RoutedEventArgs e)
        {
            isAddingNew = true;
            currentEditingCard = null;
            
            EditCardNameTextBox.Text = "";
            EditCardHolderTextBox.Text = "";
            EditCardNumberTextBox.Text = "";
            EditExpiryDateTextBox.Text = "";
            EditCVVTextBox.Text = "";
            EditCardTypeComboBox.SelectedIndex = 0;
            EditBillingAddressTextBox.Text = "";
            
            EditTitleTextBlock.Text = "Add New Card";
            DeleteButton.Visibility = Visibility.Collapsed;
            
            ViewPanel.Visibility = Visibility.Collapsed;
            EmptyStatePanel.Visibility = Visibility.Collapsed;
            EditPanel.Visibility = Visibility.Visible;
            
            EditCardNameTextBox.Focus();
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (CardListBox.SelectedItem is CardItem selectedCard)
            {
                isAddingNew = false;
                currentEditingCard = selectedCard;
                
                EditCardNameTextBox.Text = selectedCard.CardName;
                EditCardHolderTextBox.Text = selectedCard.CardHolderName;
                EditCardNumberTextBox.Text = selectedCard.CardNumber;
                EditExpiryDateTextBox.Text = selectedCard.ExpiryDate;
                EditCVVTextBox.Text = selectedCard.CVV;
                EditBillingAddressTextBox.Text = selectedCard.BillingAddress;
                
                // Set card type
                var cardType = selectedCard.CardType;
                for (int i = 0; i < EditCardTypeComboBox.Items.Count; i++)
                {
                    if (((ComboBoxItem)EditCardTypeComboBox.Items[i]).Content.ToString() == cardType)
                    {
                        EditCardTypeComboBox.SelectedIndex = i;
                        break;
                    }
                }
                
                EditTitleTextBlock.Text = "Edit Card";
                DeleteButton.Visibility = Visibility.Visible;
                
                ViewPanel.Visibility = Visibility.Collapsed;
                EditPanel.Visibility = Visibility.Visible;
                
                EditCardNameTextBox.Focus();
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(EditCardNameTextBox.Text))
            {
                MessageBox.Show("Please enter a card name.", "Validation Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                EditCardNameTextBox.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(EditCardHolderTextBox.Text))
            {
                MessageBox.Show("Please enter the card holder name.", "Validation Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                EditCardHolderTextBox.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(EditCardNumberTextBox.Text))
            {
                MessageBox.Show("Please enter the card number.", "Validation Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                EditCardNumberTextBox.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(EditExpiryDateTextBox.Text))
            {
                MessageBox.Show("Please enter the expiry date.", "Validation Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                EditExpiryDateTextBox.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(EditCVVTextBox.Text))
            {
                MessageBox.Show("Please enter the CVV.", "Validation Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                EditCVVTextBox.Focus();
                return;
            }

            string cardType = "Other";
            if (EditCardTypeComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                cardType = selectedItem.Content.ToString() ?? "Other";
            }

            CardItem? itemToSelect = null;
            
            if (isAddingNew)
            {
                var newCard = new CardItem
                {
                    CardName = EditCardNameTextBox.Text.Trim(),
                    CardHolderName = EditCardHolderTextBox.Text.Trim(),
                    CardNumber = EditCardNumberTextBox.Text.Replace(" ", "").Trim(),
                    ExpiryDate = EditExpiryDateTextBox.Text.Trim(),
                    CVV = EditCVVTextBox.Text.Trim(),
                    CardType = cardType,
                    BillingAddress = EditBillingAddressTextBox.Text.Trim()
                };
                
                VaultDataService.Instance.Data.Cards.Add(newCard);
                RefreshFilteredCards();
                SaveData();
                
                itemToSelect = newCard;
            }
            else if (currentEditingCard != null)
            {
                currentEditingCard.CardName = EditCardNameTextBox.Text.Trim();
                currentEditingCard.CardHolderName = EditCardHolderTextBox.Text.Trim();
                currentEditingCard.CardNumber = EditCardNumberTextBox.Text.Replace(" ", "").Trim();
                currentEditingCard.ExpiryDate = EditExpiryDateTextBox.Text.Trim();
                currentEditingCard.CVV = EditCVVTextBox.Text.Trim();
                currentEditingCard.CardType = cardType;
                currentEditingCard.BillingAddress = EditBillingAddressTextBox.Text.Trim();
                
                RefreshFilteredCards();
                SaveData();
                
                itemToSelect = currentEditingCard;
            }
            
            ViewPanel.Visibility = Visibility.Visible;
            EditPanel.Visibility = Visibility.Collapsed;
            
            if (itemToSelect != null)
            {
                CardListBox.SelectedItem = itemToSelect;
                DisplayCardDetails(itemToSelect);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            if (isAddingNew)
            {
                if (filteredCards.Count > 0 && CardListBox.SelectedItem == null)
                {
                    UpdateEmptyState();
                }
                else if (CardListBox.SelectedItem is CardItem card)
                {
                    DisplayCardDetails(card);
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
                
                if (CardListBox.SelectedItem is CardItem card)
                {
                    DisplayCardDetails(card);
                }
            }
            
            currentEditingCard = null;
            isAddingNew = false;
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentEditingCard != null)
            {
                var result = MessageBox.Show(
                    "Are you sure you want to delete this card?",
                    "Confirm Delete",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    VaultDataService.Instance.Data.Cards.Remove(currentEditingCard);
                    RefreshFilteredCards();
                    SaveData();
                    
                    CardListBox.SelectedItem = null;
                    currentEditingCard = null;
                    isAddingNew = false;
                    
                    UpdateEmptyState();
                }
            }
        }
    }
}

