// Copyright (c) 2025 yuki-2525
// This code is borrowed from AssetLibraryManager (https://github.com/MAIOTAchannel/AssetLibraryManager)
// Used with permission from MAIOTAchannel

#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEditor;
using UnityEditorAssetBrowser.Helper;
using UnityEditorAssetBrowser.Models;
using UnityEngine;

namespace UnityEditorAssetBrowser
{
    /// <summary>
    /// Unity Editor用のアセットブラウザーウィンドウ
    /// AvatarExplorerとKonoAssetのデータベースを統合して表示・管理する
    /// </summary>
    public class UnityEditorAssetBrowser : EditorWindow
    {
        #region Constants
        /// <summary>1ページあたりのアイテム表示数</summary>
        private const int ITEMS_PER_PAGE = 10;

        /// <summary>ウィンドウのタイトル</summary>
        private const string WINDOW_TITLE = "Unity Editor Asset Browser";

        /// <summary>AEデータベースパスのEditorPrefsキー</summary>
        private const string AE_DATABASE_PATH_KEY = "UnityEditorAssetBrowser_AEDatabasePath";

        /// <summary>KAデータベースパスのEditorPrefsキー</summary>
        private const string KA_DATABASE_PATH_KEY = "UnityEditorAssetBrowser_KADatabasePath";

        /// <summary>ワールドカテゴリーの日本語キーワード</summary>
        private const string WORLD_CATEGORY_JP = "ワールド";

        /// <summary>ワールドカテゴリーの英語キーワード</summary>
        private const string WORLD_CATEGORY_EN = "world";
        #endregion

        #region Database Fields
        /// <summary>AvatarExplorerのデータベース</summary>
        private AvatarExplorerDatabase? aeDatabase;

        /// <summary>KonoAssetのアバターデータベース</summary>
        private KonoAssetAvatarsDatabase? kaAvatarsDatabase;

        /// <summary>KonoAssetのウェアラブルデータベース</summary>
        private KonoAssetWearablesDatabase? kaWearablesDatabase;

        /// <summary>KonoAssetのワールドオブジェクトデータベース</summary>
        private KonoAssetWorldObjectsDatabase? kaWorldObjectsDatabase;
        #endregion

        #region UI Fields
        /// <summary>スクロールビューの位置</summary>
        private Vector2 scrollPosition;

        /// <summary>AEデータベースのパス</summary>
        private string aeDatabasePath = "";

        /// <summary>KAデータベースのパス</summary>
        private string kaDatabasePath = "";

        /// <summary>検索クエリ</summary>
        private string searchQuery = "";

        /// <summary>選択中のタブインデックス</summary>
        private int selectedTab = 0;

        /// <summary>タブのラベル</summary>
        private string[] tabs = { "アバター", "アバター関連", "ワールド" };

        /// <summary>フォールドアウト状態の管理</summary>
        private Dictionary<string, bool> foldouts = new Dictionary<string, bool>();

        /// <summary>画像のキャッシュ</summary>
        private Dictionary<string, Texture2D> imageCache = new Dictionary<string, Texture2D>();

        /// <summary>タイトル用のスタイル</summary>
        private GUIStyle? titleStyle;

        /// <summary>ボックス用のスタイル</summary>
        private GUIStyle? boxStyle;

        /// <summary>現在のページ番号</summary>
        private int currentPage = 0;

        /// <summary>UnityPackageのフォールドアウト状態の管理</summary>
        private Dictionary<string, bool> unityPackageFoldouts = new Dictionary<string, bool>();
        #endregion

        #region Unity Editor Window Methods
        /// <summary>
        /// メニューからウィンドウを表示する
        /// </summary>
        [MenuItem("Window/Unity Editor Asset Browser")]
        public static void ShowWindow()
        {
            GetWindow<UnityEditorAssetBrowser>(WINDOW_TITLE);
        }

        /// <summary>
        /// ウィンドウが有効になった時の処理
        /// </summary>
        private void OnEnable()
        {
            LoadSettings();

            // シーン変更時に自動的に更新を実行
            EditorApplication.hierarchyChanged += OnHierarchyChanged;
        }

