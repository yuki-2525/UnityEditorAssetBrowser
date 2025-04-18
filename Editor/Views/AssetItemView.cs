#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditorAssetBrowser.Helper;
using UnityEditorAssetBrowser.Models;
using UnityEditorAssetBrowser.Services;
using UnityEngine;

namespace UnityEditorAssetBrowser.Views
{
    public class AssetItemView
    {
        private readonly AvatarExplorerDatabase? aeDatabase;
        private Dictionary<string, bool> memoFoldouts = new Dictionary<string, bool>();
        private Dictionary<string, bool> unityPackageFoldouts = new Dictionary<string, bool>();

        public AssetItemView(AvatarExplorerDatabase? aeDatabase)
        {
            this.aeDatabase = aeDatabase;
        }

        /// <summary>
        /// AEアバターアイテムの表示
        /// </summary>
        /// <param name="item">表示するアイテム</param>
        /// <param name="showCategory">カテゴリを表示するかどうか</param>
        /// <param name="showSupportedAvatars">対応アバターを表示するかどうか</param>
        public void ShowAvatarItem(
            AvatarExplorerItem item,
            bool showCategory,
            bool showSupportedAvatars = false
        )
        {
            GUILayout.BeginVertical(EditorStyles.helpBox);
            DrawItemHeader(
                item.Title,
                item.AuthorName,
                item.ImagePath,
                item.ItemPath,
                item.CreatedDate,
                item.Category,
                item.SupportedAvatars,
                item.Tags,
                item.Memo,
                showCategory,
                showSupportedAvatars
            );
            DrawUnityPackageSection(item.ItemPath, item.Title);
            GUILayout.EndVertical();
        }

        /// <summary>
        /// KAアバターアイテムの表示
        /// </summary>
        /// <param name="item">表示するアイテム</param>
        /// <param name="showCategory">カテゴリを表示するかどうか</param>
        /// <param name="showSupportedAvatars">対応アバターを表示するかどうか</param>
        public void ShowKonoAssetItem(
            KonoAssetAvatarItem item,
            bool showCategory,
            bool showSupportedAvatars = false
        )
        {
            GUILayout.BeginVertical(EditorStyles.helpBox);
            var itemPath = Path.GetFullPath(
                Path.Combine(DatabaseService.GetKADatabasePath(), "data", item.id)
            );

            // Unix時間をDateTimeに変換
            DateTime? createdDate = null;

            if (item.description.createdAt > 0)
            {
                createdDate = DateTimeOffset
                    .FromUnixTimeMilliseconds(item.description.createdAt)
                    .DateTime;
            }

            DrawItemHeader(
                item.description.name,
                item.description.creator,
                item.description.imageFilename,
                itemPath,
                createdDate,
                null,
                null,
                item.description.tags,
                item.description.memo,
                showCategory,
                showSupportedAvatars
            );
            DrawUnityPackageSection(itemPath, item.description.name);
            GUILayout.EndVertical();
        }

        /// <summary>
        /// KAウェアラブルアイテムの表示
        /// </summary>
        /// <param name="item">表示するアイテム</param>
        /// <param name="showSupportedAvatars">対応アバターを表示するかどうか</param>
        public void ShowKonoAssetWearableItem(
            KonoAssetWearableItem item,
            bool showSupportedAvatars = false
        )
        {
            GUILayout.BeginVertical(EditorStyles.helpBox);
            var itemPath = Path.GetFullPath(
                Path.Combine(DatabaseService.GetKADatabasePath(), "data", item.id)
            );

            // Unix時間をDateTimeに変換
            DateTime? createdDate = null;

            if (item.description.createdAt > 0)
            {
                createdDate = DateTimeOffset
                    .FromUnixTimeMilliseconds(item.description.createdAt)
                    .DateTime;
            }

            DrawItemHeader(
                item.description.name,
                item.description.creator,
                item.description.imageFilename,
                itemPath,
                createdDate,
                item.category,
                item.supportedAvatars,
                item.description.tags,
                item.description.memo,
                true,
                showSupportedAvatars
            );
            DrawUnityPackageSection(itemPath, item.description.name);
            GUILayout.EndVertical();
        }

