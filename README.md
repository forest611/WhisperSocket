# WhisperSocket

WhisperSocketは、WebSocketを使用してリアルタイムな音声文字起こしを実現するサービスです。OpenAI Whisper APIを利用して、高精度な音声認識を提供します。

## 機能

- WebSocketを介したリアルタイム音声ストリーミング
- OpenAI Whisper APIを使用した高精度な音声認識
- WAVフォーマットでの音声データ処理

## 技術スタック

- .NET 8.0
- ASP.NET Core
- OpenAI API (Whisper)
- WebSocket
- Docker

## 必要条件

- .NET 8.0 SDK または Docker
- OpenAI APIキー

## セットアップ

### ローカル実行

1. リポジトリをクローン
```bash
git clone [your-repository-url]
```

2. `appsettings.json`にOpenAI APIキーを設定
```json
{
  "OpenAI": {
    "ApiKey": "your-api-key-here"
  }
}
```

3. プロジェクトを実行
```bash
dotnet run
```

### Docker実行

#### DockerHubからプル
```bash
docker pull forest611/whispersocket:latest
docker run -d -p 3000:3000 \
  -e OpenAI__ApiKey="your-api-key-here" \
  -e Server__Port=3000 \
  --name whispersocket forest611/whispersocket:latest
```

#### ソースからビルド
1. イメージのビルド
```bash
docker build -t whispersocket .
```

2. コンテナの実行
```bash
docker run -d -p 3000:3000 \
  -e OpenAI__ApiKey="your-api-key-here" \
  -e Server__Port=3000 \
  --name whispersocket whispersocket
```

## 環境変数

以下の環境変数を使用して設定を上書きできます：

- `OpenAI__ApiKey`: OpenAI APIキー
- `Server__Port`: サーバーのポート番号（デフォルト: 3000）

## 使用方法

サービスは`http://localhost:3000`でWebSocketエンドポイントを提供します。

音声データは以下の形式で送信する必要があります：
- チャンネル数: 1 (モノラル)
- サンプルレート: 16000 Hz
- ビット深度: 16 bit

## 設定

- WebSocketのKeepAliveインターバル: 5分
- デフォルトポート: 3000

## ライセンス

[ライセンスを記載]
