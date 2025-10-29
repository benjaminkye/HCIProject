using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using DigitalHelper.Models;
using DigitalHelper.Services;

namespace DigitalHelper.Views
{
    public partial class SecureNotes : Page
    {
        private ObservableCollection<SecureNoteItem> filteredNotes;
        private SecureNoteItem? currentEditingNote = null;
        private bool isAddingNew = false;

        public SecureNotes()
        {
            InitializeComponent();
            
            filteredNotes = new ObservableCollection<SecureNoteItem>();
            
            LoadNotes();
            
            NoteListBox.ItemsSource = filteredNotes;
            
            NewNoteButton.Click += NewNoteButton_Click;
            EditButton.Click += EditButton_Click;
            SearchBox.TextChanged += SearchBox_TextChanged;
            NoteListBox.SelectionChanged += NoteListBox_SelectionChanged;
            SaveButton.Click += SaveButton_Click;
            CancelButton.Click += CancelButton_Click;
            DeleteButton.Click += DeleteButton_Click;
            
            if (filteredNotes.Count > 0)
            {
                NoteListBox.SelectedIndex = 0;
            }
            else
            {
                UpdateEmptyState();
            }
        }

        private void LoadNotes()
        {
            RefreshFilteredNotes();
        }

        private void SaveData()
        {
            VaultDataService.Instance.SaveData();
        }

        private void RefreshFilteredNotes()
        {
            string searchText = SearchBox.Text.ToLower();
            filteredNotes.Clear();
            
            var allNotes = VaultDataService.Instance.Data.SecureNotes;
            var notes = string.IsNullOrWhiteSpace(searchText)
                ? allNotes
                : allNotes.Where(n => 
                    n.Title.ToLower().Contains(searchText) ||
                    n.Content.ToLower().Contains(searchText));
            
            // Sort by modified date, most recent first
            foreach (var note in notes.OrderByDescending(n => n.ModifiedDate))
            {
                filteredNotes.Add(note);
            }
            
            UpdateEmptyState();
        }

        private void UpdateEmptyState()
        {
            if (NoteListBox.SelectedItem == null || filteredNotes.Count == 0)
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
            
            RefreshFilteredNotes();
        }

        private void NoteListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (EditPanel.Visibility == Visibility.Visible)
            {
                return;
            }

            if (NoteListBox.SelectedItem is SecureNoteItem selectedNote)
            {
                DisplayNoteDetails(selectedNote);
                ViewPanel.Visibility = Visibility.Visible;
                EmptyStatePanel.Visibility = Visibility.Collapsed;
            }
            else
            {
                UpdateEmptyState();
            }
        }

        private void DisplayNoteDetails(SecureNoteItem note)
        {
            ViewTitleTextBox.Text = note.Title;
            ViewContentTextBox.Text = note.Content;
            ViewCreatedDateTextBlock.Text = note.CreatedDate.ToString("MMM d, yyyy h:mm tt");
            ViewModifiedDateTextBlock.Text = note.ModifiedDate.ToString("MMM d, yyyy h:mm tt");
        }

        private void NewNoteButton_Click(object sender, RoutedEventArgs e)
        {
            isAddingNew = true;
            currentEditingNote = null;
            
            EditNoteTitleTextBox.Text = "";
            EditContentTextBox.Text = "";
            
            EditTitleTextBlock.Text = "Add New Note";
            DeleteButton.Visibility = Visibility.Collapsed;
            
            ViewPanel.Visibility = Visibility.Collapsed;
            EmptyStatePanel.Visibility = Visibility.Collapsed;
            EditPanel.Visibility = Visibility.Visible;
            
            EditNoteTitleTextBox.Focus();
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (NoteListBox.SelectedItem is SecureNoteItem selectedNote)
            {
                isAddingNew = false;
                currentEditingNote = selectedNote;
                
                EditNoteTitleTextBox.Text = selectedNote.Title;
                EditContentTextBox.Text = selectedNote.Content;
                
                EditTitleTextBlock.Text = "Edit Note";
                DeleteButton.Visibility = Visibility.Visible;
                
                ViewPanel.Visibility = Visibility.Collapsed;
                EditPanel.Visibility = Visibility.Visible;
                
                EditNoteTitleTextBox.Focus();
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(EditNoteTitleTextBox.Text))
            {
                MessageBox.Show("Please enter a title.", "Validation Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                EditNoteTitleTextBox.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(EditContentTextBox.Text))
            {
                MessageBox.Show("Please enter content.", "Validation Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                EditContentTextBox.Focus();
                return;
            }

            SecureNoteItem? itemToSelect = null;
            
            if (isAddingNew)
            {
                var newNote = new SecureNoteItem
                {
                    Title = EditNoteTitleTextBox.Text.Trim(),
                    Content = EditContentTextBox.Text.Trim(),
                    CreatedDate = DateTime.Now,
                    ModifiedDate = DateTime.Now
                };
                
                VaultDataService.Instance.Data.SecureNotes.Add(newNote);
                RefreshFilteredNotes();
                SaveData();
                
                itemToSelect = newNote;
            }
            else if (currentEditingNote != null)
            {
                currentEditingNote.Title = EditNoteTitleTextBox.Text.Trim();
                currentEditingNote.Content = EditContentTextBox.Text.Trim();
                currentEditingNote.ModifiedDate = DateTime.Now;
                
                RefreshFilteredNotes();
                SaveData();
                
                itemToSelect = currentEditingNote;
            }
            
            ViewPanel.Visibility = Visibility.Visible;
            EditPanel.Visibility = Visibility.Collapsed;
            
            if (itemToSelect != null)
            {
                NoteListBox.SelectedItem = itemToSelect;
                DisplayNoteDetails(itemToSelect);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            if (isAddingNew)
            {
                if (filteredNotes.Count > 0 && NoteListBox.SelectedItem == null)
                {
                    UpdateEmptyState();
                }
                else if (NoteListBox.SelectedItem is SecureNoteItem note)
                {
                    DisplayNoteDetails(note);
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
                
                if (NoteListBox.SelectedItem is SecureNoteItem note)
                {
                    DisplayNoteDetails(note);
                }
            }
            
            currentEditingNote = null;
            isAddingNew = false;
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentEditingNote != null)
            {
                var result = MessageBox.Show(
                    "Are you sure you want to delete this note?",
                    "Confirm Delete",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    VaultDataService.Instance.Data.SecureNotes.Remove(currentEditingNote);
                    RefreshFilteredNotes();
                    SaveData();
                    
                    NoteListBox.SelectedItem = null;
                    currentEditingNote = null;
                    isAddingNew = false;
                    
                    UpdateEmptyState();
                }
            }
        }
    }
}

