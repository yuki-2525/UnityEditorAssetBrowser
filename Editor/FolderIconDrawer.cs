using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace UnityEditorAssetBrowser
{
    /// <summary>
    /// フォルダ内の "FolderIcon.jpg" をプロジェクトウィンドウでのフォルダサムネイルとして描画するクラス
    /// エディタ起動時に自動で有効化され、常にサムネイルを描画する
    /// </summary>
    [InitializeOnLoad]
    public static class FolderIconDrawer
    {
        static FolderIconDrawer()
        {
            // 重複登録を避けるため、一旦解除してから登録
            EditorApplication.projectWindowItemOnGUI -= OnProjectWindowItemGUI;
            EditorApplication.projectWindowItemOnGUI += OnProjectWindowItemGUI;
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

            // 自身のFolderIcon.jpgを確認
            string iconPath = Path.Combine(path, "FolderIcon.jpg").Replace("\\", "/");
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(iconPath);

            // 自身のFolderIcon.jpgがない場合、子フォルダのFolderIcon.jpgを確認
            if (texture == null)
            {
                var childFolders = Directory
                    .GetDirectories(path)
                    .Where(dir => File.Exists(Path.Combine(dir, "FolderIcon.jpg")))
                    .ToList();

                if (childFolders.Any())
                {
                    // 最初に見つかったFolderIcon.jpgを使用
                    string childIconPath = Path.Combine(childFolders[0], "FolderIcon.jpg")
                        .Replace("\\", "/");
                    texture = AssetDatabase.LoadAssetAtPath<Texture2D>(childIconPath);
                }
            }

            if (texture == null)
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

            GUI.DrawTexture(imageRect, texture);
        }
    }
}
