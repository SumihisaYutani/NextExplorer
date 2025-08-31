# NextExplorer - UI設計書

## 1. 画面構成

### 1.1 メインウィンドウ (MainWindow)
```
┌─────────────────────────────────────────────────┐
│ NextExplorer                          □ □ ✕     │
├─────────────────────────────────────────────────┤
│ ファイル(F) 編集(E) 表示(V) ツール(T) ヘルプ(H)     │
├─────────────────────────────────────────────────┤
│ [更新] [保存] [復元] [削除]   [設定] [ヘルプ]      │
├─────────────────────────────────────────────────┤
│                                                 │
│ ┌─ 現在開かれているフォルダ ───────────────────┐   │
│ │ 📁 C:\\Projects\\NextExplorer              │   │
│ │ 📁 D:\\Documents\\Work                     │   │
│ │ 📁 C:\\Users\\User\\Downloads             │   │
│ │ ┌────────────────────────┐                │   │
│ │ │セッション名: 開発作業     │ [保存]         │   │
│ │ └────────────────────────┘                │   │
│ └───────────────────────────────────────────────┘   │
│                                                 │
│ ┌─ 保存されたセッション ─────────────────────────┐ │
│ │ ⭐ 開発環境セッション     使用: 15回 [復元]    │ │
│ │ 📂 プロジェクト資料       使用: 3回  [復元]    │ │
│ │ 📂 テスト環境            使用: 7回  [復元]    │ │
│ │ 📂 ドキュメント整理       使用: 2回  [復元]    │ │
│ └───────────────────────────────────────────────────┘ │
│                                                 │
├─────────────────────────────────────────────────┤
│ 準備完了    現在のフォルダ: 3個 セッション: 4個   │
└─────────────────────────────────────────────────┘
```

### 1.2 レイアウト詳細

#### 1.2.1 ヘッダー部分
- **メニューバー**: 標準的なWindowsアプリケーションメニュー
- **ツールバー**: よく使用する操作のクイックアクセス

#### 1.2.2 メインコンテンツ領域
- **上部パネル**: 現在開かれているフォルダ一覧
- **下部パネル**: 保存されたセッション一覧
- **スプリッター**: 上下パネルのサイズ調整可能

#### 1.2.3 ステータスバー
- アプリケーション状態とフォルダ/セッション数の表示

## 2. 各コントロール仕様

### 2.1 現在のフォルダパネル

#### FolderListView (ListView)
```xml
<ListView Name="CurrentFoldersListView" 
          ItemsSource="{Binding CurrentFolders}"
          SelectionMode="Multiple">
    <ListView.View>
        <GridView>
            <GridViewColumn Header="アイコン" Width="40">
                <GridViewColumn.CellTemplate>
                    <DataTemplate>
                        <Image Source="{Binding IconPath}" Width="16" Height="16"/>
                    </DataTemplate>
                </GridViewColumn.CellTemplate>
            </GridViewColumn>
            <GridViewColumn Header="フォルダパス" Width="400" 
                            DisplayMemberBinding="{Binding Path}"/>
            <GridViewColumn Header="状態" Width="80" 
                            DisplayMemberBinding="{Binding StatusText}"/>
            <GridViewColumn Header="種類" Width="80" 
                            DisplayMemberBinding="{Binding TypeText}"/>
        </GridView>
    </ListView.View>
</ListView>
```

#### セッション保存用コントロール
```xml
<StackPanel Orientation="Horizontal" Margin="5">
    <TextBox Name="SessionNameTextBox" 
             Text="{Binding NewSessionName}" 
             Width="200" 
             Watermark="セッション名を入力..."/>
    <Button Command="{Binding SaveSessionCommand}" 
            Content="保存" 
            Margin="5,0"/>
</StackPanel>
```

### 2.2 セッション管理パネル

#### SessionListView (ListView)
```xml
<ListView Name="SavedSessionsListView" 
          ItemsSource="{Binding SavedSessions}"
          SelectedItem="{Binding SelectedSession}">
    <ListView.View>
        <GridView>
            <GridViewColumn Header="" Width="30">
                <GridViewColumn.CellTemplate>
                    <DataTemplate>
                        <TextBlock Text="{Binding FavoriteIcon}" 
                                   FontFamily="Segoe UI Symbol"/>
                    </DataTemplate>
                </GridViewColumn.CellTemplate>
            </GridViewColumn>
            <GridViewColumn Header="セッション名" Width="200" 
                            DisplayMemberBinding="{Binding Name}"/>
            <GridViewColumn Header="フォルダ数" Width="80" 
                            DisplayMemberBinding="{Binding FolderCount}"/>
            <GridViewColumn Header="使用回数" Width="80" 
                            DisplayMemberBinding="{Binding UsageCount}"/>
            <GridViewColumn Header="最終使用" Width="120" 
                            DisplayMemberBinding="{Binding LastUsedText}"/>
            <GridViewColumn Header="操作" Width="100">
                <GridViewColumn.CellTemplate>
                    <DataTemplate>
                        <StackPanel Orientation="Horizontal">
                            <Button Command="{Binding LoadSessionCommand}" 
                                    Content="復元" 
                                    Width="50" Height="25" 
                                    Margin="2"/>
                        </StackPanel>
                    </DataTemplate>
                </GridViewColumn.CellTemplate>
            </GridViewColumn>
        </GridView>
    </ListView.View>
</ListView>
```

## 3. ダイアログ設計

