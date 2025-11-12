namespace DigitalHelper.Services
{
    public static class SystemPrompts
    {
        public const string VisionJsonStrict = @"
Return ONLY strict JSON (no prose, no backticks) with this schema:
{
  ""answer"": string,
  ""boxes"": [[ymin, xmin, ymax, xmax], ...],
  ""labels"": [""label for box 0"", ""label for box 1"", ...],
  ""scores"": [0.0-1.0, ...],
  ""meta"": { ""question"": string, ""scaled"": {""w"":1000,""h"":1000}, ""native"": {""w"": number, ""h"": number} }
}
Rules:
- Coordinates are integers in a 1000×1000 space AFTER letterbox/pillarbox scaling.
- If no regions are relevant, boxes/labels/scores are empty arrays.
- Be concise in ""answer"". Never include secrets/PII.
";
    }
}
