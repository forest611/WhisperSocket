using Microsoft.AspNetCore.Mvc;
using WhisperSocket.Services;
using Microsoft.Extensions.Logging;

namespace WhisperSocket.Controllers;

public class WebSocketController : ControllerBase
{
    private readonly WhisperService _whisperService;
    private readonly ILogger<WebSocketHandler> _logger;
    
    public WebSocketController(WhisperService whisperService, ILogger<WebSocketHandler> logger)
    {
        _whisperService = whisperService;
        _logger = logger;
    }
    
    [Route("/transcribe")]
    public async Task Get()
    {
        if (HttpContext.WebSockets.IsWebSocketRequest)
        {
            using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
            var handler = new WebSocketHandler(webSocket, _whisperService, _logger);
            await handler.ReceiveAudioDataAsync(HttpContext.RequestAborted);
        }
        else
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        }
    }
}