        /// <summary>
        /// KAワールドオブジェクトアイテムの表示
        /// </summary>
        /// <param name="item">表示するアイテム</param>
        public void ShowKonoAssetWorldObjectItem(KonoAssetWorldObjectItem item)
        {
            GUILayout.BeginVertical(EditorStyles.helpBox);
            var itemPath = Path.GetFullPath(
                Path.Combine(DatabaseService.GetKADatabasePath(), "data", item.id)
            );

            // Unix時間をDateTimeに変換
            DateTime? createdDate = null;

            if (item.description.createdAt > 0)
            {
                createdDate = DateTimeOffset
                    .FromUnixTimeMilliseconds(item.description.createdAt)
                    .DateTime;
            }

            DrawItemHeader(
                item.description.name,
                item.description.creator,
                item.description.imageFilename,
                itemPath,
                createdDate,
                item.category,
                null,
                item.description.tags,
                item.description.memo,
                true,
                false
            );
            DrawUnityPackageSection(itemPath, item.description.name);
            GUILayout.EndVertical();
        }

        /// <summary>
        /// アイテムヘッダーの描画
        /// </summary>
        /// <param name="title">タイトル</param>
        /// <param name="author">作者名</param>
        /// <param name="imagePath">画像パス</param>
        /// <param name="itemPath">アイテムパス</param>
        /// <param name="createdDate">作成日（ソート用）</param>
        /// <param name="category">カテゴリ</param>
        /// <param name="supportedAvatars">対応アバター</param>
        /// <param name="tags">タグ</param>
        /// <param name="memo">メモ</param>
        /// <param name="showCategory">カテゴリを表示するかどうか</param>
        /// <param name="showSupportedAvatars">対応アバターを表示するかどうか</param>
        private void DrawItemHeader(
            string title,
            string author,
            string imagePath,
            string itemPath,
            DateTime? createdDate = null,
            string? category = null,
            string[]? supportedAvatars = null,
            string[]? tags = null,
            string? memo = null,
            bool showCategory = true,
            bool showSupportedAvatars = false
        )
        {
            GUILayout.BeginHorizontal();
            DrawItemImage(imagePath);
            GUILayout.BeginVertical();

            // タイトル
            GUILayout.Label(title, EditorStyles.boldLabel);

            // 作者名
            GUILayout.Label($"作者: {author}");

            // カテゴリ（showCategoryがtrueの場合のみ表示）
            if (showCategory && !string.IsNullOrEmpty(category))
            {
                if (aeDatabase != null)
                {
                    var item = aeDatabase.Items.FirstOrDefault(i => i.Title == title);
                    if (item != null)
                    {
                        GUILayout.Label($"カテゴリ: {item.GetAECategoryName()}");
                    }
                    else
                    {
                        GUILayout.Label($"カテゴリ: {category}");
                    }
                }
                else
                {
                    GUILayout.Label($"カテゴリ: {category}");
                }
            }

            // 対応アバター（showSupportedAvatarsがtrueの場合のみ表示）
            if (showSupportedAvatars && supportedAvatars != null && supportedAvatars.Length > 0)
            {
                string supportedAvatarsText;

                // AEのアイテムの場合、パスからアバター名を取得
                if (aeDatabase != null)
                {
                    var supportedAvatarNames = new List<string>();
                    foreach (var avatarPath in supportedAvatars)
                    {
                        var avatarItem = aeDatabase.Items.FirstOrDefault(x =>
                            x.ItemPath == avatarPath
                        );
                        if (avatarItem != null)
                        {
                            supportedAvatarNames.Add(avatarItem.Title);
                        }
                        else
                        {
                            // パスが見つからない場合はパスをそのまま表示
                            supportedAvatarNames.Add(Path.GetFileName(avatarPath));
                        }
                    }
                    supportedAvatarsText =
                        "対応アバター: " + string.Join(", ", supportedAvatarNames);
                }
                else
                {
                    // KAのアイテムの場合はそのまま表示
                    supportedAvatarsText = "対応アバター: " + string.Join(", ", supportedAvatars);
                }

                GUILayout.Label(supportedAvatarsText);
            }

            // タグ（KAのみ）
            if (tags != null && tags.Length > 0)
            {
                string tagsText = "タグ: " + string.Join(", ", tags);
                GUILayout.Label(tagsText);
            }

            // メモ（トグルで表示）
            if (!string.IsNullOrEmpty(memo))
            {
                // メモのフォールドアウト状態を管理するためのキー
                string memoKey = $"{title}_memo";
                if (!memoFoldouts.ContainsKey(memoKey))
                {
                    memoFoldouts[memoKey] = false;
                }

                // 枠の開始位置を記録
                var startRect = EditorGUILayout.GetControlRect(false, 0);
                var startY = startRect.y;

                // 行全体をクリック可能にするためのボックスを作成
                var boxRect = EditorGUILayout.GetControlRect(
                    false,
                    EditorGUIUtility.singleLineHeight
                );

                // フォールドアウトの状態を更新
                if (
                    Event.current.type == EventType.MouseDown
                    && boxRect.Contains(Event.current.mousePosition)
                )
                {
                    memoFoldouts[memoKey] = !memoFoldouts[memoKey];
                    GUI.changed = true;
                    Event.current.Use();
                }

                // ラベルを描画（▼を追加）
                string toggleText = memoFoldouts[memoKey] ? "▼メモ" : "▶メモ";
                EditorGUI.LabelField(boxRect, toggleText);

                if (memoFoldouts[memoKey])
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.LabelField(memo, EditorStyles.wordWrappedLabel);
                    EditorGUI.indentLevel--;
                }

                // 枠の終了位置を取得
                var endRect = GUILayoutUtility.GetLastRect();
                var endY = endRect.y + endRect.height;

                // 枠を描画
                var frameRect = new Rect(
                    startRect.x,
                    startY,
                    EditorGUIUtility.currentViewWidth - 20,
                    endY - startY + 10
                );
                EditorGUI.DrawRect(frameRect, new Color(0.5f, 0.5f, 0.5f, 0.2f));
                GUI.Box(frameRect, "", EditorStyles.helpBox);
            }

