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

            var sb = new StringBuilder(length + 1);
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

            if (windowTitle.Length >= 3 && windowTitle[1] == ':' && 
                (windowTitle[2] == '\\' || windowTitle[2] == '/'))
            {
                return windowTitle.Replace('/', '\\');
            }

            var pathIndex = windowTitle.LastIndexOf(" - ");
            if (pathIndex > 0)
            {
                var potentialPath = windowTitle.Substring(pathIndex + 3);
                if (System.IO.Directory.Exists(potentialPath))
                {
                    return potentialPath;
                }
            }

            if (System.IO.Directory.Exists(windowTitle))
            {
                return windowTitle;
            }

            return null;
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