using System;
using System.Collections.Generic;
using DigitalHelper.Models;

namespace DigitalHelper.Services
{
    /// <summary>
    /// Mock LLM service for MVP demo, just to show usage
    /// </summary>
    public static class MockLLMService
    {
        private static int currentStep = 0;
        private static readonly List<HelperGuidanceMessage> loginDemoScenario = new List<HelperGuidanceMessage>();

        static MockLLMService()
        {
            InitializeLoginScenario();
        }

        private static void InitializeLoginScenario()
        {
            loginDemoScenario.Add(new HelperGuidanceMessage
            {
                MessageType = "guidance",
                Icon = "📝",
                Instructions = "Fill in your username and password in the login form I've highlighted.",
                BoundingBoxes = new List<BoundingBox>
                {
                    new BoundingBox
                    {
                        Id = "login-form",
                        X = 580,
                        Y = 280,
                        Width = 440,
                        Height = 140,
                        Label = "Enter your login info here",
                        Color = "#00FF00",
                        Style = "solid",
                        PulseAnimation = true
                    }
                },
                Buttons = new List<HelperButton>
                {
                    new HelperButton
                    {
                        Id = "done",
                        Text = "I filled them in",
                        Action = "take_screenshot",
                        Icon = "✓",
                        Style = "primary"
                    },
                    new HelperButton
                    {
                        Id = "exit",
                        Text = "Exit Help",
                        Action = "exit_help_mode",
                        Style = "secondary"
                    }
                }
            });

            loginDemoScenario.Add(new HelperGuidanceMessage
            {
                MessageType = "guidance",
                Icon = "🚀",
                Instructions = "Great! Now click the 'Sign In' button to log into your account.",
                BoundingBoxes = new List<BoundingBox>
                {
                    new BoundingBox
                    {
                        Id = "signin-button",
                        X = 700,
                        Y = 460,
                        Width = 200,
                        Height = 45,
                        Label = "Click here to sign in",
                        Color = "#0099FF",
                        Style = "solid",
                        PulseAnimation = true
                    }
                },
                Buttons = new List<HelperButton>
                {
                    new HelperButton
                    {
                        Id = "done",
                        Text = "I clicked it",
                        Action = "take_screenshot",
                        Icon = "✓",
                        Style = "primary"
                    },
                    new HelperButton
                    {
                        Id = "exit",
                        Text = "Exit Help",
                        Action = "exit_help_mode",
                        Style = "secondary"
                    }
                }
            });

            loginDemoScenario.Add(new HelperGuidanceMessage
            {
                MessageType = "success",
                Icon = "✅",
                Instructions = "Excellent work! You've successfully logged in. You're all set!",
                BoundingBoxes = null,
                Buttons = new List<HelperButton>
                {
                    new HelperButton
                    {
                        Id = "finish",
                        Text = "Finish",
                        Action = "exit_help_mode",
                        Icon = "👍",
                        Style = "primary"
                    }
                }
            });
        }

        public static HelperGuidanceMessage GetNextStep()
        {
            if (currentStep < loginDemoScenario.Count)
            {
                return loginDemoScenario[currentStep++];
            }

            // Return final message if past the end
            return loginDemoScenario[loginDemoScenario.Count - 1];
        }

        public static void Reset()
        {
            currentStep = 0;
        }

        public static HelperGuidanceMessage GetWelcomeMessage()
        {
            return new HelperGuidanceMessage
            {
                MessageType = "guidance",
                Icon = "👋",
                Instructions = "Hello! I'm your digital helper. You can move me by dragging me with your mouse, toggle the menu by left-clicking me, and show/hide my messages by right clicking me!",
                Buttons = null
            };
        }

        /// <summary>
        /// Gets the mode selection message (for future chat window implementation)
        /// </summary>
        public static HelperGuidanceMessage GetModeSelectionMessage()
        {
            return new HelperGuidanceMessage
            {
                MessageType = "prompt",
                Icon = "💡",
                Instructions = "How would you like me to help you with logging in?",
                Buttons = new List<HelperButton>
                {
                    new HelperButton
                    {
                        Id = "realtime",
                        Text = "Real-Time Help",
                        Action = "start_realtime_help",
                        Icon = "🎯",
                        Style = "primary"
                    },
                    new HelperButton
                    {
                        Id = "guide",
                        Text = "Written Guide",
                        Action = "show_written_guide",
                        Icon = "📝",
                        Style = "secondary"
                    }
                }
            };
        }
    }
}


