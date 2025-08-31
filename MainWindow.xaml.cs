using NextExplorer.ViewModels;
using System;
using System.Windows;

namespace NextExplorer
{
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _viewModel;

        public MainWindow(MainViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;
        }

        private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void AboutMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var aboutMessage = @"NextExplorer v1.0.0

Windowsデスクトップでのフォルダ管理を効率化するアプリケーション

機能:
• 現在開かれているExplorerウィンドウの取得
• フォルダセッションの保存・復元
• お気に入りセッション管理

開発: Claude Code
© 2024";

            MessageBox.Show(aboutMessage, "NextExplorer について", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            
            // ウィンドウの位置とサイズを設定
            // 今後の拡張で設定ファイルから読み込むことを想定
        }

        protected override void OnClosed(EventArgs e)
        {
            // ウィンドウの位置とサイズを保存
            // 今後の拡張で設定ファイルに保存することを想定
            
            base.OnClosed(e);
        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            // F5キーでフォルダ更新
            if (e.Key == System.Windows.Input.Key.F5)
            {
                if (_viewModel.RefreshFoldersCommand.CanExecute(null))
                {
                    _viewModel.RefreshFoldersCommand.Execute(null);
                }
                e.Handled = true;
            }
        }
    }
}