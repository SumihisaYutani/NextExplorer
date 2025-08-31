using System;
using System.IO;

namespace NextExplorer.Utils
{
    public static class FilePathHelper
    {
        /// <summary>
        /// 実行ファイルのディレクトリを取得
        /// </summary>
        public static string GetExecutableDirectory()
        {
            return Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) 
                ?? Environment.CurrentDirectory;
        }

        /// <summary>
        /// データファイル用ディレクトリを取得（実行ファイル直下/Data）
        /// </summary>
        public static string GetDataDirectory()
        {
            return Path.Combine(GetExecutableDirectory(), "Data");
        }

        /// <summary>
        /// ログファイル用ディレクトリを取得（実行ファイル直下/Logs）
        /// </summary>
        public static string GetLogsDirectory()
        {
            return Path.Combine(GetExecutableDirectory(), "Logs");
        }

        /// <summary>
        /// 一時ファイル用ディレクトリを取得（実行ファイル直下/Temp）
        /// </summary>
        public static string GetTempDirectory()
        {
            return Path.Combine(GetExecutableDirectory(), "Temp");
        }

        /// <summary>
        /// ディレクトリが存在しない場合は作成
        /// </summary>
        public static void EnsureDirectoryExists(string directory)
        {
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        /// <summary>
        /// 今日の日付でログファイル名を生成
        /// </summary>
        public static string GetTodayLogFileName(string prefix = "app")
        {
            return $"{prefix}_{DateTime.Now:yyyyMMdd}.log";
        }
    }
}