using System;
using System.Collections.Generic;
using DigitalHelper.Models;

namespace DigitalHelper.Services
{
    /// <summary>
    /// Mock LLM service for MVP demo, just to show usage
    /// </summary>
    public static class MockLLMService
    {
        private static int currentStep = 0;
        private static readonly List<HelperGuidanceMessage> loginDemoScenario = new List<HelperGuidanceMessage>();

        static MockLLMService()
        {
            InitializeLoginScenario();
        }

        private static void InitializeLoginScenario()
        {
            loginDemoScenario.Add(new HelperGuidanceMessage
            {
                MessageType = "guidance",
                Icon = "📝",
                Instructions = "Fill in your username and password in the login form I've highlighted.",
                BoundingBoxes = new List<BoundingBox>
                {
                    new BoundingBox
                    {
                        Id = "login-form",
                        X = 580,
                        Y = 280,
                        Width = 440,
                        Height = 140,
                        Label = "Enter your login info here",
                        Color = "#00FF00",
                        Style = "solid",
                        PulseAnimation = true
                    }
                },
                Buttons = new List<HelperButton>
                {
                    new HelperButton
                    {
                        Id = "done",
                        Text = "I filled them in",
                        Action = "take_screenshot",
                        Icon = "✓",
                        Style = "primary"
                    },
                    new HelperButton
                    {
                        Id = "exit",
                        Text = "Exit Help",
                        Action = "exit_help_mode",
                        Style = "secondary"
                    }
                }
            });

            loginDemoScenario.Add(new HelperGuidanceMessage
            {
                MessageType = "guidance",
                Icon = "🚀",
                Instructions = "Great! Now click the 'Sign In' button to log into your account.",
                BoundingBoxes = new List<BoundingBox>
                {
                    new BoundingBox
                    {
                        Id = "signin-button",
                        X = 700,
                        Y = 460,
                        Width = 200,
                        Height = 45,
                        Label = "Click here to sign in",
                        Color = "#0099FF",
                        Style = "solid",
                        PulseAnimation = true
                    }
                },
                Buttons = new List<HelperButton>
                {
                    new HelperButton
                    {
                        Id = "done",
                        Text = "I clicked it",
                        Action = "take_screenshot",
                        Icon = "✓",
                        Style = "primary"
                    },
                    new HelperButton
                    {
                        Id = "exit",
                        Text = "Exit Help",
                        Action = "exit_help_mode",
                        Style = "secondary"
                    }
                }
            });

            loginDemoScenario.Add(new HelperGuidanceMessage
            {
                MessageType = "success",
                Icon = "✅",
                Instructions = "Excellent work! You've successfully logged in. You're all set!",
                BoundingBoxes = null,
                Buttons = new List<HelperButton>
                {
                    new HelperButton
                    {
                        Id = "finish",
                        Text = "Finish",
                        Action = "exit_help_mode",
                        Icon = "👍",
                        Style = "primary"
                    }
                }
            });
        }

        public static HelperGuidanceMessage GetNextStep()
        {
            if (currentStep < loginDemoScenario.Count)
            {
                return loginDemoScenario[currentStep++];
            }

            // Return final message if past the end
            return loginDemoScenario[loginDemoScenario.Count - 1];
        }

        public static void Reset()
        {
            currentStep = 0;
        }

        public static HelperGuidanceMessage GetWelcomeMessage()
        {
            return new HelperGuidanceMessage
            {
                MessageType = "guidance",
                Icon = "👋",
                Instructions = "Hello! I'm your digital helper. You can move me by dragging me with your mouse, toggle the menu by left-clicking me, and show/hide my messages by right clicking me!",
                Buttons = null
            };
        }

        /// <summary>
        /// Gets the mode selection message (for future chat window implementation)
        /// </summary>
        public static HelperGuidanceMessage GetModeSelectionMessage()
        {
            return new HelperGuidanceMessage
            {
                MessageType = "prompt",
                Icon = "💡",
                Instructions = "How would you like me to help you with logging in?",
                Buttons = new List<HelperButton>
                {
                    new HelperButton
                    {
                        Id = "realtime",
                        Text = "Real-Time Help",
                        Action = "start_realtime_help",
                        Icon = "🎯",
                        Style = "primary"
                    },
                    new HelperButton
                    {
                        Id = "guide",
                        Text = "Written Guide",
                        Action = "show_written_guide",
                        Icon = "📝",
                        Style = "secondary"
                    }
                }
            };
        }
    }
}

