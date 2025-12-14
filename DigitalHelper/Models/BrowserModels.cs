using System.Collections.Generic;

namespace DigitalHelper.Models
{
    public class DomSummary
    {
        public string Url { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public List<DomElement> Elements { get; set; } = new List<DomElement>();
    }
    public class DomElement
    {
        public string Id { get; set; } = string.Empty;
        public string Selector { get; set; } = string.Empty;
        public string Tag { get; set; } = string.Empty;
        public string? Text { get; set; }
        public string? Type { get; set; }
        public string? Placeholder { get; set; }
        public string? AriaLabel { get; set; }
        public string? Role { get; set; }
        public DomRect? Rect { get; set; }
        public List<DomElement> Children { get; set; } = new List<DomElement>();
    }
    public class DomRect
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }
    public class BrowserMessage
    {
        public string Type { get; set; } = string.Empty;
        public object? Data { get; set; }
        public int? TabId { get; set; }
        public bool? Connected { get; set; }
        public string? Message { get; set; }
        public string? Selector { get; set; }
        public string? Color { get; set; }
        public double? Thickness { get; set; }
        public string? FontSize { get; set; }
        public bool? Enabled { get; set; }
    }
}
