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
    var port = builder.Configuration.GetValue<int>("Server:Port");
    serverOptions.ListenAnyIP(port); // 設定ファイルからポートを読み込む
});

var app = builder.Build();

app.UseHttpsRedirection();

// WebSocketミドルウェアを追加
app.UseWebSockets(new WebSocketOptions
{
    KeepAliveInterval = TimeSpan.FromMinutes(5),  // タイムアウト時間を5分に延長
});

app.UseAuthorization();

app.MapControllers();

app.Run();