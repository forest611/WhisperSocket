using System.Net.WebSockets;
using System.Text;
using Microsoft.Extensions.Logging;

namespace WhisperSocket.Services;

public class WebSocketHandler
{
    private readonly WebSocket _webSocket;
    private readonly WhisperService _whisperService;
    private readonly List<byte> _audioBuffer;
    private bool _isRecording;
    private readonly ILogger<WebSocketHandler> _logger;
    
    public WebSocketHandler(WebSocket webSocket, WhisperService whisperService, ILogger<WebSocketHandler> logger)
    {
        _webSocket = webSocket;
        _whisperService = whisperService;
        _audioBuffer = new List<byte>();
        _isRecording = false;
        _logger = logger;
    }
    
    public async Task ReceiveAudioDataAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("WebSocket接続を開始しました");
        var buffer = new byte[4096];
        var receiveResult = await _webSocket.ReceiveAsync(
            new ArraySegment<byte>(buffer), cancellationToken);

        while (!receiveResult.CloseStatus.HasValue)
        {
            if (receiveResult.MessageType == WebSocketMessageType.Text)
            {
                var message = Encoding.UTF8.GetString(buffer, 0, receiveResult.Count);
                _logger.LogInformation("テキストメッセージを受信: {Message}", message.Trim());
                
                switch (message.Trim())
                {
                    case "start":
                        _isRecording = true;
                        _audioBuffer.Clear();
                        _logger.LogInformation("録音開始");
                        break;
                    case "end":
                        _logger.LogInformation("録音終了. バッファサイズ: {Size} bytes", _audioBuffer.Count);
                        if (_isRecording && _audioBuffer.Count > 0)
                        {
                            _logger.LogInformation("文字起こし処理を開始");
                            var transcription = await _whisperService.TranscribeAudioAsync(
                                _audioBuffer.ToArray(), 
                                cancellationToken);
                            
                            _logger.LogInformation("文字起こし結果: {Text}", transcription);
                            await SendTranscriptionAsync(transcription, cancellationToken);
                        }
                        else
                        {
                            _logger.LogWarning("録音データが空のため、文字起こしをスキップします");
                        }
                        _isRecording = false;
                        break;
                }
            }
            else if (receiveResult.MessageType == WebSocketMessageType.Binary && _isRecording)
            {
                _audioBuffer.AddRange(buffer.Take(receiveResult.Count));
                _logger.LogDebug("バイナリデータを受信: {Size} bytes, 累計: {Total} bytes", 
                    receiveResult.Count, _audioBuffer.Count);
            }
            
            receiveResult = await _webSocket.ReceiveAsync(
                new ArraySegment<byte>(buffer), cancellationToken);
        }

        _logger.LogInformation("WebSocket接続を終了します. 終了ステータス: {Status}", 
            receiveResult.CloseStatus);
            
        await _webSocket.CloseAsync(
            receiveResult.CloseStatus.Value,
            receiveResult.CloseStatusDescription,
            cancellationToken);
    }
    
    public async Task SendTranscriptionAsync(string text, CancellationToken cancellationToken)
    {
        _logger.LogInformation("文字起こし結果を送信: {Text}", text);
        var buffer = Encoding.UTF8.GetBytes(text);
        await _webSocket.SendAsync(
            new ArraySegment<byte>(buffer),
            WebSocketMessageType.Text,
            true,
            cancellationToken);
    }
}
