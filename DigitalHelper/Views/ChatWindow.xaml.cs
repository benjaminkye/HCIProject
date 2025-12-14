using DigitalHelper.Services;
using Google.GenAI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
namespace DigitalHelper.Views
{
    public partial class ChatWindow : Page
    {
        bool isFirstMessage = true;
        string userTask = string.Empty;
        public ChatWindow()
        {
            InitializeComponent();
            AddHelperMessage("Hi! What do you need help with?");
        }
        void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.GoBack();
        }
        void NewChatButton_Click(object sender, RoutedEventArgs e)
        {
            ChatMessagesPanel.Children.Clear();
            isFirstMessage = true;
            userTask = string.Empty;
            AddHelperMessage("Hi! What do you need help with?");
        }
        void MessageInput_GotFocus(object sender, RoutedEventArgs e)
        {
            if (MessageInput.Text == "Type your request here...")
            {
                MessageInput.Text = "";
                MessageInput.Foreground = (Brush)Application.Current.Resources["TextDarkBrush"];
            }
        }
        void MessageInput_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(MessageInput.Text))
            {
                MessageInput.Text = "Type your request here...";
                MessageInput.Foreground = (Brush)Application.Current.Resources["TextLightBrush"];
            }
        }
        void MessageInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) SendMessage();
        }
        void SendButton_Click(object sender, RoutedEventArgs e)
        {
            SendMessage();
        }
        void SendMessage()
        {
            string message = MessageInput.Text.Trim();
            if (string.IsNullOrWhiteSpace(message) || message == "Type your request here...") return;
            AddUserMessage(message);
            MessageInput.Text = "";
            MessageInput.Focus();
            if (isFirstMessage)
            {
                userTask = message;
                App.HelperWindowInstance?.SetUserTask(userTask);
                isFirstMessage = false;
                AddHelperMessage("Would you like a step-by-step guide or realtime guidance?");
                AddHelperMessage("The step-by-step guide will provide detailed instructions you can follow at your own pace, while the realtime help will analyze your screen and provide assistance if you ever feel stuck.");
                AddModeSelectionButtons();
            }
            else
            {
                AddHelperMessage("Please select one of the two options above or start a new chat.");
            }
        }
        void AddHelperMessage(string message)
        {
            var bubble = CreateMessageBubble(message, true);
            ChatMessagesPanel.Children.Add(bubble);
            ScrollToBottom();
        }
        void AddUserMessage(string message)
        {
            var bubble = CreateMessageBubble(message, false);
            ChatMessagesPanel.Children.Add(bubble);
            ScrollToBottom();
        }
        UIElement CreateMessageBubble(string message, bool isHelper)
        {
            var border = new Border
            {
                CornerRadius = new CornerRadius(15),
                Padding = new Thickness(15, 10, 15, 10),
                Margin = new Thickness(isHelper ? 0 : 100, 5, isHelper ? 100 : 0, 5),
                HorizontalAlignment = isHelper ? HorizontalAlignment.Left : HorizontalAlignment.Right,
                MaxWidth = 600
            };
            if (isHelper)
            {
                border.Background = (Brush)Application.Current.Resources["ContentBackgroundBrush"];
            }
            else
            {
                border.Background = (Brush)Application.Current.Resources["PrimaryBlueBrush"];
            }
            var grid = new Grid();
            TextBlock textBlock;
            if (isHelper)
            {
                double fontSize = (double)Application.Current.Resources["BodyFontSize"];
                var foreground = (Brush)Application.Current.Resources["TextDarkBrush"];
                textBlock = MarkdownRenderer.RenderToTextBlock(message, fontSize, foreground);
                textBlock.SetResourceReference(TextBlock.FontSizeProperty, "BodyFontSize");
                textBlock.SetResourceReference(TextBlock.ForegroundProperty, "TextDarkBrush");
            }
            else
            {
                double fontSize = (double)Application.Current.Resources["BodyFontSize"];
                textBlock = new TextBlock
                {
                    Text = message,
                    TextWrapping = TextWrapping.Wrap,
                    FontSize = fontSize,
                    Foreground = (Brush)Application.Current.Resources["WhiteBrush"]
                };
                textBlock.SetResourceReference(TextBlock.FontSizeProperty, "BodyFontSize");
            }
            grid.Children.Add(textBlock);
            border.Child = grid;
            return border;
        }
        void AddModeSelectionButtons()
        {
            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(0, 10, 0, 10)
            };
            var guideButton = CreateOptionButton("Guide", "📝");
            guideButton.Click += async (s, e) => {
                AddUserMessage("Step-by-step guide");
                await GenerateAndDisplayGuide();
            };
            buttonPanel.Children.Add(guideButton);
            var realtimeButton = CreateOptionButton("Realtime Help", "🎯");
            realtimeButton.Click += (s, e) => {
                AddUserMessage("Realtime help");
                AddHelperMessage("Perfect! Starting real-time help now...");
                var timer = new System.Windows.Threading.DispatcherTimer
                {
                    Interval = TimeSpan.FromMilliseconds(800)
                };
                timer.Tick += (sender, args) => {
                    timer.Stop();
                    StartRealtimeHelp();
                };
                timer.Start();
            };
            buttonPanel.Children.Add(realtimeButton);
            ChatMessagesPanel.Children.Add(buttonPanel);
            ScrollToBottom();
        }
        async Task GenerateAndDisplayGuide()
        {
            try
            {
                var streamingBubble = CreateStreamingMessageBubble();
                ChatMessagesPanel.Children.Add(streamingBubble.Border);
                streamingBubble.TextBlock.Text = "⏳ Thinking...";
                ScrollToBottom();
                var fullText = new StringBuilder();
                bool firstChunk = true;
                await foreach (var chunk in LLMService.Instance.GetHelpGuideStream(userTask))
                {
                    if (firstChunk)
                    {
                        streamingBubble.TextBlock.Text = "";
                        firstChunk = false;
                    }
                    fullText.Append(chunk);
                    await Dispatcher.InvokeAsync(() => {
                        MarkdownRenderer.UpdateTextBlock(streamingBubble.TextBlock, fullText.ToString());
                        ScrollToBottom();
                    });
                }
                Debug.WriteLine("=== RAW MARKDOWN OUTPUT ===");
                Debug.WriteLine(fullText.ToString());
                Debug.WriteLine("=== END RAW OUTPUT ===");
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Error generating guide: {ex.Message}");
                AddHelperMessage($"Sorry, I encountered an error while generating the guide: {ex.Message}");
            }
        }
        (Border Border, TextBlock TextBlock) CreateStreamingMessageBubble()
        {
            var textBlock = new TextBlock
            {
                Text = "",
                TextWrapping = TextWrapping.Wrap
            };
            textBlock.SetResourceReference(TextBlock.FontSizeProperty, "BodyFontSize");
            textBlock.SetResourceReference(TextBlock.ForegroundProperty, "TextDarkBrush");
            var grid = new Grid();
            grid.Children.Add(textBlock);
            var border = new Border
            {
                CornerRadius = new CornerRadius(15),
                Padding = new Thickness(15, 10, 15, 10),
                Margin = new Thickness(0, 5, 100, 5),
                HorizontalAlignment = HorizontalAlignment.Left,
                MaxWidth = 600
            };
            border.SetResourceReference(Border.BackgroundProperty, "ContentBackgroundBrush");
            border.Child = grid;
            return (border, textBlock);
        }
        Button CreateOptionButton(string text, string icon)
        {
            var button = new Button
            {
                Content = icon + " " + text,
                FontWeight = FontWeights.SemiBold,
                Padding = new Thickness(20, 12, 20, 12),
                Margin = new Thickness(5),
                Cursor = Cursors.Hand
            };
            button.SetResourceReference(Button.FontSizeProperty, "BodyFontSize");
            button.SetResourceReference(Button.BorderThicknessProperty, "AppBorderThickness");
            button.SetResourceReference(Button.BorderBrushProperty, "BorderMediumBrush");
            button.SetResourceReference(Button.BackgroundProperty, "ContentBackgroundBrush");
            button.Foreground = (Brush)Application.Current.Resources["TextDarkBrush"];
            button.MouseEnter += (s, e) => {
                button.Background = (Brush)Application.Current.Resources["PrimaryBlueBrush"];
                button.Foreground = (Brush)Application.Current.Resources["WhiteBrush"];
            };
            button.MouseLeave += (s, e) => {
                button.Background = (Brush)Application.Current.Resources["ContentBackgroundBrush"];
                button.Foreground = (Brush)Application.Current.Resources["TextDarkBrush"];
            };
            return button;
        }
        async void StartRealtimeHelp()
        {
            var helperWindow = App.HelperWindowInstance;
            if (helperWindow == null)
            {
                MessageBox.Show("Helper window is not available.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            AddHelperMessage("Realtime help started! If this window does not minimize automatically, feel free to minimize it yourself and direct your attention to the helper avatar!");
            Application.Current.MainWindow?.Hide();
            helperWindow.Show();
            helperWindow.Activate();
            helperWindow.SetHelperText("⏳ Thinking... (This may take a few seconds)");
            try
            {
                var screenCaptureService = new ScreenCaptureService();
                var shot = screenCaptureService.Capture1000(scale: false);
                
                Models.DomSummary? domContext = null;
                var browserBridge = BrowserBridgeService.Instance;
                Trace.WriteLine($"[Initial Help] Browser bridge connected: {browserBridge.IsConnected}");
                if (browserBridge.IsConnected)
                {
                    Trace.WriteLine("[Initial Help] Requesting DOM summary from browser...");
                    domContext = await browserBridge.RequestDomSummaryAsync();
                    if (domContext != null)
                    {
                        Trace.WriteLine($"[Initial Help] DOM summary received: {domContext.Elements.Count} top-level elements from {domContext.Url}");
                    }
                    else
                    {
                        Trace.WriteLine("[Initial Help] DOM summary returned null (timeout or error)");
                    }
                }
                else
                {
                    Trace.WriteLine("[Initial Help] Browser extension not connected - skipping DOM context");
                }
                
                var step = await LLMService.Instance.AnalyzeScreenshotAsync(shot.png1000, userTask, shot.nativeW, shot.nativeH, domContext);
                
                if (!string.IsNullOrEmpty(step.BrowserElementSelector) && browserBridge.IsConnected)
                {
                    string borderColor = "#00FF00";
                    if (Application.Current.Resources.Contains("AppBorderColorBrush"))
                    {
                        if (Application.Current.Resources["AppBorderColorBrush"] is SolidColorBrush brush)
                        {
                            borderColor = $"#{brush.Color.R:X2}{brush.Color.G:X2}{brush.Color.B:X2}";
                        }
                    }

                    double borderThickness = 4.0;
                    if (Application.Current.Properties.Contains("App.BorderThicknessOption"))
                    {
                        if (Application.Current.Properties["App.BorderThicknessOption"] is double d)
                        {
                            borderThickness = d;
                        }
                    }

                    await browserBridge.HighlightElementAsync(
                        step.BrowserElementSelector,
                        borderColor,
                        borderThickness
                    );

                    var messageWithoutBox = new Models.HelperGuidanceMessage
                    {
                        Icon = step.Icon,
                        Instructions = step.Instructions,
                        BoundingBox = null,
                        BrowserElementSelector = step.BrowserElementSelector,
                        Buttons = step.Buttons
                    };
                    helperWindow.DisplayMessage(messageWithoutBox);
                }
                else
                {
                    if (browserBridge.IsConnected)
                    {
                        await browserBridge.ClearHighlightAsync();
                    }
                    helperWindow.DisplayMessage(step);
                }
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
        void ScrollToBottom()
        {
            ChatScrollViewer.ScrollToEnd();
        }
    }
}