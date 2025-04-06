// Copyright (c) 2025 yuki-2525
// This code is borrowed from AvatarExplorer(https://github.com/yuki-2525/AvatarExplorer)
// AvatarExplorer is licensed under the MIT License. https://github.com/yuki-2525/AvatarExplorer/blob/main/LICENSE
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

        /// <summary>詳細検索の表示状態</summary>
        private bool showAdvancedSearch = false;

        /// <summary>詳細検索の各フィールド</summary>
        private string titleSearch = "";
        private string authorSearch = "";
        private string categorySearch = "";
        private string supportedAvatarsSearch = "";
        private string tagsSearch = "";
        private string memoSearch = "";

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

        /// <summary>メモのフォールドアウト状態の管理</summary>
        private Dictionary<string, bool> memoFoldouts = new Dictionary<string, bool>();

        /// <summary>UnityPackageのフォールドアウト状態の管理</summary>
        private Dictionary<string, bool> unityPackageFoldouts = new Dictionary<string, bool>();

        /// <summary>ソート方法の列挙型</summary>
        private enum SortMethod
        {
            /// <summary>追加順（新しい順）</summary>
            CreatedDateDesc,

            /// <summary>追加順（古い順）</summary>
            CreatedDateAsc,

            /// <summary>アセット名（A-Z順）</summary>
            TitleAsc,

            /// <summary>アセット名（Z-A順）</summary>
            TitleDesc,

            /// <summary>ショップ名（A-Z順）</summary>
            AuthorAsc,

            /// <summary>ショップ名（Z-A順）</summary>
            AuthorDesc,
        }

        /// <summary>現在のソート方法</summary>
        private SortMethod currentSortMethod = SortMethod.CreatedDateDesc;

        /// <summary>ソート方法のラベル</summary>
        private string[] sortLabels =
        {
            "追加順（新しい順）",
            "追加順（古い順）",
            "アセット名（A-Z順）",
            "アセット名（Z-A順）",
            "ショップ名（A-Z順）",
            "ショップ名（Z-A順）",
        };
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
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            // 基本検索フィールド
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("検索:", GUILayout.Width(60));
            var newSearchQuery = EditorGUILayout.TextField(searchQuery);
            if (newSearchQuery != searchQuery)
            {
                searchQuery = newSearchQuery;
                currentPage = 0;
            }

            // 詳細検索のトグル
            var newShowAdvancedSearch = EditorGUILayout.ToggleLeft(
                "詳細検索",
                showAdvancedSearch,
                GUILayout.Width(100)
            );
            if (newShowAdvancedSearch != showAdvancedSearch)
            {
                showAdvancedSearch = newShowAdvancedSearch;
                currentPage = 0;
            }

            // ソートボタン
            if (GUILayout.Button("▼ 表示順", GUILayout.Width(80)))
            {
                var menu = new GenericMenu();
                menu.AddItem(
                    new GUIContent("追加順（新しい順）"),
                    currentSortMethod == SortMethod.CreatedDateDesc,
                    () => SetSortMethod(SortMethod.CreatedDateDesc)
                );
                menu.AddItem(
                    new GUIContent("追加順（古い順）"),
                    currentSortMethod == SortMethod.CreatedDateAsc,
                    () => SetSortMethod(SortMethod.CreatedDateAsc)
                );
                menu.AddItem(
                    new GUIContent("アセット名（A-Z順）"),
                    currentSortMethod == SortMethod.TitleAsc,
                    () => SetSortMethod(SortMethod.TitleAsc)
                );
                menu.AddItem(
                    new GUIContent("アセット名（Z-A順）"),
                    currentSortMethod == SortMethod.TitleDesc,
                    () => SetSortMethod(SortMethod.TitleDesc)
                );
                menu.AddItem(
                    new GUIContent("ショップ名（A-Z順）"),
                    currentSortMethod == SortMethod.AuthorAsc,
                    () => SetSortMethod(SortMethod.AuthorAsc)
                );
                menu.AddItem(
                    new GUIContent("ショップ名（Z-A順）"),
                    currentSortMethod == SortMethod.AuthorDesc,
                    () => SetSortMethod(SortMethod.AuthorDesc)
                );
                menu.ShowAsContext();
            }
            EditorGUILayout.EndHorizontal();

            // 詳細検索フィールド
            if (showAdvancedSearch)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                // タイトル検索
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("タイトル:", GUILayout.Width(100));
                var newTitleSearch = EditorGUILayout.TextField(titleSearch);
                if (newTitleSearch != titleSearch)
                {
                    titleSearch = newTitleSearch;
                    currentPage = 0;
                }
                EditorGUILayout.EndHorizontal();

                // 作者名検索
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("作者名:", GUILayout.Width(100));
                var newAuthorSearch = EditorGUILayout.TextField(authorSearch);
                if (newAuthorSearch != authorSearch)
                {
                    authorSearch = newAuthorSearch;
                    currentPage = 0;
                }
                EditorGUILayout.EndHorizontal();

                // カテゴリ検索
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("カテゴリ:", GUILayout.Width(100));
                var newCategorySearch = EditorGUILayout.TextField(categorySearch);
                if (newCategorySearch != categorySearch)
                {
                    categorySearch = newCategorySearch;
                    currentPage = 0;
                }
                EditorGUILayout.EndHorizontal();

                // 対応アバター検索
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("対応アバター:", GUILayout.Width(100));
                var newSupportedAvatarsSearch = EditorGUILayout.TextField(supportedAvatarsSearch);
                if (newSupportedAvatarsSearch != supportedAvatarsSearch)
                {
                    supportedAvatarsSearch = newSupportedAvatarsSearch;
                    currentPage = 0;
                }
                EditorGUILayout.EndHorizontal();

                // タグ検索
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("タグ:", GUILayout.Width(100));
                var newTagsSearch = EditorGUILayout.TextField(tagsSearch);
                if (newTagsSearch != tagsSearch)
                {
                    tagsSearch = newTagsSearch;
                    currentPage = 0;
                }
                EditorGUILayout.EndHorizontal();

                // メモ検索
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("メモ:", GUILayout.Width(100));
                var newMemoSearch = EditorGUILayout.TextField(memoSearch);
                if (newMemoSearch != memoSearch)
                {
                    memoSearch = newMemoSearch;
                    currentPage = 0;
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.EndVertical();
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndVertical();
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
            var sortedItems = SortItems(filteredItems);

            // ページネーション
            int startIndex = currentPage * ITEMS_PER_PAGE;
            int endIndex = Mathf.Min(startIndex + ITEMS_PER_PAGE, sortedItems.Count);
            var pageItems = sortedItems.Skip(startIndex).Take(ITEMS_PER_PAGE);

            foreach (var item in pageItems)
            {
                if (item is AvatarExplorerItem aeItem)
                {
                    ShowAvatarItem(aeItem, false, false); // アバタータブではカテゴリと対応アバターを表示しない
                }
                else if (item is KonoAssetAvatarItem kaItem)
                {
                    ShowKonoAssetItem(kaItem, false, false); // アバタータブではカテゴリと対応アバターを表示しない
                }
            }
        }

        /// <summary>
        /// アバター関連アイテムコンテンツの表示
        /// </summary>
        private void ShowItemsContent()
        {
            var filteredItems = GetFilteredItems();
            var sortedItems = SortItems(filteredItems);

            // ページネーション
            int startIndex = currentPage * ITEMS_PER_PAGE;
            int endIndex = Mathf.Min(startIndex + ITEMS_PER_PAGE, sortedItems.Count);
            var pageItems = sortedItems.Skip(startIndex).Take(ITEMS_PER_PAGE);

            foreach (var item in pageItems)
            {
                if (item is AvatarExplorerItem aeItem)
                {
                    ShowAvatarItem(aeItem, true, true); // アバター関連タブではカテゴリと対応アバターを表示する
                }
                else if (item is KonoAssetWearableItem kaItem)
                {
                    ShowKonoAssetWearableItem(kaItem, true); // アバター関連タブでは対応アバターを表示する
                }
            }
        }

        /// <summary>
        /// ワールドオブジェクトコンテンツの表示
        /// </summary>
        private void ShowWorldObjectsContent()
        {
            var filteredItems = GetFilteredWorldObjects();
            var sortedItems = SortItems(filteredItems);

            // ページネーション
            int startIndex = currentPage * ITEMS_PER_PAGE;
            int endIndex = Mathf.Min(startIndex + ITEMS_PER_PAGE, sortedItems.Count);
            var pageItems = sortedItems.Skip(startIndex).Take(ITEMS_PER_PAGE);

            foreach (var item in pageItems)
            {
                if (item is AvatarExplorerItem aeItem)
                {
                    ShowAvatarItem(aeItem, true, false); // ワールドタブではカテゴリを表示し、対応アバターを表示しない
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

            return SortItems(items.Where(IsItemMatchSearch).ToList());
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

            return SortItems(items.Where(IsItemMatchSearch).ToList());
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

            return SortItems(items.Where(IsItemMatchSearch).ToList());
        }

        /// <summary>
        /// アイテムをソートする
        /// </summary>
        /// <param name="items">ソートするアイテムリスト</param>
        /// <returns>ソートされたアイテムリスト</returns>
        private List<object> SortItems(List<object> items)
        {
            switch (currentSortMethod)
            {
                case SortMethod.CreatedDateDesc:
                    return items.OrderByDescending(item => GetCreatedDate(item)).ToList();
                case SortMethod.CreatedDateAsc:
                    return items.OrderBy(item => GetCreatedDate(item)).ToList();
                case SortMethod.TitleAsc:
                    return items.OrderBy(item => GetTitle(item)).ToList();
                case SortMethod.TitleDesc:
                    return items.OrderByDescending(item => GetTitle(item)).ToList();
                case SortMethod.AuthorAsc:
                    return items.OrderBy(item => GetAuthor(item)).ToList();
                case SortMethod.AuthorDesc:
                    return items.OrderByDescending(item => GetAuthor(item)).ToList();
                default:
                    return items;
            }
        }

        /// <summary>
        /// 日付文字列をDateTimeに変換
        /// </summary>
        /// <param name="date">日付文字列</param>
        /// <returns>DateTime</returns>
        private static DateTime GetDate(string date)
        {
            try
            {
                if (date.All(char.IsDigit))
                    return DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(date)).DateTime;

                var allDigits = "";
                foreach (var c in date)
                {
                    if (char.IsDigit(c))
                        allDigits += c;
                }

                if (allDigits.Length != 14)
                    return DateTimeOffset.FromUnixTimeMilliseconds(0).DateTime;

                var year = allDigits.Substring(0, 4);
                var month = allDigits.Substring(4, 2);
                var day = allDigits.Substring(6, 2);
                var hour = allDigits.Substring(8, 2);
                var minute = allDigits.Substring(10, 2);
                var second = allDigits.Substring(12, 2);

                var dateTime = new DateTime(
                    int.Parse(year),
                    int.Parse(month),
                    int.Parse(day),
                    int.Parse(hour),
                    int.Parse(minute),
                    int.Parse(second),
                    DateTimeKind.Unspecified
                );

                // ローカルのタイムゾーンの時間をUTCに変換
                var utcDateTime = TimeZoneInfo.ConvertTimeToUtc(dateTime, TimeZoneInfo.Local);

                return utcDateTime;
            }
            catch
            {
                return DateTimeOffset.FromUnixTimeMilliseconds(0).DateTime;
            }
        }

        /// <summary>
        /// アイテムの作成日を取得
        /// </summary>
        /// <param name="item">アイテム</param>
        /// <returns>作成日（UnixTimeMilliseconds）</returns>
        private long GetCreatedDate(object item)
        {
            if (item is AvatarExplorerItem aeItem)
            {
                if (aeItem.CreatedDate == default)
                    return 0;

                // 日付文字列をUTCのDateTimeに変換
                var utcDateTime = GetDate(aeItem.CreatedDate.ToString());

                // UTCのDateTimeをUnixTimeMillisecondsに変換
                return new DateTimeOffset(utcDateTime, TimeSpan.Zero).ToUnixTimeMilliseconds();
            }
            else if (item is KonoAssetAvatarItem kaItem)
            {
                return kaItem.description.createdAt;
            }
            else if (item is KonoAssetWearableItem wearableItem)
            {
                return wearableItem.description.createdAt;
            }
            else if (item is KonoAssetWorldObjectItem worldItem)
            {
                return worldItem.description.createdAt;
            }
            return 0;
        }

        /// <summary>
        /// アイテムのタイトルを取得
        /// </summary>
        /// <param name="item">アイテム</param>
        /// <returns>タイトル</returns>
        private string GetTitle(object item)
        {
            if (item is AvatarExplorerItem aeItem)
            {
                return aeItem.Title;
            }
            else if (item is KonoAssetAvatarItem kaItem)
            {
                return kaItem.description.name;
            }
            else if (item is KonoAssetWearableItem wearableItem)
            {
                return wearableItem.description.name;
            }
            else if (item is KonoAssetWorldObjectItem worldItem)
            {
                return worldItem.description.name;
            }
            return string.Empty;
        }

        /// <summary>
        /// アイテムの作者名を取得
        /// </summary>
        /// <param name="item">アイテム</param>
        /// <returns>作者名</returns>
        private string GetAuthor(object item)
        {
            if (item is AvatarExplorerItem aeItem)
            {
                return aeItem.AuthorName;
            }
            else if (item is KonoAssetAvatarItem kaItem)
            {
                return kaItem.description.creator;
            }
            else if (item is KonoAssetWearableItem wearableItem)
            {
                return wearableItem.description.creator;
            }
            else if (item is KonoAssetWorldObjectItem worldItem)
            {
                return worldItem.description.creator;
            }
            return string.Empty;
        }

        /// <summary>
        /// アイテムが検索条件に一致するかチェック
        /// </summary>
        private bool IsItemMatchSearch(object item)
        {
            // 基本検索（スペース区切りの複数キーワードに対応）
            if (!string.IsNullOrEmpty(searchQuery))
            {
                var keywords = searchQuery.Split(
                    new[] { ' ' },
                    StringSplitOptions.RemoveEmptyEntries
                );
                bool matchesAllKeywords = true;

                foreach (var keyword in keywords)
                {
                    bool matchesKeyword = false;

                    // タイトル
                    if (GetTitle(item).IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0)
                        matchesKeyword = true;

                    // 作者名
                    if (GetAuthor(item).IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0)
                        matchesKeyword = true;

                    // カテゴリ
                    if (
                        item is AvatarExplorerItem aeItem
                        && aeItem
                            .GetCategoryName()
                            .IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0
                    )
                        matchesKeyword = true;

                    // 対応アバター
                    if (
                        item is AvatarExplorerItem aeItem2
                        && aeItem2.SupportedAvatars != null
                        && aeItem2.SupportedAvatars.Any(avatar =>
                            avatar.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0
                        )
                    )
                        matchesKeyword = true;

                    // タグ
                    if (
                        item is KonoAssetAvatarItem kaItem
                        && kaItem.description.tags != null
                        && kaItem.description.tags.Any(tag =>
                            tag.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0
                        )
                    )
                        matchesKeyword = true;

                    // メモ
                    if (
                        item is AvatarExplorerItem aeItem3
                        && !string.IsNullOrEmpty(aeItem3.Memo)
                        && aeItem3.Memo.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0
                    )
                        matchesKeyword = true;

                    if (!matchesKeyword)
                    {
                        matchesAllKeywords = false;
                        break;
                    }
                }

                if (!matchesAllKeywords)
                    return false;
            }

            // 詳細検索
            if (showAdvancedSearch)
            {
                // タイトル検索（スペース区切りでAND検索）
                if (!string.IsNullOrEmpty(titleSearch))
                {
                    var titleKeywords = titleSearch.Split(
                        new[] { ' ' },
                        StringSplitOptions.RemoveEmptyEntries
                    );
                    foreach (var keyword in titleKeywords)
                    {
                        if (GetTitle(item).IndexOf(keyword, StringComparison.OrdinalIgnoreCase) < 0)
                            return false;
                    }
                }

                // 作者名検索（スペース区切りでAND検索）
                if (!string.IsNullOrEmpty(authorSearch))
                {
                    var authorKeywords = authorSearch.Split(
                        new[] { ' ' },
                        StringSplitOptions.RemoveEmptyEntries
                    );
                    foreach (var keyword in authorKeywords)
                    {
                        if (
                            GetAuthor(item).IndexOf(keyword, StringComparison.OrdinalIgnoreCase) < 0
                        )
                            return false;
                    }
                }

                // カテゴリ検索（スペース区切りでAND検索）
                if (!string.IsNullOrEmpty(categorySearch))
                {
                    if (item is AvatarExplorerItem aeItem)
                    {
                        var categoryName = aeItem.GetCategoryName();
                        var categoryKeywords = categorySearch.Split(
                            new[] { ' ' },
                            StringSplitOptions.RemoveEmptyEntries
                        );
                        foreach (var keyword in categoryKeywords)
                        {
                            if (
                                categoryName.IndexOf(keyword, StringComparison.OrdinalIgnoreCase)
                                < 0
                            )
                                return false;
                        }
                    }
                }

                // 対応アバター検索（スペース区切りでAND検索）
                if (!string.IsNullOrEmpty(supportedAvatarsSearch))
                {
                    if (item is AvatarExplorerItem aeItem && aeItem.SupportedAvatars != null)
                    {
                        var avatarKeywords = supportedAvatarsSearch.Split(
                            new[] { ' ' },
                            StringSplitOptions.RemoveEmptyEntries
                        );
                        foreach (var keyword in avatarKeywords)
                        {
                            bool found = false;
                            foreach (var avatar in aeItem.SupportedAvatars)
                            {
                                if (
                                    avatar.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0
                                )
                                {
                                    found = true;
                                    break;
                                }
                            }
                            if (!found)
                                return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }

                // タグ検索（スペース区切りでAND検索）
                if (!string.IsNullOrEmpty(tagsSearch))
                {
                    if (item is KonoAssetAvatarItem kaItem && kaItem.description.tags != null)
                    {
                        var tagKeywords = tagsSearch.Split(
                            new[] { ' ' },
                            StringSplitOptions.RemoveEmptyEntries
                        );
                        foreach (var keyword in tagKeywords)
                        {
                            bool found = false;
                            foreach (var tag in kaItem.description.tags)
                            {
                                if (tag.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0)
                                {
                                    found = true;
                                    break;
                                }
                            }
                            if (!found)
                                return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }

                // メモ検索（スペース区切りでAND検索）
                if (!string.IsNullOrEmpty(memoSearch))
                {
                    if (item is AvatarExplorerItem aeItem && !string.IsNullOrEmpty(aeItem.Memo))
                    {
                        var memoKeywords = memoSearch.Split(
                            new[] { ' ' },
                            StringSplitOptions.RemoveEmptyEntries
                        );
                        foreach (var keyword in memoKeywords)
                        {
                            if (
                                aeItem.Memo.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) < 0
                            )
                                return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        #region Item Display Methods
        /// <summary>
        /// AEアバターアイテムの表示
        /// </summary>
        /// <param name="item">表示するアイテム</param>
        /// <param name="showCategory">カテゴリを表示するかどうか</param>
        /// <param name="showSupportedAvatars">対応アバターを表示するかどうか</param>
        private void ShowAvatarItem(
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
                showCategory, // カテゴリの表示を制御
                showSupportedAvatars // 対応アバターの表示を制御
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
        private void ShowKonoAssetItem(
            KonoAssetAvatarItem item,
            bool showCategory,
            bool showSupportedAvatars = false
        )
        {
            GUILayout.BeginVertical(EditorStyles.helpBox);
            var itemPath = Path.GetFullPath(Path.Combine(kaDatabasePath, "data", item.id));

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
                showCategory, // カテゴリの表示を制御
                showSupportedAvatars // 対応アバターの表示を制御
            );
            DrawUnityPackageSection(itemPath, item.description.name);
            GUILayout.EndVertical();
        }

        /// <summary>
        /// KAウェアラブルアイテムの表示
        /// </summary>
        /// <param name="item">表示するアイテム</param>
        /// <param name="showSupportedAvatars">対応アバターを表示するかどうか</param>
        private void ShowKonoAssetWearableItem(
            KonoAssetWearableItem item,
            bool showSupportedAvatars = false
        )
        {
            GUILayout.BeginVertical(EditorStyles.helpBox);
            var itemPath = Path.GetFullPath(Path.Combine(kaDatabasePath, "data", item.id));

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
                true, // ウェアラブルタブではカテゴリを表示する
                showSupportedAvatars // 対応アバターの表示を制御
            );
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
                null, // ワールドオブジェクトタブでは対応アバターを表示しない
                item.description.tags,
                item.description.memo,
                true, // ワールドオブジェクトタブではカテゴリを表示する
                false // ワールドオブジェクトタブでは対応アバターを表示しない
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
                        GUILayout.Label($"カテゴリ: {item.GetCategoryName()}");
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

            // 枠を描画（余白を調整）
            var frameRect = new Rect(
                startRect.x,
                startY,
                EditorGUIUtility.currentViewWidth - 20,
                endY - startY + 5 // 余白を10から5に減らす
            );
            EditorGUI.DrawRect(frameRect, new Color(0.5f, 0.5f, 0.5f, 0.2f));
            GUI.Box(frameRect, "", EditorStyles.helpBox);

            // 次のアイテムとの間に余白を追加
            EditorGUILayout.Space(5);
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

        /// <summary>
        /// ソート方法を設定
        /// </summary>
        private void SetSortMethod(SortMethod method)
        {
            if (currentSortMethod != method)
            {
                currentSortMethod = method;
                currentPage = 0;
            }
        }
        #endregion
    }
}
