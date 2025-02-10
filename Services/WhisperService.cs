using System.Text;
using OpenAI.Audio;
using OpenAI;

namespace WhisperSocket.Services;

public class WhisperService
{
    private readonly OpenAIClient _client;
    
    public WhisperService(string apiKey)
    {
        _client = new OpenAIClient(apiKey);
    }
    
    public async Task<string> TranscribeAudioAsync(byte[] audioData, CancellationToken cancellationToken)
    {
        // int16_tバッファをWAVファイルに変換
        using var memoryStream = new MemoryStream();
        await using var writer = new BinaryWriter(memoryStream);
        
        // WAVヘッダーの書き込み
        writer.Write("RIFF"u8.ToArray()); // ChunkID
        writer.Write(36 + audioData.Length); // ChunkSize
        writer.Write("WAVE"u8.ToArray()); // Format
        writer.Write("fmt "u8.ToArray()); // Subchunk1ID
        writer.Write(16); // Subchunk1Size
        writer.Write((short)1); // AudioFormat (PCM)
        writer.Write((short)1); // NumChannels (Mono)
        writer.Write(16000); // SampleRate
        writer.Write(32000); // ByteRate (SampleRate * NumChannels * BitsPerSample/8)
        writer.Write((short)2); // BlockAlign (NumChannels * BitsPerSample/8)
        writer.Write((short)16); // BitsPerSample
        writer.Write("data"u8.ToArray()); // Subchunk2ID
        writer.Write(audioData.Length); // Subchunk2Size
        
        // 音声データの書き込み
        writer.Write(audioData);
        
        // ストリームを先頭に戻す
        memoryStream.Position = 0;

        var response = await _client.GetAudioClient("whisper-1").TranscribeAudioAsync(memoryStream,"",null, cancellationToken);
            
        return response.GetRawResponse().ReasonPhrase;
    }
}
