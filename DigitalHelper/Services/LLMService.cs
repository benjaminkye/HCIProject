using System;
using System.Threading.Tasks;
using Google.GenAI;

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
                    yield return chunk.Candidates[0].Content.Parts[0].Text;
                }
            }
        }
    }
}

