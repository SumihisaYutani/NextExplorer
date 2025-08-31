using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace NextExplorer.Utils
{
    public static class Win32ApiWrapper
    {
        private const int MAX_PATH = 260;
        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_APPWINDOW = 0x40000;

        public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder strText, int maxCount);

        [DllImport("user32.dll")]
        public static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        [DllImport("user32.dll")]
        public static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern IntPtr GetParent(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("shell32.dll")]
        public static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes,
            ref SHFILEINFO psfi, uint cbSizeFileInfo, uint uFlags);

        [DllImport("shell32.dll")]
        public static extern int SHGetPathFromIDList(IntPtr pidl, StringBuilder pszPath);

        [DllImport("shell32.dll")]
        public static extern IntPtr SHGetPathFromIDListW(IntPtr pidl, StringBuilder pszPath);

        [StructLayout(LayoutKind.Sequential)]
        public struct SHFILEINFO
        {
            public IntPtr hIcon;
            public IntPtr iIcon;
            public uint dwAttributes;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szDisplayName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szTypeName;
        }

        public static List<IntPtr> GetExplorerWindowHandles()
        {
            var explorerWindows = new List<IntPtr>();

            EnumWindows((hWnd, lParam) =>
            {
                if (IsExplorerWindow(hWnd))
                {
                    explorerWindows.Add(hWnd);
                }
                return true;
            }, IntPtr.Zero);

            return explorerWindows;
        }

        private static bool IsExplorerWindow(IntPtr hWnd)
        {
            if (!IsWindowVisible(hWnd))
                return false;

            if (GetParent(hWnd) != IntPtr.Zero)
                return false;

            var className = new StringBuilder(256);
            GetClassName(hWnd, className, className.Capacity);
            
            return className.ToString() == "CabinetWClass" || 
                   className.ToString() == "ExploreWClass";
        }

        public static string GetWindowText(IntPtr hWnd)
        {
            int length = GetWindowTextLength(hWnd);
            if (length == 0) return string.Empty;

            // 長いパス名に対応するため、バッファサイズを大きく設定
            int bufferSize = Math.Max(length + 1, 32768); // 32KB確保
            var sb = new StringBuilder(bufferSize);
            GetWindowText(hWnd, sb, sb.Capacity);
            return sb.ToString();
        }

        public static string GetClassName(IntPtr hWnd)
        {
            var sb = new StringBuilder(256);
            GetClassName(hWnd, sb, sb.Capacity);
            return sb.ToString();
        }

        public static string? ExtractPathFromWindowTitle(string windowTitle)
        {
            if (string.IsNullOrEmpty(windowTitle))
                return null;

            // 直接パスの場合
            if (windowTitle.Length >= 3 && windowTitle[1] == ':' && 
                (windowTitle[2] == '\\' || windowTitle[2] == '/'))
            {
                var normalizedPath = windowTitle.Replace('/', '\\');
                if (System.IO.Directory.Exists(normalizedPath))
                    return normalizedPath;
            }

            // " - " で分割されている場合
            var pathIndex = windowTitle.LastIndexOf(" - ");
            if (pathIndex > 0)
            {
                var potentialPath = windowTitle.Substring(pathIndex + 3);
                if (System.IO.Directory.Exists(potentialPath))
                {
                    return potentialPath;
                }
            }

            // ウィンドウタイトル全体がパスの場合
            if (System.IO.Directory.Exists(windowTitle))
            {
                return windowTitle;
            }

            // 部分的に切り詰められている可能性を考慮
            // パスの最後の部分を段階的に削除して確認
            var testPath = windowTitle;
            while (!string.IsNullOrEmpty(testPath) && testPath.Length > 10)
            {
                if (System.IO.Directory.Exists(testPath))
                    return testPath;
                
                // 最後の文字を削除して再試行
                testPath = testPath.Substring(0, testPath.Length - 1);
                
                // より効率的に、区切り文字まで戻る
                var lastSeparator = Math.Max(testPath.LastIndexOf('\\'), testPath.LastIndexOf('/'));
                if (lastSeparator > 0)
                {
                    testPath = testPath.Substring(0, lastSeparator);
                    if (System.IO.Directory.Exists(testPath))
                    {
                        // 親ディレクトリが見つかった場合、子ディレクトリを探索
                        return FindLongestMatchingPath(testPath, windowTitle);
                    }
                }
                else
                {
                    break;
                }
            }

            return null;
        }

        /// <summary>
        /// 指定された親ディレクトリから、ウィンドウタイトルに最も近い子フォルダを探す
        /// </summary>
        private static string? FindLongestMatchingPath(string parentPath, string windowTitle)
        {
            try
            {
                var directories = System.IO.Directory.GetDirectories(parentPath);
                string? bestMatch = null;
                int bestMatchLength = 0;

                foreach (var dir in directories)
                {
                    var dirName = System.IO.Path.GetFileName(dir);
                    if (!string.IsNullOrEmpty(dirName) && windowTitle.Contains(dirName) && dirName.Length > bestMatchLength)
                    {
                        bestMatch = dir;
                        bestMatchLength = dirName.Length;
                    }
                }

                return bestMatch ?? parentPath;
            }
            catch
            {
                return parentPath;
            }
        }

        public static bool OpenFolder(string path)
        {
            try
            {
                if (string.IsNullOrEmpty(path) || !System.IO.Directory.Exists(path))
                    return false;

                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "explorer.exe",
                    Arguments = $"\"{path}\"",
                    UseShellExecute = true
                });

                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool IsFolderAccessible(string path)
        {
            try
            {
                if (string.IsNullOrEmpty(path))
                    return false;

                return System.IO.Directory.Exists(path) && 
                       System.IO.Directory.GetDirectories(path).Length >= 0;
            }
            catch
            {
                return false;
            }
        }

        public static string? GetFolderDisplayName(string path)
        {
            try
            {
                if (string.IsNullOrEmpty(path))
                    return null;

                var dirInfo = new System.IO.DirectoryInfo(path);
                return dirInfo.Name;
            }
            catch
            {
                return System.IO.Path.GetFileName(path);
            }
        }
    }
}