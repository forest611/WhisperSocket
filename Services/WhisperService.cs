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
        using var memoryStream = new MemoryStream();
        await using var writer = new BinaryWriter(memoryStream);
        
        // WAVヘッダーの書き込み
        writer.Write("RIFF"u8.ToArray());                   // ChunkID
        writer.Write(36 + audioData.Length);                // ChunkSize
        writer.Write("WAVE"u8.ToArray());                   // Format
        writer.Write("fmt "u8.ToArray());                   // Subchunk1ID
        writer.Write(16);                                   // Subchunk1Size (PCM)
        writer.Write((short)1);                             // AudioFormat (PCM)
        writer.Write((short)AudioSettings.NUM_CHANNELS);    // NumChannels
        writer.Write(AudioSettings.SAMPLE_RATE);            // SampleRate
        writer.Write(AudioSettings.BYTE_RATE);              // ByteRate
        writer.Write((short)AudioSettings.BLOCK_ALIGN);     // BlockAlign
        writer.Write((short)AudioSettings.BITS_PER_SAMPLE); // BitsPerSample
        writer.Write("data"u8.ToArray());                   // Subchunk2ID
        writer.Write(audioData.Length);                     // Subchunk2Size
        
        // 音声データの書き込み
        writer.Write(audioData);
        
        // ストリームを先頭に戻す
        memoryStream.Position = 0;

        var response = await _client.GetAudioClient("whisper-1").TranscribeAudioAsync(memoryStream,"audio.wav",null, cancellationToken);
            
        return response.Value.Text;
    }
}
