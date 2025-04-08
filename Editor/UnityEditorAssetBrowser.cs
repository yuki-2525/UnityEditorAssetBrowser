// Copyright (c) 2025 yuki-2525
// This code is borrowed from AvatarExplorer(https://github.com/puk06/AvatarExplorer)
// AvatarExplorer is licensed under the MIT License. https://github.com/puk06/AvatarExplorer/blob/main/LICENSE
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
using UnityEditorAssetBrowser.Services;
using UnityEditorAssetBrowser.ViewModels;
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

        /// <summary>詳細検索の表示状態</summary>
        private bool showAdvancedSearch => searchViewModel.SearchCriteria.ShowAdvancedSearch;

        /// <summary>選択中のタブインデックス</summary>
        private int selectedTab = 0;

        /// <summary>タブのラベル</summary>
        private string[] tabs = { "アバター", "アバター関連", "ワールド" };

        /// <summary>フォールドアウト状態の管理</summary>
        private Dictionary<string, bool> foldouts = new Dictionary<string, bool>();

        /// <summary>画像のキャッシュ</summary>
        private Dictionary<string, Texture2D> imageCache => ImageServices.Instance.imageCache;

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

        private AssetItem assetItem = new AssetItem();

        private SearchViewModel searchViewModel = new SearchViewModel();
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
            // DatabaseServiceから設定を読み込む
            DatabaseService.LoadSettings();

            // パスを取得
            aeDatabasePath = DatabaseService.GetAEDatabasePath();
            kaDatabasePath = DatabaseService.GetKADatabasePath();

            // データベースを読み込む
            if (!string.IsNullOrEmpty(aeDatabasePath))
                LoadAndDisplayAEDatabase();
            if (!string.IsNullOrEmpty(kaDatabasePath))
                LoadAndDisplayKADatabase();

            // SearchViewModelを初期化
            searchViewModel = new SearchViewModel(aeDatabase);

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
                // DatabaseServiceにパスを保存
                DatabaseService.SetAEDatabasePath(aeDatabasePath);
                DatabaseService.SetKADatabasePath(kaDatabasePath);
                DatabaseService.SaveSettings();
            }

            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// データベースパス入力フィールドの描画
        /// </summary>
        private void DrawDatabasePathFields()
        {
            DrawDatabasePathField(
                "AE Database Path:",
                ref aeDatabasePath,
                LoadAndDisplayAEDatabase
            );
            DrawDatabasePathField(
                "KA Database Path:",
                ref kaDatabasePath,
                LoadAndDisplayKADatabase
            );

            // リフレッシュボタンを追加（一つにまとめる）
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("更新", GUILayout.Width(100)))
            {
                LoadAndDisplayAEDatabase();
                LoadAndDisplayKADatabase();
                Repaint();
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

                // DatabaseServiceにパスを保存
                DatabaseService.SetAEDatabasePath(aeDatabasePath);
                DatabaseService.SetKADatabasePath(kaDatabasePath);
                DatabaseService.SaveSettings();
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

                    // DatabaseServiceにパスを保存
                    DatabaseService.SetAEDatabasePath(aeDatabasePath);
                    DatabaseService.SetKADatabasePath(kaDatabasePath);
                    DatabaseService.SaveSettings();
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

                // タブが切り替わったときにSearchViewModelに通知
                searchViewModel.SetCurrentTab(selectedTab);

                Repaint();
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
            var newSearchQuery = EditorGUILayout.TextField(
                searchViewModel.SearchCriteria.SearchQuery
            );
            if (newSearchQuery != searchViewModel.SearchCriteria.SearchQuery)
            {
                searchViewModel.SearchCriteria.SearchQuery = newSearchQuery;
                Repaint();
            }

            // 詳細検索のトグル
            var newShowAdvancedSearch = EditorGUILayout.ToggleLeft(
                "詳細検索",
                searchViewModel.SearchCriteria.ShowAdvancedSearch,
                GUILayout.Width(100)
            );
            if (newShowAdvancedSearch != searchViewModel.SearchCriteria.ShowAdvancedSearch)
            {
                searchViewModel.SearchCriteria.ShowAdvancedSearch = newShowAdvancedSearch;
                currentPage = 0;
                Repaint();
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
                var newTitleSearch = EditorGUILayout.TextField(
                    searchViewModel.SearchCriteria.TitleSearch
                );
                if (newTitleSearch != searchViewModel.SearchCriteria.TitleSearch)
                {
                    searchViewModel.SearchCriteria.TitleSearch = newTitleSearch;
                    Repaint();
                }
                EditorGUILayout.EndHorizontal();

                // 作者名検索
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("作者名:", GUILayout.Width(100));
                var newAuthorSearch = EditorGUILayout.TextField(
                    searchViewModel.SearchCriteria.AuthorSearch
                );
                if (newAuthorSearch != searchViewModel.SearchCriteria.AuthorSearch)
                {
                    searchViewModel.SearchCriteria.AuthorSearch = newAuthorSearch;
                    Repaint();
                }
                EditorGUILayout.EndHorizontal();

                // カテゴリ検索（アバタータブ以外で表示）
                if (selectedTab != 0)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("カテゴリ:", GUILayout.Width(100));
                    var newCategorySearch = EditorGUILayout.TextField(
                        searchViewModel.SearchCriteria.CategorySearch
                    );
                    if (newCategorySearch != searchViewModel.SearchCriteria.CategorySearch)
                    {
                        searchViewModel.SearchCriteria.CategorySearch = newCategorySearch;
                        Repaint();
                    }
                    EditorGUILayout.EndHorizontal();
                }

                // 対応アバター検索（アイテムタブのみで表示）
                if (selectedTab == 1)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("対応アバター:", GUILayout.Width(100));
                    var newSupportedAvatarsSearch = EditorGUILayout.TextField(
                        searchViewModel.SearchCriteria.SupportedAvatarsSearch
                    );
                    if (
                        newSupportedAvatarsSearch
                        != searchViewModel.SearchCriteria.SupportedAvatarsSearch
                    )
                    {
                        searchViewModel.SearchCriteria.SupportedAvatarsSearch =
                            newSupportedAvatarsSearch;
                        Repaint();
                    }
                    EditorGUILayout.EndHorizontal();
                }

                // タグ検索
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("タグ:", GUILayout.Width(100));
                var newTagsSearch = EditorGUILayout.TextField(
                    searchViewModel.SearchCriteria.TagsSearch
                );
                if (newTagsSearch != searchViewModel.SearchCriteria.TagsSearch)
                {
                    searchViewModel.SearchCriteria.TagsSearch = newTagsSearch;
                    Repaint();
                }
                EditorGUILayout.EndHorizontal();

                // メモ検索
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("メモ:", GUILayout.Width(100));
                var newMemoSearch = EditorGUILayout.TextField(
                    searchViewModel.SearchCriteria.MemoSearch
                );
                if (newMemoSearch != searchViewModel.SearchCriteria.MemoSearch)
                {
                    searchViewModel.SearchCriteria.MemoSearch = newMemoSearch;
                    Repaint();
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

            return SortItems(items.Where(searchViewModel.IsItemMatchSearch).ToList());
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

            return SortItems(items.Where(searchViewModel.IsItemMatchSearch).ToList());
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

            return SortItems(items.Where(searchViewModel.IsItemMatchSearch).ToList());
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
            // 相対パスの場合はAEDatabasePathと結合
            string fullPath = itemPath;
            if (itemPath.StartsWith("Datas\\") && aeDatabasePath != null)
            {
                // パスの区切り文字を正規化
                string normalizedItemPath = itemPath.Replace(
                    "\\",
                    Path.DirectorySeparatorChar.ToString()
                );
                string normalizedAePath = aeDatabasePath.Replace(
                    "/",
                    Path.DirectorySeparatorChar.ToString()
                );

                // Datas\Items\アイテム名 の形式の場合、AEDatabasePath\Items\アイテム名 に変換
                string itemName = Path.GetFileName(normalizedItemPath);
                fullPath = Path.Combine(normalizedAePath, "Items", itemName);
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
            if (itemPath.StartsWith("Datas\\") && aeDatabasePath != null)
            {
                // パスの区切り文字を正規化
                string normalizedItemPath = itemPath.Replace(
                    "\\",
                    Path.DirectorySeparatorChar.ToString()
                );
                string normalizedAePath = aeDatabasePath.Replace(
                    "/",
                    Path.DirectorySeparatorChar.ToString()
                );

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
        #endregion

        #region Utility Methods

        /// <summary>
        /// データベースを再読み込みする
        /// </summary>
        private void RefreshDatabases()
        {
            // 画像キャッシュをクリア
            ImageServices.Instance.ClearCache();

            // データベースを再読み込み
            if (!string.IsNullOrEmpty(aeDatabasePath))
            {
                LoadAndDisplayAEDatabase();
            }

            if (!string.IsNullOrEmpty(kaDatabasePath))
            {
                LoadAndDisplayKADatabase();
            }

            // ページをリセット
            currentPage = 0;
        }

        /// <summary>
        /// 画像キャッシュを再取得する
        /// </summary>
        private void RefreshImageCache()
        {
            // 画像キャッシュをクリア
            ImageServices.Instance.ClearCache();

            // 現在表示中のアイテムの画像を再読み込み
            var currentItems = GetCurrentTabItems();
            foreach (var item in currentItems)
            {
                string imagePath = ImageServices.GetItemImagePath(item);
                if (!string.IsNullOrEmpty(imagePath))
                {
                    string fullImagePath = GetFullImagePath(imagePath);
                    if (File.Exists(fullImagePath))
                    {
                        ImageServices.Instance.LoadTexture(fullImagePath);
                    }
                }
            }
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

        /// <summary>
        /// AEデータベースの読み込みと表示
        /// </summary>
        private void LoadAndDisplayAEDatabase()
        {
            if (string.IsNullOrEmpty(aeDatabasePath))
                return;

            DatabaseService.SetAEDatabasePath(aeDatabasePath);
            DatabaseService.LoadAndUpdateAEDatabase();
            aeDatabase = DatabaseService.GetAEDatabase();

            // パスが空欄になった場合（エラー時）に更新
            aeDatabasePath = DatabaseService.GetAEDatabasePath();
        }

        /// <summary>
        /// KAデータベースの読み込みと表示
        /// </summary>
        private void LoadAndDisplayKADatabase()
        {
            if (string.IsNullOrEmpty(kaDatabasePath))
                return;

            DatabaseService.SetKADatabasePath(kaDatabasePath);
            DatabaseService.LoadAndUpdateKADatabase();
            kaAvatarsDatabase = DatabaseService.GetKAAvatarsDatabase();
            kaWearablesDatabase = DatabaseService.GetKAWearablesDatabase();
            kaWorldObjectsDatabase = DatabaseService.GetKAWorldObjectsDatabase();

            // パスが空欄になった場合（エラー時）に更新
            kaDatabasePath = DatabaseService.GetKADatabasePath();
        }

        /// <summary>
        /// アイテムのタイトルを取得
        /// </summary>
        /// <param name="item">アイテム</param>
        /// <returns>タイトル</returns>
        private string GetTitle(object item)
        {
            return assetItem.GetTitle(item);
        }

        /// <summary>
        /// アイテムの作者名を取得
        /// </summary>
        /// <param name="item">アイテム</param>
        /// <returns>作者名</returns>
        private string GetAuthor(object item)
        {
            return assetItem.GetAuthor(item);
        }

        /// <summary>
        /// アイテムのカテゴリー名を取得
        /// </summary>
        /// <param name="item">アイテム</param>
        /// <returns>カテゴリー名</returns>
        private string GetAECategoryName(object item)
        {
            return assetItem.GetAECategoryName(item);
        }

        /// <summary>
        /// アイテムの作成日を取得
        /// </summary>
        /// <param name="item">アイテム</param>
        /// <returns>作成日（UnixTimeMilliseconds）</returns>
        private long GetCreatedDate(object item)
        {
            return assetItem.GetCreatedDate(item);
        }

        /// <summary>
        /// アイテムのメモを取得
        /// </summary>
        /// <param name="item">アイテム</param>
        /// <returns>メモ</returns>
        private string GetMemo(object item)
        {
            return assetItem.GetMemo(item);
        }
        #endregion
    }
}