            // 開くボタンとアイテムデータとの間に一行間を開ける
            EditorGUILayout.Space(5);
            DrawOpenButton(itemPath);
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// アイテム画像の描画
        /// </summary>
        /// <param name="imagePath">画像パス</param>
        private void DrawItemImage(string imagePath)
        {
            if (string.IsNullOrEmpty(imagePath))
                return;

            string fullImagePath = GetFullImagePath(imagePath);
            if (File.Exists(fullImagePath))
            {
                var texture = ImageServices.Instance.LoadTexture(fullImagePath);
                if (texture != null)
                {
                    GUILayout.Label(texture, GUILayout.Width(100), GUILayout.Height(100));
                }
            }
        }

        /// <summary>
        /// 完全な画像パスを取得
        /// </summary>
        /// <param name="imagePath">画像パス</param>
        /// <returns>完全な画像パス</returns>
        private string GetFullImagePath(string imagePath)
        {
            if (imagePath.StartsWith("Datas"))
            {
                return Path.Combine(
                    DatabaseService.GetAEDatabasePath(),
                    imagePath.Replace("Datas\\", "")
                );
            }
            return Path.Combine(DatabaseService.GetKADatabasePath(), "images", imagePath);
        }

        /// <summary>
        /// 開くボタンの描画
        /// </summary>
        /// <param name="itemPath">アイテムパス</param>
        private void DrawOpenButton(string itemPath)
        {
            // 相対パスの場合はAEDatabasePathと結合
            string fullPath = itemPath;
            if (itemPath.StartsWith("Datas\\"))
            {
                // パスの区切り文字を正規化
                string normalizedItemPath = itemPath.Replace(
                    "\\",
                    Path.DirectorySeparatorChar.ToString()
                );
                string normalizedAePath = DatabaseService
                    .GetAEDatabasePath()
                    .Replace("/", Path.DirectorySeparatorChar.ToString());

                // Datas\Items\アイテム名 の形式の場合、AEDatabasePath\Items\アイテム名 に変換
                string itemName = Path.GetFileName(normalizedItemPath);
                fullPath = Path.Combine(normalizedAePath, "Items", itemName);
            }
            else
            {
                // KAのアイテムの場合、kaDatabasePathと結合
                fullPath = Path.Combine(DatabaseService.GetKADatabasePath(), "data", itemPath);
            }

            if (Directory.Exists(fullPath))
            {
                if (GUILayout.Button("開く", GUILayout.Width(150)))
                {
                    System.Diagnostics.Process.Start("explorer.exe", fullPath);
                }
            }
            else
            {
                Debug.LogWarning($"ディレクトリが存在しません: {fullPath}");
            }
        }

