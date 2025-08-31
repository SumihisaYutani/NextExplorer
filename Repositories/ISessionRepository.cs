using NextExplorer.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NextExplorer.Repositories
{
    public interface ISessionRepository
    {
        Task<List<SessionInfo>> LoadSessionsAsync();
        Task<bool> SaveSessionAsync(SessionInfo session);
        Task<bool> SaveSessionsAsync(List<SessionInfo> sessions);
        Task<SessionInfo?> GetSessionByIdAsync(string id);
        Task<bool> DeleteSessionAsync(string id);
        Task<bool> UpdateSessionAsync(SessionInfo session);
        Task<bool> SessionExistsAsync(string id);
        Task<string> GetDataFilePathAsync();
        Task<bool> BackupDataAsync();
        Task<bool> RestoreFromBackupAsync();
    }
}