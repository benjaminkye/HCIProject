using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using DigitalHelper.Models;
using DigitalHelper.Services;

namespace DigitalHelper
{
    /// <summary>
    /// Interaction logic for HelperWindow.xaml
    /// </summary>
    public partial class HelperWindow : Window
    {
        private Window? mainWindow;
        private Point mouseDownPosition;
        private Point windowStartPosition;
        private bool isDragging = false;
        private const double DragThreshold = 5.0; // Drag threshold in case users have shaky hands
        private ScreenOverlay? screenOverlay;

        public HelperWindow()
        {
            InitializeComponent();
            mainWindow = Application.Current.MainWindow;

            AvatarContainer.MouseLeftButtonUp += HelperAvatar_MouseLeftButtonUp;
            AvatarContainer.MouseMove += HelperAvatar_MouseMove;
            
            screenOverlay = new ScreenOverlay();
            
            DisplayMessage(MockLLMService.GetWelcomeMessage());
        }

        private void HelperAvatar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                mouseDownPosition = PointToScreen(e.GetPosition(this));
                windowStartPosition = new Point(this.Left, this.Top);
                isDragging = false;
                
                AvatarContainer.CaptureMouse();
            }
        }

        private void HelperAvatar_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && AvatarContainer.IsMouseCaptured)
            {
                Point currentMousePosition = PointToScreen(e.GetPosition(this));
                
                Vector offset = currentMousePosition - mouseDownPosition;
                
                // check if beyond threshold
                if (!isDragging && (Math.Abs(offset.X) > DragThreshold || Math.Abs(offset.Y) > DragThreshold))
                {
                    isDragging = true;
                }
                
                if (isDragging)
                {
                    this.Left = windowStartPosition.X + offset.X;
                    this.Top = windowStartPosition.Y + offset.Y;
                }
            }
        }

        /// <summary>
        /// Handles mouse up on avatar - either toggles main window (if click) or ends drag
        /// </summary>
        private void HelperAvatar_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (AvatarContainer.IsMouseCaptured)
            {
                AvatarContainer.ReleaseMouseCapture();
            }

            if (!isDragging)
            {
                if (mainWindow != null)
                {
                    if (mainWindow.IsVisible)
                    {
                        mainWindow.Hide();
                    }
                    else
                    {
                        mainWindow.Show();
                        mainWindow.Activate();
                    }
                }
            }

            isDragging = false;
        }

        private void HelperAvatar_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            
            if (SpeechBubble.Visibility == Visibility.Visible)
            {
                SpeechBubble.Visibility = Visibility.Collapsed;
                SpeechBubbleTail.Visibility = Visibility.Collapsed;
            }
            else
            {
                SpeechBubble.Visibility = Visibility.Visible;
                SpeechBubbleTail.Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        /// Shows the helper window
        /// </summary>
        public new void Show()
        {
            base.Show();
            // Overlay will be shown automatically if there are bounding boxes in DisplayMessage
        }

        /// <summary>
        /// Hides the helper window and overlay
        /// </summary>
        public new void Hide()
        {
            base.Hide();
            screenOverlay?.Hide();
        }

        public void DisplayMessageFromJson(string json)
        {
            try
            {
                var message = JsonSerializer.Deserialize<HelperGuidanceMessage>(json);
                if (message != null)
                {
                    DisplayMessage(message);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error parsing message JSON: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void DisplayMessage(HelperGuidanceMessage message)
        {
            var content = HelperMessageRenderer.RenderMessage(message, HandleButtonAction);
            
            SpeechBubbleContent.Content = content;
            
            SpeechBubble.Visibility = Visibility.Visible;
            SpeechBubbleTail.Visibility = Visibility.Visible;
            
            if (screenOverlay != null)
            {
                if (message.BoundingBoxes != null && message.BoundingBoxes.Count > 0)
                {
                    screenOverlay.DisplayBoundingBoxes(message.BoundingBoxes);
                    screenOverlay.Show();
                }
                else
                {
                    screenOverlay.ClearBoundingBoxes();
                    screenOverlay.Hide();
                }
            }
        }

        /// <summary>
        /// Displays a simple text message
        /// </summary>
        public void SetHelperText(string message)
        {
            var simpleMessage = new HelperGuidanceMessage
            {
                Instructions = message,
                MessageType = "info"
            };
            DisplayMessage(simpleMessage);
        }

        private void HandleButtonAction(string buttonId, string action)
        {
            switch (action)
            {
                case "dismiss":
                case "exit_help_mode":
                    MockLLMService.Reset();
                    screenOverlay?.Hide();
                    if (mainWindow != null)
                    {
                        mainWindow.Show();
                        mainWindow.Activate();
                    }
                    break;
                    
                case "take_screenshot":
                    var nextStep = MockLLMService.GetNextStep();
                    DisplayMessage(nextStep);
                    break;
                    
                case "start_realtime_help":
                    MockLLMService.Reset();
                    var firstStep = MockLLMService.GetNextStep();
                    DisplayMessage(firstStep);
                    break;
                    
                default:
                    break;
            }
        }

        public void HideSpeechBubble()
        {
            SpeechBubble.Visibility = Visibility.Collapsed;
            SpeechBubbleTail.Visibility = Visibility.Collapsed;
        }

        public void ShowSpeechBubble()
        {
            SpeechBubble.Visibility = Visibility.Visible;
            SpeechBubbleTail.Visibility = Visibility.Visible;
        }
    }
}
