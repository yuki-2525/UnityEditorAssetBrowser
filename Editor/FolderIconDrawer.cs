using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditorAssetBrowser.Services;
using UnityEngine;

namespace UnityEditorAssetBrowser
{
    /// <summary>
    /// フォルダ内の "FolderIcon.jpg" をプロジェクトウィンドウでのフォルダサムネイルとして描画するクラス
    /// </summary>
    [InitializeOnLoad]
    public static class FolderIconDrawer
    {
        // EditorPrefsのキー：フォルダアイコン表示設定
        private const string PREFS_KEY_SHOW_FOLDER_THUMBNAIL =
            "UnityEditorAssetBrowser_ShowFolderThumbnail";
        private const string PREFS_KEY_EXCLUDE_FOLDERS = "UnityEditorAssetBrowser_ExcludeFolders";

        // 現在イベントが登録されているかどうかのフラグ
        private static bool _isRegistered = false;

        // 静的コンストラクタ：エディタ起動時に呼ばれる
        static FolderIconDrawer()
        {
            // 起動時にEditorPrefsの値で初期化
            SetEnabled(EditorPrefs.GetBool(PREFS_KEY_SHOW_FOLDER_THUMBNAIL, true));
        }

        /// <summary>
        /// フォルダアイコン描画の有効/無効を切り替える
        /// </summary>
        public static void SetEnabled(bool enabled)
        {
            if (enabled && !_isRegistered)
            {
                // 有効化：イベント登録
                EditorApplication.projectWindowItemOnGUI -= OnProjectWindowItemGUI;
                EditorApplication.projectWindowItemOnGUI += OnProjectWindowItemGUI;
                _isRegistered = true;
            }
            else if (!enabled && _isRegistered)
            {
                // 無効化：イベント解除
                EditorApplication.projectWindowItemOnGUI -= OnProjectWindowItemGUI;
                _isRegistered = false;
            }
        }

        /// <summary>
        /// プロジェクトウィンドウの各アイテム描画コールバック
        /// </summary>
        private static void OnProjectWindowItemGUI(string guid, Rect rect)
        {
            if (Event.current.type != EventType.Repaint)
                return;

            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
                return;

            // フォルダの場合のみ
            if (!File.GetAttributes(path).HasFlag(FileAttributes.Directory))
                return;

            // 除外フォルダ名判定
            string folderName = Path.GetFileName(path);
            if (ExcludeFolderService.IsExcludedFolder(folderName))
                return;

            // 自フォルダ内のFolderIcon.jpgを最優先で取得
            string selfIconPath = Path.Combine(path, "FolderIcon.jpg").Replace("\\", "/");
            Texture2D selfTexture = null;
            if (File.Exists(selfIconPath))
                selfTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(selfIconPath);

            List<string> iconPaths;
            if (selfTexture != null)
            {
                iconPaths = new List<string> { selfIconPath };
            }
            else
            {
                iconPaths = FindFolderIconsRecursive(path, 0, 4, 4);
            }
            var textures = iconPaths
                .Select(p => AssetDatabase.LoadAssetAtPath<Texture2D>(p))
                .Where(t => t != null)
                .ToList();
            if (textures.Count == 0)
                return;

            // アイコン領域に合わせて描画矩形を計算
            Rect imageRect;
            if (rect.height > 20)
            {
                // 一覧ビュー・大きいサムネイルビュー
                imageRect = new Rect(rect.x - 1, rect.y - 1, rect.width + 2, rect.width + 2);
            }
            else if (rect.x > 20)
            {
                // 詳細ビュー（リスト形式）
                imageRect = new Rect(rect.x - 1, rect.y - 1, rect.height + 2, rect.height + 2);
            }
            else
            {
                // プロジェクトウィンドウ左ペイン
                imageRect = new Rect(rect.x + 2, rect.y - 1, rect.height + 2, rect.height + 2);
            }

            // imageRectの幅・高さが奇数なら+1して偶数に揃える
            float evenWidth = imageRect.width % 2 == 0 ? imageRect.width : imageRect.width + 1;
            float evenHeight = imageRect.height % 2 == 0 ? imageRect.height : imageRect.height + 1;

            if (textures.Count == 1)
            {
                // 1枚だけなら全体に表示
                GUI.DrawTexture(imageRect, textures[0]);
            }
            else
            {
                // 2～4枚なら4分割して表示、足りない領域は白
                int halfW = Mathf.FloorToInt(evenWidth / 2f);
                int halfH = Mathf.FloorToInt(evenHeight / 2f);
                int rightW = Mathf.RoundToInt(evenWidth) - halfW;
                int bottomH = Mathf.RoundToInt(evenHeight) - halfH;
                Rect[] subRects = new Rect[]
                {
                    new Rect(imageRect.x, imageRect.y, halfW, halfH), // 左上
                    new Rect(imageRect.x + halfW, imageRect.y, rightW, halfH), // 右上
                    new Rect(imageRect.x, imageRect.y + halfH, halfW, bottomH), // 左下
                    new Rect(imageRect.x + halfW, imageRect.y + halfH, rightW, bottomH), // 右下
                };
                for (int i = 0; i < 4; i++)
                {
                    if (i < textures.Count)
                    {
                        GUI.DrawTexture(subRects[i], textures[i]);
                    }
                    else
                    {
                        EditorGUI.DrawRect(subRects[i], Color.white);
                    }
                }
            }
        }

        // 指定フォルダ以下で除外フォルダをスキップしつつFolderIcon.jpgを最大maxCount枚まで再帰的に探索（最大depth階層）
        private static List<string> FindFolderIconsRecursive(
            string root,
            int currentDepth,
            int maxDepth,
            int maxCount
        )
        {
            var result = new List<string>();
            if (currentDepth >= maxDepth)
                return result;
            try
            {
                foreach (var dir in Directory.GetDirectories(root))
                {
                    if (result.Count >= maxCount)
                        break;
                    string folderName = Path.GetFileName(dir);
                    if (ExcludeFolderService.IsExcludedFolder(folderName))
                        continue;
                    string iconPath = Path.Combine(dir, "FolderIcon.jpg").Replace("\\", "/");
                    if (File.Exists(iconPath))
                        result.Add(iconPath);
                    if (result.Count >= maxCount)
                        break;
                    // 再帰探索
                    var found = FindFolderIconsRecursive(
                        dir,
                        currentDepth + 1,
                        maxDepth,
                        maxCount - result.Count
                    );
                    result.AddRange(found);
                    if (result.Count >= maxCount)
                        break;
                }
            }
            catch { }
            return result;
        }

        [System.Serializable]
        private class ExcludeFoldersData
        {
            public List<string> folders = new List<string>();
        }
    }
}
