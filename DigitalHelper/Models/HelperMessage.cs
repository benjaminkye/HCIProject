using System;
using System.Collections.Generic;

namespace DigitalHelper.Models
{
    /// <summary>
    /// Represents a guidance message from the LLM for real-time help mode.
    /// Each message is based solely on current screenshot analysis.
    /// </summary>
    public class HelperGuidanceMessage
    {
        public string? Icon { get; set; }
        public string Instructions { get; set; } = string.Empty;
        public BoundingBox? BoundingBox { get; set; }
        public List<HelperButton>? Buttons { get; set; }
    }

    /// <summary>
    /// Represents a UI element to highlight on screen
    /// </summary>
    public class BoundingBox
    {
        public string Id { get; set; } = string.Empty;
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public string? Label { get; set; }
        public string Color { get; set; } = "#00FF00";
        public string Style { get; set; } = "solid"; // solid or dashed
        public bool PulseAnimation { get; set; } = true;
    }

    /// <summary>
    /// Represents an action button in the helper window
    /// </summary>
    public class HelperButton
    {
        public string Id { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string? Icon { get; set; }
        public string Style { get; set; } = "primary"; // primary, secondary
    }
}

