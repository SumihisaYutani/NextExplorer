using System;
using System.ComponentModel.DataAnnotations;

namespace NextExplorer.Models
{
    public class FolderInfo
    {
        [Required]
        [MaxLength(260)]
        public string Path { get; set; } = string.Empty;

        [MaxLength(255)]
        public string DisplayName { get; set; } = string.Empty;

        public bool Exists { get; set; }

        public bool IsAccessible { get; set; }

        [MaxLength(260)]
        public string? IconPath { get; set; }

        public DateTime LastAccessed { get; set; }

        public FolderType Type { get; set; }

        public string StatusText => Exists ? (IsAccessible ? "利用可能" : "アクセス拒否") : "存在しません";

        public string TypeText => Type switch
        {
            FolderType.Local => "ローカル",
            FolderType.Network => "ネットワーク",
            FolderType.Cloud => "クラウド",
            FolderType.Removable => "リムーバブル",
            _ => "不明"
        };

        public FolderInfo()
        {
            LastAccessed = DateTime.Now;
        }

        public FolderInfo(string path) : this()
        {
            Path = path;
            DisplayName = System.IO.Path.GetFileName(path) ?? path;
            DetermineType();
        }

        private void DetermineType()
        {
            if (string.IsNullOrEmpty(Path))
            {
                Type = FolderType.Local;
                return;
            }

            if (Path.StartsWith("\\\\"))
            {
                Type = FolderType.Network;
            }
            else if (Path.Length >= 2 && Path[1] == ':')
            {
                var driveInfo = new System.IO.DriveInfo(Path.Substring(0, 2));
                Type = driveInfo.DriveType switch
                {
                    System.IO.DriveType.Network => FolderType.Network,
                    System.IO.DriveType.Removable => FolderType.Removable,
                    System.IO.DriveType.CDRom => FolderType.Removable,
                    _ => FolderType.Local
                };
            }
            else
            {
                Type = FolderType.Local;
            }
        }

        public void UpdateStatus()
        {
            try
            {
                Exists = System.IO.Directory.Exists(Path);
                IsAccessible = Exists && CanAccess();
                LastAccessed = DateTime.Now;
            }
            catch
            {
                Exists = false;
                IsAccessible = false;
            }
        }

        private bool CanAccess()
        {
            try
            {
                System.IO.Directory.GetDirectories(Path);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public override string ToString()
        {
            return $"{DisplayName} ({Path})";
        }

        public override bool Equals(object? obj)
        {
            if (obj is FolderInfo other)
            {
                return string.Equals(Path, other.Path, StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Path.ToLowerInvariant().GetHashCode();
        }
    }

    public enum FolderType
    {
        Local,
        Network,
        Cloud,
        Removable
    }
}