        /// <summary>
        /// ウィンドウが無効になった時の処理
        /// </summary>
        private void OnDisable()
        {
            // イベントの解除
            EditorApplication.hierarchyChanged -= OnHierarchyChanged;
        }

        /// <summary>
        /// シーン階層が変更された時の処理
        /// </summary>
        private void OnHierarchyChanged()
        {
            // 画像キャッシュを更新
            RefreshImageCache();
        }

        /// <summary>
        /// GUIの描画処理
        /// </summary>
        private void OnGUI()
        {
            InitializeStyles();
            DrawMainWindow();
        }
        #endregion

        #region UI Drawing Methods
        /// <summary>
        /// メインウィンドウの描画
        /// </summary>
        private void DrawMainWindow()
        {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.Space(10);

            DrawDatabasePathFields();
            DrawTabBar();
            DrawSearchField();
            DrawSearchResultCount();
            DrawContentArea();

            if (GUI.changed)
            {
                SaveSettings();
            }

            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// データベースパス入力フィールドの描画
        /// </summary>
        private void DrawDatabasePathFields()
        {
            DrawDatabasePathField("AE Database Path:", ref aeDatabasePath, LoadAEDatabase);
            DrawDatabasePathField("KA Database Path:", ref kaDatabasePath, LoadKADatabase);

            // リフレッシュボタンを追加（一つにまとめる）
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("更新", GUILayout.Width(150)))
            {
                RefreshDatabases();
                RefreshImageCache();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);
        }

        /// <summary>
        /// データベースパス入力フィールドの描画
        /// </summary>
        /// <param name="label">ラベル</param>
        /// <param name="path">パス</param>
        /// <param name="onPathChanged">パス変更時のコールバック</param>
        private void DrawDatabasePathField(string label, ref string path, Action onPathChanged)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(label, GUILayout.Width(120));

            // パスを編集不可のテキストフィールドとして表示
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.TextField(path);
            EditorGUI.EndDisabledGroup();

            // 削除ボタンを追加（Browseボタンの左に配置）
            if (!string.IsNullOrEmpty(path) && GUILayout.Button("削除", GUILayout.Width(60)))
            {
                path = "";
                onPathChanged();

                // データベースオブジェクトも削除
                if (label == "AE Database Path:")
                {
                    aeDatabase = null;
                }
                else if (label == "KA Database Path:")
                {
                    kaAvatarsDatabase = null;
                    kaWearablesDatabase = null;
                    kaWorldObjectsDatabase = null;
                }

                // ページをリセット
                currentPage = 0;
            }

            if (GUILayout.Button("Browse", GUILayout.Width(60)))
            {
                var selectedPath = EditorUtility.OpenFolderPanel(
                    $"Select {label} Directory",
                    "",
                    ""
                );
                if (!string.IsNullOrEmpty(selectedPath))
                {
                    path = selectedPath;
                    onPathChanged();
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// タブバーの描画
        /// </summary>
        private void DrawTabBar()
        {
            var newTab = GUILayout.Toolbar(selectedTab, tabs);
            if (newTab != selectedTab)
            {
                selectedTab = newTab;
                currentPage = 0;
            }
            EditorGUILayout.Space(10);
        }

        /// <summary>
        /// 検索フィールドの描画
        /// </summary>
        private void DrawSearchField()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Search:", GUILayout.Width(60));
            var newSearchQuery = EditorGUILayout.TextField(searchQuery);
            if (newSearchQuery != searchQuery)
            {
                searchQuery = newSearchQuery;
                currentPage = 0;
            }
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// 検索結果件数の描画
        /// </summary>
        private void DrawSearchResultCount()
        {
            int totalItems = GetCurrentTabItemCount();
            EditorGUILayout.LabelField($"検索結果: {totalItems}件");
            EditorGUILayout.Space(10);
        }

        /// <summary>
        /// コンテンツエリアの描画
        /// </summary>
        private void DrawContentArea()
        {
            GUILayout.BeginVertical();
            DrawScrollView();
            DrawPaginationButtons();
            GUILayout.EndVertical();
        }

        /// <summary>
        /// スクロールビューの描画
        /// </summary>
        private void DrawScrollView()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(
                scrollPosition,
                GUILayout.ExpandHeight(true)
            );
            DrawCurrentTabContent();
            EditorGUILayout.EndScrollView();
        }

        /// <summary>
        /// 現在のタブのコンテンツを描画
        /// </summary>
        private void DrawCurrentTabContent()
        {
            switch (selectedTab)
            {
                case 0:
                    ShowAvatarsContent();
                    break;
                case 1:
                    ShowItemsContent();
                    break;
                case 2:
                    ShowWorldObjectsContent();
                    break;
            }
        }

        /// <summary>
        /// ページネーションボタンの描画
        /// </summary>
        private void DrawPaginationButtons()
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("前へ", GUILayout.Width(100)) && currentPage > 0)
            {
                currentPage--;
            }
            int totalPages = GetTotalPages();
            GUILayout.Label($"ページ {currentPage + 1} / {totalPages}");
            if (GUILayout.Button("次へ", GUILayout.Width(100)) && currentPage < totalPages - 1)
            {
                currentPage++;
            }
            GUILayout.EndHorizontal();
        }
        #endregion

