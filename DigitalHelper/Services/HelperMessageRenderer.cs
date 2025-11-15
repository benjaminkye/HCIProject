using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using DigitalHelper.Models;

namespace DigitalHelper.Services
{
    /// <summary>
    /// Renders HelperGuidanceMessage objects into WPF UI elements
    /// </summary>
    public static class HelperMessageRenderer
    {
        public static UIElement RenderMessage(
            HelperGuidanceMessage message, 
            Action<string, string> onButtonClick)
        {
            var mainPanel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            // Add icons, instructions, and buttons
            if (!string.IsNullOrEmpty(message.Icon))
            {
                mainPanel.Children.Add(CreateIcon(message.Icon));
            }

            mainPanel.Children.Add(CreateInstructions(message.Instructions));

            if (message.Buttons?.Count > 0)
            {
                mainPanel.Children.Add(CreateButtons(message.Buttons, onButtonClick));
            }

            return mainPanel;
        }

        private static UIElement CreateIcon(string icon)
        {
            var textBlock = new TextBlock
            {
                Text = icon,
                FontSize = 32,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 5, 0, 10)
            };

            return textBlock;
        }

        private static UIElement CreateInstructions(string instructions)
        {
            var textBlock = new TextBlock
            {
                Text = instructions,
                TextWrapping = TextWrapping.Wrap,
                FontSize = 16, // Larger for elderly users, later get from options service once implemented
                FontFamily = new FontFamily("Segoe UI"),
                TextAlignment = TextAlignment.Center,
                Foreground = (Brush)Application.Current.Resources["TextDarkBrush"],
                Margin = new Thickness(0, 0, 0, 15),
                LineHeight = 24
            };

            return textBlock;
        }

        private static UIElement CreateButtons(
            System.Collections.Generic.List<HelperButton> buttons,
            Action<string, string> onButtonClick)
        {
            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 5, 0, 0)
            };

            foreach (var buttonDef in buttons)
            {
                var button = CreateButton(buttonDef, onButtonClick);
                buttonPanel.Children.Add(button);
            }

            return buttonPanel;
        }

        private static Button CreateButton(HelperButton buttonDef, Action<string, string> onButtonClick)
        {
            var button = new Button
            {
                Content = CreateButtonContent(buttonDef),
                MinWidth = 150,
                MinHeight = 40,
                Margin = new Thickness(5),
                Padding = new Thickness(15, 8, 15, 8),
                FontSize = 14,
                FontWeight = FontWeights.SemiBold,
                Cursor = System.Windows.Input.Cursors.Hand,
                Background = GetBackgroundForButtonStyle(buttonDef.Style),
                Foreground = GetForegroundForButtonStyle(buttonDef.Style),
                BorderThickness = new Thickness(0),
                Effect = new DropShadowEffect
                {
                    Color = Colors.Gray,
                    BlurRadius = 5,
                    ShadowDepth = 2,
                    Opacity = 0.3
                }
            };

            var border = new Border
            {
                CornerRadius = new CornerRadius(8)
            };

            button.Click += (s, e) => onButtonClick(buttonDef.Id, buttonDef.Action);

            button.MouseEnter += (s, e) =>
            {
                button.Opacity = 0.85;
            };
            button.MouseLeave += (s, e) =>
            {
                button.Opacity = 1.0;
            };

            return button;
        }

        private static object CreateButtonContent(HelperButton buttonDef)
        {
            if (string.IsNullOrEmpty(buttonDef.Icon))
            {
                return buttonDef.Text;
            }

            var panel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            panel.Children.Add(new TextBlock
            {
                Text = buttonDef.Icon + " ",
                FontSize = 16,
                VerticalAlignment = VerticalAlignment.Center
            });

            panel.Children.Add(new TextBlock
            {
                Text = buttonDef.Text,
                VerticalAlignment = VerticalAlignment.Center
            });

            return panel;
        }

        private static Brush GetBackgroundForButtonStyle(string style)
        {
            return style switch
            {
                "primary" => new SolidColorBrush(Color.FromRgb(0, 120, 255)),
                "secondary" => new SolidColorBrush(Color.FromRgb(110, 117, 125)),
                _ => new SolidColorBrush(Color.FromRgb(0, 120, 255))
            };
        }

        private static Brush GetForegroundForButtonStyle(string style)
        {
            return new SolidColorBrush(Colors.White);
        }
    }
}

