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
builder.Services.AddSwaggerGen();

// URLの設定
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ListenAnyIP(3000); // HTTP
});

var app = builder.Build();

// // Configure the HTTP request pipeline.
// if (app.Environment.IsDevelopment())
// {
//     app.UseSwagger();
//     app.UseSwaggerUI();
// }

app.UseHttpsRedirection();

// WebSocketミドルウェアを追加
app.UseWebSockets(new WebSocketOptions
{
    KeepAliveInterval = TimeSpan.FromMinutes(2)
});

app.UseAuthorization();

app.MapControllers();

app.Run();