        #region Helper Methods
        /// <summary>
        /// 現在のタブのアイテム数を取得
        /// </summary>
        /// <returns>アイテム数</returns>
        private int GetCurrentTabItemCount()
        {
            switch (selectedTab)
            {
                case 0:
                    return GetFilteredAvatars().Count;
                case 1:
                    return GetFilteredItems().Count;
                case 2:
                    return GetFilteredWorldObjects().Count;
                default:
                    return 0;
            }
        }

        /// <summary>
        /// カテゴリーがワールド関連かどうかを判定
        /// </summary>
        /// <param name="category">カテゴリー名</param>
        /// <returns>ワールド関連の場合true</returns>
        private bool IsWorldCategory(string category)
        {
            return category.Contains(WORLD_CATEGORY_JP, StringComparison.OrdinalIgnoreCase)
                || category.Contains(WORLD_CATEGORY_EN, StringComparison.OrdinalIgnoreCase);
        }
        #endregion

        /// <summary>
        /// GUIスタイルの初期化
        /// </summary>
        private void InitializeStyles()
        {
            if (titleStyle == null)
            {
                titleStyle = new GUIStyle(EditorStyles.boldLabel)
                {
                    fontSize = 14,
                    margin = new RectOffset(4, 4, 4, 4),
                };
            }

            if (boxStyle == null)
            {
                boxStyle = new GUIStyle(EditorStyles.helpBox)
                {
                    padding = new RectOffset(10, 10, 10, 10),
                    margin = new RectOffset(0, 0, 5, 5),
                };
            }
        }

        /// <summary>
        /// アバターコンテンツの表示
        /// </summary>
        private void ShowAvatarsContent()
        {
            var filteredItems = GetFilteredAvatars();

            // ページネーション
            int startIndex = currentPage * ITEMS_PER_PAGE;
            int endIndex = Mathf.Min(startIndex + ITEMS_PER_PAGE, filteredItems.Count);
            var pageItems = filteredItems.Skip(startIndex).Take(ITEMS_PER_PAGE);

            foreach (var item in pageItems)
            {
                if (item is AvatarExplorerItem aeItem)
                {
                    ShowAvatarItem(aeItem);
                }
                else if (item is KonoAssetAvatarItem kaItem)
                {
                    ShowKonoAssetItem(kaItem);
                }
            }
        }

