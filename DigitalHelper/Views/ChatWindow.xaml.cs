using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using DigitalHelper.Services;

namespace DigitalHelper.Views
{
    /// <summary>
    /// Chat window for conversing with the digital helper
    /// </summary>
    public partial class ChatWindow : Page
    {
        private bool isFirstMessage = true;
        private string userTask = string.Empty;

        public ChatWindow()
        {
            InitializeComponent();
            
            AddHelperMessage("Hi! What do you need help with?");
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.GoBack();
        }

        private void NewChatButton_Click(object sender, RoutedEventArgs e)
        {
            // Clear chat and start over
            ChatMessagesPanel.Children.Clear();
            isFirstMessage = true;
            userTask = string.Empty;
            AddHelperMessage("Hi! What do you need help with?");
        }

        private void MessageInput_GotFocus(object sender, RoutedEventArgs e)
        {
            if (MessageInput.Text == "Type your request here...")
            {
                MessageInput.Text = "";
                MessageInput.Foreground = new SolidColorBrush(Colors.Black);
            }
        }

        private void MessageInput_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(MessageInput.Text))
            {
                MessageInput.Text = "Type your request here...";
                MessageInput.Foreground = new SolidColorBrush(Colors.Gray);
            }
        }

        private void MessageInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SendMessage();
            }
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            SendMessage();
        }

        private void SendMessage()
        {
            string message = MessageInput.Text.Trim();
            
            if (string.IsNullOrWhiteSpace(message) || message == "Type your request here...")
            {
                return;
            }

            AddUserMessage(message);
            
            MessageInput.Text = "";
            MessageInput.Focus();

            if (isFirstMessage)
            {
                userTask = message;
                isFirstMessage = false;
                
                // Helper responds with mode selection
                AddHelperMessage("Would you like a step-by-step guide or realtime guidance?");
                AddModeSelectionButtons();
            }
            else
            {
                // Currently we just want them to click one of the options
                AddHelperMessage("Please select one of the two options above or start a new chat.");
            }
        }

        private void AddHelperMessage(string message)
        {
            var bubble = CreateMessageBubble(message, isHelper: true);
            ChatMessagesPanel.Children.Add(bubble);
            ScrollToBottom();
        }

        private void AddUserMessage(string message)
        {
            var bubble = CreateMessageBubble(message, isHelper: false);
            ChatMessagesPanel.Children.Add(bubble);
            ScrollToBottom();
        }

        private UIElement CreateMessageBubble(string message, bool isHelper)
        {
            var border = new Border
            {
                Background = new SolidColorBrush(isHelper ? Colors.White : Color.FromRgb(0, 123, 255)),
                CornerRadius = new CornerRadius(15),
                Padding = new Thickness(15, 10, 15, 10),
                Margin = new Thickness(isHelper ? 0 : 100, 5, isHelper ? 100 : 0, 5),
                HorizontalAlignment = isHelper ? HorizontalAlignment.Left : HorizontalAlignment.Right,
                MaxWidth = 600
            };

            var grid = new Grid();
            
            var textBlock = new TextBlock
            {
                Text = message,
                TextWrapping = TextWrapping.Wrap,
                FontSize = 16,
                Foreground = new SolidColorBrush(isHelper ? Colors.Black : Colors.White)
            };

            grid.Children.Add(textBlock);
            border.Child = grid;

            return border;
        }

        private void AddModeSelectionButtons()
        {
            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(0, 10, 0, 10)
            };

            var guideButton = CreateOptionButton("Guide", "📝");
            guideButton.Click += (s, e) =>
            {
                AddUserMessage("Step-by-step guide");
                AddHelperMessage("Great! Here's a step-by-step guide for: \"" + userTask + "\"");
                AddHelperMessage("(Written guide will be implemented in LLM stage. For now, try Realtime Help!)");
            };
            buttonPanel.Children.Add(guideButton);

            var realtimeButton = CreateOptionButton("Realtime Help", "🎯");
            realtimeButton.Click += (s, e) =>
            {
                AddUserMessage("Realtime help");
                AddHelperMessage("Perfect! Starting real-time help now...");
                
                // Wait a moment then switch to helper window
                var timer = new System.Windows.Threading.DispatcherTimer
                {
                    Interval = TimeSpan.FromMilliseconds(800)
                };
                timer.Tick += (sender, args) =>
                {
                    timer.Stop();
                    StartRealtimeHelp();
                };
                timer.Start();
            };
            buttonPanel.Children.Add(realtimeButton);

            ChatMessagesPanel.Children.Add(buttonPanel);
            ScrollToBottom();
        }

        private Button CreateOptionButton(string text, string icon)
        {
            var button = new Button
            {
                Content = icon + " " + text,
                FontSize = 16,
                FontWeight = FontWeights.SemiBold,
                Padding = new Thickness(20, 12, 20, 12),
                Margin = new Thickness(5),
                Background = new SolidColorBrush(Colors.White),
                BorderBrush = new SolidColorBrush(Color.FromRgb(0, 123, 255)),
                BorderThickness = new Thickness(2),
                Cursor = Cursors.Hand
            };

            button.MouseEnter += (s, e) =>
            {
                button.Background = new SolidColorBrush(Color.FromRgb(0, 123, 255));
                button.Foreground = new SolidColorBrush(Colors.White);
            };
            button.MouseLeave += (s, e) =>
            {
                button.Background = new SolidColorBrush(Colors.White);
                button.Foreground = new SolidColorBrush(Colors.Black);
            };

            return button;
        }

        private void StartRealtimeHelp()
        {
            var helperWindow = App.HelperWindowInstance;
            
            if (helperWindow == null)
            {
                MessageBox.Show("Helper window is not available.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            MockLLMService.Reset();
            var firstStep = MockLLMService.GetNextStep();
            helperWindow.DisplayMessage(firstStep);

            Application.Current.MainWindow?.Hide();
            helperWindow.Show();
            helperWindow.Activate();
        }

        private void ScrollToBottom()
        {
            ChatScrollViewer.ScrollToEnd();
        }
    }
}

