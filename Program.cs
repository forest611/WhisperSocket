using WhisperSocket.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// WhisperServiceの登録
builder.Services.AddSingleton(new WhisperService(
    builder.Configuration["OpenAI:ApiKey"] ?? 
    throw new InvalidOperationException("OpenAI API key not found")));

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

// URLの設定
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ListenAnyIP(3000); // HTTP
});

var app = builder.Build();

app.UseHttpsRedirection();

// WebSocketミドルウェアを追加
app.UseWebSockets(new WebSocketOptions
{
    KeepAliveInterval = TimeSpan.FromMinutes(5),  // タイムアウト時間を5分に延長
    ReceiveBufferSize = AudioSettings.RECEIVE_BUFFER_SIZE  // 音声設定に合わせたバッファサイズ
});

app.UseAuthorization();

app.MapControllers();

app.Run();