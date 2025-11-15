using DigitalHelper.Services;
using Google.GenAI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

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
                App.HelperWindowInstance?.SetUserTask(userTask);
                isFirstMessage = false;
                
                // Helper responds with mode selection
                AddHelperMessage("Would you like a step-by-step guide or realtime guidance?");
                AddHelperMessage("The step-by-step guide will provide detailed instructions you can follow at your own pace, while the realtime help will analyze your screen and provide assistance if you ever feel stuck.");
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
            
            TextBlock textBlock;
            if (isHelper)
            {
                textBlock = MarkdownRenderer.RenderToTextBlock(message, 16, new SolidColorBrush(Colors.Black));
            }
            else
            {
                textBlock = new TextBlock
                {
                    Text = message,
                    TextWrapping = TextWrapping.Wrap,
                    FontSize = 16,
                    Foreground = new SolidColorBrush(Colors.White)
                };
            }

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
            guideButton.Click += async (s, e) =>
            {
                AddUserMessage("Step-by-step guide");
                await GenerateAndDisplayGuide();
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

        private async System.Threading.Tasks.Task GenerateAndDisplayGuide()
        {
            try
            {
                var streamingBubble = CreateStreamingMessageBubble();
                ChatMessagesPanel.Children.Add(streamingBubble.Border);
                
                streamingBubble.TextBlock.Text = "⏳ Thinking...";
                ScrollToBottom();
                
                var fullText = new System.Text.StringBuilder();
                bool firstChunk = true;
                
                await foreach (var chunk in LLMService.Instance.GetHelpGuideStream(userTask))
                {
                    if (firstChunk)
                    {
                        streamingBubble.TextBlock.Text = "";
                        firstChunk = false;
                    }
                    
                    fullText.Append(chunk);
                    
                    await Dispatcher.InvokeAsync(() =>
                    {
                        MarkdownRenderer.UpdateTextBlock(streamingBubble.TextBlock, fullText.ToString());
                        ScrollToBottom();
                    });
                }

                // Debug line
                System.Diagnostics.Debug.WriteLine("=== RAW MARKDOWN OUTPUT ===");
                System.Diagnostics.Debug.WriteLine(fullText.ToString());
                System.Diagnostics.Debug.WriteLine("=== END RAW OUTPUT ===");
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Error generating guide: {ex.Message}");
                AddHelperMessage($"Sorry, I encountered an error while generating the guide: {ex.Message}");
            }
        }

        private (Border Border, TextBlock TextBlock) CreateStreamingMessageBubble()
        {
            var textBlock = new TextBlock
            {
                Text = "",
                TextWrapping = TextWrapping.Wrap,
                FontSize = 16,
                Foreground = new SolidColorBrush(Colors.Black)
            };

            var grid = new Grid();
            grid.Children.Add(textBlock);

            var border = new Border
            {
                Background = new SolidColorBrush(Colors.White),
                CornerRadius = new CornerRadius(15),
                Padding = new Thickness(15, 10, 15, 10),
                Margin = new Thickness(0, 5, 100, 5),
                HorizontalAlignment = HorizontalAlignment.Left,
                MaxWidth = 600,
                Child = grid
            };

            return (border, textBlock);
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

        private async void StartRealtimeHelp()
        {
            var helperWindow = App.HelperWindowInstance;
            
            if (helperWindow == null)
            {
                MessageBox.Show("Helper window is not available.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            Application.Current.MainWindow?.Hide();
            helperWindow.Show();
            helperWindow.Activate();

            helperWindow.SetHelperText("⏳ Thinking... (This may take a few seconds)");

            try
            {
                var screenCaptureService = new ScreenCaptureService();
                var shot = screenCaptureService.Capture1000(scale: false);

                var step = await LLMService.Instance.AnalyzeScreenshotAsync(
                    shot.png1000,
                    userTask,
                    shot.nativeW,
                    shot.nativeH
                );

                helperWindow.DisplayMessage(step);
            }
            catch (ClientError cex)
            {
                Trace.WriteLine($"Gemini {cex.StatusCode} error: {cex.Message}");
                helperWindow.SetHelperText($"Error analyzing screen: {cex.Message}");
            }
            catch (HttpRequestException hex)
            {
                Trace.WriteLine($"HTTP error: {hex.Message}");
                helperWindow.SetHelperText($"Error analyzing screen: {hex.Message}");
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Unexpected error: {ex.Message}");
                helperWindow.SetHelperText($"Unexpected error: {ex.Message}");
            }
        }

        private void ScrollToBottom()
        {
            ChatScrollViewer.ScrollToEnd();
        }
    }
}

