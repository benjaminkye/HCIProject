using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DigitalHelper.Models;

namespace DigitalHelper.Services
{
    public class BrowserBridgeService
    {
        private static readonly Lazy<BrowserBridgeService> _instance = new Lazy<BrowserBridgeService>(() => new BrowserBridgeService());
        public static BrowserBridgeService Instance => _instance.Value;

        private HttpListener? _httpListener;
        private WebSocket? _webSocket;
        private bool _isRunning = false;
        private Task? _listenerTask;
        private DomSummary? _lastDomSummary;
        private TaskCompletionSource<DomSummary>? _domRequestCompletion;
        private readonly SemaphoreSlim _domRequestLock = new SemaphoreSlim(1, 1);
        private Timer? _keepAliveTimer;
        private const int KEEPALIVE_INTERVAL_MS = 15000;

        public bool IsConnected => _webSocket != null && _webSocket.State == WebSocketState.Open;

        private BrowserBridgeService()
        {
        }

        public async Task StartAsync()
        {
            if (_isRunning) return;

            _isRunning = true;
            _listenerTask = Task.Run(RunServerAsync);
            await Task.Delay(100);
        }
        public async Task StopAsync()
        {
            _isRunning = false;

            _keepAliveTimer?.Dispose();
            _keepAliveTimer = null;

            if (_webSocket != null && _webSocket.State == WebSocketState.Open)
            {
                await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Server shutting down", CancellationToken.None);
            }

            _httpListener?.Stop();
            _httpListener?.Close();
        }
        public async Task<DomSummary?> RequestDomSummaryAsync(int timeoutMs = 5000)
        {
            if (!IsConnected)
            {
                Trace.WriteLine("Cannot request DOM: Browser extension not connected");
                return null;
            }

            if (!await _domRequestLock.WaitAsync(0))
            {
                Trace.WriteLine("DOM request already in progress, waiting...");
                await _domRequestLock.WaitAsync();
            }

            try
            {
                Trace.WriteLine("Sending REQUEST_DOM to browser extension...");
                _domRequestCompletion = new TaskCompletionSource<DomSummary>();
                
                var domTask = _domRequestCompletion.Task;

                await SendMessageAsync(new
                {
                    type = "REQUEST_DOM"
                });

                var completedTask = await Task.WhenAny(
                    domTask,
                    Task.Delay(timeoutMs)
                );

                if (completedTask == domTask)
                {
                    var result = await domTask;
                    Trace.WriteLine($"DOM summary received: {result?.Elements?.Count ?? 0} elements");
                    return result;
                }

                Trace.WriteLine($"DOM request timed out after {timeoutMs}ms");
                
                var oldCompletion = _domRequestCompletion;
                _domRequestCompletion = null;
                oldCompletion?.TrySetCanceled();
                
                return null;
            }
            finally
            {
                _domRequestLock.Release();
            }
        }

