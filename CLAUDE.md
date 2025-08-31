# NextExplorer - Claude Code プロジェクト情報

## プロジェクト概要
**NextExplorer** は、Windowsデスクトップで開かれているフォルダパスを記憶し、名前付きで管理して一括で再オープンできるアプリケーションです。

## 技術仕様
- **フレームワーク**: .NET 8.0 WPF
- **言語**: C#
- **アーキテクチャ**: MVVM パターン
- **依存性注入**: Microsoft.Extensions.DependencyInjection
- **ログ**: Microsoft.Extensions.Logging
- **データ形式**: JSON (Newtonsoft.Json)

## ディレクトリ構造
```
NextExplorer/
├── Models/              # データモデル
│   ├── FolderInfo.cs    # フォルダ情報
│   ├── SessionInfo.cs   # セッション情報
│   └── AppConfig.cs     # アプリ設定
├── Services/            # ビジネスロジック
│   ├── IWindowsExplorerService.cs
│   ├── WindowsExplorerService.cs
│   ├── IFolderSessionManager.cs
│   └── FolderSessionManager.cs
├── Repositories/        # データアクセス
│   ├── ISessionRepository.cs
│   └── SessionRepository.cs
├── ViewModels/          # MVVM ビューモデル
│   └── MainViewModel.cs
├── Views/               # UI
│   └── MainWindow.xaml/xaml.cs
├── Utils/               # ユーティリティ
│   ├── Win32ApiWrapper.cs
│   ├── RelayCommand.cs
│   └── FilePathHelper.cs
├── Themes/              # UIテーマ
│   └── Generic.xaml
├── NextExplorer.sln     # Visual Studio ソリューションファイル
└── app.ico              # アプリケーションアイコン
```

## ファイル配置設定（実行ファイル直下）

### ビルド設定
- **出力先**: `bin\$(Configuration)\` (実行ファイル直下)
- **単一ファイル**: `PublishSingleFile=false` (通常配布用)
- **自己完結型**: `SelfContained=false` (.NET ランタイム必須)
- **アプリケーションアイコン**: `app.ico`

### データファイル配置
実行ファイルと同じディレクトリに以下の構造でファイルを配置：

```
NextExplorer.exe          # メイン実行ファイル
├── Data/                 # データファイル用ディレクトリ
│   ├── sessions.json     # セッションデータ
│   └── sessions_backup.json  # バックアップファイル
├── Logs/                 # ログファイル用ディレクトリ
│   ├── app_yyyyMMdd.log  # アプリケーションログ
│   └── error_yyyyMMdd.log # エラーログ
└── Temp/                 # 一時ファイル用ディレクトリ
    └── cache/            # キャッシュファイル
```

### ファイルパス管理
`Utils/FilePathHelper.cs` により以下のパスを提供：

- **データディレクトリ**: `実行ファイル直下/Data/`
- **ログディレクトリ**: `実行ファイル直下/Logs/`
- **一時ディレクトリ**: `実行ファイル直下/Temp/`

## 主要機能

### 1. フォルダ検出機能
- Windows API (user32.dll, shell32.dll) を使用
- 現在開いている Explorer ウィンドウを自動検出
- フォルダパスの抽出と状態確認
- 長いパス名への対応（32KB バッファ）
- 部分切り詰めパスの推定機能

### 2. セッション管理機能
- フォルダ群の名前付き保存
- お気に入りセッション管理
- 使用回数・最終使用日時の記録
- セッションの複製・編集・削除

### 3. 一括復元機能
- 保存したセッションの一括フォルダオープン
- アクセス不可フォルダの事前チェック
- エラー時の個別処理対応

### 4. データ永続化
- JSON形式でのセッションデータ保存
- 自動バックアップ機能
- データ破損時の復旧機能

## UI設計

### メイン画面構成
- **上部パネル**: 現在開いているフォルダ一覧（2:3の比率で表示）
- **下部パネル**: 保存されたセッション一覧
- **ツールバー**: よく使う操作のクイックアクセス
- **ステータスバー**: 動作状況表示
- **BooleanToVisibilityConverter**: UI要素の表示/非表示制御

### キーボードショートカット
- `Ctrl+R`: フォルダ更新
- `Ctrl+S`: セッション保存
- `Ctrl+O`: セッション復元
- `Delete`: セッション削除
- `F5`: 画面更新

## ビルドとデプロイ

### 開発環境要件
- .NET 8.0 SDK
- Visual Studio 2022 または VS Code
- Windows 10/11

### ビルドコマンド
```bash
# 通常ビルド
dotnet build

# リリースビルド
dotnet build --configuration Release

# 発行（単一ファイル）
dotnet publish --configuration Release --runtime win-x64 --self-contained false --single-file
```

### 配布用パッケージ
実行ファイル直下に以下のファイル/フォルダが自動作成されます：
- `Data/` - セッションデータ
- `Logs/` - ログファイル  
- `Temp/` - 一時ファイル

## セキュリティ考慮事項
- パスインジェクション攻撃の防止
- 不正パスアクセスのブロック
- データファイル暗号化対応（オプション）
- アプリケーション署名による信頼性確保

## 今後の拡張予定
- 複数ディスプレイ対応
- フォルダ階層表示機能
- セッションのエクスポート/インポート
- ホットキー対応
- システムトレイ常駐機能

## 開発履歴
- **v1.0.0**: 初回実装完了
  - 基本的なフォルダ管理機能
  - セッション保存・復元機能
  - MVVM アーキテクチャ採用
  - 実行ファイル直下へのファイル配置対応

- **v1.1.0**: UI改善とパス検出機能強化
  - アプリケーションアイコンの追加
  - メインウィンドウのレイアウト調整（上下パネル比率 2:3）
  - Win32ApiWrapper.cs の大幅改良
    - 長いパス名対応（32KB バッファ）
    - 切り詰められたパスの推定機能
    - 親ディレクトリからの子フォルダ検索機能
  - BooleanToVisibilityConverter の追加
  - Visual Studio ソリューションファイルの作成
  - PublishSingleFile設定の変更（false）

## コミットとプッシュコマンド

### 設定変更の確認
```bash
git status
git diff
```

### コミット
```bash
git add .
git commit -m "Improve UI layout and enhance folder path detection

- Add application icon (app.ico) and configure in project file
- Adjust main window layout ratio (upper:lower = 2:3)
- Enhance Win32ApiWrapper.cs with advanced path detection:
  - Support for long path names (32KB buffer)
  - Truncated path estimation functionality
  - Parent-to-child directory search algorithm
- Add BooleanToVisibilityConverter to Generic.xaml
- Create Visual Studio solution file
- Change PublishSingleFile setting to false for standard distribution

🤖 Generated with [Claude Code](https://claude.ai/code)

Co-Authored-By: Claude <noreply@anthropic.com>"
```

### プッシュ
```bash
git push origin master
```