        /// <summary>
        /// アバター関連アイテムコンテンツの表示
        /// </summary>
        private void ShowItemsContent()
        {
            var allItems = new List<object>();

            // AEのアイテム（type≠0）を追加
            if (aeDatabase?.Items != null)
            {
                allItems.AddRange(aeDatabase.Items.Where(item => item.Type != "0"));
            }

            // KAのウェアラブルを追加
            if (kaWearablesDatabase?.data != null)
            {
                allItems.AddRange(kaWearablesDatabase.data);
            }

            // 検索フィルター適用
            var filteredItems = allItems
                .Where(item =>
                {
                    if (string.IsNullOrEmpty(searchQuery))
                        return true;

                    if (item is AvatarExplorerItem aeItem)
                        return aeItem.Title.Contains(
                            searchQuery,
                            StringComparison.OrdinalIgnoreCase
                        );

                    if (item is KonoAssetWearableItem kaItem)
                        return kaItem.description.name.Contains(
                            searchQuery,
                            StringComparison.OrdinalIgnoreCase
                        );

                    return false;
                })
                .ToList();

            // ページネーション
            int startIndex = currentPage * ITEMS_PER_PAGE;
            int endIndex = Mathf.Min(startIndex + ITEMS_PER_PAGE, filteredItems.Count);
            var pageItems = filteredItems.Skip(startIndex).Take(ITEMS_PER_PAGE);

            foreach (var item in pageItems)
            {
                if (item is AvatarExplorerItem aeItem)
                {
                    ShowAvatarItem(aeItem);
                }
                else if (item is KonoAssetWearableItem kaItem)
                {
                    ShowKonoAssetWearableItem(kaItem);
                }
            }
        }

        /// <summary>
        /// ワールドオブジェクトコンテンツの表示
        /// </summary>
        private void ShowWorldObjectsContent()
        {
            var filteredItems = GetFilteredWorldObjects();

            // ページネーション
            int startIndex = currentPage * ITEMS_PER_PAGE;
            int endIndex = Mathf.Min(startIndex + ITEMS_PER_PAGE, filteredItems.Count);
            var pageItems = filteredItems.Skip(startIndex).Take(ITEMS_PER_PAGE);

            foreach (var item in pageItems)
            {
                if (item is AvatarExplorerItem aeItem)
                {
                    ShowAvatarItem(aeItem);
                }
                else if (item is KonoAssetWorldObjectItem worldItem)
                {
                    ShowKonoAssetWorldObjectItem(worldItem);
                }
            }
        }

        /// <summary>
        /// 総ページ数を取得
        /// </summary>
        /// <returns>総ページ数</returns>
        private int GetTotalPages()
        {
            int totalItems = 0;
            switch (selectedTab)
            {
                case 0:
                    totalItems = GetFilteredAvatars().Count;
                    break;
                case 1:
                    totalItems = GetFilteredItems().Count;
                    break;
                case 2:
                    totalItems = GetFilteredWorldObjects().Count;
                    break;
            }
            return Mathf.Max(1, Mathf.CeilToInt((float)totalItems / ITEMS_PER_PAGE));
        }

        /// <summary>
        /// フィルターされたアバターリストを取得
        /// </summary>
        /// <returns>フィルターされたアバターリスト</returns>
        private List<object> GetFilteredAvatars()
        {
            var items = new List<object>();

            // AEのアバター（type=0）を追加
            if (aeDatabase?.Items != null)
            {
                items.AddRange(aeDatabase.Items.Where(item => item.Type == "0"));
            }

            // KAのアバターを追加
            if (kaAvatarsDatabase?.data != null)
            {
                items.AddRange(kaAvatarsDatabase.data);
            }

            if (string.IsNullOrEmpty(searchQuery))
                return items;

            return items
                .Where(item =>
                {
                    if (item is AvatarExplorerItem aeItem)
                    {
                        return aeItem.Title.IndexOf(searchQuery, StringComparison.OrdinalIgnoreCase)
                                >= 0
                            || aeItem.AuthorName.IndexOf(
                                searchQuery,
                                StringComparison.OrdinalIgnoreCase
                            ) >= 0;
                    }
                    else if (item is KonoAssetAvatarItem kaItem)
                    {
                        return kaItem.description.name.IndexOf(
                                searchQuery,
                                StringComparison.OrdinalIgnoreCase
                            ) >= 0
                            || kaItem.description.creator.IndexOf(
                                searchQuery,
                                StringComparison.OrdinalIgnoreCase
                            ) >= 0;
                    }
                    return false;
                })
                .ToList();
        }

