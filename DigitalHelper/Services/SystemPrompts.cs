namespace DigitalHelper.Services
{
    public static class SystemPrompts
    {
        public const string VisionJsonStrict = @"
Return ONLY strict JSON (no prose, no backticks) with this schema:
{
  ""answer"": string,
  ""boxes"": [[ymin, xmin, ymax, xmax], ...]
}
Rules:
- Coordinates are integers in a 1000×1000 space AFTER letterbox/pillarbox scaling.
- If no regions are relevant, boxes/labels/scores are empty arrays.
- Be concise in ""answer"". Never include secrets/PII.
";
    }
}
