using Microsoft.Extensions.Logging;
using NextExplorer.Models;
using NextExplorer.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace NextExplorer.Repositories
{
    public class SessionRepository : ISessionRepository
    {
        private readonly ILogger<SessionRepository> _logger;
        private readonly string _dataDirectory;
        private readonly string _dataFilePath;
        private readonly string _backupFilePath;

        public SessionRepository(ILogger<SessionRepository> logger)
        {
            _logger = logger;
            
            // FilePathHelperを使用して実行ファイル直下にデータディレクトリを配置
            _dataDirectory = FilePathHelper.GetDataDirectory();
            _dataFilePath = Path.Combine(_dataDirectory, "sessions.json");
            _backupFilePath = Path.Combine(_dataDirectory, "sessions_backup.json");
            
            EnsureDataDirectoryExists();
        }

        private void EnsureDataDirectoryExists()
        {
            try
            {
                FilePathHelper.EnsureDirectoryExists(_dataDirectory);
                _logger.LogInformation($"Data directory ensured: {_dataDirectory}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to create data directory: {_dataDirectory}");
            }
        }

        public async Task<List<SessionInfo>> LoadSessionsAsync()
        {
            try
            {
                if (!File.Exists(_dataFilePath))
                {
                    _logger.LogInformation("Sessions file does not exist, returning empty list");
                    return new List<SessionInfo>();
                }

                var jsonContent = await File.ReadAllTextAsync(_dataFilePath);
                if (string.IsNullOrWhiteSpace(jsonContent))
                {
                    _logger.LogWarning("Sessions file is empty");
                    return new List<SessionInfo>();
                }

                var sessionData = JsonConvert.DeserializeObject<SessionFileData>(jsonContent);
                if (sessionData?.Sessions == null)
                {
                    _logger.LogWarning("Invalid session data format");
                    return new List<SessionInfo>();
                }

                _logger.LogInformation($"Loaded {sessionData.Sessions.Count} sessions from file");
                return sessionData.Sessions;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to deserialize sessions file");
                // Try to restore from backup
                return await TryRestoreFromBackupAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load sessions");
                return new List<SessionInfo>();
            }
        }

        public async Task<bool> SaveSessionAsync(SessionInfo session)
        {
            try
            {
                if (session == null)
                {
                    _logger.LogWarning("Cannot save null session");
                    return false;
                }

                var sessions = await LoadSessionsAsync();
                var existingSession = sessions.FirstOrDefault(s => s.Id == session.Id);
                
                if (existingSession != null)
                {
                    // Update existing session
                    var index = sessions.IndexOf(existingSession);
                    sessions[index] = session;
                    _logger.LogInformation($"Updated existing session: {session.Name}");
                }
                else
                {
                    // Add new session
                    sessions.Add(session);
                    _logger.LogInformation($"Added new session: {session.Name}");
                }

                return await SaveSessionsAsync(sessions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to save session: {session?.Name}");
                return false;
            }
        }

        public async Task<bool> SaveSessionsAsync(List<SessionInfo> sessions)
        {
            try
            {
                if (sessions == null)
                {
                    _logger.LogWarning("Cannot save null sessions list");
                    return false;
                }

                // Create backup before saving
                await BackupDataAsync();

                var sessionData = new SessionFileData
                {
                    Version = "1.0.0",
                    LastModified = DateTime.UtcNow,
                    Sessions = sessions
                };

                var jsonContent = JsonConvert.SerializeObject(sessionData, Formatting.Indented, new JsonSerializerSettings
                {
                    DateFormatHandling = DateFormatHandling.IsoDateFormat,
                    DateTimeZoneHandling = DateTimeZoneHandling.Utc
                });

                await File.WriteAllTextAsync(_dataFilePath, jsonContent);
                _logger.LogInformation($"Saved {sessions.Count} sessions to file");
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save sessions to file");
                return false;
            }
        }

        public async Task<SessionInfo?> GetSessionByIdAsync(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                    return null;

                var sessions = await LoadSessionsAsync();
                return sessions.FirstOrDefault(s => s.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to get session by ID: {id}");
                return null;
            }
        }

        public async Task<bool> DeleteSessionAsync(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    _logger.LogWarning("Cannot delete session: ID is null or empty");
                    return false;
                }

                var sessions = await LoadSessionsAsync();
                var sessionToRemove = sessions.FirstOrDefault(s => s.Id == id);
                
                if (sessionToRemove == null)
                {
                    _logger.LogWarning($"Session not found for deletion: {id}");
                    return false;
                }

                sessions.Remove(sessionToRemove);
                var success = await SaveSessionsAsync(sessions);
                
                if (success)
                {
                    _logger.LogInformation($"Deleted session: {sessionToRemove.Name}");
                }
                
                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to delete session: {id}");
                return false;
            }
        }

        public async Task<bool> UpdateSessionAsync(SessionInfo session)
        {
            // UpdateSessionAsync is essentially the same as SaveSessionAsync for this implementation
            return await SaveSessionAsync(session);
        }

        public async Task<bool> SessionExistsAsync(string id)
        {
            try
            {
                var session = await GetSessionByIdAsync(id);
                return session != null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to check if session exists: {id}");
                return false;
            }
        }

        public async Task<string> GetDataFilePathAsync()
        {
            return await Task.FromResult(_dataFilePath);
        }

        public async Task<bool> BackupDataAsync()
        {
            try
            {
                if (!File.Exists(_dataFilePath))
                    return true; // No file to backup

                await Task.Run(() => File.Copy(_dataFilePath, _backupFilePath, true));
                _logger.LogInformation("Created backup of sessions data");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create backup");
                return false;
            }
        }

        public async Task<bool> RestoreFromBackupAsync()
        {
            try
            {
                if (!File.Exists(_backupFilePath))
                {
                    _logger.LogWarning("No backup file found for restoration");
                    return false;
                }

                await Task.Run(() => File.Copy(_backupFilePath, _dataFilePath, true));
                _logger.LogInformation("Restored sessions data from backup");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to restore from backup");
                return false;
            }
        }

        private async Task<List<SessionInfo>> TryRestoreFromBackupAsync()
        {
            try
            {
                _logger.LogInformation("Attempting to restore from backup due to corrupted main file");
                
                if (await RestoreFromBackupAsync())
                {
                    return await LoadSessionsAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to restore from backup");
            }
            
            return new List<SessionInfo>();
        }
    }

    internal class SessionFileData
    {
        public string Version { get; set; } = "1.0.0";
        public DateTime LastModified { get; set; }
        public List<SessionInfo> Sessions { get; set; } = new();
    }
}