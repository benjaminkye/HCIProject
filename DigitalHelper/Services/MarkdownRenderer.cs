using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace DigitalHelper.Services
{
    /// <summary>
    /// Simple renderer for LLM output
    /// Handles bold (**text**), italic (*text*), and inline code (`text`)
    /// </summary>
    public static class MarkdownRenderer
    {
        public static TextBlock RenderToTextBlock(string markdownText, double fontSize = 16, Brush? foreground = null)
        {
            var textBlock = new TextBlock
            {
                TextWrapping = TextWrapping.Wrap,
                FontSize = fontSize,
                Foreground = foreground ?? new SolidColorBrush(Colors.Black)
            };

            ParseAndAddInlines(textBlock, markdownText);

            return textBlock;
        }

        public static void UpdateTextBlock(TextBlock textBlock, string markdownText)
        {
            textBlock.Inlines.Clear();
            ParseAndAddInlines(textBlock, markdownText);
        }

        private static void ParseAndAddInlines(TextBlock textBlock, string text)
        {
            if (string.IsNullOrEmpty(text))
                return;

            // Regex to match formatting
            var pattern = @"(\*\*([^\*]+?)\*\*)|(\*([^\*\n]+?)\*)|(`([^`]+?)`)";
            var regex = new Regex(pattern);

            int lastIndex = 0;

            foreach (Match match in regex.Matches(text))
            {
                if (match.Index > lastIndex)
                {
                    var plainText = text.Substring(lastIndex, match.Index - lastIndex);
                    textBlock.Inlines.Add(new Run(plainText));
                }

                if (match.Groups[1].Success) // bold
                {
                    var boldRun = new Run(match.Groups[2].Value)
                    {
                        FontWeight = FontWeights.Bold
                    };
                    textBlock.Inlines.Add(boldRun);
                }
                else if (match.Groups[3].Success) // italic
                {
                    var italicRun = new Run(match.Groups[4].Value)
                    {
                        FontStyle = FontStyles.Italic
                    };
                    textBlock.Inlines.Add(italicRun);
                }
                else if (match.Groups[5].Success) // code
                {
                    var codeRun = new Run(match.Groups[6].Value)
                    {
                        FontFamily = new FontFamily("Consolas, Courier New"),
                        Background = new SolidColorBrush(Color.FromRgb(240, 240, 240)),
                        Foreground = new SolidColorBrush(Color.FromRgb(214, 51, 132))
                    };
                    textBlock.Inlines.Add(codeRun);
                }

                lastIndex = match.Index + match.Length;
            }

            // Add any remaining text after the last match
            if (lastIndex < text.Length)
            {
                var remainingText = text.Substring(lastIndex);
                textBlock.Inlines.Add(new Run(remainingText));
            }
        }
    }
}

