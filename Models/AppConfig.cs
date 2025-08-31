using System;

namespace NextExplorer.Models
{
    public class AppConfig
    {
        public string Version { get; set; } = "1.0.0";
        public StartupSettings Startup { get; set; } = new();
        public UISettings UI { get; set; } = new();
        public SecuritySettings Security { get; set; } = new();
        public PerformanceSettings Performance { get; set; } = new();
    }

    public class StartupSettings
    {
        public bool AutoStart { get; set; } = false;
        public bool RestoreLastSession { get; set; } = false;
        public bool AutoSaveOnExit { get; set; } = true;
        public bool MinimizeToTray { get; set; } = false;
    }

    public class UISettings
    {
        public WindowState MainWindow { get; set; } = new();
        public string Theme { get; set; } = "Light";
        public string Language { get; set; } = "ja-JP";
        public FontSettings Font { get; set; } = new();
    }

    public class WindowState
    {
        public double Left { get; set; } = 100;
        public double Top { get; set; } = 100;
        public double Width { get; set; } = 800;
        public double Height { get; set; } = 600;
        public bool IsMaximized { get; set; } = false;
    }

    public class FontSettings
    {
        public string Family { get; set; } = "Yu Gothic UI";
        public double Size { get; set; } = 12;
    }

    public class SecuritySettings
    {
        public bool EncryptDataFiles { get; set; } = false;
        public string? EncryptionKeyHash { get; set; }
        public bool BlockInvalidPaths { get; set; } = true;
    }

    public class PerformanceSettings
    {
        public int FolderCheckTimeout { get; set; } = 5000;
        public int CacheExpiration { get; set; } = 30;
        public int MaxConcurrentFolders { get; set; } = 20;
        public bool EnableFolderWatching { get; set; } = false;
    }
}