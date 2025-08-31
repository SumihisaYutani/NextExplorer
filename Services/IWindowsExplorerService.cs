using NextExplorer.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NextExplorer.Services
{
    public interface IWindowsExplorerService
    {
        Task<List<FolderInfo>> GetOpenExplorerWindowsAsync();
        bool OpenFolder(string path);
        bool IsFolderAccessible(string path);
        Task<bool> OpenMultipleFoldersAsync(IEnumerable<FolderInfo> folders);
        Task<List<FolderInfo>> ValidateFoldersAsync(IEnumerable<FolderInfo> folders);
    }
}