using Microsoft.Extensions.Logging;
using NextExplorer.Models;
using NextExplorer.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NextExplorer.Services
{
    public class FolderSessionManager : IFolderSessionManager
    {
        private readonly IWindowsExplorerService _explorerService;
        private readonly ISessionRepository _sessionRepository;
        private readonly ILogger<FolderSessionManager> _logger;

        public FolderSessionManager(
            IWindowsExplorerService explorerService,
            ISessionRepository sessionRepository,
            ILogger<FolderSessionManager> logger)
        {
            _explorerService = explorerService;
            _sessionRepository = sessionRepository;
            _logger = logger;
        }

        public async Task<List<FolderInfo>> GetCurrentOpenFoldersAsync()
        {
            try
            {
                var folders = await _explorerService.GetOpenExplorerWindowsAsync();
                
                // 重複除去
                var uniqueFolders = folders
                    .GroupBy(f => f.Path.ToLowerInvariant())
                    .Select(g => g.First())
                    .ToList();

                _logger.LogInformation($"Retrieved {uniqueFolders.Count} unique open folders");
                return uniqueFolders;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current open folders");
                return new List<FolderInfo>();
            }
        }

        public async Task<bool> SaveSessionAsync(string name, List<FolderInfo> folders, string? description = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    _logger.LogWarning("Cannot save session: name is null or empty");
                    return false;
                }

                if (folders == null || !folders.Any())
                {
                    _logger.LogWarning("Cannot save session: no folders provided");
                    return false;
                }

                // フォルダの状態を更新
                var validatedFolders = await _explorerService.ValidateFoldersAsync(folders);

                var session = new SessionInfo(name, validatedFolders)
                {
                    Description = description
                };

                var success = await _sessionRepository.SaveSessionAsync(session);
                
                if (success)
                {
                    _logger.LogInformation($"Successfully saved session '{name}' with {validatedFolders.Count} folders");
                }
                else
                {
                    _logger.LogWarning($"Failed to save session '{name}'");
                }

                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error saving session '{name}'");
                return false;
            }
        }

        public async Task<List<SessionInfo>> LoadAllSessionsAsync()
        {
            try
            {
                var sessions = await _sessionRepository.LoadSessionsAsync();
                _logger.LogInformation($"Loaded {sessions.Count} sessions");
                return sessions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading sessions");
                return new List<SessionInfo>();
            }
        }

        public async Task<SessionInfo?> GetSessionByIdAsync(string sessionId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(sessionId))
                    return null;

                return await _sessionRepository.GetSessionByIdAsync(sessionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting session by ID: {sessionId}");
                return null;
            }
        }

        public async Task<bool> DeleteSessionAsync(string sessionId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(sessionId))
                {
                    _logger.LogWarning("Cannot delete session: ID is null or empty");
                    return false;
                }

                var success = await _sessionRepository.DeleteSessionAsync(sessionId);
                
                if (success)
                {
                    _logger.LogInformation($"Successfully deleted session: {sessionId}");
                }
                else
                {
                    _logger.LogWarning($"Failed to delete session: {sessionId}");
                }

                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting session: {sessionId}");
                return false;
            }
        }

        public async Task<bool> RestoreSessionAsync(string sessionId)
        {
            var session = await GetSessionByIdAsync(sessionId);
            if (session == null)
            {
                _logger.LogWarning($"Cannot restore session: session not found - {sessionId}");
                return false;
            }

            return await RestoreSessionAsync(session);
        }

        public async Task<bool> RestoreSessionAsync(SessionInfo session)
        {
            try
            {
                if (session == null)
                {
                    _logger.LogWarning("Cannot restore session: session is null");
                    return false;
                }

                // フォルダの状態を更新
                session.UpdateFolderStatuses();

                // アクセス可能なフォルダのみを開く
                var accessibleFolders = session.GetAccessibleFolders();
                var inaccessibleFolders = session.GetInaccessibleFolders();

                if (inaccessibleFolders.Any())
                {
                    _logger.LogWarning($"Session '{session.Name}' has {inaccessibleFolders.Count} inaccessible folders");
                }

                if (!accessibleFolders.Any())
                {
                    _logger.LogWarning($"Session '{session.Name}' has no accessible folders");
                    return false;
                }

                var success = await _explorerService.OpenMultipleFoldersAsync(accessibleFolders);

                if (success)
                {
                    // 使用状況を更新
                    session.UpdateUsage();
                    await _sessionRepository.UpdateSessionAsync(session);
                    
                    _logger.LogInformation($"Successfully restored session '{session.Name}' with {accessibleFolders.Count} folders");
                }

                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error restoring session: {session?.Name}");
                return false;
            }
        }

        public async Task<bool> UpdateSessionAsync(SessionInfo session)
        {
            try
            {
                if (session == null)
                {
                    _logger.LogWarning("Cannot update session: session is null");
                    return false;
                }

                session.UpdatedAt = DateTime.Now;
                var success = await _sessionRepository.UpdateSessionAsync(session);
                
                if (success)
                {
                    _logger.LogInformation($"Successfully updated session: {session.Name}");
                }
                else
                {
                    _logger.LogWarning($"Failed to update session: {session.Name}");
                }

                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating session: {session?.Name}");
                return false;
            }
        }

        public async Task<List<SessionInfo>> GetFavoriteSessionsAsync()
        {
            try
            {
                var allSessions = await LoadAllSessionsAsync();
                var favorites = allSessions.Where(s => s.IsFavorite).ToList();
                
                _logger.LogInformation($"Found {favorites.Count} favorite sessions");
                return favorites;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting favorite sessions");
                return new List<SessionInfo>();
            }
        }

        public async Task<List<SessionInfo>> SearchSessionsAsync(string searchText)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchText))
                    return await LoadAllSessionsAsync();

                var allSessions = await LoadAllSessionsAsync();
                var searchLower = searchText.ToLowerInvariant();

                var results = allSessions.Where(s =>
                    s.Name.ToLowerInvariant().Contains(searchLower) ||
                    (s.Description?.ToLowerInvariant().Contains(searchLower) ?? false) ||
                    s.Tags.Any(t => t.ToLowerInvariant().Contains(searchLower)) ||
                    s.Folders.Any(f => f.Path.ToLowerInvariant().Contains(searchLower))
                ).ToList();

                _logger.LogInformation($"Search for '{searchText}' returned {results.Count} results");
                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error searching sessions: {searchText}");
                return new List<SessionInfo>();
            }
        }

        public async Task<SessionInfo?> GetLastUsedSessionAsync()
        {
            try
            {
                var allSessions = await LoadAllSessionsAsync();
                var lastUsed = allSessions
                    .Where(s => s.LastUsed.HasValue)
                    .OrderByDescending(s => s.LastUsed)
                    .FirstOrDefault();

                if (lastUsed != null)
                {
                    _logger.LogInformation($"Last used session: {lastUsed.Name}");
                }

                return lastUsed;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting last used session");
                return null;
            }
        }
    }
}