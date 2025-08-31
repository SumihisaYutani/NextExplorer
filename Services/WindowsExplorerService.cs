using Microsoft.Extensions.Logging;
using NextExplorer.Models;
using NextExplorer.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NextExplorer.Services
{
    public class WindowsExplorerService : IWindowsExplorerService
    {
        private readonly ILogger<WindowsExplorerService> _logger;

        public WindowsExplorerService(ILogger<WindowsExplorerService> logger)
        {
            _logger = logger;
        }

        public async Task<List<FolderInfo>> GetOpenExplorerWindowsAsync()
        {
            return await Task.Run(() =>
            {
                var folders = new List<FolderInfo>();
                
                try
                {
                    var explorerWindows = Win32ApiWrapper.GetExplorerWindowHandles();
                    _logger.LogInformation($"Found {explorerWindows.Count} Explorer windows");

                    foreach (var hwnd in explorerWindows)
                    {
                        var windowTitle = Win32ApiWrapper.GetWindowText(hwnd);
                        if (string.IsNullOrEmpty(windowTitle))
                            continue;

                        var path = Win32ApiWrapper.ExtractPathFromWindowTitle(windowTitle);
                        if (!string.IsNullOrEmpty(path))
                        {
                            var folderInfo = new FolderInfo(path);
                            folderInfo.UpdateStatus();
                            
                            if (!folders.Any(f => f.Equals(folderInfo)))
                            {
                                folders.Add(folderInfo);
                                _logger.LogDebug($"Added folder: {path}");
                            }
                        }
                    }

                    _logger.LogInformation($"Extracted {folders.Count} unique folder paths");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error getting open Explorer windows");
                }

                return folders;
            });
        }

        public bool OpenFolder(string path)
        {
            try
            {
                if (string.IsNullOrEmpty(path))
                {
                    _logger.LogWarning("Cannot open folder: path is null or empty");
                    return false;
                }

                if (!System.IO.Directory.Exists(path))
                {
                    _logger.LogWarning($"Cannot open folder: path does not exist - {path}");
                    return false;
                }

                var success = Win32ApiWrapper.OpenFolder(path);
                if (success)
                {
                    _logger.LogInformation($"Successfully opened folder: {path}");
                }
                else
                {
                    _logger.LogWarning($"Failed to open folder: {path}");
                }

                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error opening folder: {path}");
                return false;
            }
        }

        public bool IsFolderAccessible(string path)
        {
            try
            {
                return Win32ApiWrapper.IsFolderAccessible(path);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking folder accessibility: {path}");
                return false;
            }
        }

        public async Task<bool> OpenMultipleFoldersAsync(IEnumerable<FolderInfo> folders)
        {
            if (folders == null)
            {
                _logger.LogWarning("Cannot open folders: collection is null");
                return false;
            }

            var folderList = folders.ToList();
            if (!folderList.Any())
            {
                _logger.LogWarning("Cannot open folders: collection is empty");
                return false;
            }

            return await Task.Run(() =>
            {
                int successCount = 0;
                int totalCount = folderList.Count;

                _logger.LogInformation($"Attempting to open {totalCount} folders");

                foreach (var folder in folderList)
                {
                    try
                    {
                        if (OpenFolder(folder.Path))
                        {
                            successCount++;
                            // Small delay to prevent overwhelming the system
                            System.Threading.Thread.Sleep(200);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error opening folder: {folder.Path}");
                    }
                }

                _logger.LogInformation($"Opened {successCount}/{totalCount} folders successfully");
                return successCount > 0;
            });
        }

        public async Task<List<FolderInfo>> ValidateFoldersAsync(IEnumerable<FolderInfo> folders)
        {
            if (folders == null)
                return new List<FolderInfo>();

            return await Task.Run(() =>
            {
                var validatedFolders = new List<FolderInfo>();

                foreach (var folder in folders)
                {
                    try
                    {
                        folder.UpdateStatus();
                        validatedFolders.Add(folder);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error validating folder: {folder.Path}");
                        folder.Exists = false;
                        folder.IsAccessible = false;
                        validatedFolders.Add(folder);
                    }
                }

                var accessibleCount = validatedFolders.Count(f => f.IsAccessible);
                _logger.LogInformation($"Validated {validatedFolders.Count} folders, {accessibleCount} accessible");

                return validatedFolders;
            });
        }
    }
}