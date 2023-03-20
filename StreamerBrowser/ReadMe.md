# Streamer Browser

## 概要

Streamer Browserは、Windows用の新しいアプリケーションで、URL、ブックマーク、ブラウザ表示ウインドウが独立しており、YouTubeライブなどの配信で余計な表示をせずにウェブページを表示できる機能があります。作者はVtuberの椎半音です。

## 特徴

1. **独立したURL、ブックマーク、ブラウザ表示ウインドウ**: 配信中に余計な表示がなく、クリーンなウェブページを表示できます。
2. **NGワードぼかし機能**: NGワードを登録し、そのキーワードがあれば自動的にぼかしをかけることができます。
3. **ぼかし切り替え機能**: 画面上のぼかしを切り替える機能があります。
4. **Webインターフェース**: ブックマークへのジャンプ、ぼかしの切り替えなどの機能をWebインターフェースで実行できます。Stream Deckなどの外部機器からも制御が可能です。

## 配信用URL

[https://github.com/ShiiSharp/StreamerBrowser](https://github.com/ShiiSharp/StreamerBrowser)

## 使用方法

1. 配信用URLからアプリケーションをダウンロードしてインストールします。
2. ダブルクリックでアプリケーションを起動します。
3. OBSでウインドウキャプチャを追加し、"Streamer Browser Web Window"を選択します。キャプチャ方式は、"Windows 10 (1903以降)"を選びます。
4. (音声もキャプチャする場合) OBSでアプリケーション音声キャプチャを追加し、"Streamer Browser Web Window"を選択します。

### Stream Deckへの登録方法
1. Stream Deckの設定アプリを起動します
2. [Webサイト]を追加します。
3. [URL]欄に以下のアドレスを入力します。
 * **ブックマークへのジャンプ**: "http://localhost:9801/[ブックマークの番号]" ただし、一番上のブックマークは0番です。
 * **ぼかしの切り替え**: "http://localhost:9801/fx"
 * **強制的にぼかしを適用**: "http://localhost:9801/fx/0"
 * **強制的にぼかしを解除**: "http://localhost:9801/fx/1"
4. アイコンを適当に変更し、説明文を入力します。
## サポートと連絡先

問題がある場合や機能リクエストがある場合は、以下の連絡先でサポートを受けることができます。

* Twitter: [@csharpvtuber](https://twitter.com/csharpvtuber)

Streamer Browserをお楽しみください！
