namespace WhisperSocket.Services;

public static class AudioSettings
{
    // 音声フォーマット設定
    public const int SAMPLE_RATE = 16000;      // サンプリングレート (Hz)
    public const int BITS_PER_SAMPLE = 16;     // ビット深度
    public const int NUM_CHANNELS = 1;         // チャンネル数（モノラル）
    
    // バッファサイズ設定
    public const int BYTES_PER_SAMPLE = BITS_PER_SAMPLE / 8;
    public const int BYTES_PER_SECOND = SAMPLE_RATE * NUM_CHANNELS * BYTES_PER_SAMPLE;
    public const int RECEIVE_BUFFER_SIZE = BYTES_PER_SECOND * 2;  // 2秒分のバッファ
    
    // WAV関連の定数
    public const int BYTE_RATE = SAMPLE_RATE * NUM_CHANNELS * BITS_PER_SAMPLE / 8;
    public const int BLOCK_ALIGN = NUM_CHANNELS * BITS_PER_SAMPLE / 8;
}
