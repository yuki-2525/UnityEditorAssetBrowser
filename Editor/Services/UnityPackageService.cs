// Copyright (c) 2025 yuki-2525

#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditorAssetBrowser.Services;
using UnityEditorAssetBrowser.Views;
using UnityEngine;

namespace UnityEditorAssetBrowser.Services
{
    /// <summary>
    /// UnityPackage操作を支援するサービスクラス
    /// UnityPackageファイルの検索、読み込み、書き込みなどの機能を提供する
    /// </summary>
    public static class UnityPackageServices
    {
        /// <summary>
        /// 指定されたディレクトリ内のUnityPackageファイルを検索する
        /// サブディレクトリも再帰的に検索する
        /// </summary>
        /// <param name="directory">検索対象のディレクトリパス</param>
        /// <returns>見つかったUnityPackageファイルのパス配列。ディレクトリが存在しない場合は空の配列を返す</returns>
        public static string[] FindUnityPackages(string directory)
        {
            if (directory == null)
            {
                Debug.LogError("ディレクトリパスがnullです");
                return Array.Empty<string>();
            }

            if (string.IsNullOrEmpty(directory))
            {
                Debug.LogError("ディレクトリパスが空です");
                return Array.Empty<string>();
            }

            if (!Directory.Exists(directory))
            {
                Debug.LogError($"ディレクトリが存在しません: {directory}");
                return Array.Empty<string>();
            }

            try
            {
                return Directory.GetFiles(directory, "*.unitypackage", SearchOption.AllDirectories);
            }
            catch (Exception ex)
                when (ex is UnauthorizedAccessException || ex is PathTooLongException)
            {
                Debug.LogError($"UnityPackageファイルの検索中にエラーが発生しました: {ex.Message}");
                return Array.Empty<string>();
            }
        }

        /// <summary>
        /// UnityPackageをインポートし、フォルダのサムネイルを設定する
        /// </summary>
        /// <param name="packagePath">パッケージパス</param>
        /// <param name="imagePath">サムネイル画像パス</param>
        public static void ImportPackageAndSetThumbnails(string packagePath, string imagePath)
        {
            // インポート前のフォルダ一覧を取得
            var beforeFolders = GetAssetFolders();

            // インポート処理を実行（完了時のコールバックを指定）
            AssetDatabase.ImportPackage(packagePath, true);

            // 既存のハンドラが残っていると多重実行になるため、一旦解除
            if (_importCompletedHandler != null)
            {
                AssetDatabase.importPackageCompleted -= _importCompletedHandler;
            }

            _importCompletedHandler = packageName =>
            {
                // アセットデータベースを更新
                AssetDatabase.Refresh();

                // インポート後のフォルダ一覧を取得
                var afterFolders = GetAssetFolders();

                // 新しく追加されたフォルダを特定
                var newFolders = afterFolders.Except(beforeFolders).ToList();
                if (newFolders.Any())
                {
                    // サムネイル設定を実行
                    SetFolderThumbnails(newFolders, imagePath);
                }
                else
                {
                    Debug.LogWarning("[UnityPackageService] 新規フォルダが見つかりませんでした");
                }

                // ハンドラを使い捨てにする
                if (_importCompletedHandler != null)
                {
                    AssetDatabase.importPackageCompleted -= _importCompletedHandler;
                    _importCompletedHandler = null;
                }
            };

            AssetDatabase.importPackageCompleted += _importCompletedHandler;
        }

        /// <summary>
        /// Assetsフォルダ内のフォルダ一覧を取得
        /// </summary>
        /// <returns>フォルダパスのリスト</returns>
        private static List<string> GetAssetFolders()
        {
            var folders = new List<string>();
            // AssetDatabaseを使用してフォルダを取得
            string[] guids = AssetDatabase.FindAssets("t:Folder", new[] { "Assets" });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (!string.IsNullOrEmpty(path))
                {
                    folders.Add(path);
                }
            }
            return folders;
        }