        /// <summary>
        /// フィルターされたアイテムリストを取得
        /// </summary>
        /// <returns>フィルターされたアイテムリスト</returns>
        private List<object> GetFilteredItems()
        {
            var items = new List<object>();
            if (aeDatabase != null)
            {
                // Type!=0 かつ CustomCategoryに"ワールド"または"world"が含まれていないアイテムを追加
                items.AddRange(
                    aeDatabase.Items.Where(item =>
                        item.Type != "0"
                        && !item.CustomCategory.Contains(
                            "ワールド",
                            StringComparison.OrdinalIgnoreCase
                        )
                        && !item.CustomCategory.Contains(
                            "world",
                            StringComparison.OrdinalIgnoreCase
                        )
                    )
                );
            }
            if (kaWearablesDatabase != null)
            {
                items.AddRange(kaWearablesDatabase.data);
            }

            if (string.IsNullOrEmpty(searchQuery))
                return items;

            return items
                .Where(item =>
                {
                    if (item is AvatarExplorerItem aeItem)
                    {
                        return aeItem.Title.IndexOf(searchQuery, StringComparison.OrdinalIgnoreCase)
                                >= 0
                            || aeItem.AuthorName.IndexOf(
                                searchQuery,
                                StringComparison.OrdinalIgnoreCase
                            ) >= 0;
                    }
                    else if (item is KonoAssetWearableItem kaItem)
                    {
                        return kaItem.description.name.IndexOf(
                                searchQuery,
                                StringComparison.OrdinalIgnoreCase
                            ) >= 0
                            || kaItem.description.creator.IndexOf(
                                searchQuery,
                                StringComparison.OrdinalIgnoreCase
                            ) >= 0;
                    }
                    return false;
                })
                .ToList();
        }

        /// <summary>
        /// フィルターされたワールドオブジェクトリストを取得
        /// </summary>
        /// <returns>フィルターされたワールドオブジェクトリスト</returns>
        private List<object> GetFilteredWorldObjects()
        {
            var items = new List<object>();

            // AEのワールドアイテムを追加
            if (aeDatabase?.Items != null)
            {
                items.AddRange(
                    aeDatabase.Items.Where(item =>
                        item.Type != "0"
                        && (
                            item.CustomCategory.Contains(
                                "ワールド",
                                StringComparison.OrdinalIgnoreCase
                            )
                            || item.CustomCategory.Contains(
                                "world",
                                StringComparison.OrdinalIgnoreCase
                            )
                        )
                    )
                );
            }

            // KAのワールドオブジェクトを追加
            if (kaWorldObjectsDatabase?.data != null)
            {
                items.AddRange(kaWorldObjectsDatabase.data);
            }

            if (string.IsNullOrEmpty(searchQuery))
                return items;

            return items
                .Where(item =>
                {
                    if (item is AvatarExplorerItem aeItem)
                    {
                        return aeItem.Title.IndexOf(searchQuery, StringComparison.OrdinalIgnoreCase)
                                >= 0
                            || aeItem.AuthorName.IndexOf(
                                searchQuery,
                                StringComparison.OrdinalIgnoreCase
                            ) >= 0;
                    }
                    else if (item is KonoAssetWorldObjectItem kaItem)
                    {
                        return kaItem.description.name.IndexOf(
                                searchQuery,
                                StringComparison.OrdinalIgnoreCase
                            ) >= 0
                            || kaItem.description.creator.IndexOf(
                                searchQuery,
                                StringComparison.OrdinalIgnoreCase
                            ) >= 0;
                    }
                    return false;
                })
                .ToList();
        }

        #region Item Display Methods
        /// <summary>
        /// AEアバターアイテムの表示
        /// </summary>
        /// <param name="item">表示するアイテム</param>
        private void ShowAvatarItem(AvatarExplorerItem item)
        {
            GUILayout.BeginVertical(EditorStyles.helpBox);
            DrawItemHeader(item.Title, item.AuthorName, item.ImagePath, item.ItemPath);
            DrawUnityPackageSection(item.ItemPath, item.Title);
            GUILayout.EndVertical();
        }

