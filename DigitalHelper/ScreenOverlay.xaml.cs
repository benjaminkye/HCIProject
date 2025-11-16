using System;
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
        private BoundingBox? currentBoundingBox;

        public ScreenOverlay()
        {
            InitializeComponent();
            
            this.Left = 0;
            this.Top = 0;
            this.Width = SystemParameters.VirtualScreenWidth;
            this.Height = SystemParameters.VirtualScreenHeight;
        }

        public void DisplayBoundingBox(BoundingBox boundingBox)
        {
            currentBoundingBox = boundingBox;
            RedrawBoundingBox();
        }

        public void RefreshBoundingBox()
        {
            if (currentBoundingBox != null)
            {
                RedrawBoundingBox();
            }
        }

        private void RedrawBoundingBox()
        {
            OverlayCanvas.Children.Clear();

            if (currentBoundingBox == null)
            {
                return;
            }

            DrawBoundingBox(currentBoundingBox);
        }

        private void DrawBoundingBox(BoundingBox box)
        {
            Brush strokeBrush;
            if (Application.Current.Resources.Contains("AppBorderColorBrush"))
            {
                strokeBrush = Application.Current.Resources["AppBorderColorBrush"] as Brush ?? Brushes.Blue;
            }
            else
            {
                strokeBrush = Brushes.Blue;
            }

            double thickness = 4;
            if (Application.Current.Properties.Contains("App.BorderThicknessOption"))
            {
                if (Application.Current.Properties["App.BorderThicknessOption"] is double d)
                {
                    thickness = d;
                }
            }

            var rectangle = new Rectangle
            {
                Width = box.Width,
                Height = box.Height,
                Stroke = strokeBrush,
                StrokeThickness = thickness,
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


        public void ClearBoundingBox()
        {
            currentBoundingBox = null;
            OverlayCanvas.Children.Clear();
        }
    }
}

