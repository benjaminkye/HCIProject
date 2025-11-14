using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
using System.Threading.Tasks;
using DigitalHelper.Models;
using Google.GenAI;
using Google.GenAI.Types;

namespace DigitalHelper.Services
{
    /// <summary>
    /// Service for interacting with Gemini LLM
    /// </summary>
    public class LLMService
    {
        private static LLMService? _instance;
        private static readonly object _lock = new();

        private string? _apiKey;
        private Client? _model;

        public static LLMService Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new LLMService();
                        }
                    }
                }
                return _instance;
            }
        }

        private LLMService()
        {
        }

        public void SetApiKey(string apiKey)
        {
            _apiKey = apiKey;
            _model = new Client(apiKey: _apiKey);
        }

        public string? GetApiKey()
        {
            return _apiKey;
        }

        /// <summary>
        /// Streams a help guide based on the user's request
        /// </summary>
        public async System.Collections.Generic.IAsyncEnumerable<string> GetHelpGuideStream(string userRequest)
        {
            // System prompt to guide the LLM's behavior
            string systemPrompt = """
                You are a helpful digital assistant designed to help elderly users with technology tasks.

                Your responses should be:
                - Clear and easy to understand
                - Broken down into simple, numbered steps
                - Patient and encouraging in tone
                - Avoiding technical jargon when possible

                FORMATTING INSTRUCTIONS:
                Adhere to the following formatting guidelines. If not using one of the options below, simply use plain text. (No ### like in markdown)
                - Use **bold** for step numbers (e.g., **Step 1:**) and important headings
                - Use *italic* for emphasis on key actions or warnings
                - Use `backticks` for button names, menu items, or technical terms that users need to click or find
                - Do not layer formatting (e.g., "**Step 1: *Open your internet browser**", "*Once you've entered your details, click the `Sign In` or `Log In` button again*.")
                - Keep explanations simple and friendly

                Example formatted response:
                [Start of response]
                **Step 1**
                Open the `Start Menu` by clicking the *Windows* icon in the bottom-left corner of your screen.

                **Step 2**
                Type `Control Panel` and press *Enter* to open it.
                [End of response]

                When a user asks for help with a task, provide a complete step-by-step guide that they can follow.
                
                """;

            string fullPrompt = $"{systemPrompt}\n\nUser Task: {userRequest}";

            await foreach (var chunk in _model!.Models.GenerateContentStreamAsync(
                model: "gemini-2.5-flash-lite",
                contents: fullPrompt))
            {
                if (chunk?.Candidates?[0]?.Content?.Parts?[0]?.Text != null)
                {
                    yield return chunk!.Candidates![0]!.Content!.Parts![0]!.Text!;
                }
            }
        }

        /// <summary>
        /// Analyzes a screenshot to provide the next step for realtime help
        /// </summary>
        /// <returns>HelperGuidanceMessage with instructions and bounding box</returns>
        public async Task<HelperGuidanceMessage> AnalyzeScreenshotAsync(
            byte[] screenshotPng,
            string userTask,
            int nativeWidth,
            int nativeHeight)
        {
            string prompt = $"""
                You are a patient, helpful digital assistant for elderly users who need help with technology.
                The user needs help with the following task: "{userTask}"
                Given this task, do the following:
                1. Analyze the attached screenshot. This screenshot may be at any stage of the task, so use context clues to figure out what stage the user is at.
                2. Determine the next action the user should take.
                3. Locate UI element(s) involved. Group related elements if appropriate (username + password together) and generate coordinates for a bounding box that surrounds the target area.
                4. Return JSON following this format (no other text):
                {"{"}
                    "instruction": "Instruction text here", // string
                    "box-2d": [ymin, xmin, ymax, xmax], // 0-1000 coordinates
                {"}"}
                Your response MUST be ONLY raw JSON.
                """;


            var response = await _model!.Models.GenerateContentAsync(
                model: "gemini-2.5-pro", // Flash faster and cheaper but worse, pro better but slower (sometimes by a lot) and more $
                contents: new List<Content>
                {
                    new Content
                    {
                        Parts = new List<Part>
                        {
                            new Part
                            {
                                InlineData = new Blob
                                {
                                    MimeType = "image/png",
                                    Data = screenshotPng
                                }
                            },
                            new Part
                            {
                                Text = prompt
                            }
                        }
                    }
                },
                config: new GenerateContentConfig
                {
                    ResponseMimeType = "application/json"
                    //ThinkingConfig = new ThinkingConfig
                    //{
                    //    ThinkingBudget = -1
                    //}
                }
            );

            string jsonResponse = response?.Candidates?[0]?.Content?.Parts?[0]?.Text?.Trim() ?? "";
            Trace.WriteLine(jsonResponse);

            if (jsonResponse.StartsWith("```json"))
            {
                jsonResponse = jsonResponse.Substring(7);
            }
            if (jsonResponse.StartsWith("```"))
            {
                jsonResponse = jsonResponse.Substring(3);
            }
            if (jsonResponse.EndsWith("```"))
            {
                jsonResponse = jsonResponse.Substring(0, jsonResponse.Length - 3);
            }
            jsonResponse = jsonResponse.Trim();
            
            Trace.WriteLine("Post-processed json:");
            Trace.WriteLine(jsonResponse);

            var jsonDoc = JsonDocument.Parse(jsonResponse);
            var root = jsonDoc.RootElement;
            
            string instruction = root.GetProperty("instruction").GetString() ?? "Continue with the next step.";
            var boxArray = root.GetProperty("box-2d");

            double Clamp(double v) => Math.Min(1000.0, Math.Max(0.0, v));
            double ymin = Clamp(boxArray[0].GetDouble());
            double xmin = Clamp(boxArray[1].GetDouble());
            double ymax = Clamp(boxArray[2].GetDouble());
            double xmax = Clamp(boxArray[3].GetDouble());

            double scaleX = nativeWidth / 1000.0;
            double scaleY = nativeHeight / 1000.0;

            var boundingBox = new BoundingBox
            {
                Id = "target",
                X = xmin * scaleX,
                Y = ymin * scaleY,
                Width = Math.Abs(xmax - xmin) * scaleX,
                Height = Math.Abs(ymax - ymin) * scaleY,
                Color = "#00FF00",
                Style = "solid",
                PulseAnimation = true
            };

            var message = new HelperGuidanceMessage
            {
                MessageType = "guidance",
                Icon = "",
                Instructions = instruction,
                BoundingBoxes = new List<BoundingBox> { boundingBox },
                Buttons = new List<HelperButton>
                {
                    new HelperButton
                    {
                        Id = "nextStep",
                        Text = "More Help",
                        Action = "take_screenshot",
                        Icon = "",
                        Style = "primary"
                    },
                    new HelperButton
                    {
                        Id = "exit",
                        Text = "Exit",
                        Action = "exit_help_mode",
                        Style = "secondary"
                    }
                }
            };

            return message;
        }
    }
}