        /// <summary>
        /// KAアバターアイテムの表示
        /// </summary>
        /// <param name="item">表示するアイテム</param>
        private void ShowKonoAssetItem(KonoAssetAvatarItem item)
        {
            GUILayout.BeginVertical(EditorStyles.helpBox);
            var itemPath = Path.GetFullPath(Path.Combine(kaDatabasePath, "data", item.id));
            DrawItemHeader(
                item.description.name,
                item.description.creator,
                item.description.imageFilename,
                itemPath
            );
            DrawUnityPackageSection(itemPath, item.description.name);
            GUILayout.EndVertical();
        }

        /// <summary>
        /// KAウェアラブルアイテムの表示
        /// </summary>
        /// <param name="item">表示するアイテム</param>
        private void ShowKonoAssetWearableItem(KonoAssetWearableItem item)
        {
            GUILayout.BeginVertical(EditorStyles.helpBox);
            var itemPath = Path.GetFullPath(Path.Combine(kaDatabasePath, "data", item.id));
            DrawItemHeader(
                item.description.name,
                item.description.creator,
                item.description.imageFilename,
                itemPath
            );
            DrawItemDetails(item.category, item.supportedAvatars);
            DrawUnityPackageSection(itemPath, item.description.name);
            GUILayout.EndVertical();
        }

        /// <summary>
        /// KAワールドオブジェクトアイテムの表示
        /// </summary>
        /// <param name="item">表示するアイテム</param>
        private void ShowKonoAssetWorldObjectItem(KonoAssetWorldObjectItem item)
        {
            GUILayout.BeginVertical(EditorStyles.helpBox);
            var itemPath = Path.GetFullPath(Path.Combine(kaDatabasePath, "data", item.id));
            DrawItemHeader(
                item.description.name,
                item.description.creator,
                item.description.imageFilename,
                itemPath
            );
            DrawItemDetails(item.category);
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
        private void DrawItemHeader(string title, string author, string imagePath, string itemPath)
        {
            GUILayout.BeginHorizontal();
            DrawItemImage(imagePath);
            GUILayout.BeginVertical();
            GUILayout.Label(title, EditorStyles.boldLabel);
            GUILayout.Label($"作者: {author}");
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
                var texture = LoadTexture(fullImagePath);
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
                return Path.Combine(aeDatabasePath, imagePath.Replace("Datas\\", ""));
            }
            return Path.Combine(kaDatabasePath, "images", imagePath);
        }

        /// <summary>
        /// 開くボタンの描画
        /// </summary>
        /// <param name="itemPath">アイテムパス</param>
        private void DrawOpenButton(string itemPath)
        {
            if (Directory.Exists(itemPath))
            {
                if (GUILayout.Button("開く", GUILayout.Width(150)))
                {
                    System.Diagnostics.Process.Start("explorer.exe", itemPath);
                }
            }
        }

        /// <summary>
        /// アイテム詳細の描画
        /// </summary>
        /// <param name="category">カテゴリー</param>
        /// <param name="supportedAvatars">対応アバター</param>
        private void DrawItemDetails(string category, string[]? supportedAvatars = null)
        {
            GUILayout.Label($"カテゴリー: {category}");
            if (supportedAvatars != null && supportedAvatars.Length > 0)
            {
                GUILayout.Label($"対応アバター: {string.Join(", ", supportedAvatars)}");
            }
        }

        /// <summary>
        /// UnityPackageセクションの描画
        /// </summary>
        /// <param name="itemPath">アイテムパス</param>
        /// <param name="itemName">アイテム名</param>
        private void DrawUnityPackageSection(string itemPath, string itemName)
        {
            var unityPackages = DatabaseHelper.FindUnityPackages(itemPath);
            if (!unityPackages.Any())
                return;

            if (!unityPackageFoldouts.ContainsKey(itemName))
            {
                unityPackageFoldouts[itemName] = false;
            }

            // 枠の開始位置を記録
            var startRect = EditorGUILayout.GetControlRect(false, 0);
            var startY = startRect.y;

            // 行全体をクリック可能にするためのボックスを作成
            var boxRect = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight);
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
        #endregion