        /// <summary>
        /// フォルダのサムネイルを設定する
        /// </summary>
        /// <param name="folders">フォルダパスのリスト</param>
        /// <param name="imagePath">サムネイル画像パス</param>
        private static void SetFolderThumbnails(List<string> folders, string imagePath)
        {
            if (folders == null || !folders.Any())
            {
                Debug.LogWarning("[UnityPackageService] フォルダが指定されていません");
                return;
            }

            if (string.IsNullOrEmpty(imagePath))
            {
                Debug.LogWarning("[UnityPackageService] サムネイル画像パスが指定されていません");
                return;
            }

            // AssetItemViewのインスタンスを作成してGetFullImagePathを呼び出す
            var assetItemView = new AssetItemView(null);
            string fullImagePath = assetItemView.GetFullImagePath(imagePath);

            if (string.IsNullOrEmpty(fullImagePath))
            {
                Debug.LogWarning("[UnityPackageService] 完全な画像パスを取得できませんでした");
                return;
            }

            // 画像ファイルが存在するか確認
            if (!File.Exists(fullImagePath))
            {
                Debug.LogWarning(
                    $"[UnityPackageService] サムネイル画像が見つかりません: {fullImagePath}"
                );
                return;
            }

            // 保存先決定ロジック（除外フォルダが含まれる場合の特別処理）
            var targetFolders = new HashSet<string>();
            var excluded = folders
                .Where(f => ExcludeFolderService.IsExcludedFolder(f.Split('/').Last()))
                .ToList();
            if (excluded.Any())
            {
                // 最も浅い除外フォルダの1つ上のフォルダを保存先とする
                var shallowest = excluded.OrderBy(f => f.Count(c => c == '/')).First();
                var parts = shallowest.Split('/');
                if (parts.Length > 1)
                {
                    string parent = string.Join("/", parts.Take(parts.Length - 1));
                    // Assets直下のFolderIcon.jpgは絶対に保存しない
                    if (!string.IsNullOrEmpty(parent) && !IsRootFolderIcon(parent))
                        targetFolders.Add(parent);
                }
                // それより下の階層の新規フォルダは無視
            }
            else
            {
                // 除外フォルダがなければ、最も深い共通の親ディレクトリのみを対象にする
                if (folders.Any())
                {
                    string commonParent = GetDeepestCommonParent(folders);
                    if (!string.IsNullOrEmpty(commonParent))
                    {
                        string bestFolder = FindBestThumbnailFolder(commonParent);
                        if (!string.IsNullOrEmpty(bestFolder) && !IsRootFolderIcon(bestFolder))
                        {
                            targetFolders.Add(bestFolder);
                        }
                    }
                }
            }

            if (!targetFolders.Any())
            {
                Debug.LogWarning("[UnityPackageService] 対象フォルダが見つかりませんでした");
                return;
            }

            // 各フォルダにサムネイル画像をコピー
            foreach (string folder in targetFolders)
            {
                try
                {
                    string targetPath = Path.Combine(folder, "FolderIcon.jpg");
                    File.Copy(fullImagePath, targetPath, true);
                }
                catch (Exception ex)
                {
                    Debug.LogWarning(
                        $"[UnityPackageService] サムネイル画像のコピーに失敗しました: {folder} - {ex.Message}"
                    );
                }
            }

            // 最適なサムネイル保存先を決定する
            static string FindBestThumbnailFolder(string folder)
            {
                string[] parts = folder.Split('/');
                // 除外フォルダがパスに含まれる場合は、最初の除外フォルダの1つ上を返す
                for (int i = 1; i < parts.Length; i++)
                {
                    if (ExcludeFolderService.IsExcludedFolder(parts[i]))
                    {
                        return string.Join("/", parts.Take(i));
                    }
                }
                // 再帰的に最適な深さを探す
                string current = folder;
                while (true)
                {
                    var dirs = Directory.GetDirectories(current).ToList();
                    var files = Directory
                        .GetFiles(current)
                        .Where(f => Path.GetExtension(f) != ".meta")
                        .ToList();
                    // フォルダが1つだけ、かつ除外フォルダでなく、ファイルが無い場合はさらに深く
                    if (
                        dirs.Count == 1
                        && !ExcludeFolderService.IsExcludedFolder(Path.GetFileName(dirs[0]))
                        && files.Count == 0
                    )
                    {
                        current = dirs[0];
                        continue;
                    }
                    break;
                }
                return current;
            }

            // アセットデータベースを更新して表示を更新
            AssetDatabase.Refresh();
        }

        // importPackageCompleted 用の一時ハンドラ
        private static AssetDatabase.ImportPackageCallback? _importCompletedHandler;

        /// <summary>
        /// 指定したパスがAssets直下のFolderIcon.jpgか判定する
        /// 例: Assets/FolderIcon.jpg → true, Assets/Folder1/FolderIcon.jpg → false
        /// </summary>
        /// <param name="folderPath">判定するフォルダパス</param>
        /// <returns>Assets直下のFolderIcon.jpgならtrue</returns>
        private static bool IsRootFolderIcon(string folderPath)
        {
            if (string.IsNullOrEmpty(folderPath))
                return false;
            var parts = folderPath.Split('/');
            return parts.Length == 2 && parts[0] == "Assets" && parts[1] == "FolderIcon.jpg";
        }

        // 複数パスの最も深い共通の親ディレクトリを求める
        private static string GetDeepestCommonParent(IEnumerable<string> paths)
        {
            if (paths == null || !paths.Any())
                return string.Empty;
            var splitPaths = paths.Select(p => p.Split('/')).ToList();
            int minLen = splitPaths.Min(arr => arr.Length);
            List<string> common = new List<string>();
            for (int i = 0; i < minLen; i++)
            {
                string part = splitPaths[0][i];
                if (splitPaths.All(arr => arr[i] == part))
                {
                    common.Add(part);
                }
                else
                {
                    break;
                }
            }
            return common.Count > 0 ? string.Join("/", common) : string.Empty;
        }

        [Serializable]
        private class ExcludeFoldersData
        {
            public List<string> folders = new List<string>();
        }
    }
}
