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
    /// <summary>
    /// アセットアイテムの表示を管理するビュー
    /// AvatarExplorerとKonoAssetのアイテムを統一的に表示する
    /// </summary>
    public class AssetItemView
    {
        /// <summary>AvatarExplorerデータベース</summary>
        private readonly AvatarExplorerDatabase? aeDatabase;

        /// <summary>メモのフォールドアウト状態</summary>
        private readonly Dictionary<string, bool> memoFoldouts = new();

        /// <summary>UnityPackageのフォールドアウト状態</summary>
        private readonly Dictionary<string, bool> unityPackageFoldouts = new();

        private readonly AssetItem assetItemHelper = new AssetItem();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="aeDatabase">AvatarExplorerデータベース</param>
        public AssetItemView(AvatarExplorerDatabase? aeDatabase)
        {
            this.aeDatabase = aeDatabase;
        }

        /// <summary>
        /// 完全な画像パスを取得
        /// </summary>
        /// <param name="imagePath">画像パス</param>
        /// <returns>完全な画像パス</returns>
        public string GetFullImagePath(string imagePath)
        {
            if (string.IsNullOrEmpty(imagePath))
            {
                Debug.LogWarning("[AssetItemView] 画像パスが未設定です");
                return string.Empty;
            }

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
        /// AEアバターアイテムの表示
        /// </summary>
        /// <param name="item">表示するアイテム</param>
        public void ShowAvatarItem(AvatarExplorerItem item)
        {
            GUILayout.BeginVertical(EditorStyles.helpBox);
            int boothItemId = assetItemHelper.GetBoothItemId(item);
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
                boothItemId
            );
            DrawUnityPackageSection(item.ItemPath, item.Title, item.ImagePath);
            GUILayout.EndVertical();
        }

        /// <summary>
        /// KAアバターアイテムの表示
        /// </summary>
        /// <param name="item">表示するアイテム</param>
        public void ShowKonoAssetItem(KonoAssetAvatarItem item)
        {
            GUILayout.BeginVertical(EditorStyles.helpBox);
            var itemPath = Path.GetFullPath(
                Path.Combine(DatabaseService.GetKADatabasePath(), "data", item.id)
            );
            DateTime? createdDate = null;
            if (item.description.createdAt > 0)
            {
                createdDate = DateTimeOffset
                    .FromUnixTimeMilliseconds(item.description.createdAt)
                    .DateTime;
            }
            int boothItemId = assetItemHelper.GetBoothItemId(item);
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
                boothItemId
            );
            DrawUnityPackageSection(
                itemPath,
                item.description.name,
                item.description.imageFilename
            );
            GUILayout.EndVertical();
        }

        /// <summary>
        /// KAウェアラブルアイテムの表示
        /// </summary>
        /// <param name="item">表示するアイテム</param>
        public void ShowKonoAssetWearableItem(KonoAssetWearableItem item)
        {
            GUILayout.BeginVertical(EditorStyles.helpBox);
            var itemPath = Path.GetFullPath(
                Path.Combine(DatabaseService.GetKADatabasePath(), "data", item.id)
            );
            DateTime? createdDate = null;
            if (item.description.createdAt > 0)
            {
                createdDate = DateTimeOffset
                    .FromUnixTimeMilliseconds(item.description.createdAt)
                    .DateTime;
            }
            int boothItemId = assetItemHelper.GetBoothItemId(item);
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
                boothItemId
            );
            DrawUnityPackageSection(
                itemPath,
                item.description.name,
                item.description.imageFilename
            );
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
            DateTime? createdDate = null;
            if (item.description.createdAt > 0)
            {
                createdDate = DateTimeOffset
                    .FromUnixTimeMilliseconds(item.description.createdAt)
                    .DateTime;
            }
            int boothItemId = assetItemHelper.GetBoothItemId(item);
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
                boothItemId
            );
            DrawUnityPackageSection(
                itemPath,
                item.description.name,
                item.description.imageFilename
            );
            GUILayout.EndVertical();
        }

        /// <summary>
        /// KAその他アセットアイテムの表示
        /// </summary>
        /// <param name="item">表示するアイテム</param>
        public void ShowKonoAssetOtherAssetItem(KonoAssetOtherAssetItem item)
        {
            GUILayout.BeginVertical(EditorStyles.helpBox);
            var itemPath = Path.GetFullPath(
                Path.Combine(DatabaseService.GetKADatabasePath(), "data", item.id)
            );
            DateTime? createdDate = null;
            if (item.description.createdAt > 0)
            {
                createdDate = DateTimeOffset
                    .FromUnixTimeMilliseconds(item.description.createdAt)
                    .DateTime;
            }
            int boothItemId = assetItemHelper.GetBoothItemId(item);
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
                boothItemId
            );
            DrawUnityPackageSection(
                itemPath,
                item.description.name,
                item.description.imageFilename
            );
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
        /// <param name="boothItemId">BoothアイテムID</param>
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
            int boothItemId = 0
        )
        {
            GUILayout.BeginHorizontal();
            DrawItemImage(imagePath);
            
            GUILayout.BeginVertical();
            DrawItemBasicInfo(title, author);
            DrawItemMetadata(title, category, supportedAvatars, tags, memo);
            DrawItemActionButtons(itemPath, boothItemId);
            GUILayout.EndVertical();
            
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// アイテムの基本情報（タイトル・作者）を描画
        /// </summary>
        private void DrawItemBasicInfo(string title, string author)
        {
            GUILayout.Label(title, EditorStyles.boldLabel);
            GUILayout.Label($"作者: {author}");
        }

        /// <summary>
        /// アイテムのメタデータ（カテゴリ・対応アバター・タグ・メモ）を描画
        /// </summary>
        private void DrawItemMetadata(string title, string? category, string[]? supportedAvatars, string[]? tags, string? memo)
        {
            // カテゴリ
            if (!string.IsNullOrEmpty(category))
            {
                DrawCategory(title, category);
            }

            // 対応アバター
            if (supportedAvatars != null && supportedAvatars.Length > 0)
            {
                DrawSupportedAvatars(supportedAvatars);
            }

            // タグ
            if (tags != null && tags.Length > 0)
            {
                GUILayout.Label($"タグ: {string.Join(", ", tags)}", EditorStyles.wordWrappedLabel);
            }

            // メモ
            if (!string.IsNullOrEmpty(memo))
            {
                DrawMemo(title, memo);
            }
        }

        /// <summary>
        /// アクションボタン（エクスプローラー・Booth）を描画
        /// </summary>
        private void DrawItemActionButtons(string itemPath, int boothItemId)
        {
            EditorGUILayout.Space(5);
            DrawExplorerOpenButton(itemPath);
            
            if (boothItemId > 0)
            {
                DrawBoothOpenButton(boothItemId);
            }
        }

        /// <summary>
        /// Booth商品ページを開くボタンを描画
        /// </summary>
        private void DrawBoothOpenButton(int boothItemId)
        {
            if (GUILayout.Button("商品ページを開く", GUILayout.Width(150)))
            {
                Application.OpenURL($"https://booth.pm/ja/items/{boothItemId}");
            }
        }

        /// <summary>
        /// カテゴリの描画
        /// </summary>
        /// <param name="title">タイトル</param>
        /// <param name="category">カテゴリ</param>
        private void DrawCategory(string title, string? category)
        {
            if (aeDatabase != null)
            {
                var item = aeDatabase.Items.FirstOrDefault(i => i.Title == title);
                GUILayout.Label(
                    item != null ? $"カテゴリ: {item.GetAECategoryName()}" : $"カテゴリ: {category}"
                );
            }
            else
            {
                GUILayout.Label($"カテゴリ: {category}");
            }
        }

        /// <summary>
        /// 対応アバターの描画
        /// </summary>
        /// <param name="supportedAvatars">対応アバターのパス配列</param>
        private void DrawSupportedAvatars(string[] supportedAvatars)
        {
            string supportedAvatarsText =
                aeDatabase != null
                    ? GetAESupportedAvatarsText(supportedAvatars)
                    : $"対応アバター: {string.Join(", ", supportedAvatars)}";

            GUILayout.Label(supportedAvatarsText, EditorStyles.wordWrappedLabel);
        }

        /// <summary>
        /// AEの対応アバターのテキストを取得
        /// </summary>
        /// <param name="supportedAvatars">対応アバターのパス配列</param>
        /// <returns>対応アバターの表示テキスト</returns>
        private string GetAESupportedAvatarsText(string[] supportedAvatars)
        {
            var supportedAvatarNames = supportedAvatars.Select(avatarPath =>
            {
                var avatarItem = aeDatabase?.Items.FirstOrDefault(x => x.ItemPath == avatarPath);
                return avatarItem?.Title ?? Path.GetFileName(avatarPath);
            });

            return "対応アバター: " + string.Join(", ", supportedAvatarNames);
        }

        /// <summary>
        /// メモの描画
        /// </summary>
        /// <param name="title">タイトル</param>
        /// <param name="memo">メモ</param>
        private void DrawMemo(string title, string? memo)
        {
            string memoKey = $"{title}_memo";
            if (!memoFoldouts.ContainsKey(memoKey))
            {
                memoFoldouts[memoKey] = false;
            }

            var startRect = EditorGUILayout.GetControlRect(false, 0);
            var startY = startRect.y;
            var boxRect = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight);

            if (
                Event.current.type == EventType.MouseDown
                && boxRect.Contains(Event.current.mousePosition)
            )
            {
                memoFoldouts[memoKey] = !memoFoldouts[memoKey];
                GUI.changed = true;
                Event.current.Use();
            }

            string toggleText = memoFoldouts[memoKey] ? "▼メモ" : "▶メモ";
            EditorGUI.LabelField(boxRect, toggleText);

            if (memoFoldouts[memoKey])
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.LabelField(memo ?? string.Empty, EditorStyles.wordWrappedLabel);
                EditorGUI.indentLevel--;
            }

            var endRect = GUILayoutUtility.GetLastRect();
            var endY = endRect.y + endRect.height;
            var frameRect = new Rect(
                startRect.x,
                startY,
                EditorGUIUtility.currentViewWidth - 20,
                endY - startY + 10
            );
            EditorGUI.DrawRect(frameRect, new Color(0.5f, 0.5f, 0.5f, 0.2f));
            GUI.Box(frameRect, "", EditorStyles.helpBox);
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
        /// "Explorerで開く"ボタンの描画
        /// </summary>
        /// <param name="itemPath">アイテムパス</param>
        private void DrawExplorerOpenButton(string itemPath)
        {
            string fullPath = itemPath;
            if (itemPath.StartsWith("Datas\\"))
            {
                string normalizedItemPath = itemPath.Replace(
                    "\\",
                    Path.DirectorySeparatorChar.ToString()
                );
                string normalizedAePath = DatabaseService
                    .GetAEDatabasePath()
                    .Replace("/", Path.DirectorySeparatorChar.ToString());
                string itemName = Path.GetFileName(normalizedItemPath);
                fullPath = Path.Combine(normalizedAePath, "Items", itemName);
            }
            else
            {
                fullPath = Path.Combine(DatabaseService.GetKADatabasePath(), "data", itemPath);
            }

            if (Directory.Exists(fullPath))
            {
                if (GUILayout.Button("Explorerで開く", GUILayout.Width(150)))
                {
                    System.Diagnostics.Process.Start("explorer.exe", fullPath);
                }
            }
        }

        /// <summary>
        /// UnityPackageアイテムの描画
        /// </summary>
        /// <param name="package">パッケージパス</param>
        /// <param name="imagePath">サムネイル画像パス</param>
        private void DrawUnityPackageItem(string package, string imagePath)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(Path.GetFileName(package));
            if (GUILayout.Button("インポート", GUILayout.Width(100)))
            {
                // フォルダサムネイル生成設定を取得
                bool generateFolderThumbnail = EditorPrefs.GetBool(
                    "UnityEditorAssetBrowser_GenerateFolderThumbnail",
                    true
                );
                if (generateFolderThumbnail)
                {
                    // サムネイルも生成する
                    UnityPackageServices.ImportPackageAndSetThumbnails(package, imagePath);
                }
                else
                {
                    // 通常のUnityパッケージインポートのみ
                    AssetDatabase.ImportPackage(package, true);
                }
            }
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// UnityPackageセクションの描画
        /// </summary>
        /// <param name="itemPath">アイテムパス</param>
        /// <param name="itemName">アイテム名</param>
        /// <param name="imagePath">サムネイル画像パス</param>
        private void DrawUnityPackageSection(string itemPath, string itemName, string imagePath)
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
                    for (int i = 0; i < unityPackages.Count(); i++)
                    {
                        DrawUnityPackageItem(unityPackages.ElementAt(i), imagePath);

                        // 最後のアイテム以外の後に線を描画
                        if (i < unityPackages.Count() - 1)
                        {
                            var lineRect = EditorGUILayout.GetControlRect(false, 1);
                            // 色を循環させる（赤、青、緑、黄、紫、水色）
                            Color[] colors = new Color[]
                            {
                                new Color(1f, 0f, 0f, 0.5f), // 赤
                                new Color(0f, 0f, 1f, 0.5f), // 青
                                new Color(0f, 1f, 0f, 0.5f), // 緑
                                new Color(1f, 1f, 0f, 0.5f), // 黄
                                new Color(1f, 0f, 1f, 0.5f), // 紫
                                new Color(0f, 1f, 1f, 0.5f), // 水色
                            };
                            Color lineColor = colors[i % colors.Length];
                            EditorGUI.DrawRect(lineRect, lineColor);
                        }
                    }
                    EditorGUI.indentLevel--;
                }

                // 次のアイテムとの間に余白を追加
                EditorGUILayout.Space(5);
            }
            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// アイテムヘッダー情報を保持するクラス
        /// </summary>
        private class ItemHeaderInfo
        {
            public string Title { get; set; } = string.Empty;
            public string Author { get; set; } = string.Empty;
            public string ImagePath { get; set; } = string.Empty;
            public string ItemPath { get; set; } = string.Empty;
            public DateTime? CreatedDate { get; set; }
            public string? Category { get; set; }
            public string[]? SupportedAvatars { get; set; }
            public string[]? Tags { get; set; }
            public string? Memo { get; set; }
            public bool ShowCategory { get; set; } = true;
            public bool ShowSupportedAvatars { get; set; }
        }
    }
}