### 3.1 セッション詳細ダイアログ
```
┌─────────────────────────────────────┐
│ セッション詳細                      │
├─────────────────────────────────────┤
│ セッション名: [開発環境セッション    ] │
│ 説明: [日常の開発作業用フォルダ群     ] │
│                                    │
│ ☑ お気に入り                        │
│                                    │
│ タグ: [開発] [プロジェクト] [+追加]    │
│                                    │
│ ┌─ 含まれるフォルダ ─────────────┐   │
│ │ 📁 C:\\Projects\\NextExplorer │   │
│ │ 📁 D:\\Documents\\Spec        │   │
│ │ 📁 C:\\Tools                  │   │
│ │                              │   │
│ │ [追加] [削除] [上へ] [下へ]     │   │
│ └────────────────────────────────┘   │
│                                    │
│              [OK] [キャンセル]       │
└─────────────────────────────────────┘
```

### 3.2 設定ダイアログ
```
┌─────────────────────────────────────┐
│ 設定                                │
├─┬───────────────────────────────────┤
│ │ ┌─ 全般 ─────────────────────┐   │
│一││ ☑ Windows起動時に自動起動    │   │
│般││ ☑ 最後のセッションを復元     │   │
│ ││ ☑ 終了時に現在の状態を保存   │   │
│表││ ☑ システムトレイに最小化     │   │
│示│└─────────────────────────────┘   │
│ │                                 │
│セ│ ┌─ 表示設定 ─────────────────┐   │
│キ││ テーマ: [Light ▼]            │   │
│ュ││ 言語: [日本語 ▼]             │   │
│リ││ フォント: [Yu Gothic UI ▼] 12pt │
│テ│└─────────────────────────────┘   │
│ィ│                                │
│パ│ ┌─ セキュリティ ─────────────┐   │
│フ││ ☑ データファイルを暗号化     │   │
│ォ││ ☑ 不正なパスアクセスをブロック │   │
│｜│└─────────────────────────────┘   │
│マ│                                │
│ン│ ┌─ パフォーマンス ───────────┐   │
│ス││ フォルダチェック timeout: [5000]ms │
│ ││ キャッシュ有効期限: [30]分    │   │
│ ││ 最大同時フォルダ数: [20]     │   │
│ │└─────────────────────────────┘   │
├─┴───────────────────────────────────┤
│              [OK] [キャンセル] [適用] │
└─────────────────────────────────────┘
```

### 3.3 エラー/警告ダイアログ
```
┌─────────────────────────────────────┐
│ ⚠ 警告                              │
├─────────────────────────────────────┤
│ 以下のフォルダにアクセスできません：    │
│                                    │
│ • D:\\OldProject (存在しません)       │
│ • \\\\server\\share (アクセス拒否)    │
│                                    │
│ これらのフォルダを除いて復元しますか？  │
│                                    │
│ ☑ 今後この警告を表示しない           │
│                                    │
│         [はい] [いいえ] [キャンセル]   │
└─────────────────────────────────────┘
```

## 4. アニメーション・視覚効果

### 4.1 状態変化アニメーション
- **フォルダ読み込み**: プログレスバーとスピナー
- **セッション保存**: フェードイン効果
- **エラー表示**: 赤色ハイライト (500ms)
- **成功通知**: 緑色ハイライト (300ms)

### 4.2 ホバー効果
```xml
<Style TargetType="Button">
    <Style.Triggers>
        <Trigger Property="IsMouseOver" Value="True">
            <Setter Property="Background" Value="#E1F5FE"/>
            <Setter Property="BorderBrush" Value="#0288D1"/>
        </Trigger>
    </Style.Triggers>
</Style>
```

## 5. レスポンシブ設計

### 5.1 ウィンドウサイズ対応
- **最小サイズ**: 600x400
- **推奨サイズ**: 800x600  
- **最大サイズ**: 制限なし

### 5.2 DPI スケーリング対応
```xml
<Application.Resources>
    <system:Double x:Key="ScaleFactor">1.0</system:Double>
</Application.Resources>
```

## 6. アクセシビリティ

### 6.1 キーボードナビゲーション
- **Tab順序**: 論理的な順番で設定
- **ショートカットキー**:
  - `Ctrl+R`: フォルダ更新
  - `Ctrl+S`: セッション保存  
  - `Ctrl+O`: セッション復元
  - `Delete`: セッション削除
  - `F5`: 画面更新

### 6.2 スクリーンリーダー対応
```xml
<Button AutomationProperties.Name="セッションを復元"
        AutomationProperties.HelpText="選択したセッションのフォルダをすべて開きます"/>
```

## 7. カスタマイズ可能項目

### 7.1 外観カスタマイズ
- **色テーマ**: Light/Dark/System
- **アクセントカラー**: ユーザー選択可能
- **フォント設定**: ファミリー、サイズ、太さ

### 7.2 レイアウトカスタマイズ
- **列幅**: ユーザーが調整可能
- **列順序**: ドラッグ&ドロップで変更可能
- **パネル分割**: スプリッターで比率調整

## 8. 国際化対応

### 8.1 多言語リソース
```xml
<ResourceDictionary Source="Resources/Strings.ja-JP.xaml"/>
<ResourceDictionary Source="Resources/Strings.en-US.xaml"/>
```

### 8.2 フォーマット対応
- **日時表示**: 各言語の標準フォーマット
- **数値表示**: 桁区切り文字の地域化
- **文字方向**: RTL言語への将来対応

## 9. タッチ操作対応

### 9.1 タッチジェスチャ
- **タップ**: 選択
- **ダブルタップ**: 復元実行
- **長押し**: コンテキストメニュー
- **スワイプ**: スクロール

### 9.2 タッチフレンドリーUI
- **最小タッチターゲット**: 44x44ピクセル
- **間隔調整**: タッチ操作に適した間隔
- **スクロール**: 慣性スクロール対応