        #region Database Loading Methods
        /// <summary>
        /// AEデータベースの読み込み
        /// </summary>
        private void LoadAEDatabase()
        {
            if (string.IsNullOrEmpty(aeDatabasePath))
                return;

            Debug.Log($"Loading AE database from: {aeDatabasePath}");
            aeDatabase = DatabaseHelper.LoadAEDatabase(aeDatabasePath);
            if (aeDatabase != null)
            {
                Debug.Log(
                    $"AE database loaded successfully. Items count: {aeDatabase.Items.Count}"
                );
            }
            else
            {
                // エラーポップアップを表示
                EditorUtility.DisplayDialog(
                    "パスエラー",
                    "入力したパスが誤っています\n\n\"VRC-Avatar-Explorer-v○○/Data\"\nを指定してください",
                    "OK"
                );

                // パスを空欄に戻す
                aeDatabasePath = "";
                SaveSettings();
            }
        }

        /// <summary>
        /// KAデータベースの読み込み
        /// </summary>
        private void LoadKADatabase()
        {
            if (string.IsNullOrEmpty(kaDatabasePath))
                return;

            Debug.Log($"Loading KA database from: {kaDatabasePath}");
            var metadataPath = Path.Combine(kaDatabasePath, "metadata");

            if (!Directory.Exists(metadataPath))
            {
                Debug.LogWarning($"Metadata directory not found at: {metadataPath}");

                // エラーポップアップを表示
                EditorUtility.DisplayDialog(
                    "パスエラー",
                    "入力したパスが誤っています\n\nKonoAssetの設定にある\n\"アプリデータの保存先\"と\n同一のディレクトリを指定してください",
                    "OK"
                );

                // パスを空欄に戻す
                kaDatabasePath = "";
                SaveSettings();
                return;
            }

            LoadKADatabaseFile(metadataPath, "avatars.json", ref kaAvatarsDatabase);
            LoadKADatabaseFile(metadataPath, "avatarWearables.json", ref kaWearablesDatabase);
            LoadKADatabaseFile(metadataPath, "worldObjects.json", ref kaWorldObjectsDatabase);
        }

        /// <summary>
        /// KAデータベースファイルの読み込み
        /// </summary>
        /// <typeparam name="T">データベースの型</typeparam>
        /// <param name="metadataPath">メタデータパス</param>
        /// <param name="filename">ファイル名</param>
        /// <param name="database">データベース参照</param>
        private void LoadKADatabaseFile<T>(string metadataPath, string filename, ref T? database)
            where T : class
        {
            var filePath = Path.Combine(metadataPath, filename);
            if (!File.Exists(filePath))
                return;

            var baseDb = DatabaseHelper.LoadKADatabase(filePath);
            if (baseDb == null)
                return;

            database = JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(baseDb));

