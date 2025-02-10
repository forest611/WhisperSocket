using WhisperSocket.Services;
using Microsoft.Extensions.Configuration;

var builder = WebApplication.CreateBuilder(args);

// 環境変数からの設定をサポート
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();  // すべての環境変数を読み込む

// WhisperServiceの登録
var apiKey = builder.Configuration["OpenAI:ApiKey"];
if (string.IsNullOrEmpty(apiKey))
{
    Console.WriteLine("Error: OpenAI API key is not set. Please set the OPENAI_APIKEY environment variable.");
    Environment.Exit(1);
}
builder.Services.AddSingleton(new WhisperService(apiKey));

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