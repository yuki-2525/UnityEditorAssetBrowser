// Copyright (c) 2025 yuki-2525

#nullable enable

using System;
using System.IO;

namespace UnityEditorAssetBrowser.Services
{
    /// <summary>
    /// UnityPackage操作を支援するサービスクラス
    /// </summary>
    public static class UnityPackageServices
    {
        /// <summary>
        /// 指定されたディレクトリ内のUnityPackageファイルを検索する
        /// </summary>
        /// <param name="directory">検索対象のディレクトリ</param>
        /// <returns>見つかったUnityPackageファイルのパス配列</returns>
        public static string[] FindUnityPackages(string directory)
        {
            if (string.IsNullOrEmpty(directory) || !Directory.Exists(directory))
            {
                return Array.Empty<string>();
            }

            return Directory.GetFiles(directory, "*.unitypackage", SearchOption.AllDirectories);
        }
    }
}