        /// <summary>
        /// UnityPackageセクションの描画
        /// </summary>
        /// <param name="itemPath">アイテムパス</param>
        /// <param name="itemName">アイテム名</param>
        private void DrawUnityPackageSection(string itemPath, string itemName)
        {
            // 相対パスの場合はAEDatabasePathと結合
            string fullPath = itemPath;
            if (itemPath.StartsWith("Datas\\"))
            {
                // パスの区切り文字を正規化
                string normalizedItemPath = itemPath.Replace(
                    "\\",
                    Path.DirectorySeparatorChar.ToString()
                );
                string normalizedAePath = DatabaseService
                    .GetAEDatabasePath()
                    .Replace("/", Path.DirectorySeparatorChar.ToString());

                // Datas\Items\アイテム名 の形式の場合、AEDatabasePath\Items\アイテム名 に変換
                string fileName = Path.GetFileName(normalizedItemPath);
                fullPath = Path.Combine(normalizedAePath, "Items", fileName);
            }

            var unityPackages = UnityPackageServices.FindUnityPackages(fullPath);
            if (!unityPackages.Any())
            {
                return;
            }

            // フォールドアウトの状態を初期化（キーが存在しない場合）
            if (!unityPackageFoldouts.ContainsKey(itemName))
            {
                unityPackageFoldouts[itemName] = false;
            }

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                // 行全体をクリック可能にするためのボックスを作成
                var boxRect = EditorGUILayout.GetControlRect(
                    false,
                    EditorGUIUtility.singleLineHeight
                );
                var foldoutRect = new Rect(
                    boxRect.x,
                    boxRect.y,
                    EditorGUIUtility.singleLineHeight,
                    boxRect.height
                );
                var labelRect = new Rect(
                    boxRect.x + EditorGUIUtility.singleLineHeight,
                    boxRect.y,
                    boxRect.width - EditorGUIUtility.singleLineHeight,
                    boxRect.height
                );

                // フォールドアウトの状態を更新
                if (
                    Event.current.type == EventType.MouseDown
                    && boxRect.Contains(Event.current.mousePosition)
                )
                {
                    unityPackageFoldouts[itemName] = !unityPackageFoldouts[itemName];
                    GUI.changed = true;
                    Event.current.Use();
                }

                // フォールドアウトとラベルを描画
                unityPackageFoldouts[itemName] = EditorGUI.Foldout(
                    foldoutRect,
                    unityPackageFoldouts[itemName],
                    ""
                );
                EditorGUI.LabelField(labelRect, "UnityPackage");

                if (unityPackageFoldouts[itemName])
                {
                    EditorGUI.indentLevel++;
                    foreach (var package in unityPackages)
                    {
                        DrawUnityPackageItem(package);
                    }
                    EditorGUI.indentLevel--;
                }

                // 次のアイテムとの間に余白を追加
                EditorGUILayout.Space(5);
            }
            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// UnityPackageアイテムの描画
        /// </summary>
        /// <param name="package">パッケージパス</param>
        private void DrawUnityPackageItem(string package)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(Path.GetFileName(package));
            if (GUILayout.Button("インポート", GUILayout.Width(100)))
            {
                AssetDatabase.ImportPackage(package, true);
            }
            GUILayout.EndHorizontal();
        }
    }
}
