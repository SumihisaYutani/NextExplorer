using Microsoft.Extensions.Logging;
using NextExplorer.Models;
using NextExplorer.Services;
using NextExplorer.Utils;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace NextExplorer.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly IFolderSessionManager _sessionManager;
        private readonly ILogger<MainViewModel> _logger;
        
        private ObservableCollection<FolderInfo> _currentFolders;
        private ObservableCollection<SessionInfo> _savedSessions;
        private SessionInfo? _selectedSession;
        private string _newSessionName;
        private string _statusMessage;
        private bool _isLoading;

        public ObservableCollection<FolderInfo> CurrentFolders
        {
            get => _currentFolders;
            set => SetProperty(ref _currentFolders, value);
        }

        public ObservableCollection<SessionInfo> SavedSessions
        {
            get => _savedSessions;
            set => SetProperty(ref _savedSessions, value);
        }

        public SessionInfo? SelectedSession
        {
            get => _selectedSession;
            set
            {
                SetProperty(ref _selectedSession, value);
                LoadSessionCommand.RaiseCanExecuteChanged();
                DeleteSessionCommand.RaiseCanExecuteChanged();
            }
        }

        public string NewSessionName
        {
            get => _newSessionName;
            set
            {
                SetProperty(ref _newSessionName, value);
                SaveSessionCommand.RaiseCanExecuteChanged();
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                SetProperty(ref _isLoading, value);
                RefreshFoldersCommand.RaiseCanExecuteChanged();
                SaveSessionCommand.RaiseCanExecuteChanged();
                LoadSessionCommand.RaiseCanExecuteChanged();
            }
        }

        public RelayCommand RefreshFoldersCommand { get; }
        public RelayCommand SaveSessionCommand { get; }
        public RelayCommand LoadSessionCommand { get; }
        public RelayCommand DeleteSessionCommand { get; }
        public RelayCommand<SessionInfo> LoadSpecificSessionCommand { get; }
        public RelayCommand<SessionInfo> DeleteSpecificSessionCommand { get; }
        public RelayCommand<SessionInfo> ToggleFavoriteCommand { get; }

        public MainViewModel(IFolderSessionManager sessionManager, ILogger<MainViewModel> logger)
        {
            _sessionManager = sessionManager;
            _logger = logger;
            
            _currentFolders = new ObservableCollection<FolderInfo>();
            _savedSessions = new ObservableCollection<SessionInfo>();
            _newSessionName = string.Empty;
            _statusMessage = "準備完了";

            RefreshFoldersCommand = new RelayCommand(async () => await RefreshFoldersAsync(), () => !IsLoading);
            SaveSessionCommand = new RelayCommand(async () => await SaveSessionAsync(), CanSaveSession);
            LoadSessionCommand = new RelayCommand(async () => await LoadSessionAsync(), CanLoadSession);
            DeleteSessionCommand = new RelayCommand(async () => await DeleteSessionAsync(), CanDeleteSession);
            LoadSpecificSessionCommand = new RelayCommand<SessionInfo>(async session => await LoadSpecificSessionAsync(session));
            DeleteSpecificSessionCommand = new RelayCommand<SessionInfo>(async session => await DeleteSpecificSessionAsync(session));
            ToggleFavoriteCommand = new RelayCommand<SessionInfo>(async session => await ToggleFavoriteAsync(session));

            _ = InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            await RefreshFoldersAsync();
            await LoadSavedSessionsAsync();
        }

        private async Task RefreshFoldersAsync()
        {
            if (IsLoading) return;

            try
            {
                IsLoading = true;
                StatusMessage = "フォルダを取得中...";

                var folders = await _sessionManager.GetCurrentOpenFoldersAsync();
                
                CurrentFolders.Clear();
                foreach (var folder in folders)
                {
                    CurrentFolders.Add(folder);
                }

                StatusMessage = $"現在のフォルダ: {CurrentFolders.Count}個";
                _logger.LogInformation($"Refreshed folders: {CurrentFolders.Count} found");
            }
            catch (Exception ex)
            {
                StatusMessage = "フォルダの取得に失敗しました";
                _logger.LogError(ex, "Failed to refresh folders");
                MessageBox.Show("フォルダの取得に失敗しました。", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task SaveSessionAsync()
        {
            if (IsLoading || !CanSaveSession()) return;

            try
            {
                IsLoading = true;
                StatusMessage = "セッションを保存中...";

                if (!CurrentFolders.Any())
                {
                    MessageBox.Show("保存するフォルダがありません。", "情報", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var folders = CurrentFolders.ToList();
                var success = await _sessionManager.SaveSessionAsync(NewSessionName, folders);

                if (success)
                {
                    NewSessionName = string.Empty;
                    await LoadSavedSessionsAsync();
                    StatusMessage = "セッションを保存しました";
                    _logger.LogInformation($"Session saved successfully: {NewSessionName}");
                }
                else
                {
                    StatusMessage = "セッションの保存に失敗しました";
                    MessageBox.Show("セッションの保存に失敗しました。", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                StatusMessage = "セッションの保存に失敗しました";
                _logger.LogError(ex, "Failed to save session");
                MessageBox.Show("セッションの保存に失敗しました。", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadSessionAsync()
        {
            if (SelectedSession != null)
            {
                await LoadSpecificSessionAsync(SelectedSession);
            }
        }

        private async Task LoadSpecificSessionAsync(SessionInfo? session)
        {
            if (session == null || IsLoading) return;

            try
            {
                IsLoading = true;
                StatusMessage = $"セッション '{session.Name}' を復元中...";

                // フォルダの状態を確認
                session.UpdateFolderStatuses();
                var inaccessibleFolders = session.GetInaccessibleFolders();
                
                if (inaccessibleFolders.Any())
                {
                    var message = $"以下のフォルダにアクセスできません:\n\n{string.Join("\n", inaccessibleFolders.Select(f => $"• {f.Path}"))}\n\nアクセス可能なフォルダのみ復元しますか？";
                    var result = MessageBox.Show(message, "警告", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    
                    if (result == MessageBoxResult.No)
                    {
                        StatusMessage = "復元をキャンセルしました";
                        return;
                    }
                }

                var success = await _sessionManager.RestoreSessionAsync(session);

                if (success)
                {
                    await LoadSavedSessionsAsync(); // Update usage count
                    StatusMessage = $"セッション '{session.Name}' を復元しました";
                    _logger.LogInformation($"Session restored successfully: {session.Name}");
                }
                else
                {
                    StatusMessage = "セッションの復元に失敗しました";
                    MessageBox.Show("セッションの復元に失敗しました。", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                StatusMessage = "セッションの復元に失敗しました";
                _logger.LogError(ex, $"Failed to load session: {session?.Name}");
                MessageBox.Show("セッションの復元に失敗しました。", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task DeleteSessionAsync()
        {
            if (SelectedSession != null)
            {
                await DeleteSpecificSessionAsync(SelectedSession);
            }
        }

        private async Task DeleteSpecificSessionAsync(SessionInfo? session)
        {
            if (session == null || IsLoading) return;

            try
            {
                var result = MessageBox.Show(
                    $"セッション '{session.Name}' を削除しますか？\nこの操作は取り消せません。",
                    "確認",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.No)
                    return;

                IsLoading = true;
                StatusMessage = $"セッション '{session.Name}' を削除中...";

                var success = await _sessionManager.DeleteSessionAsync(session.Id);

                if (success)
                {
                    await LoadSavedSessionsAsync();
                    SelectedSession = null;
                    StatusMessage = $"セッション '{session.Name}' を削除しました";
                    _logger.LogInformation($"Session deleted successfully: {session.Name}");
                }
                else
                {
                    StatusMessage = "セッションの削除に失敗しました";
                    MessageBox.Show("セッションの削除に失敗しました。", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                StatusMessage = "セッションの削除に失敗しました";
                _logger.LogError(ex, $"Failed to delete session: {session?.Name}");
                MessageBox.Show("セッションの削除に失敗しました。", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task ToggleFavoriteAsync(SessionInfo? session)
        {
            if (session == null) return;

            try
            {
                session.IsFavorite = !session.IsFavorite;
                await _sessionManager.UpdateSessionAsync(session);
                
                _logger.LogInformation($"Toggled favorite for session: {session.Name} -> {session.IsFavorite}");
            }
            catch (Exception ex)
            {
                session.IsFavorite = !session.IsFavorite; // Revert change
                _logger.LogError(ex, $"Failed to toggle favorite: {session.Name}");
                MessageBox.Show("お気に入りの変更に失敗しました。", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadSavedSessionsAsync()
        {
            try
            {
                var sessions = await _sessionManager.LoadAllSessionsAsync();
                
                SavedSessions.Clear();
                foreach (var session in sessions.OrderByDescending(s => s.LastUsed).ThenBy(s => s.Name))
                {
                    SavedSessions.Add(session);
                }

                var statusText = $"セッション: {SavedSessions.Count}個";
                if (CurrentFolders?.Count > 0)
                {
                    statusText = $"現在のフォルダ: {CurrentFolders.Count}個 " + statusText;
                }
                StatusMessage = statusText;

                _logger.LogInformation($"Loaded {SavedSessions.Count} saved sessions");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load saved sessions");
                MessageBox.Show("セッションの読み込みに失敗しました。", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanSaveSession()
        {
            return !IsLoading && !string.IsNullOrWhiteSpace(NewSessionName) && CurrentFolders?.Any() == true;
        }

        private bool CanLoadSession()
        {
            return !IsLoading && SelectedSession != null;
        }

        private bool CanDeleteSession()
        {
            return !IsLoading && SelectedSession != null;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}