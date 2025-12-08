using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
namespace DigitalHelper.Views
{
    public partial class Settings : Page
    {
        bool _isInitializing;
        const string FontSizeOptionKey = "App.FontSizeOption";
        const string ThemeOptionKey = "App.ThemeOption";
        const string BorderColorOptionKey = "App.BorderColorOption";
        const string BorderThicknessOptionKey = "App.BorderThicknessOption";
        public Settings()
        {
            _isInitializing = true;
            InitializeComponent();
            InitializeControls();
            ApplyPulseAnimationToPreview();
            _isInitializing = false;
        }
        void ApplySelectedSettingsToResources()
        {
            if (FontSizeComboBox != null) ApplyFontSizeFromCombo();
            if (ThemeComboBox != null) ApplyThemeFromCombo();
            if (BorderColorComboBox != null) ApplyBorderColorFromCombo();
            if (BorderThicknessSlider != null) ApplyBorderThicknessFromSlider();
        }
        void InitializeControls()
        {
            if (FontSizeComboBox != null)
            {
                int index = 1;
                if (Application.Current.Properties.Contains(FontSizeOptionKey))
                {
                    string text = Application.Current.Properties[FontSizeOptionKey] as string ?? "Medium";
                    index = FontSizeLabelToIndex(text);
                }
                FontSizeComboBox.SelectedIndex = index;
            }
            if (ThemeComboBox != null)
            {
                int index = 0;
                if (Application.Current.Properties.Contains(ThemeOptionKey))
                {
                    string text = Application.Current.Properties[ThemeOptionKey] as string ?? "Light";
                    index = ThemeLabelToIndex(text);
                }
                ThemeComboBox.SelectedIndex = index;
            }
            if (BorderColorComboBox != null)
            {
                int index = 0;
                if (Application.Current.Properties.Contains(BorderColorOptionKey))
                {
                    string text = Application.Current.Properties[BorderColorOptionKey] as string ?? "Blue";
                    index = BorderColorLabelToIndex(text);
                }
                BorderColorComboBox.SelectedIndex = index;
            }
            if (BorderThicknessSlider != null)
            {
                if (Application.Current.Properties.Contains(BorderThicknessOptionKey))
                {
                    if (Application.Current.Properties[BorderThicknessOptionKey] is double d) BorderThicknessSlider.Value = d;
                    else BorderThicknessSlider.Value = 4;
                }
                else
                {
                    BorderThicknessSlider.Value = 4;
                }
            }
            ApplySelectedSettingsToResources();
            ApplyThemeFromCombo();
            ApplyFontSizeFromCombo();
            ApplyBorderColorFromCombo();
            ApplyBorderThicknessFromSlider();

            if (PreviewRectangle != null && BorderThicknessSlider != null)
            {
                PreviewRectangle.StrokeThickness = BorderThicknessSlider.Value;
            }
        }
        int FontSizeLabelToIndex(string text)
        {
            text = text.Trim();
            if (string.Equals(text, "Small", StringComparison.OrdinalIgnoreCase)) return 0;
            if (string.Equals(text, "Medium", StringComparison.OrdinalIgnoreCase)) return 1;
            if (string.Equals(text, "Large", StringComparison.OrdinalIgnoreCase)) return 2;
            if (string.Equals(text, "Extra Large", StringComparison.OrdinalIgnoreCase)) return 3;
            return 1;
        }
        int ThemeLabelToIndex(string text)
        {
            text = text.Trim();
            if (string.Equals(text, "Light", StringComparison.OrdinalIgnoreCase)) return 0;
            if (string.Equals(text, "Dark", StringComparison.OrdinalIgnoreCase)) return 1;
            if (string.Equals(text, "Auto", StringComparison.OrdinalIgnoreCase)) return 2;
            return 0;
        }
        int BorderColorLabelToIndex(string text)
        {
            text = text.Trim();
            if (string.Equals(text, "Blue", StringComparison.OrdinalIgnoreCase)) return 0;
            if (string.Equals(text, "Green", StringComparison.OrdinalIgnoreCase)) return 1;
            if (string.Equals(text, "Red", StringComparison.OrdinalIgnoreCase)) return 2;
            if (string.Equals(text, "Purple", StringComparison.OrdinalIgnoreCase)) return 3;
            if (string.Equals(text, "Orange", StringComparison.OrdinalIgnoreCase)) return 4;
            return 0;
        }
        void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.GoBack();
        }
        void FontSizeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isInitializing) return;
            ApplyFontSizeFromCombo();
        }
        void ThemeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isInitializing) return;
            ApplyThemeFromCombo();
        }
        void BorderColorComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isInitializing) return;
            ApplyBorderColorFromCombo();
        }
        void BorderThicknessSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_isInitializing) return;
            ApplyBorderThicknessFromSlider();
        }
        void ApplyFontSizeFromCombo()
        {
            if (FontSizeComboBox.SelectedItem is not ComboBoxItem item) return;
            string text = (item.Content?.ToString() ?? string.Empty).Trim();
            double body;
            double heading;
            double title;
            if (string.Equals(text, "Small", StringComparison.OrdinalIgnoreCase))
            {
                body = 12;
                heading = 16;
                title = 26;
            }
            else if (string.Equals(text, "Large", StringComparison.OrdinalIgnoreCase))
            {
                body = 16;
                heading = 20;
                title = 34;
            }
            else if (string.Equals(text, "Extra Large", StringComparison.OrdinalIgnoreCase))
            {
                body = 18;
                heading = 22;
                title = 38;
            }
            else
            {
                body = 14;
                heading = 18;
                title = 30;
            }
            Application.Current.Resources["BodyFontSize"] = body;
            Application.Current.Resources["HeadingFontSize"] = heading;
            Application.Current.Resources["TitleFontSize"] = title;
            
            Application.Current.Resources["IconFontSize"] = body * 2.3;
            Application.Current.Resources["BodyLineHeight"] = body * 1.5;
            Application.Current.Resources["ButtonIconFontSize"] = body * 1.1;
            
            Application.Current.Properties[FontSizeOptionKey] = text;
        }
        void ApplyThemeFromCombo()
        {
            if (ThemeComboBox.SelectedItem is not ComboBoxItem item) return;
            string text = (item.Content?.ToString() ?? string.Empty).Trim();
            Application.Current.Properties[ThemeOptionKey] = text;
            if (string.Equals(text, "Dark", StringComparison.OrdinalIgnoreCase)) ApplyDarkTheme();
            else ApplyLightTheme();
        }
        void ApplyLightTheme()
        {
            SetBrush("BackgroundBrush", Color.FromRgb(254, 245, 212));
            SetBrush("ContentBackgroundBrush", Color.FromRgb(255, 254, 240));
            SetBrush("ChatBackgroundBrush", Color.FromArgb(0x73, 0x85, 0xB7, 0xE5));
            SetBrush("TextDarkBrush", Color.FromRgb(26, 26, 26));
            SetBrush("TextMediumBrush", Color.FromRgb(44, 44, 44));
            SetBrush("TextLightBrush", Color.FromRgb(102, 102, 102));
            SetBrush("BorderLightBrush", Color.FromRgb(245, 217, 138));
            SetBrush("BorderMediumBrush", Color.FromRgb(252, 171, 20));
            SetBrush("DarkSidebarBrush", Color.FromRgb(253, 184, 54));
            SetBrush("SidebarHoverBrush", Color.FromRgb(252, 171, 20));
            SetBrush("WindowBorderBrush", Color.FromRgb(224, 154, 16));
            SetBrush("TopBarBrush", Color.FromRgb(252, 171, 20));
            SetBrush("ReadOnlyBackgroundBrush", Color.FromRgb(255, 249, 230));
            SetBrush("SectionBackgroundBrush", Color.FromRgb(254, 245, 212));
        }
        void ApplyDarkTheme()
        {
            SetBrush("BackgroundBrush", Color.FromRgb(18, 18, 18));
            SetBrush("ContentBackgroundBrush", Color.FromRgb(32, 32, 32));
            SetBrush("ChatBackgroundBrush", Color.FromRgb(24, 24, 24));
            SetBrush("TextDarkBrush", Color.FromRgb(245, 245, 245));
            SetBrush("TextMediumBrush", Color.FromRgb(210, 210, 210));
            SetBrush("TextLightBrush", Color.FromRgb(160, 160, 160));
            SetBrush("BorderLightBrush", Color.FromRgb(70, 70, 70));
            SetBrush("BorderMediumBrush", Color.FromRgb(110, 110, 110));
            SetBrush("DarkSidebarBrush", Color.FromRgb(40, 40, 40));
            SetBrush("SidebarHoverBrush", Color.FromRgb(70, 70, 70));
            SetBrush("WindowBorderBrush", Color.FromRgb(90, 90, 90));
            SetBrush("TopBarBrush", Color.FromRgb(60, 60, 60));
            SetBrush("ReadOnlyBackgroundBrush", Color.FromRgb(35, 35, 35));
            SetBrush("SectionBackgroundBrush", Color.FromRgb(30, 30, 30));
        }
        void ApplyBorderColorFromCombo()
        {
            if (BorderColorComboBox.SelectedItem is not ComboBoxItem item) return;
            string text = (item.Content?.ToString() ?? string.Empty).Trim();
            Application.Current.Properties[BorderColorOptionKey] = text;
            Color borderColor;
            if (string.Equals(text, "Green", StringComparison.OrdinalIgnoreCase)) borderColor = Color.FromRgb(76, 175, 80);
            else if (string.Equals(text, "Red", StringComparison.OrdinalIgnoreCase)) borderColor = Color.FromRgb(244, 67, 54);
            else if (string.Equals(text, "Purple", StringComparison.OrdinalIgnoreCase)) borderColor = Color.FromRgb(156, 39, 176);
            else if (string.Equals(text, "Orange", StringComparison.OrdinalIgnoreCase)) borderColor = Color.FromRgb(244, 162, 97);
            else borderColor = Color.FromRgb(0, 123, 255);
            SetBrush("AppBorderColorBrush", borderColor);
            //SetBrush("BorderMediumBrush", borderColor);
            RefreshOverlayIfActive();
        }
        void ApplyBorderThicknessFromSlider()
        {
            double v = BorderThicknessSlider.Value;
            Application.Current.Properties[BorderThicknessOptionKey] = v;
            Application.Current.Resources["AppBorderThickness"] = new Thickness(v);
            
            if (PreviewRectangle != null)
            {
                PreviewRectangle.StrokeThickness = v;
            }
            
            RefreshOverlayIfActive();
        }
        void RefreshOverlayIfActive()
        {
            App.HelperWindowInstance?.ScreenOverlayInstance?.RefreshBoundingBox();
        }
        void ApplyPulseAnimationToPreview()
        {
            if (PreviewRectangle != null)
            {
                var animation = new DoubleAnimation
                {
                    From = 1.0,
                    To = 0.5,
                    Duration = TimeSpan.FromSeconds(1),
                    AutoReverse = true,
                    RepeatBehavior = RepeatBehavior.Forever
                };
                PreviewRectangle.BeginAnimation(UIElement.OpacityProperty, animation);
            }
        }
        
        static void SetBrush(string key, Color color)
        {
            object existing = Application.Current.Resources[key];
            if (existing is SolidColorBrush brush)
            {
                if (brush.IsFrozen)
                {
                    SolidColorBrush clone = brush.Clone();
                    clone.Color = color;
                    Application.Current.Resources[key] = clone;
                }
                else
                {
                    brush.Color = color;
                }
            }
            else
            {
                Application.Current.Resources[key] = new SolidColorBrush(color);
            }
        }
    }
}