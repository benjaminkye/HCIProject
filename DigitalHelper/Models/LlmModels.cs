namespace DigitalHelper.Models
{
    public sealed class VisionReply
    {
        public string? answer { get; set; }
        public int[][] boxes { get; set; } = [];
        public string[] labels { get; set; } = [];
        public double[] scores { get; set; } = [];
        public Meta meta { get; set; } = new();
        public sealed class Meta { public Dim scaled { get; set; } = new() { w = 1000, h = 1000 }; public Dim native { get; set; } = new(); public string? question { get; set; } }
        public sealed class Dim { public int w, h; }
    }
    public sealed class ChatItem
    {
        public string Role { get; set; } = "user";
        public string? Text { get; set; }
        public System.Windows.Media.ImageSource? Preview { get; set; }
        public VisionReply? Raw { get; set; }
    }
}
