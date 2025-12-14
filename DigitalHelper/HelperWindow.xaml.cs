using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
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
        private string? currentUserTask;
        private string? currentBrowserHighlightSelector; 

        public ScreenOverlay? ScreenOverlayInstance => screenOverlay;

        public HelperWindow()
        {
            InitializeComponent();
            mainWindow = Application.Current.MainWindow;

            AvatarContainer.MouseLeftButtonUp += HelperAvatar_MouseLeftButtonUp;
            AvatarContainer.MouseMove += HelperAvatar_MouseMove;
            
            screenOverlay = new ScreenOverlay();
            
            DisplayMessage(new HelperGuidanceMessage
            {
                Icon = "👋",
                Instructions = "Hello! I'm your digital helper. You can move me by dragging me with your mouse, toggle the menu by left-clicking me, and show/hide my messages by right clicking me!",
                Buttons = null
            });
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

        public void DisplayMessage(HelperGuidanceMessage message)
        {
            var content = HelperMessageRenderer.RenderMessage(message, HandleButtonAction);
            
            SpeechBubbleContent.Content = content;
            
            SpeechBubble.Visibility = Visibility.Visible;
            SpeechBubbleTail.Visibility = Visibility.Visible;
            
            if (!string.IsNullOrEmpty(message.BrowserElementSelector))
            {
                currentBrowserHighlightSelector = message.BrowserElementSelector;
                Trace.WriteLine($"[HelperWindow] Stored browser highlight selector: {currentBrowserHighlightSelector}");
            }
            else if (message.BoundingBox == null)
            {
                Trace.WriteLine($"[HelperWindow] Clearing browser highlight selector (was: {currentBrowserHighlightSelector})");
                currentBrowserHighlightSelector = null;
            }
            
            if (screenOverlay != null)
            {
                if (message.BoundingBox != null)
                {
                    screenOverlay.DisplayBoundingBox(message.BoundingBox);
                    screenOverlay.Show();
                }
                else
                {
                    screenOverlay.ClearBoundingBox();
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
            };
            DisplayMessage(simpleMessage);
        }

        public void SetUserTask(string task)
        {
            currentUserTask = task;
        }

        private async void HandleButtonAction(string buttonId, string action)
        {
            switch (action)
            {
                case "dismiss":
                case "exit_help_mode":
                    currentUserTask = null;
                    currentBrowserHighlightSelector = null;
                    screenOverlay?.Hide();
                    
                    var bridge = BrowserBridgeService.Instance;
                    if (bridge.IsConnected)
                    {
                        await bridge.ClearHighlightAsync();
                    }
                    
                    SetHelperText("I hope I was of use! I'll be around if you need any more help!");
                    await Task.Delay(1000);
                    HideSpeechBubble();
                    
                    if (mainWindow != null)
                    {
                        mainWindow.Show();
                        mainWindow.Activate();
                    }
                    break;
                    
                case "take_screenshot":
                    // User clicked done, so repeat ss loop
                    if (string.IsNullOrEmpty(currentUserTask))
                    {
                        SetHelperText("No active task. Please start a new help session.");
                        return;
                    }

                    SetHelperText("⏳ Thinking... (This may take a few seconds)");

                    try
                    {
                        // Small delay in case some stuff closes because of clicking on "done"
                        await Task.Delay(1000);
                        var screenCaptureService = new ScreenCaptureService();
                        var shot = screenCaptureService.Capture1000(scale: false);

                        DomSummary? domContext = null;
                        var browserBridge = BrowserBridgeService.Instance;
                        Trace.WriteLine($"Browser bridge connected: {browserBridge.IsConnected}");
                        if (browserBridge.IsConnected)
                        {
                            Trace.WriteLine("Requesting DOM summary from browser...");
                            domContext = await browserBridge.RequestDomSummaryAsync();
                            if (domContext != null)
                            {
                                Trace.WriteLine($"DOM summary received: {domContext.Elements.Count} top-level elements from {domContext.Url}");
                            }
                            else
                            {
                                Trace.WriteLine("DOM summary returned null (timeout or error)");
                            }
                        }
                        else
                        {
                            Trace.WriteLine("Browser extension not connected - skipping DOM context");
                        }

                        var nextStep = await LLMService.Instance.AnalyzeScreenshotAsync(
                            shot.png1000,
                            currentUserTask,
                            shot.nativeW,
                            shot.nativeH,
                            domContext
                        );

                        if (!string.IsNullOrEmpty(nextStep.BrowserElementSelector) && browserBridge.IsConnected)
                        {
                            currentBrowserHighlightSelector = nextStep.BrowserElementSelector;

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
                                nextStep.BrowserElementSelector,
                                borderColor,
                                borderThickness
                            );

                            screenOverlay?.Hide();
                            var messageWithoutBox = new HelperGuidanceMessage
                            {
                                Icon = nextStep.Icon,
                                Instructions = nextStep.Instructions,
                                BoundingBox = null,
                                Buttons = nextStep.Buttons
                            };
                            DisplayMessage(messageWithoutBox);
                        }
                        else
                        {
                            currentBrowserHighlightSelector = null;
                            if (browserBridge.IsConnected)
                            {
                                await browserBridge.ClearHighlightAsync();
                            }
                            DisplayMessage(nextStep);
                        }
                    }
                    catch (Exception ex)
                    {
                        SetHelperText($"Error analyzing screen: {ex.Message}");
                    }
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

        public async Task RefreshBrowserHighlightAsync()
        {
            Trace.WriteLine($"[HelperWindow] RefreshBrowserHighlightAsync called. Current selector: {currentBrowserHighlightSelector ?? "null"}");
            
            if (string.IsNullOrEmpty(currentBrowserHighlightSelector))
            {
                Trace.WriteLine("[HelperWindow] No browser highlight active - skipping refresh");
                return;
            }

            var browserBridge = BrowserBridgeService.Instance;
            if (!browserBridge.IsConnected)
            {
                Trace.WriteLine("[HelperWindow] Browser not connected - skipping refresh");
                return;
            }

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

            Trace.WriteLine($"[HelperWindow] Refreshing browser highlight: selector={currentBrowserHighlightSelector}, color={borderColor}, thickness={borderThickness}");
            
            await browserBridge.HighlightElementAsync(
                currentBrowserHighlightSelector,
                borderColor,
                borderThickness
            );
            
            Trace.WriteLine("[HelperWindow] Browser highlight refresh complete");
        }
    }
}
