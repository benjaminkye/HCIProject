using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using DigitalHelper.Models;

namespace DigitalHelper
{
    /// <summary>
    /// Transparent overlay window for displaying bounding boxes and highlights
    /// </summary>
    public partial class ScreenOverlay : Window
    {
        public ScreenOverlay()
        {
            InitializeComponent();
            
            this.Left = 0;
            this.Top = 0;
            this.Width = SystemParameters.VirtualScreenWidth;
            this.Height = SystemParameters.VirtualScreenHeight;
        }

        public void DisplayBoundingBoxes(List<BoundingBox> boundingBoxes)
        {
            OverlayCanvas.Children.Clear();

            if (boundingBoxes == null || boundingBoxes.Count == 0)
            {
                return;
            }

            foreach (var box in boundingBoxes)
            {
                DrawBoundingBox(box);
            }
        }

        private void DrawBoundingBox(BoundingBox box)
        {
            var rectangle = new Rectangle
            {
                Width = box.Width,
                Height = box.Height,
                Stroke = GetBrushFromHex(box.Color),
                StrokeThickness = 4,
                StrokeDashArray = box.Style == "dashed" ? new DoubleCollection { 5, 3 } : null,
                Fill = Brushes.Transparent
            };

            Canvas.SetLeft(rectangle, box.X);
            Canvas.SetTop(rectangle, box.Y);

            OverlayCanvas.Children.Add(rectangle);

            if (box.PulseAnimation)
            {
                AddPulseAnimation(rectangle);
            }

            if (!string.IsNullOrEmpty(box.Label))
            {
                DrawLabel(box);
            }
        }

        private void DrawLabel(BoundingBox box)
        {
            var label = new Border
            {
                Background = GetBrushFromHex(box.Color),
                CornerRadius = new CornerRadius(4),
                Padding = new Thickness(8, 4, 8, 4),
                Child = new TextBlock
                {
                    Text = box.Label,
                    Foreground = Brushes.White,
                    FontSize = 16,
                    FontWeight = FontWeights.Bold
                }
            };

            Canvas.SetLeft(label, box.X);
            Canvas.SetTop(label, box.Y - 35);

            OverlayCanvas.Children.Add(label);
        }

        private void AddPulseAnimation(UIElement element)
        {
            var animation = new DoubleAnimation
            {
                From = 1.0,
                To = 0.5,
                Duration = TimeSpan.FromSeconds(1),
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever
            };

            element.BeginAnimation(UIElement.OpacityProperty, animation);
        }

        private Brush GetBrushFromHex(string hex)
        {
            try
            {
                var result = new BrushConverter().ConvertFrom(hex);
                return (result as Brush) ?? Brushes.LimeGreen;
            }
            catch
            {
                return Brushes.LimeGreen; // Default color, can specify with options later
            }
        }

        public void ClearBoundingBoxes()
        {
            OverlayCanvas.Children.Clear();
        }
    }
}