            var itemCount = GetItemCount(database);
            Debug.Log($"{filename} loaded successfully. Items count: {itemCount}");
        }

        /// <summary>
        /// データベースのアイテム数を取得
        /// </summary>
        /// <typeparam name="T">データベースの型</typeparam>
        /// <param name="database">データベース</param>
        /// <returns>アイテム数</returns>
        private int GetItemCount<T>(T? database)
            where T : class
        {
            if (database is KonoAssetAvatarsDatabase avatarsDb)
                return avatarsDb.data.Length;
            if (database is KonoAssetWearablesDatabase wearablesDb)
                return wearablesDb.data.Length;
            if (database is KonoAssetWorldObjectsDatabase worldObjectsDb)
                return worldObjectsDb.data.Length;
            return 0;
        }
        #endregion

        #region Utility Methods
        /// <summary>
        /// テクスチャを読み込む
        /// </summary>
        /// <param name="path">テクスチャパス</param>
        /// <returns>読み込んだテクスチャ</returns>
        private Texture2D? LoadTexture(string path)
        {
            if (string.IsNullOrEmpty(path))
                return null;

            if (imageCache.TryGetValue(path, out var cachedTexture))
            {
                return cachedTexture;
            }

            if (!File.Exists(path))
                return null;

            try
            {
                var bytes = File.ReadAllBytes(path);
                var texture = new Texture2D(2, 2);
                if (UnityEngine.ImageConversion.LoadImage(texture, bytes))
                {
                    imageCache[path] = texture;
                    return texture;
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Error loading texture from {path}: {ex.Message}");
            }

            return null;
        }

        /// <summary>
        /// 設定の読み込み
        /// </summary>
        private void LoadSettings()
        {
            aeDatabasePath = EditorPrefs.GetString(AE_DATABASE_PATH_KEY, "");
            kaDatabasePath = EditorPrefs.GetString(KA_DATABASE_PATH_KEY, "");

            if (!string.IsNullOrEmpty(aeDatabasePath))
                LoadAEDatabase();
            if (!string.IsNullOrEmpty(kaDatabasePath))
                LoadKADatabase();
        }

        /// <summary>
        /// 設定の保存
        /// </summary>
        private void SaveSettings()
        {
            EditorPrefs.SetString(AE_DATABASE_PATH_KEY, aeDatabasePath);
            EditorPrefs.SetString(KA_DATABASE_PATH_KEY, kaDatabasePath);
        }

        /// <summary>
        /// データベースを再読み込みする
        /// </summary>
        private void RefreshDatabases()
        {
            Debug.Log("Refreshing databases...");

            // 画像キャッシュをクリア
            imageCache.Clear();

            // データベースを再読み込み
            if (!string.IsNullOrEmpty(aeDatabasePath))
            {
                LoadAEDatabase();
            }

            if (!string.IsNullOrEmpty(kaDatabasePath))
            {
                LoadKADatabase();
            }

            // ページをリセット
            currentPage = 0;

            Debug.Log("Databases refreshed successfully.");
        }

        /// <summary>
        /// 画像キャッシュを再取得する
        /// </summary>
        private void RefreshImageCache()
        {
            Debug.Log("Refreshing image cache...");

            // 画像キャッシュをクリア
            imageCache.Clear();

            // 現在表示中のアイテムの画像を再読み込み
            var currentItems = GetCurrentTabItems();
            foreach (var item in currentItems)
            {
                string imagePath = GetItemImagePath(item);
                if (!string.IsNullOrEmpty(imagePath))
                {
                    string fullImagePath = GetFullImagePath(imagePath);
                    if (File.Exists(fullImagePath))
                    {
                        LoadTexture(fullImagePath);
                    }
                }
            }

            Debug.Log("Image cache refreshed successfully.");
        }

        /// <summary>
        /// 現在のタブのアイテムを取得
        /// </summary>
        /// <returns>現在のタブのアイテムリスト</returns>
        private List<object> GetCurrentTabItems()
        {
            switch (selectedTab)
            {
                case 0:
                    return GetFilteredAvatars();
                case 1:
                    return GetFilteredItems();
                case 2:
                    return GetFilteredWorldObjects();
                default:
                    return new List<object>();
            }
        }

        /// <summary>
        /// アイテムの画像パスを取得
        /// </summary>
        /// <param name="item">アイテム</param>
        /// <returns>画像パス</returns>
        private string GetItemImagePath(object item)
        {
            if (item is AvatarExplorerItem aeItem)
            {
                return aeItem.ImagePath;
            }
            else if (item is KonoAssetAvatarItem kaItem)
            {
                return kaItem.description.imageFilename;
            }
            else if (item is KonoAssetWearableItem wearableItem)
            {
                return wearableItem.description.imageFilename;
            }
            else if (item is KonoAssetWorldObjectItem worldItem)
            {
                return worldItem.description.imageFilename;
            }

            return string.Empty;
        }
        #endregion
    }
}
