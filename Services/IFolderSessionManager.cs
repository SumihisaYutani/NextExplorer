using NextExplorer.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NextExplorer.Services
{
    public interface IFolderSessionManager
    {
        Task<List<FolderInfo>> GetCurrentOpenFoldersAsync();
        Task<bool> SaveSessionAsync(string name, List<FolderInfo> folders, string? description = null);
        Task<List<SessionInfo>> LoadAllSessionsAsync();
        Task<SessionInfo?> GetSessionByIdAsync(string sessionId);
        Task<bool> DeleteSessionAsync(string sessionId);
        Task<bool> RestoreSessionAsync(string sessionId);
        Task<bool> RestoreSessionAsync(SessionInfo session);
        Task<bool> UpdateSessionAsync(SessionInfo session);
        Task<List<SessionInfo>> GetFavoriteSessionsAsync();
        Task<List<SessionInfo>> SearchSessionsAsync(string searchText);
        Task<SessionInfo?> GetLastUsedSessionAsync();
    }
}