        public async Task HighlightElementAsync(string selector, string? color = null, double? thickness = null)
        {
            if (!IsConnected)
            {
                Trace.WriteLine("[BrowserBridge] Cannot highlight element - not connected");
                return;
            }

            Trace.WriteLine($"[BrowserBridge] Sending HIGHLIGHT_ELEMENT - selector: {selector}, color: {color ?? "#00FF00"}, thickness: {thickness ?? 4.0}");

            await SendMessageAsync(new
            {
                type = "HIGHLIGHT_ELEMENT",
                selector = selector,
                color = color ?? "#00FF00",
                thickness = thickness ?? 4.0
            });
        }

        public async Task ClearHighlightAsync()
        {
            if (!IsConnected) return;
            Trace.WriteLine("Sending CLEAR_HIGHLIGHT to browser extension");
            await SendMessageAsync(new
            {
                type = "CLEAR_HIGHLIGHT"
            });
        }

        public async Task SetZoomAsync(string fontSize, bool enabled)
        {
            if (!IsConnected) return;

            await SendMessageAsync(new
            {
                type = "SET_ZOOM",
                fontSize = fontSize,
                enabled = enabled
            });
        }
        public async Task SetZoomEnabledAsync(bool enabled)
        {
            if (!IsConnected) return;

            await SendMessageAsync(new
            {
                type = "SET_ZOOM_ENABLED",
                enabled = enabled
            });
        }

        private async Task RunServerAsync()
        {
            while (_isRunning)
            {
                try
                {
                    _httpListener = new HttpListener();
                    _httpListener.Prefixes.Add("http://localhost:9876/");
                    _httpListener.Start();

                    while (_isRunning)
                    {
                        var context = await _httpListener.GetContextAsync();

                        if (context.Request.IsWebSocketRequest)
                        {
                            if (_webSocket != null && _webSocket.State == WebSocketState.Open)
                            {
                                context.Response.StatusCode = 409;
                                context.Response.Close();
                                Trace.WriteLine("Rejected new WebSocket connection - already connected");
                                continue;
                            }
                            
                            var wsContext = await context.AcceptWebSocketAsync(null);
                            _webSocket = wsContext.WebSocket;
                            Trace.WriteLine("Browser extension WebSocket connected");

                            _ = HandleWebSocketAsync(_webSocket);
                        }
                        else
                        {
                            context.Response.StatusCode = 400;
                            context.Response.Close();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Trace.WriteLine($"WebSocket server error: {ex.Message}");
                    await Task.Delay(3000);
                }
            }
        }

        private async Task HandleWebSocketAsync(WebSocket webSocket)
        {
            var buffer = new byte[1024 * 16];
            var messageBuilder = new StringBuilder();

            try
            {
                _keepAliveTimer?.Dispose();
                _keepAliveTimer = new Timer(async _ =>
                {
                    if (webSocket.State == WebSocketState.Open)
                    {
                        try
                        {
                            await SendPingAsync(webSocket);
                        }
                        catch (Exception ex)
                        {
                            Trace.WriteLine($"Keepalive ping failed: {ex.Message}");
                        }
                    }
                }, null, TimeSpan.FromMilliseconds(KEEPALIVE_INTERVAL_MS), TimeSpan.FromMilliseconds(KEEPALIVE_INTERVAL_MS));

                while (webSocket.State == WebSocketState.Open && _isRunning)
                {
                    var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        Trace.WriteLine($"WebSocket close received - Status: {result.CloseStatus}, Description: {result.CloseStatusDescription}");
                        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                        break;
                    }
                    else if (result.MessageType == WebSocketMessageType.Text)
                    {
                        var chunk = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        messageBuilder.Append(chunk);

                        if (result.EndOfMessage)
                        {
                            var completeMessage = messageBuilder.ToString();
                            messageBuilder.Clear();
                            await HandleIncomingMessageAsync(completeMessage);
                        }
                    }
                }
            }
            catch (WebSocketException wsEx)
            {
                Trace.WriteLine($"WebSocket error: {wsEx.Message} (Code: {wsEx.WebSocketErrorCode})");
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"WebSocket communication error: {ex.Message}");
            }
            finally
            {
                _keepAliveTimer?.Dispose();
                _keepAliveTimer = null;

                Trace.WriteLine($"WebSocket handler exiting. State: {webSocket.State}");
                
                if (_webSocket == webSocket && webSocket.State != WebSocketState.Open)
                {
                    _webSocket = null;
                    Trace.WriteLine("Browser extension WebSocket disconnected");
                }
                
                if (webSocket.State == WebSocketState.Open)
                {
                    try
                    {
                        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Handler exiting", CancellationToken.None);
                    }
                    catch { }
                }
            }
        }

        private async Task HandleIncomingMessageAsync(string messageJson)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(messageJson))
                {
                    Trace.WriteLine("Received empty message from browser");
                    return;
                }

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                var message = JsonSerializer.Deserialize<BrowserMessage>(messageJson, options);

                if (message == null)
                {
                    Trace.WriteLine("Failed to deserialize browser message");
                    return;
                }

                if (string.IsNullOrWhiteSpace(message.Type))
                {
                    Trace.WriteLine($"Received message with empty type. Message JSON: {messageJson}");
                    return;
                }

                switch (message.Type)
                {
                    case "DOM_SUMMARY":
                        Trace.WriteLine("Received DOM_SUMMARY from browser extension");
                        if (message.Data != null)
                        {
                            var domJson = JsonSerializer.Serialize(message.Data);
                            var domOptions = new JsonSerializerOptions
                            {
                                PropertyNameCaseInsensitive = true
                            };
                            _lastDomSummary = JsonSerializer.Deserialize<DomSummary>(domJson, domOptions);

                            if (_lastDomSummary != null)
                            {
                                Trace.WriteLine($"Parsed DOM summary: URL={_lastDomSummary.Url}, Elements={_lastDomSummary.Elements.Count}");
                            }

                            if (_domRequestCompletion != null && _lastDomSummary != null)
                            {
                                _domRequestCompletion.TrySetResult(_lastDomSummary);
                                _domRequestCompletion = null;
                            }
                        }
                        else
                        {
                            Trace.WriteLine("DOM_SUMMARY message had null Data");
                        }
                        break;

                    case "CONNECTION_STATUS":
                        Trace.WriteLine($"Browser extension connected: {message.Connected}");
                        break;

                    case "ERROR":
                        Trace.WriteLine($"Browser extension error: {message.Message}");
                        break;

                    case "HIGHLIGHT_SUCCESS":
                    case "ZOOM_SUCCESS":
                    case "PONG":
                        break;

                    default:
                        Trace.WriteLine($"Unknown message type: '{message.Type}' (JSON: {messageJson})");
                        break;
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Error handling message: {ex.Message}. JSON: {messageJson}");
            }

            await Task.CompletedTask;
        }

        private async Task SendMessageAsync(object message)
        {
            if (_webSocket == null || _webSocket.State != WebSocketState.Open)
            {
                return;
            }

            try
            {
                var json = JsonSerializer.Serialize(message);
                var bytes = Encoding.UTF8.GetBytes(json);
                await _webSocket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Error sending message: {ex.Message}");
            }
        }

        private async Task SendPingAsync(WebSocket webSocket)
        {
            if (webSocket.State != WebSocketState.Open)
            {
                return;
            }

            try
            {
                var json = JsonSerializer.Serialize(new { type = "PING" });
                var bytes = Encoding.UTF8.GetBytes(json);
                await webSocket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Error sending ping: {ex.Message}");
            }
        }
    }
}
