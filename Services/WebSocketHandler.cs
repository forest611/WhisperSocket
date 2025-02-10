using System.Net.WebSockets;
using System.Text;
using Microsoft.Extensions.Logging;

namespace WhisperSocket.Services;

public class WebSocketHandler
{
    private readonly WebSocket _webSocket;
    private readonly WhisperService _whisperService;
    private readonly ILogger<WebSocketHandler> _logger;
    private readonly List<byte> _audioBuffer;
    private bool _isRecording;

    public WebSocketHandler(WebSocket webSocket, WhisperService whisperService, ILogger<WebSocketHandler> logger)
    {
        _webSocket = webSocket;
        _whisperService = whisperService;
        _logger = logger;
        _audioBuffer = new List<byte>();
    }

    public async Task ReceiveAudioDataAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("WebSocket接続を開始しました");
        
        try
        {
            await ProcessWebSocketMessagesAsync(cancellationToken);
        }
        catch (WebSocketException ex) when (ex.WebSocketErrorCode == WebSocketError.ConnectionClosedPrematurely)
        {
            _logger.LogWarning("クライアントが予期せず切断されました: {Message}", ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError("WebSocket処理中にエラーが発生しました: {Message}", ex.Message);
            await CloseWithErrorAsync(cancellationToken);
        }
    }

    private async Task ProcessWebSocketMessagesAsync(CancellationToken cancellationToken)
    {
        var buffer = new byte[AudioSettings.RECEIVE_BUFFER_SIZE];
        var receiveResult = await _webSocket.ReceiveAsync(
            new ArraySegment<byte>(buffer), cancellationToken);

        while (!receiveResult.CloseStatus.HasValue)
        {
            await ProcessMessageAsync(buffer, receiveResult, cancellationToken);
            receiveResult = await _webSocket.ReceiveAsync(
                new ArraySegment<byte>(buffer), cancellationToken);
        }

        await CloseGracefullyAsync(receiveResult, cancellationToken);
    }

    private async Task ProcessMessageAsync(byte[] buffer, WebSocketReceiveResult receiveResult, CancellationToken cancellationToken)
    {
        switch (receiveResult.MessageType)
        {
            case WebSocketMessageType.Text:
                await HandleTextMessageAsync(buffer, receiveResult, cancellationToken);
                break;
            
            case WebSocketMessageType.Binary when _isRecording:
                HandleBinaryMessage(buffer, receiveResult);
                break;
        }
    }

    private async Task HandleTextMessageAsync(byte[] buffer, WebSocketReceiveResult receiveResult, CancellationToken cancellationToken)
    {
        var message = Encoding.UTF8.GetString(buffer, 0, receiveResult.Count).Trim();
        _logger.LogInformation("テキストメッセージを受信: {Message}", message);

        switch (message)
        {
            case "start":
                StartRecording();
                break;
            
            case "end":
                await EndRecordingAsync(cancellationToken);
                break;
        }
    }

    private void StartRecording()
    {
        _isRecording = true;
        _audioBuffer.Clear();
        _logger.LogInformation("録音開始");
    }

    private async Task EndRecordingAsync(CancellationToken cancellationToken)
    {
        var duration = (double)_audioBuffer.Count / AudioSettings.BYTES_PER_SECOND;
        _logger.LogInformation("録音終了. バッファサイズ: {Size} bytes ({Duration:F2}秒)", 
            _audioBuffer.Count, duration);
        
        if (_isRecording && _audioBuffer.Count > 0)
        {
            await TranscribeAndSendAsync(cancellationToken);
        }
        else
        {
            _logger.LogWarning("録音データが空のため、文字起こしをスキップします");
        }
        
        _isRecording = false;
    }

    private void HandleBinaryMessage(byte[] buffer, WebSocketReceiveResult receiveResult)
    {
        // 受信データが16bit PCM, 16000Hzであることを期待
        if (receiveResult.Count % AudioSettings.BYTES_PER_SAMPLE != 0)
        {
            _logger.LogWarning("受信データが{Bits}bitアライメントではありません: {Size} bytes", 
                AudioSettings.BITS_PER_SAMPLE, receiveResult.Count);
            return;
        }

        _audioBuffer.AddRange(buffer.Take(receiveResult.Count));
        var duration = (double)_audioBuffer.Count / AudioSettings.BYTES_PER_SECOND;
        _logger.LogDebug("バイナリデータを受信: {Size} bytes, 累計: {Total} bytes ({Duration:F2}秒)",
            receiveResult.Count, _audioBuffer.Count, duration);
    }

    private async Task TranscribeAndSendAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("文字起こし処理を開始");
        var transcription = await _whisperService.TranscribeAudioAsync(
            _audioBuffer.ToArray(),
            cancellationToken);

        _logger.LogInformation("文字起こし結果: {Text}", transcription);
        await SendTranscriptionAsync(transcription, cancellationToken);
    }

    private async Task CloseGracefullyAsync(WebSocketReceiveResult receiveResult, CancellationToken cancellationToken)
    {
        await _webSocket.CloseAsync(
            receiveResult.CloseStatus.Value,
            receiveResult.CloseStatusDescription,
            cancellationToken);
    }

    private async Task CloseWithErrorAsync(CancellationToken cancellationToken)
    {
        if (_webSocket.State == WebSocketState.Open)
        {
            await _webSocket.CloseAsync(
                WebSocketCloseStatus.InternalServerError,
                "Internal server error",
                cancellationToken);
        }
    }

    public async Task SendTranscriptionAsync(string text, CancellationToken cancellationToken)
    {
        var buffer = Encoding.UTF8.GetBytes(text);
        await _webSocket.SendAsync(
            new ArraySegment<byte>(buffer),
            WebSocketMessageType.Text,
            true,
            cancellationToken);
    }
}
